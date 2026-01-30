# Data Model: Test Progress Delay

**Feature**: 006-test-progress-delay
**Date**: 2026-01-31

## Entity Extensions

This feature extends existing entities rather than creating new ones.

### 1. UserPreferences (extend existing)

**File**: `src/ReqChecker.App/Services/PreferencesService.cs` (nested class)

**Current State**:
```csharp
public class UserPreferences
{
    public string Theme { get; set; } = "Dark";
    public bool SidebarExpanded { get; set; } = true;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
```

**Extended State**:
```csharp
public class UserPreferences
{
    public string Theme { get; set; } = "Dark";
    public bool SidebarExpanded { get; set; } = true;
    public bool TestProgressDelayEnabled { get; set; } = true;      // NEW
    public int TestProgressDelayMs { get; set; } = 500;             // NEW
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
```

| Field | Type | Default | Constraints | Description |
|-------|------|---------|-------------|-------------|
| `TestProgressDelayEnabled` | `bool` | `true` | - | Whether inter-test delay is active |
| `TestProgressDelayMs` | `int` | `500` | 0-3000 | Delay duration in milliseconds |

**Validation Rules**:
- `TestProgressDelayMs` clamped to range [0, 3000] on load
- If value outside range in JSON, reset to default (500)

**JSON Representation**:
```json
{
  "theme": "Dark",
  "sidebarExpanded": true,
  "testProgressDelayEnabled": true,
  "testProgressDelayMs": 500,
  "lastUpdated": "2026-01-31T12:00:00Z"
}
```

---

### 2. RunSettings (extend existing)

**File**: `src/ReqChecker.Core/Models/RunSettings.cs`

**Current State**:
```csharp
public class RunSettings
{
    public int DefaultTimeout { get; set; } = 30000;
    public int DefaultRetryCount { get; set; } = 0;
    public int RetryDelayMs { get; set; } = 5000;
    public BackoffStrategy RetryBackoff { get; set; } = BackoffStrategy.None;
}
```

**Extended State**:
```csharp
public class RunSettings
{
    public int DefaultTimeout { get; set; } = 30000;
    public int DefaultRetryCount { get; set; } = 0;
    public int RetryDelayMs { get; set; } = 5000;
    public BackoffStrategy RetryBackoff { get; set; } = BackoffStrategy.None;
    public int InterTestDelayMs { get; set; } = 0;                  // NEW
}
```

| Field | Type | Default | Constraints | Description |
|-------|------|---------|-------------|-------------|
| `InterTestDelayMs` | `int` | `0` | ≥0 | Delay between test completions (0 = disabled) |

**Usage Pattern**:
- Default is 0 (no delay) in model
- ViewModel sets this from preferences when starting a run
- If `TestProgressDelayEnabled` is false, pass 0
- If `TestProgressDelayEnabled` is true, pass `TestProgressDelayMs`

---

### 3. PreferencesService (extend existing)

**File**: `src/ReqChecker.App/Services/PreferencesService.cs`

**New Observable Properties**:
```csharp
[ObservableProperty]
private bool _testProgressDelayEnabled = true;

[ObservableProperty]
private int _testProgressDelayMs = 500;
```

**Auto-Save Handlers**:
```csharp
partial void OnTestProgressDelayEnabledChanged(bool value) => Save();
partial void OnTestProgressDelayMsChanged(int value) => Save();
```

**Property Binding**:
- `TestProgressDelayEnabled` ↔ `UserPreferences.TestProgressDelayEnabled`
- `TestProgressDelayMs` ↔ `UserPreferences.TestProgressDelayMs`

---

## State Transitions

### Delay Setting State

```
                    ┌─────────────────┐
                    │   Enabled       │
                    │   500ms default │
                    └────────┬────────┘
                             │ User adjusts
                             ▼
    ┌────────────────────────┴────────────────────────┐
    │                                                  │
    ▼                                                  ▼
┌───────────┐                                    ┌───────────┐
│  Enabled  │◄──────── Toggle ON ────────────────│ Disabled  │
│ 0-3000ms  │                                    │    0ms    │
└───────────┘─────────── Toggle OFF ────────────►└───────────┘
```

### Test Execution State (with delay)

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Executing  │────►│  Reporting  │────►│   Delaying  │
│   Test N    │     │  Progress   │     │  (if N<last)│
└─────────────┘     └─────────────┘     └──────┬──────┘
                                               │
                         ┌─────────────────────┘
                         │
                         ▼
                    ┌─────────────┐
                    │  Executing  │
                    │   Test N+1  │
                    └─────────────┘
```

**Cancellation During Delay**:
```
┌─────────────┐     Cancel Request
│   Delaying  │─────────────────────►  OperationCanceledException
└─────────────┘     (< 200ms)                     │
                                                  ▼
                                           ┌─────────────┐
                                           │  Cancelled  │
                                           └─────────────┘
```

---

## Relationships

```
UserPreferences ──────────────────────► PreferencesService
     │                                        │
     │ persisted to                           │ exposes via
     │                                        │
     ▼                                        ▼
preferences.json              RunProgressViewModel (binds properties)
                                              │
                                              │ passes value to
                                              ▼
                                        RunSettings.InterTestDelayMs
                                              │
                                              │ consumed by
                                              ▼
                                    SequentialTestRunner.RunTestsAsync()
```

---

## Backward Compatibility

**Scenario**: Existing `preferences.json` without delay fields

**Behavior**:
- `System.Text.Json` ignores missing fields
- Properties retain default values (`true`, `500`)
- Next save adds fields to JSON

**No Migration Required**: Default values match expected behavior.
