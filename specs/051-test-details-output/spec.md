# Feature Specification: Test Details Output for All Test Types

**Feature Branch**: `051-test-details-output`
**Created**: 2026-02-23
**Status**: Draft
**Input**: User description: "Fix missing dedicated details output for all test types that currently only show [General] + [Timing] in the results view"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Network Test Details (Priority: P1)

A network engineer runs Ping, DnsResolve, TcpPortOpen, and UdpPortOpen tests and views the results. Currently these tests only show duration and attempt count in the details view, even though they capture rich diagnostic data (ping statistics, resolved addresses, connection times, port status). After this fix, each network test displays its key evidence in a dedicated section so the engineer can diagnose issues without needing to inspect raw JSON.

**Why this priority**: Network tests are the most commonly used test types and produce the most diagnostic data. Ping and DnsResolve in particular are foundational diagnostic tools where seeing the actual results (not just pass/fail) is critical.

**Independent Test**: Run each of the four network tests, view the results details, and verify each displays a dedicated section with its key evidence fields.

**Acceptance Scenarios**:

1. **Given** a completed Ping test, **When** the user views the result details, **Then** a `[Ping]` section displays the target host, success rate, average round-trip time, and per-attempt results.
2. **Given** a completed DnsResolve test, **When** the user views the result details, **Then** a `[DNS]` section displays the hostname, resolved addresses, address count, and resolution time.
3. **Given** a completed TcpPortOpen test, **When** the user views the result details, **Then** a `[TCP]` section displays the host, port, connection status, and connect time.
4. **Given** a completed UdpPortOpen test, **When** the user views the result details, **Then** a `[UDP]` section displays the response status, round-trip time, payload sizes, and response data preview.

---

### User Story 2 - System & Security Test Details (Priority: P1)

An IT administrator runs DiskSpace, WindowsService, MtlsConnect, and CertificateExpiry tests. Currently these show only duration in the details view. After this fix, each displays a dedicated section: disk usage statistics, service status, mTLS certificate details, and certificate expiry information. This lets the administrator understand the actual system state without needing external tools.

**Why this priority**: These tests capture high-value diagnostic data (disk usage percentages, certificate expiry dates, service states) that is essential for system readiness assessment. Hiding this data behind a pass/fail indicator significantly reduces the tool's usefulness.

**Independent Test**: Run each of the four system/security tests, view the results details, and verify each displays a dedicated section with its key evidence fields.

**Acceptance Scenarios**:

1. **Given** a completed DiskSpace test, **When** the user views the result details, **Then** a `[Disk Space]` section displays the path, total space, free space, percent free, minimum required, and whether the threshold was met.
2. **Given** a completed WindowsService test, **When** the user views the result details, **Then** a `[Service]` section displays the service name, display name, current status, expected status, start type, and whether the status matched.
3. **Given** a completed MtlsConnect test, **When** the user views the result details, **Then** a `[mTLS]` section displays the connection status, certificate subject, issuer, thumbprint, validity dates, and whether the certificate has a private key.
4. **Given** a completed CertificateExpiry test, **When** the user views the result details, **Then** a `[Certificate]` section displays the host, subject, issuer, thumbprint, expiry date, days until expiry, and expiry/validity status.

---

### User Story 3 - File System Test Details (Priority: P2)

A QA engineer runs FileExists and DirectoryExists tests. Currently these show only duration. After this fix, each displays a dedicated section showing the path, existence status, and file/directory metadata (size, timestamps, attributes, counts). This helps the engineer confirm not just that a file exists, but its key properties.

**Why this priority**: File system tests are simpler and their pass/fail result is more self-explanatory than network or security tests. The additional metadata is helpful but less critical for immediate diagnosis.

**Independent Test**: Run FileExists and DirectoryExists tests, view the results details, and verify each displays a dedicated section with its key evidence fields.

**Acceptance Scenarios**:

1. **Given** a completed FileExists test, **When** the user views the result details, **Then** a `[File]` section displays the path, exists status, expected status, file size, and last modified date.
2. **Given** a completed DirectoryExists test, **When** the user views the result details, **Then** a `[Directory]` section displays the path, exists status, expected status, file count, directory count, and creation time.

---

### Edge Cases

- What happens when an evidence field is null or missing? The field is silently omitted from the section (consistent with how existing converter sections handle missing data).
- What happens when the test fails with an error (e.g., exception before evidence is collected)? The dedicated section is not rendered (evidenceData will be null), and only `[General]` and error information are shown. This is the existing behavior.
- What happens when evidence data overlaps with generic sections (e.g., MtlsConnect sets ResponseCode which triggers `[Response]`)? Both the dedicated section and the generic section are rendered, giving the user complete information.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a `[Ping]` section for Ping test results showing: host, success count/total, success rate, and average round-trip time.
- **FR-002**: System MUST display a `[DNS]` section for DnsResolve test results showing: hostname, resolved addresses (listed individually), address count, and resolution time.
- **FR-003**: System MUST display a `[TCP]` section for TcpPortOpen test results showing: host, port, connection status, and connect time.
- **FR-004**: System MUST display a `[UDP]` section for UdpPortOpen test results showing: response status, round-trip time, payload sizes, and response data preview (if available).
- **FR-005**: System MUST display a `[Disk Space]` section for DiskSpace test results showing: path, total space, free space, percent free, minimum required, and threshold status.
- **FR-006**: System MUST display a `[Service]` section for WindowsService test results showing: service name, display name, current status, expected status, start type, and status match.
- **FR-007**: System MUST display a `[mTLS]` section for MtlsConnect test results showing: connection status, response time, certificate subject, issuer, thumbprint, validity dates, and private key status.
- **FR-008**: System MUST display a `[Certificate]` section for CertificateExpiry test results showing: host, port, subject, issuer, thumbprint, expiry date, days until expiry, and expired/valid status.
- **FR-009**: System MUST display a `[File]` section for FileExists test results showing: path, exists status, expected exists status, file size, and last modified date.
- **FR-010**: System MUST display a `[Directory]` section for DirectoryExists test results showing: path, exists status, expected exists status, file count, directory count, and creation time.
- **FR-011**: Each dedicated section MUST silently omit fields that are null or missing from the evidence data (consistent with existing converter behavior).
- **FR-012**: Each dedicated section MUST be detected using evidence keys unique to that test type to avoid false matches with other test types.

### Key Entities

- **Test Evidence Sections**: Each of the 10 new converter sections maps evidence dictionary keys to formatted display lines. Each section is identified by a unique combination of evidence keys.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 26 test types display meaningful diagnostic information in the results details view — zero test types show only `[General]` + `[Timing]`.
- **SC-002**: Users can identify the root cause of a test failure from the details view without needing to inspect raw JSON evidence data.
- **SC-003**: Each new section renders within the existing converter without introducing additional user-facing latency.
- **SC-004**: Existing test types that already have dedicated sections (OsVersion, InstalledSoftware, EnvironmentVariable, SystemRam, CpuCores, WebSocket, Proxy, Traceroute) continue to render identically.

## Assumptions

- All 10 test types already capture the necessary evidence data in their `ResponseData` JSON — this feature only adds rendering, not data collection.
- The converter detects each section using evidence key combinations unique to each test type (e.g., Ping is detected by `successRate` + `pingResults`, DnsResolve by `hostname` + `addresses`).
- Section naming follows existing conventions: short, descriptive bracketed names (e.g., `[Ping]`, `[DNS]`, `[TCP]`).
- Display formatting follows existing patterns: aligned key-value pairs with consistent label widths.
- The 10 affected test types are: Ping, DnsResolve, TcpPortOpen, UdpPortOpen, DiskSpace, WindowsService, MtlsConnect, CertificateExpiry, FileExists, DirectoryExists.
- This feature modifies a single file: the details converter. No changes to test implementations or other application files are needed.
