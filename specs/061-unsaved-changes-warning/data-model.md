# Data Model: Unsaved Changes Warning

**Feature**: 061-unsaved-changes-warning | **Date**: 2026-03-14

## Entities

### FieldBaseline (in-memory, session-only)

A snapshot of editable field values captured when TestConfigViewModel initializes or after a successful save.

| Field | Type | Description |
|-------|------|-------------|
| Values | `Dictionary<string, string?>` | Map of field name → string value at capture time |

**Keys**:
- `"Timeout"` → `Timeout?.ToString()` (nullable int as string)
- `"RetryCount"` → `RetryCount?.ToString()` (nullable int as string)
- `"{ParameterName}"` → `TestParameterViewModel.Value` for each editable/password parameter

**Lifecycle**:
1. **Created**: When `TestConfigViewModel` initializes parameters (after `InitializeParameters()`)
2. **Updated**: After successful `SaveAsync()` — re-captured from current values
3. **Destroyed**: When ViewModel is disposed (navigated away)

### HasUnsavedChanges (computed, not stored)

| Field | Type | Description |
|-------|------|-------------|
| HasUnsavedChanges | `bool` (computed) | `true` if any current value differs from its baseline counterpart |

**Computation**: Iterates `_baseline.Values` dictionary. For each key, compares baseline value against current value using string equality. Returns `true` on first mismatch.

## State Transitions

```text
                    ┌──────────────────┐
                    │   Page Loads     │
                    │  (baseline = ∅)  │
                    └────────┬─────────┘
                             │
                    InitializeParameters()
                             │
                             ▼
                    ┌──────────────────┐
                    │     Clean        │◄──────── SaveAsync() succeeds
                    │ HasUnsavedChanges│         (re-capture baseline)
                    │    = false       │
                    └────────┬─────────┘
                             │
                     User edits a field
                     (value ≠ baseline)
                             │
                             ▼
                    ┌──────────────────┐
                    │     Dirty        │
                    │ HasUnsavedChanges│
                    │    = true        │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
         User reverts   User clicks    User clicks
         all changes      Save           Back
              │              │              │
              ▼              ▼              ▼
           Clean          Clean       Show Dialog
         (no dialog)   (baseline     ┌─────┴─────┐
                        updated)     │           │
                                  Discard      Stay
                                     │           │
                                  GoBack()    Close dialog
                                  (clean)    (remain dirty)
```

## No Persistence

This feature adds no persistent data. All state is session-scoped and lives in `TestConfigViewModel` instance memory. The baseline dictionary is garbage-collected when the ViewModel is disposed.
