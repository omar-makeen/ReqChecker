# Feature Specification: Traceroute Test Type

**Feature Branch**: `050-traceroute-test`
**Created**: 2026-02-22
**Status**: Draft
**Input**: User description: "I need to add new test Traceroute | Trace network hops to target (diagnostic) | host, maxHops, timeout"

## Clarifications

### Session 2026-02-22

- Q: How should the hop-by-hop trace data be displayed in the `[Traceroute]` details section? → A: Use `tracert`-style compact lines — one line per hop in the format `  1   12ms  192.168.1.1`, with `*` for timed-out hops (e.g., `  3     *   *`).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Traceroute Diagnostics (Priority: P1)

A network engineer configures a Traceroute test in a profile to diagnose the network path to a target host. The test sends ICMP echo requests with incrementing TTL values from 1 up to `maxHops`, recording each intermediate router (hop) that responds. The result shows the complete route with IP addresses and round-trip times for each hop, giving the engineer visibility into the network path.

**Why this priority**: The core purpose of this test type is to reveal the network path to a destination. Without hop-by-hop tracing, there is no traceroute functionality.

**Independent Test**: Can be fully tested by configuring a Traceroute test with a well-known public host (e.g., `8.8.8.8`), running it, and verifying the result shows a list of hops with IP addresses and round-trip times, ending at the target or at the `maxHops` limit.

**Acceptance Scenarios**:

1. **Given** a profile with a Traceroute test specifying `host` as a reachable IP or hostname, **When** the test runs, **Then** the result is Pass with evidence showing each hop number, responding IP address, and round-trip time in milliseconds.
2. **Given** a profile with a Traceroute test specifying a reachable host, **When** the trace reaches the target before `maxHops`, **Then** the result is Pass and the hop list ends at the target's IP address with a `reachedTarget` indicator of true.
3. **Given** a profile with a Traceroute test specifying `host` as a hostname, **When** the test runs, **Then** the hostname is resolved to an IP address before tracing begins, and both the hostname and resolved IP appear in the evidence.

---

### User Story 2 - Unreachable or Partial Route (Priority: P1)

An IT administrator runs a Traceroute test to diagnose why a remote host is unreachable. The trace may not reach the target within the maximum hops. The result clearly indicates how far the trace got, which hops responded, which timed out (shown as `*`), and whether the target was ultimately reached.

**Why this priority**: Traceroute is most valuable when diagnosing failures. Handling partial routes and timeouts at individual hops is essential for real-world diagnostic usefulness.

**Independent Test**: Can be tested by configuring a Traceroute test with a non-routable IP (e.g., `192.0.2.1`) or a very low `maxHops` value, running it, and verifying the result shows a partial route with timed-out hops and `reachedTarget` as false.

**Acceptance Scenarios**:

1. **Given** a profile with a Traceroute test where the target is not reachable within `maxHops`, **When** the test runs, **Then** the result is Fail with evidence showing all hops attempted, timed-out hops marked with `*`, and `reachedTarget` as false.
2. **Given** a profile with a Traceroute test where some intermediate hops do not respond to ICMP, **When** the test runs, **Then** those hops show `*` for the address and no round-trip time, while subsequent hops that do respond are still recorded.
3. **Given** a profile with a Traceroute test where `host` is an empty string or missing, **When** the test runs, **Then** the result is Fail with a configuration error message stating the host parameter is required.

---

### User Story 3 - Custom Trace Parameters (Priority: P2)

A DevOps engineer needs to customize trace behavior for specific network environments — for example, limiting the trace to 15 hops in a known-shallow network, or increasing the per-hop timeout on a high-latency WAN link. They set `maxHops` and `timeout` parameters in the profile to control trace depth and hop timeout.

**Why this priority**: Default values work for most cases, but enterprise networks often need tuning. This story adds flexibility without changing the core tracing logic.

**Independent Test**: Can be tested by configuring a Traceroute test with `maxHops` set to 5 and `timeout` set to 2000, running it, and verifying the trace stops at 5 hops and each hop probe respects the 2-second timeout.

**Acceptance Scenarios**:

1. **Given** a profile with a Traceroute test where `maxHops` is set to 5, **When** the test runs, **Then** the trace attempts at most 5 hops, stopping early if the target is reached.
2. **Given** a profile with a Traceroute test where `timeout` is set to 2000, **When** a hop does not respond within 2000 ms, **Then** that hop is marked as timed out (`*`) and the trace proceeds to the next hop.
3. **Given** a profile with a Traceroute test where `maxHops` and `timeout` are omitted, **When** the test runs, **Then** defaults of 30 hops and 5000 ms per hop are used.

---

### Edge Cases

- What happens when the host resolves to a loopback address (127.0.0.1)? The trace succeeds immediately with a single hop showing the loopback address, result is Pass.
- What happens when DNS resolution fails for the hostname? The test fails with an error message indicating the hostname could not be resolved, before any tracing begins.
- What happens when the user cancels the test mid-trace? The test returns a Skipped status with partial hop data collected so far.
- What happens when `maxHops` is set to 0 or a negative number? The test fails with a configuration error stating that `maxHops` must be a positive integer.
- What happens when every hop times out (no responses at all)? The test fails with all hops showing `*` and `reachedTarget` as false.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support a `Traceroute` test type that traces the network path to a target host by sending ICMP echo requests with incrementing TTL values and recording each hop's response.
- **FR-002**: System MUST accept a required `host` parameter specifying the target hostname or IP address to trace.
- **FR-003**: System MUST accept an optional `maxHops` parameter (positive integer) specifying the maximum number of hops to trace, defaulting to 30.
- **FR-004**: System MUST accept an optional `timeout` parameter (in milliseconds) specifying the per-hop timeout, defaulting to 5000.
- **FR-005**: For each hop, the system MUST record: hop number, responding IP address (or `*` if timed out), and round-trip time in milliseconds.
- **FR-006**: System MUST resolve hostnames to IP addresses before tracing and include both the original hostname and resolved IP in the evidence.
- **FR-007**: System MUST set `reachedTarget` to true in the evidence when the trace reaches the target host, and false otherwise.
- **FR-008**: System MUST report Pass when the target host is reached within `maxHops`, and Fail when the target is not reached.
- **FR-009**: System MUST report distinct error messages for: missing or empty host parameter, DNS resolution failure, and invalid `maxHops` value.
- **FR-010**: System MUST continue tracing through timed-out hops (recording them as `*`) rather than stopping at the first non-responding hop.
- **FR-011**: System MUST stop tracing early when the target host responds (i.e., do not continue to `maxHops` after reaching the target).
- **FR-012**: System MUST display traceroute evidence in the results details view under a `[Traceroute]` section showing host, resolved IP, hop count, whether the target was reached, and the hop-by-hop list rendered as `tracert`-style compact lines (one line per hop: `  1   12ms  192.168.1.1`, timed-out hops shown as `  3     *   *`).
- **FR-013**: System MUST include the Traceroute test type in the conditional build manifest so it can be included or excluded via the `IncludeTests` build parameter.
- **FR-014**: System MUST update the README to document the Traceroute test type, its parameters, and usage examples.
- **FR-015**: System MUST update the built-in test type count across the README and TestManifest.props comment to reflect the addition.

### Key Entities

- **Traceroute Test Parameters**: Configuration for the test including target host, maximum hops, and per-hop timeout.
- **Traceroute Evidence**: Runtime data captured during test execution including the resolved target IP, hop list (with hop number, address, and round-trip time for each), total hop count, whether the target was reached, and total trace duration.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can diagnose network routing by configuring and running a Traceroute test, receiving a clear pass/fail result with a hop-by-hop route within the configured timeout period.
- **SC-002**: Test evidence displays all relevant trace details (target host, resolved IP, each hop with address and timing, reached-target indicator) in the results view.
- **SC-003**: Error messages clearly distinguish between configuration errors (missing host, invalid maxHops), DNS failures, and trace failures (target not reached).
- **SC-004**: The test integrates consistently with existing application features: test selection, dependency chaining, retry logic, result history, and PDF export all work with Traceroute results.

## Assumptions

- Traceroute uses ICMP echo requests with incrementing TTL, matching the standard `traceroute` / `tracert` behavior. This requires the same network permissions as the existing Ping test.
- The `timeout` parameter applies per-hop (not to the entire trace). The total trace time is bounded by `maxHops * timeout`.
- Each hop sends a single ICMP probe (not the traditional 3 probes per hop). This keeps execution time reasonable for an automated validation tool rather than a full network diagnostic utility.
- Evidence keys follow the project's camelCase convention (e.g., `host`, `resolvedIp`, `maxHops`, `reachedTarget`, `hopCount`, `hops`).
- The hop list in evidence is an ordered array where each entry contains `hop` (number), `address` (IP string or `*`), and `roundtripMs` (integer or null for timed-out hops).
- ICMP-based tracing may be blocked by firewalls on some networks; this is an inherent limitation shared with the existing Ping test type and is not a defect.
