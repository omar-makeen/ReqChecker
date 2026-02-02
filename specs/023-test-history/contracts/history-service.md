# History Service Contract

**Feature**: 023-test-history
**Type**: Internal Service Interface
**Date**: 2026-02-02

## Interface: IHistoryService

### Methods

#### LoadHistoryAsync

Loads all historical runs from disk.

```csharp
Task<List<RunReport>> LoadHistoryAsync();
```

**Behavior**:
- Returns empty list if history file doesn't exist
- Returns empty list if file is corrupted (logs warning, backs up corrupted file)
- Throws only on unrecoverable I/O errors

**Called**: App startup

---

#### SaveRunAsync

Persists a new test run to history.

```csharp
Task SaveRunAsync(RunReport report);
```

**Parameters**:
- `report`: Complete run report with all test results

**Behavior**:
- Appends to in-memory list
- Writes entire history to disk (atomic)
- Generates RunId if empty
- Sets timestamp if not set

**Called**: After test execution completes

---

#### DeleteRunAsync

Removes a specific run from history.

```csharp
Task DeleteRunAsync(string runId);
```

**Parameters**:
- `runId`: Unique identifier of run to delete

**Behavior**:
- Removes from in-memory list
- Writes updated history to disk
- No-op if runId not found (logs debug message)

**Called**: User deletes individual run

---

#### ClearHistoryAsync

Removes all historical runs.

```csharp
Task ClearHistoryAsync();
```

**Behavior**:
- Clears in-memory list
- Writes empty history to disk
- Does NOT delete the history file (preserves version/metadata)

**Called**: User confirms "Clear All History"

---

#### GetStats

Returns history storage statistics.

```csharp
HistoryStats GetStats();
```

**Returns**:
```csharp
public record HistoryStats(
    int TotalRuns,
    long FileSizeBytes,
    DateTimeOffset? OldestRun,
    DateTimeOffset? NewestRun
);
```

**Behavior**:
- Computed from in-memory data
- FileSizeBytes may be approximate

**Called**: History view displays storage info

---

## Events

#### HistoryChanged

Raised when history is modified.

```csharp
event EventHandler<HistoryChangedEventArgs>? HistoryChanged;

public record HistoryChangedEventArgs(
    HistoryChangeType ChangeType,  // Added, Deleted, Cleared
    string? AffectedRunId
);
```

**Used by**: HistoryViewModel to refresh UI

---

## Error Handling

| Error | Handling |
|-------|----------|
| File not found | Return empty, create on first save |
| JSON parse error | Log warning, backup file, return empty |
| I/O exception | Log error, surface to UI with friendly message |
| Out of disk space | Log error, notify user to clear history |

---

## Thread Safety

- All async methods are thread-safe
- In-memory list protected by lock
- File writes are atomic (write to temp, rename)

---

## File Format

**Location**: `%APPDATA%/ReqChecker/history.json`

```json
{
  "version": "1.0",
  "lastUpdated": "2026-02-02T10:30:00Z",
  "runs": [
    {
      "runId": "abc123",
      "profileId": "...",
      "profileName": "...",
      "startTime": "...",
      "endTime": "...",
      "duration": "00:01:30",
      "machineInfo": { ... },
      "results": [ ... ],
      "summary": { ... }
    }
  ]
}
```
