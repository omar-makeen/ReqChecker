# Research: Remove Pass Rate Trend Chart

**Branch**: `025-remove-trend-chart` | **Date**: 2026-02-03

## Research Summary

This is a code removal feature with no technical unknowns. Research focused on identifying all code to remove and verifying no dependencies will break.

## Findings

### 1. LineChart Control Usage

**Search Results:**
- `LineChart.xaml` - Control definition
- `LineChart.xaml.cs` - Control code-behind (263 lines)
- `HistoryView.xaml` - Only usage of LineChart control
- `HistoryViewModel.cs` - TrendDataPoints property that feeds the chart

**Conclusion:** LineChart is only used in HistoryView. Safe to delete entirely.

### 2. TrendDataPoints & Related Code

**In HistoryViewModel.cs:**
```csharp
// Line 66 - Property declaration
public ObservableCollection<LineChart.ChartDataPoint> TrendDataPoints { get; } = new();

// Line 71 - FlakyTests property (may be related)
public ObservableCollection<TestTrendData> FlakyTests { get; } = new();

// Methods that use TrendDataPoints:
- UpdateTrendDataPoints() - Called from OnActiveFilterChanged and OnHistoryRunsChanged
- ComputeFlakyTests() - Called from UpdateTrendDataPoints
```

**Decision:** Remove TrendDataPoints, UpdateTrendDataPoints(), and related code. FlakyTests may also be removed if not used elsewhere.

### 3. Grid Layout Impact

**Current HistoryView.xaml Grid:**
```xml
<Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>  <!-- Row 0: Header -->
    <RowDefinition Height="Auto"/>  <!-- Row 1: Status Message -->
    <RowDefinition Height="Auto"/>  <!-- Row 2: Trend Chart - REMOVE -->
    <RowDefinition Height="Auto"/>  <!-- Row 3: Filter Tabs -->
    <RowDefinition Height="*"/>     <!-- Row 4: List/Empty State -->
</Grid.RowDefinitions>
```

**After removal:**
- Delete Row 2 definition
- Update Filter Tabs from `Grid.Row="3"` to `Grid.Row="2"`
- Update List/Empty State from `Grid.Row="4"` to `Grid.Row="3"`

### 4. TestTrendData Model

**File:** `src/ReqChecker.Core/Models/TestTrendData.cs`

This model is used for FlakyTests. If FlakyTests is removed, this model can also be deleted.

**Decision:** Keep TestTrendData for now - may be used in future features. Only remove if causing build errors.

## Decisions Summary

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Delete LineChart control entirely | Only used for trend chart | Keep for future use - rejected (YAGNI) |
| Remove TrendDataPoints property | No longer needed | Keep property but don't populate - rejected (dead code) |
| Keep TestTrendData model | May be useful later | Delete - deferred until confirmed unused |
| Update grid row numbers | Required for layout | None |

## No Clarifications Needed

All code paths are clear. This is a straightforward removal operation.
