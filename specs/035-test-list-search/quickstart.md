# Quickstart: Search & Filter in Test List

**Feature Branch**: `035-test-list-search`
**Date**: 2026-02-07

## What This Feature Does

Adds a search box to the Test List page toolbar. As the user types, the test list filters in real-time to show only tests whose display name, type, or description contains the search text (case-insensitive substring match). The test count updates to show "X of Y tests", Select All operates on visible items only, and clearing the search restores the full list.

## Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/ViewModels/TestListViewModel.cs` | Add `SearchText`, `FilteredTestsView`, `FilteredCount`, `IsFilterActive`, `TestCountDisplay` properties; modify Select All to operate on filtered items; clear search on profile load |
| `src/ReqChecker.App/Views/TestListView.xaml` | Add search TextBox in toolbar row; bind ListBox to `FilteredTestsView`; add no-results empty state; update test count badge binding |

## Files NOT Modified

- No changes to `SelectableTestItem`, `TestDefinition`, `IAppState`, or any test runner.
- No new files created (no new models, services, converters, or views).
- No new NuGet packages.

## How It Works

```
User sees Test List with all tests
  → Types "DNS" in search box
    → SearchText property updates (UpdateSourceTrigger=PropertyChanged)
      → OnSearchTextChanged fires
        → FilteredTestsView.Refresh() called
          → Filter predicate runs on each SelectableTestItem:
              - item.Test.DisplayName.Contains("dns", OrdinalIgnoreCase) → match
              - item.Test.Type.Contains("dns", OrdinalIgnoreCase) → match
              - item.Test.Description?.Contains("dns", OrdinalIgnoreCase) → match
          → ListBox shows only matching items
          → TestCountDisplay updates: "2 of 12 tests"
          → IsAllSelected reflects visible items only
  → Clicks X (clear button)
    → SearchText = ""
      → Filter shows all items
      → TestCountDisplay: "12 tests"
```

## Key Behaviors

1. **Select All + Filter**: Select All only toggles visible items. Hidden items keep their selection state.
2. **Run + Filter**: Run command uses full selection state (including hidden selected tests). Filter is purely visual.
3. **Profile Reload**: Search box clears automatically when a new profile is loaded.
4. **Whitespace**: Leading/trailing whitespace is trimmed. Empty/whitespace-only search shows all tests.

## Build & Test

```bash
dotnet build
dotnet run --project src/ReqChecker.App
```

1. Load a profile with 5+ tests of mixed types
2. Type a partial test name — verify only matching tests appear
3. Type a test type (e.g., "Ping") — verify all tests of that type appear
4. Type a keyword from a test description — verify the test appears
5. Verify test count shows "X of Y tests" while filtered
6. Click X to clear — verify all tests return
7. With filter active, click Select All — verify only visible items toggle
8. With filter active and some hidden tests selected, click Run — verify hidden selected tests are included
9. Load a new profile — verify search box clears
