# Quickstart: Fix Run Button Style & UX

## What Changed

Two small fixes to the Run button on the Test Suite page:

1. **Contextual tooltip** — Shows "Select at least one test to run" when disabled, "Execute all selected tests and view results" when enabled.
2. **Font size consistency** — Removed inline `FontSize="15"` so the button text uses the style's `FontSize="16"`.

## Files Modified

- `src/ReqChecker.App/Views/TestListView.xaml` — Run button tooltip and content

## How to Verify

1. Build and run the app
2. Load a test profile
3. **Tooltip (enabled)**: Hover over Run button → should say "Execute all selected tests and view results"
4. **Tooltip (disabled)**: Deselect all tests → hover over disabled Run button → should say "Select at least one test to run"
5. **Font size**: Button text should render at 16px (matching `PrimaryButtonLarge` style), not 15px
