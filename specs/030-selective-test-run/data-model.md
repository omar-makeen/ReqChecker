# Data Model: Selective Test Run

**Feature**: 030-selective-test-run
**Date**: 2026-02-07

## Entities

### SelectableTestItem (NEW — App layer ViewModel)

Lightweight wrapper around `TestDefinition` that adds UI selection state.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `TestDefinition` | `TestDefinition` | (required) | The underlying test definition from the profile |
| `IsSelected` | `bool` | `true` | Whether this test is selected for the next run |

**Lifecycle**: Created when a profile is loaded into `TestListViewModel`. Destroyed when navigating away or loading a new profile. Never persisted.

**Relationships**: One-to-one wrapper around `TestDefinition`. The `TestListViewModel` holds an `ObservableCollection<SelectableTestItem>`.

### IAppState Extension

New property added to the shared state interface.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `SelectedTestIds` | `IReadOnlyList<string>?` | `null` | Test IDs selected for the next run; `null` means "run all" |

**Lifecycle**: Set by `TestListViewModel` before navigation to `RunProgressView`. Read by `RunProgressViewModel` on construction. Cleared (set to `null`) after consumption.

## State Transitions

```
Profile Loaded → All SelectableTestItems created with IsSelected=true
                 SelectedTestIds in AppState = null (run-all default)

User unchecks test → SelectableTestItem.IsSelected = false
                     Button label updates to "Run N of M Tests"
                     Card opacity reduces to 0.5

User checks "Select All" → All items IsSelected = true
                           Button reverts to "Run All Tests"

User clicks Run → SelectedTestIds written to AppState
                  Navigation to RunProgress
                  RunProgressViewModel reads IDs, filters profile
                  SelectedTestIds cleared in AppState

Navigation away → SelectableTestItems destroyed
                  Next visit creates fresh items (all selected)
```

## No Schema Changes

The `Profile` JSON schema, `TestDefinition` model, and `ITestRunner` interface are **not modified** by this feature. Selection is purely a UI-layer concept.
