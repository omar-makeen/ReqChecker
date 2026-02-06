# Tasks: Premium Test Execution UX Polish

**Input**: Design documents from `/specs/028-premium-test-execution-ux/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not requested — manual visual testing only.

**Organization**: Tasks are grouped by user story. Each story modifies a different file, so all user stories can be implemented in parallel.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: User Story 1 - Correct Progress Ring Initial State (Priority: P1)

**Goal**: Fix the progress ring to show a clean empty track at 0% with no arc artifact visible.

**Independent Test**: Click "Run Tests" and observe the progress ring in the first moment — should show clean empty track with "0% Complete" centered, no partial arc visible.

### Implementation for User Story 1

- [ ] T001 [P] [US1] Fix `UpdateArc()` to use `Visibility.Collapsed` instead of `Visibility.Hidden` when Progress ≤ 0 in `src/ReqChecker.App/Controls/ProgressRing.xaml.cs`
- [ ] T002 [P] [US1] Fix `UpdateVisualState()` to check Progress > 0 before setting `_progressArc.Visibility = Visible` — guard against overriding the Collapsed state from `UpdateArc()` in `src/ReqChecker.App/Controls/ProgressRing.xaml.cs`

**Checkpoint**: Progress ring at 0% shows only the background track with no arc segment visible (SC-001).

---

## Phase 2: User Story 2 - Preparing State Before Test Execution (Priority: P1)

**Goal**: Show "Preparing..." in the status card immediately when test execution begins, before the first test starts running.

**Independent Test**: Click "Run Tests" and observe the status card — should immediately show "Preparing..." with spinner, then transition to first test name when execution begins.

### Implementation for User Story 2

- [X] T003 [P] [US2] Set `CurrentTestName = "Preparing..."` in `StartTestsAsync()` after resetting counters and before calling `RunTestsAsync()` in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`

**Checkpoint**: Users see "Preparing..." status within 100ms of clicking "Run Tests" (SC-002). Cancellation during preparing phase works cleanly (FR-007 — already supported by existing cancel logic).

---

## Phase 3: User Story 3 - Stable Progress Ring Layout on Completion (Priority: P1)

**Goal**: Eliminate the vertical layout shift when the page transitions from running to completed state.

**Independent Test**: Run a test suite and watch the progress ring position — the ring's vertical center must not move when transitioning to completed state.

### Implementation for User Story 3

- [X] T004 [P] [US3] Replace the left-column `StackPanel` (line 156) with a `Grid` using fixed row definitions: Row 0 (Auto) for progress ring, Row 1 (fixed height) for card area, Row 2 (Auto) for stats summary in `src/ReqChecker.App/Views/RunProgressView.xaml`
- [X] T005 [US3] Place both "Currently Running" card and "Completion Summary" card in the same Grid row (Row 1) so they overlap in the same fixed-height slot — card visibility bindings remain unchanged in `src/ReqChecker.App/Views/RunProgressView.xaml`

**Checkpoint**: Progress ring vertical position remains identical (zero pixel shift) between running and completed states (SC-003).

---

## Phase 4: User Story 4 - Premium Visual Polish and Consistency (Priority: P2)

**Goal**: Normalize card visual weight and ensure all elements feel cohesive across state transitions.

**Independent Test**: Run test suite end-to-end and evaluate visual consistency across initial → running → completed transitions. No flicker, jumps, or inconsistencies.

### Implementation for User Story 4

- [ ] T006 [US4] Normalize completion summary card: reduce CheckmarkCircle icon from FontSize="48" to FontSize="24", change heading from TextH3 to TextBody style, ensure card height matches the "Currently Running" card in `src/ReqChecker.App/Views/RunProgressView.xaml`
- [ ] T007 [US4] Review and verify consistent margins, padding, and spacing between both cards and the stats summary grid in `src/ReqChecker.App/Views/RunProgressView.xaml`

**Checkpoint**: End-to-end test execution flow feels cohesive with no visual artifacts (SC-004, SC-005).

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all stories combined

- [X] T008 Build the project and verify no compilation errors
- [X] T009 Run quickstart.md end-to-end validation: load profile, run 4 tests, verify all 4 fixes visually

---

## Dependencies & Execution Order

### Phase Dependencies

- **US1 (Phase 1)**: No dependencies — modifies `ProgressRing.xaml.cs` only
- **US2 (Phase 2)**: No dependencies — modifies `RunProgressViewModel.cs` only
- **US3 (Phase 3)**: No dependencies — modifies `RunProgressView.xaml` only
- **US4 (Phase 4)**: Depends on US3 completion (same file: `RunProgressView.xaml`)
- **Polish (Phase 5)**: Depends on all user stories being complete

### Parallel Opportunities

US1, US2, and US3 each modify **different files** and can all be implemented in parallel:

```text
Parallel Group 1 (all independent, different files):
  T001 + T002  →  ProgressRing.xaml.cs        (US1)
  T003         →  RunProgressViewModel.cs      (US2)
  T004 + T005  →  RunProgressView.xaml         (US3)

Sequential (same file as US3):
  T006 + T007  →  RunProgressView.xaml         (US4, after US3)

Final:
  T008 + T009  →  Build + validate             (after all)
```

---

## Implementation Strategy

### MVP First (User Stories 1-3)

1. Implement US1 + US2 + US3 in parallel (3 different files)
2. **STOP and VALIDATE**: Test all 3 fixes independently
3. These 3 stories address the core bugs the user reported

### Full Delivery

1. Complete US1 + US2 + US3 in parallel → Bug fixes done
2. Complete US4 → Visual polish applied on top
3. Build + validate end-to-end → Ship

---

## Notes

- All 3 P1 stories modify different files and have zero dependencies on each other
- US4 (P2) must wait for US3 since both modify RunProgressView.xaml
- Total: 9 tasks across 3 files — no new files created
- No tests requested — validation is manual visual inspection per quickstart.md
