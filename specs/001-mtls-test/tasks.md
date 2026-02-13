# Tasks: Mutual TLS Client Certificate Authentication Test

**Input**: Design documents from `/specs/001-mtls-test/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Unit tests included in user story phases (not TDD — implementation first, tests after).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Source**: `src/ReqChecker.Infrastructure/Tests/`, `src/ReqChecker.App/Converters/`, `src/ReqChecker.App/Profiles/`
- **Tests**: `tests/ReqChecker.Infrastructure.Tests/Tests/`

---

## Phase 1: Setup

**Purpose**: No setup needed — project exists, no new NuGet packages required. Skip to Foundational phase.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the evidence model and core infrastructure that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T001 Create `MtlsConnectTestEvidence` class and `MtlsConnectTest` skeleton (attribute, interface, empty `ExecuteAsync`) in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — follow patterns from `UdpPortOpenTest.cs` and `HttpGetTest.cs`
- [x] T002 Implement parameter extraction method (`ExtractParameters`) in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — extract `url` (required, HTTPS only), `clientCertPath` (required), `expectedStatus` (default 200), `timeout` (default 30000), `skipServerCertValidation` (default false); validate URL scheme is HTTPS and cert path is non-empty
- [x] T003 Implement PFX certificate loading method (`LoadClientCertificate`) in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — load via `new X509Certificate2(path, password)`, verify `HasPrivateKey`, populate evidence with certificate metadata (Subject, Issuer, Thumbprint, NotBefore, NotAfter); handle `FileNotFoundException` and `CryptographicException`
- [x] T004 Implement `HttpClientHandler` creation method (`CreateHandler`) in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — set `ClientCertificateOptions = Manual`, add client cert, configure `ServerCertificateCustomValidationCallback` for skip-validation opt-in and server cert subject capture
- [x] T005 Implement helper methods (`BuildTechnicalDetails`, `BuildEvidence`) in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — build multi-line technical details with cert info, connection details, error info; build `TestEvidence` with `ResponseData` (serialized evidence JSON), `ResponseCode`, and `TimingBreakdown`

**Checkpoint**: Foundation ready — `MtlsConnectTest` class has all helper methods, user story implementation can begin

---

## Phase 3: User Story 1 — Verify Server Requires Client Certificate (Priority: P1) MVP

**Goal**: Users can test whether an HTTPS endpoint enforces mutual TLS by providing a PFX client certificate and verifying the connection succeeds with the expected HTTP status code

**Independent Test**: Configure an mTLS endpoint URL + valid PFX file → run test → verify pass with HTTP 200 and certificate details in evidence

### Implementation for User Story 1

- [x] T006 [US1] Implement core `ExecuteAsync` main flow in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — orchestrate: extract parameters → load PFX (password from `context?.Password`) → create handler → create `HttpClient` → send GET request with timeout via `CancellationTokenSource` → compare status code against `expectedStatus` → build Pass/Fail result with evidence. Follow `HttpGetTest.cs` timing pattern (stopwatch, responseStart/responseTime).
- [x] T007 [US1] Implement error handling in `ExecuteAsync` in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — catch and categorize: `OperationCanceledException` (user cancel → Skipped), `TaskCanceledException` (timeout → ErrorCategory.Timeout), `CryptographicException` (bad PFX → Configuration), `FileNotFoundException` (missing PFX → Configuration), `ArgumentException` (invalid params → Configuration), `HttpRequestException` (TLS failure → Network), generic `Exception` (→ Network). Each handler: stop stopwatch, set EndTime/Duration, build TestError with Category/Message/ExceptionType/StackTrace, set HumanSummary.
- [x] T008 [US1] Implement HTTP status validation result in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — when TLS handshake succeeds but status code mismatches expected: set `ErrorCategory.Validation`, human summary like "mTLS handshake succeeded but server returned HTTP {actual} (expected {expected})", populate evidence with `ErrorDetail`.
- [x] T009 [P] [US1] Add `MtlsConnect` icon mapping in `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs` — add `"MtlsConnect" => SymbolRegular.ShieldKeyhole24` to the switch expression (after `UdpPortOpen` line). If `ShieldKeyhole24` is not available in WPF-UI 4.2.0, use `SymbolRegular.ShieldLock24` as fallback.
- [x] T010 [P] [US1] Add `MtlsConnect` color mapping in `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs` — add `"MtlsConnect"` to the network test category: change `"Ping" or "HttpGet" or "DnsLookup" or "DnsResolve" or "TcpPortOpen" or "UdpPortOpen"` to include `or "MtlsConnect"`
- [x] T011 [P] [US1] Add sample mTLS test definition to `src/ReqChecker.App/Profiles/default-profile.json` — add test with `type: "MtlsConnect"`, placeholder URL (`https://your-mtls-endpoint.example.com/health`), placeholder cert path, `expectedStatus: 200`, `timeout: 30000`, `skipServerCertValidation: false`, `credentialRef: "mtls-cert-password"`, field policies (url/clientCertPath/expectedStatus/timeout Editable, skipServerCertValidation Editable, credentialRef Hidden)
- [x] T012 [US1] Verify build succeeds: run `dotnet build src/ReqChecker.App/ReqChecker.App.csproj` — zero errors, zero warnings related to new code

**Checkpoint**: User Story 1 complete — users can configure and run an mTLS test with a valid PFX certificate and see pass/fail results with certificate details

---

## Phase 4: User Story 2 — Verify Certificate Chain Trust (Priority: P2)

**Goal**: Users can identify certificate chain validation failures with clear, distinct error messages

**Independent Test**: Provide a PFX with an untrusted CA certificate → run test → verify failure message specifically mentions chain/trust validation (not a generic TLS error)

### Implementation for User Story 2

- [x] T013 [US2] Enhance `HttpRequestException` error handling in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — inspect inner exception chain for chain trust indicators: check for `AuthenticationException` inner exceptions containing "chain" or "trust" or "RemoteCertificateChainErrors" in message; when detected, use specific human summary: "mTLS handshake failed — certificate chain validation error (untrusted CA or incomplete chain)" instead of generic "server rejected client certificate". Set `ErrorDetail` in evidence with the chain-specific error message.
- [x] T014 [US2] Add chain validation details to `BuildTechnicalDetails` in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — when error contains chain-related keywords, append diagnostic lines: "Possible causes: certificate not signed by a trusted CA, intermediate certificates missing from PFX, root CA not in trusted store"

**Checkpoint**: User Story 2 complete — chain validation errors produce distinct, actionable messages

---

## Phase 5: User Story 3 — Test Certificate Expiration Handling (Priority: P3)

**Goal**: Users can identify expired or not-yet-valid certificates with clear, specific error messages before the connection is attempted

**Independent Test**: Provide a PFX with an expired certificate → run test → verify failure message specifically mentions expiration with the certificate dates

### Implementation for User Story 3

- [x] T015 [US3] Add pre-connection certificate date validation in `LoadClientCertificate` method in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — after loading PFX and populating evidence metadata, check `cert.NotAfter < DateTime.UtcNow` (expired) and `cert.NotBefore > DateTime.UtcNow` (not yet valid). If expired: throw `ArgumentException` with message "Client certificate expired on {cert.NotAfter:yyyy-MM-dd} (subject: {cert.Subject})". If not yet valid: throw `ArgumentException` with message "Client certificate is not yet valid until {cert.NotBefore:yyyy-MM-dd} (subject: {cert.Subject})". Set evidence `ErrorDetail` before throwing.
- [x] T016 [US3] Enhance expiration error messages in `ExecuteAsync` exception handling in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs` — in the `ArgumentException` catch block, detect expiration-specific messages and set targeted human summaries: "Client certificate is expired (expired {date})" or "Client certificate is not yet valid (valid from {date})". Use `ErrorCategory.Configuration` for these cases since they are fixable by the user.

**Checkpoint**: User Story 3 complete — expired/not-yet-valid certificates are caught before connection with specific dates in error messages

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Unit tests, build validation, and final verification

- [x] T017 [P] Create unit test file `tests/ReqChecker.Infrastructure.Tests/Tests/MtlsConnectTestTests.cs` — test configuration error scenarios: missing URL, missing clientCertPath, non-HTTPS URL, PFX file not found, cancelled token returns Skipped. Use `JsonObject` for parameters, `FluentAssertions` for assertions. Follow `UdpPortOpenTestTests.cs` patterns.
- [x] T018 Run full test suite: `dotnet test` — verify all existing tests still pass and new mTLS tests pass
- [x] T019 Run quickstart.md verification: build app (`dotnet build src/ReqChecker.App/ReqChecker.App.csproj`), verify zero errors, confirm `MtlsConnect` type appears in converters and default profile

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies — can start immediately
- **User Story 1 (Phase 3)**: Depends on Phase 2 completion — CORE implementation
- **User Story 2 (Phase 4)**: Depends on Phase 3 completion (enhances error handling from US1)
- **User Story 3 (Phase 5)**: Depends on Phase 3 completion (enhances certificate validation from US1); can run in parallel with US2
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational only — no dependencies on other stories
- **User Story 2 (P2)**: Depends on US1 (enhances existing error handling code)
- **User Story 3 (P3)**: Depends on US1 (enhances existing certificate loading code); independent of US2

### Within Each User Story

- Parameter extraction before certificate loading
- Certificate loading before connection logic
- Core implementation before error handling refinements
- Converters and profile updates can run in parallel with test class implementation

### Parallel Opportunities

- T009, T010, T011 can all run in parallel with T006-T008 (different files)
- T013-T014 (US2) and T015-T016 (US3) can run in parallel (different code sections)
- T017 (unit tests) can run in parallel with T018-T019 (different focus)

---

## Parallel Example: User Story 1

```bash
# These can run in parallel (different files):
Task T009: "Add MtlsConnect icon mapping in TestTypeToIconConverter.cs"
Task T010: "Add MtlsConnect color mapping in TestTypeToColorConverter.cs"
Task T011: "Add sample mTLS test to default-profile.json"

# While these run sequentially (same file, dependent):
Task T006: "Implement core ExecuteAsync main flow"
Task T007: "Implement error handling in ExecuteAsync"
Task T008: "Implement HTTP status validation result"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational (T001-T005)
2. Complete Phase 3: User Story 1 (T006-T012)
3. **STOP and VALIDATE**: Build succeeds, test card appears in UI, basic mTLS test works
4. Deploy/demo if ready

### Incremental Delivery

1. Complete Foundational → Core infrastructure ready
2. Add User Story 1 → Basic mTLS testing works → Deploy (MVP!)
3. Add User Story 2 → Chain error messages improved → Deploy
4. Add User Story 3 → Expiration detection added → Deploy
5. Add Polish → Unit tests, final verification → Deploy (Complete!)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All user stories share the same `MtlsConnectTest.cs` file — US2 and US3 enhance US1's implementation
- No new NuGet packages needed — all APIs built into .NET 8
- PFX password flows through existing `credentialRef` + PromptAtRun mechanism (no credential code changes)
- Commit after each phase completion for clean git history
