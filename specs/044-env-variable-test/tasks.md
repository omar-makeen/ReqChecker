# Tasks: EnvironmentVariable Test

**Input**: Design documents from `/specs/044-env-variable-test/`
**Prerequisites**: plan.md (required), spec.md (required)

**Tests**: Included — unit tests are part of the plan's source code structure.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No project initialization needed — existing multi-project solution. Skip to Phase 2.

---

## Phase 2: Foundational (EnvironmentVariable Test Scaffolding)

**Purpose**: Create the EnvironmentVariableTest class scaffolding with parameter extraction and validation that all user stories depend on.

**CRITICAL**: All user story tasks depend on T001 completing first.

- [X] T001 Create `EnvironmentVariableTest` class scaffolding in `src/ReqChecker.Infrastructure/Tests/EnvironmentVariableTest.cs`. Include: `[TestType("EnvironmentVariable")]` attribute, `ITest` implementation, `ExecuteAsync` method skeleton with stopwatch/result boilerplate (follow `OsVersionTest.cs` pattern exactly), parameter extraction for `variableName` (string, required), `expectedValue` (string?, optional), and `matchType` (string?, optional) from `testDefinition.Parameters` JsonObject. Add parameter validation: fail with `ErrorCategory.Configuration` if `variableName` is null/empty/whitespace; fail with `ErrorCategory.Configuration` if `matchType` is provided without `expectedValue`; fail with `ErrorCategory.Configuration` if `matchType` is set to an unrecognized value (valid: "exact", "contains", "regex", "pathContains"); default `matchType` to "exact" when `expectedValue` is provided and `matchType` is omitted. Add `OperationCanceledException`/generic `Exception` catch blocks.

**Checkpoint**: Class compiles with parameter validation, but no pass/fail logic yet.

---

## Phase 3: User Story 1 — Verify That a Named Environment Variable Exists (Priority: P1) 🎯 MVP

**Goal**: Answer "Does variable X exist?" with a clear pass/fail result, evidence, and proper UI display.

**Independent Test**: Add profile entry `variableName: "PATH"` → test passes with value shown (truncated if long).

### Implementation for User Story 1

- [X] T002 [US1] Implement existence-check logic in `src/ReqChecker.Infrastructure/Tests/EnvironmentVariableTest.cs`. After parameter validation: read the variable via `Environment.GetEnvironmentVariable(variableName)`. If `expectedValue` is not provided (existence-only mode): if variable is null or empty, set `Status = Fail`, `Error` with `Category = ErrorCategory.Validation`, `HumanSummary = "'{variableName}' is not defined"`. If variable exists and has a non-empty value, set `Status = Pass`, `HumanSummary = "{variableName} is set to '{truncatedValue}'"` (truncate at 200 chars with "…" ellipsis if longer). Build evidence dictionary with keys: `variableName`, `found` (bool), `actualValue` (full value), `matchType` ("existence"), `expectedValue` (null), `matchResult` (null). Serialize to `Evidence.ResponseData` via `JsonSerializer.Serialize`. Set `Evidence.Timing = new TimingBreakdown { TotalMs = (int)stopwatch.ElapsedMilliseconds }`.

- [X] T003 [P] [US1] Add `"EnvironmentVariable"` icon mapping in `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs`. Add case `"EnvironmentVariable" => SymbolRegular.SlideText24` in the switch expression, after the `"InstalledSoftware"` case.

- [X] T004 [P] [US1] Add `"EnvironmentVariable"` color mapping in `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs`. Add `or "EnvironmentVariable"` to the `"ProcessList" or "RegistryRead" or "WindowsService" or "OsVersion" or "InstalledSoftware"` pattern in the switch expression (AccentSecondary group).

- [X] T005 [P] [US1] Add `[Environment Variable]` section in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`. Insert after the `[Installed Software]` section block (after line ~109). Key on presence of `"variableName"` AND `"found"` in `evidenceData` (both unique to EnvironmentVariableTest evidence): emit `[Environment Variable]` header, then `Variable:` (variableName), `Found:` (yes/no), `Value:` (actualValue, truncated to 200 chars with "…" if longer, or "N/A" if not found). If `matchType` is not "existence", also show `Match Type:` and `Expected:` and `Match Result:` (pass/fail). If `matchType` is "pathContains", add a `[Path Entries]` sub-section listing each semicolon-delimited entry from `actualValue` (max 20 entries, then "… and N more").

- [X] T006 [US1] Add existence-check `EnvironmentVariable` test entry to `src/ReqChecker.App/Profiles/default-profile.json`. Add as `test-025` after the existing `test-024`. Type: `"EnvironmentVariable"`, displayName: `"Check Environment Variable (PATH)"`, description: `"Verifies that the PATH environment variable is defined on the machine."`, parameters: `{ "variableName": "PATH", "expectedValue": null, "matchType": null }`, fieldPolicy: `{ "variableName": "Editable", "expectedValue": "Editable", "matchType": "Editable" }`, requiresAdmin: false, dependsOn: [].

**Checkpoint**: US1 complete — "Does variable X exist?" works end-to-end with evidence, summary, icon, color, and details panel.

---

## Phase 4: User Story 2 — Validate Environment Variable Value (Priority: P2)

**Goal**: When `expectedValue` is configured, compare the variable's value using the specified `matchType` (exact, contains, regex).

**Independent Test**: Set `variableName: "OS"` with `expectedValue: "Windows_NT"` and `matchType: "exact"` → passes on Windows.

### Implementation for User Story 2

- [X] T007 [US2] Add value-matching logic in `src/ReqChecker.Infrastructure/Tests/EnvironmentVariableTest.cs`. After the existence check (variable must exist and be non-empty for any match mode): implement three match modes. **exact**: case-insensitive `string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase)` — pass with `HumanSummary = "{variableName} equals '{expectedValue}'"`, fail with `"{variableName} value '{truncated}' does not equal '{expectedValue}'"`. **contains**: case-insensitive `actualValue.Contains(expectedValue, StringComparison.OrdinalIgnoreCase)` — pass with `"{variableName} contains '{expectedValue}'"`, fail with `"{variableName} value does not contain '{expectedValue}'"`. **regex**: wrap in `Regex.IsMatch(actualValue, expectedValue, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5))` with try/catch for `RegexMatchTimeoutException` (fail with `ErrorCategory.Configuration` and message "Regex evaluation timed out after 5 seconds") and `ArgumentException` (fail with `ErrorCategory.Configuration` and message "Invalid regex pattern: {ex.Message}") — pass with `"{variableName} matches pattern '{expectedValue}'"`, fail with `"{variableName} value does not match pattern '{expectedValue}'"`. Update evidence dictionary: set `matchType` to the actual mode used, `expectedValue` to the configured value, `matchResult` to "pass" or "fail".

- [X] T008 [US2] Add exact-match `EnvironmentVariable` test entry to `src/ReqChecker.App/Profiles/default-profile.json`. Add as `test-026` after `test-025`. Type: `"EnvironmentVariable"`, displayName: `"Check OS Variable (Exact Match)"`, description: `"Verifies that the OS environment variable equals 'Windows_NT'."`, parameters: `{ "variableName": "OS", "expectedValue": "Windows_NT", "matchType": "exact" }`, fieldPolicy: `{ "variableName": "Editable", "expectedValue": "Editable", "matchType": "Editable" }`, requiresAdmin: false, dependsOn: [].

**Checkpoint**: US2 complete — exact, contains, and regex matching all work. US1 still works independently (no expectedValue = existence-only pass).

---

## Phase 5: User Story 3 — Check PATH-Style Variable for Specific Directory (Priority: P3)

**Goal**: When `matchType` is "pathContains", split the variable by semicolons and check if any entry matches the expected path (case-insensitive, trailing-separator normalized).

**Independent Test**: Set `variableName: "PATH"`, `expectedValue: "C:\Windows\System32"`, `matchType: "pathContains"` → passes on any Windows machine.

### Implementation for User Story 3

- [X] T009 [US3] Add pathContains logic in `src/ReqChecker.Infrastructure/Tests/EnvironmentVariableTest.cs`. In the match-mode switch, add a `"pathContains"` case: split `actualValue` by `;`, trim each entry, normalize by `TrimEnd('\\', '/')`, compare each against `expectedValue.TrimEnd('\\', '/')` using `string.Equals(..., StringComparison.OrdinalIgnoreCase)`. If any entry matches, pass with `HumanSummary = "{variableName} contains path '{expectedValue}'"`. If none match, fail with `"{variableName} does not contain path '{expectedValue}'"`. Add `pathEntries` to evidence dictionary: an array of the first 50 semicolon-delimited entries (for diagnostics, stored as `List<string>`).

- [X] T010 [US3] Add pathContains `EnvironmentVariable` test entry to `src/ReqChecker.App/Profiles/default-profile.json`. Add as `test-027` after `test-026`. Type: `"EnvironmentVariable"`, displayName: `"Check PATH Contains System32"`, description: `"Verifies that the PATH variable contains C:\\Windows\\System32 as a directory entry."`, parameters: `{ "variableName": "PATH", "expectedValue": "C:\\Windows\\System32", "matchType": "pathContains" }`, fieldPolicy: `{ "variableName": "Editable", "expectedValue": "Editable", "matchType": "Editable" }`, requiresAdmin: false, dependsOn: [].

**Checkpoint**: US3 complete — pathContains with trailing-separator normalization works. All three stories work independently.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Unit tests and build verification.

- [X] T011 Create unit tests in `tests/ReqChecker.Infrastructure.Tests/Tests/EnvironmentVariableTestTests.cs`. Follow `OsVersionTestTests.cs` pattern: private `_test` field, `MakeDefinition(JsonObject)` helper. Include tests organized by region: **Parameter Validation** — empty variableName returns Fail with Configuration error; matchType without expectedValue returns Fail with Configuration error; unrecognized matchType returns Fail with Configuration error. **Cancellation** — cancelled token returns Skipped status. **Result Properties** — valid variableName populates TestId, TestType, DisplayName, StartTime, EndTime, Duration, non-null Evidence. **Existence Check (US1)** — variableName `"PATH"` (universally present) with no expectedValue returns Pass, non-empty HumanSummary, non-null Evidence.ResponseData with `variableName` key and `found` = true. variableName `"NONEXISTENT_VAR_12345"` returns Fail with summary containing "not defined". **Exact Match (US2)** — variableName `"OS"` with expectedValue `"Windows_NT"` and matchType `"exact"` returns Pass. variableName `"OS"` with expectedValue `"Linux"` and matchType `"exact"` returns Fail. **Contains Match** — variableName `"OS"` with expectedValue `"Windows"` and matchType `"contains"` returns Pass. **Regex Match** — variableName `"OS"` with expectedValue `"^Windows.*"` and matchType `"regex"` returns Pass. Invalid regex pattern returns Fail with Configuration error. **PathContains (US3)** — variableName `"PATH"` with expectedValue `"C:\Windows\System32"` and matchType `"pathContains"` returns Pass. variableName `"PATH"` with expectedValue `"C:\NonExistent\Path\12345"` and matchType `"pathContains"` returns Fail. **Evidence Structure** — parse Evidence.ResponseData JSON, verify keys: variableName, found, actualValue, matchType, expectedValue, matchResult.

- [X] T012 Build and run all tests: `dotnet build` then `dotnet test tests/ReqChecker.Infrastructure.Tests/` — verify all existing and new tests pass.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Skipped — existing project
- **Phase 2 (Foundational)**: T001 — BLOCKS all user story tasks
- **Phase 3 (US1)**: T002 depends on T001; T003/T004/T005 are parallel (different files); T006 depends on T002
- **Phase 4 (US2)**: T007 depends on T002; T008 depends on T007
- **Phase 5 (US3)**: T009 depends on T007 (match-mode switch must exist); T010 depends on T009
- **Phase 6 (Polish)**: T011 depends on T009 (all logic complete); T012 depends on T011

### User Story Dependencies

- **US1 (P1)**: Depends on T001 (foundational). No dependency on other stories.
- **US2 (P2)**: Depends on US1 being complete (T002 — existence check must exist before value-matching is added).
- **US3 (P3)**: Depends on US2 being complete (T007 — match-mode switch must exist before pathContains is added).

### Within Each User Story

- Core logic before converter updates
- Converter updates are parallel (different files)
- Profile entries after implementation is complete

### Parallel Opportunities

```text
After T001 completes:
  T003 [Icon converter]  ─┐
  T004 [Color converter]  ├── all parallel (different files)
  T005 [Details converter] ┘
  T002 [Core logic]       ── sequential (same file as T001)

After T002 completes:
  T006 [Profile entry US1] ── can run parallel with T007

After T007 completes:
  T008 [Profile entry US2] ─┐
  T009 [pathContains logic]  ┘  parallel (different files)

After T009 completes:
  T010 [Profile entry US3] ─┐
  T011 [Unit tests]         ┘  parallel (different files)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 (foundational class + parameter validation)
2. Complete T002 (existence-check logic)
3. Complete T003, T004, T005 in parallel (converters)
4. Complete T006 (profile entry)
5. **STOP and VALIDATE**: Run app, execute "Check Environment Variable (PATH)" test → should pass with value info

### Incremental Delivery

1. T001 → T002 + T003/T004/T005 + T006 → **US1 MVP done**
2. T007 + T008 → **US2 value matching done**
3. T009 + T010 → **US3 pathContains done**
4. T011 + T012 → **All tests passing, feature complete**

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- The `TestResultSummaryConverter` does NOT need modification — the wildcard `_` case already returns `HumanSummary`
- DI registration is automatic via reflection — no manual registration needed
- No new NuGet packages — `Environment.GetEnvironmentVariable` is built into .NET
- Guard the `[Environment Variable]` details section on `"variableName"` AND `"found"` keys to avoid collisions with other test types
- Regex evaluation uses a 5-second timeout to prevent catastrophic backtracking
- Commit after each phase checkpoint
