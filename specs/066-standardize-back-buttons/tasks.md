# Tasks: Standardize Back Buttons

**Input**: Design documents from `/specs/066-standardize-back-buttons/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not requested — no test tasks included.

**Organization**: All three user stories (position, style, label) are applied together per view since they modify the same element. Tasks are organized per-view to enable clean, atomic commits. Each task delivers all three stories for one view.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to
- Include exact file paths in descriptions

---

## Phase 1: User Story 1+2+3 — TestConfigView (Priority: P1) 🎯 MVP

**Goal**: Update the back button that's already in Column 0 — restyle from GhostButton to SecondaryButton, update label to "Back to Tests", standardize tooltip.

**Independent Test**: Open any test config page → back button is on the left with SecondaryButton style, "Back to Tests" label, ArrowLeft icon, and "Return to the test suite" tooltip. Unsaved changes prompt still works.

### Implementation

- [X] T001 [P] [US1] [US2] [US3] Restyle back button in src/ReqChecker.App/Views/TestConfigView.xaml: change `Style="{StaticResource GhostButton}"` to `Style="{StaticResource SecondaryButton}"`, change `<TextBlock Text="Back"/>` to `<TextBlock Text="Back to Tests"/>`, change `ToolTip="Return to test list"` to `ToolTip="Return to the test suite"`. Keep `Command="{Binding BackCommand}"` and `x:Name="BackButton"` unchanged (preserves unsaved changes prompt and FocusManager).

**Checkpoint**: TestConfigView back button matches target style, label, and tooltip. Unsaved changes prompt still works.

---

## Phase 2: User Story 1+2+3 — HistoryView (Priority: P1)

**Goal**: Move back button from Column 2 (right action area) to Column 0 (left, before title). Restructure header grid from 3 to 4 columns.

**Independent Test**: Open History page → back button is on the left before the title, "Clear All" is on the right. Same SecondaryButton style, "Back to Tests" label, and tooltip.

### Implementation

- [X] T002 [P] [US1] [US2] [US3] Restructure header grid and move back button in src/ReqChecker.App/Views/HistoryView.xaml: (1) Add 4th ColumnDefinition to header Grid making it `Auto | Auto | * | Auto`, (2) Add standard back button element in Grid.Column="0" with `Style="{StaticResource SecondaryButton}"`, `Command="{Binding NavigateToTestListCommand}"`, `Margin="0,0,16,0"`, `VerticalAlignment="Center"`, `ToolTip="Return to the test suite"`, `ToolTipService.InitialShowDelay="400"`, `ToolTipService.ShowOnDisabled="True"`, containing StackPanel with ArrowLeft24 icon and "Back to Tests" text, (3) Shift icon container from Grid.Column="0" to Grid.Column="1", (4) Shift title StackPanel from Grid.Column="1" to Grid.Column="2", (5) Shift action buttons from Grid.Column="2" to Grid.Column="3", (6) Remove back button from the action buttons StackPanel in Column 3 ("Clear All" stays), (7) Keep `x:Name="BackToTestsButton"` on the new Column 0 button for FocusManager.

**Checkpoint**: HistoryView back button is on the left, "Clear All" on the right. Style, label, and tooltip match TestConfigView.

---

## Phase 3: User Story 1+2+3 — ResultsView (Priority: P1)

**Goal**: Move back button from Column 2 (right action area) to Column 0 (left, before title). Add missing ToolTipService.ShowOnDisabled.

**Independent Test**: Open Results page → back button is on the left before the title, "Re-run Failed" and "Export" are on the right. Same style and label. Empty-state "Go to Tests" button is unchanged.

### Implementation

- [X] T003 [P] [US1] [US2] [US3] Restructure header grid and move back button in src/ReqChecker.App/Views/ResultsView.xaml: (1) Add 4th ColumnDefinition to header Grid making it `Auto | Auto | * | Auto`, (2) Add standard back button element in Grid.Column="0" with `Style="{StaticResource SecondaryButton}"`, `Command="{Binding NavigateToTestListCommand}"`, `Margin="0,0,16,0"`, `VerticalAlignment="Center"`, `ToolTip="Return to the test suite"`, `ToolTipService.InitialShowDelay="400"`, `ToolTipService.ShowOnDisabled="True"`, containing StackPanel with ArrowLeft24 icon and "Back to Tests" text, (3) Shift icon container from Grid.Column="0" to Grid.Column="1", (4) Shift title StackPanel from Grid.Column="1" to Grid.Column="2", (5) Shift action buttons from Grid.Column="2" to Grid.Column="3", (6) Remove back button from the action buttons StackPanel in Column 3 ("Re-run Failed" and "Export" stay), (7) Keep `x:Name="BackToTestsButton"` on the new Column 0 button for FocusManager, (8) Do NOT modify the empty-state "Go to Tests" button (FR-007).

**Checkpoint**: ResultsView back button is on the left. Action buttons on the right. Empty-state button unchanged.

---

## Phase 4: User Story 1+2+3 — RunProgressView (Priority: P1)

**Goal**: Move back button from Column 2 (completion area) to Column 0 (left, before title). Restyle from PrimaryButton to SecondaryButton. Preserve conditional visibility.

**Independent Test**: Run tests → during execution, no back button visible (Cancel button on right). After completion, back button appears on the left with SecondaryButton style, "View Results" stays on the right.

### Implementation

- [X] T004 [P] [US1] [US2] [US3] Restructure header grid and move back button in src/ReqChecker.App/Views/RunProgressView.xaml: (1) Add 4th ColumnDefinition to header Grid making it `Auto | Auto | * | Auto`, (2) Add standard back button element in Grid.Column="0" with `Style="{StaticResource SecondaryButton}"`, `Command="{Binding NavigateToTestListCommand}"`, `Margin="0,0,16,0"`, `VerticalAlignment="Center"`, `ToolTip="Return to the test suite"`, `ToolTipService.InitialShowDelay="400"`, `ToolTipService.ShowOnDisabled="True"`, and `Visibility="{Binding IsComplete, Converter={StaticResource BoolToVisibilityConverter}}"` (preserve FR-006), containing StackPanel with ArrowLeft24 icon and "Back to Tests" text, (3) Shift icon container from Grid.Column="0" to Grid.Column="1", (4) Shift title StackPanel from Grid.Column="1" to Grid.Column="2", (5) Shift action/cancel buttons from Grid.Column="2" to Grid.Column="3", (6) Remove back button from the completion StackPanel in Column 3 ("View Results" stays), (7) Cancel button (conditional on IsRunning) stays in Column 3 unmodified, (8) FocusManager stays on CancelButton.

**Checkpoint**: RunProgressView back button appears only after completion, on the left, with SecondaryButton style. Cancel button and View Results unaffected.

---

## Dependencies & Execution Order

### Phase Dependencies

- **All phases**: Independent — each modifies a different file
- **All 4 tasks are marked [P]**: They can all run in parallel

### User Story Dependencies

- **US1 (Position)**, **US2 (Style)**, **US3 (Label)**: All applied together per task since they modify the same element in each file. No cross-story dependencies.

### Parallel Opportunities

- All 4 tasks modify different XAML files with no shared dependencies — all can execute in parallel.

---

## Parallel Example

```bash
# All 4 tasks can run simultaneously (different files):
Task T001: TestConfigView.xaml — restyle + relabel (position already correct)
Task T002: HistoryView.xaml — grid restructure + move + restyle
Task T003: ResultsView.xaml — grid restructure + move + add ShowOnDisabled
Task T004: RunProgressView.xaml — grid restructure + move + restyle + preserve visibility
```

---

## Implementation Strategy

### MVP First (TestConfigView Only)

1. Complete T001: Simplest change — style + label + tooltip only
2. **STOP and VALIDATE**: Verify back button looks correct, unsaved changes still works
3. Proceed to T002-T004 for remaining views

### Full Delivery (Recommended — Small Scope)

1. Complete T001-T004 in parallel (all are independent)
2. **VALIDATE**: Check all 4 views match visually
3. Verify preserved behaviors (unsaved changes, conditional visibility, empty state)
4. Single commit for all changes

---

## Notes

- All 4 tasks modify different files — no merge conflicts possible when running in parallel
- All stories (US1, US2, US3) are applied together per task because they modify the same element
- No C# code changes — XAML only
- No new files or dependencies
- The empty-state "Go to Tests" button on ResultsView is explicitly excluded (FR-007)
