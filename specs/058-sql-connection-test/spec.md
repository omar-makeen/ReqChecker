# Feature Specification: SqlConnection Test Type

**Feature Branch**: `058-sql-connection-test`
**Created**: 2026-02-26
**Status**: Draft
**Input**: User description: "I need to add new test SqlConnection │ Database connectivity (SQL Server, PostgreSQL, MySQL) │ connectionString, credentialRef, dbType"

## Clarifications

### Session 2026-02-26

- Q: Should the test disable connection pooling to always measure real TCP connection establishment time, or allow pooling? → A: Disable connection pooling — always measure fresh TCP connection time (inject `Pooling=false` or equivalent internally). This ensures response time reflects actual server reachability.
- Q: Should the evidence capture and display the database server version? → A: Yes — capture `serverVersion` (e.g., "Microsoft SQL Server 2019", "PostgreSQL 16.2") and add a `Version` line to the `[SqlConnection]` details section. Always show on success, `n/a` on failure.
- Q: What exact label layout should the `[SqlConnection]` details section use in the results view? → A: Compact aligned — 12-char fixed-width labels matching SmtpConnect/LdapBind style. Always show: `Type` (database type), `Server` (hostname), `Database` (name), `Version` (server version string), `Time` (ms or `n/a`). Conditionally show: `Auth` (`yes`/`no`, only when `credentialRef` is provided). All always-present fields appear on every outcome; `n/a` for undetermined values on failure paths.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic SQL Server Connectivity Check (Priority: P1)

A database administrator configures a SqlConnection test in a profile to verify that a SQL Server instance is reachable and accepting connections. They provide a connection string and the test opens a connection, executes a lightweight health-check query (`SELECT 1`), and reports pass/fail with connection details. The result shows the database type, server address, database name, response time, and pass/fail status.

**Why this priority**: The core purpose of this test type is to validate that a database server is reachable and responding. SQL Server is the most common database in Windows-centric enterprise environments where ReqChecker is deployed.

**Independent Test**: Can be fully tested by configuring a SqlConnection test with `dbType: "SqlServer"` and a valid `connectionString` pointing to a known SQL Server instance, running it, and verifying the result shows pass/fail with server, database, and response time in the evidence.

**Acceptance Scenarios**:

1. **Given** a profile with a SqlConnection test specifying `dbType: "SqlServer"` and a valid `connectionString`, **When** the test runs and the server accepts the connection, **Then** the result is Pass with evidence showing the database type, server, database name, and response time.
2. **Given** a profile with a SqlConnection test specifying an unreachable server in the connection string, **When** the test runs and the connection times out, **Then** the result is Fail with an error message indicating the database server is unreachable.
3. **Given** a profile with a SqlConnection test specifying an invalid database name in the connection string, **When** the test runs, **Then** the result is Fail with an error message indicating the database does not exist or is inaccessible.

---

### User Story 2 - PostgreSQL and MySQL Connectivity (Priority: P1)

A DevOps engineer needs to verify connectivity to PostgreSQL or MySQL databases in a mixed-database environment. They set the `dbType` parameter to `PostgreSQL` or `MySQL` and provide the appropriate connection string. The test connects to the specified database engine and reports the same structured evidence as SQL Server tests.

**Why this priority**: Multi-database support is essential for organizations that use more than one database engine. PostgreSQL and MySQL are the two most widely deployed open-source databases.

**Independent Test**: Can be tested by configuring a SqlConnection test with `dbType: "PostgreSQL"` (or `"MySQL"`) and a valid connection string, running it, and verifying the result shows pass/fail with database type, server, database name, and response time.

**Acceptance Scenarios**:

1. **Given** a profile with a SqlConnection test where `dbType` is `"PostgreSQL"` and the connection string is valid, **When** the test runs, **Then** the result is Pass with evidence showing `dbType: PostgreSQL`, server, database, and response time.
2. **Given** a profile with a SqlConnection test where `dbType` is `"MySQL"` and the connection string is valid, **When** the test runs, **Then** the result is Pass with evidence showing `dbType: MySQL`, server, database, and response time.
3. **Given** a profile with a SqlConnection test where `dbType` is an unsupported value (e.g., `"Oracle"`), **When** the test runs, **Then** the result is Fail with a configuration error listing the supported database types.

---

### User Story 3 - Authenticated Database Connection via Credential Store (Priority: P2)

A security-conscious administrator wants to validate database connectivity using credentials stored in the application's credential store rather than embedding credentials in the connection string. They specify a `credentialRef` and the test resolves the username/password from the credential store, injects them into the connection string, and reports whether authentication succeeded.

**Why this priority**: Credential store integration avoids plaintext passwords in profile JSON files. Many organizations require credential management for compliance. However, most connection strings already embed credentials or use integrated/Windows authentication, so this extends beyond the core connectivity check.

**Independent Test**: Can be tested by configuring a SqlConnection test with `credentialRef` pointing to valid stored credentials and a connection string without embedded credentials, running it, and verifying the result shows a successful authenticated connection.

**Acceptance Scenarios**:

1. **Given** a profile with a SqlConnection test where `credentialRef` references valid stored credentials, **When** the test runs, **Then** the resolved username/password are injected into the connection and the result shows authentication succeeded.
2. **Given** a profile with a SqlConnection test where `credentialRef` references credentials that do not exist in the credential store, **When** the test runs, **Then** the result is Fail with a configuration error indicating the credential reference could not be resolved.
3. **Given** a profile with a SqlConnection test where `credentialRef` is provided and the connection string also contains embedded credentials, **When** the test runs, **Then** the credential store credentials take precedence (override the connection string values).

---

### User Story 4 - Configuration Validation (Priority: P2)

A user misconfigures a SqlConnection test with missing or invalid parameters. The test detects the configuration error before attempting any network activity and reports a clear, actionable error message.

**Why this priority**: Early validation with clear error messages prevents confusing test failures and speeds up profile debugging.

**Independent Test**: Can be tested by configuring a SqlConnection test with missing `connectionString`, missing `dbType`, or an unsupported `dbType` value, running it, and verifying each produces a specific configuration error without any network calls.

**Acceptance Scenarios**:

1. **Given** a profile with a SqlConnection test where `connectionString` is empty or missing, **When** the test runs, **Then** the result is Fail with a configuration error stating the connection string is required.
2. **Given** a profile with a SqlConnection test where `dbType` is empty or missing, **When** the test runs, **Then** the result is Fail with a configuration error stating the database type is required.
3. **Given** a profile with a SqlConnection test where `dbType` is an unrecognized value, **When** the test runs, **Then** the result is Fail with a configuration error listing the supported database types (SqlServer, PostgreSQL, MySQL).

---

### Edge Cases

- What happens when the connection string is syntactically invalid (e.g., missing semicolons, unrecognized keys)? The test fails with a configuration error indicating the connection string format is invalid.
- What happens when the database server requires SSL but the connection string does not specify it? The test fails with a network or authentication error from the database driver, and the error message is forwarded to the user.
- What happens when `dbType` is provided with incorrect casing (e.g., `"sqlserver"` instead of `"SqlServer"`)? The test performs case-insensitive matching so `"sqlserver"`, `"SQLSERVER"`, and `"SqlServer"` all resolve to SQL Server.
- What happens when the connection string contains `Integrated Security=true` (Windows authentication) for SQL Server? The test uses the current Windows identity for authentication; `credentialRef` is ignored if Windows authentication is specified in the connection string.
- What happens when the database server accepts the connection but the specified database does not exist? The test fails with a descriptive error indicating the database is not accessible.
- What happens when the connection succeeds but the health-check query fails (e.g., permissions)? The test reports Fail with an error message indicating the query failed, since a healthy connection should be able to execute `SELECT 1`.
- What happens when the connection string specifies a non-standard port? The test uses whatever port is specified in the connection string; no separate `port` parameter is needed since the connection string already encodes this.
- What happens when `credentialRef` is provided and the connection string uses Windows/integrated authentication? The `credentialRef` credentials are ignored because integrated authentication takes precedence in the connection string.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support a `SqlConnection` test type that connects to a database server and validates connectivity by opening a connection and executing a lightweight health-check query.
- **FR-002**: System MUST accept a required `connectionString` parameter specifying the database connection string (server address, database name, authentication details, and other driver-specific options).
- **FR-003**: System MUST accept a required `dbType` parameter specifying the database engine. Supported values: `SqlServer`, `PostgreSQL`, `MySQL`. Matching MUST be case-insensitive.
- **FR-004**: System MUST accept an optional `credentialRef` parameter (string) referencing stored username/password credentials for database authentication.
- **FR-005**: When `credentialRef` is provided, the system MUST resolve the username/password from the credential store and inject them into the connection (overriding any credentials in the connection string). For SQL Server: set `User ID` and `Password`. For PostgreSQL: set `Username` and `Password`. For MySQL: set `Uid` and `Pwd`.
- **FR-006**: System MUST open a connection to the database server using the resolved connection string and report Pass when the connection opens successfully and the health-check query returns.
- **FR-007**: After a successful connection, the system MUST execute a lightweight health-check query to verify the database is responsive: `SELECT 1` for all supported database types.
- **FR-008**: System MUST capture evidence including: database type (`dbType`), server address (parsed from connection string), database name (parsed from connection string), server version (retrieved after successful connection), response time (ms), and connection succeeded (boolean).
- **FR-009**: When `credentialRef` is provided, evidence MUST include `authenticated: true/false` indicating whether credential-based authentication succeeded.
- **FR-010**: System MUST report Pass when the connection opens and the health-check query succeeds. System MUST report Fail when the connection or query fails.
- **FR-011**: System MUST validate that `connectionString` is not empty and fail with a configuration error if missing.
- **FR-012**: System MUST validate that `dbType` is one of the supported values (`SqlServer`, `PostgreSQL`, `MySQL`) using case-insensitive comparison, and fail with a configuration error listing the supported types if invalid.
- **FR-013**: System MUST report distinct error messages for: missing connection string, missing/unsupported database type, connection string format error, DNS resolution failure, connection timeout, authentication failure, database not found, and credential reference not resolved.
- **FR-014**: System MUST display SqlConnection test evidence in the results details view under a `[SqlConnection]` section on all outcomes (pass, timeout, auth failure), with compact aligned layout using 12-character fixed-width labels matching SmtpConnect/LdapBind style. Always show: `Type:       ` (database type), `Server:     ` (hostname), `Database:   ` (name), `Version:    ` (server version string, `n/a` if connection failed), `Time:       ` (response time in ms, `n/a` if not determined). Conditionally show: `Auth:       ` (`yes`/`no`, only when `credentialRef` is provided). All "always" fields are present on every outcome; on failure paths where a value couldn't be determined, display `n/a`. End section with an empty line.
- **FR-015**: System MUST enforce a default connection timeout of 15 seconds. If the connection string specifies a different timeout, the connection string value takes precedence.
- **FR-016**: System MUST disable connection pooling for each test execution to ensure the response time reflects actual TCP connection establishment, not pool reuse. The system injects the appropriate driver-specific pooling-disable option (e.g., `Pooling=false` for SQL Server and MySQL, `Pooling=false` for PostgreSQL) into the connection string internally, overriding any user-specified pooling setting.
- **FR-017**: The full connection string MUST NOT be included in evidence data. Only the parsed server address and database name are captured, to avoid exposing credentials or sensitive connection details.
- **FR-018**: System MUST include the SqlConnection test type in the conditional build manifest so it can be included or excluded via the `IncludeTests` build parameter.
- **FR-019**: System MUST update the README to document the SqlConnection test type, its parameters, and usage examples.
- **FR-020**: System MUST update the built-in test type count across the README and TestManifest.props comment to reflect the addition.
- **FR-021**: System MUST add a SqlConnection test entry to the default profile.

### Key Entities

- **SqlConnection Test Parameters**: Configuration for the test including the connection string, database type (SqlServer, PostgreSQL, MySQL), and an optional credential reference for credential-store-based authentication.
- **SqlConnection Evidence**: Runtime data captured during test execution including the database type, server address, database name, server version, response time (ms), connection success status, and authentication result.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can validate SQL Server, PostgreSQL, and MySQL database connectivity by configuring and running a SqlConnection test, receiving a clear pass/fail result with connection details and response time.
- **SC-002**: Test evidence displays all relevant database connection details (type, server, database, response time, authentication result) in the results view.
- **SC-003**: Error messages clearly distinguish between configuration errors (missing connection string, unsupported database type), network failures (unreachable, DNS, timeout), and authentication failures.
- **SC-004**: The test integrates consistently with existing application features: test selection, dependency chaining, retry logic, result history, and PDF export all work with SqlConnection results.

## Assumptions

- Each database type requires its own client library/driver (NuGet packages) to establish connections. SQL Server uses `Microsoft.Data.SqlClient`, PostgreSQL uses `Npgsql`, and MySQL uses `MySqlConnector`. These are standard, well-maintained database drivers.
- The connection string format is database-specific: SQL Server uses `Server=...;Database=...;`, PostgreSQL uses `Host=...;Database=...;`, MySQL uses `Server=...;Database=...;`. The user is responsible for providing a valid connection string for the selected `dbType`.
- Server address and database name are parsed from the connection string for evidence display. Parsing is best-effort — if a field cannot be extracted (e.g., non-standard connection string format), the evidence shows `n/a` for that field.
- The health-check query (`SELECT 1`) is intentionally lightweight and does not require any specific database permissions beyond the ability to connect and execute queries.
- The default timeout is 15 seconds (longer than the 10-second network test default) to accommodate slower database connection handshakes, especially over SSL/TLS or through connection pools. Connection strings that specify their own timeout override this default.
- When `credentialRef` is provided, the resolved username/password are injected into the connection string by setting the appropriate driver-specific keys. This overrides any existing credential fields in the connection string.
- The `connectionString` parameter is treated as sensitive data. The full connection string is NOT included in evidence — only the parsed server and database name are captured.
- Evidence keys follow the project's camelCase convention (e.g., `dbType`, `server`, `database`, `serverVersion`, `responseTimeMs`, `connectionSucceeded`, `authenticated`).
- This test is categorized under "Network" (alongside HttpGet, DnsResolve, TcpConnect, SmtpConnect, LdapBind) since it validates network connectivity to a database service.
- The default profile entry uses `dbType: "SqlServer"` and `connectionString: "Server=db.example.com;Database=master;Integrated Security=true;"` as a placeholder that users will customize for their environment.
