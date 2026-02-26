# Feature Specification: LdapBind Test Type

**Feature Branch**: `057-ldap-bind-test`
**Created**: 2026-02-26
**Status**: Draft
**Input**: User description: "I need to add new test LdapBind │ LDAP/Active Directory connectivity │ server, port, useSsl, credentialRef"

## Clarifications

### Session 2026-02-26

- Q: Should the `[LdapBind]` details section always show all "always" fields (Server, Port, Bind Type, Response Time) on every outcome, including timeout, TLS failure, and authentication failure? → A: Yes — always show all fields on every outcome. On failure paths where a value couldn't be determined, show `n/a`.
- Q: In multi-domain Active Directory environments, should the test follow LDAP referrals (redirects to other servers) or only evaluate the specified server? → A: Disable referral chasing — test only evaluates the specified server. A referral response is treated as a successful connection (server responded) but the test does not follow the redirect.
- Q: What exact label layout should the `[LdapBind]` details section use in the results view? → A: Compact aligned — short labels mirroring SmtpConnect style. Always show: `Server`, `Port`, `Bind` (anonymous/authenticated), `Time` (ms). Conditionally show: `TLS` (version, only when `useSsl` is true), `Auth` (yes/no, only when `credentialRef` is provided).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic LDAP Server Connectivity Check (Priority: P1)

A network engineer configures an LdapBind test in a profile to verify that an LDAP or Active Directory server is reachable and accepting connections. The test connects to the specified server and port, performs an anonymous LDAP bind, and reports whether the bind succeeded. The result shows the server, port, response time, and pass/fail status.

**Why this priority**: The core purpose of this test type is to validate that an LDAP/AD server is reachable and responding to bind requests. Without basic connectivity verification, the test has no value.

**Independent Test**: Can be fully tested by configuring an LdapBind test with a known LDAP server hostname and port 389, running it, and verifying the result shows pass/fail with the server address, port, and response time in the evidence.

**Acceptance Scenarios**:

1. **Given** a profile with an LdapBind test specifying a reachable `server` and `port`, **When** the test runs and the server accepts the anonymous bind, **Then** the result is Pass with evidence showing the server, port, bind type (anonymous), and response time.
2. **Given** a profile with an LdapBind test specifying an unreachable `server`, **When** the test runs and the connection times out, **Then** the result is Fail with an error message indicating the LDAP server is unreachable.
3. **Given** a profile with an LdapBind test specifying a reachable `server` but a non-LDAP `port`, **When** the test runs and the bind fails, **Then** the result is Fail with an error message indicating the server did not respond as an LDAP server.

---

### User Story 2 - SSL/TLS-Secured LDAP Connectivity (Priority: P1)

An IT administrator needs to verify that an LDAP server supports encrypted connections (LDAPS). They enable the `useSsl` parameter and the test connects over implicit SSL (typically port 636). The evidence shows whether the SSL/TLS negotiation succeeded and the TLS protocol version used.

**Why this priority**: Most production LDAP/AD environments require encrypted connections. Verifying LDAPS capability is essential for validating directory infrastructure security compliance.

**Independent Test**: Can be tested by configuring an LdapBind test with `useSsl` set to true against an LDAP server on port 636, running it, and verifying the evidence shows successful TLS negotiation with the protocol version.

**Acceptance Scenarios**:

1. **Given** a profile with an LdapBind test where `useSsl` is true and the server supports LDAPS, **When** the test runs, **Then** the result is Pass with evidence showing TLS negotiation succeeded and the negotiated TLS protocol version.
2. **Given** a profile with an LdapBind test where `useSsl` is true and the server does not support LDAPS on the specified port, **When** the test runs, **Then** the result is Fail with an error message indicating TLS negotiation failed.
3. **Given** a profile with an LdapBind test where `useSsl` is true and `port` is omitted, **When** the test runs, **Then** the port defaults to 636 (standard LDAPS port).

---

### User Story 3 - Authenticated LDAP Bind (Priority: P2)

A directory administrator wants to verify that LDAP authentication works with stored credentials. They configure a `credentialRef` referencing stored credentials (username/password), and the test performs an authenticated simple bind instead of an anonymous bind. The result shows whether authentication succeeded.

**Why this priority**: Verifying authenticated binds is important for validating that service accounts and user credentials can connect to the directory, but it extends beyond basic connectivity and requires credential management. Many users only need to verify reachability and TLS.

**Independent Test**: Can be tested by configuring an LdapBind test with `credentialRef` pointing to valid stored credentials, `useSsl` set to true, running it against an LDAP server, and verifying the evidence shows successful authenticated bind.

**Acceptance Scenarios**:

1. **Given** a profile with an LdapBind test where `credentialRef` references valid stored credentials, **When** the test runs and the authenticated bind succeeds, **Then** the result is Pass with evidence showing authentication succeeded and the bind type (authenticated).
2. **Given** a profile with an LdapBind test where `credentialRef` references invalid credentials, **When** the test runs and the bind is rejected, **Then** the result is Fail with an error message indicating authentication failed (invalid credentials).
3. **Given** a profile with an LdapBind test where `credentialRef` references a credential that does not exist in the credential store, **When** the test runs, **Then** the result is Fail with an error message indicating the credential reference could not be resolved.

---

### User Story 4 - Configuration Validation (Priority: P2)

A user misconfigures an LdapBind test with missing or invalid parameters. The test detects the configuration error before attempting any network activity and reports a clear, actionable error message.

**Why this priority**: Early validation with clear error messages prevents confusing test failures and speeds up profile debugging.

**Independent Test**: Can be tested by configuring an LdapBind test with missing `server`, zero `port`, or out-of-range `port`, running it, and verifying each produces a specific configuration error without any network calls.

**Acceptance Scenarios**:

1. **Given** a profile with an LdapBind test where `server` is empty or missing, **When** the test runs, **Then** the result is Fail with a configuration error stating the server parameter is required.
2. **Given** a profile with an LdapBind test where `port` is zero, negative, or greater than 65535, **When** the test runs, **Then** the result is Fail with a configuration error stating port must be between 1 and 65535.

---

### Edge Cases

- What happens when the server closes the connection immediately after connecting? The test fails with an error message indicating the connection was reset by the server.
- What happens when DNS resolution fails for the server hostname? The test fails with an error message indicating the hostname could not be resolved, before any connection attempt.
- What happens when `port` is omitted and `useSsl` is false? The test defaults to port 389 (standard LDAP).
- What happens when `port` is omitted and `useSsl` is true? The test defaults to port 636 (standard LDAPS).
- What happens when `useSsl` is omitted? The test defaults to false (plain connection on port 389).
- What happens when `credentialRef` is provided but `useSsl` is false? The test still attempts the authenticated bind over the plain connection, but evidence warns that credentials are being sent without encryption.
- What happens when the server rejects anonymous binds? The test fails with an error message indicating the server rejected the anonymous bind.
- What happens when the server requires STARTTLS but `useSsl` is false? The test fails because it does not perform STARTTLS; users should enable `useSsl` for servers requiring encryption.
- What happens when an Active Directory server is used? The test works identically — AD exposes an LDAP interface and accepts standard LDAP bind operations.
- What happens when the server returns an LDAP referral (redirect to another server)? Referral chasing is disabled; the test treats the referral response as a successful connection to the specified server and does not follow the redirect.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support an `LdapBind` test type that connects to an LDAP or Active Directory server and performs a bind operation to validate connectivity and optionally authentication.
- **FR-002**: System MUST accept a required `server` parameter specifying the target LDAP server hostname or IP address.
- **FR-003**: System MUST accept an optional `port` parameter (integer 1–65535) specifying the LDAP server port. When omitted, the default is 389 if `useSsl` is false, or 636 if `useSsl` is true.
- **FR-004**: System MUST accept an optional `useSsl` parameter (boolean) specifying whether to connect over implicit SSL/TLS (LDAPS), defaulting to false.
- **FR-005**: System MUST accept an optional `credentialRef` parameter (string) referencing stored username/password credentials for an authenticated LDAP simple bind.
- **FR-006**: When `credentialRef` is not provided, the system MUST perform an anonymous bind to verify the server accepts connections.
- **FR-007**: When `credentialRef` is provided, the system MUST resolve a username/password credential pair from the existing credential store and perform an authenticated simple bind.
- **FR-008**: When `useSsl` is true, the system MUST establish an SSL/TLS connection (LDAPS) before performing the bind. The system MUST accept all server certificates (including self-signed and expired) without validation, to support internal directory servers.
- **FR-009**: System MUST capture evidence including: server, port, useSsl setting, bind type (anonymous or authenticated), response time (ms), TLS negotiated (yes/no), TLS protocol version (when applicable), and authentication result (when `credentialRef` is provided).
- **FR-010**: System MUST report Pass when the connection is established and the bind operation succeeds (anonymous or authenticated). System MUST report Fail when connection or bind fails.
- **FR-011**: System MUST validate that `server` is not empty and fail with a configuration error if missing.
- **FR-012**: System MUST validate that `port` is between 1 and 65535 (inclusive), failing with a descriptive configuration error for invalid values.
- **FR-013**: System MUST report distinct error messages for: missing server, invalid port, DNS resolution failure, connection timeout, TLS negotiation failure, credential not found, anonymous bind rejected, and authentication failure.
- **FR-014**: System MUST display LdapBind test evidence in the results details view under an `[LdapBind]` section on all outcomes (pass, timeout, TLS failure, auth failure), with compact aligned layout — fixed-width labels on their own lines. Use these exact labels: `Server` (hostname/IP), `Port` (number), `Bind` (`anonymous` or `authenticated`), `Time` (response time in ms). Conditionally display: `TLS` (negotiated protocol version, only when `useSsl` is true), `Auth` (`yes`/`no`, only when `credentialRef` is provided). All "always" fields (Server, Port, Bind, Time) are present on every outcome; on failure paths where a value couldn't be determined, display `n/a`. Conditional fields not applicable to the test configuration MUST be omitted entirely.
- **FR-015**: System MUST disable LDAP referral chasing. The test MUST only evaluate the specified server and MUST NOT follow referrals to other servers. A referral response from the server is treated as a successful connection (the server responded to the bind request).
- **FR-016**: System MUST include the LdapBind test type in the conditional build manifest so it can be included or excluded via the `IncludeTests` build parameter.
- **FR-017**: System MUST update the README to document the LdapBind test type, its parameters, and usage examples.
- **FR-018**: System MUST update the built-in test type count across the README and TestManifest.props comment to reflect the addition.
- **FR-019**: System MUST add an LdapBind test entry to the default profile.

### Key Entities

- **LdapBind Test Parameters**: Configuration for the test including the target server hostname, port number, whether to use SSL/TLS (LDAPS), and an optional credential reference for authenticated binds.
- **LdapBind Evidence**: Runtime data captured during test execution including the server, port, bind type, response time (ms), TLS negotiation status and protocol version, and authentication result.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can validate LDAP/Active Directory server connectivity by configuring and running an LdapBind test, receiving a clear pass/fail result with bind outcome and response time.
- **SC-002**: Test evidence displays all relevant LDAP connection details (server, port, bind type, TLS status, authentication result) in the results view.
- **SC-003**: Error messages clearly distinguish between configuration errors (missing server, invalid port), network failures (unreachable, DNS failure, timeout), TLS failures, and authentication failures.
- **SC-004**: The test integrates consistently with existing application features: test selection, dependency chaining, retry logic, result history, and PDF export all work with LdapBind results.

## Assumptions

- The test uses the `System.DirectoryServices.Protocols` namespace (`LdapConnection`, `LdapDirectoryIdentifier`) which is built into .NET 8.0 on Windows. No new NuGet packages are required.
- When `useSsl` is true, the test uses implicit SSL/TLS (LDAPS) by setting `SessionOptions.SecureSocketLayer = true`. STARTTLS is not used — users who need encrypted connections should set `useSsl: true` and use port 636. Certificate validation is skipped (all certificates accepted) to support internal servers with self-signed certs.
- Anonymous bind is performed by calling `Bind()` with no credentials. Authenticated simple bind is performed by calling `Bind(NetworkCredential)` with the resolved username/password from `credentialRef`.
- The connection timeout is 10 seconds, matching standard network test expectations.
- The default port is context-dependent: 389 for plaintext LDAP, 636 for LDAPS. This is determined by the `useSsl` parameter value.
- Evidence keys follow the project's camelCase convention (e.g., `server`, `port`, `useSsl`, `bindType`, `responseTimeMs`, `tlsNegotiated`, `tlsVersion`, `authenticated`).
- When `credentialRef` is provided without `useSsl`, the test proceeds but the evidence includes a warning that credentials are transmitted without encryption.
- Active Directory servers are fully compatible since AD implements the standard LDAP protocol. The `server` parameter can be a domain controller hostname, IP address, or AD domain name.
- The default profile entry uses `server: "dc.example.com"` and `port: 389` as a placeholder that users will customize for their environment.
- This test is categorized under "Network" (alongside HttpGet, DnsResolve, TcpConnect, SmtpConnect) since it validates network connectivity to a directory service.
