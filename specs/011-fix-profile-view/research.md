# Research: Fix Profile Selector View

**Feature**: 011-fix-profile-view
**Date**: 2026-01-31

## Problem Analysis

### Root Cause

The Profile Selector view crashes due to a WPF data binding error:

```
Error: A TwoWay or OneWayToSource binding cannot work on the read-only property 'Count'
of type 'System.Collections.Generic.List`1[ReqChecker.Core.Models.TestDefinition]'.
```

### Technical Explanation

1. **WPF `Run.Text` binding default**: `Run.Text` dependency property uses `TwoWay` binding mode by default
2. **Read-only property**: `List<T>.Count` is a read-only property (no setter)
3. **Conflict**: TwoWay binding attempts to write back to source, which fails on read-only properties
4. **Cascade**: Each navigation attempt triggers the error, causing multiple error dialogs

## Solution

### Decision

Add `Mode=OneWay` to all `Run.Text` bindings that reference read-only collection properties.

### Rationale

- `OneWay` binding only reads from source, never writes back
- Minimal change with zero risk to existing functionality
- Standard WPF pattern for read-only property bindings

### Alternatives Rejected

| Alternative | Reason Rejected |
|-------------|-----------------|
| Use TextBlock instead of Run | Breaks inline text layout (Run is needed inside TextBlock for multiple text segments) |
| Create ViewModel wrapper property | Over-engineering for a simple binding mode fix |
| Use StringFormat on parent TextBlock | Doesn't work with Run elements |

## Bindings to Fix

| Location | Binding | Fix |
|----------|---------|-----|
| Line 214 | `{Binding Tests.Count}` | Add `Mode=OneWay` |
| Line 229 | `{Binding SchemaVersion}` | Add `Mode=OneWay` (preventive) |

Note: Lines 136 and 250 use converters which handle the binding direction, but could be made explicit with `Mode=OneWay` for consistency.

## References

- [WPF Binding Mode Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/data/data-binding-overview)
- Default binding modes by property type in WPF
