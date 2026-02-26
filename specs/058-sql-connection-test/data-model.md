# Data Model: SqlConnection Test Type

**Feature**: 058-sql-connection-test

## Parameters Schema (Input)

Extracted from `testDefinition.Parameters` JsonObject.

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `connectionString` | string | Yes | — | Database connection string (driver-specific format) |
| `dbType` | string | Yes | — | Database engine: `SqlServer`, `PostgreSQL`, `MySQL` (case-insensitive) |
| `credentialRef` | string | No | — | Credential reference for authentication via credential store |

## Evidence Schema (Output)

Serialized to `TestEvidence.ResponseData` via `JsonSerializer.Serialize`.

### Always-Present Keys

| Key | Type | Description | Failure Sentinel |
|-----|------|-------------|-----------------|
| `dbType` | string | Canonical database type (`SqlServer`, `PostgreSQL`, `MySQL`) | value from parameter |
| `server` | string | Server address parsed from connection string | `"n/a"` |
| `database` | string | Database name parsed from connection string | `"n/a"` |
| `serverVersion` | string | Database server version string | `"n/a"` |
| `responseTimeMs` | long | Connection + query time in milliseconds | `-1` (rendered as `n/a`) |
| `connectionSucceeded` | bool | Whether the connection opened successfully | `false` |

### Conditional Keys

| Key | Type | Condition | Description |
|-----|------|-----------|-------------|
| `authenticated` | bool | `credentialRef` provided | Whether credential-based auth succeeded |

### Detection Key Pair (Converter)

The `[SqlConnection]` section in `TestResultDetailsConverter` is triggered when evidence contains both `dbType` AND `connectionSucceeded` keys. No other test type uses either key — confirmed collision-free.

## Details Section Layout

12-character fixed-width label alignment matching SmtpConnect/LdapBind style.

### Always-Present Lines

```
[SqlConnection]
Type:       {dbType}
Server:     {server}
Database:   {database}
Version:    {serverVersion}
Time:       {responseTimeMs >= 0 ? "{responseTimeMs} ms" : "n/a"}
```

### Conditional Lines

```
Auth:       {authenticated ? "yes" : "no"}    ← only when credentialRef provided
```

### Trailing

```
{empty line}
```

## Connection String Builder Mapping

| dbType | Connection Class | Builder Class | Server Prop | Database Prop | Username Prop | Password Prop |
|--------|-----------------|---------------|-------------|---------------|---------------|---------------|
| SqlServer | `SqlConnection` | `SqlConnectionStringBuilder` | `DataSource` | `InitialCatalog` | `UserID` | `Password` |
| PostgreSQL | `NpgsqlConnection` | `NpgsqlConnectionStringBuilder` | `Host` | `Database` | `Username` | `Password` |
| MySQL | `MySqlConnection` | `MySqlConnectionStringBuilder` | `Server` | `Database` | `UserID` | `Password` |

All builders support `Pooling` property (set to `false` for testing).
