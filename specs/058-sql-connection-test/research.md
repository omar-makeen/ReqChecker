# Research: SqlConnection Test Type

**Feature**: 058-sql-connection-test
**Date**: 2026-02-26

## R-001: NuGet Packages Required

**Decision**: Three NuGet packages needed — none are built into net8.0.
- `Microsoft.Data.SqlClient` 6.1.4 (SQL Server)
- `Npgsql` 10.0.1 (PostgreSQL)
- `MySqlConnector` 2.5.0 (MySQL)

**Rationale**: Each database engine requires its own ADO.NET driver. All three are the standard, well-maintained community/vendor packages for their respective databases. MySqlConnector is preferred over Oracle's `MySql.Data` because it implements truly asynchronous I/O (Oracle's connector has a long-standing bug where async methods execute synchronously).

**Alternatives Considered**: `MySql.Data` (Oracle) — rejected due to async bug and GPL licensing.

## R-002: Connection String Parsing (Server & Database Extraction)

**Decision**: Use driver-specific `DbConnectionStringBuilder` subclasses for parsing.

| Driver | Builder Class | Server Property | Database Property |
|--------|--------------|-----------------|-------------------|
| SQL Server | `SqlConnectionStringBuilder` | `DataSource` | `InitialCatalog` |
| PostgreSQL | `NpgsqlConnectionStringBuilder` | `Host` | `Database` |
| MySQL | `MySqlConnectionStringBuilder` | `Server` | `Database` |

**Rationale**: Builder classes handle all connection string keyword aliases automatically (e.g., `Server`, `Data Source`, `Address` all resolve to the same property). Best-effort parsing — if the connection string is invalid, the builder throws and we catch it as a configuration error.

## R-003: Server Version Retrieval

**Decision**: Use `connection.ServerVersion` (string property) after `OpenAsync()` — available on all three drivers.

| Driver | Example Output |
|--------|---------------|
| SQL Server | `"16.00.4165"` |
| PostgreSQL | `"16.2"` |
| MySQL | `"8.0.35"` |

**Rationale**: `ServerVersion` is part of the base `DbConnection` class, so the same property works uniformly across all three. Must be read after the connection is in the `Open` state.

## R-004: Pooling Disable

**Decision**: All three drivers use the identical keyword: `Pooling=false`.

**Rationale**: Ensures each test measures actual TCP connection establishment time. Set via builder property: `builder.Pooling = false;` (available on all three builder classes).

## R-005: Connection Timeout

**Decision**: All three drivers default to 15 seconds. No need to inject a timeout if the spec's default matches the driver default.

| Driver | Builder Property |
|--------|-----------------|
| SQL Server | `ConnectTimeout` |
| PostgreSQL | `Timeout` |
| MySQL | `ConnectionTimeout` |

**Rationale**: The spec requires 15-second default with connection-string override. Since all three drivers already default to 15s, we only need to check whether the user's connection string already specifies a timeout — if so, we leave it alone.

## R-006: Credential Injection

**Decision**: Use typed builder properties for credential injection.

| Driver | Username Property | Password Property |
|--------|-------------------|-------------------|
| SQL Server | `UserID` | `Password` |
| PostgreSQL | `Username` | `Password` |
| MySQL | `UserID` | `Password` |

**Rationale**: Builder properties handle the correct connection string keyword automatically.

## R-007: Health Check Query

**Decision**: `SELECT 1` works universally on SQL Server, PostgreSQL, and MySQL.

**Rationale**: Validates TCP connection alive, SQL parser functional, credentials valid, and database accessible. Returns `(int) 1` on SQL Server/PostgreSQL, `(long) 1` on MySQL — result is non-null = healthy.

## R-008: Async API Support

**Decision**: All three drivers support `OpenAsync(CancellationToken)` and `ExecuteScalarAsync(CancellationToken)`.

**Rationale**: Unlike LdapBind (which uses synchronous `LdapConnection.Bind()`), all database drivers follow the ADO.NET `DbConnection`/`DbCommand` pattern with true async support. This allows proper cancellation token propagation.

## R-009: Evidence Detection Keys

**Decision**: Use `dbType` + `connectionSucceeded` as the detection key pair for the `[SqlConnection]` converter section.

**Rationale**: Neither `dbType` nor `connectionSucceeded` appears in any other test type's evidence. No collision risk confirmed by searching all test files in `src/ReqChecker.Infrastructure/Tests/`.

## R-010: Build Manifest & Profile State

**Decision**: Current counts and insertion points:

| Item | Current Value | After SqlConnection |
|------|---------------|---------------------|
| TestManifest count | 32 | 33 |
| Last KnownTestType | LdapBind (line 63) | SqlConnection after |
| Last Compile block | LdapBind (lines 205-207) | SqlConnection after |
| Last profile entry | test-043 (LdapBind) | test-044 (SqlConnection) |
| README count | 32 | 33 |
| Last Network row | LdapBind (line 138) | SqlConnection after |

## R-011: Unified ADO.NET Pattern

**Decision**: Use a factory method that returns the appropriate `DbConnection` and `DbConnectionStringBuilder` based on `dbType`. All three drivers implement `DbConnection`, `DbCommand`, and `DbConnectionStringBuilder`.

```
CreateConnection(dbType, connectionString) → DbConnection
CreateBuilder(dbType, connectionString) → DbConnectionStringBuilder
```

**Rationale**: This keeps the main `ExecuteAsync` method clean with a single code path for all three databases. Database-specific details (builder property names, connection class) are isolated in the factory methods.
