# Tasks: UX Quick Wins

**Input**: Design documents from `/specs/063-ux-quick-wins/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Tests**: Not explicitly requested in spec. Test tasks included for ViewModel property additions only (P1, P2) since they are easily unit-testable.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. No setup or foundational phase needed — all changes modify existing files in an existing project.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: User Story 1 - Profile Name in Test Execution Header (Priority: P1) 🎯 MVP

**Goal**: Display the current profile name in the RunProgress header so users know which profile is being executed.

**Independent Test**: Load a profile, run tests, verify profile name appears in header beneath "Test Execution" title. Verify long names truncate with ellipsis and show full name via tooltip.

### Implementation for User Story 1

- [X] T001 [P] [US1] Add `ProfileName` computed property to `RunProgressViewModel` that returns `CurrentProfile?.Name ?? string.Empty` in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`
- [X] T002 [P] [US1] Add unit test for `ProfileName` property (returns profile name when profile loaded, empty when null) in `tests/ReqChecker.App.Tests/ViewModels/RunProgressViewModelTests.cs`
- [X] T003 [US1] Add profile name TextBlock to RunProgress header StackPanel between title and subtitle — use `TextTrimming="CharacterEllipsis"`, `MaxWidth` constrained, with `ToolTip` bound to full name, hidden when empty via `Visibility` binding in `src/ReqChecker.App/Views/RunProgressView.xaml`

**Checkpoint**: Profile name visible in RunProgress header. Truncates with tooltip for long names. Hidden when no profile loaded.

---

## Phase 2: User Story 2 - Test Count Badge on Sidebar Navigation (Priority: P2)

**Goal**: Show a small numeric badge on the "Test Suite" sidebar nav item indicating the number of loaded tests.

**Independent Test**: Load a profile, verify sidebar "Test Suite" item shows badge with test count. Unload profile, verify badge disappears. Check in both expanded and compact sidebar modes.

### Implementation for User Story 2

- [X] T004 [P] [US2] Add `TestCount` (int) and `HasTests` (bool) observable properties to `MainViewModel`, synced from `IAppState.CurrentProfileChanged` event, computing from `CurrentProfile?.Tests.Count ?? 0` in `src/ReqChecker.App/ViewModels/MainViewModel.cs`
- [X] T005 [P] [US2] Add unit tests for `TestCount` and `HasTests` properties (returns count when profile loaded, 0 when null, updates on profile change) in `tests/ReqChecker.App.Tests/ViewModels/MainViewModelTests.cs`
- [X] T006 [US2] Add custom badge overlay to NavTests NavigationViewItem — small rounded Border with TextBlock bound to `TestCount`, visibility bound to `HasTests`, positioned as overlay in a Grid wrapping the nav item content, styled for both expanded and compact sidebar modes in `src/ReqChecker.App/Views/MainWindow.xaml`

**Checkpoint**: Badge shows test count on sidebar. Disappears when no profile. Works in expanded and compact modes.

---

## Phase 3: User Story 3 - Export Keyboard Shortcut (Priority: P3)

**Goal**: Allow Ctrl+E to toggle the export dropdown on the Results page.

**Independent Test**: Navigate to Results page with completed test run, press Ctrl+E, verify dropdown opens. Press Ctrl+E again, verify it closes. Navigate to another page, press Ctrl+E, verify nothing happens.

### Implementation for User Story 3

- [ ] T007 [US3] Add `KeyBinding` with `Key="E" Modifiers="Ctrl"` bound to `ToggleExportMenuCommand` in the `InputBindings` section of `src/ReqChecker.App/Views/ResultsView.xaml`

**Checkpoint**: Ctrl+E toggles export dropdown on Results page. Ignored on other pages and when export in progress.

---

## Phase 4: User Story 4 - Filter Tab Transition Animation (Priority: P4)

**Goal**: Add a subtle fade transition when switching between result filter tabs (All/Passed/Failed/Skipped).

**Independent Test**: Run tests with mixed results, switch between filter tabs, verify brief fade-out/fade-in transition (~200ms total). Rapidly click tabs, verify no visual glitches. Switch to empty filter, verify empty state fades in.

### Implementation for User Story 4

- [ ] T008 [P] [US4] Add `x:Name` to the results ListBox (or its container) and define fade-out/fade-in Storyboard resources (opacity 1→0 in 100ms QuadraticEase EaseIn, opacity 0→1 in 100ms QuadraticEase EaseOut) in `src/ReqChecker.App/Views/ResultsView.xaml`
- [ ] T009 [US4] Add filter transition logic in code-behind: subscribe to ViewModel `ActiveFilter` property changes, trigger fade-out storyboard, apply filter refresh on completion callback, then trigger fade-in storyboard. Handle rapid switching by stopping any in-progress animation and snapping to final state before starting new transition in `src/ReqChecker.App/Views/ResultsView.xaml.cs`

**Checkpoint**: Filter tab switches play smooth fade transition. Rapid switching has no glitches. Empty state fades in correctly.

---

## Phase 5: User Story 5 - Complete Tooltip Coverage (Priority: P5)

**Goal**: Ensure every interactive button and control in the application has a descriptive tooltip with consistent 400ms delay.

**Independent Test**: Hover over every button/control across all views. Verify tooltip appears within 400ms. Verify disabled controls also show tooltips.

### Implementation for User Story 5

- [ ] T010 [P] [US5] Audit all XAML views in `src/ReqChecker.App/Views/` for interactive buttons and controls missing tooltips — document gaps
- [ ] T011 [US5] Add missing tooltips to all identified controls using pattern: `ToolTipService.InitialShowDelay="400"`, `ToolTipService.ShowOnDisabled="True"`, `ToolTip` with `ModernToolTip` style, across all view files in `src/ReqChecker.App/Views/` and `src/ReqChecker.App/Controls/`

**Checkpoint**: 100% tooltip coverage on interactive controls. All use 400ms delay and ShowOnDisabled.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup

- [ ] T012 Run all existing unit tests to verify no regressions: `dotnet test tests/ReqChecker.App.Tests/`
- [ ] T013 Verify all 5 features work together in a full manual walkthrough: load profile → check badge → run tests → check header → switch filters → Ctrl+E export → hover tooltips

---

## Dependencies & Execution Order

### Phase Dependencies

- **User Stories (Phases 1-5)**: No shared setup needed — all are fully independent
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: No dependencies on other stories — can start immediately
- **US2 (P2)**: No dependencies on other stories — can start immediately
- **US3 (P3)**: No dependencies on other stories — can start immediately
- **US4 (P4)**: No dependencies on other stories — can start immediately
- **US5 (P5)**: No dependencies — benefits from running last to catch any tooltips added by other stories

### Within Each User Story

- ViewModel changes before XAML bindings (T001 before T003, T004 before T006)
- Tests can run in parallel with ViewModel changes [P]
- Code-behind after XAML structure (T008 before T009)

### Parallel Opportunities

- **All 5 user stories** can run in parallel since they touch different files:
  - US1: RunProgressViewModel + RunProgressView
  - US2: MainViewModel + MainWindow
  - US3: ResultsView (InputBindings only)
  - US4: ResultsView (ListBox + code-behind)
  - US5: Multiple view files (tooltip additions)
- **Note**: US3 and US4 both modify ResultsView.xaml — run US3 first (1 line change) then US4

---

## Parallel Example: All Stories

```bash
# These can all start simultaneously (different files):
Task T001: "Add ProfileName property in RunProgressViewModel.cs"
Task T004: "Add TestCount/HasTests in MainViewModel.cs"

# After T001 completes:
Task T003: "Add profile name TextBlock in RunProgressView.xaml"

# After T004 completes:
Task T006: "Add badge overlay in MainWindow.xaml"

# Independent (ResultsView.xaml — run sequentially):
Task T007: "Add Ctrl+E InputBinding in ResultsView.xaml"
Task T008: "Add fade Storyboard resources in ResultsView.xaml"
Task T009: "Add filter transition logic in ResultsView.xaml.cs"

# Last (after all other views modified):
Task T010: "Audit tooltip gaps"
Task T011: "Add missing tooltips"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: US1 (Profile name in header)
2. **STOP and VALIDATE**: Test independently — load profile, run tests, verify header
3. Proceed to remaining stories

### Incremental Delivery

1. US1 → Profile name visible → Validate
2. US2 → Test count badge → Validate
3. US3 → Ctrl+E shortcut → Validate
4. US4 → Filter animations → Validate
5. US5 → Tooltip completeness → Validate
6. Polish → Full regression test

---

## Notes

- All 5 stories are independent — any can be skipped without affecting others
- US3 is the smallest (single InputBinding addition)
- US5 scope depends on audit results — may be zero changes if all controls already have tooltips
- US3 and US4 share ResultsView.xaml — coordinate edits to avoid merge conflicts
- Commit after each user story for clean git history
