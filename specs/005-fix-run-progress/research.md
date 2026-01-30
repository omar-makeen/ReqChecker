# Research: Fix Run Progress View UI Bugs

**Date**: 2026-01-30
**Branch**: 005-fix-run-progress

## Root Cause Analysis

### Bug 1: Progress Ring Arc Not Updating (CRITICAL)

**Symptom**: Progress ring shows 100% text in the center, but the visual arc/border stroke remains at initial state - showing only a tiny filled portion near the top instead of a complete circle. The arc is NOT updating as progress changes.

**Screenshot Evidence**: The cyan gradient arc shows minimal fill (appears to be at ~5% visually) while the text displays "100%".

**Root Cause**: The `ProgressRing.xaml.cs:UpdateArc()` method is called but the arc geometry is not being rendered properly. Two potential issues:

1. **Property Change Not Triggering Update**: The `Progress` property binding from `RunProgressViewModel.ProgressPercentage` may not be triggering `OnProgressChanged` callback properly, or the `UpdateArc()` method is not being called when progress changes.

2. **Arc Calculation Issue**: In `UpdateArc()` (lines 162-207), when Progress reaches 100%, the arc calculation creates start and end points that are identical (both at -90 degrees after full rotation), resulting in a degenerate arc:

```csharp
// Line 177-182: At 100%, progressAngle = 360, so endRad = startRad + 2π = startRad
var progressAngle = (Progress / 100.0) * 360.0;  // 360 at 100%
var startAngle = -90.0;
var endRad = (startAngle + progressAngle) * Math.PI / 180.0;  // Same as startRad when 360°
```

3. **Initial State Bug**: The arc may be set during `OnLoaded` but never updated afterward because the binding doesn't trigger `UpdateArc()`.

**Fix Required**:
1. Verify `OnProgressChanged` callback is firing when `ProgressPercentage` changes
2. Add special case for 100% to draw a full ellipse instead of calculating arc
3. Ensure `UpdateArc()` is called every time `Progress` property changes
4. Debug: Add logging to confirm `UpdateArc()` is being invoked during test execution

---

### Bug 2: "Currently Running" Shows Stale Test Name

**Symptom**: "System File Check" displayed in "Currently Running" section when all tests completed

**Root Cause**: In `RunProgressViewModel.cs:OnTestCompleted()` (lines 141-163), `CurrentTestName` is set to the *completed* test's name, but is never cleared when all tests finish. The `StartTestsAsync()` finally block (lines 134-138) sets `IsRunning=false` and `IsComplete=true` but doesn't clear `CurrentTestName`.

**Evidence**:
```csharp
// Line 148: Sets name to COMPLETED test, not NEXT test
CurrentTestName = result.DisplayName;

// Lines 134-138: Doesn't clear CurrentTestName
finally
{
    IsRunning = false;
    IsComplete = true;
    // Missing: CurrentTestName = null;
}
```

**Fix**: Clear `CurrentTestName` when `IsComplete` is set to true, or set it to the name of the *next* test to run (if any).

---

### Bug 3: Results List Shows "Waiting for results..."

**Symptom**: Empty state visible despite "Completed Tests: 4" header

**Root Cause**: The `RunProgressView.xaml` (lines 305-324) uses a `CountToVisibilityConverter` with `ConverterParameter=Invert` for the empty state visibility. However, the `ItemsControl` (lines 241-303) and the empty state `Border` (lines 305-324) are both direct children of the same Grid row. The issue is the `ScrollViewer.Style` (lines 294-302) that collapses the ScrollViewer when count is 0 - this doesn't properly coordinate with the empty state visibility.

**Additional Issue**: The `ObservableCollection` updates happen via `Dispatcher.Invoke()`, but the visibility converter may not be notified of `Count` changes because `ObservableCollection.Count` doesn't raise `PropertyChanged`.

**Evidence**:
```xml
<!-- Line 307: Relies on Count property but ObservableCollection.Count doesn't notify -->
Visibility="{Binding TestResults.Count, Converter={StaticResource CountToVisibilityConverter}, ConverterParameter=Invert}"
```

**Fix**:
1. Add a separate `TestResultsCount` property that notifies on change
2. Or use a custom converter that binds to the collection itself
3. Or trigger refresh of empty state visibility when items are added

---

### Bug 4: Cancel Button Not Responding

**Symptom**: Clicking Cancel has no visible effect

**Root Cause**: The `CancelCommand` in `RunProgressViewModel.cs` (lines 169-173) calls `Cts?.Cancel()` which sets the cancellation token, but the test runner may have already completed by the time the user clicks Cancel (since tests run fast). Additionally, there's no visual feedback that cancellation was requested.

**Evidence**:
```csharp
// Lines 169-173: Just calls Cancel, no state update
[RelayCommand]
private void Cancel()
{
    Cts?.Cancel();
    // Missing: Visual feedback, button state change
}
```

**Fix**:
1. Disable Cancel button when `IsComplete=true` or `IsRunning=false`
2. Add visual feedback (e.g., "Cancelling..." state)
3. Handle case where tests complete before cancellation takes effect

---

### Bug 5: State Synchronization Issue

**Symptom**: Multiple UI elements show contradictory states (100% complete + "Currently Running" + empty results list)

**Root Cause**: The `OnTestCompleted` callback updates state incrementally but the "final" completion happens in the `finally` block of `StartTestsAsync`, which runs AFTER the last `OnTestCompleted` callback. The timing looks like:
1. Last test completes → OnTestCompleted sets CurrentTestName to last test
2. RunTestsAsync returns → finally block sets IsComplete=true
3. UI shows: 100% (correct) + "System File Check" (stale) + 4 completed (header) + empty list (stale)

**Evidence**: The `Progress<T>` callback can be queued on the UI thread while the finally block runs synchronously, causing race conditions in UI state.

**Fix**:
1. Batch all final state updates in a single Dispatcher.Invoke call
2. Add an explicit "completion" state update after RunTestsAsync completes
3. Clear/update CurrentTestName when IsComplete becomes true

---

## Decisions

### D1: Progress Ring Arc Update
**Decision**: Fix property binding and add special case for 100% as full ellipse
**Rationale**: The arc must update in real-time as progress changes, and 100% should show complete circle
**Implementation**:
1. Verify `OnProgressChanged` is being triggered by binding
2. Ensure `UpdateArc()` is called for every progress change
3. Add condition in `UpdateArc()` to show full ellipse at 100%
4. Consider using `Mode=TwoWay` or explicit `UpdateSourceTrigger` if needed

### D2: State Update Strategy
**Decision**: Add explicit `OnCompletion()` method that atomically updates all state
**Rationale**: Ensures UI shows consistent state by updating all properties together
**Implementation**: Call `OnCompletion()` after `RunTestsAsync` completes

### D3: Cancel Button UX
**Decision**: Disable Cancel button when not running, add "Cancelling..." intermediate state
**Rationale**: Clear affordance for button state, visual feedback on action
**Implementation**: Bind `IsEnabled` to `IsRunning`, add `IsCancelling` property

### D4: Results List Refresh
**Decision**: Add `HasResults` computed property that notifies when items added
**Rationale**: WPF binding works better with explicit boolean properties
**Implementation**: Add property, raise notification in `OnTestCompleted`

---

## Summary of Fixes Required

| Bug | File | Fix |
|-----|------|-----|
| 1 | ProgressRing.xaml.cs | Fix arc update binding, handle 100% as full ellipse |
| 1 | RunProgressView.xaml | Verify Progress binding is correct |
| 2 | RunProgressViewModel.cs | Clear CurrentTestName on completion |
| 3 | RunProgressViewModel.cs | Add HasResults property |
| 3 | RunProgressView.xaml | Bind empty state to HasResults |
| 4 | RunProgressViewModel.cs | Add IsCancelling, disable Cancel when done |
| 4 | RunProgressView.xaml | Bind Cancel IsEnabled to IsRunning |
| 5 | RunProgressViewModel.cs | Add OnCompletion() for atomic state updates |
