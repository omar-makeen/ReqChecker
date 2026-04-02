# Research: Standardize Back Buttons

**Date**: 2026-03-14
**Feature**: 066-standardize-back-buttons

## Finding 1: Header Grid Layout Pattern

**Decision**: All four views already use the same 3-column grid: `Auto | * | Auto`. The repositioning strategy is to move the back button from Column 2 (right action area) to Column 0 (left, before title), shifting the icon container to Column 0's existing content by wrapping both in a StackPanel or adding a Column.

**Rationale**: TestConfigView already has the back button in Column 0 (the reference layout). The other three views have the back button inside a StackPanel in Column 2 alongside action buttons. Moving it to Column 0 requires restructuring the header Grid to add a fourth column or placing the back button before the icon in Column 0.

**Alternatives considered**:
1. **Add a 4th column (Column 0 = Back, Column 1 = Icon, Column 2 = Title, Column 3 = Actions)** — cleanest separation, each element gets its own column. Matches TestConfigView's pattern where Back is Column 0, Icon+Title is Column 1, Actions is Column 2.
2. **Put back button inside Column 0 alongside the icon** — creates crowding, breaks the visual pattern.

**Decision**: Use the 3-column pattern from TestConfigView: Column 0 = Back button, Column 1 = Icon+Title (StackPanel or nested Grid), Column 2 = Action buttons. This means the page icon moves from Column 0 into Column 1 alongside the title.

## Finding 2: Style Standardization

**Decision**: Use `SecondaryButton` for all back buttons.

**Rationale**: `SecondaryButton` is defined with `Height=40`, `AccentPrimary` foreground and border, transparent background, hover fills with accent. It's visible enough to find but doesn't compete with `PrimaryButton` gradient CTAs. Currently:
- TestConfigView uses `GhostButton` (too subtle)
- HistoryView/ResultsView use `SecondaryButton` (correct)
- RunProgressView uses `PrimaryButton` (wrong semantic — competes with forward actions)

**Alternatives considered**: `GhostButton` (too subtle for a navigation element users need to find quickly).

## Finding 3: Action Buttons After Repositioning

**Decision**: When back button moves to Column 0, the remaining action buttons stay in Column 2. For views with multiple action buttons, remove the back button from the existing StackPanel — the other buttons stay.

**Details per view**:
- **HistoryView**: Remove back button from Column 2 StackPanel; "Clear All" button stays in Column 2 alone.
- **ResultsView**: Remove back button from Column 2 StackPanel; "Re-run Failed" and "Export" buttons remain.
- **RunProgressView**: Remove back button from Column 2 completion StackPanel; "View Results" button stays. The Cancel button (shown during execution) is unaffected.
- **TestConfigView**: Already correct — back in Column 0, "Save" button in Column 2.

## Finding 4: FocusManager Consideration

**Decision**: Keep `FocusManager.FocusedElement` pointing to the back button on pages where it's already the focus target.

**Current state**:
- TestConfigView: focuses BackButton (correct)
- HistoryView: focuses BackToTestsButton (keep)
- ResultsView: focuses BackToTestsButton (keep)
- RunProgressView: focuses CancelButton (keep — Cancel is more important during execution)

## Finding 5: Tooltip Standardization

**Decision**: All back buttons use `ToolTip="Return to the test suite"`, `ToolTipService.InitialShowDelay="400"`, `ToolTipService.ShowOnDisabled="True"`.

**Current gaps**: ResultsView back button is missing `ToolTipService.ShowOnDisabled="True"`. HistoryView tooltip says "Return to the test suite" (correct). TestConfigView says "Return to test list" (different wording — standardize to "Return to the test suite").
