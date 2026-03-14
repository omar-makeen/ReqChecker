# Quickstart: Subtle Chip Badge

**Feature**: 064-subtle-chip-badge
**Date**: 2026-03-14

## Prerequisites

- .NET 8.0 SDK
- Windows 10/11

## Build

```bash
dotnet build src/ReqChecker.App/
```

## Test

```bash
dotnet test tests/ReqChecker.App.Tests/
```

## Verify

1. Run the app: `dotnet run --project src/ReqChecker.App/`
2. Load a profile (e.g., the bundled startup profile)
3. Observe the Test Suite page header — the "X tests" badge should appear muted (surface background, secondary text), clearly distinct from the accent-colored "Run All Tests" button
4. Observe the sidebar "Test Suite" nav item — the badge should use the same muted chip style
5. Toggle light/dark theme in Settings — both badges should remain readable

## Files Modified

| File | Change |
|------|--------|
| `src/ReqChecker.App/Views/TestListView.xaml` | Restyle page header badge |
| `src/ReqChecker.App/MainWindow.xaml` | Restyle sidebar badge |
