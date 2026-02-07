# Quickstart: Re-run Failed Tests Only

**Feature Branch**: `034-rerun-failed-tests`
**Date**: 2026-02-07

## What This Feature Does

Adds a "Re-run Failed" button to the Results page. After a test run completes with failures, the user clicks this button to re-execute only the failed tests (plus any tests that were skipped because they depended on a failed test). This avoids re-running the entire suite.

## Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/ViewModels/ResultsViewModel.cs` | Add `HasFailedTests` property, `RerunFailedTestsCommand`, and helper to collect re-run IDs |
| `src/ReqChecker.App/Views/ResultsView.xaml` | Add PrimaryButton after "Back to Tests" with ArrowRepeatAll24 icon, Visibility bound to HasFailedTests |

## Files NOT Modified

- No changes to `IAppState`, `NavigationService`, `SequentialTestRunner`, or any test type.
- No new files created (no new models, services, or views).

## How It Works

```
User sees Results page with failures
  → Clicks "Re-run Failed" (PrimaryButton, gradient, ArrowRepeatAll24 icon)
    → ResultsViewModel collects:
        - All TestIds where Status == Fail
        - All TestIds where Status == Skipped AND Error.Category == Dependency
    → Calls IAppState.SetSelectedTestIds(combinedIds)
    → Calls NavigationService.NavigateToRunProgress()
      → RunProgressViewModel picks up SelectedTestIds (existing flow)
      → Filters profile to only those tests
      → Executes and shows results
```

## Build & Test

```bash
dotnet build
dotnet run --project src/ReqChecker.App
```

1. Load a profile with tests that will fail (e.g., wrong hostname in Ping test)
2. Run all tests
3. On Results page, verify "Re-run Failed" button appears (gradient style, between Back and export buttons)
4. Click it — only failed tests should run
5. Run with all-passing profile — button should be hidden
