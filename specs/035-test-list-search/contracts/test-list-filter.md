# Contract: Test List Filter

**Feature**: 035-test-list-search
**Date**: 2026-02-07

## ViewModel Contract: TestListViewModel

### New Properties

```
SearchText : string
  - Bindable property (two-way binding from search TextBox)
  - Default: "" (empty string)
  - On change: trims value, refreshes ICollectionView filter
  - Cleared when new profile is loaded

FilteredTestsView : ICollectionView
  - Read-only property, created from SelectableTests ObservableCollection
  - Filter predicate checks DisplayName, Type, Description
  - ListBox binds to this instead of raw SelectableTests
  - Refreshed on every SearchText change

FilteredCount : int
  - Read-only computed property
  - Returns count of items passing the current filter
  - Updated after each filter refresh

IsFilterActive : bool
  - Read-only computed property
  - Returns true when SearchText.Trim() is non-empty

TestCountDisplay : string
  - Read-only computed property
  - Returns "X of Y tests" when IsFilterActive, "Y tests" otherwise
  - X = FilteredCount, Y = TotalCount
```

### Modified Behavior

```
ToggleSelectAllCommand:
  - When IsFilterActive: iterates only visible items in FilteredTestsView
  - When not active: iterates all items (existing behavior)

IsAllSelected (getter):
  - When IsFilterActive: reflects state of visible items only
  - When not active: reflects state of all items (existing behavior)

SelectedCount:
  - Always counts all selected items (visible + hidden)
  - This ensures Run button label accurately reflects what will run

RunButtonLabel:
  - Uses SelectedCount (all selected, not just visible)
  - Accurately shows how many tests will run if user clicks Run
```

## View Contract: TestListView.xaml

### Search Box Element

```
Location: Row 2 (toolbar area), before Select All checkbox
Type: TextBox
Properties:
  - Text: {Binding SearchText, UpdateSourceTrigger=PropertyChanged}
  - PlaceholderText: "Search tests..."
  - Icon: Search16 (leading position via TextBox.Icon or custom template)
  - Clear button: Built-in or custom X button to clear text
  - Margin: consistent with toolbar spacing
  - Focus: accent border on focus (existing TextBox style)
```

### Empty State (No Results)

```
Location: Row 3 (replaces ListBox when filter matches nothing)
Visibility: Visible when IsFilterActive AND FilteredCount == 0
Content:
  - Icon: Search24 or similar
  - Text: "No tests match your search"
  - Subtle, centered, matching existing empty state styling
```

### Test Count Badge (Modified)

```
Location: Header area (existing badge)
Text: {Binding TestCountDisplay}
  - Filtered: "3 of 12 tests"
  - Unfiltered: "12 tests"
```

### ListBox Binding (Modified)

```
Before: ItemsSource="{Binding SelectableTests}"
After:  ItemsSource="{Binding FilteredTestsView}"
```
