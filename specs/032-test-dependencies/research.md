# Research: Test Dependencies / Skip-on-Fail

**Feature**: 032-test-dependencies
**Date**: 2026-02-07

## Research Summary

No NEEDS CLARIFICATION items existed in the Technical Context. All technology choices are established by the existing codebase. Research focused on best practices for the implementation approach.

## R1: Dependency Graph Validation (Cycle Detection)

**Decision**: Use iterative DFS-based cycle detection on the dependency graph at profile load time.

**Rationale**: The profile's test list forms a directed graph where each `dependsOn` entry is an edge. DFS with a "visiting" set detects back-edges (cycles) in O(V+E) time. For profiles with 5–30 tests, this is instantaneous. No external library needed — straightforward to implement with a HashSet-based visited/visiting pattern.

**Alternatives considered**:
- Topological sort (Kahn's algorithm): Would also detect cycles, but the runner doesn't need to reorder tests (per clarification — out-of-order dependents are skipped, not reordered). DFS is simpler for validation-only.
- Third-party graph library: Overkill for the scale; adds an unnecessary dependency.

## R2: Dependency Check During Execution

**Decision**: Maintain a `Dictionary<string, TestStatus>` of completed test results indexed by test ID. Before executing each test, check its `DependsOn` list against this dictionary. If any prerequisite is not `Pass`, skip the test.

**Rationale**: The runner already processes tests sequentially. Adding a dictionary lookup before each test is O(k) where k = number of dependencies (typically 1). This fits naturally into the existing `for` loop in `SequentialTestRunner.RunTestsAsync` without restructuring.

**Alternatives considered**:
- Topological reordering before execution: Rejected per clarification — the system must not reorder tests. Out-of-order dependents are skipped with a reason.
- Separate dependency resolver service: Unnecessary indirection; the logic is simple enough to live in the runner.

## R3: Schema Migration (v2 → v3)

**Decision**: Create a `V2ToV3Migration` that adds empty `DependsOn` lists to existing tests. Bump `CurrentSchemaVersion` to 3 in `ProfileMigrationPipeline`.

**Rationale**: Existing profiles (schema v1 and v2) have no `dependsOn` field. System.Text.Json deserialization will default `List<string>` to `null` for missing fields; the migration explicitly sets it to an empty list for clarity. The embedded `default-profile.json` stays at schema v1 (it's migrated at runtime) but gets `dependsOn` fields added.

**Alternatives considered**:
- No migration, rely on JSON deserializer defaults: Risky — `null` vs empty list creates ambiguity. Better to migrate explicitly.
- Bump to v3 in the embedded profile: Not needed — the migration pipeline handles this at runtime.

## R4: ErrorCategory Extension

**Decision**: Add `Dependency` value to the `ErrorCategory` enum for dependency-skipped tests.

**Rationale**: FR-010 requires distinguishing dependency skips from other skip reasons (Permission, Configuration). A dedicated enum value makes filtering and display straightforward. The existing pattern uses `ErrorCategory` on `TestError` for all skip reasons.

**Alternatives considered**:
- Reuse `Configuration` category: Would conflate dependency skips with config errors, violating FR-010.
- Add a separate `SkipReason` property to `TestResult`: Over-engineered; the existing `TestError.Category` + `TestError.Message` pattern already handles this.

## R5: UI Dependency Indicator

**Decision**: Add a small text label below the test description in the test card showing "Depends on: {prerequisite display name(s)}". Use existing `TextCaption` style with a link icon.

**Rationale**: The test card already has conditional elements (RequiresAdmin indicator, type badge). Adding another conditional line follows the established pattern. Using prerequisite display names (not IDs) makes it human-readable per FR-008.

**Alternatives considered**:
- Tooltip-only: Too hidden; users won't discover it.
- Separate "dependencies" column: The card layout is single-column; adding a column would break the design.

## R6: Validation Error Display

**Decision**: Add an `ObservableProperty` for validation error messages on `TestListViewModel`. Display as an inline warning banner at the top of the test list (before the Select All row) when validation errors exist.

**Rationale**: Per clarification, validation errors should appear as an inline banner on the test list page. The ViewModel already manages profile loading; adding validation there is a natural fit.

**Alternatives considered**:
- Modal dialog: Rejected per clarification.
- Disable Run button with tooltip: Rejected per clarification.
