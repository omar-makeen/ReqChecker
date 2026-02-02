# Quickstart: Test History Feature

**Feature**: 023-test-history
**Date**: 2026-02-02

## Overview

This feature adds persistent test history with trend analysis and flaky test detection.

## Key Components

### 1. History Service (Infrastructure Layer)

**Location**: `src/ReqChecker.Infrastructure/History/`

```csharp
public interface IHistoryService
{
    // Load history from disk (call on startup)
    Task<List<RunReport>> LoadHistoryAsync();

    // Save a new run to history (call after test completion)
    Task SaveRunAsync(RunReport report);

    // Delete a specific run
    Task DeleteRunAsync(string runId);

    // Clear all history
    Task ClearHistoryAsync();

    // Get storage statistics
    HistoryStats GetStats();
}
```

**Pattern**: Follow `PreferencesService` for JSON serialization.

### 2. History View (App Layer) - PREMIUM UI REQUIRED

**Location**: `src/ReqChecker.App/Views/HistoryView.xaml`

**MANDATORY**: Must use premium/authentic UI styling matching other pages.

**Premium UI Elements to Use**:
- `AnimatedPageHeader` style with gradient accent line
- `Card` style for content sections
- `PrimaryButton` / `SecondaryButton` / `GhostButton` styles
- `StatusPass` / `StatusFail` / `StatusSkip` colors
- `TextPrimary` / `TextSecondary` / `TextTertiary` typography
- `BackgroundBase` / `BackgroundSurface` / `BackgroundElevated` backgrounds
- Entrance animations (like `AnimatedResultItem` in ResultsView)

**Layout**:
```
┌──────────────────────────────────────────────────────────┐
│  [AnimatedPageHeader: "Test History" + History24 icon]  │
│  [Gradient accent line]                    [Clear All]  │
├──────────────────────────────────────────────────────────┤
│  ┌─────────────── Card ────────────────────────────────┐ │
│  │        LineChart: Pass Rate Over Time               │ │
│  │        (styled with accent colors)                  │ │
│  └─────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────┤
│  Filter: [FilterTab style] All | Profile A | Profile B   │
├──────────────────────────────────────────────────────────┤
│  ┌─────────────── Card (virtualized list) ─────────────┐ │
│  │ [ExpanderCard] Run 1 | 95% pass | 1.2s      [Del]  │ │
│  │ [ExpanderCard] Run 2 | 88% pass | 2.1s      [Del]  │ │
│  └─────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
```

**Reference**: Copy styling patterns from `ResultsView.xaml` and `DiagnosticsView.xaml`.

### 3. LineChart Control

**Location**: `src/ReqChecker.App/Controls/LineChart.xaml`

**Properties**:
- `DataPoints`: Collection of (DateTime, double) values
- `MinY` / `MaxY`: Y-axis range (default 0-100 for percentages)
- `PointColor`: Brush for data points
- `LineColor`: Brush for line

**Pattern**: Follow `DonutChart` for custom WPF control implementation.

### 4. Navigation Integration

**MainWindow.xaml**: Add after NavResults, before NavDiagnostics:
```xml
<ui:NavigationViewItem
    x:Name="NavHistory"
    Content="History"
    Icon="{ui:SymbolIcon History24}"
    TargetPageType="{x:Type views:HistoryView}"/>
```

## Integration Points

### Auto-Save on Test Completion

In `RunProgressViewModel` or wherever test execution completes:

```csharp
// After test run completes successfully
await _historyService.SaveRunAsync(runReport);
```

### Viewing Historical Run Details

When user selects a run from history list:
- Load full RunReport from history
- Navigate to ResultsView with historical data
- Or show inline details in HistoryView

### Flaky Test Detection

Compute on-demand in HistoryViewModel:

```csharp
public List<TestTrendData> GetFlakyTests(string? profileId = null)
{
    var runs = profileId != null
        ? _history.Where(r => r.ProfileId == profileId)
        : _history;

    return runs
        .SelectMany(r => r.Results)
        .GroupBy(t => t.TestId)
        .Select(g => ComputeTrend(g))
        .Where(t => t.IsFlaky)
        .ToList();
}
```

## File Structure After Implementation

```
src/
├── ReqChecker.Core/
│   └── Models/
│       └── HistoryStore.cs         # New: Container for history JSON
│
├── ReqChecker.Infrastructure/
│   └── History/
│       ├── IHistoryService.cs      # New: Interface
│       └── HistoryService.cs       # New: JSON persistence
│
└── ReqChecker.App/
    ├── Controls/
    │   ├── LineChart.xaml          # New: Line graph control
    │   └── LineChart.xaml.cs
    ├── ViewModels/
    │   └── HistoryViewModel.cs     # New: History view logic
    ├── Views/
    │   ├── HistoryView.xaml        # New: History UI
    │   └── HistoryView.xaml.cs
    └── MainWindow.xaml             # Modified: Add history nav item
```

## Dependencies

No new NuGet packages required. Uses:
- System.Text.Json (built-in)
- CommunityToolkit.Mvvm (existing)
- WPF-UI (existing)

## Testing Checklist

### Functionality
- [ ] History persists after app restart
- [ ] New runs appear in history automatically
- [ ] Can delete individual runs
- [ ] Can clear all history with confirmation
- [ ] Line graph displays correctly with 2+ runs
- [ ] Flaky tests are identified correctly
- [ ] Empty state shown when no history
- [ ] Performance acceptable with 100+ runs
- [ ] Corrupted history file handled gracefully

### Premium UI (MANDATORY)
- [ ] Page header uses AnimatedPageHeader style with gradient accent line
- [ ] Icon container with History24 icon in accent background
- [ ] Cards use Card style with proper elevation
- [ ] Buttons use PrimaryButton/SecondaryButton/GhostButton styles
- [ ] Status colors match StatusPass/StatusFail/StatusSkip
- [ ] Typography uses TextPrimary/TextSecondary/TextTertiary
- [ ] Backgrounds use BackgroundBase/BackgroundSurface/BackgroundElevated
- [ ] List items have entrance animations
- [ ] Empty state matches premium design (icon + message + action)
- [ ] Dark and Light themes both work correctly
