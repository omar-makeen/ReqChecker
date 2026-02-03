# Quickstart: Remove Pass Rate Trend Chart

**Branch**: `025-remove-trend-chart` | **Date**: 2026-02-03

## Overview

Remove the Pass Rate Trend chart from the Test History page and clean up all related unused code.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension

## Quick Implementation Guide

### Step 1: Delete LineChart Control

Delete these files:
- `src/ReqChecker.App/Controls/LineChart.xaml`
- `src/ReqChecker.App/Controls/LineChart.xaml.cs`

```bash
rm src/ReqChecker.App/Controls/LineChart.xaml
rm src/ReqChecker.App/Controls/LineChart.xaml.cs
```

### Step 2: Update HistoryView.xaml

**File**: `src/ReqChecker.App/Views/HistoryView.xaml`

#### 2a. Remove controls namespace (line 7)

```xml
<!-- DELETE THIS LINE -->
xmlns:controls="clr-namespace:ReqChecker.App.Controls"
```

#### 2b. Update Grid.RowDefinitions (lines 50-56)

```xml
<!-- BEFORE: 5 rows -->
<Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
</Grid.RowDefinitions>

<!-- AFTER: 4 rows -->
<Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
</Grid.RowDefinitions>
```

#### 2c. Delete Trend Chart Border (lines 175-201)

Delete the entire `<!-- Trend Chart -->` section:

```xml
<!-- DELETE THIS ENTIRE BLOCK -->
<!-- Trend Chart -->
<Border Grid.Row="2"
        Style="{StaticResource Card}"
        ...>
    ...
</Border>
```

#### 2d. Update Row Numbers

Change Filter Tabs from `Grid.Row="3"` to `Grid.Row="2"`:
```xml
<!-- Filter Tabs -->
<Border Grid.Row="2" ...>
```

Change History List from `Grid.Row="4"` to `Grid.Row="3"`:
```xml
<!-- History List with virtualization -->
<ListBox Grid.Row="3" ...>
```

Change Empty State from `Grid.Row="4"` to `Grid.Row="3"`:
```xml
<!-- Empty state when no history -->
<Grid Grid.Row="3" ...>
```

### Step 3: Update HistoryViewModel.cs

**File**: `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`

#### 3a. Remove import (line 7)

```csharp
// DELETE THIS LINE
using ReqChecker.App.Controls;
```

#### 3b. Remove TrendDataPoints property (around line 66)

```csharp
// DELETE THIS BLOCK
/// <summary>
/// Gets trend data points for line chart.
/// </summary>
public ObservableCollection<LineChart.ChartDataPoint> TrendDataPoints { get; } = new();
```

#### 3c. Remove FlakyTests property (around line 71)

```csharp
// DELETE THIS BLOCK
/// <summary>
/// Gets flaky tests.
/// </summary>
public ObservableCollection<TestTrendData> FlakyTests { get; } = new();
```

#### 3d. Remove UpdateTrendDataPoints call from OnActiveFilterChanged

```csharp
// BEFORE
partial void OnActiveFilterChanged(string? value)
{
    FilteredHistory?.Refresh();
    UpdateTrendDataPoints();  // DELETE THIS LINE
}

// AFTER
partial void OnActiveFilterChanged(string? value)
{
    FilteredHistory?.Refresh();
}
```

#### 3e. Remove UpdateTrendDataPoints call from OnHistoryRunsChanged

```csharp
// BEFORE
partial void OnHistoryRunsChanged(ObservableCollection<RunReport> value)
{
    OnPropertyChanged(nameof(IsHistoryEmpty));
    UpdateTrendDataPoints();  // DELETE THIS LINE
}

// AFTER
partial void OnHistoryRunsChanged(ObservableCollection<RunReport> value)
{
    OnPropertyChanged(nameof(IsHistoryEmpty));
}
```

#### 3f. Delete UpdateTrendDataPoints method (entire method)

```csharp
// DELETE THIS ENTIRE METHOD
/// <summary>
/// Updates trend data points for line chart based on current history.
/// </summary>
public void UpdateTrendDataPoints()
{
    ...
}
```

#### 3g. Delete ComputeFlakyTests method (entire method)

```csharp
// DELETE THIS ENTIRE METHOD
/// <summary>
/// Computes flaky tests from history.
/// </summary>
private void ComputeFlakyTests()
{
    ...
}
```

## Verification

1. **Build**: `dotnet build src/ReqChecker.App`
2. **Run the app**: `dotnet run --project src/ReqChecker.App`
3. **Navigate to Test History** (with history data)
4. **Verify**:
   - No trend chart visible
   - History list displays correctly
   - Filter tabs work
   - Empty state works (if no history)
   - No errors in console

## Files Changed

| File | Action |
|------|--------|
| `src/ReqChecker.App/Controls/LineChart.xaml` | DELETE |
| `src/ReqChecker.App/Controls/LineChart.xaml.cs` | DELETE |
| `src/ReqChecker.App/Views/HistoryView.xaml` | MODIFY |
| `src/ReqChecker.App/ViewModels/HistoryViewModel.cs` | MODIFY |

## Expected Code Reduction

- ~280 lines removed (LineChart control)
- ~60 lines removed (ViewModel methods)
- ~30 lines removed (XAML)
- **Total: ~370 lines of code removed**
