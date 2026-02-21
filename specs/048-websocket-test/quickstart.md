# Quickstart: WebSocket Connectivity Test

**Feature**: 048-websocket-test
**Date**: 2026-02-21

## Implementation Steps

### Step 1: Create WebSocketTest.cs

Create `src/ReqChecker.Infrastructure/Tests/WebSocketTest.cs` implementing `ITest` with `[TestType("WebSocket")]`.

Follow the pattern from UdpPortOpenTest:
- Extract parameters from `TestDefinition.Parameters`
- Validate URL scheme (ws:// or wss://)
- Create `ClientWebSocket`, configure headers/subprotocol
- Connect with linked CancellationTokenSource for timeout
- Optionally send message and receive response
- Close gracefully, build TestResult with evidence

### Step 2: Register in TestManifest.props

Add to `src/ReqChecker.Infrastructure/TestManifest.props`:
- `<KnownTestType Include="WebSocket" SourceFile="Tests\WebSocketTest.cs" />`
- Conditional compile block matching existing pattern

### Step 3: Add Converter Section

Add `[WebSocket]` section to `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`:
- Trigger on `evidenceData.ContainsKey("wsUrl")`
- Display URL, Connected, Connect time, Subprotocol, Sent, Received, Match

### Step 4: Add Profile Entries

Add WebSocket test entries to:
- `src/ReqChecker.App/Profiles/default-profile.json`
- `src/ReqChecker.App/Profiles/sample-diagnostics.json`

### Step 5: Update README

Add WebSocket to `README.md`:
- Add row to test summary table
- Add full reference section under Network Tests

### Step 6: Build and Verify

```bash
dotnet build
```

Then launch the app and:
1. Load default profile
2. Run all tests
3. Expand WebSocket result → verify `[WebSocket]` section appears
4. Verify pass/fail behavior with valid/invalid endpoints

## Verification Checklist

- [ ] WebSocketTest.cs compiles and is registered via TestTypeAttribute
- [ ] TestManifest.props includes WebSocket entry with conditional compile
- [ ] `dotnet build` succeeds with 0 errors
- [ ] `dotnet build /p:IncludeTests="Ping"` excludes WebSocket (no compile error)
- [ ] Default profile has at least one WebSocket test entry
- [ ] Sample diagnostics profile has at least one WebSocket test entry
- [ ] Running the app shows WebSocket tests in test list
- [ ] Expanding a WebSocket result shows [WebSocket] section with evidence
- [ ] Connection to valid wss:// endpoint passes
- [ ] Connection to invalid endpoint fails with user-friendly error
- [ ] Timeout works correctly (test fails after configured timeout)
- [ ] Message send/receive works with echo server
- [ ] README includes WebSocket in summary table and reference section
