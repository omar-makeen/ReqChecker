# Tasks: SqlConnection Test Type

**Input**: Design documents from `/specs/058-sql-connection-test/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Not requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. US4 (Configuration Validation) is fully covered by Phase 2 foundational work (parameter validation + error mapping).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Add NuGet dependencies and register the new test type in the build manifest

- [x] T001 Add NuGet package references (Microsoft.Data.SqlClient 6.1.4, Npgsql 10.0.1, MySqlConnector 2.5.0) to `src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj`
- [x] T002 Register SqlConnection in `src/ReqChecker.Infrastructure/TestManifest.props`: update header comment count 32→33, add `<KnownTestType Include="SqlConnection" SourceFile="Tests\SqlConnectionTest.cs" />` after LdapBind, add conditional `<Compile Include="Tests\SqlConnectionTest.cs" />` block after LdapBind block

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the test class skeleton with parameter extraction, validation, factory methods, error mapping, and failure evidence helper. This phase fully satisfies US4 (Configuration Validation).

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Create `src/ReqChecker.Infrastructure/Tests/SqlConnectionTest.cs` with class skeleton: `[TestType("SqlConnection")]`, implement `ITest`, constants (`DefaultTimeoutSeconds = 15`, `HealthCheckQuery = "SELECT 1"`), parameter extraction (`connectionString` required, `dbType` required case-insensitive, `credentialRef` optional), validation (`ArgumentException` for empty connectionString, unsupported dbType listing supported types), factory methods (`CreateBuilder`, `CreateConnection`, `ParseServer`, `ParseDatabase` returning appropriate types per dbType), `SetFailureEvidence` helper for consistent failure-path evidence, and error mapping (`ArgumentException`→Configuration, `DbException`→Network/Timeout/Permission, `SocketException`→Network, `OperationCanceledException`→Skipped). Method body should return a Fail result with placeholder — actual connection logic added in Phase 3.

**Checkpoint**: Foundation ready — class compiles, validation works, factory methods return correct types per dbType

---

## Phase 3: User Story 1 — Basic SQL Server Connectivity (Priority: P1) 🎯 MVP

**Goal**: Connect to SQL Server, execute `SELECT 1`, and report pass/fail with structured evidence (dbType, server, database, serverVersion, responseTimeMs, connectionSucceeded)

**Independent Test**: Configure a SqlConnection test with `dbType: "SqlServer"` and a valid connection string, run it, verify Pass result with all evidence keys populated and `[SqlConnection]` details section displayed

### Implementation for User Story 1

- [x] T004 [US1] Implement `ExecuteAsync` connection logic in `src/ReqChecker.Infrastructure/Tests/SqlConnectionTest.cs`: build connection string via `CreateBuilder` with `Pooling=false`, create connection via `CreateConnection`, `OpenAsync(ct)`, read `ServerVersion`, `ExecuteScalarAsync("SELECT 1", ct)`, populate always-present evidence keys (`dbType`, `server`, `database`, `serverVersion`, `responseTimeMs`, `connectionSucceeded`), return Pass on success, catch exceptions and call `SetFailureEvidence` with appropriate error classification
- [x] T005 [US1] Add `[SqlConnection]` section to `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`: detect via `evidenceData.ContainsKey("dbType") && evidenceData.ContainsKey("connectionSucceeded")`, render 12-char aligned labels (Type, Server, Database, Version, Time), end with `sections.Add(string.Empty)`

**Checkpoint**: SQL Server connectivity works end-to-end with details display

---

## Phase 4: User Story 2 — PostgreSQL and MySQL Connectivity (Priority: P1)

**Goal**: Extend the existing factory methods to support PostgreSQL and MySQL connections using the same unified ADO.NET pattern

**Independent Test**: Configure SqlConnection tests with `dbType: "PostgreSQL"` and `dbType: "MySQL"` with valid connection strings, run each, verify Pass results with correct evidence per database type

### Implementation for User Story 2

- [x] T006 [US2] Verify and test PostgreSQL and MySQL paths in factory methods in `src/ReqChecker.Infrastructure/Tests/SqlConnectionTest.cs`: ensure `CreateBuilder`/`CreateConnection`/`ParseServer`/`ParseDatabase` return correct types for PostgreSQL (`NpgsqlConnectionStringBuilder`/`NpgsqlConnection`/`Host`/`Database`) and MySQL (`MySqlConnectionStringBuilder`/`MySqlConnection`/`Server`/`Database`), verify `Pooling=false` and `ServerVersion` work for all three drivers, confirm `SELECT 1` returns non-null for all

**Checkpoint**: All three database types (SqlServer, PostgreSQL, MySQL) connect and report structured evidence

---

## Phase 5: User Story 3 — Credential Store Authentication (Priority: P2)

**Goal**: Resolve username/password from `credentialRef` via the credential store and inject into the connection string, reporting `authenticated` in evidence

**Independent Test**: Configure a SqlConnection test with `credentialRef` pointing to stored credentials and a connection string without embedded credentials, verify authenticated connection with `Auth: yes` in details

### Implementation for User Story 3

- [x] T007 [US3] Implement credential injection in `src/ReqChecker.Infrastructure/Tests/SqlConnectionTest.cs`: when `credentialRef` is provided, resolve credentials via `ICredentialStore`, inject username/password into builder using driver-specific properties (SqlServer: `UserID`/`Password`, PostgreSQL: `Username`/`Password`, MySQL: `UserID`/`Password`), add conditional `authenticated` evidence key (`true` on success, `false` on auth failure), handle missing credential reference as configuration error
- [x] T008 [US3] Add conditional `Auth:` line to `[SqlConnection]` section in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`: render `Auth:       yes` or `Auth:       no` only when `authenticated` key exists in evidence

**Checkpoint**: Credential store authentication works with all three database types, `Auth` line appears only when credentialRef is provided

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Default profile entry, README documentation, and build verification

- [x] T009 [P] Add test-044 (SqlConnection) entry to `src/ReqChecker.App/Profiles/default-profile.json` after test-043 (LdapBind) with `dbType: "SqlServer"`, `connectionString: "Server=db.example.com;Database=master;Integrated Security=true;"`, fieldPolicy for dbType/connectionString/credentialRef as Editable
- [x] T010 [P] Update `README.md`: change count 32→33 in all occurrences, add `| Network | SqlConnection | Database connectivity check (SQL Server, PostgreSQL, MySQL) |` row after LdapBind in the test types table, add `#### SqlConnection` reference section after LdapBind section with parameter table and JSON example
- [x] T011 Verify full solution build succeeds with `dotnet build` — 0 errors, 0 warnings

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (NuGet packages + manifest registration)
- **User Story 1 (Phase 3)**: Depends on Phase 2 (class skeleton with factory methods)
- **User Story 2 (Phase 4)**: Depends on Phase 3 (ExecuteAsync connection logic already wired)
- **User Story 3 (Phase 5)**: Depends on Phase 3 (needs working connection logic to add credential injection)
- **Polish (Phase 6)**: T009 and T010 can start after Phase 2; T011 depends on all phases complete

### User Story Dependencies

- **US1 (P1)**: Depends on Phase 2 — core connectivity for SQL Server
- **US2 (P1)**: Depends on US1 — verifies the factory methods already handle PostgreSQL/MySQL
- **US3 (P2)**: Depends on US1 — adds credential injection on top of working connection logic
- **US4 (P2)**: Fully covered by Phase 2 (validation + error mapping) — no separate tasks needed

### Within Each User Story

- Connection logic before converter display
- Core implementation before credential injection
- All stories complete before final build verification

### Parallel Opportunities

- T001 and T002 modify different files — can run in parallel
- T009 and T010 modify different files — can run in parallel
- T009/T010 can start as early as Phase 2 completion (no code dependency)
- US2 and US3 are independent of each other (but both depend on US1)

---

## Parallel Example: Phase 6

```bash
# Launch profile and README updates together (different files):
Task: "Add test-044 to default-profile.json"
Task: "Update README.md with SqlConnection docs"

# Then verify build (depends on all changes):
Task: "Verify full solution build"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (NuGet packages + manifest)
2. Complete Phase 2: Foundational (class skeleton, validation, factories)
3. Complete Phase 3: User Story 1 (SQL Server connection + converter)
4. **STOP and VALIDATE**: Test with a SQL Server connection string
5. Basic connectivity works end-to-end

### Incremental Delivery

1. Setup + Foundational → Class compiles, validation works
2. Add US1 → SQL Server connectivity works → MVP!
3. Add US2 → PostgreSQL and MySQL also work
4. Add US3 → Credential store authentication works
5. Polish → Profile, README, build clean

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- US4 (Configuration Validation) is entirely satisfied by Phase 2 foundational work — parameter validation and error mapping cover all US4 acceptance scenarios
- All three database drivers share the ADO.NET `DbConnection`/`DbCommand` pattern — factory methods isolate driver-specific details
- Connection pooling is always disabled (`Pooling=false`) to measure real TCP connection time
- Evidence detection key pair: `dbType` + `connectionSucceeded` (confirmed collision-free)
- Connection string is never stored in evidence (security: only parsed server/database captured)
