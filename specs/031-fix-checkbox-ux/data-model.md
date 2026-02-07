# Data Model: Fix Checkbox Visibility and Select All Placement

**Feature**: 031-fix-checkbox-ux
**Date**: 2026-02-07

## No Data Model Changes

This feature is a pure UI/visual fix. No entities, state transitions, persistence, or data structures are added or modified.

**Existing entities used (unchanged)**:
- `SelectableTestItem` — wraps `TestDefinition` with `IsSelected` bool (used by checkboxes)
- `TestListViewModel` — exposes `IsAllSelected`, `HasSelectedTests`, `SelectableTests` (used by Select All)

Both remain identical. Only their XAML visual representation changes.
