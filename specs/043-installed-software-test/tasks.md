# Tasks: InstalledSoftware Test

**Input**: Design documents from `/specs/043-installed-software-test/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

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

## Phase 2: Foundational (Registry Search Infrastructure)

**Purpose**: Create the InstalledSoftwareTest class scaffolding with the shared registry search helper that all user stories depend on.

**CRITICAL**: All user story tasks depend on T001 completing first.

- [X] T001 Create `InstalledSoftwareTest` class scaffolding in `src/ReqChecker.Infrastructure/Tests/InstalledSoftwareTest.cs`. Include: `[TestType("InstalledSoftware")]` attribute, `ITest` implementation, `ExecuteAsync` method skeleton with stopwatch/result boilerplate (follow `OsVersionTest.cs` pattern exactly), parameter extraction for `softwareName` (string, required) and `minimumVersion` (string?, optional) from `testDefinition.Parameters` JsonObject, and `OperationCanceledException`/generic `Exception` catch blocks. Add a private helper method `SearchRegistry(string softwareName, CancellationToken ct)` that enumerates all three registry hives (`HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`, `HKLM\SOFTWARE\WOW6432Node\...\Uninstall`, `HKCU\Software\...\Uninstall`), performs case-insensitive substring match on `DisplayName`, collects matching records (displayName, version, installLocation, publisher, installDate), skips entries with null/empty DisplayName, and returns a `List<Dictionary<string, string?>>` of all matches. Add a private helper `SelectPrimaryMatch(List<...> matches)` that sorts matches by highest parseable `System.Version` (unparseable versions sort last) and returns the first entry. Add parameter validation: fail with `ErrorCategory.Configuration` if `softwareName` is null/empty/whitespace.

**Checkpoint**: Class compiles, registry search works, but no pass/fail logic yet.

---

## Phase 3: User Story 1 — Check That a Named Application Is Installed (Priority: P1)

**Goal**: Answer "Is X installed?" with a clear pass/fail result, evidence, and proper UI display.

**Independent Test**: Add profile entry `softwareName: "Microsoft Edge"` → test passes with version and location shown.

### Implementation for User Story 1

- [X] T002 [US1] Implement found/not-found logic in `src/ReqChecker.Infrastructure/Tests/InstalledSoftwareTest.cs`. After registry search: if no matches found, set `Status = Fail`, `Error` with `Category = ErrorCategory.Validation`, `HumanSummary = "'{softwareName}' not found in installed programs"`. If found (and no `minimumVersion`), set `Status = Pass`, `HumanSummary = "{displayName} {version} installed"` (or `"{displayName} installed (version unknown)"` if version is null/empty). Build evidence dictionary with keys: `displayName`, `version` (or "unknown"), `installLocation`, `publisher`, `installDate`, and `allMatches` (array of {displayName, version} for every match). Serialize to `Evidence.ResponseData` via `JsonSerializer.Serialize`. Set `Evidence.Timing = new TimingBreakdown { TotalMs = (int)stopwatch.ElapsedMilliseconds }`.

- [X] T003 [P] [US1] Add `"InstalledSoftware"` icon mapping in `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs`. Add case `"InstalledSoftware" => SymbolRegular.AppFolder24` in the switch expression, after the `"OsVersion"` case.

- [X] T004 [P] [US1] Add `"InstalledSoftware"` color mapping in `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs`. Add `or "InstalledSoftware"` to the `"ProcessList" or "RegistryRead" or "WindowsService" or "OsVersion"` pattern in the switch expression (AccentSecondary group).

- [X] T005 [P] [US1] Add `[Installed Software]` section in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`. Insert after the `[OS Version]` section block (after line ~66). Keyed on presence of `"displayName"` in `evidenceData`: emit `[Installed Software]` header, then conditionally add `Name:`, `Version:`, `Location:`, `Publisher:`, `Install Date:` lines from evidenceData values. If `evidenceData` contains `"allMatches"` with more than 1 entry, add a blank line and `[All Matches]` sub-header listing each match's displayName and version.

- [X] T006 [US1] Add informational `InstalledSoftware` test entry to `src/ReqChecker.App/Profiles/default-profile.json`. Add as `test-022` after the existing `test-021`. Type: `"InstalledSoftware"`, displayName: `"Check Installed Software (Edge)"`, description: `"Verifies that Microsoft Edge is installed on the machine."`, parameters: `{ "softwareName": "Microsoft Edge", "minimumVersion": null }`, fieldPolicy: `{ "softwareName": "Editable", "minimumVersion": "Editable" }`, requiresAdmin: false, dependsOn: [].

**Checkpoint**: US1 complete — "Is X installed?" works end-to-end with registry search, evidence, summary, icon, color, and details panel.

---

## Phase 4: User Story 2 — Enforce a Minimum Version Requirement (Priority: P2)

**Goal**: When `minimumVersion` is configured, compare the found software's version against it.

**Independent Test**: Set `softwareName: "Microsoft Edge"` with `minimumVersion: "1.0.0"` → passes. Set `minimumVersion: "999.0.0"` → fails with version comparison message.

### Implementation for User Story 2

- [X] T007 [US2] Add `minimumVersion` validation and comparison logic in `src/ReqChecker.Infrastructure/Tests/InstalledSoftwareTest.cs`. After parameter extraction: if `minimumVersion` is provided, validate it parses as `System.Version` — if not, fail with `ErrorCategory.Configuration` and message `"Invalid minimumVersion format '{val}': expected major.minor[.patch[.revision]]"`. After finding primary match: if `minimumVersion` is set and the primary match's version is null/unparseable, fail with `"Cannot verify version for {displayName} — version unknown"`. If version is parseable, compare via `System.Version` — if `installedVersion >= minimumVersion`, pass with `HumanSummary = "{displayName} {version} meets minimum {minimumVersion}"`, else fail with `"{displayName} {version} does not meet minimum {minimumVersion}"`. Strip any non-numeric suffix (e.g., "-beta") from the installed version string before parsing by taking the substring up to the first character that is not a digit or period.

- [X] T008 [US2] Add minimum-version `InstalledSoftware` test entry to `src/ReqChecker.App/Profiles/default-profile.json`. Add as `test-023` after `test-022`. Type: `"InstalledSoftware"`, displayName: `"Check Edge Minimum Version (100.0)"`, description: `"Verifies that Microsoft Edge version 100.0 or newer is installed."`, parameters: `{ "softwareName": "Microsoft Edge", "minimumVersion": "100.0" }`, fieldPolicy: `{ "softwareName": "Editable", "minimumVersion": "Editable" }`, requiresAdmin: false, dependsOn: [].

**Checkpoint**: US2 complete — version gating works. US1 still works independently (no minimumVersion = informational pass).

---

## Phase 5: User Story 3 — Informational Mode (Priority: P3)

**Goal**: Ensure the informational mode (no `minimumVersion`) produces a distinct summary format and is clearly intentional.

**Independent Test**: Set `softwareName: "Microsoft Edge"` with no `minimumVersion` → summary shows "{name} {version} installed" with no mention of a minimum.

### Implementation for User Story 3

- [X] T009 [US3] Add not-found-expected `InstalledSoftware` test entry to `src/ReqChecker.App/Profiles/default-profile.json`. Add as `test-024` after `test-023`. Type: `"InstalledSoftware"`, displayName: `"Check Nonexistent Software (Expected Fail)"`, description: `"Tests the not-found error path by searching for software that does not exist. Expected to fail."`, parameters: `{ "softwareName": "NonExistentSoftware12345", "minimumVersion": null }`, fieldPolicy: `{ "softwareName": "Editable", "minimumVersion": "Hidden" }`, requiresAdmin: false, dependsOn: [].

**Checkpoint**: US3 complete — informational mode is implicit in US1's found-without-minimumVersion path. The third profile entry demonstrates the failure path.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Unit tests and build verification.

- [X] T010 Create unit tests in `tests/ReqChecker.Infrastructure.Tests/Tests/InstalledSoftwareTestTests.cs`. Follow `OsVersionTestTests.cs` pattern: private `_test` field, `MakeDefinition(JsonObject)` helper. Include tests organized by region: **Parameter Validation** — empty softwareName returns Fail with Configuration error; invalid minimumVersion format returns Fail with Configuration error. **Cancellation** — cancelled token returns Skipped status. **Result Properties** — valid softwareName populates TestId, TestType, DisplayName, StartTime, EndTime, Duration, non-null Evidence. **Found Software** — softwareName `"Microsoft Edge"` (universally present) returns Pass, non-empty HumanSummary, non-null Evidence.ResponseData with `displayName` key. **Not Found** — softwareName `"NonExistentSoftware12345"` returns Fail with Validation error containing "not found". **Minimum Version Pass** — softwareName `"Microsoft Edge"` with minimumVersion `"1.0"` returns Pass with summary containing "meets minimum". **Minimum Version Fail** — softwareName `"Microsoft Edge"` with minimumVersion `"999.0"` returns Fail with summary containing "does not meet minimum". **Evidence Structure** — parse Evidence.ResponseData JSON, verify keys: displayName, version, installLocation, publisher, installDate, allMatches.

- [X] T011 Build and run all tests: `dotnet build` then `dotnet test tests/ReqChecker.Infrastructure.Tests/` — verify all existing and new tests pass.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Skipped — existing project
- **Phase 2 (Foundational)**: T001 — BLOCKS all user story tasks
- **Phase 3 (US1)**: T002 depends on T001; T003/T004/T005 are parallel (different files); T006 depends on T002
- **Phase 4 (US2)**: T007 depends on T002; T008 depends on T007
- **Phase 5 (US3)**: T009 has no code dependency (profile entry only), can run after T008
- **Phase 6 (Polish)**: T010 depends on T007 (all logic complete); T011 depends on T010

### User Story Dependencies

- **US1 (P1)**: Depends on T001 (foundational). No dependency on other stories.
- **US2 (P2)**: Depends on US1 being complete (T002 — found/not-found logic must exist before version comparison is added).
- **US3 (P3)**: No code dependency — only adds a profile entry. Can run any time after T008.

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
  T009 [Profile entry US3]  ├── parallel (same file but independent additions)
  T010 [Unit tests]         ┘
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 (foundational class + registry search)
2. Complete T002 (found/not-found logic)
3. Complete T003, T004, T005 in parallel (converters)
4. Complete T006 (profile entry)
5. **STOP and VALIDATE**: Run app, execute "Check Installed Software (Edge)" test → should pass with version info

### Incremental Delivery

1. T001 → T002 + T003/T004/T005 + T006 → **US1 MVP done**
2. T007 + T008 → **US2 version gating done**
3. T009 → **US3 failure demo done**
4. T010 + T011 → **All tests passing, feature complete**

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- The `TestResultSummaryConverter` does NOT need modification — the wildcard `_` case already returns `HumanSummary`
- DI registration is automatic via reflection — no manual registration needed
- No new NuGet packages — `Microsoft.Win32.Registry` is built into .NET on Windows
- Commit after each phase checkpoint
