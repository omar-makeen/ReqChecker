# Tasks: ProxyConnectivity Test Type

**Input**: Design documents from `/specs/049-proxy-test/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Not requested — manual testing via app launch (consistent with all other test types).

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Register the new test type in the conditional build system

- [x] T001 Register ProxyConnectivity in `src/ReqChecker.Infrastructure/TestManifest.props` — add `<KnownTestType Include="ProxyConnectivity" SourceFile="Tests\ProxyConnectivityTest.cs" />` to the registry ItemGroup AND a conditional `<Compile Include="Tests\ProxyConnectivityTest.cs" />` ItemGroup following the existing pattern (see WebSocket entry as reference)

---

## Phase 2: User Story 1 — Basic Proxy Reachability (Priority: P1) MVP

**Goal**: An IT administrator can configure and run a ProxyConnectivity test with an HTTP proxy URL and a target URL, receiving a pass/fail result with timing evidence and clear error messages.

**Independent Test**: Load the app, open the default profile, run the ProxyConnectivity test with an HTTP proxy and `https://www.example.com`, verify the result shows pass/fail with evidence (proxy URL, target URL, proxy type, connection time, status code).

### Implementation for User Story 1

- [x] T002 [US1] Create `src/ReqChecker.Infrastructure/Tests/ProxyConnectivityTest.cs` with the following structure: class decorated with `[TestType("ProxyConnectivity")]` implementing `ITest`; `ExecuteAsync` method with `TestResult` scaffolding (StartTime, StopWatch, try/catch/finally); parameter extraction method that reads required `proxyUrl` (string, validated to start with `http://`, `https://`, `socks4://`, or `socks5://`), required `testUrl` (string, validated to start with `http://` or `https://`), optional `timeout` (int, default 30000, must be positive), and optional `expectedStatus` (int, nullable). Throw `ArgumentException` for validation failures. Use the existing test patterns from HttpGetTest.cs and WebSocketTest.cs as reference.

- [x] T003 [US1] Implement the core HTTP proxy execution logic in `src/ReqChecker.Infrastructure/Tests/ProxyConnectivityTest.cs`: create an `HttpClientHandler` with `UseProxy = true` and `Proxy = new WebProxy(proxyUrl)`, create an `HttpClient` with that handler (dispose both in finally), create a linked `CancellationTokenSource` for timeout, send an `HttpGet` request to `testUrl` through the proxy, capture evidence in a `Dictionary<string, object>` with camelCase keys: `proxyUrl`, `testUrl`, `proxyType` (inferred from URL scheme — e.g., `http`), `proxyReached` (bool), `targetReached` (bool), `connectTimeMs` (stopwatch elapsed), `statusCode` (response status code). Serialize evidence via `JsonSerializer.Serialize(evidence)` into `TestEvidence.ResponseData`. Set `TestResult.Status` to Pass when connection succeeds (and `expectedStatus` matches if specified). Build a human-readable `HumanSummary` (e.g., "Connected to https://www.example.com via http://proxy:8080 in 142ms (200 OK)").

- [x] T004 [US1] Implement error handling in `src/ReqChecker.Infrastructure/Tests/ProxyConnectivityTest.cs`: add catch blocks for `OperationCanceledException` (user cancellation vs timeout — same pattern as WebSocketTest), `HttpRequestException` (proxy unreachable, target unreachable — set `proxyReached`/`targetReached` evidence accordingly, distinguish proxy connection errors from target errors using exception message/inner exception), `SocketException` (network-level proxy errors with user-friendly messages), `UriFormatException` (malformed URLs), `ArgumentException` (parameter validation errors), and general `Exception`. Each catch block must set `TestResult.Status = Fail`, populate `TestResult.Error` with appropriate `ErrorCategory` (Network, Timeout, Configuration), and include a descriptive `HumanSummary`. For `expectedStatus` mismatch: compare `response.StatusCode` to `expectedStatus` and fail with "Status mismatch: expected {expected}, got {actual}".

- [x] T005 [P] [US1] Add `[Proxy]` evidence section to `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`: add a new block (after the `[WebSocket]` section) that checks `evidenceData.ContainsKey("proxyUrl")` and renders evidence rows: `Proxy:` (proxyUrl), `Target:` (testUrl), `Type:` (proxyType), `Connected:` (proxyReached as yes/no), `Status:` (statusCode with status text from existing `GetStatusText` helper), `Connect:` (connectTimeMs with "ms" suffix). Use the same pattern as the `[WebSocket]` section with camelCase key lookups.

- [x] T006 [P] [US1] Add sample ProxyConnectivity test entry to `src/ReqChecker.App/Profiles/default-profile.json`: add a new test object with `"type": "ProxyConnectivity"`, `"displayName": "Check HTTP Proxy"`, `"description": "Validates connectivity through an HTTP proxy server."`, parameters `proxyUrl` (`http://proxy.example.com:8080`), `testUrl` (`https://www.example.com`), `timeout` (30000), `expectedStatus` (200), with `fieldPolicy` setting `proxyUrl` and `testUrl` to `Editable` and `timeout`/`expectedStatus` to `Editable`. Follow the existing entry format for id, dependsOn, etc.

- [x] T007 [P] [US1] Add sample ProxyConnectivity test entry to `src/ReqChecker.App/Profiles/sample-diagnostics.json`: add a new test object following the same pattern as T006 but using the GUID-style id format used in this file (e.g., `10000000-0000-0000-0000-00000000000d` — next sequential value after the last existing entry). Match the format of the WebSocket entry in this file.

**Checkpoint**: At this point, the app builds and ProxyConnectivity tests can be configured and run with HTTP proxies. Results show evidence details under [Proxy] section.

---

## Phase 3: User Story 2 — SOCKS Proxy Support (Priority: P2)

**Goal**: A network engineer can configure a ProxyConnectivity test with a `socks5://` or `socks4://` proxy URL, and the test correctly connects through the SOCKS proxy with accurate proxy type inference in evidence.

**Independent Test**: Configure a ProxyConnectivity test with `socks5://` proxy URL, run it, verify the evidence shows `proxyType: socks5` and the connection succeeds or fails with a SOCKS-specific error message.

### Implementation for User Story 2

- [x] T008 [US2] Enhance proxy type inference and SOCKS-specific error handling in `src/ReqChecker.Infrastructure/Tests/ProxyConnectivityTest.cs`: ensure the `proxyType` evidence value correctly maps all supported schemes (`http://` → `http`, `https://` → `https`, `socks4://` → `socks4`, `socks5://` → `socks5`). Add a helper method `InferProxyType(string proxyUrl)` that extracts the scheme. In the `HttpRequestException` catch block, detect SOCKS-specific failures (e.g., SOCKS protocol mismatch when connecting to an HTTP proxy with `socks5://` scheme) and provide descriptive error messages like "SOCKS5 connection failed — verify the proxy supports SOCKS5 protocol".

**Checkpoint**: SOCKS4 and SOCKS5 proxies work alongside HTTP proxies. Evidence correctly reflects the inferred proxy type.

---

## Phase 4: User Story 3 — Authenticated Proxy (Priority: P2)

**Goal**: A system administrator can configure proxy credentials (`proxyUsername`, `proxyPassword`) and the test authenticates with the proxy, reporting auth success/failure in evidence with credentials redacted.

**Independent Test**: Configure a ProxyConnectivity test with `proxyUsername` and `proxyPassword`, run against an authenticated proxy, verify the evidence shows `authSucceeded: true` and `proxyUsername` but never shows the password.

### Implementation for User Story 3

- [x] T009 [US3] Add proxy authentication support to `src/ReqChecker.Infrastructure/Tests/ProxyConnectivityTest.cs`: extend the parameter extraction method to read optional `proxyUsername` (string) and `proxyPassword` (string) from `TestDefinition.Parameters`. When both are provided, set `webProxy.Credentials = new NetworkCredential(proxyUsername, proxyPassword)` on the `WebProxy` instance before configuring the handler. Add `proxyUsername` (string, never the password) and `authSucceeded` (bool) to the evidence dictionary only when authentication was attempted. In the `HttpRequestException` catch block, detect HTTP 407 (Proxy Authentication Required) responses and set `authSucceeded = false` with error message "Proxy authentication failed — verify credentials". When no credentials are provided but the proxy returns 407, set error message "Proxy requires authentication — provide proxyUsername and proxyPassword parameters".

**Checkpoint**: Authenticated proxies work. Credentials are redacted from evidence. Auth failures produce clear, distinct error messages.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Documentation updates and final verification

- [x] T010 [P] Update `README.md`: change test count from "24" to "25" in all locations (4 occurrences: Key Features line, conditional build comment, IncludeTests description, test types heading). Add `| Network | ProxyConnectivity | HTTP/SOCKS proxy reachability with optional authentication |` row to the test types table (after WebSocket). Add a `#### ProxyConnectivity` reference section under Network Tests with parameter table (proxyUrl, testUrl, proxyUsername, proxyPassword, timeout, expectedStatus) and two JSON examples (basic HTTP proxy, authenticated proxy) — follow the format of the WebSocket section.

- [x] T011 Verify build succeeds by running `dotnet build` — expect 0 errors, 0 warnings

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **US1 (Phase 2)**: Depends on T001 (manifest registration) — this is the MVP
- **US2 (Phase 3)**: Depends on US1 completion (T002-T004 specifically — the core test class must exist)
- **US3 (Phase 4)**: Depends on US1 completion (T002-T004 — extends the parameter extraction and error handling)
- **Polish (Phase 5)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Phase 1 — no dependencies on other stories
- **User Story 2 (P2)**: Extends US1's test class — must start after T002-T004 are complete
- **User Story 3 (P2)**: Extends US1's test class — must start after T002-T004 are complete
- **US2 and US3**: Independent of each other — can run in parallel if desired

### Within Each User Story

- T002 → T003 → T004 (sequential — scaffolding, then logic, then error handling)
- T005, T006, T007 can run in parallel with each other (different files)
- T005, T006, T007 can run in parallel with T003/T004 (different files)

### Parallel Opportunities

- T005, T006, T007 are all [P] — different files, no dependencies on each other
- US2 (T008) and US3 (T009) are independent — can run in parallel after US1
- T010 (README) and T011 (build verification) — T010 is [P], T011 depends on all code being complete

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: T001 (manifest registration)
2. Complete Phase 2: T002-T007 (core test + converter + profiles)
3. **STOP and VALIDATE**: Build, launch app, run ProxyConnectivity test with HTTP proxy
4. Working MVP with HTTP proxy support, evidence display, and sample profiles

### Incremental Delivery

1. T001 → T002-T007 → MVP with HTTP proxy support
2. T008 → SOCKS proxy support added
3. T009 → Authenticated proxy support added
4. T010-T011 → README docs and final build verification

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- No new NuGet packages — uses built-in System.Net.Http and System.Net.WebProxy
- Evidence uses `Dictionary<string, object>` with explicit camelCase keys (not a typed class)
- Per-test HttpClient/HttpClientHandler — dispose in finally block (research decision R2)
- Password never appears in evidence (research decision R6)
- Commit after each phase or logical group
