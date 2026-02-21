# Feature Specification: WebSocket Connectivity Test

**Feature Branch**: `048-websocket-test`
**Created**: 2026-02-21
**Status**: Draft
**Input**: User description: "I need to add new test WebSocket | WebSocket handshake & connectivity"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Verify WebSocket Endpoint Connectivity (Priority: P1)

An IT administrator needs to validate that a WebSocket endpoint is reachable and completes the handshake successfully. They add a WebSocket test to their profile with the endpoint URL, run the profile, and see whether the connection succeeded or failed with appropriate evidence (connection time, handshake status).

**Why this priority**: The core value of a WebSocket test is verifying that the handshake completes. Without this, no other WebSocket functionality is useful.

**Independent Test**: Can be tested by configuring a WebSocket test against a public echo server (e.g., `wss://echo.websocket.org`) and verifying the test passes with connection evidence.

**Acceptance Scenarios**:

1. **Given** a profile with a WebSocket test targeting a valid `wss://` endpoint, **When** the test runs, **Then** the test passes and evidence shows the connection time and handshake status.
2. **Given** a profile with a WebSocket test targeting an unreachable endpoint, **When** the test runs, **Then** the test fails with a clear error message indicating the connection could not be established.
3. **Given** a profile with a WebSocket test and a timeout of 5 seconds, **When** the endpoint does not respond within 5 seconds, **Then** the test fails with a timeout error.

---

### User Story 2 - Send Message and Validate Response (Priority: P2)

A QA engineer needs to verify that a WebSocket endpoint not only accepts connections but also responds correctly to messages. They configure the test with a message to send after connection and optionally an expected response pattern to validate against.

**Why this priority**: Many WebSocket endpoints require message exchange verification beyond just a handshake. This enables functional validation of echo servers, health check endpoints, and message-based protocols.

**Independent Test**: Can be tested by sending a message to a WebSocket echo server and verifying the response matches the sent message.

**Acceptance Scenarios**:

1. **Given** a WebSocket test with a `message` parameter set, **When** the test runs, **Then** the message is sent after successful connection and the response is captured in evidence.
2. **Given** a WebSocket test with `message` and `expectedResponse` parameters, **When** the response matches the expected pattern, **Then** the test passes.
3. **Given** a WebSocket test with `message` and `expectedResponse` parameters, **When** the response does not match, **Then** the test fails with evidence showing both the expected and actual values.
4. **Given** a WebSocket test with a `message` but no `expectedResponse`, **When** the test runs, **Then** the test passes as long as any response is received (informational mode).

---

### User Story 3 - WebSocket with Custom Headers and Subprotocol (Priority: P3)

A DevOps engineer needs to test a WebSocket endpoint that requires specific headers (e.g., authorization tokens, API keys) or a subprotocol negotiation during the handshake. They configure custom headers and/or a subprotocol in the test parameters.

**Why this priority**: Enterprise WebSocket endpoints commonly require authentication headers or subprotocol negotiation. Without this, the test cannot reach protected endpoints.

**Independent Test**: Can be tested by configuring custom headers against an endpoint that requires them and verifying the handshake succeeds.

**Acceptance Scenarios**:

1. **Given** a WebSocket test with custom `headers` (e.g., Authorization), **When** the test runs, **Then** the headers are included in the WebSocket handshake request.
2. **Given** a WebSocket test with a `subprotocol` parameter, **When** the test runs, **Then** the subprotocol is negotiated during the handshake and the accepted protocol is recorded in evidence.

---

### Edge Cases

- What happens when the URL uses `ws://` (unencrypted) instead of `wss://`? The test should support both schemes.
- What happens when the server closes the connection immediately after handshake? The test should still pass the handshake test (US1) but may fail message exchange (US2).
- What happens when the WebSocket endpoint redirects? The test should follow redirects or report the redirect clearly.
- What happens when the server sends a binary response but the user expects a text response? The test should handle both text and binary frames and display them appropriately in evidence.
- What happens when the response timeout expires but the connection was successful? The handshake should be marked as passed, but the message exchange should be marked as timed out.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support a `WebSocket` test type that connects to a WebSocket endpoint and verifies the handshake completes.
- **FR-002**: System MUST accept `url` as a required parameter supporting both `ws://` and `wss://` schemes.
- **FR-003**: System MUST accept an optional `timeout` parameter (default: 10000ms) controlling the maximum time for the entire test (connection + message exchange).
- **FR-004**: System MUST accept an optional `message` parameter containing text to send after successful connection.
- **FR-005**: System MUST accept an optional `expectedResponse` parameter for validating the response received after sending a message.
- **FR-006**: System MUST accept optional `headers` parameter as an array of name/value pairs to include in the handshake request.
- **FR-007**: System MUST accept an optional `subprotocol` parameter to request during handshake negotiation.
- **FR-008**: System MUST capture connection evidence including: connection time, handshake status, negotiated subprotocol (if any), and server response.
- **FR-009**: System MUST properly close the WebSocket connection after the test completes (normal closure).
- **FR-010**: System MUST handle connection errors with specific, user-friendly messages for: connection refused, DNS failure, timeout, TLS errors, and unexpected closure.
- **FR-011**: System MUST integrate with the existing conditional build system (TestManifest.props) so it can be included or excluded from builds.
- **FR-012**: System MUST display WebSocket-specific evidence in the expanded result card via the TestResultDetailsConverter (e.g., `[WebSocket]` section with URL, connection time, message sent/received).
- **FR-013**: System MUST support cancellation via the standard CancellationToken mechanism.

### Key Entities

- **WebSocket Test**: A test type that validates WebSocket endpoint connectivity and optional message exchange. Configured via profile JSON with parameters for URL, timeout, message, expected response, headers, and subprotocol.
- **WebSocket Evidence**: Captured data from the test execution including connection timing, handshake result, sent message, received response, and negotiated subprotocol.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can configure and run a WebSocket connectivity test from the profile UI with the same ease as existing test types (Ping, HttpGet, etc.).
- **SC-002**: The WebSocket test completes within the configured timeout period, reporting pass/fail within 1 second of the connection outcome.
- **SC-003**: Expanded test result cards show a `[WebSocket]` section with connection details (URL, connection time, handshake status, message exchange results).
- **SC-004**: All error scenarios produce user-friendly messages that help users diagnose the issue without technical knowledge (e.g., "Connection refused" instead of raw exception text).
- **SC-005**: The test type appears in the default profile and sample diagnostics profile with at least one example entry each.

## Assumptions

- The test uses the standard WebSocket protocol (RFC 6455). No support for Socket.IO, SignalR, or other higher-level protocols is needed.
- The test does not maintain persistent connections — it connects, optionally sends one message and waits for one response, then disconnects.
- Binary message frames are supported but displayed as hex in the evidence (text frames displayed as-is).
- The default timeout of 10000ms is higher than TcpPortOpen (5000ms) because WebSocket handshakes involve HTTP upgrade negotiation.
- No new external packages are needed — the built-in WebSocket client available in the runtime is sufficient.
- The test follows the same patterns as existing tests: TestTypeAttribute registration, conditional build in TestManifest.props, evidence capture in ResponseData JSON, and details display in TestResultDetailsConverter.
