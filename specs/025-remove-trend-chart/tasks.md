# Tasks: Remove Pass Rate Trend Chart

**Input**: Design documents from `/specs/025-remove-trend-chart/`
**Prerequisites**: plan.md, spec.md, quickstart.md

**Tests**: Not requested - manual visual testing only.

**Organization**: Tasks organized by user story for independent implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

- **Project Type**: Desktop WPF application
- **Paths**: `src/ReqChecker.App/`

---

## Phase 1: Setup

**Purpose**: No setup needed - this is a removal task

*No tasks - existing project structure is sufficient*

---

## Phase 2: Foundational (Delete Unused Control)

**Purpose**: Remove the LineChart control files that are no longer needed

- [x] T001 [P] Delete LineChart XAML file `src/ReqChecker.App/Controls/LineChart.xaml`
- [x] T002 [P] Delete LineChart code-behind file `src/ReqChecker.App/Controls/LineChart.xaml.cs`

**Checkpoint**: LineChart control files deleted

---

## Phase 3: User Story 1 - Cleaner History Page (Priority: P1)

**Goal**: User sees history list without trend chart clutter

**Independent Test**: Navigate to Test History page with history data and verify no chart is visible

### Implementation for User Story 1

- [x] T003 [US1] Remove controls namespace declaration from `src/ReqChecker.App/Views/HistoryView.xaml` (line 7: `xmlns:controls`)
- [x] T004 [US1] Remove one row from Grid.RowDefinitions in `src/ReqChecker.App/Views/HistoryView.xaml` (reduce from 5 to 4 rows)
- [x] T005 [US1] Delete Trend Chart Border element (Grid.Row="2") from `src/ReqChecker.App/Views/HistoryView.xaml`
- [x] T006 [US1] Update Filter Tabs from Grid.Row="3" to Grid.Row="2" in `src/ReqChecker.App/Views/HistoryView.xaml`
- [x] T007 [US1] Update History ListBox from Grid.Row="4" to Grid.Row="3" in `src/ReqChecker.App/Views/HistoryView.xaml`
- [x] T008 [US1] Update Empty State Grid from Grid.Row="4" to Grid.Row="3" in `src/ReqChecker.App/Views/HistoryView.xaml`

**Checkpoint**: User Story 1 complete - History page displays without trend chart

---

## Phase 4: User Story 2 - Faster Page Load (Priority: P2)

**Goal**: Remove unused ViewModel code for improved performance

**Independent Test**: Navigate to Test History and verify no chart-related processing occurs

### Implementation for User Story 2

- [x] T009 [US2] Remove `using ReqChecker.App.Controls;` import from `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`
- [x] T010 [US2] Remove TrendDataPoints property from `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`
- [x] T011 [US2] Remove FlakyTests property from `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`
- [x] T012 [US2] Remove UpdateTrendDataPoints() call from OnActiveFilterChanged in `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`
- [x] T013 [US2] Remove UpdateTrendDataPoints() call from OnHistoryRunsChanged in `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`
- [x] T014 [US2] Delete UpdateTrendDataPoints() method from `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`
- [x] T015 [US2] Delete ComputeFlakyTests() method from `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`

**Checkpoint**: User Story 2 complete - No chart-related code executed

---

## Phase 5: Polish & Verification

**Purpose**: Final validation and cleanup

- [x] T016 Build and verify no compilation errors with `dotnet build src/ReqChecker.App`
- [ ] T017 Manual verification: Navigate to History page with data, verify list displays correctly
- [ ] T018 Manual verification: Verify filter tabs still work
- [ ] T019 Manual verification: Verify empty state displays when no history

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies - can start immediately
- **User Story 1 (Phase 3)**: Can start in parallel with Phase 2 (different files)
- **User Story 2 (Phase 4)**: Can start in parallel with Phase 3 (different files)
- **Polish (Phase 5)**: Depends on all previous phases

### Task Dependencies

```
T001, T002 (Delete controls) ─┐
                               │
T003-T008 (XAML changes) ─────┼──> T016 (Build) ──> T017-T019 (Verify)
                               │
T009-T015 (ViewModel) ────────┘
```

### Parallel Opportunities

**Maximum parallelism possible:**
- T001 + T002 can run in parallel (different files in same folder)
- T003-T008 can run sequentially (same file)
- T009-T015 can run sequentially (same file)
- BUT T003-T008 and T009-T015 can run in parallel (different files)

---

## Parallel Example

```bash
# These task groups can run in parallel (different files):

# Group A: Delete control files
Task: T001 - Delete LineChart.xaml
Task: T002 - Delete LineChart.xaml.cs

# Group B: Update HistoryView.xaml (run sequentially within group)
Task: T003-T008

# Group C: Update HistoryViewModel.cs (run sequentially within group)
Task: T009-T015

# Groups A, B, C can all run in parallel
```

---

## Implementation Strategy

### Quick Path (All at Once)

Since this is a removal task with clear file boundaries:
1. Delete T001, T002 (control files)
2. Edit T003-T008 (XAML - one file, sequential edits)
3. Edit T009-T015 (ViewModel - one file, sequential edits)
4. Build and verify T016-T019

All can be done in one session.

---

## Summary

| Metric | Count |
|--------|-------|
| Total Tasks | 19 |
| US1 Tasks | 6 |
| US2 Tasks | 7 |
| Foundational Tasks | 2 |
| Polish Tasks | 4 |
| Parallel Opportunities | 3 groups (controls, XAML, ViewModel) |
| Files Deleted | 2 |
| Files Modified | 2 |

**Suggested MVP Scope**: User Story 1 only (tasks T001-T008, T016-T017) removes the chart from UI

**Expected Code Reduction**: ~370 lines
