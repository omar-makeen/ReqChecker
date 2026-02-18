# Tasks: OS Version Validation Test

**Input**: Design documents from `/specs/042-os-version-test/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Included — plan.md specifies unit tests as a deliverable.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No new projects or packages needed. Existing project structure is sufficient.

- [x] T001 Verify solution builds cleanly before changes: `dotnet build`

---

## Phase 2: Foundational (Core Test Skeleton)

**Purpose**: Create the OsVersionTest class with parameter extraction, OS detection, informational mode, validation, and error handling. This is the base that all user stories build on.

- [x] T002 Create `src/ReqChecker.Infrastructure/Tests/OsVersionTest.cs` with: `[TestType("OsVersion")]` attribute, `ITest` implementation, `ExecuteAsync` method skeleton, parameter extraction (`minimumBuild` as int?, `expectedVersion` as string?), OS version detection via `Environment.OSVersion.Version`, cancellation token handling (`OperationCanceledException` → Skipped), base `TestResult` construction (TestId, TestType, DisplayName, StartTime, EndTime, Duration via Stopwatch), informational mode — when neither `minimumBuild` nor `expectedVersion` is set, return Pass with detected version in HumanSummary (FR-005), parameter conflict validation — both set → Fail with `ErrorCategory.Configuration` (FR-006), format validation — invalid `expectedVersion` string → Fail with `ErrorCategory.Configuration` (FR-007), general exception handler → Fail with `ErrorCategory.Unknown`
- [x] T003 Create `tests/ReqChecker.Infrastructure.Tests/Tests/OsVersionTestTests.cs` with: test class scaffolding, `private readonly OsVersionTest _test = new()`, `MakeDefinition(JsonObject)` helper (Id="test-os-001", Type="OsVersion"), unit tests for: informational mode (no params → Pass), both params set → Fail/Configuration, invalid expectedVersion format → Fail/Configuration, negative minimumBuild → Fail/Configuration, cancellation → Skipped, result properties populated (TestId, TestType, DisplayName, StartTime, EndTime, Duration > TimeSpan.Zero)

**Checkpoint**: OsVersionTest compiles, informational and validation paths work, unit tests pass

---

## Phase 3: User Story 1 — Validate Minimum OS Version (Priority: P1)

**Goal**: Support a `minimumBuild` parameter that compares the detected Windows build number against the configured minimum.

**Independent Test**: Run with `minimumBuild` set to a value below the current machine's build → Pass. Set to 99999 → Fail.

### Implementation for User Story 1

- [x] T004 [US1] Add minimum build comparison logic to `src/ReqChecker.Infrastructure/Tests/OsVersionTest.cs`: extract build number from `Environment.OSVersion.Version.Build`, compare against `minimumBuild` parameter, return Pass if detected >= required, Fail with `ErrorCategory.Validation` if detected < required (FR-003), set HumanSummary to describe outcome e.g. "OS build 22631 meets minimum requirement of 19045" or "OS build 19045 does not meet minimum requirement of 22631" (FR-009)
- [x] T005 [US1] Add minimum build unit tests to `tests/ReqChecker.Infrastructure.Tests/Tests/OsVersionTestTests.cs`: test with minimumBuild=1 → Pass (any machine meets this), test with minimumBuild=999999 → Fail/Validation, verify HumanSummary contains build numbers on both pass and fail paths, verify Evidence.ResponseData is non-empty JSON

**Checkpoint**: Minimum build comparison works end-to-end, all unit tests pass

---

## Phase 4: User Story 2 — Exact Version Match (Priority: P2)

**Goal**: Support an `expectedVersion` parameter for strict version comparison ("major.minor.build").

**Independent Test**: Run with `expectedVersion` set to the current machine's version → Pass. Set to a different version → Fail.

### Implementation for User Story 2

- [x] T006 [US2] Add exact version match logic to `src/ReqChecker.Infrastructure/Tests/OsVersionTest.cs`: parse `expectedVersion` as `Version` (major.minor.build), compare against `Environment.OSVersion.Version` on Major, Minor, and Build components, return Pass on exact match, Fail with `ErrorCategory.Validation` on mismatch (FR-004), set HumanSummary to describe outcome e.g. "OS version 10.0.22631 matches expected version" or "OS version 10.0.19045 does not match expected version 10.0.22631" (FR-009)
- [x] T007 [US2] Add exact version match unit tests to `tests/ReqChecker.Infrastructure.Tests/Tests/OsVersionTestTests.cs`: test with expectedVersion matching current machine → Pass, test with expectedVersion="0.0.1" → Fail/Validation, verify HumanSummary contains version strings on both paths

**Checkpoint**: Both minimum build and exact match modes work, all unit tests pass

---

## Phase 5: User Story 3 — Report OS Details in Evidence (Priority: P3)

**Goal**: Capture rich OS evidence (product name, version, build, architecture) and add UI mappings so the test type is visually distinct.

**Independent Test**: Run any OsVersion test, inspect result details for product name, version, build number, and architecture fields. Verify the test list shows a desktop icon in purple.

### Implementation for User Story 3

- [x] T008 [US3] Enhance evidence capture in `src/ReqChecker.Infrastructure/Tests/OsVersionTest.cs`: read ProductName from registry `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion` via `Microsoft.Win32.Registry`, read architecture via `System.Runtime.InteropServices.RuntimeInformation.OSArchitecture`, serialize evidence as JSON `{ productName, version, buildNumber, architecture }` to `TestEvidence.ResponseData`, populate `TestEvidence.Timing` with `TotalMs` (FR-008)
- [x] T009 [P] [US3] Add "OsVersion" icon mapping to `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs`: add `"OsVersion" => SymbolRegular.Desktop24` case after "RegistryRead" in the switch expression (FR-010)
- [x] T010 [P] [US3] Add "OsVersion" colour mapping to `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs`: add `"OsVersion"` to the `AccentSecondary` group alongside "ProcessList", "RegistryRead", "WindowsService" (FR-010)
- [x] T011 [P] [US3] Add test-019 OsVersion entry to `src/ReqChecker.App/Profiles/default-profile.json`: id="test-019", type="OsVersion", displayName="Check Windows Version", parameters with minimumBuild=null and expectedVersion=null (informational mode), fieldPolicy both Editable, requiresAdmin=false, dependsOn=[] (FR-011)
- [x] T012 [US3] Add evidence unit tests to `tests/ReqChecker.Infrastructure.Tests/Tests/OsVersionTestTests.cs`: verify ResponseData JSON contains "productName", "version", "buildNumber", "architecture" keys, verify productName is non-empty string, verify architecture matches RuntimeInformation.OSArchitecture.ToString(), verify Timing.TotalMs >= 0

**Checkpoint**: Full evidence captured, icon/colour visible in test list, default profile includes OsVersion test

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validate everything works together, no regressions

- [x] T013 Run full test suite: `dotnet test tests/ReqChecker.Infrastructure.Tests/` — all existing tests plus new OsVersion tests must pass (SC-004)
- [x] T014 Build and launch the application, verify test-019 appears in the test list with Desktop icon and purple colour, run it, confirm informational mode passes with OS details visible in results

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — verify build
- **Foundational (Phase 2)**: Depends on Phase 1 — creates the core test class and test file
- **US1 (Phase 3)**: Depends on Phase 2 — adds minimum build comparison to existing class
- **US2 (Phase 4)**: Depends on Phase 2 — adds exact match comparison (independent of US1)
- **US3 (Phase 5)**: Depends on Phase 2 — adds evidence and UI mappings (independent of US1/US2)
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational (Phase 2) only
- **User Story 2 (P2)**: Depends on Foundational (Phase 2) only — can run in parallel with US1
- **User Story 3 (P3)**: Depends on Foundational (Phase 2) only — can run in parallel with US1/US2

### Within Each User Story

- Implementation before tests (tests validate the implementation)
- Core logic before human summary
- All tasks in a story complete before checkpoint

### Parallel Opportunities

- T009, T010, T011 can all run in parallel (different files, no dependencies)
- US1, US2, US3 can start in parallel after Phase 2 (all modify OsVersionTest.cs but different code paths; sequential recommended for single developer)

---

## Parallel Example: User Story 3

```bash
# These three tasks modify different files and can run in parallel:
Task T009: "Add OsVersion icon in TestTypeToIconConverter.cs"
Task T010: "Add OsVersion colour in TestTypeToColorConverter.cs"
Task T011: "Add test-019 entry in default-profile.json"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: Foundational (T002, T003)
3. Complete Phase 3: User Story 1 (T004, T005)
4. **STOP and VALIDATE**: OsVersion test works with minimum build comparison
5. Can demo/ship — informational mode + minimum build covers the primary use case

### Incremental Delivery

1. Foundational → Base test with informational mode + validation
2. Add US1 → Minimum build comparison → Test independently → MVP ready
3. Add US2 → Exact version match → Test independently → Enhanced
4. Add US3 → Rich evidence + UI integration → Test independently → Complete
5. Polish → Full regression check → Ship

---

## Notes

- All implementation is in a single class (`OsVersionTest.cs`) — user stories add code paths, not new files
- T009/T010/T011 are one-line changes each — quick parallel wins
- No new NuGet packages needed
- No elevated privileges required
- Test executes locally (no network) — fast and deterministic
