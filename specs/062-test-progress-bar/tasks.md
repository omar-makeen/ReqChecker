# Tasks: Test Progress Bar Enhancements

**Input**: Design documents from `/specs/062-test-progress-bar/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Tests are included — the spec defines measurable success criteria requiring validation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No new project setup needed — all changes are within existing files.

*No setup tasks required.*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No foundational tasks needed — all changes build on existing `RunProgressViewModel` infrastructure.

*No foundational tasks required.*

---

## Phase 3: User Story 1 — Show Sequential Test Position During Execution (Priority: P1) 🎯 MVP

**Goal**: Display a "Test X of Y" counter near the progress ring that increments after each test completes.

**Independent Test**: Run any test suite and verify a sequential position counter is visible during execution, incrementing after each test.

### Tests for User Story 1

- [X] T001 [P] [US1] Create test file `tests/ReqChecker.App.Tests/ViewModels/RunProgressViewModelTests.cs` with test: `TestPositionText_ShouldShowCorrectPosition_DuringExecution` — set `CurrentTestIndex` and `TotalTests`, verify `TestPositionText` returns "Test X of Y"
- [X] T002 [P] [US1] Add test `TestPositionText_ShouldUpdate_WhenCurrentTestIndexChanges` — change `CurrentTestIndex`, verify `TestPositionText` updates accordingly
- [X] T003 [P] [US1] Add test `TestPositionText_ShouldBeEmpty_WhenNotRunning` — verify `TestPositionText` is empty when `IsRunning` is false

### Implementation for User Story 1

- [X] T004 [US1] Add computed `TestPositionText` property to `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` that returns `"Test {CurrentTestIndex + 1} of {TotalTests}"` when `IsRunning` is true, or empty string otherwise. Add `OnPropertyChanged(nameof(TestPositionText))` to `OnCurrentTestIndexChanged` and `OnIsRunningChanged` partial methods.
- [X] T005 [US1] Add a `TextBlock` bound to `TestPositionText` in `src/ReqChecker.App/Views/RunProgressView.xaml`, positioned below the `ProgressRing` control (Grid.Row="0" area) and above the card area (Grid.Row="1"). Use `TextH3` style, `TextSecondary` foreground, center-aligned. Visibility bound to `IsRunning` via `BoolToVisibilityConverter`.

**Checkpoint**: "Test X of Y" counter visible during execution, incrementing after each test completes

---

## Phase 4: User Story 2 — Auto-Navigate to Results After Completion (Priority: P2)

**Goal**: After all tests complete (not cancelled), automatically navigate to results after a 3-second delay. Cancellable by user clicking either navigation button.

**Independent Test**: Run tests, wait for completion, verify auto-navigation to results after ~3 seconds without clicking anything.

### Tests for User Story 2

- [X] T006 [P] [US2] Add test `AutoNavTimer_ShouldNotStart_WhenRunCancelled` to `tests/ReqChecker.App.Tests/ViewModels/RunProgressViewModelTests.cs` — simulate cancellation (set `IsCancelling = true` before completion), verify no auto-navigation timer is created
- [X] T007 [P] [US2] Add test `ViewResults_ShouldStopAutoNavTimer` — verify that calling `ViewResultsCommand` stops the timer (no double-navigation)
- [X] T008 [P] [US2] Add test `NavigateToTestList_ShouldStopAutoNavTimer` — verify that calling `NavigateToTestListCommand` stops the timer
- [X] T009 [P] [US2] Add test `Dispose_ShouldStopAutoNavTimer` — verify that disposing the ViewModel stops and nullifies the timer

### Implementation for User Story 2

- [X] T010 [US2] Add private `DispatcherTimer? _autoNavTimer` field to `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`. Create a `StartAutoNavigationTimer()` method that initializes a `DispatcherTimer` with a 3-second interval. On Tick, call `ViewResults()` and stop the timer. Only start the timer when `RunReport` is not null.
- [X] T011 [US2] Call `StartAutoNavigationTimer()` at the end of `OnCompletion()` in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`, but only when `IsCancelling` is false (normal completion, not user cancellation).
- [X] T012 [US2] Add a `StopAutoNavigationTimer()` method that calls `_autoNavTimer?.Stop()` and sets `_autoNavTimer = null`. Call it at the beginning of both `NavigateToTestList()` and `ViewResults()` methods in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`.
- [X] T013 [US2] Implement `IDisposable` on `RunProgressViewModel` in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` with a `_disposed` guard. In `Dispose()`, call `StopAutoNavigationTimer()` and dispose the CancellationTokenSource if still alive. This follows the pattern from `ProfileSelectorViewModel`.

**Checkpoint**: Auto-navigation fires 3 seconds after completion; cancellable by clicking either button; no auto-nav on cancelled runs

---

## Phase 5: User Story 3 — Show Completion Summary with Pass/Fail Counts (Priority: P2)

**Goal**: Enhance the completion card to show pass/fail/skip breakdown with color-coded counts and a distinct all-passed message.

**Independent Test**: Run tests with mixed outcomes, verify completion card shows breakdown. Run tests that all pass, verify "All X tests passed" message.

### Tests for User Story 3

- [X] T014 [P] [US3] Add test `CompletionSummaryText_ShouldShowAllPassed_WhenNoFailuresOrSkips` to `tests/ReqChecker.App.Tests/ViewModels/RunProgressViewModelTests.cs` — set CompletedTests=5, FailedTests=0, SkippedTests=0, TotalTests=5, verify text is "All 5 tests passed"
- [X] T015 [P] [US3] Add test `CompletionSummaryText_ShouldShowBreakdown_WhenMixedResults` — set CompletedTests=3, FailedTests=1, SkippedTests=1, verify text contains pass/fail/skip counts
- [X] T016 [P] [US3] Add test `CompletionSummaryText_ShouldShowAllSkipped_WhenAllSkipped` — set CompletedTests=0, FailedTests=0, SkippedTests=5, verify text shows "0 passed, 0 failed, 5 skipped"

### Implementation for User Story 3

- [X] T017 [US3] Add computed `CompletionSummaryText` property to `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`. Return "All {TotalTests} tests passed" when `FailedTests == 0 && SkippedTests == 0 && CompletedTests > 0`. Otherwise return "{CompletedTests} passed, {FailedTests} failed, {SkippedTests} skipped". Add `OnPropertyChanged(nameof(CompletionSummaryText))` to `OnIsCompleteChanged`.
- [X] T018 [US3] Update the completion summary card in `src/ReqChecker.App/Views/RunProgressView.xaml` (lines 232-254). Replace the static "All tests completed" TextBlock and "{0} tests executed" TextBlock with: (1) a TextBlock bound to `CompletionSummaryText` using `TextBody` style, and (2) color-coded pass/fail/skip counts below it using `StatusPass`/`StatusFail`/`StatusSkip` foreground colors (horizontal StackPanel with three small TextBlocks). Keep the CheckmarkCircle24 icon.

**Checkpoint**: Completion card shows "All X tests passed" or breakdown with color-coded counts

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [X] T019 Run all tests in `tests/ReqChecker.App.Tests/` to verify no regressions
- [X] T020 Run quickstart.md manual validation scenarios 1–10

---

## Dependencies & Execution Order

### Phase Dependencies

- **User Story 1 (Phase 3)**: No dependencies — can start immediately
- **User Story 2 (Phase 4)**: No dependencies on US1 — can start in parallel
- **User Story 3 (Phase 5)**: No dependencies on US1 or US2 — can start in parallel
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent — `TestPositionText` computed property + XAML binding
- **User Story 2 (P2)**: Independent — `DispatcherTimer` + `IDisposable` + timer cancellation
- **User Story 3 (P2)**: Independent — `CompletionSummaryText` computed property + XAML update

### Within Each User Story

- Tests written first → verify they fail → then implement
- ViewModel changes before XAML changes

### Parallel Opportunities

- **All three user stories can be developed in parallel** — they modify different parts of the same files with no conflicts:
  - US1: adds `TestPositionText` property + TextBlock below progress ring
  - US2: adds timer field + methods + IDisposable
  - US3: adds `CompletionSummaryText` property + updates completion card
- All test tasks within each story (T001–T003, T006–T009, T014–T016) can run in parallel

---

## Parallel Example: All Stories

```bash
# All three stories can start simultaneously since they are independent:

# Story 1 (position counter):
Task T001-T003: Tests for TestPositionText
Task T004: Add TestPositionText property
Task T005: Add TextBlock to XAML

# Story 2 (auto-navigation):
Task T006-T009: Tests for auto-nav timer
Task T010-T013: Timer implementation + IDisposable

# Story 3 (completion summary):
Task T014-T016: Tests for CompletionSummaryText
Task T017: Add CompletionSummaryText property
Task T018: Update completion card XAML
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 3: User Story 1 (T001–T005)
2. **STOP and VALIDATE**: Run tests, verify "Test X of Y" counter works
3. This delivers the most impactful improvement — sequential position awareness

### Incremental Delivery

1. Add User Story 1 → "Test X of Y" counter (MVP!)
2. Add User Story 2 → auto-navigation after completion
3. Add User Story 3 → enhanced completion summary
4. Polish → full regression + manual validation

---

## Notes

- All three user stories modify `RunProgressViewModel.cs` and `RunProgressView.xaml` but touch different sections — no merge conflicts expected
- US2 requires `IDisposable` implementation — follows the pattern from `ProfileSelectorViewModel` (feature 060)
- The existing `NavigationService.TrackViewModel()` already calls `Dispose()` on IDisposable ViewModels, so timer cleanup is automatic
- `DispatcherTimer` requires `System.Windows.Threading` namespace — already available in WPF projects
- Tests may need to mock `DispatcherTimer` behavior or test the ViewModel state changes without actual timer execution
