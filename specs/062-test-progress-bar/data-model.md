# Data Model: Test Progress Bar Enhancements

**Feature**: 062-test-progress-bar | **Date**: 2026-03-14

## Entities

### TestPositionText (computed, not stored)

A formatted string displayed near the progress ring during test execution.

| Field | Type | Description |
|-------|------|-------------|
| TestPositionText | `string` (computed) | `"Test {CurrentTestIndex + 1} of {TotalTests}"` during execution; empty when not running |

**Computation**: Derived from existing `CurrentTestIndex` (int, 0-based) and `TotalTests` (int). Updated via `OnPropertyChanged` when `CurrentTestIndex` changes.

### CompletionSummaryText (computed, not stored)

A formatted string displayed in the completion card after all tests finish.

| Field | Type | Description |
|-------|------|-------------|
| CompletionSummaryText | `string` (computed) | "All X tests passed" when no failures/skips; otherwise "X passed, Y failed, Z skipped" |

**Computation**: Derived from existing `CompletedTests`, `FailedTests`, `SkippedTests` counters. Updated when `IsComplete` changes to true.

### AutoNavigationTimer (transient, session-only)

A timer that fires once after test completion to auto-navigate to results.

| Field | Type | Description |
|-------|------|-------------|
| _autoNavTimer | `DispatcherTimer?` | Nullable; created on completion, stopped on navigation or dispose |
| Interval | 3 seconds | Fixed delay before auto-navigation |

**Lifecycle**:
1. **Created**: In `OnCompletion()` when `IsCancelling` is false and `RunReport` is not null
2. **Fires**: After 3 seconds, calls `ViewResults()` then stops itself
3. **Cancelled**: When user clicks "View Results" or "Back to Tests", or ViewModel is disposed
4. **Not created**: When test run was cancelled by user

## State Transitions

```text
                    ┌──────────────────┐
                    │   Tests Running  │
                    │  TestPositionText│
                    │  = "Test X of Y" │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              │                             │
         All complete                  User cancels
              │                             │
              ▼                             ▼
    ┌─────────────────┐          ┌─────────────────┐
    │   Complete       │          │   Cancelled      │
    │ CompletionSummary│          │ No auto-nav      │
    │ Timer starts (3s)│          │ Manual nav only   │
    └────────┬─────────┘          └──────────────────┘
             │
    ┌────────┼────────────┐
    │        │            │
Timer fires  Click       Click
    │     "Results"   "Back"
    │        │            │
    ▼        ▼            ▼
 Results   Results     Test List
 (timer    (timer      (timer
  stops)    stops)      stops)
```

## No Persistence

This feature adds no persistent data. All state is session-scoped and lives in `RunProgressViewModel` instance memory. The timer is garbage-collected when the ViewModel is disposed.
