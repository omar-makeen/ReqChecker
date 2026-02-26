# Tasks: LdapBind Test Type

**Input**: Design documents from `/specs/057-ldap-bind-test/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Not requested — no test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add NuGet dependency and register the new test type in the build system

- [x] T001 Add `System.DirectoryServices.Protocols` 8.0.0 NuGet package to `src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj`
- [x] T002 [P] Register LdapBind in `src/ReqChecker.Infrastructure/TestManifest.props` — add `<KnownTestType Include="LdapBind" SourceFile="Tests\LdapBindTest.cs" />` entry (after RegistryWrite, line 62), add conditional `<Compile Include="Tests\LdapBindTest.cs" />` block (after RegistryWrite block, line 202), and update the header comment count from 31 to 32

---

## Phase 2: Foundational (Class Skeleton + Configuration Validation)

**Purpose**: Create the test class with parameter extraction, validation, and evidence structure. This phase also satisfies **User Story 4 (Configuration Validation, P2)** since validation errors are thrown before any network activity.

**Covers**: FR-002, FR-003, FR-004, FR-005, FR-011, FR-012, FR-013 (config errors)

- [x] T003 Create `src/ReqChecker.Infrastructure/Tests/LdapBindTest.cs` with `[TestType("LdapBind")]` class implementing `ITest`, containing: constants (`DefaultLdapPort = 389`, `DefaultLdapsPort = 636`, `TimeoutMs = 10000`), `ExecuteAsync` method skeleton, parameter extraction from `testDefinition.Parameters` JsonObject (`server` required string, `port` optional int, `useSsl` optional bool defaulting to false, `credentialRef` optional string), context-dependent default port (636 if useSsl, else 389), validation (throw `ArgumentException` for empty server or out-of-range port), `try/catch` structure with error mapping (`ArgumentException` → `ErrorCategory.Configuration`, `LdapException` → `ErrorCategory.Network`/`Timeout`/`Permission`, `SocketException` → `Network`, `OperationCanceledException` → `Skipped`), evidence `Dictionary<string, object>` initialization with always-present keys (`server`, `port`, `useSsl`, `bindType`, `responseTimeMs`), and `TestResult` population with `TestEvidence.ResponseData` serialized via `JsonSerializer.Serialize`. Follow the SmtpConnectTest pattern in the same directory.

**Checkpoint**: Class compiles, parameter validation works. `dotnet build src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj /p:IncludeTests="LdapBind"` succeeds.

---

## Phase 3: User Story 1 — Basic LDAP Server Connectivity Check (Priority: P1) 🎯 MVP

**Goal**: Connect to an LDAP server and perform an anonymous bind, reporting pass/fail with evidence.

**Independent Test**: Configure an LdapBind test with a known LDAP server hostname and port 389, run it, verify pass/fail with server, port, bind type (anonymous), and response time in evidence.

**Covers**: FR-001, FR-006, FR-009, FR-010, FR-013 (network errors), FR-015

### Implementation for User Story 1

- [x] T004 [US1] Implement anonymous bind logic in `src/ReqChecker.Infrastructure/Tests/LdapBindTest.cs` — create `LdapDirectoryIdentifier(server, port, false, false)`, create `LdapConnection(identifier)` with `SessionOptions.ReferralChasing = ReferralChasingOptions.None`, `Timeout = TimeSpan.FromMilliseconds(TimeoutMs)`, `AuthType = AuthType.Anonymous`, start `Stopwatch`, call `connection.Bind()`, stop stopwatch, populate evidence with `bindType = "anonymous"` and `responseTimeMs = stopwatch.ElapsedMilliseconds`, set `TestResult.Status = TestStatus.Pass` on success. On failure paths, set `responseTimeMs = -1` (sentinel for n/a), `bindType = "n/a"` if bind was never attempted. Handle distinct error messages for DNS resolution failure, connection timeout, connection reset, and anonymous bind rejection per FR-013.
- [x] T005 [P] [US1] Add `[LdapBind]` section to `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs` — insert before the `[Response]` section, detect via `evidenceData.ContainsKey("bindType") && evidenceData.ContainsKey("responseTimeMs")`, render labels: `Server` (always), `Port` (always), `Bind` (always, value from `bindType` key), `Time` (always, render `n/a` if value < 0, else `{value} ms`). Add conditional `TLS` line (only when `tlsNegotiated` is `"True"` or `"true"`, show `tlsVersion` value) and conditional `Auth` line (only when `authenticated` key exists, show `yes`/`no`). End with `sections.Add(string.Empty)`. Use 10-char fixed-width label alignment matching SmtpConnect style.

**Checkpoint**: Anonymous bind works end-to-end. App shows `[LdapBind]` section with Server, Port, Bind, Time fields in results details view.

---

## Phase 4: User Story 2 — SSL/TLS-Secured LDAP Connectivity (Priority: P1)

**Goal**: Support LDAPS connections with implicit SSL/TLS, capturing TLS negotiation status and protocol version.

**Independent Test**: Configure an LdapBind test with `useSsl: true` against port 636, run it, verify evidence shows TLS negotiation succeeded with protocol version.

**Covers**: FR-004, FR-008, FR-009 (TLS evidence)

### Implementation for User Story 2

- [x] T006 [US2] Add LDAPS support to `src/ReqChecker.Infrastructure/Tests/LdapBindTest.cs` — when `useSsl` is true, set `connection.SessionOptions.SecureSocketLayer = true` and `connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true` (skip validation, accept all certs including self-signed). After successful bind, attempt to read TLS version from `connection.SessionOptions.SslInformation` and populate evidence with `tlsNegotiated = true` and `tlsVersion` (e.g., `"Tls12"`, `"Tls13"`). If `SslInformation` is not accessible, set `tlsNegotiated = true` without `tlsVersion`. Add distinct error handling for TLS negotiation failure (e.g., `LdapException` during SSL handshake → error message "TLS negotiation failed").

**Checkpoint**: LDAPS connections work. Evidence shows TLS version. Details view shows `TLS:` line when useSsl is true, omits it when false.

---

## Phase 5: User Story 3 — Authenticated LDAP Bind (Priority: P2)

**Goal**: Support authenticated simple bind using credentials resolved from `credentialRef` via `TestExecutionContext`.

**Independent Test**: Configure an LdapBind test with `credentialRef` pointing to valid stored credentials, `useSsl: true`, run it, verify evidence shows authenticated bind succeeded.

**Covers**: FR-005, FR-007, FR-009 (auth evidence), FR-013 (auth errors)

### Implementation for User Story 3

- [x] T007 [US3] Add authenticated bind support in `src/ReqChecker.Infrastructure/Tests/LdapBindTest.cs` — when `context?.Username` is not null (credentialRef resolved by SequentialTestRunner), set `connection.AuthType = AuthType.Basic`, call `connection.Bind(new NetworkCredential(context.Username, context.Password ?? string.Empty))` instead of anonymous bind. Set evidence `bindType = "authenticated"` and `authenticated = true/false` based on bind success. Add `warning = "Credentials sent without encryption"` to evidence when credentialRef is provided but `useSsl` is false. Handle distinct errors: credential not found (context null when credentialRef was specified → `ErrorCategory.Configuration`), authentication failure (invalid credentials → `ErrorCategory.Permission` with "Authentication failed" message).

**Checkpoint**: Authenticated binds work. Evidence shows `Bind: authenticated`, `Auth: yes/no`. Warning appears when credentials sent without TLS.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Default profile entry, documentation, and build verification

- [x] T008 [P] Add LdapBind test entry (test-043) to `src/ReqChecker.App/Profiles/default-profile.json` — insert after test-042 (RegistryWrite) with `type: "LdapBind"`, `displayName: "LDAP Server Bind Check"`, `description: "Verifies LDAP/Active Directory server connectivity via anonymous bind."`, `parameters: { server: "dc.example.com", port: 389 }`, `fieldPolicy: { server: "Editable", port: "Editable", useSsl: "Editable", credentialRef: "Editable" }`, no timeout/retryCount overrides, no dependencies
- [x] T009 [P] Update `README.md` — add `LdapBind` to the Network category row in the test type summary table, update total test type count from 31 to 32 in all occurrences, add `#### LdapBind` detailed reference section under Network tests with description paragraph, parameter table (server/string/required, port/int/optional/389 or 636, useSsl/bool/optional/false, credentialRef/string/optional), and JSON profile example
- [x] T010 Verify full application build succeeds with `dotnet build src/ReqChecker.App/ReqChecker.App.csproj` — confirm no compiler errors, TestManifest validation passes (ValidateManifestSync, ValidateIncludeTests), and the new test type count is 32

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on T001 (NuGet package) and T002 (manifest) from Setup
- **US1 (Phase 3)**: Depends on T003 (class skeleton) from Foundational — BLOCKS all feature work
- **US2 (Phase 4)**: Depends on T004 (anonymous bind) from US1 — extends existing connection logic
- **US3 (Phase 5)**: Depends on T004 (anonymous bind) from US1 — extends existing bind logic
- **Polish (Phase 6)**: T008/T009 can start after T003; T010 must be last

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational (Phase 2) — No dependencies on other stories
- **US2 (P1)**: Depends on US1 (T004) — Extends the connection with SSL/TLS options
- **US3 (P2)**: Depends on US1 (T004) — Extends the bind with authentication. Independent of US2 (SSL and auth are orthogonal features)
- **US4 (P2)**: Satisfied by Foundational (T003) — Parameter validation is the first code in ExecuteAsync

### Within Each User Story

- Core connection/bind logic before evidence population
- Evidence population before details converter (T005 depends on T004's evidence keys)

### Parallel Opportunities

- T001 and T002 can run in parallel (different files)
- T004 and T005 can run in parallel (different projects: Infrastructure vs App)
- T006 (US2) and T007 (US3) can run in parallel after T004 completes (orthogonal features in same file, but different code sections)
- T008 and T009 can run in parallel (different files: profile JSON vs README)

---

## Parallel Example: Phase 1 (Setup)

```
# Launch both setup tasks together:
Task T001: "Add NuGet package to ReqChecker.Infrastructure.csproj"
Task T002: "Register LdapBind in TestManifest.props"
```

## Parallel Example: Phase 3 (US1)

```
# After T003 completes, launch US1 tasks together:
Task T004: "Implement anonymous bind in LdapBindTest.cs"
Task T005: "Add [LdapBind] section to TestResultDetailsConverter.cs"
```

## Parallel Example: Phase 6 (Polish)

```
# Launch profile and README tasks together:
Task T008: "Add test-043 to default-profile.json"
Task T009: "Update README.md with LdapBind documentation"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001, T002)
2. Complete Phase 2: Foundational (T003) — class skeleton + validation
3. Complete Phase 3: US1 (T004, T005) — anonymous bind + details view
4. **STOP and VALIDATE**: Build the app, configure an LdapBind test, verify it runs
5. Deploy/demo if ready — basic LDAP connectivity checking works

### Incremental Delivery

1. Setup + Foundational → Class compiles, validation works
2. Add US1 → Anonymous bind works end-to-end (MVP!)
3. Add US2 → LDAPS support adds encryption verification
4. Add US3 → Authenticated bind adds credential validation
5. Polish → Default profile + README complete the feature

### Single Developer (Recommended Order)

```
T001 → T002 → T003 → T004 → T005 → T006 → T007 → T008 → T009 → T010
```

All 10 tasks in sequence, building incrementally on the same file set.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- US4 (Configuration Validation) is satisfied by T003 in the Foundational phase — validation code runs before any network activity
- US2 and US3 are orthogonal: SSL and authentication can be implemented independently and combined freely by the user
- The single-developer path is recommended since most tasks modify the same file (`LdapBindTest.cs`)
- Commit after each phase checkpoint for clean git history
