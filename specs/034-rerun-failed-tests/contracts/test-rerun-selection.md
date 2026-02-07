# Contract: Test Re-run Selection

**Feature Branch**: `034-rerun-failed-tests`
**Date**: 2026-02-07

## Overview

No new APIs, services, or interfaces are introduced. This feature adds a single ViewModel command that uses existing contracts.

## Command Contract: RerunFailedTests

**Trigger**: User clicks "Re-run Failed" button on Results page

**Preconditions**:
- `Report` is not null (a completed run exists)
- `Report.Results` contains at least one result with `Status == TestStatus.Fail`
- `CurrentProfile` is loaded in `IAppState`

**Behavior**:
1. Collect all `TestId` values from `Report.Results` where `Status == TestStatus.Fail`
2. Collect all `TestId` values from `Report.Results` where `Status == TestStatus.Skipped` AND `Error.Category == ErrorCategory.Dependency`
3. Union both sets (deduplicated)
4. Call `IAppState.SetSelectedTestIds(rerunIds)`
5. Call `NavigationService.NavigateToRunProgress()`

**Postconditions**:
- `IAppState.SelectedTestIds` contains the union of failed + dependency-skipped test IDs
- App navigates to RunProgress page
- RunProgressViewModel consumes the IDs and clears them (existing behavior)

**Error Handling**:
- If `NavigationService` is null: command is not executable (standard CanExecute pattern)
- If no failed tests exist: button is hidden (Visibility binding), command unreachable
- If all selected test IDs are absent from current profile: RunProgressViewModel creates empty test list, runner handles gracefully
