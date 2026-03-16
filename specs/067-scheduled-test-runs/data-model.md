# Data Model: Scheduled Test Runs

**Feature Branch**: `067-scheduled-test-runs`
**Date**: 2026-03-16

## Entities

### Schedule

Represents a user-defined scheduled test run (one-time or recurring).

| Field             | Type                | Description                                                    | Constraints                            |
|-------------------|---------------------|----------------------------------------------------------------|----------------------------------------|
| Id                | string (GUID)       | Unique identifier                                              | Auto-generated on creation             |
| Name              | string              | User-given descriptive name                                    | Required, non-empty                    |
| ProfileFilePath   | string              | Absolute path to the profile JSON file                         | Required, must exist at creation time  |
| ProfileName       | string              | Display name of the profile (denormalized for UI)              | Captured at creation time              |
| ScheduleType      | ScheduleType (enum) | OneTime or Recurring                                           | Required                               |
| ScheduledTime     | DateTime            | For one-time: the exact run time. For recurring: first run time | Must be in the future at creation      |
| Recurrence        | RecurrencePattern?  | Recurrence configuration (null for one-time)                   | Required if ScheduleType == Recurring  |
| Status            | ScheduleStatus      | Current lifecycle state                                        | Default: Active                        |
| NextRunTime       | DateTime?           | Computed next execution time                                   | Null when Completed/Expired/Paused     |
| LastRunTime       | DateTime?           | When the schedule last executed                                | Null if never run                      |
| LastRunOutcome    | ScheduleOutcome?    | Outcome of the most recent execution                           | Null if never run                      |
| CreatedAt         | DateTime            | When the schedule was created                                  | Auto-set, UTC                          |
| UpdatedAt         | DateTime            | When the schedule was last modified                            | Auto-set, UTC                          |

### RecurrencePattern

Defines the repeat behavior for recurring schedules.

| Field           | Type                  | Description                                            | Constraints                            |
|-----------------|-----------------------|--------------------------------------------------------|----------------------------------------|
| FrequencyType   | RecurrenceType (enum) | Hourly, Daily, Weekly, CustomInterval                  | Required                               |
| IntervalValue   | int                   | Repeat every N units (e.g., every 2 hours)             | >= 1; for CustomInterval minutes >= 5  |
| IntervalUnit    | IntervalUnit? (enum)  | Minutes, Hours, Days (for CustomInterval only)         | Required if FrequencyType == Custom    |
| DaysOfWeek      | List\<DayOfWeek\>?    | Selected days (for Weekly only)                        | Required if FrequencyType == Weekly    |
| TimeOfDay       | TimeSpan              | Time of day to run (for Daily/Weekly)                  | Required for Daily/Weekly              |
| EndDate         | DateTime?             | Optional end date after which schedule expires          | Must be after ScheduledTime if set     |

### ScheduleExecutionRecord

Links a schedule to a specific run in history.

| Field            | Type                    | Description                                     | Constraints           |
|------------------|-------------------------|-------------------------------------------------|-----------------------|
| Id               | string (GUID)           | Unique identifier                               | Auto-generated        |
| ScheduleId       | string                  | Reference to parent Schedule                    | Required              |
| ExecutionTime    | DateTime                | When execution started                          | Auto-set, UTC         |
| Outcome          | ScheduleOutcome (enum)  | Completed, Skipped, Missed, Failed              | Required              |
| HistoryRunId     | string?                 | Reference to RunReport in history               | Set on Completed      |
| ErrorMessage     | string?                 | Error details if Failed                         | Set on Failed         |

## Enums

### ScheduleType
- `OneTime` — Runs once at the specified time
- `Recurring` — Repeats according to the recurrence pattern

### ScheduleStatus
- `Active` — Scheduled and waiting for next run
- `Paused` — User-paused, will not execute until resumed
- `Completed` — One-time schedule that has finished executing
- `Expired` — Recurring schedule past its end date
- `Missed` — Schedule was due but app was not running (transient, resets on next check)

### RecurrenceType
- `Hourly` — Every N hours (IntervalValue = N)
- `Daily` — Every day at TimeOfDay
- `Weekly` — On selected DaysOfWeek at TimeOfDay
- `CustomInterval` — Every N minutes/hours/days (uses IntervalUnit)

### IntervalUnit
- `Minutes`
- `Hours`
- `Days`

### ScheduleOutcome
- `Completed` — Tests ran successfully (may include test failures)
- `Skipped` — Skipped due to overlapping execution
- `Missed` — App was not running at scheduled time
- `Failed` — Execution error (profile not found, unhandled exception)

## State Transitions

```
Schedule Status Lifecycle:

  [Created] → Active
  Active → Paused       (user pauses)
  Paused → Active       (user resumes)
  Active → Completed    (one-time run finishes)
  Active → Expired      (recurring end date passes)
  Active → Active       (recurring run completes, next run calculated)
  Active ↔ Missed       (app was closed during scheduled time; resets to Active on launch)
  Any → [Deleted]       (user deletes)
```

## Persistence Format

**File**: `%APPDATA%/ReqChecker/schedules.json`

```json
{
  "version": 1,
  "schedules": [
    {
      "id": "guid-here",
      "name": "Nightly Production Check",
      "profileFilePath": "C:\\profiles\\production.json",
      "profileName": "Production",
      "scheduleType": "Recurring",
      "scheduledTime": "2026-03-16T22:00:00",
      "recurrence": {
        "frequencyType": "Daily",
        "intervalValue": 1,
        "timeOfDay": "22:00:00",
        "endDate": null
      },
      "status": "Active",
      "nextRunTime": "2026-03-17T22:00:00",
      "lastRunTime": "2026-03-16T22:00:00",
      "lastRunOutcome": "Completed",
      "createdAt": "2026-03-15T10:00:00Z",
      "updatedAt": "2026-03-16T22:01:30Z"
    }
  ],
  "executionRecords": [
    {
      "id": "guid-here",
      "scheduleId": "parent-guid",
      "executionTime": "2026-03-16T22:00:00Z",
      "outcome": "Completed",
      "historyRunId": "run-guid-in-history",
      "errorMessage": null
    }
  ]
}
```

## Relationships

```
Schedule 1──* ScheduleExecutionRecord
Schedule 1──1 RecurrencePattern (embedded, nullable)
ScheduleExecutionRecord *──0..1 RunReport (via historyRunId reference)
Schedule *──1 Profile (via profileFilePath reference)
```
