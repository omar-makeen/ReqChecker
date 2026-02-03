# Implementation Plan: Remove Pass Rate Trend Chart

**Branch**: `025-remove-trend-chart` | **Date**: 2026-02-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/025-remove-trend-chart/spec.md`

## Summary

Remove the Pass Rate Trend chart from the Test History page and clean up all related unused code. This simplifies the UI, reduces maintenance burden, and frees screen space for the history list.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (code removal)
**Testing**: Manual visual testing
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop application
**Performance Goals**: N/A (removal improves performance)
**Constraints**: Must not break existing functionality
**Scale/Scope**: Single page modification + code cleanup

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- ✅ No new libraries needed (removing code)
- ✅ No new projects needed
- ✅ Follows existing patterns (simple removal)
- ✅ Changes are scoped to single feature
- ✅ Reduces complexity (net code removal)

## Project Structure

### Documentation (this feature)

```text
specs/025-remove-trend-chart/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
└── checklists/          # Validation checklists
```

### Source Code (files to modify/delete)

```text
src/ReqChecker.App/
├── Controls/
│   ├── LineChart.xaml           # DELETE
│   └── LineChart.xaml.cs        # DELETE
├── Views/
│   ├── HistoryView.xaml         # MODIFY: Remove chart XAML
│   └── HistoryView.xaml.cs      # MODIFY: Remove chart namespace if needed
└── ViewModels/
    └── HistoryViewModel.cs      # MODIFY: Remove TrendDataPoints & related code
```

**Structure Decision**: Existing WPF MVVM structure, modifying 3 files and deleting 2 files.

## Implementation Details

### Files to DELETE

1. `src/ReqChecker.App/Controls/LineChart.xaml`
2. `src/ReqChecker.App/Controls/LineChart.xaml.cs`

### Files to MODIFY

#### 1. HistoryView.xaml

**Remove:**
- The `xmlns:controls` namespace declaration (line 7)
- The entire Trend Chart Border element (Grid.Row="2", lines 176-201)
- Update Grid.RowDefinitions to remove unused row

**Current grid structure:**
```
Row 0: Header (Auto)
Row 1: Status Message (Auto)
Row 2: Trend Chart (Auto)       <-- REMOVE
Row 3: Filter Tabs (Auto)
Row 4: History List / Empty State (*)
```

**New grid structure:**
```
Row 0: Header (Auto)
Row 1: Status Message (Auto)
Row 2: Filter Tabs (Auto)
Row 3: History List / Empty State (*)
```

#### 2. HistoryView.xaml.cs

- Remove `using ReqChecker.App.Controls;` if present and unused

#### 3. HistoryViewModel.cs

**Remove:**
- `using ReqChecker.App.Controls;` import
- `TrendDataPoints` property declaration
- `UpdateTrendDataPoints()` method
- `ComputeFlakyTests()` method (if only used for trends)
- `FlakyTests` property (if only used for trends)
- Calls to `UpdateTrendDataPoints()` in:
  - `OnActiveFilterChanged()`
  - `OnHistoryRunsChanged()`

**Keep:**
- `IsHistoryEmpty` property
- `HistoryRuns` collection
- All navigation, filter, delete, and clear functionality

## Complexity Tracking

No violations - this is a net removal of code, reducing complexity.

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Breaking other features | Verify filter tabs, list, empty state still work |
| Missing dependencies | Search codebase for LineChart usage before deleting |
| Grid layout issues | Test with history data after row removal |
