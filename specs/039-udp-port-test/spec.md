# Feature Specification: UDP Port Reachability Test

**Feature Branch**: `039-udp-port-test`
**Created**: 2026-02-13
**Status**: Draft
**Input**: User description: "create new test UDP port reachability"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic UDP Port Connectivity Check (Priority: P1)

An IT administrator needs to verify that a critical UDP-based service (DNS server, VoIP server, NTP server, or game server) is reachable before deploying client software that depends on it. The administrator configures a test profile with the target host and UDP port, runs the test suite, and receives confirmation that the UDP endpoint is responding.

**Why this priority**: This is the core value proposition of the feature. Without basic UDP reachability validation, administrators cannot verify connectivity to UDP-based services, which are common in enterprise environments (DNS on port 53, SNMP on port 161, syslog on port 514, etc.).

**Independent Test**: Can be fully tested by configuring a single UDP port test pointing to a known-reachable UDP service (e.g., 8.8.8.8:53 for Google DNS), running the test, and verifying a "Pass" result with evidence showing the response received.

**Acceptance Scenarios**:

1. **Given** a test profile with a UdpPortOpen test targeting a reachable UDP service, **When** the test executes, **Then** the result shows "Pass" with evidence containing the response time and confirmation of receipt
2. **Given** a test profile with a UdpPortOpen test targeting an unreachable UDP host, **When** the test executes, **Then** the result shows "Fail" with error category "Network" and technical details explaining no response received
3. **Given** a test profile with a UdpPortOpen test that has a 2-second timeout, **When** the UDP service does not respond within 2 seconds, **Then** the result shows "Fail" with error category "Timeout"

---

### User Story 2 - Custom Payload Validation (Priority: P2)

An administrator needs to validate not just that a UDP port is open, but that the service responds correctly to a specific protocol handshake (e.g., DNS query, SNMP GET request). The administrator configures a test with a custom UDP payload and optional expected response pattern, runs the test, and receives confirmation that the service responded with the expected data.

**Why this priority**: Many UDP services require protocol-specific payloads to respond meaningfully. A generic "send any data" approach may not trigger responses from services that expect valid protocol messages. This adds significant value for testing real-world UDP services.

**Independent Test**: Can be tested by configuring a DNS query payload (hex-encoded DNS question packet) targeting 8.8.8.8:53, running the test, and verifying the response contains a DNS answer section.

**Acceptance Scenarios**:

1. **Given** a test configured with a valid DNS query payload, **When** the test executes against a DNS server, **Then** the result shows "Pass" with evidence containing the DNS response
2. **Given** a test configured with an expected response pattern, **When** the UDP service responds with matching data, **Then** the result shows "Pass"
3. **Given** a test configured with an expected response pattern, **When** the UDP service responds with non-matching data, **Then** the result shows "Fail" with error category "Validation" and details showing expected vs. actual response

---

### User Story 3 - Field-Level Policy Support (Priority: P3)

A company wants to distribute a test profile to employees where the UDP port test targets are locked to corporate infrastructure endpoints (internal NTP server, internal DNS server) but the timeout parameter is editable by end users to accommodate different network conditions.

**Why this priority**: Consistency with existing ReqChecker field-level policy architecture. All test types support Locked/Editable/Hidden/PromptAtRun policies, and UDP tests must integrate seamlessly with this existing feature.

**Independent Test**: Can be tested by loading a profile with a UdpPortOpen test where the `host` parameter has policy "Locked" and the `timeout` parameter has policy "Editable", verifying the UI renders the host field as read-only with a lock icon and the timeout field as editable.

**Acceptance Scenarios**:

1. **Given** a bundled profile with UdpPortOpen test parameters marked as "Locked", **When** the user views the test configuration page, **Then** those fields display with lock icons and cannot be edited
2. **Given** a user-provided profile with UdpPortOpen test parameters marked as "PromptAtRun", **When** the user starts the test run, **Then** the system prompts for those values before executing the test

---

### Edge Cases

- What happens when the UDP port is filtered by a firewall (packets dropped silently vs. ICMP unreachable)?
- How does the system handle DNS resolution failure for the target host?
- What happens when the UDP payload is too large (exceeds MTU causing fragmentation)?
- How does the system differentiate between "port unreachable" vs. "host unreachable" vs. "no response"?
- What happens when the target host is specified as an IPv6 address?
- How does the system handle invalid port numbers (0, 65536+, negative)?
- What happens when the timeout is set to 0 or a negative value?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST send a UDP datagram to the specified host and port
- **FR-002**: System MUST support both IPv4 and IPv6 addresses for the target host
- **FR-003**: System MUST support DNS hostname resolution for the target host
- **FR-004**: System MUST wait for a UDP response packet for up to the configured timeout duration
- **FR-005**: System MUST mark the test as "Pass" if a UDP response is received within the timeout period
- **FR-006**: System MUST mark the test as "Fail" with error category "Timeout" if no response is received within the timeout period
- **FR-007**: System MUST mark the test as "Fail" with error category "Network" if DNS resolution fails or network is unreachable
- **FR-008**: System MUST mark the test as "Fail" with error category "Validation" if a response is received but does not match the expected response pattern (when pattern is specified)
- **FR-009**: System MUST record the round-trip time (RTT) in milliseconds when a response is received
- **FR-010**: System MUST support configurable timeout values (in milliseconds)
- **FR-011**: System MUST support custom UDP payload data (hex-encoded or base64-encoded string)
- **FR-012**: System MUST support optional expected response pattern matching (hex-encoded or base64-encoded string)
- **FR-013**: System MUST use a default payload (single null byte) when no custom payload is specified
- **FR-014**: System MUST validate that the port parameter is within the valid range (1-65535)
- **FR-015**: System MUST handle ICMP "Port Unreachable" responses and mark as "Fail" with error category "Network"
- **FR-016**: System MUST capture and include the actual response data (or lack thereof) in the test evidence
- **FR-017**: System MUST support the standard field-level policies (Locked, Editable, Hidden, PromptAtRun) for all test parameters
- **FR-018**: System MUST provide a human-readable summary message (e.g., "UDP port 53 on 8.8.8.8 responded in 12 ms")
- **FR-019**: System MUST provide technical details including sent payload size, received payload size, and response data preview
- **FR-020**: System MUST respect the test's timeout setting even if it differs from the global default timeout

### Key Entities *(include if feature involves data)*

- **UdpPortOpenTest**: A test definition representing a UDP port reachability check
  - Attributes: host (string), port (integer), timeout (integer), payload (string, optional), expectedResponse (string, optional), encoding (string, optional)
  - Relationships: Inherits from TestDefinition, produces TestResult

- **UdpPortTestEvidence**: Structured evidence captured during UDP test execution
  - Attributes: responded (boolean), roundTripTimeMs (integer, nullable), remoteEndpoint (string), payloadSentBytes (integer), payloadReceivedBytes (integer, nullable), responseDataPreview (string, nullable)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Administrators can verify UDP service reachability in under 5 seconds per endpoint (given typical network latency)
- **SC-002**: System accurately distinguishes between "port unreachable" and "timeout" scenarios with 95%+ accuracy
- **SC-003**: Test results provide sufficient technical detail for administrators to troubleshoot connectivity issues without needing external tools
- **SC-004**: UDP tests integrate seamlessly with existing test runner infrastructure (sequential execution, cancellation, retry with backoff, dependency support)
- **SC-005**: 100% of UDP test parameters support field-level policies (Locked/Editable/Hidden/PromptAtRun) consistent with other test types
- **SC-006**: Custom UDP payloads enable successful protocol-specific validation for common services (DNS, NTP, SNMP) in 90%+ of use cases
