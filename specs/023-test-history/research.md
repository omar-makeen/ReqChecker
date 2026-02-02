# Research: Test History Feature

**Feature**: 023-test-history
**Date**: 2026-02-02

## Research Topics

### 1. JSON Storage Strategy for History

**Decision**: Single JSON file (`history.json`) containing array of RunReport objects

**Rationale**:
- Consistent with existing PreferencesService pattern
- Simple atomic read/write operations
- Easy to backup and restore
- Human-readable for debugging

**Alternatives Considered**:
- **One file per run**: More complex file management, harder to query across runs
- **SQLite database**: Overkill for this use case, adds external dependency
- **Binary serialization**: Not human-readable, harder to debug

**Implementation Notes**:
- File location: `%APPDATA%/ReqChecker/history.json`
- Use System.Text.Json with same options as PreferencesService
- Load entire history into memory on startup (acceptable for 500 runs ~10MB max)
- Save after each new run or deletion

### 2. Line Graph Control Implementation

**Decision**: Custom WPF UserControl using Path geometry (similar to DonutChart)

**Rationale**:
- Follows existing DonutChart pattern in the codebase
- No external charting library dependencies
- Full control over styling to match premium UI design
- Lightweight implementation for simple time-series data

**Alternatives Considered**:
- **LiveCharts2**: Adds NuGet dependency, may conflict with WPF-UI styling
- **OxyPlot**: Heavy dependency for a single chart type
- **ScottPlot**: Same concerns as above

**Implementation Notes**:
- Use Polyline or Path for the line
- X-axis: Run timestamps (or run index if many runs)
- Y-axis: Pass rate (0-100%)
- Data points with tooltips showing run details
- Optional: area fill below line for visual appeal

### 3. Flaky Test Detection Algorithm

**Decision**: Track pass/fail ratio per test across last N runs of same profile

**Rationale**:
- Simple algorithm that matches spec: "passed in some runs, failed in others"
- Profile-scoped to avoid false positives from different test configurations
- Last 10 runs provides recent trend without noise from old data

**Alternatives Considered**:
- **All-time analysis**: Too much data, old failures may not be relevant
- **Statistical significance testing**: Overcomplicated for this feature
- **Machine learning**: Way overkill

**Implementation Notes**:
- Flaky threshold: Test has both Pass and Fail in last 10 runs of same profile
- Calculate on-demand when viewing history (not stored)
- Display as badge/indicator on test results
- Show pass/fail ratio (e.g., "7/10 passed")

### 4. History View Navigation Integration

**Decision**: Add new NavigationViewItem after Results, before Diagnostics

**Rationale**:
- Logical flow: Profiles → Tests → Results → History → Diagnostics
- History is a natural follow-up after viewing current results
- Keeps diagnostic/system info at the end

**Implementation Notes**:
- Icon: `History24` (Fluent icon from WPF-UI)
- NavigationService update: Add `NavigateToHistory()` method
- MainWindow: Add NavHistory NavigationViewItem

### 5. Performance with Large History

**Decision**: Use UI virtualization for history list, lazy load details

**Rationale**:
- WPF VirtualizingStackPanel handles 500+ items efficiently
- Match existing ResultsView pattern which uses virtualization
- Summary data loads fast; full details load on selection

**Implementation Notes**:
- History list shows: date, profile name, pass rate, duration (summary)
- Full test results loaded only when user selects a run
- Consider pagination if history exceeds 1000 runs (future enhancement)

## Resolved Clarifications

All technical clarifications have been resolved:

| Topic | Decision |
|-------|----------|
| Storage format | Single JSON file in AppData |
| Charting | Custom LineChart control |
| Flaky detection | Pass/fail ratio in last 10 runs |
| Navigation | New item after Results |
| Performance | Virtualized list with lazy loading |
