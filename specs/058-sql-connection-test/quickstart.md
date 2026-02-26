# Quickstart: SqlConnection Test Type

**Feature**: 058-sql-connection-test

## Profile Configuration Examples

### SQL Server (Anonymous / Windows Auth)

```json
{
  "id": "test-044",
  "type": "SqlConnection",
  "displayName": "SQL Server Connectivity",
  "description": "Verifies SQL Server database connectivity.",
  "parameters": {
    "dbType": "SqlServer",
    "connectionString": "Server=db.example.com;Database=master;Integrated Security=true;"
  },
  "fieldPolicy": {
    "dbType": "Editable",
    "connectionString": "Editable",
    "credentialRef": "Editable"
  }
}
```

### PostgreSQL

```json
{
  "id": "test-045",
  "type": "SqlConnection",
  "displayName": "PostgreSQL Connectivity",
  "parameters": {
    "dbType": "PostgreSQL",
    "connectionString": "Host=pgserver.example.com;Database=myapp;Username=appuser;Password=secret;"
  }
}
```

### MySQL with Credential Store

```json
{
  "id": "test-046",
  "type": "SqlConnection",
  "displayName": "MySQL Connectivity",
  "parameters": {
    "dbType": "MySQL",
    "connectionString": "Server=mysql.example.com;Database=inventory;",
    "credentialRef": "mysql-prod-creds"
  }
}
```

## Expected Test Output

### Success (SQL Server)

```
[SqlConnection]
Type:       SqlServer
Server:     db.example.com
Database:   master
Version:    16.00.4165
Time:       142 ms
```

### Success (PostgreSQL with credentialRef)

```
[SqlConnection]
Type:       PostgreSQL
Server:     pgserver.example.com
Database:   myapp
Version:    16.2
Time:       89 ms
Auth:       yes
```

### Failure (Timeout)

```
[SqlConnection]
Type:       MySQL
Server:     mysql.example.com
Database:   inventory
Version:    n/a
Time:       n/a
```

Error: `Database server unreachable: connection timed out`

### Failure (Configuration)

Error: `Database type is required. Supported types: SqlServer, PostgreSQL, MySQL`

## Integration Scenarios

1. **Basic connectivity**: Configure with `dbType` + `connectionString`, run test, verify Pass/Fail
2. **Multi-database profile**: Add SqlConnection tests for each database in a single profile (SQL Server + PostgreSQL + MySQL)
3. **Credential store auth**: Set `credentialRef`, omit credentials from connection string
4. **Dependency chaining**: Use `dependsOn` to ensure network connectivity (e.g., DnsResolve or TcpConnect) passes before attempting database connection
5. **Selective build**: Build with only SqlConnection via `dotnet build /p:IncludeTests="SqlConnection"`
