# Research: Re-run Failed Tests Only

**Feature Branch**: `034-rerun-failed-tests`
**Date**: 2026-02-07

## Decision 1: Test Selection Mechanism

**Decision**: Reuse the existing `IAppState.SetSelectedTestIds()` + `NavigationService.NavigateToRunProgress()` flow.

**Rationale**: The selective test execution infrastructure (feature 030) already provides exactly the pattern needed. `RunProgressViewModel` reads `SelectedTestIds` from `IAppState`, filters the current profile's tests to only include those IDs, then clears the state. No new execution path is needed.

**Alternatives considered**:
- Creating a dedicated "re-run" mode in the runner — rejected as over-engineering; the existing selective run mechanism is functionally identical.
- Passing test IDs directly to `NavigateToRunProgress()` — rejected because the navigation method takes no parameters by design; state is passed via `IAppState`.

## Decision 2: Identifying Dependency-Skipped Tests

**Decision**: Use `ErrorCategory.Dependency` on `TestResult.Error.Category` to distinguish dependency-skipped tests from other skip reasons.

**Rationale**: The `SequentialTestRunner` already sets `Error.Category = ErrorCategory.Dependency` when a test is skipped because a prerequisite failed. This is distinct from:
- `ErrorCategory.Permission` (admin privilege skips)
- `ErrorCategory.Configuration` (credentials not provided)
- Cancellation skips (which use `ErrorCategory.Unknown` with "Test was cancelled")

**Alternatives considered**:
- Parsing the error message string for "Prerequisite" — rejected as fragile and locale-sensitive.
- Cross-referencing `DependsOn` lists against failed test IDs — rejected as unnecessarily complex when the error category already encodes this information.

## Decision 3: Button Visibility Strategy

**Decision**: Use `Visibility` binding to a `HasFailedTests` computed property, hiding the button entirely when no tests failed.

**Rationale**: The spec states the button must be "hidden or disabled" (FR-002). Hiding is preferred because:
- A disabled PrimaryButton (gradient, white text) still draws visual attention even when non-functional.
- Export buttons use `IsEnabled` because they're always conceptually relevant; the re-run button is not relevant when all tests pass.
- Consistent with the app's pattern of hiding rather than disabling irrelevant actions (e.g., GhostButton actions in empty states).

**Alternatives considered**:
- Using `IsEnabled` binding — rejected because a disabled gradient button is visually confusing.
- Conditional Visibility with fade animation — rejected as over-engineered; standard Visibility collapse is sufficient since the button animates in with the header anyway.

## Decision 4: Re-run Test Collection Logic

**Decision**: Collect failed test IDs + dependency-skipped test IDs from the current `RunReport`, then pass all of them as `SelectedTestIds`.

**Rationale**: The `SequentialTestRunner` will re-evaluate dependencies during execution anyway, but including dependency-skipped tests in the selection ensures they appear in the filtered profile and get a chance to run. Without including them, they would be absent from the profile copy and silently skipped.

**Implementation**:
```
failedIds = Report.Results.Where(r => r.Status == TestStatus.Fail).Select(r => r.TestId)
depSkippedIds = Report.Results.Where(r => r.Status == TestStatus.Skipped && r.Error?.Category == ErrorCategory.Dependency).Select(r => r.TestId)
rerunIds = failedIds.Union(depSkippedIds).ToList()
```

## Decision 5: Profile Staleness Handling

**Decision**: Use the currently loaded profile (`IAppState.CurrentProfile`). If a failed test ID no longer exists in the profile, it is silently excluded by `RunProgressViewModel`'s existing filter logic (`Tests.Where(t => selectedTestIds.Contains(t.Id))`).

**Rationale**: The `RunProgressViewModel` already handles this case — it creates a profile copy with only tests whose IDs match the selection. Non-matching IDs are simply excluded. If the result is zero tests, the runner handles the empty case gracefully.

**Alternatives considered**:
- Storing the original profile snapshot with the run report — rejected as wasteful (profiles are already loaded from embedded resources or files).
- Validating test IDs before navigation and showing a warning — rejected as unnecessary complexity; the edge case is rare and the existing behavior is correct.
