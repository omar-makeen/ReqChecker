# Tasks: Fix Run Progress View UI Bugs

**Input**: Design documents from `/specs/005-fix-run-progress/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4, US5)
- Include exact file paths in descriptions

## Path Conventions

- **App layer (WPF)**: `src/ReqChecker.App/`
- **Tests**: `tests/ReqChecker.App.Tests/`

---

## Phase 1: Investigation & Verification

**Purpose**: Confirm root causes identified in research.md before implementing fixes

- [X] T001 [P] Add debug logging to ProgressRing.UpdateArc() to verify it's being called when Progress changes in src/ReqChecker.App/Controls/ProgressRing.xaml.cs
- [X] T002 [P] Add debug logging to RunProgressViewModel.OnTestCompleted() to trace state updates in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T003 Run app with logging enabled, execute tests, and verify logs confirm root causes

**Checkpoint**: Root causes confirmed via debug output - ready to implement fixes

---

## Phase 2: Foundational - State Management Fixes

**Purpose**: Fix core state management issues that affect multiple UI elements

**CRITICAL**: Multiple user stories depend on proper state synchronization

- [X] T004 Add HasResults computed property that notifies when TestResults.Count changes in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T005 Add IsCancelling property for visual feedback during cancellation in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T006 Create OnCompletion() method for atomic final state updates (clear CurrentTestName, set IsComplete, stop spinner) in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T007 Update OnTestCompleted() to set CurrentTestName to NEXT test (if any) instead of completed test in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T008 Call OnCompletion() in the finally block of StartTestsAsync after all tests complete in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs

**Checkpoint**: ViewModel state management is correct - ready for UI binding fixes

---

## Phase 3: User Story 1 - Accurate Progress Display (Priority: P1)

**Goal**: Progress ring percentage matches completion ratio, and visual arc updates in real-time to match.

**Independent Test**: Run 4 tests and verify the arc visually shows 25%, 50%, 75%, 100% as each test completes.

### Implementation for User Story 1

- [X] T009 [US1] Verify OnProgressChanged callback is firing when Progress property changes via binding in src/ReqChecker.App/Controls/ProgressRing.xaml.cs
- [X] T010 [US1] Fix UpdateArc() to ensure it's called on every Progress property change in src/ReqChecker.App/Controls/ProgressRing.xaml.cs
- [X] T011 [US1] Add special case in UpdateArc() for Progress >= 100 to render full ellipse instead of degenerate arc in src/ReqChecker.App/Controls/ProgressRing.xaml.cs
- [X] T012 [US1] Verify Progress binding in RunProgressView.xaml uses correct Mode and UpdateSourceTrigger in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: Progress ring arc updates correctly from 0% to 100%

---

## Phase 4: User Story 2 - Synchronized State Display (Priority: P1)

**Goal**: All UI elements show consistent state - no "Currently Running" test when all tests complete.

**Independent Test**: Run tests to completion and verify "Currently Running" section hides or shows completion state.

### Implementation for User Story 2

- [X] T013 [US2] Add IsTestRunning computed property (true only when a test is actively executing) in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T014 [US2] Update CurrentlyRunning section visibility to bind to IsTestRunning in src/ReqChecker.App/Views/RunProgressView.xaml
- [X] T015 [US2] Add completion summary section that shows when IsComplete is true in src/ReqChecker.App/Views/RunProgressView.xaml
- [X] T016 [US2] Ensure all property updates use Dispatcher.Invoke for thread safety in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs

**Checkpoint**: UI elements show consistent state at all times

---

## Phase 5: User Story 3 - Results List Display (Priority: P1)

**Goal**: Completed test results appear in the list immediately, no "Waiting for results..." when tests have completed.

**Independent Test**: Run 4 tests and verify each result appears in the list as it completes.

### Implementation for User Story 3

- [X] T017 [US3] Change empty state visibility binding from TestResults.Count to HasResults property in src/ReqChecker.App/Views/RunProgressView.xaml
- [X] T018 [US3] Ensure OnPropertyChanged is raised for HasResults when items are added to TestResults in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T019 [US3] Verify ItemsControl ItemsSource binding is correct and updates with collection in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: Results list shows all completed tests, no stale empty state

---

## Phase 6: User Story 4 - Cancel Button Functionality (Priority: P2)

**Goal**: Cancel button stops execution when clicked, is disabled when not running.

**Independent Test**: Start tests, click Cancel immediately, verify tests stop and UI shows cancelled state.

### Implementation for User Story 4

- [X] T020 [US4] Update CancelCommand to set IsCancelling=true before calling Cts.Cancel() in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T021 [US4] Bind Cancel button IsEnabled to IsRunning && !IsCancelling in src/ReqChecker.App/Views/RunProgressView.xaml
- [X] T022 [US4] Add "Cancelling..." visual feedback when IsCancelling is true in src/ReqChecker.App/Views/RunProgressView.xaml
- [X] T023 [US4] Handle cancellation gracefully in StartTestsAsync - set appropriate final state in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs

**Checkpoint**: Cancel button works correctly with visual feedback

---

## Phase 7: User Story 5 - Post-Completion Actions (Priority: P2)

**Goal**: Clear navigation options available when tests complete or are cancelled.

**Independent Test**: Complete tests and verify "Back to Tests" or similar button is visible and works.

### Implementation for User Story 5

- [X] T024 [US5] Add BackToTestsCommand in RunProgressViewModel in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T025 [US5] Add navigation button(s) that appear when IsComplete or IsCancelling is true in src/ReqChecker.App/Views/RunProgressView.xaml
- [X] T026 [US5] Ensure navigation buttons are enabled only after execution stops in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: Users can navigate away from progress view after completion

---

## Phase 8: Cleanup & Manual Testing

**Purpose**: Remove debug code and validate all fixes work together

- [X] T027 [P] Remove debug logging added in T001-T002 (or convert to Serilog Debug level) in src/ReqChecker.App/Controls/ProgressRing.xaml.cs and src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [ ] T028 Run quickstart.md Scenario 1: Progress Ring at 100% - verify complete circle
- [ ] T029 Run quickstart.md Scenario 2: Currently Running state on completion
- [ ] T030 Run quickstart.md Scenario 3: Completed Tests list shows all results
- [ ] T031 Run quickstart.md Scenario 4: Cancel button responsiveness
- [ ] T032 Run quickstart.md Scenario 5: State synchronization - all elements consistent
- [ ] T033 Run quickstart.md Scenario 6: Single test profile
- [ ] T034 Run quickstart.md Scenario 7: All tests skipped

---

## Dependencies & Execution Order

### Phase Dependencies

- **Investigation (Phase 1)**: No dependencies - start immediately to confirm root causes
- **Foundational (Phase 2)**: Can start after Phase 1 confirms issues - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 (Phase 3): Progress ring arc fix - Core visual bug
  - US2 (Phase 4): State synchronization - Can run parallel with US1
  - US3 (Phase 5): Results list fix - Can run parallel with US1, US2
  - US4 (Phase 6): Cancel button - Can run parallel with US1-3
  - US5 (Phase 7): Navigation options - Can run after US4 (depends on completion states)
- **Cleanup (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Phase 2 - ProgressRing.xaml.cs fixes
- **User Story 2 (P1)**: Depends on Phase 2 - ViewModel state properties
- **User Story 3 (P1)**: Depends on Phase 2 (HasResults property) - View binding fixes
- **User Story 4 (P2)**: Depends on Phase 2 (IsCancelling property) - Command and binding fixes
- **User Story 5 (P2)**: Depends on US4 completion states being correct

### Parallel Opportunities

- T001, T002 can run in parallel (different files)
- T004, T005 can run in parallel (different properties in same file)
- US1, US2, US3, US4 implementation phases can run in parallel after Phase 2
- T027 cleanup tasks can run in parallel (different files)

---

## Critical Files Summary

| File | Bug Fixes |
|------|-----------|
| `src/ReqChecker.App/Controls/ProgressRing.xaml.cs` | T001, T009, T010, T011, T027 - Arc rendering fixes |
| `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` | T002, T004-T008, T013, T016, T018, T020, T023, T024, T27 - State management |
| `src/ReqChecker.App/Views/RunProgressView.xaml` | T012, T14, T15, T17, T19, T21, T22, T25, T26 - Binding and visibility fixes |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- This is a bug fix - minimize changes to existing architecture
- All UI updates must happen on UI thread (Dispatcher.Invoke)
- Test each phase checkpoint before proceeding to next phase
- Use quickstart.md scenarios for validation at each checkpoint
