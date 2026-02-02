# Tasks: Test History

**Input**: Design documents from `/specs/023-test-history/`
**Prerequisites**: plan.md, spec.md, data-model.md, research.md, quickstart.md, contracts/

**Tests**: Manual testing (per existing project pattern - no automated test tasks)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Paths follow existing project structure: `src/ReqChecker.{App|Core|Infrastructure}/`

---

## Phase 1: Setup

**Purpose**: Create foundational models and service interfaces

- [x] T001 [P] Create HistoryStore model in src/ReqChecker.Core/Models/HistoryStore.cs
- [x] T002 [P] Create TestTrendData model in src/ReqChecker.Core/Models/TestTrendData.cs
- [x] T003 [P] Create HistoryStats record in src/ReqChecker.Core/Models/HistoryStats.cs
- [x] T004 Create IHistoryService interface in src/ReqChecker.Infrastructure/History/IHistoryService.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement core history service that ALL user stories depend on

**⚠️ CRITICAL**: No UI work can begin until this phase is complete

- [x] T005 Implement HistoryService with JSON persistence in src/ReqChecker.Infrastructure/History/HistoryService.cs
- [x] T006 Add LoadHistoryAsync method (read from %APPDATA%/ReqChecker/history.json)
- [x] T007 Add SaveRunAsync method (append run, write to file atomically)
- [x] T008 Add DeleteRunAsync method (remove by runId, write to file)
- [x] T009 Add ClearHistoryAsync method (empty list, write to file)
- [x] T010 Add GetStats method (compute TotalRuns, FileSizeBytes, OldestRun, NewestRun)
- [x] T011 Add corruption detection and backup (detect invalid JSON, backup file, return empty)
- [x] T012 Register IHistoryService in DI container in src/ReqChecker.App/App.xaml.cs
- [x] T013 Hook history auto-save after test completion in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs

**Checkpoint**: History service ready - UI implementation can begin

---

## Phase 3: User Story 1 - View Test Run History (Priority: P1) 🎯 MVP

**Goal**: Users can view a list of past test runs with date, profile name, and pass rate

**Independent Test**: Run multiple test sessions, navigate to history view, verify all runs appear correctly

### Implementation for User Story 1

- [x] T014 [P] [US1] Create HistoryViewModel with ObservableCollection of runs in src/ReqChecker.App/ViewModels/HistoryViewModel.cs
- [x] T015 [P] [US1] Add LoadHistoryCommand that calls IHistoryService.LoadHistoryAsync
- [x] T016 [P] [US1] Add SelectedRun property for viewing run details
- [x] T017 [US1] Create HistoryView.xaml with premium AnimatedPageHeader in src/ReqChecker.App/Views/HistoryView.xaml
- [x] T018 [US1] Add gradient accent line and History24 icon container to HistoryView header
- [x] T019 [US1] Add virtualized ListBox for history runs using Card style in HistoryView.xaml
- [x] T020 [US1] Create AnimatedHistoryItem style with entrance animations (pattern from ResultsView)
- [x] T021 [US1] Display run summary: date, profile name, pass rate, duration for each item
- [x] T022 [US1] Add premium empty state (icon + "No test history" message + "Run Tests" button)
- [x] T023 [US1] Create HistoryView.xaml.cs code-behind in src/ReqChecker.App/Views/HistoryView.xaml.cs
- [x] T024 [US1] Add NavigationViewItem for History in src/ReqChecker.App/MainWindow.xaml (after Results, before Diagnostics)
- [x] T025 [US1] Update NavigationService with NavigateToHistory method in src/ReqChecker.App/Services/NavigationService.cs
- [x] T026 [US1] Add run detail expansion or navigation to show full test results
- [x] T027 [US1] Implement profile filter tabs using FilterTab style
- [x] T028 [US1] Ensure Dark and Light theme compatibility

**Checkpoint**: User Story 1 complete - users can view history list and run details

---

## Phase 4: User Story 2 - Compare Pass Rates Over Time (Priority: P2)

**Goal**: Users can see a line graph showing pass rate trends over time

**Independent Test**: Run tests multiple times with varying results, view trend visualization, verify accuracy

### Implementation for User Story 2

- [x] T029 [P] [US2] Create LineChart.xaml UserControl in src/ReqChecker.App/Controls/LineChart.xaml
- [x] T030 [P] [US2] Create LineChart.xaml.cs with DependencyProperties in src/ReqChecker.App/Controls/LineChart.xaml.cs
- [x] T031 [US2] Add DataPoints property (collection of DateTime, double pairs)
- [x] T032 [US2] Add MinY/MaxY properties (default 0-100 for percentages)
- [x] T033 [US2] Add PointColor and LineColor brush properties
- [x] T034 [US2] Implement chart rendering using Path/Polyline geometry (follow DonutChart pattern)
- [x] T035 [US2] Add X-axis labels (dates or run indices)
- [x] T036 [US2] Add Y-axis labels (percentage values)
- [x] T037 [US2] Style LineChart with accent colors and premium design
- [x] T038 [US2] Add data point tooltips showing run details on hover
- [x] T039 [US2] Add LineChart to HistoryView.xaml inside a Card
- [x] T040 [US2] Add TrendDataPoints computed property to HistoryViewModel
- [x] T041 [US2] Update chart when profile filter changes

**Checkpoint**: User Story 2 complete - users can visualize pass rate trends

---

## Phase 5: User Story 3 - Identify Flaky Tests (Priority: P3)

**Goal**: Users can identify tests with inconsistent pass/fail results

**Independent Test**: Run same suite multiple times with varying test outcomes, verify flaky tests are flagged

### Implementation for User Story 3

- [x] T042 [P] [US3] Add GetFlakyTests method to HistoryViewModel
- [x] T043 [US3] Implement flaky detection algorithm (pass AND fail in last 10 runs of same profile)
- [x] T044 [US3] Add FlakyTests observable collection to HistoryViewModel
- [x] T045 [US3] Add flaky test indicator badge/icon to history run items
- [x] T046 [US3] Create FlakyTestsCard section in HistoryView.xaml
- [x] T047 [US3] Display flaky test list with pass/fail ratio (e.g., "7/10 passed")
- [x] T048 [US3] Style flaky indicator with StatusFail/StatusPass colors
- [x] T049 [US3] Add tooltip showing recent pass/fail history for each flaky test

**Checkpoint**: User Story 3 complete - users can identify flaky tests

---

## Phase 6: User Story 4 - Manage History Storage (Priority: P4)

**Goal**: Users can delete individual runs and clear all history

**Independent Test**: Accumulate history, delete individual runs, clear all, verify storage reclaimed

### Implementation for User Story 4

- [x] T050 [P] [US4] Add DeleteRunCommand to HistoryViewModel
- [x] T051 [P] [US4] Add ClearAllCommand to HistoryViewModel
- [x] T052 [US4] Add delete button (SecondaryButton with Dismiss24 icon) to each history item
- [x] T053 [US4] Add "Clear All" button (SecondaryButton) to HistoryView header
- [x] T054 [US4] Implement confirmation dialog for Clear All action
- [x] T055 [US4] Add storage stats display (total runs, approximate file size)
- [x] T056 [US4] Update UI after delete/clear operations
- [x] T057 [US4] Handle errors with user-friendly messages

**Checkpoint**: User Story 4 complete - users can manage history storage

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, error handling, and documentation

- [x] T058 Verify all premium UI elements are consistent with ResultsView/DiagnosticsView
- [x] T059 Test Dark and Light themes thoroughly
- [x] T060 Verify history loads within 2 seconds for 100+ runs
- [x] T061 Test corrupted history file recovery
- [x] T062 Test large history (500+ runs) with virtualization
- [x] T063 Update CLAUDE.md if needed
- [x] T064 Run quickstart.md validation checklist

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational - MVP milestone
- **User Story 2 (Phase 4)**: Depends on Foundational (can parallel with US1)
- **User Story 3 (Phase 5)**: Depends on Foundational (can parallel with US1/US2)
- **User Story 4 (Phase 6)**: Depends on Foundational (can parallel with US1/US2/US3)
- **Polish (Phase 7)**: Depends on all user stories

### User Story Dependencies

| Story | Can Start After | Dependencies on Other Stories |
|-------|-----------------|------------------------------|
| US1 (View History) | Phase 2 complete | None - core MVP |
| US2 (Trend Graph) | Phase 2 complete | Independent (uses same data) |
| US3 (Flaky Tests) | Phase 2 complete | Independent (uses same data) |
| US4 (Manage Storage) | Phase 2 complete | Independent (CRUD operations) |

### Within Each User Story

- ViewModel logic before XAML views
- Core functionality before styling polish
- Premium UI styling is mandatory for all views

---

## Parallel Opportunities

### Phase 1 (All parallel)
```
T001: HistoryStore model
T002: TestTrendData model
T003: HistoryStats record
```

### After Phase 2 (User Stories can parallel)
```
Developer A: US1 (View History) - MVP
Developer B: US2 (LineChart) - can work simultaneously
Developer C: US3 (Flaky Tests) - can work simultaneously
Developer D: US4 (Delete/Clear) - can work simultaneously
```

### Within US1 (Parallel tasks)
```
T014: HistoryViewModel
T015: LoadHistoryCommand
T016: SelectedRun property
```

### Within US2 (Parallel tasks)
```
T029: LineChart.xaml
T030: LineChart.xaml.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T013) - CRITICAL
3. Complete Phase 3: User Story 1 (T014-T028)
4. **STOP and VALIDATE**: Test history viewing independently
5. Deploy/demo MVP

### Incremental Delivery

| Milestone | Stories Complete | Value Delivered |
|-----------|------------------|-----------------|
| MVP | US1 | View and browse test history |
| +Trends | US1 + US2 | Visualize pass rate over time |
| +Flaky | US1 + US2 + US3 | Identify unreliable tests |
| Complete | All | Full history management |

### Recommended Sequence (Solo Developer)

1. Phase 1 → Phase 2 → **US1** (MVP)
2. **US2** (LineChart adds significant value)
3. **US4** (Delete/Clear - maintenance need)
4. **US3** (Flaky detection - advanced analysis)
5. Phase 7 (Polish)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- Premium UI styling is MANDATORY for all views (FR-013)
- Follow DonutChart pattern for LineChart implementation
- Follow PreferencesService pattern for HistoryService
- Manual testing per existing project pattern
