# Data Model: Test History Feature

**Feature**: 023-test-history
**Date**: 2026-02-02

## Entities

### 1. HistoryStore (New)

Represents the persisted history collection.

```
HistoryStore
├── Version: string              # Schema version for future migrations
├── LastUpdated: DateTimeOffset  # Last modification timestamp
└── Runs: List<RunReport>        # All historical runs (reuses existing model)
```

**File Location**: `%APPDATA%/ReqChecker/history.json`

**JSON Schema**:
```json
{
  "version": "1.0",
  "lastUpdated": "2026-02-02T10:30:00Z",
  "runs": [
    { /* RunReport object */ }
  ]
}
```

### 2. RunReport (Existing - Reused)

Already defined in `ReqChecker.Core.Models.RunReport`. No changes needed.

```
RunReport
├── RunId: string               # Unique identifier (GUID)
├── ProfileId: string           # Source profile ID
├── ProfileName: string         # Source profile name (preserved even if profile deleted)
├── StartTime: DateTimeOffset   # Run start timestamp
├── EndTime: DateTimeOffset     # Run end timestamp
├── Duration: TimeSpan          # Total execution time
├── MachineInfo: MachineInfo    # Environment details
├── Results: List<TestResult>   # Individual test results
└── Summary: RunSummary         # Aggregate statistics
```

### 3. TestResult (Existing - Reused)

Already defined in `ReqChecker.Core.Models.TestResult`. No changes needed.

```
TestResult
├── TestId: string          # Unique test identifier
├── DisplayName: string     # Human-readable name
├── TestType: string        # Test category
├── Status: TestStatus      # Pass/Fail/Skipped
├── Duration: TimeSpan      # Execution time
├── AttemptCount: int       # Retry attempts
└── Error: TestError?       # Error details if failed
```

### 4. TestTrendData (New - Computed)

Computed at runtime for flaky test detection. Not persisted.

```
TestTrendData
├── TestId: string              # Test identifier
├── TestName: string            # Display name
├── ProfileId: string           # Profile scope
├── TotalRuns: int              # Number of runs analyzed
├── PassCount: int              # Times passed
├── FailCount: int              # Times failed
├── SkipCount: int              # Times skipped
├── PassRate: double            # PassCount / (TotalRuns - SkipCount)
├── IsFlaky: bool               # Has both pass and fail results
└── RecentResults: List<TestStatus>  # Last N results for display
```

## Relationships

```
┌─────────────────┐
│  HistoryStore   │
│  (history.json) │
└────────┬────────┘
         │ contains many
         ▼
┌─────────────────┐
│   RunReport     │
│   (existing)    │
└────────┬────────┘
         │ contains many
         ▼
┌─────────────────┐
│   TestResult    │
│   (existing)    │
└─────────────────┘
         │
         │ aggregated into
         ▼
┌─────────────────┐
│  TestTrendData  │
│   (computed)    │
└─────────────────┘
```

## Validation Rules

### HistoryStore
- Version must be a valid semantic version string
- Runs list can be empty but not null
- LastUpdated must be valid UTC timestamp

### RunReport (for history)
- RunId must be unique across all stored runs
- StartTime must be before EndTime
- ProfileName must be preserved (not null/empty) even if profile deleted

### TestTrendData
- TotalRuns must be > 0 for meaningful analysis
- IsFlaky = true only when PassCount > 0 AND FailCount > 0

## State Transitions

### History Lifecycle

```
[Empty] ──(first test run)──► [Has Runs]
   │                              │
   │                              ├──(run tests)──► [More Runs]
   │                              │
   │                              ├──(delete run)──► [Fewer Runs or Empty]
   │                              │
   └──────────────────────────────┴──(clear all)──► [Empty]
```

### Storage Operations

| Operation | Trigger | Result |
|-----------|---------|--------|
| Load | App startup | Read history.json into memory |
| Save | After test run completes | Append run, write to file |
| Delete | User deletes single run | Remove from list, write to file |
| Clear | User clears all history | Empty list, write to file |

## Data Volume Estimates

| Scenario | Runs | Est. File Size | Load Time |
|----------|------|----------------|-----------|
| Light use | 10 | ~100 KB | < 100ms |
| Normal use | 100 | ~1 MB | < 500ms |
| Heavy use | 500 | ~5 MB | < 2s |
| Maximum | 1000+ | ~10 MB+ | Consider pagination |

Assumptions:
- Average 50 tests per profile
- ~2 KB per test result (including error details)
- JSON overhead ~20%
