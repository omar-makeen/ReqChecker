# Data Model: Premium Test Execution Page

**Branch**: `020-premium-test-execution` | **Date**: 2026-02-02

## Overview

This is a UI-only feature with no new data model requirements. All data structures already exist.

## Existing Entities Used

### TestResult (existing)

**Location**: `ReqChecker.Core/Models/TestResult.cs`

| Property | Type | Description |
|----------|------|-------------|
| TestId | string | Test definition ID |
| TestType | string | Test type identifier |
| DisplayName | string | User-facing test name |
| Status | TestStatus | Pass, Fail, or Skipped |
| StartTime | DateTimeOffset | Test start timestamp |
| EndTime | DateTimeOffset | Test end timestamp |
| Duration | TimeSpan | Execution time |
| HumanSummary | string | User-friendly result message |

### TestStatus (existing enum)

**Location**: `ReqChecker.Core/Enums/TestStatus.cs`

| Value | Description |
|-------|-------------|
| Pass | Test completed successfully |
| Fail | Test failed |
| Skipped | Test was skipped |

## ViewModel Additions

### RunProgressViewModel

**New computed property**:

```csharp
public string HeaderSubtitle => IsComplete
    ? $"{TotalTests} tests completed"
    : IsRunning
        ? $"Running {CurrentTestIndex + 1} of {TotalTests} tests"
        : "Ready to run";
```

**Property change notifications** (add to existing partial methods):
- `OnIsCompleteChanged` → notify `HeaderSubtitle`
- `OnIsRunningChanged` → notify `HeaderSubtitle`
- `OnCurrentTestIndexChanged` → notify `HeaderSubtitle`
- `OnTotalTestsChanged` → notify `HeaderSubtitle`

## No New Entities Required

This feature reuses existing data structures and only adds:
1. One computed property in ViewModel
2. XAML changes for visual presentation
