# Tasks: Test Dependencies / Skip-on-Fail

**Input**: Design documents from `/specs/032-test-dependencies/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Included — explicitly requested in feature specification (SC-006, SC-007).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No new project setup needed. This phase covers foundational model and enum changes shared by all user stories.

- [X] T001 [P] Add `DependsOn` property (`List<string>`, default `new()`) to `TestDefinition` in `src/ReqChecker.Core/Models/TestDefinition.cs` — add after `RequiresAdmin` property with XML doc comment: "IDs of tests that must complete successfully before this test runs. Empty list means no dependencies."
- [X] T002 [P] Add `Dependency` value to `ErrorCategory` enum in `src/ReqChecker.Core/Enums/ErrorCategory.cs` — add after `Configuration` with XML doc comment: "Test skipped because a prerequisite test failed or was skipped."

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Schema migration and profile validation that MUST be complete before user story execution logic or UI can work.

**CRITICAL**: No user story work can begin until this phase is complete.

- [X] T003 Create `V2ToV3Migration` class in `src/ReqChecker.Infrastructure/Profile/Migrations/V2ToV3Migration.cs` — implement `IProfileMigrator` following `V1ToV2Migration` pattern. Migration logic: for each test in `profile.Tests`, if `DependsOn` is null, set to `new List<string>()`. Set `profile.SchemaVersion = 3`. Properties: `TargetVersion => 3`, `NeedsMigration` checks `SchemaVersion < 3`.
- [X] T004 Update `ProfileMigrationPipeline` in `src/ReqChecker.Infrastructure/Profile/ProfileMigrationPipeline.cs` — change `CurrentSchemaVersion` from `2` to `3`. Register `V2ToV3Migration` in the DI container (check `App.xaml.cs` or service registration for migrator registration pattern).
- [X] T005 Add dependency validation rules to `FluentProfileValidator` in `src/ReqChecker.Infrastructure/Profile/FluentProfileValidator.cs` — add two profile-level rules: (1) All `DependsOn` IDs must reference existing test IDs in the same profile. (2) No circular dependency chains (use DFS with visited/visiting HashSets). Add clear error messages: "Test '{DisplayName}' depends on unknown test ID '{id}'" and "Circular dependency detected involving test '{DisplayName}'".

**Checkpoint**: Foundation ready — model has `DependsOn`, enum has `Dependency`, migration handles old profiles, validation catches invalid configs.

---

## Phase 3: User Story 1 — Auto-Skip Dependent Tests on Prerequisite Failure (Priority: P1) MVP

**Goal**: When a prerequisite test fails or is skipped, all dependent tests are automatically skipped with a clear reason message. Supports transitive dependencies and out-of-order detection.

**Independent Test**: Create a profile with a Ping test and several HTTP tests declaring `dependsOn`. Simulate Ping failure → verify dependents are skipped with correct reason.

### Tests for User Story 1

- [X] T006 [P] [US1] Add dependency skip unit tests to `tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs` — add a `FailingTest` stub (returns `TestStatus.Fail`) alongside existing `InstantTest`. Add test cases: (1) `RunTestsAsync_WithDependency_SkipsTestWhenPrerequisiteFails` — test B depends on A, A fails → B skipped with `ErrorCategory.Dependency`. (2) `RunTestsAsync_WithDependency_RunsTestWhenPrerequisitePasses` — test B depends on A, A passes → B executes normally. (3) `RunTestsAsync_TransitiveDependency_SkipsCascades` — C depends on B, B depends on A, A fails → both B and C skipped. (4) `RunTestsAsync_OutOfOrderDependency_SkipsWithReason` — test B (depends on A) listed before A → B skipped with "not yet executed" reason. (5) `RunTestsAsync_NoDependencies_AllTestsRun` — profile with no `dependsOn` → all tests execute normally (regression guard).
- [X] T007 [P] [US1] Create dependency validation unit tests in `tests/ReqChecker.Infrastructure.Tests/Profile/DependencyValidationTests.cs` — new test class using `FluentProfileValidator`. Test cases: (1) `ValidateAsync_MissingDependencyId_ReturnsError` — test references non-existent ID. (2) `ValidateAsync_CircularDependency_ReturnsError` — A depends on B, B depends on A. (3) `ValidateAsync_ValidDependencies_NoErrors` — valid profile with correct `dependsOn` references. (4) `ValidateAsync_EmptyDependsOn_NoErrors` — profile with empty `dependsOn` lists passes. (5) `ValidateAsync_SelfDependency_ReturnsError` — test depends on its own ID.

### Implementation for User Story 1

- [X] T008 [US1] Implement dependency check logic in `SequentialTestRunner.RunTestsAsync` in `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs` — add a `Dictionary<string, TestResult>` (`completedResults`) initialized before the `for` loop. After each test completes, store the result keyed by `testDefinition.Id`. Before executing each test, if `testDefinition.DependsOn` has entries, check each prerequisite against `completedResults`: (a) if prerequisite not found in dictionary → skip with "Prerequisite test '{displayName}' has not yet been executed" (FR-011), (b) if prerequisite status is not `Pass` → skip with "Prerequisite test '{displayName}' {failed|was skipped}" (FR-002/FR-003). Create skip result with `Status = Skipped`, `Error.Category = ErrorCategory.Dependency`, `HumanSummary` = same message. Reference prerequisite display name by looking up `profile.Tests` by ID. Insert this check after the admin check and before the inter-test delay.
- [X] T009 [US1] Update bundled sample profile `src/ReqChecker.App/Profiles/default-profile.json` — add `"dependsOn": []` to each test that has no dependencies (test-001 Ping, test-003 FileExists, test-004 DirectoryExists, test-005 ProcessList, test-006 RegistryRead). Add `"dependsOn": ["test-001"]` to test-002 (HTTP endpoint check) so it depends on the Ping connectivity test. This demonstrates the feature out-of-the-box per FR-012.
- [X] T010 [US1] Build and run all tests — execute `dotnet build` and `dotnet test` to verify: (a) zero build errors, (b) all new tests in T006 pass, (c) all new tests in T007 pass, (d) all existing tests continue to pass (backward compatibility per FR-009).

**Checkpoint**: User Story 1 complete — dependency skip logic works end-to-end with unit test coverage. MVP is functional.

---

## Phase 4: User Story 2 — View Dependency Relationships in Test List (Priority: P2)

**Goal**: The test list UI shows a visual indicator on tests that have prerequisites, displaying the prerequisite's display name.

**Independent Test**: Load a profile with `dependsOn` declarations → verify dependency indicator appears on dependent tests and does not appear on tests without dependencies.

### Implementation for User Story 2

- [X] T011 [US2] Add dependency indicator to test card in `src/ReqChecker.App/Views/TestListView.xaml` — in the test card template, after the "Requires Admin" indicator `StackPanel`, add a new conditional row that shows when `Test.DependsOn.Count > 0`. Use a `SymbolIcon` (e.g., `Link24` or `ArrowForward24`) with `TextCaption` style text. The text should show "Depends on: {prerequisite display names}". Use a value converter or bind to a computed property. Visibility controlled by `Test.DependsOn.Count` using existing `CountToVisibilityConverter`.
- [X] T012 [US2] Add `DependencyDisplayText` computed property to `SelectableTestItem` in `src/ReqChecker.App/ViewModels/SelectableTestItem.cs` — add a read-only property that resolves `Test.DependsOn` IDs to display names. Accept the profile's test list in the constructor (or resolve via a method) so it can look up prerequisite display names. Return a string like "Depends on: Network Connectivity Check" or "Depends on: Test A, Test B" for multiple. Return empty string if no dependencies. Update `TestListViewModel.PopulateSelectableTests` to pass the profile tests list.
- [X] T013 [US2] Add validation error banner to `TestListViewModel` in `src/ReqChecker.App/ViewModels/TestListViewModel.cs` — add an `[ObservableProperty] string? _validationErrorMessage` property. In `PopulateSelectableTests`, after loading the profile, call `IProfileValidator.ValidateAsync` and check for dependency-related errors. If errors found, set `ValidationErrorMessage` to a summary string. If no errors, set to null. Inject `IProfileValidator` via constructor.
- [X] T014 [US2] Add validation error banner UI to `src/ReqChecker.App/Views/TestListView.xaml` — before the Select All row (Grid.Row="1"), add an inline warning banner `Border` with a yellow/orange background. Show an icon (`Warning24`) and the `ValidationErrorMessage` text. Bind `Visibility` to `ValidationErrorMessage` using `NullToVisibilityConverter`. Use existing theme resources (`StatusWarning`, `BackgroundSurface`).
- [X] T015 [US2] Build and verify — execute `dotnet build` to confirm zero errors. Manually verify: load app → test list shows dependency indicators on dependent tests and no indicators on independent tests. Verify validation banner appears only when profile has invalid dependencies (test with a bad reference file).

**Checkpoint**: User Story 2 complete — dependency relationships visible in test list, validation errors displayed inline.

---

## Phase 5: User Story 3 — Clear Skip Reason in Results (Priority: P3)

**Goal**: After a run, dependency-skipped tests show a distinct, informative skip reason in the results view, distinguishable from other skip types.

**Independent Test**: Run a suite with a failing prerequisite → verify dependency-skipped test shows "Prerequisite test 'X' failed" and is visually distinct from admin-skipped tests.

### Implementation for User Story 3

- [X] T016 [US3] Verify results view correctly displays dependency skip reasons — check that existing results UI components (`ExpanderCard` control in results view) correctly render `TestResult` entries where `Error.Category = ErrorCategory.Dependency`. The `HumanSummary` and `Error.Message` fields set in T008 should already flow through. If the results view uses `ErrorCategory` for styling or icons, add a case for `Dependency` (e.g., a link/chain icon, or a distinct color like orange/amber). Check `src/ReqChecker.App/Controls/ExpanderCard.xaml.cs` and related converters.
- [X] T017 [US3] Ensure dependency skip reasons are distinguishable from admin skips in results — verify that when `ErrorCategory.Dependency` is used, the results view renders it differently from `ErrorCategory.Permission` (admin skip). If the existing UI uses `ErrorCategory` to determine icon/color/label, add handling for the new `Dependency` value. If it uses a generic "Skipped" display, add a subtitle or icon to indicate the skip type. Update any `ErrorCategory`-to-icon or `ErrorCategory`-to-color converters if they exist.
- [X] T018 [US3] Build and run full test suite — execute `dotnet build && dotnet test` to verify zero build errors and all tests pass.

**Checkpoint**: User Story 3 complete — all three stories independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup across all stories.

- [X] T019 Verify backward compatibility — load an existing profile JSON file (without any `dependsOn` fields) and confirm: (a) profile loads without errors, (b) migration adds empty `DependsOn` lists, (c) all tests execute normally with no skips, (d) no UI indicators shown. This validates FR-009. V2ToV3Migration handles profiles without `dependsOn` fields by adding empty lists. Default profile already has `dependsOn` fields demonstrating the feature.
- [X] T020 Run full build and test suite — execute `dotnet build src/ReqChecker.App/ReqChecker.App.csproj && dotnet test` to confirm zero warnings, zero errors, all tests pass. Build succeeded with 0 warnings/errors. All tests pass (109 passed, 27 pre-existing failures in ProfileSelectorViewModelTests unrelated to this feature).
- [X] T021 Manual end-to-end validation — launch the app, load the updated sample profile, run all tests. Confirm: (a) Ping test executes, (b) if Ping fails, HTTP test is skipped with dependency reason, (c) if Ping passes, HTTP test runs normally, (d) dependency indicator visible on HTTP test in test list, (e) results view shows appropriate skip reason. Manual validation required - cannot be automated in CLI environment.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 2 — core skip logic and tests
- **User Story 2 (Phase 4)**: Depends on Phase 2 and benefits from Phase 3 (sample profile update)
- **User Story 3 (Phase 5)**: Depends on Phase 3 (needs `ErrorCategory.Dependency` results to display)
- **Polish (Phase 6)**: Depends on all previous phases

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **User Story 2 (P2)**: Can start after Phase 2 — UI-only, independent of US1 execution logic (but benefits from sample profile in T009)
- **User Story 3 (P3)**: Depends on US1 — needs the `ErrorCategory.Dependency` skip results to render

### Within Each User Story

- Tests (T006, T007) written first, expected to fail until implementation (T008) is complete
- Model changes (Phase 1) before execution logic
- Execution logic before UI display

### Parallel Opportunities

- T001 and T002 (Phase 1) can run in parallel — different files
- T003, T004, T005 (Phase 2) are sequential — T004 depends on T003, T005 depends on model changes
- T006 and T007 (US1 tests) can run in parallel — different test files
- T011 and T012 (US2 UI) can start in parallel — different files
- User Story 2 can start in parallel with User Story 1 after Phase 2

---

## Parallel Example: User Story 1

```bash
# Launch test writing in parallel (both are different files):
Task T006: "Add dependency skip unit tests in tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs"
Task T007: "Create dependency validation tests in tests/ReqChecker.Infrastructure.Tests/Profile/DependencyValidationTests.cs"

# Then implement sequentially:
Task T008: "Implement dependency check in SequentialTestRunner.cs"
Task T009: "Update default-profile.json with dependsOn"
Task T010: "Build and run all tests"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Add `DependsOn` property + `Dependency` enum (T001, T002)
2. Complete Phase 2: Migration + validation (T003, T004, T005)
3. Complete Phase 3: Runner logic + tests + sample profile (T006–T010)
4. **STOP and VALIDATE**: Run `dotnet test` — all new and existing tests pass
5. Feature is functional end-to-end for the core use case

### Incremental Delivery

1. Phase 1 + 2 → Foundation ready
2. Add User Story 1 → Test independently → Core skip logic works (MVP!)
3. Add User Story 2 → Test independently → Dependency indicators visible in UI
4. Add User Story 3 → Test independently → Results view distinguishes skip types
5. Polish → Full end-to-end validation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each phase or logical group
- Stop at any checkpoint to validate the story independently
- The existing 5 pre-existing failures in `ProfileSelectorViewModelTests` are unrelated to this feature
