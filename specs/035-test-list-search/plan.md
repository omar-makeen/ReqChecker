# Implementation Plan: Search & Filter in Test List

**Branch**: `035-test-list-search` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/035-test-list-search/spec.md`

## Summary

Add a search box to the Test List page toolbar that filters tests in real-time by display name, type, or description (case-insensitive substring match). Uses WPF `ICollectionView` with a Filter predicate on the existing `SelectableTests` collection — the same pattern used in `ResultsViewModel` and `HistoryViewModel`. Select All operates on visible items only, selection state is preserved for hidden items, and the search clears on profile reload. Only two files are modified.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (in-memory filtering on the already-loaded test collection)
**Testing**: Manual verification (load profile, type search terms, verify filtering behavior)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop WPF application
**Performance Goals**: Instant filtering (no perceptible delay) — profiles have at most a few hundred tests
**Constraints**: Must use existing ICollectionView pattern; no new services or interfaces; Select All must respect filter
**Scale/Scope**: 2 files modified, ~60 lines of ViewModel code + ~40 lines of XAML

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is a blank template — no project-specific gates defined. No violations possible.

**Post-Phase 1 re-check**: No new projects, no new packages, no new abstractions introduced. Feature is a minimal addition to existing ViewModel + View using established patterns.

## Project Structure

### Documentation (this feature)

```text
specs/035-test-list-search/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output — research decisions
├── data-model.md        # Phase 1 output — entity usage analysis
├── quickstart.md        # Phase 1 output — implementation guide
├── contracts/           # Phase 1 output — filter contract
│   └── test-list-filter.md
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (files modified)

```text
src/ReqChecker.App/
├── ViewModels/
│   └── TestListViewModel.cs    # Add search/filter properties, modify Select All behavior
└── Views/
    └── TestListView.xaml        # Add search TextBox, bind to filtered view, add no-results state
```

**Structure Decision**: This is a minimal 2-file change within the existing WPF application structure. No new files, projects, or directories are created.

## Implementation Details

### Phase 1: ViewModel Logic (TestListViewModel.cs)

**Add search property**:
- `SearchText` (string, ObservableProperty): Bound two-way to search TextBox
- On change: trim, refresh `FilteredTestsView`, update computed properties

**Add ICollectionView**:
- `FilteredTestsView` (ICollectionView): Created from `CollectionViewSource.GetDefaultView(SelectableTests)`
- Filter predicate: `FilterTest(object obj)` — checks DisplayName, Type, Description against trimmed SearchText
- Refreshed on every `SearchText` change
- Re-created when `SelectableTests` is repopulated (profile change)

**Add computed properties**:
- `FilteredCount` (int): Count of items passing filter
- `IsFilterActive` (bool): True when `SearchText.Trim()` is non-empty
- `TestCountDisplay` (string): "X of Y tests" when filtered, "Y tests" when unfiltered

**Modify Select All behavior**:
- `ToggleSelectAllCommand`: When `IsFilterActive`, iterate only items in `FilteredTestsView.Cast<SelectableTestItem>()`
- `IsAllSelected` getter: When `IsFilterActive`, reflect state of visible items only

**Clear on profile load**:
- Set `SearchText = string.Empty` in `PopulateSelectableTests()` before rebuilding the collection

### Phase 2: View Layer (TestListView.xaml)

**Add search TextBox in toolbar row** (Row 2, before Select All checkbox):
- `TextBox` with `PlaceholderText="Search tests..."`
- `Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"`
- Search icon (Search16) and clear button (X)
- Consistent with premium design system (accent focus border)

**Bind ListBox to filtered view**:
- Change `ItemsSource="{Binding SelectableTests}"` to `ItemsSource="{Binding FilteredTestsView}"`

**Add no-results empty state**:
- Visible when `IsFilterActive` is true AND `FilteredCount` is 0
- Centered message: "No tests match your search"
- Styled consistently with existing empty states

**Update test count badge**:
- Bind to `TestCountDisplay` instead of raw `TotalCount`

### Key Design Decisions (from research.md)

1. **ICollectionView pattern** — reuses established pattern from ResultsViewModel and HistoryViewModel
2. **Single search box, multi-field match** — simpler than separate search modes, matches spec intent
3. **Select All on visible only** — per FR-010, hidden items retain selection state (FR-011)
4. **Hide vs disable empty state** — show "No tests match" message, not a disabled list
5. **Clear on profile reload** — per FR-012, simplest approach is setting SearchText = "" in profile change handler

## Complexity Tracking

No constitution violations. No complexity justifications needed.

| Aspect | Assessment |
|--------|-----------|
| New files | 0 |
| Modified files | 2 |
| New packages | 0 |
| New abstractions | 0 |
| Lines of code (estimated) | ~100 |
