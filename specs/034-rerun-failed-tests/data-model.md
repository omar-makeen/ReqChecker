# Data Model: Re-run Failed Tests Only

**Feature Branch**: `034-rerun-failed-tests`
**Date**: 2026-02-07

## Entities (No New Entities)

This feature introduces no new data entities. It operates entirely on existing models:

### RunReport (existing, read-only usage)

| Field       | Type               | Usage in This Feature                          |
|-------------|--------------------|-------------------------------------------------|
| Results     | List\<TestResult\> | Iterated to find failed and dependency-skipped tests |

### TestResult (existing, read-only usage)

| Field    | Type        | Usage in This Feature                                      |
|----------|-------------|-------------------------------------------------------------|
| TestId   | string      | Collected into the re-run selection list                    |
| Status   | TestStatus  | Filtered: `Fail` → always included; `Skipped` → conditionally |
| Error    | TestError?  | Checked for `Category == ErrorCategory.Dependency`          |

### TestError (existing, read-only usage)

| Field    | Type          | Usage in This Feature                                     |
|----------|---------------|-----------------------------------------------------------|
| Category | ErrorCategory | `Dependency` → test was skipped due to a failed prerequisite |

### ErrorCategory Enum (existing)

| Value         | Meaning for This Feature                               |
|---------------|--------------------------------------------------------|
| Dependency    | Test skipped because a `dependsOn` prerequisite failed — include in re-run |
| Permission    | Admin skip — do NOT include in re-run                  |
| Configuration | Credentials skip — do NOT include in re-run            |
| Unknown       | Cancellation skip — do NOT include in re-run           |

### IAppState (existing, write usage)

| Method              | Usage in This Feature                         |
|---------------------|------------------------------------------------|
| SetSelectedTestIds  | Called with the list of failed + dependency-skipped test IDs |

## State Transitions

```
Results Page (report loaded)
  │
  ├─ All tests passed → "Re-run Failed" button hidden
  │
  └─ Has failed tests → "Re-run Failed" button visible
       │
       └─ User clicks "Re-run Failed"
            │
            ├─ Collect failed test IDs
            ├─ Collect dependency-skipped test IDs
            ├─ Store combined list in IAppState.SelectedTestIds
            └─ Navigate to RunProgress page
                 │
                 └─ (existing flow takes over)
```

## New ViewModel Properties

### ResultsViewModel (modified)

| Property/Command       | Type           | Description                                        |
|------------------------|----------------|----------------------------------------------------|
| HasFailedTests         | bool (computed)| True when Report contains at least one Fail result |
| RerunFailedTestsCommand| IRelayCommand  | Collects failed+dep-skipped IDs, sets state, navigates |
