# Feature Specification: SSL Certificate Expiry Test

**Feature Branch**: `041-cert-expiry-test`
**Created**: 2026-02-18
**Status**: Draft
**Input**: User description: "Add new test CertificateExpiry to validate remote SSL certificate validity window"

## Clarifications

### Session 2026-02-18

- Q: Should the test support STARTTLS protocol negotiation (SMTP port 25/587, LDAP port 389) or only direct TLS connections? → A: Direct TLS only — connect and immediately perform TLS handshake. STARTTLS is out of scope and would belong in a separate test type if needed.
- Q: Should `expectedSubject` match against Subject DN only, or also check Subject Alternative Names (SANs)? → A: Match against both Subject DN and SAN entries — either matching constitutes a pass. This aligns with RFC 6125 and modern certificate practices where hostnames are placed in SANs.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Check Remote Certificate Validity (Priority: P1)

An IT administrator needs to proactively verify that SSL/TLS certificates on critical endpoints (web servers, API gateways, mail servers) are currently valid and not approaching expiration. The administrator configures a test profile with the target host, runs the test suite, and receives a clear pass/fail result indicating whether the certificate is valid and has sufficient remaining lifetime before expiry.

**Why this priority**: Certificate expiration is one of the most common causes of service outages and security warnings. Proactive monitoring of certificate validity windows is the core value proposition of this test type.

**Independent Test**: Can be fully tested by configuring a CertificateExpiry test targeting a known public endpoint (e.g., www.google.com:443), running the test, and verifying a "Pass" result with evidence showing the certificate subject, issuer, expiry date, and days remaining.

**Acceptance Scenarios**:

1. **Given** a test profile with a CertificateExpiry test targeting a host with a valid, non-expiring-soon certificate, **When** the test executes, **Then** the result shows "Pass" with evidence containing the certificate subject, issuer, validity dates, and days until expiry
2. **Given** a test profile with a CertificateExpiry test targeting a host whose certificate has already expired, **When** the test executes, **Then** the result shows "Fail" with error category "Validation" and a human-readable message indicating the certificate expired on a specific date
3. **Given** a test profile with a CertificateExpiry test targeting a host whose certificate expires within the configured warning window (e.g., 30 days), **When** the test executes, **Then** the result shows "Fail" with error category "Validation" and a message indicating the certificate expires in N days
4. **Given** a test profile with a CertificateExpiry test targeting an unreachable host, **When** the test executes, **Then** the result shows "Fail" with error category "Network" and technical details explaining the connection failure

---

### User Story 2 - Certificate Identity Verification (Priority: P2)

An administrator needs to verify not only that a certificate is valid, but that it belongs to the expected organization or matches expected attributes. The administrator configures optional expected values for the certificate subject, issuer, or thumbprint. The test validates the certificate's identity attributes against these expected values in addition to checking the validity window.

**Why this priority**: Certificate identity verification catches misconfigurations such as a wrong certificate deployed to a server, a certificate replaced by an untrusted issuer, or a man-in-the-middle scenario. This adds security assurance beyond simple expiry checking.

**Independent Test**: Can be tested by configuring a CertificateExpiry test with an `expectedSubject` value matching the target host, running the test, and verifying the result confirms the certificate subject matches.

**Acceptance Scenarios**:

1. **Given** a test configured with an `expectedSubject` value, **When** the remote certificate's Subject DN or any SAN entry matches that value, **Then** the result shows "Pass"
2. **Given** a test configured with an `expectedSubject` value, **When** neither the Subject DN nor any SAN entry matches that value, **Then** the result shows "Fail" with error category "Validation" and details showing expected value vs. actual subject and SANs
3. **Given** a test configured with an `expectedIssuer` value, **When** the remote certificate's Issuer field does not match, **Then** the result shows "Fail" with error category "Validation"
4. **Given** a test configured with an `expectedThumbprint` value, **When** the remote certificate's thumbprint does not match (case-insensitive), **Then** the result shows "Fail" with error category "Validation"

---

### User Story 3 - Field-Level Policy Support (Priority: P3)

A company distributes a test profile where the target hosts are locked to corporate endpoints but the warning threshold is editable by end users. The CertificateExpiry test integrates with the existing field-level policy system (Locked/Editable/Hidden/PromptAtRun) to enforce these constraints.

**Why this priority**: Consistency with existing ReqChecker field-level policy architecture. All test types support field policies, and CertificateExpiry tests must integrate seamlessly.

**Independent Test**: Can be tested by loading a profile with a CertificateExpiry test where the `host` parameter has policy "Locked" and the `warningDaysBeforeExpiry` parameter has policy "Editable", verifying the UI renders the host field as read-only and the warning threshold as editable.

**Acceptance Scenarios**:

1. **Given** a profile with CertificateExpiry test parameters marked as "Locked", **When** the user views the test configuration page, **Then** those fields display with lock icons and cannot be edited
2. **Given** a profile with CertificateExpiry test parameters marked as "PromptAtRun", **When** the user starts the test run, **Then** the system prompts for those values before executing

---

### Edge Cases

- What happens when the remote server presents a self-signed certificate? The test should still retrieve and evaluate the certificate's validity window; chain validation is a separate concern controlled by `skipChainValidation`.
- What happens when the remote server requires client authentication (mTLS) to complete the TLS handshake? The test fails with error category "Network" since this test type does not provide client certificates (the MtlsConnect test covers that scenario).
- What happens when the remote server presents an intermediate certificate instead of a leaf certificate? The test evaluates whichever certificate the server presents as its leaf.
- What happens when the TLS port is not 443 (e.g., LDAPS on 636, SMTPS on 465, custom port)? The `port` parameter supports any valid port number (1-65535).
- What happens when the certificate's NotBefore date is in the future? The test fails with error category "Validation" indicating the certificate is not yet valid.
- What happens when the host resolves to an IPv6 address? The system handles both IPv4 and IPv6 transparently.
- What happens when `warningDaysBeforeExpiry` is set to 0? The test only fails if the certificate is already expired, effectively disabling the warning window.
- What happens when the connection times out before the TLS handshake completes? The test fails with error category "Timeout".
- What happens when the target service requires STARTTLS negotiation (e.g., SMTP on port 25/587, LDAP on port 389)? This is out of scope — the test only supports direct TLS connections. Services requiring STARTTLS will fail with error category "Network".

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST establish a direct TLS connection to the specified host and port and retrieve the remote server's leaf certificate. STARTTLS protocol negotiation (SMTP, LDAP, etc.) is explicitly out of scope
- **FR-002**: System MUST evaluate the certificate's validity window by comparing `NotBefore` and `NotAfter` dates against the current date/time (UTC)
- **FR-003**: System MUST mark the test as "Pass" only when the certificate is currently valid AND the number of days until expiry is greater than or equal to the configured warning threshold
- **FR-004**: System MUST mark the test as "Fail" with error category "Validation" when the certificate has already expired
- **FR-005**: System MUST mark the test as "Fail" with error category "Validation" when the certificate is not yet valid (NotBefore is in the future)
- **FR-006**: System MUST mark the test as "Fail" with error category "Validation" when the certificate expires within the configured warning window
- **FR-007**: System MUST support a configurable warning threshold (`warningDaysBeforeExpiry`) with a default of 30 days
- **FR-008**: System MUST support a configurable connection timeout with a default of 10 seconds
- **FR-009**: System MUST support a configurable TLS port with a default of 443
- **FR-010**: System MUST mark the test as "Fail" with error category "Network" when the TLS connection cannot be established (host unreachable, connection refused, DNS resolution failure, client certificate required)
- **FR-011**: System MUST mark the test as "Fail" with error category "Timeout" when the TLS handshake does not complete within the configured timeout
- **FR-012**: System MUST validate that the port parameter is within the valid range (1-65535) and mark as "Fail" with error category "Configuration" for invalid values
- **FR-013**: System MUST support optional certificate identity verification by matching the `expectedSubject` value against both the Subject DN field and Subject Alternative Name (SAN) entries — a match in either constitutes a pass (substring match for Subject DN, exact match for individual SAN entries)
- **FR-014**: System MUST support optional certificate issuer verification by matching the Issuer field against an expected value (substring match)
- **FR-015**: System MUST support optional certificate thumbprint verification (case-insensitive exact match)
- **FR-016**: System MUST mark the test as "Fail" with error category "Validation" when any configured identity/issuer/thumbprint assertion fails
- **FR-017**: System MUST support a `skipChainValidation` option (default false) to allow retrieving certificates from endpoints with self-signed or untrusted chains
- **FR-018**: System MUST capture certificate evidence including: Subject, Issuer, Thumbprint, NotBefore, NotAfter, days until expiry, Subject Alternative Names, and TLS protocol version
- **FR-019**: System MUST provide a human-readable summary message (e.g., "Certificate for example.com expires in 45 days (2026-04-04)" or "Certificate for example.com expired 3 days ago (2026-02-15)")
- **FR-020**: System MUST provide technical details including all captured certificate evidence fields and connection metadata
- **FR-021**: System MUST support the standard field-level policies (Locked, Editable, Hidden, PromptAtRun) for all test parameters
- **FR-022**: System MUST support both IPv4 and IPv6 addresses as well as DNS hostnames for the target host
- **FR-023**: System MUST be cancellable via cancellation token and mark as "Skipped" when cancelled

### Key Entities

- **CertificateExpiryTest**: A test definition representing a remote SSL/TLS certificate validity check
  - Attributes: host (string, required), port (integer, default 443), warningDaysBeforeExpiry (integer, default 30), timeout (integer, default 10000ms), skipChainValidation (boolean, default false), expectedSubject (string, optional), expectedIssuer (string, optional), expectedThumbprint (string, optional)
  - Relationships: Inherits from TestDefinition, produces TestResult

- **CertificateExpiryTestEvidence**: Structured evidence captured during certificate validation
  - Attributes: host (string), port (integer), subject (string), issuer (string), thumbprint (string), notBefore (datetime), notAfter (datetime), daysUntilExpiry (integer, negative if expired), isExpired (boolean), isNotYetValid (boolean), expiresWithinWarningWindow (boolean), subjectAlternativeNames (string array), tlsProtocolVersion (string), responseTimeMs (integer)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Administrators can verify certificate validity for a remote endpoint in under 10 seconds (given typical network latency)
- **SC-002**: System correctly identifies expired, not-yet-valid, and soon-to-expire certificates with 100% accuracy when the TLS handshake succeeds
- **SC-003**: Test results provide sufficient certificate detail (subject, issuer, dates, days remaining, SANs) for administrators to assess certificate health without needing external tools
- **SC-004**: CertificateExpiry tests integrate seamlessly with existing test runner infrastructure (sequential execution, cancellation, retry, dependency support)
- **SC-005**: 100% of CertificateExpiry test parameters support field-level policies (Locked/Editable/Hidden/PromptAtRun) consistent with other test types
- **SC-006**: The configurable warning threshold enables organizations to detect certificates approaching expiry with sufficient lead time for renewal (default 30 days covers typical certificate renewal workflows)
