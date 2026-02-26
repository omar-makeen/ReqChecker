# Implementation Plan: SqlConnection Test Type

**Branch**: `058-sql-connection-test` | **Date**: 2026-02-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/058-sql-connection-test/spec.md`

## Summary

Add a `SqlConnection` test type that validates database connectivity to SQL Server, PostgreSQL, and MySQL by opening a connection, executing `SELECT 1`, and reporting pass/fail with evidence (type, server, database, version, response time). Uses three NuGet packages (`Microsoft.Data.SqlClient`, `Npgsql`, `MySqlConnector`) with a unified ADO.NET `DbConnection`/`DbCommand` pattern. Disables connection pooling for accurate timing. Supports credential store authentication via `credentialRef`.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: Microsoft.Data.SqlClient 6.1.4, Npgsql 10.0.1, MySqlConnector 2.5.0 (all NuGet — not built-in)
**Storage**: N/A (in-memory test results)
**Testing**: Not requested
**Target Platform**: Windows desktop (WPF-UI)
**Project Type**: Existing multi-project solution (Infrastructure + App)
**Performance Goals**: 15-second default connection timeout
**Constraints**: Connection pooling disabled; connection string not stored in evidence (security)
**Scale/Scope**: Single test type addition (32 → 33 test types)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is unconfigured (template placeholders). No gates to evaluate. PASS.

## Project Structure

### Documentation (this feature)

```text
specs/058-sql-connection-test/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (files to create/modify)

```text
src/ReqChecker.Infrastructure/
├── ReqChecker.Infrastructure.csproj    # MODIFY: Add 3 NuGet packages
├── TestManifest.props                  # MODIFY: Register SqlConnection (32 → 33)
└── Tests/
    └── SqlConnectionTest.cs            # CREATE: New test implementation

src/ReqChecker.App/
├── Converters/
│   └── TestResultDetailsConverter.cs   # MODIFY: Add [SqlConnection] section
└── Profiles/
    └── default-profile.json            # MODIFY: Add test-044

README.md                               # MODIFY: Add SqlConnection docs (32 → 33)
```

**Structure Decision**: Follows the established pattern — new test class in `Tests/`, converter update in App, manifest and profile updates. No new projects or directories needed.

## File Changes

### 1. `src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj`

Add 3 NuGet package references to the existing `<ItemGroup>`:

```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.4" />
<PackageReference Include="MySqlConnector" Version="2.5.0" />
<PackageReference Include="Npgsql" Version="10.0.1" />
```

### 2. `src/ReqChecker.Infrastructure/TestManifest.props`

- Update header comment count: 32 → 33
- Add `<KnownTestType Include="SqlConnection" SourceFile="Tests\SqlConnectionTest.cs" />` after LdapBind (line 63)
- Add conditional `<Compile Include="Tests\SqlConnectionTest.cs" />` block after LdapBind block (line 207)

### 3. `src/ReqChecker.Infrastructure/Tests/SqlConnectionTest.cs` (NEW)

Create test class implementing `ITest` with `[TestType("SqlConnection")]`:

- **Constants**: `DefaultTimeoutSeconds = 15`, `HealthCheckQuery = "SELECT 1"`
- **Parameter extraction**: `connectionString` (required), `dbType` (required, case-insensitive), `credentialRef` (optional)
- **Validation**: Empty connectionString → `ArgumentException`, unsupported dbType → `ArgumentException` listing supported types
- **Factory methods**:
  - `CreateBuilder(dbType, connectionString)` → returns `DbConnectionStringBuilder` (SqlConnectionStringBuilder / NpgsqlConnectionStringBuilder / MySqlConnectionStringBuilder)
  - `CreateConnection(dbType, connectionString)` → returns `DbConnection` (SqlConnection / NpgsqlConnection / MySqlConnection)
  - `ParseServer(dbType, builder)` → extracts server from builder (DataSource / Host / Server)
  - `ParseDatabase(dbType, builder)` → extracts database from builder (InitialCatalog / Database / Database)
- **Connection string modification**: Set `Pooling=false` via builder. If `credentialRef` provided, inject username/password via builder properties.
- **Execution flow**: `OpenAsync(ct)` → read `ServerVersion` → `ExecuteScalarAsync("SELECT 1", ct)` → Pass
- **Evidence**: Always-present keys (`dbType`, `server`, `database`, `serverVersion`, `responseTimeMs`, `connectionSucceeded`), conditional (`authenticated`)
- **Error mapping**: `ArgumentException` → Configuration, `DbException` → Network/Timeout/Permission (based on message content), `SocketException` → Network, `OperationCanceledException` → Skipped
- **SetFailureEvidence** helper for consistent failure-path evidence

### 4. `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`

Insert `[SqlConnection]` section after `[LdapBind]` block (after line 698), before `[Response]` section:

- Detect via `evidenceData.ContainsKey("dbType") && evidenceData.ContainsKey("connectionSucceeded")`
- Always-present labels (12-char alignment): `Type:       `, `Server:     `, `Database:   `, `Version:    `, `Time:       `
- Conditional: `Auth:       ` (only when `authenticated` key exists)
- End with `sections.Add(string.Empty)`

### 5. `src/ReqChecker.App/Profiles/default-profile.json`

Add test-044 after test-043 (LdapBind):

```json
{
  "id": "test-044",
  "type": "SqlConnection",
  "displayName": "SQL Server Connectivity Check",
  "description": "Verifies SQL Server database connectivity via connection and health-check query.",
  "parameters": {
    "dbType": "SqlServer",
    "connectionString": "Server=db.example.com;Database=master;Integrated Security=true;"
  },
  "fieldPolicy": {
    "dbType": "Editable",
    "connectionString": "Editable",
    "credentialRef": "Editable"
  },
  "timeout": null,
  "retryCount": null,
  "requiresAdmin": false,
  "dependsOn": []
}
```

### 6. `README.md`

- Update count 32 → 33 in all occurrences (lines 91, 101, 122)
- Add `| Network | SqlConnection | Database connectivity check (SQL Server, PostgreSQL, MySQL) |` after LdapBind row (line 138)
- Add `#### SqlConnection` reference section after LdapBind section with parameter table and JSON example
