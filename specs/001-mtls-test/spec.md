# Feature Specification: Mutual TLS Client Certificate Authentication Test

**Feature Branch**: `001-mtls-test`
**Created**: 2026-02-13
**Status**: Draft
**Input**: User description: "I need to create new test Mutual TLS / client certificate auth"

## Clarifications

### Session 2026-02-13

- Q: What certificate input format should the test use — PFX/PKCS#12 (Windows native, single file) vs PEM (separate cert + key files)? → A: PFX/PKCS#12 only. This is the native Windows format, aligns with .NET's X509Certificate2, and simplifies UX to a single file picker with optional password.
- Q: What defines test success — TLS handshake only, or TLS handshake + HTTP response verification? → A: TLS handshake + HTTP response. Success means the TLS handshake completed with the client cert AND the server returned the expected HTTP status code (e.g., 200). This confirms the full end-to-end authentication flow.
- Q: How should the PFX password be handled — stored in profile JSON or prompted at runtime? → A: PromptAtRun only. The PFX password is never persisted in the profile; the user is prompted at test execution time. This follows the existing secure credential pattern (credentialRef / PromptAtRun field policy) used by FTP tests.
- Q: Should the test support loading certificates from the Windows Certificate Store (by thumbprint) in addition to PFX files? → A: PFX file only for the initial release. Windows Certificate Store support deferred as a future enhancement to keep the initial scope tight and shippable.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Verify Server Requires Client Certificate (Priority: P1)

A user needs to test whether a service endpoint properly enforces mutual TLS by requiring clients to present valid certificates for authentication. This verifies that the service rejects unauthenticated connections and only allows connections from clients with valid certificates.

**Why this priority**: This is the core security validation - ensuring the server actually enforces client certificate requirements. Without this, mTLS security is not effective.

**Independent Test**: Can be fully tested by configuring an mTLS endpoint URL, providing a valid client PFX certificate file, and verifying the TLS handshake succeeds and the server returns the expected HTTP status code.

**Acceptance Scenarios**:

1. **Given** a service endpoint configured for mutual TLS, **When** the test runs with a valid client PFX certificate, **Then** the TLS handshake succeeds and the server returns the expected HTTP status code
2. **Given** a service endpoint configured for mutual TLS, **When** the test runs without a client certificate, **Then** the TLS handshake is rejected with an authentication failure
3. **Given** a service endpoint configured for mutual TLS, **When** the test runs with an invalid or expired client certificate, **Then** the connection is rejected with a certificate validation error

---

### User Story 2 - Verify Certificate Chain Trust (Priority: P2)

A user needs to test that the service properly validates the entire certificate chain, including intermediate certificates and the trusted root CA. This ensures the mTLS implementation follows proper PKI trust validation.

**Why this priority**: While less critical than basic certificate requirement, proper chain validation prevents sophisticated attacks using certificates from untrusted CAs.

**Independent Test**: Can be tested independently by providing PFX files with different chain configurations and verifying proper validation behavior.

**Acceptance Scenarios**:

1. **Given** a client PFX containing a certificate signed by a trusted CA with the complete chain, **When** the test runs, **Then** the TLS handshake succeeds and the server returns the expected HTTP status code
2. **Given** a client PFX containing a certificate signed by an untrusted CA, **When** the test runs, **Then** the connection is rejected with a trust validation error
3. **Given** a client PFX with an incomplete certificate chain, **When** the test runs, **Then** the connection fails with a chain validation error

---

### User Story 3 - Test Certificate Expiration Handling (Priority: P3)

A user needs to verify that the service properly handles expired client certificates and certificates that are not yet valid, ensuring time-based certificate validation is working correctly.

**Why this priority**: Important for operational security but less critical than the core authentication mechanism. Expired certificates should be rare in production.

**Independent Test**: Can be tested independently by attempting connections with PFX files containing certificates with various validity date ranges.

**Acceptance Scenarios**:

1. **Given** a PFX containing an expired client certificate, **When** the test runs, **Then** the connection is rejected with an expiration error
2. **Given** a PFX containing a client certificate with a future "not before" date, **When** the test runs, **Then** the connection is rejected with a validity period error
3. **Given** a PFX containing a client certificate within its validity period, **When** the test runs, **Then** the TLS handshake succeeds and the server returns the expected HTTP status code

---

### Edge Cases

- What happens when the PFX file path is invalid or the file is not readable?
- What happens when the PFX password is incorrect?
- What happens when the PFX file is corrupted or not a valid PKCS#12 file?
- How does the system handle certificate revocation (CRL/OCSP)?
- What happens when the service endpoint uses a self-signed server certificate?
- How does the system handle certificate hostname/SAN mismatch?
- What happens when the PFX file does not contain a private key?
- What happens when the TLS handshake succeeds but the HTTP response status does not match the expected status?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support testing HTTPS endpoints that require client certificate authentication (mutual TLS)
- **FR-002**: System MUST allow users to configure the target endpoint URL for mTLS testing
- **FR-003**: System MUST allow users to specify the client certificate PFX/PKCS#12 file path
- **FR-004**: System MUST support PFX files with optional password protection. The password MUST NOT be persisted in the profile; users are prompted at test execution time
- **FR-005**: System MUST validate that the PFX file can be loaded and contains both a valid certificate and private key before attempting connection
- **FR-006**: System MUST allow users to configure an expected HTTP status code (default: 200) to verify the server response after successful TLS handshake
- **FR-007**: System MUST report success only when both the mTLS handshake completes and the server returns the expected HTTP status code
- **FR-008**: System MUST report failure with specific error details when certificate authentication fails or the HTTP response does not match
- **FR-009**: System MUST distinguish between different failure types (certificate rejected, chain invalid, expired certificate, connection refused, incorrect PFX password, unexpected HTTP status, etc.)
- **FR-010**: System MUST allow users to optionally specify a trusted CA certificate for server certificate validation
- **FR-011**: System MUST support testing endpoints with self-signed server certificates by providing an explicit opt-in option to skip server certificate validation. Server validation MUST be enabled by default; users must deliberately enable the "Skip Server Certificate Validation" setting to bypass it
- **FR-012**: System MUST timeout connections that take longer than a configured threshold (default: 30 seconds)
- **FR-013**: System MUST include response time metrics in test results

### Key Entities

- **Mutual TLS Test**: Represents a test configuration for verifying client certificate authentication, including endpoint URL, PFX file path, optional password, expected HTTP status code, and validation settings
- **Test Result**: Captures the outcome of the mTLS connection attempt, including success/failure status, specific error details, certificate validation information, HTTP response status, and response time

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can successfully test mTLS endpoints and receive clear pass/fail results within 5 seconds for responsive servers
- **SC-002**: System accurately distinguishes between at least 6 different failure scenarios (certificate rejected, expired, chain invalid, server unreachable, timeout, unexpected HTTP status)
- **SC-003**: Test results include actionable error messages that help users identify and resolve certificate configuration issues
- **SC-004**: Users can configure and run an mTLS test with all required parameters in under 2 minutes

## Assumptions

1. Certificate files are in PFX/PKCS#12 format (Windows/.NET standard)
2. Default connection timeout of 30 seconds is reasonable for most network environments
3. Users have access to valid client PFX files for testing
4. Server certificate validation follows standard TLS trust practices unless explicitly disabled
5. Most users will test internal services or development endpoints where self-signed certificates are common
6. The PFX file may contain the full certificate chain (intermediates + root); no separate chain file is needed
7. The test uses HTTP GET to verify the server response after TLS handshake (consistent with existing HttpGet test pattern)
8. Certificate source is file-based PFX only; Windows Certificate Store (thumbprint) support is out of scope for initial release
