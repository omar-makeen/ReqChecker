# Data Model: Subtle Chip Badge

**Feature**: 064-subtle-chip-badge
**Date**: 2026-03-14

## No Data Model Changes

This feature is a visual-only restyling of existing XAML elements. No new entities, properties, or data flows are introduced.

### Existing Properties (unchanged)

| ViewModel | Property | Type | Description |
|-----------|----------|------|-------------|
| `MainViewModel` | `TestCount` | `int` | Number of tests in current profile |
| `MainViewModel` | `HasTests` | `bool` | Whether TestCount > 0 |
| `TestListViewModel` | `TestCountDisplay` | `string` | Formatted display string (e.g., "8 tests") |

All bindings remain unchanged. Only the visual presentation (XAML styling properties) of the bound values changes.
