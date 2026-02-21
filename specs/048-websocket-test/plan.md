# Implementation Plan: WebSocket Connectivity Test

**Branch**: `048-websocket-test` | **Date**: 2026-02-21 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/048-websocket-test/spec.md`

## Summary

Add a `WebSocket` test type that performs WebSocket handshake verification and optional message exchange. The test connects to `ws://` or `wss://` endpoints, validates the handshake, optionally sends a message and validates the response, and supports custom headers and subprotocol negotiation. Follows existing test patterns (TestTypeAttribute, conditional build, evidence capture, details converter).

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: System.Net.WebSockets.ClientWebSocket (built-in, no new packages)
**Storage**: N/A (in-memory test results; parameters persisted in profile JSON files)
**Testing**: Manual — build, run app, execute WebSocket tests, verify evidence display
**Target Platform**: Windows 10/11 (x64) desktop application
**Project Type**: Existing multi-project solution (App, Core, Infrastructure)
**Performance Goals**: Complete within configured timeout (default 10000ms)
**Constraints**: Single message exchange only, no persistent connections, RFC 6455 only
**Scale/Scope**: 1 new test class, 1 converter update, 2 profile updates, 1 manifest update, 1 README update

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is an unfilled template — no gates defined. No violations possible. PASS.

## Project Structure

### Documentation (this feature)

```text
specs/048-websocket-test/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── Tests/
│   │   └── WebSocketTest.cs              # NEW — WebSocket test implementation
│   └── TestManifest.props                # MODIFY — register WebSocket type
├── ReqChecker.App/
│   ├── Converters/
│   │   └── TestResultDetailsConverter.cs # MODIFY — add [WebSocket] section
│   └── Profiles/
│       ├── default-profile.json          # MODIFY — add WebSocket test entries
│       └── sample-diagnostics.json       # MODIFY — add WebSocket test entries
README.md                                 # MODIFY — add WebSocket to test reference
```

**Structure Decision**: Follows existing project structure. One new file (WebSocketTest.cs) plus modifications to 5 existing files.

## Complexity Tracking

No violations — no complexity tracking needed.

## Implementation Design

### WebSocketTest.cs — Test Implementation

**Pattern**: Follows UdpPortOpenTest (most similar: connection + optional payload + response validation)

**Parameters**:

| Parameter | Type | Required | Default | Source |
|-----------|------|----------|---------|--------|
| url | string | Yes | — | FR-002 |
| timeout | int | No | 10000 | FR-003 |
| message | string | No | null | FR-004 |
| expectedResponse | string | No | null | FR-005 |
| headers | JsonArray | No | [] | FR-006 |
| subprotocol | string | No | null | FR-007 |

**Execution Flow**:

1. Extract and validate parameters (url must start with `ws://` or `wss://`)
2. Create ClientWebSocket instance
3. Set custom headers from `headers` parameter
4. Add subprotocol if specified
5. Connect with timeout via linked CancellationTokenSource
6. Record connection time in evidence
7. If `message` is set: send text message, wait for response with remaining timeout
8. If `expectedResponse` is set: compare response against expected (exact match)
9. Close WebSocket gracefully (CloseAsync with NormalClosure)
10. Build TestResult with evidence

**Evidence (ResponseData JSON)**:

```json
{
  "wsUrl": "wss://echo.example.com",
  "connected": true,
  "connectTimeMs": 245,
  "subprotocol": "graphql-ws",
  "messageSent": "hello",
  "messageReceived": "hello",
  "responseMatched": true,
  "messageType": "text",
  "closeStatus": "NormalClosure"
}
```

**Error Handling** (FR-010):

| Exception Type | Error Category | User Message |
|---------------|----------------|--------------|
| WebSocketException (connect) | Network | "WebSocket connection failed: {reason}" |
| UriFormatException | Configuration | "Invalid WebSocket URL: must start with ws:// or wss://" |
| OperationCanceledException (timeout) | Timeout | "WebSocket connection timed out after {timeout}ms" |
| OperationCanceledException (user) | Network | "Test cancelled by user" |
| SocketException | Network | Map socket error codes (ConnectionRefused, HostNotFound, etc.) |
| HttpRequestException (TLS) | Network | "TLS/SSL error connecting to {host}" |

### TestResultDetailsConverter — [WebSocket] Section

**Trigger**: `evidenceData.ContainsKey("wsUrl")` — unique to WebSocket test (no other test emits `wsUrl`)

**Output format**:

```
[WebSocket]
URL:        wss://echo.example.com
Connected:  yes
Connect:    245 ms
Subprotocol: graphql-ws
Sent:       hello
Received:   hello
Match:      yes
```

### TestManifest.props — Registration

Add `<KnownTestType Include="WebSocket" SourceFile="Tests\WebSocketTest.cs" />` and corresponding conditional compile block.

### Profile Entries

Add WebSocket test entries to both default-profile.json and sample-diagnostics.json following existing patterns.

### README.md — Documentation

Add WebSocket to the test summary table and add a full reference section under Network Tests with parameter table and JSON example.
