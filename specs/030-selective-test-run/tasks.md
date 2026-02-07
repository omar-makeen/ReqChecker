# Tasks: Selective Test Run

**Input**: Design documents from `/specs/030-selective-test-run/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Tests are included as they help validate the selection logic correctness (critical for SC-002: "100% of runs execute exactly the tests the user selected").

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Extend IAppState and AppState to support passing selected test IDs between ViewModels, and create the SelectableTestItem wrapper ViewModel.

- [x] T001 [P] Add `SelectedTestIds` property (`IReadOnlyList<string>?`) and `SetSelectedTestIds(IReadOnlyList<string>?)` method to `src/ReqChecker.Core/Interfaces/IAppState.cs`
- [x] T002 [P] Implement `SelectedTestIds` property and `SetSelectedTestIds()` in `src/ReqChecker.App/Services/AppState.cs` — store value, no event needed (consumed once on navigation)
- [x] T003 [P] Create `SelectableTestItem` class extending `ObservableObject` in `src/ReqChecker.App/ViewModels/SelectableTestItem.cs` — wraps `TestDefinition`, adds `[ObservableProperty] bool _isSelected = true`

**Checkpoint**: Foundation ready — three new/modified files, no existing behavior changed. `dotnet build` should pass.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add the AccentCheckBox style to the design system so it's available for all UI tasks.

**Warning**: No user story UI work can begin until this style exists.

- [x] T004 Add `AccentCheckBox` style to `src/ReqChecker.App/Resources/Styles/Controls.xaml` — use `AccentPrimary` for checked fill, `DurationFast` (150ms) transition, 8px spacing grid alignment, 18x18 size matching card icon scale

**Checkpoint**: Style resource available. `dotnet build` should pass.

---

## Phase 3: User Story 1 — Select and Run Specific Tests (Priority: P1) MVP

**Goal**: Users can check/uncheck individual tests via per-card checkboxes and run only the selected subset.

**Independent Test**: Load a profile with 2+ tests, uncheck some, click Run, verify only checked tests execute and appear in results.

### Tests for User Story 1

- [x] T005 [P] [US1] Create test file `tests/ReqChecker.App.Tests/ViewModels/TestListViewModelTests.cs` with tests: (a) `SelectableTests_InitializedAllSelected_WhenProfileLoaded` — verify all items have `IsSelected=true`; (b) `RunCommand_StoresOnlySelectedTestIds_InAppState` — uncheck 2 of 4, run, assert `AppState.SelectedTestIds` contains only the 2 checked IDs; (c) `RunCommand_Disabled_WhenNoTestsSelected` — uncheck all, assert `CanExecute` is false
- [x] T006 [P] [US1] Add test to `tests/ReqChecker.App.Tests/ViewModels/TestListViewModelTests.cs`: `RunProgressViewModel_FiltersProfile_UsingSelectedTestIds` — set `SelectedTestIds` in mock AppState, construct `RunProgressViewModel`, verify `TotalTests` matches filtered count

### Implementation for User Story 1

- [x] T007 [US1] Refactor `TestListViewModel` in `src/ReqChecker.App/ViewModels/TestListViewModel.cs` — add `ObservableCollection<SelectableTestItem> SelectableTests` property; populate from `CurrentProfile.Tests` in constructor and on `CurrentProfileChanged`; replace `RunAllTests()` to write selected IDs to `_appState.SetSelectedTestIds()` before `_navigationService.NavigateToRunProgress()`; add `HasSelectedTests` computed property; update `RunAllTestsCommand` `CanExecute` to require `HasSelectedTests`
- [x] T008 [US1] Update `RunProgressViewModel` in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` — in constructor, read `_appState.SelectedTestIds`; if non-null, create a shallow copy of `CurrentProfile` with `Tests` filtered to only those IDs; set `TotalTests` from filtered count; clear `SelectedTestIds` in AppState after consumption; pass filtered profile to `_testRunner.RunTestsAsync()` in `StartTestsAsync()`
- [x] T009 [US1] Update `TestListView.xaml` in `src/ReqChecker.App/Views/TestListView.xaml` — change `ItemsSource` from `CurrentProfile.Tests` to `SelectableTests`; add a `CheckBox` column (Style=`AccentCheckBox`) as the first element in each test card grid (before type icon), bound to `IsSelected` with `Click` event handled via command or direct binding (must NOT trigger card navigation); add opacity binding on card Border: `Opacity` bound to `IsSelected` via `BoolToOpacityConverter` (1.0 when true, 0.5 when false) with 150ms animation using `DurationFast`
- [x] T010 [US1] Add `BoolToOpacityConverter` to `src/ReqChecker.App/Converters/` (if not already existing) — returns 1.0 for true, 0.5 for false; register in App.xaml or TestListView.xaml resources

**Checkpoint**: User Story 1 complete. Users can check/uncheck tests and run a subset. `dotnet build && dotnet test` should pass. Default behavior (all selected) is identical to prior "Run All" flow.

---

## Phase 4: User Story 2 — Select All / Deselect All (Priority: P2)

**Goal**: A header checkbox toggles all tests on/off, with indeterminate state when partially selected.

**Independent Test**: Toggle "Select All" checkbox, verify all per-card checkboxes update. Uncheck one test, verify header shows indeterminate. Check header again, verify all become checked.

### Tests for User Story 2

- [x] T011 [P] [US2] Add tests to `tests/ReqChecker.App.Tests/ViewModels/TestListViewModelTests.cs`: (a) `IsAllSelected_ReturnsTrue_WhenAllSelected`; (b) `IsAllSelected_ReturnsFalse_WhenNoneSelected`; (c) `IsAllSelected_ReturnsNull_WhenPartiallySelected`; (d) `ToggleSelectAll_ChecksAll_WhenIndeterminate`; (e) `ToggleSelectAll_UnchecksAll_WhenAllChecked`; (f) `ToggleSelectAll_ChecksAll_WhenNoneChecked`

### Implementation for User Story 2

- [x] T012 [US2] Add `IsAllSelected` (`bool?`) computed property to `TestListViewModel` in `src/ReqChecker.App/ViewModels/TestListViewModel.cs` — returns `true` if all `IsSelected`, `false` if none, `null` if mixed; add `ToggleSelectAllCommand` that sets all items to checked (if currently indeterminate or none) or unchecked (if all checked); subscribe to each `SelectableTestItem.PropertyChanged` to raise `OnPropertyChanged(nameof(IsAllSelected))` when any `IsSelected` changes
- [x] T013 [US2] Add "Select All" checkbox to header row in `src/ReqChecker.App/Views/TestListView.xaml` — position between test count label and run button; bind `IsChecked` to `IsAllSelected` (TwoWay); set `IsThreeState="False"` (user cannot manually set indeterminate, only computed); use `AccentCheckBox` style; add label text "Select All" next to it

**Checkpoint**: User Story 2 complete. Select All toggles all checkboxes. Indeterminate state shows when partial. `dotnet build && dotnet test` should pass.

---

## Phase 5: User Story 3 — Selection Count Feedback (Priority: P3)

**Goal**: The run button label dynamically shows "Run All Tests" or "Run N of M Tests" based on selection count.

**Independent Test**: Toggle selections and verify the button label updates in real time. With 0 selected, button is disabled.

### Tests for User Story 3

- [x] T014 [P] [US3] Add tests to `tests/ReqChecker.App.Tests/ViewModels/TestListViewModelTests.cs`: (a) `RunButtonLabel_ShowsRunAllTests_WhenAllSelected`; (b) `RunButtonLabel_ShowsRunNofM_WhenSubsetSelected`; (c) `RunButtonLabel_ShowsRunTests_WhenNoneSelected`

### Implementation for User Story 3

- [x] T015 [US3] Add `RunButtonLabel` computed string property to `TestListViewModel` in `src/ReqChecker.App/ViewModels/TestListViewModel.cs` — returns "Run All Tests" when `SelectedCount == TotalCount`, "Run N of M Tests" when `0 < SelectedCount < TotalCount`, "Run Tests" when `SelectedCount ==0`; add `SelectedCount` and `TotalCount` helper properties; raise `OnPropertyChanged(nameof(RunButtonLabel))` when any `IsSelected` changes (piggyback on existing subscription from T012)
- [x] T016 [US3] Update run button in `src/ReqChecker.App/Views/TestListView.xaml` — replace hardcoded "Run All Tests" TextBlock with binding to `RunButtonLabel`; ensure `IsEnabled` is bound to `HasSelectedTests` (from T007)

**Checkpoint**: User Story 3 complete. Button dynamically reflects selection count. `dotnet build && dotnet test` should pass.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification, edge case handling, and cleanup.

- [ ] T017 Verify edge case: profile reload resets selection in `src/ReqChecker.App/ViewModels/TestListViewModel.cs` — ensure `OnCurrentProfileChanged` rebuilds `SelectableTests` with all items `IsSelected=true`, unsubscribes from old items' `PropertyChanged`, subscribes to new items
- [ ] T018 Verify edge case: single-test profile in `src/ReqChecker.App/Views/TestListView.xaml` — confirm checkbox and "Select All" still appear, button says "Run All Tests" when the one test is checked
- [ ] T019 Run full build and test suite: `dotnet build && dotnet test` — zero errors, zero warnings, no new test failures
- [ ] T020 Manual smoke test: load a profile with 3+ tests, uncheck 2 tests, run, verify only selected tests execute and appear in results/history with correct counts

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: No dependency on Phase 1 (different files); can run in parallel with Phase 1
- **User Story 1 (Phase 3)**: Depends on Phase 1 (T001-T003) and Phase 2 (T004) completion
- **User Story 2 (Phase 4)**: Depends on User Story 1 (builds on `SelectableTests` and `PropertyChanged` subscription)
- **User Story 3 (Phase 5)**: Depends on User Story 1 (builds on `SelectableTests`); can run in parallel with User Story 2
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Core feature. Requires Phase 1 + Phase 2. No dependency on other stories.
- **User Story 2 (P2)**: Adds Select All. Requires US1 (needs `SelectableTests` collection and `PropertyChanged` subscription pattern).
- **User Story 3 (P3)**: Adds dynamic label. Requires US1 (needs `SelectableTests` collection). Independent of US2 — can run in parallel with it.

### Within Each User Story

- Tests written first (T005/T006, T011, T014)
- ViewModel logic before XAML view changes
- Integration/wiring last

### Parallel Opportunities

- **Phase 1**: T001, T002, T003 are all different files — run in parallel
- **Phase 1 + 2**: T004 (Controls.xaml) can run in parallel with T001-T003
- **US1 Tests**: T005 and T006 can run in parallel (same file but independent test methods)
- **US2 + US3**: Can run in parallel after US1 (touch different parts of same files, but US3 doesn't depend on US2)

---

## Parallel Example: Phase 1 + 2

```bash
# All four setup tasks touch different files — launch together:
Task T001: "Add SelectedTestIds to IAppState.cs"
Task T002: "Implement SelectedTestIds in AppState.cs"
Task T003: "Create SelectableTestItem.cs"
Task T004: "Add AccentCheckBox style to Controls.xaml"
```

## Parallel Example: User Story 1

```bash
# Tests first (parallel):
Task T005: "Unit tests for TestListViewModel selection logic"
Task T006: "Unit test for RunProgressViewModel filtering"

# Then implementation (sequential within story):
Task T007: "Refactor TestListViewModel with SelectableTests"
Task T008: "Update RunProgressViewModel to filter by SelectedTestIds"
Task T009: "Update TestListView.xaml with checkboxes and opacity"
Task T010: "Add BoolToOpacityConverter"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003) + Phase 2: Foundational (T004)
2. Complete Phase 3: User Story 1 (T005-T010)
3. **STOP and VALIDATE**: Test independently — users can check/uncheck and run subsets
4. This alone delivers the core value: debugging one failing test without running all 20+

### Incremental Delivery

1. Complete Setup + Foundational (T001-T004) → Foundation ready
2. Add User Story 1 (T005-T010) → Test independently → **MVP!**
3. Add User Story 2 (T011-T013) → Test independently → Bulk selection convenience
4. Add User Story 3 (T014-T016) → Test independently → Polish with dynamic label
5. Polish (T017-T020) → Final validation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each phase completion
- Stop at any checkpoint to validate story independently
- No changes to ITestRunner, SequentialTestRunner, or profile JSON schema
- Default all-selected behavior must be preserved throughout (SC-003)
