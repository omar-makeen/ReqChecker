# Research: Diagnostics Auto-Load

**Feature**: 017-diagnostics-auto-load
**Date**: 2026-02-01

## Research Questions

### 1. Can MachineInfoCollector.Collect() be reused independently?

**Decision**: Yes, use existing static method directly

**Rationale**:
- `MachineInfoCollector.Collect()` is a static method with no dependencies
- Already returns a complete `MachineInfo` object with all required data
- No need to modify Infrastructure layer

**Alternatives Considered**:
- Creating a new service interface: Rejected - adds unnecessary abstraction for simple static call
- Caching collected data: Rejected - spec requires fresh data on each navigation

### 2. How to trigger data load on page navigation?

**Decision**: Call LoadMachineInfo() from code-behind when page loads/navigates

**Rationale**:
- WPF pages fire `Loaded` event on navigation
- DiagnosticsView.xaml.cs already has DataContext set to ViewModel
- Simple pattern: `Loaded += (s, e) => ((DiagnosticsViewModel)DataContext).LoadMachineInfo()`

**Alternatives Considered**:
- INavigationAware interface: Rejected - would require framework changes, over-engineering
- Behavior/attached property: Rejected - adds complexity for single use case
- Constructor load: Rejected - ViewModel is singleton, wouldn't refresh on navigation

### 3. Should machine info collection block UI thread?

**Decision**: Run collection synchronously (sub-millisecond operation)

**Rationale**:
- `MachineInfoCollector.Collect()` tested execution time: <5ms on typical hardware
- Network interface enumeration is fast (local WMI call)
- Async would add complexity without benefit

**Alternatives Considered**:
- Task.Run + await: Rejected - unnecessary overhead for fast operation
- Background worker: Rejected - over-engineering

### 4. How to separate Last Run data from Current Machine data?

**Decision**: Add new `CurrentMachineInfo` property separate from `LastRunReport.MachineInfo`

**Rationale**:
- `LastRunReport.MachineInfo` captures machine state at test time (historical)
- `CurrentMachineInfo` shows current state (live)
- Clear semantic separation
- XAML bindings can choose which to display

**Alternatives Considered**:
- Overwrite LastRunReport.MachineInfo: Rejected - violates spec requirement to preserve test-time data
- Single MachineInfo property with mode flag: Rejected - complicates bindings and logic

### 5. Error handling for machine info collection failure?

**Decision**: Catch exceptions, set CurrentMachineInfo to null, display graceful empty state

**Rationale**:
- Network interface enumeration could fail in rare edge cases (permissions, virtualization)
- Null CurrentMachineInfo triggers existing empty state handling in XAML
- Log error for diagnostics

**Alternatives Considered**:
- Show error message in UI: Rejected - too intrusive for edge case
- Retry logic: Rejected - over-engineering for unlikely scenario

## Technical Findings

### Existing Code Analysis

**DiagnosticsViewModel.cs**:
- Currently uses `LastRunReport?.MachineInfo` for all machine data
- `MachineInfoSummary` computed from `LastRunReport.MachineInfo`
- `UpdateSummaries()` method sets MachineInfoSummary to null when no LastRunReport

**DiagnosticsView.xaml**:
- Network interfaces bound to `LastRunReport.MachineInfo.NetworkInterfaces`
- Machine info summary bound to `MachineInfoSummary` string property
- Empty state for network interfaces based on count == 0

**MachineInfoCollector.cs**:
- Static `Collect()` method returns complete `MachineInfo`
- Already handles network interface enumeration
- No external dependencies beyond .NET BCL

### Implementation Approach

1. Add `CurrentMachineInfo` property to `DiagnosticsViewModel`
2. Add `LoadMachineInfo()` method that calls `MachineInfoCollector.Collect()`
3. Update `MachineInfoSummary` to use `CurrentMachineInfo` instead of `LastRunReport.MachineInfo`
4. Update XAML network interfaces binding to use `CurrentMachineInfo.NetworkInterfaces`
5. Add `Loaded` event handler in `DiagnosticsView.xaml.cs` to call `LoadMachineInfo()`
6. Keep `LastRunSummary` behavior unchanged (still depends on `LastRunReport`)

## Conclusion

This is a straightforward implementation with minimal risk:
- Reuses existing `MachineInfoCollector.Collect()` without modification
- Adds ~30 lines to ViewModel
- Updates ~5 XAML bindings
- No new dependencies or architectural changes
