# Quickstart: Selective Test Run

**Feature**: 030-selective-test-run
**Date**: 2026-02-07

## Overview

This feature adds checkbox-based test selection to the test list page. Users can select which tests to run instead of always running all tests in a profile.

## Key Files to Modify

| # | File | Change Summary |
|---|------|----------------|
| 1 | `src/ReqChecker.App/ViewModels/SelectableTestItem.cs` | **NEW** — Observable wrapper adding `IsSelected` to `TestDefinition` |
| 2 | `src/ReqChecker.App/ViewModels/TestListViewModel.cs` | Add `SelectableTests` collection, `IsAllSelected`, selection logic, dynamic button label |
| 3 | `src/ReqChecker.App/Views/TestListView.xaml` | Add checkboxes in cards, "Select All" in header, opacity binding, dynamic button text |
| 4 | `src/ReqChecker.Core/Interfaces/IAppState.cs` | Add `SelectedTestIds` property and `SetSelectedTestIds()` method |
| 5 | `src/ReqChecker.App/Services/AppState.cs` | Implement `SelectedTestIds` storage |
| 6 | `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` | Filter `CurrentProfile.Tests` using `SelectedTestIds` from AppState |
| 7 | `src/ReqChecker.App/Resources/Styles/Controls.xaml` | Add `AccentCheckBox` style matching premium design language |
| 8 | `tests/ReqChecker.App.Tests/ViewModels/TestListViewModelTests.cs` | **NEW** — Unit tests for selection logic |

## Build & Test

```bash
# Build
dotnet build src/ReqChecker.App/ReqChecker.App.csproj

# Run tests
dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj

# Run full solution
dotnet build
dotnet test
```

## Implementation Notes

1. **No ITestRunner changes** — The runner already iterates `profile.Tests`. We create a shallow copy of the `Profile` with a filtered `Tests` list and pass that to the existing `RunTestsAsync()`.

2. **SelectableTestItem pattern** — Do NOT add `IsSelected` to `TestDefinition` (Core model). Use a wrapper ViewModel in the App layer.

3. **AppState communication** — `TestListViewModel` writes `SelectedTestIds` to `IAppState` before navigation. `RunProgressViewModel` reads and consumes them. This follows the established `CurrentProfile` / `LastRunReport` pattern.

4. **Default behavior preserved** — When `SelectedTestIds` is `null` in AppState, `RunProgressViewModel` runs ALL tests. This ensures the existing flow (direct navigation without selection) still works.

5. **Animation** — Use `DurationFast` (150ms) from `Animations.xaml` for the opacity transition on deselected cards. Bind card `Opacity` to `IsSelected` via a converter or trigger.
