# Tasks: Re-run Failed Tests Only

**Input**: Design documents from `/specs/034-rerun-failed-tests/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: User Story 1 - Re-run Failed Tests from Results Page (Priority: P1) 🎯 MVP

**Goal**: Add a "Re-run Failed" PrimaryButton to the Results page that collects failed test IDs, stores them via `IAppState.SetSelectedTestIds`, and navigates to the Run Progress page.

**Independent Test**: Run a profile with a mix of passing and failing tests. On the Results page, verify the "Re-run Failed" button appears (gradient PrimaryButton, ArrowRepeatAll24 icon, positioned after "Back to Tests"). Click it — only failed tests should execute. Run with all-passing profile — button should be hidden.

### Implementation for User Story 1

- [ ] T001 [P] [US1] Add `HasFailedTests` computed property and `RerunFailedTestsCommand` to `src/ReqChecker.App/ViewModels/ResultsViewModel.cs` — `HasFailedTests` returns true when `Report?.Results` contains any result with `Status == TestStatus.Fail`; notify on change when `Report` changes. `RerunFailedTestsCommand` collects all `TestId` values where `Status == Fail`, calls `_appState.SetSelectedTestIds(failedIds)`, then `_navigationService.NavigateToRunProgress()`. CanExecute: `HasFailedTests && _navigationService != null`.
- [ ] T002 [P] [US1] Add "Re-run Failed" PrimaryButton to `src/ReqChecker.App/Views/ResultsView.xaml` — insert after "Back to Tests" button and before the JSON export button in the header action StackPanel. Use `Style="{StaticResource PrimaryButton}"`, `Command="{Binding RerunFailedTestsCommand}"`, `Visibility` bound to `HasFailedTests` via `BooleanToVisibilityConverter`. Icon: `ArrowRepeatAll24` with `FontSize="16"` and `Margin="0,0,8,0"`. Button `Margin="0,0,8,0"`. Update TabIndex values: Re-run Failed = 2, shift JSON/CSV/PDF to 3/4/5.
- [ ] T003 [US1] Verify `dotnet build` succeeds with zero errors, then launch app and manually test: (1) run profile with failing tests → button visible, click re-runs only failed tests; (2) run profile with all passing → button hidden.

**Checkpoint**: User Story 1 is fully functional. Users can re-run failed tests with a single click. `dotnet build` succeeds.

---

## Phase 2: User Story 2 - Re-run Includes Dependency-Skipped Tests (Priority: P2)

**Goal**: Enhance the re-run command to also include tests that were skipped due to a dependency on a failed test (identified by `ErrorCategory.Dependency`), so the full dependency chain is re-tested.

**Independent Test**: Run a profile where test A fails and test B (which depends on A) is skipped. Click "Re-run Failed" — both A and B should be included. Verify tests skipped due to cancellation or admin privileges are NOT included.

### Implementation for User Story 2

- [ ] T004 [US2] Extend `RerunFailedTestsCommand` in `src/ReqChecker.App/ViewModels/ResultsViewModel.cs` — in addition to collecting `Fail` test IDs, also collect `TestId` values where `Status == TestStatus.Skipped` AND `Error?.Category == ErrorCategory.Dependency`. Union both sets (deduplicated) before calling `SetSelectedTestIds`.
- [ ] T005 [US2] Verify `dotnet build` succeeds, then launch app and manually test: (1) run profile with dependency chain where A fails and B (depends on A) is skipped → re-run includes both A and B; (2) cancel a run midway → cancelled-skipped tests are NOT included in re-run.

**Checkpoint**: Both user stories are complete. Failed tests AND their dependency-skipped dependents are included in re-run. `dotnet build` succeeds.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (US1)**: No dependencies — can start immediately (no setup or foundational phase needed)
- **Phase 2 (US2)**: Depends on Phase 1 completion (extends the same command)

### User Story Dependencies

- **User Story 1 (P1)**: Independent — implements core re-run with failed-only collection
- **User Story 2 (P2)**: Extends US1 — adds dependency-skipped test collection to the existing command

### Within Each User Story

- T001 and T002 are parallelizable (ViewModel and XAML are different files)
- T003 depends on both T001 and T002 (build verification)
- T004 depends on T001 (extends the same command)
- T005 depends on T004 (build verification)

### Parallel Opportunities

- T001 and T002 can run in parallel (different files, no dependencies)

---

## Parallel Example: User Story 1

```bash
# Launch both US1 implementation tasks together:
Task: "Add HasFailedTests + RerunFailedTestsCommand to ResultsViewModel.cs"
Task: "Add Re-run Failed PrimaryButton to ResultsView.xaml"

# Then verify:
Task: "Build and manual test"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 + T002 in parallel → core re-run functionality
2. T003: Build and verify → **MVP complete**
3. **STOP and VALIDATE**: Button works for failed-only re-run

### Incremental Delivery

1. US1 (T001-T003) → Re-run failed tests works → Demo/validate
2. US2 (T004-T005) → Dependency-skipped tests also included → Complete feature

---

## Notes

- Only 2 source files modified: `ResultsViewModel.cs` and `ResultsView.xaml`
- No new files, packages, or abstractions introduced
- Reuses existing `IAppState.SetSelectedTestIds` + `NavigateToRunProgress` infrastructure
- `ErrorCategory.Dependency` is already set by `SequentialTestRunner` for dependency-skipped tests
- Commit after each phase checkpoint
