# Feature Specification: New Test Types (DnsResolve, TcpPortOpen, WindowsService, DiskSpace)

**Feature Branch**: `033-new-test-types`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "New Test Types: DnsResolve, TcpPortOpen, WindowsService, DiskSpace — IT admins validating deployment readiness commonly need: 'Can DNS resolve this hostname?', 'Is port 443 open?', 'Is the SQL Server service running?', 'Is there 10 GB free on C:?'. These are standard pre-deployment checks we can't express today."

## Clarifications

### Session 2026-02-07

- Q: Should the `WindowsService` test accept service lookup by display name in addition to internal service name? → A: Internal service name only (e.g., `MSSQLSERVER`). Simpler, unambiguous, locale-independent.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — DNS Resolution Check (Priority: P1)

An IT admin building a deployment-readiness profile adds a "DNS Resolution" test to verify that an internal hostname (e.g., `sqlserver.corp.local`) resolves to an expected IP address before the application installer attempts to connect. When the test runs, it resolves the hostname and reports the returned IP addresses, pass/fail status, and resolution time.

**Why this priority**: DNS resolution is the most fundamental network prerequisite — if DNS fails, nothing downstream works. It also fills a gap: the sample profile already references a "DnsLookup" test type that has no implementation.

**Independent Test**: Can be fully tested by adding a DnsResolve test to any profile and running it against a known hostname; delivers immediate value by confirming DNS infrastructure health.

**Acceptance Scenarios**:

1. **Given** a profile with a DnsResolve test for `www.google.com`, **When** the test runs on a machine with working DNS, **Then** the test passes and evidence includes the resolved IP address(es) and resolution time.
2. **Given** a profile with a DnsResolve test for `nonexistent.invalid`, **When** the test runs, **Then** the test fails with a clear "DNS resolution failed" message and a Network error category.
3. **Given** a profile with a DnsResolve test that specifies an expected IP address, **When** the hostname resolves to a different IP, **Then** the test fails with a Validation error indicating the mismatch.
4. **Given** a profile with a DnsResolve test, **When** the test is cancelled mid-execution, **Then** the test is marked as Skipped.

---

### User Story 2 — TCP Port Connectivity Check (Priority: P1)

An IT admin adds a "TCP Port Open" test to verify that a specific host and port combination is reachable (e.g., `sqlserver.corp.local:1433` for SQL Server or `proxy.corp.local:443` for HTTPS). The test attempts a TCP connection and reports whether the port is open, connection time, and any errors.

**Why this priority**: Port connectivity is a critical deployment prerequisite — knowing a service is listening before attempting to install or configure dependent software prevents cryptic downstream failures.

**Independent Test**: Can be fully tested by adding a TcpPortOpen test targeting any reachable host:port and running the profile; delivers value by confirming network path and firewall rules.

**Acceptance Scenarios**:

1. **Given** a profile with a TcpPortOpen test for `www.google.com` port `443`, **When** the test runs on a machine with internet access, **Then** the test passes and evidence includes the connection time.
2. **Given** a profile with a TcpPortOpen test for a host on a blocked port, **When** the test runs, **Then** the test fails with a Network error category and a clear "connection refused" or "timed out" message.
3. **Given** a profile with a TcpPortOpen test with a 2-second timeout, **When** the target is unreachable and the timeout elapses, **Then** the test fails with a Timeout error category.
4. **Given** a profile with a TcpPortOpen test, **When** the test is cancelled mid-connection, **Then** the test is marked as Skipped.

---

### User Story 3 — Windows Service Status Check (Priority: P2)

An IT admin adds a "Windows Service" test to verify that a required service (e.g., `MSSQLSERVER`, `W3SVC`) is installed and running before deploying an application that depends on it. The test queries the service status and reports whether it is running, stopped, or not installed.

**Why this priority**: Checking service state is a common pre-deployment need, but it is Windows-specific and slightly narrower in scope than the network-related tests.

**Independent Test**: Can be fully tested by adding a WindowsService test targeting a known Windows service (e.g., `Winmgmt`) and running the profile; delivers value by confirming service dependencies.

**Acceptance Scenarios**:

1. **Given** a profile with a WindowsService test for `Winmgmt` (WMI), **When** the test runs on a Windows machine where the service is running, **Then** the test passes and evidence includes the service display name, status, and start type.
2. **Given** a profile with a WindowsService test for a service that is installed but stopped, **When** the test runs, **Then** the test fails with a Validation error and a message indicating the service exists but is not running.
3. **Given** a profile with a WindowsService test for a service name that does not exist, **When** the test runs, **Then** the test fails with a clear "service not found" message.
4. **Given** a profile with a WindowsService test, **When** the test runs on a non-Windows platform, **Then** the test is skipped with a message indicating it requires Windows.

---

### User Story 4 — Disk Space Check (Priority: P2)

An IT admin adds a "Disk Space" test to verify that a target drive (e.g., `C:`) has a minimum amount of free space (e.g., 10 GB) before deploying large application packages. The test queries the drive and reports available space, total space, and whether the minimum threshold is met.

**Why this priority**: Disk space checks prevent failed installations due to insufficient storage — a common deployment blocker — but are simpler and less urgent than network prerequisites.

**Independent Test**: Can be fully tested by adding a DiskSpace test targeting the system drive with a reasonable threshold and running the profile; delivers value by confirming storage readiness.

**Acceptance Scenarios**:

1. **Given** a profile with a DiskSpace test for drive `C:` with a minimum of 1 GB, **When** the test runs on a machine with more than 1 GB free, **Then** the test passes and evidence includes total space, free space, and percentage free.
2. **Given** a profile with a DiskSpace test for drive `C:` with a minimum of 999999 GB, **When** the test runs, **Then** the test fails with a Validation error indicating insufficient free space and showing the actual vs. required amounts.
3. **Given** a profile with a DiskSpace test for a drive letter that does not exist (e.g., `Z:`), **When** the test runs, **Then** the test fails with a Configuration error and a "drive not found" message.
4. **Given** a profile with a DiskSpace test, **When** the test runs on a non-Windows platform, **Then** the test accepts a path (e.g., `/`) instead of a drive letter and checks the mount point.

---

### Edge Cases

- What happens when DNS resolution returns multiple IP addresses? The test passes if any address is returned (unless an expected IP is specified, in which case it must be in the list).
- What happens when a TCP port test targets `localhost`? It works normally — useful for checking local services.
- What happens when the admin provides a display name instead of the internal service name? The test only accepts the internal service name (e.g., `MSSQLSERVER`); it fails with a Configuration error if the name doesn't match any installed service. The evidence for a successful test reports the display name so the admin can verify they targeted the right service.
- What happens when disk space is exactly at the threshold? The test passes (threshold is inclusive — "at least X GB free").
- What happens when the user specifies the minimum disk space in MB instead of GB? The parameter uses a consistent unit (GB) with decimal support (e.g., `0.5` for 500 MB).
- What happens when a DnsResolve test is used but the profile type string says `DnsLookup`? Both type identifiers should be supported to maintain backward compatibility with existing profiles.

## Requirements *(mandatory)*

### Functional Requirements

#### DnsResolve

- **FR-001**: System MUST support a `DnsResolve` test type that resolves a given hostname and reports the resulting IP address(es).
- **FR-002**: System MUST accept a `hostname` parameter (required) specifying the name to resolve.
- **FR-003**: System MUST accept an optional `expectedAddress` parameter; when provided, the test passes only if the resolved addresses include the expected IP.
- **FR-004**: System MUST report resolved addresses, resolution time, and address family (IPv4/IPv6) in the test evidence.
- **FR-005**: System MUST also register under the type identifier `DnsLookup` for backward compatibility with existing profiles that reference that name.

#### TcpPortOpen

- **FR-006**: System MUST support a `TcpPortOpen` test type that attempts a TCP connection to a specified host and port.
- **FR-007**: System MUST accept `host` (required) and `port` (required, integer) parameters.
- **FR-008**: System MUST accept an optional `connectTimeout` parameter (in milliseconds, default 5000) to limit the connection attempt duration.
- **FR-009**: System MUST report connection success/failure, connection time, and the remote endpoint in the test evidence.

#### WindowsService

- **FR-010**: System MUST support a `WindowsService` test type that checks whether a named Windows service is installed and in a running state.
- **FR-011**: System MUST accept a `serviceName` parameter (required) — the internal (short) service name only (e.g., `MSSQLSERVER`, not "SQL Server (MSSQLSERVER)"). Display name lookup is not supported.
- **FR-012**: System MUST accept an optional `expectedStatus` parameter (default: `Running`) allowing admins to check for other states (e.g., `Stopped` for a service that should be disabled).
- **FR-013**: System MUST report the service display name, current status, and start type in the test evidence.
- **FR-014**: System MUST skip with a clear reason message when run on a non-Windows platform.

#### DiskSpace

- **FR-015**: System MUST support a `DiskSpace` test type that checks available free space on a specified drive or path.
- **FR-016**: System MUST accept a `path` parameter (required, e.g., `C:\` or `/`) identifying the drive or mount point to check.
- **FR-017**: System MUST accept a `minimumFreeGB` parameter (required, decimal number) specifying the minimum free space in gigabytes.
- **FR-018**: System MUST report total space, free space, percentage free, and the threshold in the test evidence.
- **FR-019**: System MUST pass the test when free space is greater than or equal to `minimumFreeGB`.

#### Cross-cutting

- **FR-020**: All four test types MUST appear in the test list with a distinct icon and color category appropriate to their function.
- **FR-021**: All four test types MUST support the existing profile features: timeout overrides, retry count, `dependsOn` dependencies, `requiresAdmin` flag, and field policies.
- **FR-022**: All four test types MUST honor cancellation — if the user cancels a run, in-progress tests must stop promptly and report as Skipped.
- **FR-023**: All four test types MUST be automatically discovered and available without manual registration steps by the user.

### Key Entities

- **TestDefinition**: Extended with four new valid `type` values (`DnsResolve`, `TcpPortOpen`, `WindowsService`, `DiskSpace`), each with its own parameter schema (via the existing flexible `Parameters` object).
- **TestResult / TestEvidence**: No structural changes — each new test type populates `ResponseData` with a type-specific dictionary of results, and uses the existing `Timing` breakdown.

## Assumptions

- The `DnsResolve` test type also registers as `DnsLookup` because the sample profile already references that name in test type converters and icon mappings.
- The `WindowsService` test uses the internal service name (e.g., `MSSQLSERVER`) rather than the display name for the `serviceName` parameter, since internal names are stable and unambiguous.
- Disk space thresholds use gigabytes (GB) as the unit, with decimal support for sub-GB values (e.g., `0.5` = 500 MB), because GB is the natural unit for deployment-scale checks.
- The `TcpPortOpen` test performs only a connection check (TCP handshake) — it does not send or receive application-layer data.
- All four test types follow the existing parameter extraction pattern using the `Parameters` JSON object on `TestDefinition`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An IT admin can add any of the four new test types to a profile and execute them successfully within a single application session — no restart or manual configuration required.
- **SC-002**: Each new test type completes execution (pass or fail) within the configured timeout, defaulting to 30 seconds if no override is set.
- **SC-003**: Each new test type produces a clear, human-readable summary message that a non-technical user can understand (e.g., "DNS resolved www.google.com to 142.250.80.46 in 12 ms").
- **SC-004**: Existing profiles that reference the `DnsLookup` type continue to work without any profile file edits by the user.
- **SC-005**: All four test types pass the existing test infrastructure requirements: they appear in the test list with icons, support dependencies, respect cancellation, and produce exportable results.
