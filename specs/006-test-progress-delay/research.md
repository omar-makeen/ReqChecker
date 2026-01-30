# Research: Test Progress Delay for User Visibility

**Feature**: 006-test-progress-delay
**Date**: 2026-01-31

## Research Tasks

### 1. Where to Insert Delay in Test Execution Flow

**Decision**: Insert delay in `SequentialTestRunner.cs` after `progress?.Report(testResult)`

**Rationale**:
- Same location as existing retry delays in `RetryPolicy.cs`
- Delay occurs after test completes but before next test starts
- Uses same async/cancellation pattern already proven in codebase
- Allows UI to fully update before delay starts

**Alternatives Considered**:
- **ViewModel callback (OnTestCompleted)**: Rejected - would delay UI thread dispatch, causing jank
- **Before test execution**: Rejected - spec says "no delay before first test"
- **In RetryPolicy**: Rejected - that's for retry attempts, not inter-test delays

**Code Location**: `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs` around line 115

### 2. Cancellation Pattern for Immediate Response

**Decision**: Pass existing `CancellationToken` to `Task.Delay()`

**Rationale**:
- `Task.Delay(delay, cancellationToken)` immediately throws `OperationCanceledException` when token is cancelled
- This is the standard .NET pattern, already used in `RetryPolicy.cs`
- Guarantees <200ms cancellation response (per SC-004)

**Alternatives Considered**:
- **Manual polling loop**: Rejected - more complex, no benefit
- **Timer with manual cancellation**: Rejected - `Task.Delay` already handles this

**Implementation**:
```csharp
if (runSettings.InterTestDelayMs > 0 && i < tests.Count - 1)
{
    await Task.Delay(runSettings.InterTestDelayMs, cancellationToken);
}
```

### 3. UI Control Components

**Decision**: Use WPF-UI `ToggleSwitch` + standard WPF `Slider`

**Rationale**:
- WPF-UI 4.2.0 provides modern `ToggleSwitch` control matching app theme
- Standard WPF `Slider` works well for continuous value (0-3000ms)
- Both support data binding to ViewModel properties
- Existing app already uses these control types

**Alternatives Considered**:
- **NumberBox**: Rejected - slider provides better UX for range selection
- **ComboBox with presets (250/500/1000/2000)**: Rejected - less flexible than slider
- **Settings page only**: Rejected - clarification specified inline on Run Progress view

**UI Layout**:
```
[ToggleSwitch: Demo Mode]  [Slider ●━━━━━━━━━━━━━━]  [500 ms]
```

### 4. Persistence Mechanism

**Decision**: Extend existing `PreferencesService` and `UserPreferences` class

**Rationale**:
- Follows established pattern in codebase
- Auto-save via MVVM partial methods already working
- Uses existing `%APPDATA%/ReqChecker/preferences.json` location
- No new dependencies or infrastructure needed

**Alternatives Considered**:
- **Separate settings file**: Rejected - adds complexity, existing pattern works
- **Registry**: Rejected - not portable, existing file-based approach is better
- **In-memory only**: Rejected - spec requires persistence (FR-006)

**Data Shape**:
```json
{
  "theme": "Dark",
  "sidebarExpanded": true,
  "testProgressDelayEnabled": true,
  "testProgressDelayMs": 500,
  "lastUpdated": "2026-01-31T10:00:00Z"
}
```

### 5. Default Values

**Decision**:
- `TestProgressDelayEnabled`: `true` (default ON)
- `TestProgressDelayMs`: `500` (half second)

**Rationale**:
- 500ms is the minimum readable duration specified in spec (acceptance scenario 1)
- Default ON because the feature was requested specifically to solve visibility issue
- Users can disable if they prefer fast execution

**Alternatives Considered**:
- **Default OFF**: Rejected - defeats purpose of feature; user explicitly asked for delay
- **1000ms default**: Rejected - 500ms sufficient for readability, 1000ms feels slow
- **250ms default**: Rejected - too fast to read test names reliably

### 6. Visual Feedback During Delay

**Decision**: Keep current test visible in "Currently Running" section during delay

**Rationale**:
- Current test name already displays in UI
- No additional visual indicator needed - the pause itself is the feedback
- Simpler implementation, no UI changes beyond controls

**Alternatives Considered**:
- **Progress bar during delay**: Rejected - adds complexity, minimal value
- **"Pausing..." text**: Rejected - confusing, test already completed
- **Countdown timer**: Rejected - over-engineering for 0.5-3 second delay

## Summary

All research questions resolved. Key findings:

| Aspect | Decision | Key Reason |
|--------|----------|------------|
| Delay location | After `progress?.Report()` in SequentialTestRunner | Follows existing retry pattern |
| Cancellation | `Task.Delay(ms, token)` | Standard .NET, immediate response |
| UI controls | ToggleSwitch + Slider inline | WPF-UI consistency, per clarification |
| Persistence | Extend PreferencesService | Existing pattern, auto-save |
| Defaults | Enabled, 500ms | Spec requirement, readable duration |
| Feedback | Current test remains visible | Simple, effective |

No unknowns remain. Ready for implementation.
