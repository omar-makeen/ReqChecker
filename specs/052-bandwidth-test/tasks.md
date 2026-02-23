# Tasks: Bandwidth Test Type

**Input**: Design documents from `/specs/052-bandwidth-test/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Not requested in feature specification. Manual testing via profile JSON execution.

**Organization**: US1 (throughput check) and US2 (duration bounding) are both P1 and map to the same download loop implementation, so they share a phase. US3 (validation) maps to the parameter extraction method within the same file.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Build Manifest Registration)

**Purpose**: Register the Bandwidth test type in the build system so the file compiles

- [x] T001 Add `Bandwidth` to `KnownTestType` registry and conditional compile block in `src/ReqChecker.Infrastructure/TestManifest.props`. Add `<KnownTestType Include="Bandwidth" SourceFile="Tests\BandwidthTest.cs" />` after the Traceroute entry (line 57). Add conditional `<ItemGroup>` block after the Traceroute block (after line 177). Update the comment on line 9 from `26 test types` to `27 test types`.

---

## Phase 2: User Story 1+2 — Core Bandwidth Test (Priority: P1) — MVP

**Goal**: Implement the Bandwidth test that downloads from a URL, measures throughput in Mbps, compares against a minimum threshold, and respects a configurable duration cap.

**Independent Test**: Configure a profile with `{"type": "Bandwidth", "parameters": {"url": "https://speed.cloudflare.com/__down?bytes=10000000", "minimumMbps": 1, "durationSeconds": 5}}`, run it, and verify pass/fail result with measured throughput evidence.

### Implementation

- [x] T002 [US1] Create `src/ReqChecker.Infrastructure/Tests/BandwidthTest.cs` with class scaffolding: `[TestType("Bandwidth")]` attribute, implement `ITest`, private `BandwidthTestParameters` nested class with properties `Url` (string), `MinimumMbps` (double, default 0.0), `DurationSeconds` (int, default 10), and a private const `DefaultDurationSeconds = 10`. Add required `using` statements: `ReqChecker.Core.Execution`, `ReqChecker.Core.Interfaces`, `ReqChecker.Core.Models`, `ReqChecker.Core.Enums`, `System.Diagnostics`, `System.Net.Http`, `System.Text.Json`. Initialize `ExecuteAsync` method signature matching `ITest` interface with a `TestResult` initialized to `TestStatus.Fail` and `StartTime = DateTime.UtcNow`.

- [x] T003 [US3] Implement `ExtractParameters` method in `src/ReqChecker.Infrastructure/Tests/BandwidthTest.cs`. Extract `url` (required, throw `ArgumentException` if empty/missing), validate URL starts with `http://` or `https://` (throw `ArgumentException` otherwise). Extract optional `minimumMbps` (double, default 0.0, throw if negative). Extract optional `durationSeconds` (int, default 10, throw if <= 0). Return populated `BandwidthTestParameters`. Follow the exact pattern from `ProxyConnectivityTest.ExtractParameters` using `testDefinition.Parameters["key"]?.ToString()` and `double.TryParse` / `int.TryParse`.

- [x] T004 [US1] Implement the download and measurement logic in `ExecuteAsync` in `src/ReqChecker.Infrastructure/Tests/BandwidthTest.cs`. After calling `ExtractParameters`: (1) Create `HttpClient` with `AllowAutoRedirect = true` via `HttpClientHandler`. (2) Create a `CancellationTokenSource` with `TimeSpan.FromSeconds(durationSeconds)` linked to the incoming `cancellationToken`. (3) Send `HttpMethod.Get` request with `HttpCompletionOption.ResponseHeadersRead`. (4) Check `response.EnsureSuccessStatusCode()` — catch `HttpRequestException` for HTTP errors. (5) Get response stream via `response.Content.ReadAsStreamAsync()`. (6) Start a `Stopwatch`. (7) Read in a loop with `stream.ReadAsync(buffer, linkedCts.Token)` using an 81920-byte buffer, accumulating `totalBytesDownloaded`. (8) On `OperationCanceledException` from the duration timeout (not user cancellation), break out of the loop normally. (9) Stop the stopwatch.

- [x] T005 [US1] Implement throughput calculation, pass/fail logic, and evidence capture in `ExecuteAsync` in `src/ReqChecker.Infrastructure/Tests/BandwidthTest.cs`. After the download loop completes: (1) Calculate `elapsedSeconds = stopwatch.Elapsed.TotalSeconds`. (2) Calculate `measuredMbps = (totalBytesDownloaded * 8.0) / (elapsedSeconds * 1_000_000)`. Round to 2 decimal places using `Math.Round(measuredMbps, 2)`. (3) Determine `thresholdMet = measuredMbps >= parameters.MinimumMbps`. (4) Build `Dictionary<string, object>` evidence with camelCase keys: `url`, `measuredMbps`, `minimumMbps`, `bytesDownloaded`, `elapsedSeconds` (rounded to 2 decimals), `thresholdMet`. (5) Set `result.Status = thresholdMet ? TestStatus.Pass : TestStatus.Fail`. (6) Set `result.Evidence = new TestEvidence { ResponseData = JsonSerializer.Serialize(evidence) }`. (7) Build human-readable `result.HumanSummary` like `"Download speed: 25.47 Mbps (minimum: 10.00 Mbps) — threshold met"` or `"Download speed: 4.82 Mbps (minimum: 10.00 Mbps) — threshold not met"`. Handle zero-bytes edge case: if `totalBytesDownloaded == 0` and no exception was thrown, fail with a connection error message.

- [x] T006 [US1] Implement error handling in `ExecuteAsync` in `src/ReqChecker.Infrastructure/Tests/BandwidthTest.cs`. Add catch blocks following the `ProxyConnectivityTest.cs` pattern: (1) `OperationCanceledException` when `cancellationToken.IsCancellationRequested` → Fail with `"Test cancelled by user"`, `ErrorCategory.Unknown`. (2) `HttpRequestException` → Fail with `ErrorCategory.Network`, include HTTP status code in message if available. (3) `ArgumentException` → Fail with `ErrorCategory.Configuration` (from `ExtractParameters` validation). (4) General `Exception` → Fail with `ErrorCategory.Unknown`. Always set `result.EndTime`, `result.Duration`, `stopwatch.Stop()` in each catch block. Dispose `HttpClient` and `HttpClientHandler` in a `finally` block.

**Checkpoint**: BandwidthTest.cs is complete. Build should succeed: `dotnet build src/ReqChecker.App/ReqChecker.App.csproj`

---

## Phase 3: User Story 3 — Details Output & Documentation (Priority: P2)

**Goal**: Add the `[Bandwidth]` converter section for test results display and document the test type in the README.

**Independent Test**: Run a Bandwidth test and verify the details pane shows the `[Bandwidth]` section with all 6 fields correctly formatted.

### Implementation

- [x] T007 [P] [US3] Add `[Bandwidth]` section to `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`. Insert after the `[Directory]` section block (before the `[Response]` section). Detection: `evidenceData.ContainsKey("measuredMbps") && evidenceData.ContainsKey("bytesDownloaded")`. Render 6 fields with aligned labels: `URL:` (raw value), `Speed:` (`{value:F2} Mbps`), `Minimum:` (`{value:F2} Mbps`), `Downloaded:` (use existing `FormatBytes()` helper — parse `bytesDownloaded` as `long`), `Duration:` (`{value:F2} s`), `Threshold:` (`met` or `not met` based on `thresholdMet` boolean). End with `sections.Add(string.Empty)`. Use `double.TryParse` for numeric fields and the `ToString() is "True" or "true"` pattern for the boolean.

- [x] T008 [P] [US3] Update `README.md` to document the Bandwidth test type. (1) Add row to the test type summary table (line ~134, after Traceroute): `| Network | Bandwidth | Minimum download throughput check |`. (2) Update the test type count on line 122 from `26` to `27`. (3) Add a `#### Bandwidth` reference section under `### Network Tests` (after the Traceroute section) with: description, parameter table (`url` string required, `minimumMbps` number optional default 0, `durationSeconds` int optional default 10), and a JSON example matching quickstart.md.

**Checkpoint**: Full feature is complete. Details converter renders `[Bandwidth]` section, README is updated.

---

## Phase 4: Polish & Verification

**Purpose**: Build verification and final validation

- [x] T009 Run full build: `dotnet build src/ReqChecker.App/ReqChecker.App.csproj` — verify 0 errors, 0 warnings
- [x] T010 Verify selective build: `dotnet build src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj /p:IncludeTests="Bandwidth"` — verify Bandwidth compiles in isolation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (US1+US2)**: Depends on Phase 1 (T001 must complete before T002 so the build succeeds)
- **Phase 3 (US3)**: T007 and T008 can start after Phase 2 (need evidence keys defined in BandwidthTest.cs) and can run in parallel with each other
- **Phase 4 (Polish)**: Depends on all previous phases

### Task Dependencies

```
T001 (manifest) → T002 (scaffolding) → T003 (parameters) → T004 (download loop) → T005 (calculation) → T006 (error handling)
                                                                                                            ↓
                                                                                          T007 [P] (converter) + T008 [P] (README)
                                                                                                            ↓
                                                                                                    T009 → T010 (verification)
```

### Parallel Opportunities

- **T007 + T008**: Different files (`TestResultDetailsConverter.cs` vs `README.md`), no dependencies between them — run in parallel after T006
- **T009 + T010**: Could run sequentially (T009 first as it's the broader build)

---

## Implementation Strategy

### MVP First (User Story 1+2)

1. Complete T001 (manifest registration)
2. Complete T002–T006 (BandwidthTest.cs — full implementation)
3. **STOP and VALIDATE**: Build succeeds, test runs via profile JSON
4. This delivers a working Bandwidth test without UI details output

### Full Delivery

5. Complete T007 + T008 in parallel (converter + README)
6. Complete T009–T010 (verification)
7. Feature complete

---

## Notes

- All 3 user stories map to the same `BandwidthTest.cs` file — US1 (throughput check) and US2 (duration cap) are inherent in the download loop, US3 (validation) is the `ExtractParameters` method
- No new NuGet packages required — `System.Net.Http` is built-in
- Evidence uses dictionary-based camelCase keys (matching Traceroute/Proxy pattern, not POCO)
- `FormatBytes()` helper already exists in the converter (added in 051) — reuse it for the `Downloaded` field
- Detection key pair `measuredMbps` + `bytesDownloaded` is unique across all 26 existing test types
