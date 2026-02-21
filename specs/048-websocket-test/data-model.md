# Data Model: WebSocket Connectivity Test

**Feature**: 048-websocket-test
**Date**: 2026-02-21

## Entities

### WebSocket Test Parameters (profile JSON → test execution)

| Field | Type | Required | Default | Validation |
|-------|------|----------|---------|------------|
| url | string | Yes | — | Must start with `ws://` or `wss://`, must be valid URI |
| timeout | int | No | 10000 | Must be > 0 |
| message | string | No | null | Any text content |
| expectedResponse | string | No | null | Only valid when `message` is also set |
| headers | JsonArray | No | [] | Array of `{ "name": "...", "value": "..." }` objects |
| subprotocol | string | No | null | WebSocket subprotocol identifier (e.g., "graphql-ws") |

### WebSocket Evidence (test execution → ResponseData JSON)

| Field | Type | Always Present | Description |
|-------|------|---------------|-------------|
| wsUrl | string | Yes | The WebSocket URL tested (unique trigger key for converter) |
| connected | bool | Yes | Whether the handshake completed successfully |
| connectTimeMs | long | Yes | Time to complete handshake in milliseconds |
| subprotocol | string | No | Negotiated subprotocol (if requested and accepted) |
| messageSent | string | No | The message sent (only when `message` param is set) |
| messageReceived | string | No | The response received (text as-is, binary as hex) |
| responseMatched | bool | No | Whether response matched expectedResponse (only when both set) |
| messageType | string | No | "text" or "binary" (only when message exchange occurred) |
| closeStatus | string | No | WebSocket close status code name (e.g., "NormalClosure") |

### Converter Display Mapping (evidence → [WebSocket] section)

| Evidence Key | Display Label | Format |
|-------------|--------------|--------|
| wsUrl | URL | As-is |
| connected | Connected | "yes" / "no" |
| connectTimeMs | Connect | "{value} ms" |
| subprotocol | Subprotocol | As-is (omit row if null) |
| messageSent | Sent | As-is (omit row if null) |
| messageReceived | Received | As-is, truncated to 500 chars (omit row if null) |
| responseMatched | Match | "yes" / "no" (omit row if null) |

## State Transitions

```
[Not Connected] → ConnectAsync() → [Connected/Handshake Complete]
                                  → [Failed: timeout/refused/DNS/TLS]

[Connected] → SendAsync() → [Message Sent]
                          → [Failed: connection closed]

[Message Sent] → ReceiveAsync() → [Response Received] → Validate → [Pass/Fail]
                                → [Timeout: no response]

[Any State] → CloseAsync() → [Closed]
            → Abort() → [Aborted] (fallback if close fails)
```

## Relationships

- WebSocket test is a new `ITest` implementation, registered via `[TestType("WebSocket")]`
- Parameters come from `TestDefinition.Parameters` (JsonObject)
- Evidence is serialized to `TestEvidence.ResponseData` as JSON string
- Timing breakdown uses `TestEvidence.Timing` (TotalMs, ConnectMs, ExecuteMs)
- TestResultDetailsConverter reads evidence keys to render `[WebSocket]` section
