# Feature Specification: ProxyConnectivity Test Type

**Feature Branch**: `049-proxy-test`
**Created**: 2026-02-22
**Status**: Draft
**Input**: User description: "I need to add new test ProxyConnectivity | Validate HTTP/SOCKS proxy reachability | proxyUrl, testUrl, proxyType"

## Clarifications

### Session 2026-02-22

- Q: How should `proxyType` and `proxyUrl` scheme interact when both encode the proxy type? → A: Remove `proxyType` parameter; infer proxy type from `proxyUrl` scheme (`http://` → HTTP, `socks5://` → SOCKS5). This matches how the WebSocket test infers protocol from `ws://` vs `wss://`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Proxy Reachability (Priority: P1)

An IT administrator configures a ProxyConnectivity test in a profile to verify that a corporate proxy server is reachable and can successfully relay traffic. The test connects through the specified proxy to a target URL and reports whether the connection succeeded, how long it took, and whether the expected HTTP status was returned.

**Why this priority**: The core value of this test type is confirming that a proxy server is up and forwarding traffic. Without this, the remaining stories have no foundation.

**Independent Test**: Can be fully tested by configuring a ProxyConnectivity test with a known working HTTP proxy and a public target URL (e.g., http://www.example.com), running it, and verifying the result shows pass with connection timing evidence.

**Acceptance Scenarios**:

1. **Given** a profile with a ProxyConnectivity test specifying an HTTP proxy URL (e.g., `http://proxy:8080`) and a target URL, **When** the test runs and the proxy successfully relays the request, **Then** the result is Pass with evidence showing the proxy URL, target URL, inferred proxy type, connection status, and elapsed time.
2. **Given** a profile with a ProxyConnectivity test specifying a proxy URL that is unreachable, **When** the test runs, **Then** the result is Fail with a clear error message indicating the proxy could not be reached.
3. **Given** a profile with a ProxyConnectivity test specifying a reachable proxy but an unreachable target URL, **When** the test runs, **Then** the result is Fail with a message distinguishing that the proxy was reachable but the target was not.

---

### User Story 2 - SOCKS Proxy Support (Priority: P2)

A network engineer needs to validate that a SOCKS5 proxy is reachable and functioning. They configure the test with a `socks5://` proxy URL. The test connects through the SOCKS proxy to the target URL and reports success or failure. The proxy type is automatically inferred from the URL scheme.

**Why this priority**: SOCKS proxies are commonly used in enterprise environments alongside HTTP proxies. Supporting both proxy types covers the majority of real-world proxy configurations.

**Independent Test**: Can be tested by configuring a ProxyConnectivity test with a `socks5://` proxy URL and a target URL, running it, and verifying the result.

**Acceptance Scenarios**:

1. **Given** a profile with a ProxyConnectivity test where `proxyUrl` uses `socks5://` scheme, **When** the test runs against a working SOCKS5 proxy, **Then** the result is Pass with evidence showing the inferred proxy type as `socks5`.
2. **Given** a profile with a ProxyConnectivity test where `proxyUrl` uses `socks5://` scheme but the server is an HTTP-only proxy, **When** the test runs, **Then** the result is Fail with a descriptive error.

---

### User Story 3 - Authenticated Proxy (Priority: P2)

A system administrator needs to verify connectivity through a proxy that requires username/password authentication. They configure the test with proxy credentials. The test authenticates with the proxy before relaying the request to the target URL.

**Why this priority**: Many corporate proxies require authentication. Without credential support, the test would fail on any authenticated proxy, limiting real-world usefulness.

**Independent Test**: Can be tested by configuring a ProxyConnectivity test with proxy credentials against an authenticated proxy, running it, and verifying the result reports authentication success.

**Acceptance Scenarios**:

1. **Given** a profile with a ProxyConnectivity test including valid proxy credentials, **When** the test runs against an authenticated proxy, **Then** the result is Pass with evidence indicating authentication succeeded.
2. **Given** a profile with a ProxyConnectivity test including invalid proxy credentials, **When** the test runs against an authenticated proxy, **Then** the result is Fail with a message indicating authentication failure (e.g., HTTP 407).
3. **Given** a profile with a ProxyConnectivity test with no credentials, **When** the test runs against a proxy that requires authentication, **Then** the result is Fail with a message indicating credentials are required.

---

### Edge Cases

- What happens when the proxy URL is malformed or uses an unsupported scheme? The test fails with a configuration error before attempting any connection, listing supported schemes (`http://`, `https://`, `socks4://`, `socks5://`).
- What happens when the connection times out? The test fails with a timeout error indicating whether the timeout occurred during proxy connection or target relay.
- What happens when `expectedStatus` is set but the response status differs? The test fails with a mismatch message showing expected vs. actual status code.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support a `ProxyConnectivity` test type that connects to a target URL through a specified proxy server and reports success or failure.
- **FR-002**: System MUST accept a required `proxyUrl` parameter specifying the proxy server address (e.g., `http://proxy.corp.com:8080` or `socks5://proxy.corp.com:1080`). The proxy type is inferred from the URL scheme.
- **FR-003**: System MUST accept a required `testUrl` parameter specifying the target URL to request through the proxy.
- **FR-004**: System MUST accept optional `proxyUsername` and `proxyPassword` parameters for proxy authentication.
- **FR-005**: System MUST accept an optional `timeout` parameter (in milliseconds) defaulting to 30000.
- **FR-006**: System MUST accept an optional `expectedStatus` parameter to validate the HTTP response status code from the target URL.
- **FR-007**: System MUST capture evidence including: proxy URL, target URL, inferred proxy type, whether the proxy was reached, whether the target responded, connection time, and response status code.
- **FR-008**: System MUST report distinct error messages for: proxy unreachable, proxy authentication failure, target unreachable through proxy, timeout, and response status mismatch.
- **FR-009**: System MUST validate that `proxyUrl` starts with a supported scheme (`http://`, `https://`, `socks4://`, or `socks5://`) and fail with a configuration error otherwise.
- **FR-010**: System MUST validate that `testUrl` starts with `http://` or `https://` and fail with a configuration error otherwise.
- **FR-011**: System MUST support the `PromptAtRun` field policy for `proxyPassword` to allow secure credential entry at runtime.
- **FR-012**: System MUST display proxy test evidence in the results details view under a `[Proxy]` section showing all captured evidence fields.
- **FR-013**: System MUST include the ProxyConnectivity test type in the conditional build manifest so it can be included or excluded via the `IncludeTests` build parameter.

### Key Entities

- **ProxyConnectivity Test Parameters**: Configuration for the test including proxy URL (with scheme determining proxy type), target URL, optional credentials, timeout, and expected status.
- **ProxyConnectivity Evidence**: Runtime data captured during test execution including connection status, timing, inferred proxy type, authentication outcome, and response status code.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can validate proxy reachability by configuring and running a ProxyConnectivity test, receiving a clear pass/fail result within the configured timeout period.
- **SC-002**: Test evidence displays all relevant connection details (proxy URL, target URL, inferred proxy type, connection time, response status) in the results view.
- **SC-003**: Error messages clearly distinguish between proxy-level failures (unreachable, auth failure) and target-level failures (target unreachable through proxy, status mismatch).
- **SC-004**: The test integrates consistently with existing application features: test selection, dependency chaining, retry logic, result history, and PDF export all work with ProxyConnectivity results.

## Assumptions

- The `proxyUrl` format follows standard URI conventions: `scheme://host:port` (e.g., `http://proxy.corp.com:8080`, `socks5://10.0.0.1:1080`). The scheme determines the proxy type.
- SOCKS4 proxy support is included alongside SOCKS5 as both are commonly encountered in enterprise environments.
- The `testUrl` target is always an HTTP/HTTPS URL; the test does not support non-HTTP protocols through the proxy.
- Proxy credentials (if provided) use basic authentication for HTTP proxies and username/password authentication for SOCKS proxies.
- The test does not validate the content of the target response body — only the connection success and optional status code match.
- Evidence keys follow the project's camelCase convention (e.g., `proxyUrl`, `testUrl`, `proxyType`, `connectTimeMs`).
- Proxy credentials are redacted from evidence output (password is never included in captured evidence).
