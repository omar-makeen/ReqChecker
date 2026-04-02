# Quickstart: Standardize Back Buttons

## What Changed

Unified the back button across 4 views so it always appears in the same position (top-left), with the same style (SecondaryButton), label ("Back to Tests"), and tooltip.

## Files Modified

- `src/ReqChecker.App/Views/TestConfigView.xaml` — Restyle from GhostButton to SecondaryButton, update label and tooltip
- `src/ReqChecker.App/Views/HistoryView.xaml` — Move back button from right (Column 2) to left (Column 0), restyle
- `src/ReqChecker.App/Views/ResultsView.xaml` — Move back button from right (Column 2) to left (Column 0), add ShowOnDisabled
- `src/ReqChecker.App/Views/RunProgressView.xaml` — Move back button from right (Column 2) to left (Column 0), restyle from PrimaryButton to SecondaryButton

## How to Verify

1. Build and run the app
2. Load a test profile

**Test Config page** (click any test card):
- Back button is on the left, before the page title
- Styled as SecondaryButton (outlined border, accent color)
- Label says "Back to Tests" with ArrowLeft icon
- Click it — unsaved changes prompt still works if you edited something

**Results page** (run tests first, then view results):
- Back button is on the left, before the page title
- Same style, label, and icon as TestConfig
- "Re-run Failed" and "Export" buttons are on the right

**History page**:
- Back button is on the left, before the page title
- Same style, label, and icon
- "Clear All" button is on the right

**Run Progress page** (run tests):
- During execution: Cancel button shown (no back button)
- After completion: Back button appears on the left, "View Results" on the right
- Same SecondaryButton style, not PrimaryButton gradient

**Empty state on Results**:
- "Go to Tests" button should be UNCHANGED (centered, GhostButton)

**Theme toggle**:
- Switch dark/light theme — back buttons remain readable and consistent
