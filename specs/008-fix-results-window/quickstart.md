# Quickstart: Fix Test Results Window

**Feature**: 008-fix-results-window
**Date**: 2026-01-31

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- Windows OS (WPF application)

## Quick Verification

After implementing fixes, verify with these steps:

### 1. Build the Application
```bash
cd C:\workspace\pulsar\ReqChecker
dotnet build
```

### 2. Run Tests
```bash
dotnet test tests/ReqChecker.App.Tests --filter "FullyQualifiedName~ResultsViewModel"
```

### 3. Manual Verification

1. **Launch app**: `dotnet run --project src/ReqChecker.App`
2. **Load Sample Diagnostics profile**
3. **Run All Tests** (wait for completion)
4. **Click "Results"** in sidebar - verify data displays
5. **Click JSON button** - verify save dialog appears
6. **Click CSV button** - verify save dialog appears
7. **Click filter tabs** - verify filtering works
8. **Verify "Results" menu item** is highlighted

## Key Files

| File | Purpose |
|------|---------|
| `src/ReqChecker.App/Services/NavigationService.cs` | Fix data loading |
| `src/ReqChecker.App/ViewModels/ResultsViewModel.cs` | Add CanExport property |
| `src/ReqChecker.App/Views/ResultsView.xaml` | Bind export button state |
| `src/ReqChecker.App/MainWindow.xaml.cs` | Fix menu highlighting |

## Expected Behavior After Fix

| Scenario | Before | After |
|----------|--------|-------|
| Click Results after running tests | 0 Tests, empty screen | Shows all test results with counts |
| Click JSON button with results | Nothing happens | Save dialog appears |
| Click CSV button with results | Nothing happens | Save dialog appears |
| Navigate to Results | Menu item not highlighted | Results menu item highlighted |
| Click Results with no prior run | Empty screen | "No results" message displayed |

## Troubleshooting

### App won't build
- Ensure .NET 8.0 SDK installed: `dotnet --version`
- Close running instances of ReqChecker.App

### Tests fail
- Check if app is running (locks DLLs)
- Run `dotnet build` first

### Data still not loading
- Verify `NavigationService.NavigateToResults()` gets AppState and sets Report
- Check `IAppState.LastRunReport` has data after test run
