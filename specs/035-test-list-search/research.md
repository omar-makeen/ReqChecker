# Research: Search & Filter in Test List

**Feature Branch**: `035-test-list-search`
**Date**: 2026-02-07

## Decision 1: Filtering Mechanism

**Decision**: Use WPF `ICollectionView` with a `Filter` predicate on the existing `SelectableTests` collection, matching the pattern established in `ResultsViewModel` and `HistoryViewModel`.

**Rationale**: The app already uses `CollectionViewSource.GetDefaultView()` + `Filter` predicate + `Refresh()` in two view models. This is the idiomatic WPF approach for in-memory filtering — no new dependencies, no new patterns, and the ListBox bound to `SelectableTests` will automatically respect the view's filter.

**Alternatives considered**:
- Creating a separate `FilteredTests` ObservableCollection that gets repopulated on each keystroke — rejected because it breaks item references (selection state would need manual sync) and duplicates data.
- Using LINQ `.Where()` to create a new list on each keystroke — rejected for same reasons as above plus it would lose the staggered animation on items.
- Using `CollectionViewSource` in XAML — rejected because the filter predicate logic belongs in the ViewModel, not the View.

## Decision 2: Search Scope (Name + Type + Description)

**Decision**: Search all three fields in a single pass — `TestDefinition.DisplayName`, `TestDefinition.Type`, and `TestDefinition.Description` — using `string.Contains()` with `StringComparison.OrdinalIgnoreCase`.

**Rationale**: The spec defines three user stories (P1: name, P2: type, P3: description) but all three use the same search box. A single filter predicate that checks all three fields in OR fashion is simpler than building separate search modes. The `TestDefinition` model already exposes all three properties directly via the `SelectableTestItem.Test` wrapper.

**Alternatives considered**:
- Separate search modes (dropdown: "Search by Name / Type / Description") — rejected as over-engineered for the use case; users want to type and find, not configure search scope.
- Fuzzy matching (Levenshtein distance, word boundary matching) — rejected per spec assumption: "substring match, not word-boundary or fuzzy match."

## Decision 3: Select All Behavior with Active Filter

**Decision**: The `IsAllSelected` checkbox will operate on the `ICollectionView`'s visible items only. Hidden items retain their `IsSelected` state. The `SelectedCount` and `TotalCount` properties will show filtered vs total counts.

**Rationale**: The spec explicitly requires this (FR-010, FR-011). The existing `ToggleSelectAllCommand` iterates `SelectableTests` — it needs to be updated to iterate only the `ICollectionView`'s current items. The `SelectedCount` property needs to count only visible selected items.

**Alternatives considered**:
- Select All always operates on all items regardless of filter — rejected because it contradicts FR-010 and would confuse users who expect Select All to mean "select everything I can see."
- Disable Select All when filter is active — rejected as unnecessarily restrictive.

## Decision 4: Test Count Indicator Format

**Decision**: Show "X of Y tests" format in the existing count badge when a filter is active (e.g., "3 of 12 tests"). When no filter is active, show the original format (e.g., "12 tests").

**Rationale**: FR-007 requires the count indicator to reflect visible vs total. The existing `TotalCount` property and test count badge in the header can be updated to show the dual-count format. This is the most intuitive format — users immediately understand that 3 items match out of 12 total.

**Alternatives considered**:
- Always showing "X of Y" even without filter (e.g., "12 of 12 tests") — rejected as noisy when no filter is active.
- Showing only the filtered count — rejected because users lose context of how many total tests exist.

## Decision 5: Search Box UI Design

**Decision**: Use a WPF-UI `TextBox` with `PlaceholderText`, a Search16 icon as leading visual, and a clear (X) button. Place it in the existing toolbar row (Row 2) next to the Select All checkbox, stretching to fill available width.

**Rationale**: FR-001 specifies placement "in the toolbar area, between the header and the test list." FR-009 requires a clear button. FR-013 requires visual consistency with the premium design system. The toolbar row already contains the Select All checkbox — adding the search box beside it keeps the toolbar compact. WPF-UI's TextBox already supports `PlaceholderText` and can be styled with the app's accent colors for focus state.

**Alternatives considered**:
- Adding a new row above the toolbar — rejected as it adds vertical space and separates search from the list controls.
- Using a custom search control — rejected; a styled TextBox with icon is sufficient and consistent with the app's approach of styling native controls.

## Decision 6: Profile Reload Behavior

**Decision**: Clear the search text in the `OnCurrentProfileChanged` handler, which will automatically clear the filter via the property change notification chain.

**Rationale**: FR-012 requires the search box to clear when a new profile is loaded. The `TestListViewModel` already handles profile changes in `PopulateSelectableTests()` — adding `SearchText = string.Empty` at the start of that method is the simplest approach. The `ICollectionView` filter will be re-established on the new collection anyway.

**Alternatives considered**:
- Persisting the search text across profile loads — rejected as contradicts FR-012 and would confuse users.
- Clearing search text in the view layer (XAML trigger) — rejected because state management belongs in the ViewModel.
