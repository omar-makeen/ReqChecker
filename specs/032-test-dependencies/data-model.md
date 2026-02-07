# Data Model: Test Dependencies / Skip-on-Fail

**Feature**: 032-test-dependencies
**Date**: 2026-02-07

## Entity Changes

### TestDefinition (Modified)

**File**: `src/ReqChecker.Core/Models/TestDefinition.cs`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Id | string | `""` | *Existing* — Unique identifier within profile |
| Type | string | `""` | *Existing* — Test type identifier |
| DisplayName | string | `""` | *Existing* — User-facing name |
| Description | string? | `null` | *Existing* — Optional help text |
| Parameters | JsonObject | `new()` | *Existing* — Type-specific parameters |
| FieldPolicy | Dictionary | `new()` | *Existing* — Per-field editability |
| Timeout | int? | `null` | *Existing* — Override timeout (ms) |
| RetryCount | int? | `null` | *Existing* — Override retry count |
| RequiresAdmin | bool | `false` | *Existing* — Needs elevation |
| **DependsOn** | **List\<string\>** | **new()** | **NEW** — IDs of prerequisite tests that must pass before this test runs. Empty list = no dependencies. |

**Validation rules**:
- Each ID in `DependsOn` must reference an existing test ID in the same profile
- No circular dependency chains allowed
- Empty list and null are both valid (no dependencies)

**JSON representation**:
```json
{
  "id": "test-002",
  "type": "HttpGet",
  "displayName": "HTTPS Connectivity Check",
  "dependsOn": ["test-001"],
  ...
}
```

### ErrorCategory (Modified Enum)

**File**: `src/ReqChecker.Core/Enums/ErrorCategory.cs`

| Value | Description |
|-------|-------------|
| Network | *Existing* — Connection, DNS, socket errors |
| Timeout | *Existing* — Operation timed out |
| Permission | *Existing* — Access denied, elevation required |
| Validation | *Existing* — Unexpected response, assertion failed |
| Configuration | *Existing* — Invalid parameters or configuration |
| Unknown | *Existing* — Unclassified error |
| **Dependency** | **NEW** — Test skipped because a prerequisite test failed or was skipped |

### TestResult (Unchanged)

No structural changes. Dependency skips are expressed using existing fields:
- `Status` = `TestStatus.Skipped`
- `Error.Category` = `ErrorCategory.Dependency`
- `Error.Message` = Human-readable reason naming the failed prerequisite
- `HumanSummary` = Same message for UI display
- `Duration` = `TimeSpan.Zero` (test was not executed)
- `AttemptCount` = `0` (no execution attempted)

### Profile (Unchanged)

The `SchemaVersion` field will be bumped from 2 to 3 by migration, but the Profile model itself needs no changes.

## Schema Migration: V2 → V3

**File**: `src/ReqChecker.Infrastructure/Profile/Migrations/V2ToV3Migration.cs` (new)

**Purpose**: Ensures all TestDefinitions in v2 profiles have an explicit `DependsOn` property set to an empty list.

**Migration logic**:
1. For each test in `profile.Tests`: if `DependsOn` is null, set it to `new List<string>()`
2. Set `profile.SchemaVersion = 3`

**Pipeline update**: `ProfileMigrationPipeline.CurrentSchemaVersion` bumped from 2 to 3.

## Relationships

```text
Profile (1) ──has──▶ (many) TestDefinition
TestDefinition (1) ──dependsOn──▶ (0..many) TestDefinition  [by ID reference]
TestDefinition (1) ──produces──▶ (1) TestResult
TestResult.Error.Category ──may be──▶ ErrorCategory.Dependency
```

## State Transitions

### Test Execution with Dependencies

```text
For each test in profile order:
  ┌─ Has DependsOn entries?
  │   No  → Execute normally
  │   Yes → Check each prerequisite in completed results
  │         ┌─ All prerequisites Pass? → Execute normally
  │         ├─ Any prerequisite Fail/Skip? → Skip (Dependency reason)
  │         └─ Any prerequisite not yet executed? → Skip (not yet executed reason)
  └─ Record result in completed results dictionary
```
