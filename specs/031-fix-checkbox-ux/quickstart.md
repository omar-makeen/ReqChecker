# Quickstart: Fix Checkbox Visibility and Select All Placement

**Feature**: 031-fix-checkbox-ux
**Date**: 2026-02-07

## Prerequisites

- .NET 8.0 SDK (net8.0-windows TFM)
- Windows OS (WPF application)

## Build & Run

```bash
# Build the solution
dotnet build

# Run the app
dotnet run --project src/ReqChecker.App

# Run tests
dotnet test
```

## What to Verify

1. **Checkbox checkmarks**: Navigate to Test Suite with a loaded profile. All checkboxes should display a visible ✓ when checked. Toggle checkboxes to verify the checkmark appears/disappears correctly.

2. **Select All placement**: The "Select All" checkbox should appear in a toolbar row between the page header and the test list — not in the header row next to the Run button.

3. **Theme testing**: Switch between Dark and Light themes (via sidebar "Light Mode" toggle). Checkmarks should remain visible in both themes.

4. **Regression check**: Select/deselect individual tests, use Select All, run a subset — all existing functionality should work identically.

## Files Modified

| File | Change |
|------|--------|
| `src/ReqChecker.App/Resources/Styles/Controls.xaml` | Fix checkmark Path geometry in AccentCheckBox style |
| `src/ReqChecker.App/Views/TestListView.xaml` | Move Select All to toolbar row, update grid layout |
