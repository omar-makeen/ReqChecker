# Tasks: Fix Test Results Window

**Input**: Design documents from `/specs/008-fix-results-window/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md

**Tests**: Unit tests included as this is a bug fix requiring verification of correct behavior.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: WPF Desktop App with MVVM architecture
- **Source**: `src/ReqChecker.App/`
- **Tests**: `tests/ReqChecker.App.Tests/`

---

## Phase 1: Setup (Verification)

**Purpose**: Verify current state and ensure build works

- [X] T001 Verify solution builds successfully by running `dotnet build`
- [X] T002 Run existing tests to establish baseline with `dotnet test`

---

## Phase 2: Foundational (No blocking prerequisites)

**Purpose**: This bug fix has no foundational changes - all modifications are in existing files

**⚠️ Note**: No new entities, services, or infrastructure required. Proceeding directly to user stories.

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - View Previous Test Results (Priority: P1) 🎯 MVP

**Goal**: Fix data loading so test results display when navigating to Results view

**Independent Test**: Run tests, click "Results" in sidebar, verify all test results appear with correct counts

### Tests for User Story 1

- [X] T003 [P] [US1] Add test for NavigateToResults loading Report from AppState in tests/ReqChecker.App.Tests/ViewModels/ResultsViewModelTests.cs
- [X] T004 [P] [US1] Add test for FilteredResults populated when Report is set in tests/ReqChecker.App.Tests/ViewModels/ResultsViewModelTests.cs

### Implementation for User Story 1

- [X] T005 [US1] Modify NavigateToResults() to get AppState from DI and set viewModel.Report = appState.LastRunReport in src/ReqChecker.App/Services/NavigationService.cs
- [X] T006 [US1] Verify empty state message displays when Report is null (no prior test run) in src/ReqChecker.App/Views/ResultsView.xaml

**Checkpoint**: User Story 1 complete - Results view now shows test data when navigating via sidebar

---

## Phase 4: User Story 2 - Export Results to JSON (Priority: P2)

**Goal**: Fix JSON export button to work when results are available

**Independent Test**: Run tests, navigate to Results, click JSON button, verify save dialog appears

### Tests for User Story 2

- [X] T007 [P] [US2] Add test for CanExport returns true when Report is set in tests/ReqChecker.App.Tests/ViewModels/ResultsViewModelTests.cs
- [X] T008 [P] [US2] Add test for CanExport returns false when Report is null in tests/ReqChecker.App.Tests/ViewModels/ResultsViewModelTests.cs

### Implementation for User Story 2

- [X] T009 [US2] Add CanExport computed property (Report != null) to ResultsViewModel in src/ReqChecker.App/ViewModels/ResultsViewModel.cs
- [X] T010 [US2] Bind JSON export button IsEnabled to CanExport property in src/ReqChecker.App/Views/ResultsView.xaml

**Checkpoint**: User Story 2 complete - JSON export button enabled/disabled based on data availability

---

## Phase 5: User Story 3 - Export Results to CSV (Priority: P2)

**Goal**: Fix CSV export button to work when results are available

**Independent Test**: Run tests, navigate to Results, click CSV button, verify save dialog appears

### Implementation for User Story 3

- [X] T011 [US3] Bind CSV export button IsEnabled to CanExport property in src/ReqChecker.App/Views/ResultsView.xaml

**Checkpoint**: User Story 3 complete - CSV export button enabled/disabled based on data availability

**Note**: Uses same CanExport property added in US2, so no additional ViewModel changes needed

---

## Phase 6: User Story 4 - Filter Test Results (Priority: P3)

**Goal**: Verify filter tabs work correctly (All/Passed/Failed/Skipped)

**Independent Test**: Run tests with mixed outcomes, click each filter tab, verify correct filtering

### Implementation for User Story 4

- [X] T012 [US4] Manual verification: Run app with Sample Diagnostics profile, run tests, navigate to Results, test each filter tab
- [X] T013 [US4] If filters not working: Debug FilterTestResult predicate and FilteredResults binding in src/ReqChecker.App/ViewModels/ResultsViewModel.cs

**Checkpoint**: User Story 4 complete - All filter tabs correctly filter results

**Note**: Filter logic already exists. Once US1 (data loading) is fixed, filters should work automatically.

---

## Phase 7: User Story 5 - Navigation Menu Highlighting (Priority: P3)

**Goal**: Highlight "Results" menu item when Results view is active

**Independent Test**: Click Results in sidebar, verify menu item is highlighted

### Implementation for User Story 5

- [X] T014 [US5] Add NavResults.IsActive = true in NavigateWithAnimation() case "Results" in src/ReqChecker.App/MainWindow.xaml.cs

**Checkpoint**: User Story 5 complete - Results menu item highlighted when active

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup

- [X] T015 Run all unit tests to verify no regressions with `dotnet test`
- [X] T016 Manual end-to-end testing following quickstart.md validation steps
- [X] T017 Verify all success criteria from spec.md are met

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately
- **Foundational (Phase 2)**: N/A - no foundational work needed
- **User Story 1 (Phase 3)**: No dependencies - can start after Setup
- **User Story 2 (Phase 4)**: No dependencies on US1 for implementation, but testing requires US1 complete
- **User Story 3 (Phase 5)**: Depends on US2 (uses CanExport property)
- **User Story 4 (Phase 6)**: Depends on US1 (filters need data to filter)
- **User Story 5 (Phase 7)**: No dependencies - can run in parallel with US1-4
- **Polish (Phase 8)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Core fix - No dependencies
- **User Story 2 (P2)**: Independent implementation, but practical testing requires US1
- **User Story 3 (P2)**: Depends on US2's CanExport property
- **User Story 4 (P3)**: Practical testing requires US1 (needs data to filter)
- **User Story 5 (P3)**: Fully independent - can run in parallel

### Within Each User Story

- Tests written first (where applicable)
- ViewModel changes before View changes
- Core implementation before integration

### Parallel Opportunities

Within Phase 3 (User Story 1):
- T003 and T004 can run in parallel (different test methods)

Within Phase 4 (User Story 2):
- T007 and T008 can run in parallel (different test methods)

Across User Stories:
- US5 (T014) can run in parallel with any other story

---

## Parallel Example: User Story 1 & 5

```bash
# These can run in parallel (different files):
Task: T005 [US1] Modify NavigateToResults() in NavigationService.cs
Task: T014 [US5] Add NavResults.IsActive in MainWindow.xaml.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 3: User Story 1 (T003-T006)
3. **STOP and VALIDATE**: Test by running app, executing tests, clicking Results
4. Data should now display in Results view

### Recommended Execution Order

1. T001-T002: Setup verification
2. T003-T006: User Story 1 (core data loading fix)
3. T007-T010: User Story 2 (JSON export enable/disable)
4. T011: User Story 3 (CSV export - trivial, uses US2's property)
5. T012-T013: User Story 4 (verify filters - likely works after US1)
6. T014: User Story 5 (menu highlighting - independent)
7. T015-T017: Polish and final verification

### Time Estimate

- **MVP (US1 only)**: ~30 minutes
- **All Stories**: ~1-2 hours
- **Risk**: Low - all changes are localized bug fixes

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- User Story 1 is the critical fix - all others depend on it working
- Most implementation follows existing patterns (see research.md)
- No new files created - all modifications to existing files
- Commit after each user story completion for easy rollback
