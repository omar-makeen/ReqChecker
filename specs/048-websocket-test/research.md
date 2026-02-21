# Research: WebSocket Connectivity Test

**Feature**: 048-websocket-test
**Date**: 2026-02-21

## Decision: WebSocket Client Library

- **Decision**: Use built-in `System.Net.WebSockets.ClientWebSocket` (.NET 8)
- **Rationale**: Available in the runtime without additional packages. Supports async connect/send/receive, custom headers, subprotocols, and cancellation tokens. Matches project constraint of no new external packages.
- **Alternatives considered**:
  - `WebSocket4Net` (rejected — unnecessary external dependency)
  - `System.Net.WebSockets.Managed` (rejected — only needed for older .NET, not .NET 8)

## Decision: Response Match Mode

- **Decision**: Exact string match for `expectedResponse` validation
- **Rationale**: Consistent with UdpPortOpenTest which uses exact byte matching. Simplest mental model for users. If regex/contains support is needed later, a `matchType` parameter can be added in a future feature (same pattern as EnvironmentVariable test).
- **Alternatives considered**:
  - Contains match (rejected — too lenient, could mask real issues)
  - Regex match (rejected — over-engineering for v1, can be added later)

## Decision: Evidence Key Design

- **Decision**: Use `wsUrl` as the unique evidence key (instead of generic `url`) to trigger the `[WebSocket]` converter section
- **Rationale**: TcpPortOpen already uses `connected` + `connectTimeMs`. HttpGet uses `url`. Using `wsUrl` as the WebSocket URL key is unambiguous — no other test type will ever emit it. The converter triggers on `evidenceData.ContainsKey("wsUrl")`.
- **Alternatives considered**:
  - `url` + `connected` (rejected — `url` overlaps with HttpGet/HttpPost evidence)
  - `closeStatus` as trigger (rejected — may not always be present on early failures)
  - `wsConnected` (viable but `wsUrl` is more descriptive and always present)

## Decision: Timeout Strategy

- **Decision**: Single timeout covering entire test (connect + send + receive), using linked CancellationTokenSource
- **Rationale**: Consistent with TcpPortOpen and UdpPortOpen patterns. Simpler configuration — one parameter instead of separate connect/receive timeouts. The 10000ms default gives enough time for TLS + HTTP upgrade + message exchange.
- **Alternatives considered**:
  - Separate connect and receive timeouts (rejected — adds complexity without clear user value for v1)

## Decision: Binary Message Handling

- **Decision**: Support receiving binary frames, display as hex string in evidence. Send only text frames.
- **Rationale**: Most WebSocket readiness checks use text messages. Binary support for receiving ensures the test doesn't fail on binary responses. Hex display is consistent with UdpPortOpen evidence display.
- **Alternatives considered**:
  - Send binary frames too (rejected — over-engineering, text covers 95% of use cases)
  - Ignore binary responses (rejected — would cause silent failures)

## Decision: Connection Closure

- **Decision**: Always attempt graceful close (CloseAsync with NormalClosure status) before disposing
- **Rationale**: RFC 6455 requires a closing handshake. Graceful closure prevents server-side resource leaks and error logs. Use a short close timeout (2 seconds) to avoid blocking if server is unresponsive.
- **Alternatives considered**:
  - Abort without close (rejected — violates RFC 6455, causes server-side errors)
  - Infinite close timeout (rejected — could hang indefinitely)

## Decision: Redirect Handling

- **Decision**: Do not follow redirects — report the redirect status in evidence
- **Rationale**: ClientWebSocket does not natively support HTTP redirects during the upgrade handshake. Implementing redirect following would require manual HTTP request + upgrade, which is significant complexity. Users can update their URL if they encounter a redirect.
- **Alternatives considered**:
  - Follow redirects manually (rejected — too complex for v1, can be added later)
