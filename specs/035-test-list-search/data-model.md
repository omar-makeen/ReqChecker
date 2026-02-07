# Data Model: Search & Filter in Test List

**Feature Branch**: `035-test-list-search`
**Date**: 2026-02-07

## Entities (No New Entities)

This feature introduces no new data entities. It operates entirely on existing models:

### TestDefinition (existing, read-only usage)

| Field       | Type    | Usage in This Feature                          |
|-------------|---------|------------------------------------------------|
| DisplayName | string  | Matched against search text (case-insensitive substring) |
| Type        | string  | Matched against search text (case-insensitive substring) |
| Description | string? | Matched against search text if not null (case-insensitive substring) |

### SelectableTestItem (existing, no modifications to class)

| Field      | Type           | Usage in This Feature                                     |
|------------|----------------|-----------------------------------------------------------|
| Test       | TestDefinition | Accessed to check DisplayName, Type, Description against search |
| IsSelected | bool           | Preserved when items are hidden by filter; Select All operates on visible items only |

## New ViewModel Properties

### TestListViewModel (modified)

| Property/Field          | Type             | Description                                                |
|-------------------------|------------------|------------------------------------------------------------|
| SearchText              | string           | Bound to search TextBox; drives filter refresh on change   |
| FilteredTestsView       | ICollectionView  | CollectionView wrapping SelectableTests with Filter predicate |
| FilteredCount           | int (computed)   | Number of tests visible after filtering                    |
| IsFilterActive          | bool (computed)  | True when SearchText is non-empty after trimming           |
| TestCountDisplay        | string (computed)| "X of Y tests" when filtered, "Y tests" when unfiltered   |

### Modified Existing Properties

| Property       | Change Description                                                    |
|----------------|-----------------------------------------------------------------------|
| IsAllSelected  | Toggle operates on visible (filtered) items only, not all items       |
| SelectedCount  | Counts only visible selected items when filter is active              |
| RunButtonLabel | Reflects visible selected count when filter is active                 |

## State Transitions

```
Test List Page (profile loaded)
  │
  ├─ SearchText is empty → All tests visible, normal behavior
  │
  └─ User types in search box
       │
       ├─ SearchText changed → FilteredTestsView.Refresh()
       │    │
       │    ├─ Matches found → Show matching tests, update FilteredCount
       │    │    └─ TestCountDisplay = "X of Y tests"
       │    │
       │    └─ No matches → Show empty state message
       │         └─ TestCountDisplay = "0 of Y tests"
       │
       ├─ User clicks clear (X) button
       │    └─ SearchText = "" → All tests visible
       │         └─ TestCountDisplay = "Y tests"
       │
       ├─ User clicks Select All (while filtered)
       │    └─ Toggles IsSelected on visible items only
       │         └─ Hidden items retain their selection state
       │
       └─ User clicks Run (while filtered)
            └─ Run uses selection state (includes hidden selected tests)

Profile Reload
  └─ SearchText cleared → Filter removed → New tests populated
```

## Filter Predicate Logic

```
FilterTest(SelectableTestItem item):
  if SearchText is empty or whitespace:
    return true  (show all)

  trimmedSearch = SearchText.Trim()

  return item.Test.DisplayName.Contains(trimmedSearch, OrdinalIgnoreCase)
      OR item.Test.Type.Contains(trimmedSearch, OrdinalIgnoreCase)
      OR (item.Test.Description != null
          AND item.Test.Description.Contains(trimmedSearch, OrdinalIgnoreCase))
```
