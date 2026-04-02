# Tasks: Fix Run Button Style & UX

**Input**: Design documents from `/specs/065-fix-run-button-style/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not requested — no test tasks included.

**Organization**: Tasks grouped by user story. US1 is already satisfied (see research.md) — no tasks needed.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: User Story 2 - Helpful Disabled State (Priority: P2) 🎯 MVP

**Goal**: Add a contextual tooltip to the Run button that shows "Select at least one test to run" when disabled and "Execute all selected tests and view results" when enabled.

**Independent Test**: Deselect all tests, hover over the disabled Run button — tooltip should say "Select at least one test to run". Select tests, hover — tooltip should say "Execute all selected tests and view results".

### Implementation for User Story 2

- [X] T001 [US2] Replace static ToolTip attribute with contextual tooltip using DataTrigger on HasSelectedTests in src/ReqChecker.App/Views/TestListView.xaml (lines 124-134). Remove `ToolTip="Execute all selected tests and view results"`, add `ToolTipService.ShowOnDisabled="True"`, and add `Button.ToolTip` element with a `ToolTip Style="{StaticResource ModernToolTip}"` containing a TextBlock with a Style trigger: default Text="Execute all selected tests and view results", DataTrigger on `{Binding HasSelectedTests}` Value="False" sets Text="Select at least one test to run".

**Checkpoint**: Disabled tooltip shows guidance text, enabled tooltip shows action description.

---

## Phase 2: User Story 3 - Consistent Font Size (Priority: P3)

**Goal**: Remove the inline `FontSize="15"` override on the Run button's TextBlock so the `PrimaryButtonLarge` style's `FontSize="16"` applies naturally.

**Independent Test**: Run button text should render at 16px (matching the style), not 15px.

### Implementation for User Story 3

- [X] T002 [US3] Remove `FontSize="15"` from the TextBlock inside the Run button's StackPanel content in src/ReqChecker.App/Views/TestListView.xaml (line 132). Change `<TextBlock Text="{Binding RunButtonLabel}" FontSize="15"/>` to `<TextBlock Text="{Binding RunButtonLabel}"/>`.

**Checkpoint**: Button text uses style-defined font size (16px) with no inline override.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (US2)**: No dependencies — can start immediately
- **Phase 2 (US3)**: No dependencies — can start immediately (different line in same file)

### User Story Dependencies

- **User Story 1 (P1)**: Already satisfied — `PrimaryButtonLarge` style exists and is applied. No tasks needed.
- **User Story 2 (P2)**: Independent. Modifies tooltip attributes on the Button element (lines 124-134).
- **User Story 3 (P3)**: Independent. Modifies TextBlock content inside the Button (line 132).

### Parallel Opportunities

- T001 and T002 modify different parts of the same file but do not conflict — they can be applied sequentially in either order.

---

## Implementation Strategy

### MVP First (User Story 2 Only)

1. Complete T001: Contextual disabled tooltip
2. **STOP and VALIDATE**: Test tooltip in both enabled/disabled states
3. Proceed to T002 for polish

### Incremental Delivery

1. T001 → Contextual tooltip → Validate (MVP!)
2. T002 → Font size consistency → Validate
3. Both changes are in the same file — commit together or separately

---

## Notes

- US1 (Premium Run Button Appearance) requires no implementation — research confirmed `PrimaryButtonLarge` already exists and is correctly applied
- Both tasks modify `src/ReqChecker.App/Views/TestListView.xaml` — same file, different sections
- No new files, no new dependencies, no C# code changes — XAML only
- Commit after both tasks for a single clean commit
