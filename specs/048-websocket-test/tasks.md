# Tasks: WebSocket Connectivity Test

**Input**: Design documents from `/specs/048-websocket-test/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: No automated tests — manual verification via app execution.

**Organization**: Tasks are grouped by user story. US1 (handshake) is the foundation; US2 (message exchange) and US3 (headers/subprotocol) extend it.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Build System Registration)

**Purpose**: Register the new test type so it compiles and is discoverable.

- [x] T001 Add WebSocket KnownTestType entry and conditional compile block to src/ReqChecker.Infrastructure/TestManifest.props

**Checkpoint**: Build system recognizes WebSocket as a valid test type name.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the WebSocketTest.cs skeleton that all user stories build upon.

- [x] T002 Create src/ReqChecker.Infrastructure/Tests/WebSocketTest.cs with class skeleton: [TestType("WebSocket")] attribute, ITest implementation, parameter extraction (url, timeout), URL validation (ws:// or wss:// scheme), and empty ExecuteAsync returning a placeholder TestResult

**Checkpoint**: `dotnet build` succeeds with WebSocketTest.cs compiled. The test type is registered but not yet functional.

---

## Phase 3: User Story 1 - WebSocket Handshake Connectivity (Priority: P1) MVP

**Goal**: Connect to a WebSocket endpoint, verify handshake completes, capture connection evidence (time, status), handle errors with user-friendly messages.

**Independent Test**: Configure a WebSocket test against a wss:// endpoint, run it, verify pass/fail with connection evidence in the expanded result card.

### Implementation for User Story 1

- [x] T003 [US1] Implement WebSocket connection logic in src/ReqChecker.Infrastructure/Tests/WebSocketTest.cs: create ClientWebSocket, connect with linked CancellationTokenSource for timeout, record connection time via Stopwatch, build evidence dictionary with wsUrl, connected, connectTimeMs keys, close gracefully with CloseAsync(NormalClosure) and 2-second close timeout
- [x] T004 [US1] Implement error handling in src/ReqChecker.Infrastructure/Tests/WebSocketTest.cs: catch WebSocketException (connection failed), SocketException (connection refused, host not found), OperationCanceledException (timeout vs user cancellation), UriFormatException (invalid URL), HttpRequestException (TLS errors) — each mapped to appropriate ErrorCategory with user-friendly messages per plan.md error handling table
- [x] T005 [P] [US1] Add [WebSocket] section to src/ReqChecker.App/Converters/TestResultDetailsConverter.cs: trigger on evidenceData.ContainsKey("wsUrl"), display URL, Connected (yes/no), Connect time (ms) — insert after the [CPU Cores] block and before the [Response] block
- [x] T006 [P] [US1] Add WebSocket handshake-only test entries to src/ReqChecker.App/Profiles/default-profile.json: at least one wss:// connectivity test (e.g., wss://echo.websocket.events) with displayName, description, fieldPolicy for url as Editable, timeout as Editable
- [x] T007 [P] [US1] Add WebSocket test entries to src/ReqChecker.App/Profiles/sample-diagnostics.json: matching entries as default-profile.json

**Checkpoint**: WebSocket handshake test works end-to-end. Expanding the result shows [WebSocket] section with URL, Connected, Connect time. Errors show user-friendly messages.

---

## Phase 4: User Story 2 - Message Exchange & Response Validation (Priority: P2)

**Goal**: After successful handshake, send a text message, receive a response, optionally validate against expectedResponse. Capture sent/received messages in evidence.

**Independent Test**: Configure a WebSocket test with message "hello" and expectedResponse "hello" against an echo server, verify pass with message evidence.

### Implementation for User Story 2

- [x] T008 [US2] Add message send/receive logic to src/ReqChecker.Infrastructure/Tests/WebSocketTest.cs: after successful connect, if message parameter is set, send text frame via SendAsync, receive response via ReceiveAsync with remaining timeout, handle binary responses (display as hex), add messageSent, messageReceived, messageType keys to evidence dictionary
- [x] T009 [US2] Add expectedResponse validation to src/ReqChecker.Infrastructure/Tests/WebSocketTest.cs: if expectedResponse is set, perform exact string comparison against received response, set responseMatched in evidence, fail test if mismatch with evidence showing expected vs actual, pass without expectedResponse if any response received (informational mode)
- [x] T010 [US2] Update [WebSocket] converter section in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs: add Sent, Received (truncated to 500 chars), and Match (yes/no) rows — only display when corresponding evidence keys are present
- [x] T011 [US2] Update WebSocket profile entries in src/ReqChecker.App/Profiles/default-profile.json: add at least one test entry with message parameter (e.g., echo test with message "hello" and expectedResponse "hello")

**Checkpoint**: Message exchange works. Expanding result shows Sent/Received/Match. Mismatch produces clear failure.

---

## Phase 5: User Story 3 - Custom Headers & Subprotocol (Priority: P3)

**Goal**: Support custom headers in the handshake request and subprotocol negotiation. Capture negotiated subprotocol in evidence.

**Independent Test**: Configure a WebSocket test with custom headers and verify they are sent. Configure with subprotocol and verify negotiation is recorded.

### Implementation for User Story 3

- [x] T012 [US3] Add custom headers support to src/ReqChecker.Infrastructure/Tests/WebSocketTest.cs: extract headers JsonArray parameter, iterate and add each name/value pair to ClientWebSocket.Options.SetRequestHeader() before connecting
- [x] T013 [US3] Add subprotocol support to src/ReqChecker.Infrastructure/Tests/WebSocketTest.cs: extract subprotocol parameter, call ClientWebSocket.Options.AddSubProtocol() before connecting, after connection record negotiated subprotocol from ClientWebSocket.SubProtocol in evidence
- [x] T014 [US3] Update [WebSocket] converter section in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs: add Subprotocol row — only display when subprotocol evidence key is present and non-empty

**Checkpoint**: Headers are sent in handshake. Subprotocol is negotiated and shown in evidence.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation updates and final verification.

- [x] T015 [P] Add WebSocket row to test summary table and full reference section (under Network Tests) with parameter table and JSON example in README.md
- [x] T016 Build and verify: run dotnet build with 0 errors, launch app, load default profile, run WebSocket tests, expand results to verify [WebSocket] section displays correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — registers build system entry
- **Foundational (Phase 2)**: Depends on Phase 1 — creates compilable test skeleton
- **US1 (Phase 3)**: Depends on Phase 2 — implements core handshake logic
- **US2 (Phase 4)**: Depends on US1 (T003) — extends test with message exchange
- **US3 (Phase 5)**: Depends on US1 (T003) — extends test with headers/subprotocol
- **Polish (Phase 6)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational — can be tested independently (handshake only)
- **User Story 2 (P2)**: Depends on US1 — extends the same WebSocketTest.cs file
- **User Story 3 (P3)**: Depends on US1 — extends the same WebSocketTest.cs file. Independent of US2.

### Parallel Opportunities

- T005, T006, T007 can run in parallel (different files, after T003/T004 complete)
- T015 can run in parallel with Phase 6 verification
- US2 and US3 are independent of each other (both extend US1)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: Foundational (T002)
3. Complete Phase 3: US1 (T003-T007)
4. **STOP and VALIDATE**: WebSocket handshake works, evidence displays, errors are clear

### Incremental Delivery

1. Setup + Foundational → Test type registered and compiles
2. US1 → Handshake connectivity (MVP!)
3. US2 → Message exchange + response validation
4. US3 → Custom headers + subprotocol
5. Polish → README update, final build verification

---

## Notes

- All user stories modify the same file (WebSocketTest.cs) so US2 and US3 must be sequential after US1
- Converter updates (T005, T010, T014) also modify the same file but can be batched per story
- Profile updates touch the same JSON files but different test entries (no conflicts)
- Evidence uses `wsUrl` as the unique converter trigger key (avoids collision with TcpPortOpen)
- Graceful close uses 2-second timeout to avoid blocking on unresponsive servers
