# Tasks: Navigation Selection State Synchronization

**Input**: Design documents from `/specs/009-nav-selection-sync/`
**Prerequisites**: plan.md, spec.md, research.md

**Tests**: Manual UI testing only (per plan.md). No automated test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single WPF project**: `src/ReqChecker.App/`
- Views in `src/ReqChecker.App/Views/`
- ViewModels in `src/ReqChecker.App/ViewModels/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify current state and prepare for modifications

- [x] T001 Read current MainWindow.xaml.cs to understand existing navigation logic in src/ReqChecker.App/MainWindow.xaml.cs
- [x] T002 [P] Read current MainWindow.xaml to identify IsActive settings in src/ReqChecker.App/MainWindow.xaml
- [x] T003 [P] Read current ResultsView.xaml to understand empty state bindings in src/ReqChecker.App/Views/ResultsView.xaml
- [x] T004 [P] Read current ResultsViewModel.cs to check Report property implementation in src/ReqChecker.App/ViewModels/ResultsViewModel.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add centralized navigation selection management infrastructure

**⚠️ CRITICAL**: User story implementations depend on this infrastructure

- [x] T005 Add ClearNavigationSelection() helper method to MainWindow.xaml.cs that sets IsActive=false on NavProfiles, NavTests, NavResults, NavDiagnostics
- [x] T006 Add SetNavigationSelection(string tag) helper method to MainWindow.xaml.cs that calls ClearNavigationSelection() then sets IsActive=true for matching nav item

**Checkpoint**: Selection helpers ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Empty Results State Display (Priority: P1)

**Goal**: Display "No test results available" message when Results view has no data

**Independent Test**: Launch app without running tests, click Results in sidebar, verify empty state message is visible (not blank screen)

### Implementation for User Story 1

- [x] T007 [US1] Investigate NullToVisibilityConverter behavior in src/ReqChecker.App/Views/ResultsView.xaml - check if empty state visibility binding works correctly
- [x] T008 [US1] Add HasReport computed property (returns Report != null) to ResultsViewModel in src/ReqChecker.App/ViewModels/ResultsViewModel.cs
- [x] T009 [US1] Add OnPropertyChanged("HasReport") call in Report setter in src/ReqChecker.App/ViewModels/ResultsViewModel.cs
- [x] T010 [US1] Update ResultsView.xaml empty state visibility to use HasReport property with inverted BoolToVisibilityConverter in src/ReqChecker.App/Views/ResultsView.xaml
- [x] T011 [US1] Ensure main results content area hides when HasReport is false in src/ReqChecker.App/Views/ResultsView.xaml

**Checkpoint**: User Story 1 complete - empty state displays correctly when no results exist

---

## Phase 4: User Story 2 - Single Navigation Item Selection (Priority: P1)

**Goal**: Only one navigation item appears selected at any time

**Independent Test**: Click each sidebar item (Profiles, Tests, Results, Diagnostics) and verify only the clicked item is highlighted

### Implementation for User Story 2

- [x] T012 [US2] Remove hardcoded IsActive="True" from NavTests in src/ReqChecker.App/MainWindow.xaml
- [x] T013 [US2] Update NavItem_Click handler to call SetNavigationSelection(tag) at start (before navigation) in src/ReqChecker.App/MainWindow.xaml.cs
- [x] T014 [US2] Update OnWindowLoaded to call SetNavigationSelection("Tests") instead of direct NavTests.IsActive=true in src/ReqChecker.App/MainWindow.xaml.cs
- [x] T015 [US2] Update OnWindowLoaded to call SetNavigationSelection("Profiles") instead of direct NavProfiles.IsActive=true in src/ReqChecker.App/MainWindow.xaml.cs
- [x] T016 [US2] Remove inline NavResults.IsActive=true from NavigateWithAnimation Results case (now handled by SetNavigationSelection) in src/ReqChecker.App/MainWindow.xaml.cs

**Checkpoint**: User Story 2 complete - exactly one nav item selected at all times when clicking sidebar

---

## Phase 5: User Story 3 - Programmatic Navigation Selection Sync (Priority: P1)

**Goal**: Sidebar selection updates when navigation occurs via buttons (View Results, Back to Tests)

**Independent Test**: Run tests, click "View Results" button, verify sidebar shows Results selected (not Tests)

### Implementation for User Story 3

- [x] T017 [US3] Verify SetNavigationSelection is called in NavigateWithAnimation for all cases (Profiles, Tests, Results, Diagnostics) in src/ReqChecker.App/MainWindow.xaml.cs
- [x] T018 [US3] Add NavigationService event or callback mechanism for sidebar sync when ViewModel triggers navigation (optional - evaluate if T017 covers all paths) - T017 covers all paths
- [ ] T019 [US3] Test "Back to Tests" button from ResultsView updates sidebar to Tests selection

**Checkpoint**: User Story 3 complete - sidebar always reflects current view regardless of navigation source

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup

- [x] T020 Build project to verify no compilation errors: dotnet build src/ReqChecker.App
- [ ] T021 Manual test: Launch app, click Results without running tests, verify empty state message
- [ ] T022 Manual test: Click through all nav items (Profiles, Tests, Results, Diagnostics), verify only one selected
- [ ] T023 Manual test: Run tests, click "View Results" button, verify Results selected in sidebar
- [ ] T024 Manual test: Click "Back to Tests" button, verify Tests selected in sidebar
- [x] T025 Code cleanup: Remove any commented-out or dead code related to old IsActive handling

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - read current code state
- **Foundational (Phase 2)**: Depends on Setup - adds helper methods
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User stories can then proceed in priority order (P1 → P1 → P1)
  - Note: All three stories are P1 but address different bugs
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (Empty State)**: Depends on Phase 2 - Independent of US2/US3
- **User Story 2 (Single Selection)**: Depends on Phase 2 - Uses helper methods from Foundational
- **User Story 3 (Programmatic Sync)**: Depends on Phase 2 AND Phase 4 - Builds on US2's SetNavigationSelection usage

### Within Each User Story

- Investigation/read tasks first
- ViewModel changes before View changes
- Core implementation before integration

### Parallel Opportunities

- T002, T003, T004 can run in parallel (reading different files)
- T008, T009 can run in parallel with T012 (different files: ViewModel vs XAML)
- US1 (ResultsView changes) can run in parallel with US2 (MainWindow changes)

---

## Parallel Example: Setup Phase

```bash
# Launch all read tasks together:
Task: "T002 [P] Read MainWindow.xaml"
Task: "T003 [P] Read ResultsView.xaml"
Task: "T004 [P] Read ResultsViewModel.cs"
```

## Parallel Example: User Story 1 + User Story 2

```bash
# Since US1 modifies ResultsView/ResultsViewModel and US2 modifies MainWindow:
# Developer A: T007, T008, T009, T010, T011 (US1 - ResultsView)
# Developer B: T012, T013, T014, T015, T016 (US2 - MainWindow)
```

---

## Implementation Strategy

### MVP First (All Stories Are P1)

1. Complete Phase 1: Setup (read code)
2. Complete Phase 2: Foundational (add helpers)
3. Complete Phase 3: User Story 1 (empty state)
4. Complete Phase 4: User Story 2 (single selection)
5. Complete Phase 5: User Story 3 (programmatic sync)
6. Complete Phase 6: Polish (verify all fixes)

### Recommended Order

Since all stories are P1 (critical bugs), the recommended order is:

1. **US2 first** - Fixes the most visible bug (multiple selections)
2. **US3 second** - Builds on US2's work (uses same helper methods)
3. **US1 last** - Independent fix, different files

This allows US2+US3 to share work on MainWindow.xaml.cs while US1 is independent.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All three user stories are P1 priority (critical bugs)
- Manual testing only (per plan.md specification)
- Commit after each task or logical group
- Total tasks: 25
