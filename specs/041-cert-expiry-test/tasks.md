# Tasks: SSL Certificate Expiry Test

**Input**: Design documents from `/specs/041-cert-expiry-test/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Unit tests are included as the plan explicitly defines a test file and test categories.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Foundational (Core Types & Helpers)

**Purpose**: Create the shared evidence class, parameters class, parameter extraction, and result-building helpers that all user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T001 Create `CertificateExpiryTestEvidence` public class with all evidence fields (Host, Port, Subject, Issuer, Thumbprint, NotBefore, NotAfter, DaysUntilExpiry, IsExpired, IsNotYetValid, ExpiresWithinWarningWindow, SubjectAlternativeNames, TlsProtocolVersion, ResponseTimeMs, ChainValidationSkipped, ErrorDetail) per data-model.md in `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`
- [X] T002 Create private `CertificateExpiryParameters` class with validated fields (Host, Port, WarningDaysBeforeExpiry, Timeout, SkipChainValidation, ExpectedSubject, ExpectedIssuer, ExpectedThumbprint) per data-model.md in `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`
- [X] T003 Implement `ExtractParameters()` private method: extract and validate all parameters from `TestDefinition.Parameters` JsonObject — host (required, non-empty), port (default 443, range 1-65535), warningDaysBeforeExpiry (default 30, >= 0), timeout (default 10000, > 0), skipChainValidation (default false), expectedSubject/expectedIssuer/expectedThumbprint (optional, non-empty if provided). Throw `ArgumentException` on validation failures. Follow `MtlsConnectTest.ExtractParameters()` pattern in `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`
- [X] T004 Implement result-building helper methods: `BuildSuccessResult()`, `BuildValidationFailResult()` (accepts reason string for specific validation failure), `BuildCancelledResult()`, `BuildTimeoutResult()`, `BuildNetworkErrorResult()`, `BuildConfigurationErrorResult()`, `BuildTechnicalDetails()` (multi-line string with all cert evidence fields), and `BuildEvidence()` (serializes `CertificateExpiryTestEvidence` to `TestEvidence.ResponseData`). Follow `MtlsConnectTest` result-builder pattern in `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`

**Checkpoint**: Foundation ready — evidence, parameters, extraction, and result builders complete. User story implementation can begin.

---

## Phase 2: User Story 1 — Check Remote Certificate Validity (Priority: P1) 🎯 MVP

**Goal**: Connect to a remote TLS endpoint, retrieve the server certificate, evaluate its validity window (expired, not-yet-valid, approaching expiry), and return a pass/fail result with rich evidence.

**Independent Test**: Configure a CertificateExpiry test targeting `www.google.com:443`, run the test, verify "Pass" with evidence showing certificate subject, issuer, expiry date, and days remaining.

### Implementation for User Story 1

- [X] T005 [US1] Implement `ConnectAndGetCertificateAsync()` private method: create `TcpClient`, connect to host:port with linked cancellation token (user cancel + timeout), wrap in `SslStream` with `RemoteCertificateValidationCallback` that captures the server `X509Certificate2` (when `skipChainValidation` true → callback returns true; when false → returns default validation result; always capture cert), call `AuthenticateAsClientAsync(host)` to send SNI automatically, return captured certificate. Dispose TcpClient/SslStream properly. Reference research.md R1 and R4 in `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`
- [X] T006 [US1] Implement `EvaluateCertificate()` private method (validity window portion only, identity matching added in US2): accept `X509Certificate2` and `CertificateExpiryParameters`, compute `daysUntilExpiry = (cert.NotAfter.ToUniversalTime() - DateTime.UtcNow).Days`, check: (1) cert.NotAfter < UtcNow → return validation fail "expired", (2) cert.NotBefore > UtcNow → return validation fail "not yet valid", (3) daysUntilExpiry < warningDaysBeforeExpiry → return validation fail "expires within warning window", (4) all pass → return null (success). Populate evidence fields. In `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`
- [X] T007 [US1] Implement `ExecuteAsync()` method on `[TestType("CertificateExpiry")] CertificateExpiryTest : ITest`: initialize `TestResult` with `Status = Fail`, start `Stopwatch`, call `ExtractParameters()`, cancellation check, create linked `CancellationTokenSource` for timeout, call `ConnectAndGetCertificateAsync()`, call `EvaluateCertificate()`, build success or validation-fail result. Exception handling: `OperationCanceledException` when `cancellationToken.IsCancellationRequested` → Skipped, `TaskCanceledException`/`OperationCanceledException` (timeout) → Timeout, `ArgumentException` → Configuration, `SocketException` → Network, `AuthenticationException` → Network, general `Exception` → Network. Set `HumanSummary` per FR-019 (e.g., "Certificate for example.com expires in 45 days (2026-04-04)"). In `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`

**Checkpoint**: US1 complete — CertificateExpiry test can connect, retrieve certificate, evaluate validity window, and return pass/fail with evidence. Core MVP is functional.

---

## Phase 3: User Story 2 — Certificate Identity Verification (Priority: P2)

**Goal**: Add optional identity verification: match `expectedSubject` against both Subject DN and SAN entries, match `expectedIssuer` against Issuer DN, match `expectedThumbprint` against certificate thumbprint.

**Independent Test**: Configure a CertificateExpiry test with `expectedSubject` set to the target hostname, run the test, verify subject/SAN matching works correctly.

### Implementation for User Story 2

- [X] T008 [US2] Implement `ExtractSanEntries()` private static method: accept `X509Certificate2`, find `X509SubjectAlternativeNameExtension` via `cert.Extensions.OfType<X509SubjectAlternativeNameExtension>().FirstOrDefault()`, call `EnumerateDnsNames()` to get DNS SAN entries as `string[]`. Return empty array if no SAN extension found. Reference research.md R2 in `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`
- [X] T009 [US2] Extend `EvaluateCertificate()` method with identity assertion logic after validity window checks: (1) if `expectedSubject` is set → substring match against `cert.Subject` (case-insensitive) OR exact match (case-insensitive) against any SAN DNS entry from `ExtractSanEntries()` — fail with "Subject/SAN mismatch" showing expected vs actual subject + SANs if neither matches per FR-013 clarification, (2) if `expectedIssuer` is set → substring match against `cert.Issuer` (case-insensitive) — fail with "Issuer mismatch" if no match per FR-014, (3) if `expectedThumbprint` is set → case-insensitive exact match against `cert.Thumbprint` — fail with "Thumbprint mismatch" if no match per FR-015. In `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`

**Checkpoint**: US2 complete — identity verification works independently. Test can now optionally assert certificate subject (via DN or SAN), issuer, and thumbprint.

---

## Phase 4: User Story 3 — Profile & UI Integration (Priority: P3)

**Goal**: Register CertificateExpiry in the UI (icon + color) and add sample profile entries with field-level policies so the test is discoverable and configurable in the app.

**Independent Test**: Load the default profile, verify CertificateExpiry tests appear in the test list with the correct icon and color, verify field policies render correctly (editable fields).

### Implementation for User Story 3

- [X] T010 [P] [US3] Add `"CertificateExpiry"` icon mapping to the switch expression in `TestTypeToIconConverter.Convert()` — use `SymbolRegular.Certificate24` (or `ShieldCheckmark24` if Certificate24 unavailable). Place after the `"MtlsConnect"` entry. In `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs`
- [X] T011 [P] [US3] Add `"CertificateExpiry"` to the network/security color group in `TestTypeToColorConverter.Convert()` — append to the existing `"Ping" or "HttpGet" or ... or "MtlsConnect"` pattern so it uses the `StatusInfo` (blue) brush. In `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs`
- [X] T012 [P] [US3] Add two sample CertificateExpiry test entries to the `tests` array in the default profile: (1) `test-017` happy path targeting `www.google.com:443` with `warningDaysBeforeExpiry: 30`, all params with `fieldPolicy: "Editable"`, `dependsOn: ["test-001"]`; (2) `test-018` expected-failure path targeting `expired.badssl.com:443` with `warningDaysBeforeExpiry: 30`, `skipChainValidation: true`, all params `fieldPolicy: "Editable"`, empty `dependsOn`. Follow existing entry format (see MtlsConnect entries test-013 through test-016 for reference). In `src/ReqChecker.App/Profiles/default-profile.json`

**Checkpoint**: US3 complete — CertificateExpiry tests are visible in the UI with correct icon/color and field-level policies work on sample profile entries.

---

## Phase 5: Polish & Verification

**Purpose**: Unit tests, build verification, and final validation.

- [X] T013 [P] Create unit tests in `tests/ReqChecker.Infrastructure.Tests/Tests/CertificateExpiryTestTests.cs` following `MtlsConnectTestTests` pattern: (1) Parameter validation tests — missing host → Configuration error, empty host → Configuration error, invalid port 0 → Configuration error, invalid port 65536 → Configuration error, negative port → Configuration error, invalid timeout 0 → Configuration error, negative timeout → Configuration error, negative warningDaysBeforeExpiry → Configuration error, valid default port (no port param) → passes validation (fails on network); (2) Cancellation tests — pre-cancelled token → Skipped status with "cancelled" in HumanSummary; (3) Result property tests — TestId/TestType/DisplayName populated correctly, StartTime/EndTime/Duration set, HumanSummary and TechnicalDetails non-empty on failure
- [X] T014 Build solution and verify compilation succeeds: `dotnet build src/ReqChecker.App/ReqChecker.App.csproj`
- [X] T015 Run unit tests and verify all pass: `dotnet test tests/ReqChecker.Infrastructure.Tests/`

**Checkpoint**: All tasks complete — feature is fully implemented, tested, and verified.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 1)**: No dependencies — can start immediately. BLOCKS all user stories.
- **US1 (Phase 2)**: Depends on Foundational (Phase 1) completion.
- **US2 (Phase 3)**: Depends on US1 (Phase 2) — extends `EvaluateCertificate()` and adds SAN extraction.
- **US3 (Phase 4)**: Depends on Foundational (Phase 1) only — modifies different files, can run in parallel with US1/US2.
- **Polish (Phase 5)**: Depends on US1 + US2 + US3 completion.

### User Story Dependencies

- **User Story 1 (P1)**: Requires Foundational phase. Core MVP — no other story dependencies.
- **User Story 2 (P2)**: Requires US1 — extends the `EvaluateCertificate()` method with identity checks.
- **User Story 3 (P3)**: Requires Foundational only — touches different files (converters, profile JSON). Can run in parallel with US1/US2.

### Within Each User Story

- Models/types before logic (evidence class → extraction → evaluation → orchestration)
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- **T010, T011, T012** can all run in parallel (different files: icon converter, color converter, profile JSON)
- **T013** (unit tests) can run in parallel with T010-T012 (different file)
- **US3 tasks (T010-T012)** can run in parallel with US1 (T005-T007) since they touch different files

---

## Parallel Example: User Story 3 + Polish

```bash
# These can all launch simultaneously (different files, no dependencies on each other):
Task: "T010 [US3] Add CertificateExpiry icon in TestTypeToIconConverter.cs"
Task: "T011 [US3] Add CertificateExpiry color in TestTypeToColorConverter.cs"
Task: "T012 [US3] Add sample entries in default-profile.json"
Task: "T013 Create unit tests in CertificateExpiryTestTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Foundational (T001-T004)
2. Complete Phase 2: User Story 1 (T005-T007)
3. **STOP and VALIDATE**: Test by adding a CertificateExpiry entry to a profile pointing to `www.google.com:443` — should Pass with certificate evidence
4. Test `expired.badssl.com:443` — should Fail with Validation error

### Incremental Delivery

1. Foundational (T001-T004) → Core types and helpers ready
2. US1 (T005-T007) → Certificate validity checking works → **MVP!**
3. US2 (T008-T009) → Identity verification (subject/SAN, issuer, thumbprint) added
4. US3 (T010-T012) → UI integration and sample profile entries (can overlap with US1/US2)
5. Polish (T013-T015) → Unit tests and build verification

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- US2 extends US1's `EvaluateCertificate()` method — this is the only cross-story dependency
- US3 can proceed independently in parallel with US1/US2 (different files)
- No new NuGet packages required
- DI auto-discovers the `[TestType("CertificateExpiry")]` class — no registration code needed
- Commit after each phase checkpoint for clean history
