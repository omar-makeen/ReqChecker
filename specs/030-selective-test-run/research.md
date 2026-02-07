# Research: Selective Test Run

**Feature**: 030-selective-test-run
**Date**: 2026-02-07

## R1: How to Pass Selected Tests to the Runner

**Decision**: Create a filtered copy of the `Profile` object with only the selected `TestDefinition` items in its `Tests` list, then pass that filtered profile to the existing `ITestRunner.RunTestsAsync()`.

**Rationale**: The `SequentialTestRunner` iterates `profile.Tests` and builds its `RunReport` using `profile.Id` and `profile.Name`. By cloning the profile with a filtered test list, we:
- Require zero changes to `ITestRunner` interface
- Require zero changes to `SequentialTestRunner`
- Preserve correct `ProfileId`/`ProfileName` in reports and history
- Downstream pages (RunProgress, Results, History) work unchanged since they consume whatever the runner returns

**Alternatives considered**:
- Add `IEnumerable<TestDefinition>` overload to `ITestRunner` — rejected because it changes a core interface, requires updating all implementations, and the report needs profile metadata
- Add `SelectedTests` filter property to `RunSettings` — rejected because it mixes execution config with selection state

## R2: How to Communicate Selection from TestList to RunProgress

**Decision**: Store selected test IDs in `IAppState` via a new `SelectedTestIds` property (`IReadOnlyList<string>?`). `TestListViewModel` writes the IDs before navigating. `RunProgressViewModel` reads them on construction and filters `CurrentProfile.Tests`.

**Rationale**: AppState is already the established communication channel between ViewModels (used for `CurrentProfile` and `LastRunReport`). This follows the existing pattern without adding new coupling or modifying `NavigationService`.

**Alternatives considered**:
- Pass selected tests via `NavigationService` parameter — rejected because it breaks the current pattern where `NavigateToRunProgress()` takes no parameters and ViewModels are DI-resolved
- Store a full filtered `Profile` in AppState — rejected because it would duplicate profile data and complicate the `CurrentProfile` semantics

## R3: ViewModel Pattern for Per-Test Selection State

**Decision**: Create a `SelectableTestItem` wrapper class using `ObservableObject` from CommunityToolkit.Mvvm. It wraps a `TestDefinition` and adds an `IsSelected` observable property. `TestListViewModel` creates an `ObservableCollection<SelectableTestItem>` from `CurrentProfile.Tests`.

**Rationale**: `TestDefinition` is a Core model and should not be polluted with UI state. A lightweight wrapper follows the MVVM pattern used elsewhere in the app (e.g., `TestResult` displayed in `RunProgressViewModel`). Using `ObservableObject` gives free `INotifyPropertyChanged` support for checkbox binding.

**Alternatives considered**:
- Add `IsSelected` directly to `TestDefinition` — rejected because it leaks UI state into the domain model in the Core project
- Use ListBox multi-selection with `SelectedItems` — rejected because WPF `SelectedItems` is not bindable and requires code-behind; checkbox-per-item is cleaner for this UX

## R4: Checkbox Visual Style for Premium Feel

**Decision**: Create an `AccentCheckBox` style in `Controls.xaml` that uses `AccentPrimary` color for the checked state, matches the existing 8px spacing grid, and uses the app's `DurationFast` (150ms) for state transitions. The checkbox is positioned inside the test card grid as the first column (before the type icon).

**Rationale**: The app has no existing checkbox style (confirmed by Controls.xaml audit). The new style must match the premium design language: accent colors, smooth transitions, consistent spacing. Placing it inside the card grid (not as a separate column outside the card) maintains the card-as-unit visual pattern.

**Alternatives considered**:
- Use default WPF CheckBox — rejected because it looks generic and doesn't match the cyan/indigo accent theme
- Use a toggle switch instead — rejected because checkboxes are the standard pattern for multi-item selection; toggle switches imply on/off settings

## R5: Select All with Indeterminate State

**Decision**: `TestListViewModel` exposes `IsAllSelected` (bool?) property: `true` when all selected, `false` when none selected, `null` when mixed (indeterminate). The "Select All" checkbox in the header binds `IsChecked` to this three-state property. Toggling it sets all items to the opposite of the current majority (or all-checked if indeterminate).

**Rationale**: WPF `CheckBox` natively supports `IsThreeState` with `IsChecked` binding to `bool?`. This matches the Windows File Explorer and Outlook selection patterns users already understand.

**Alternatives considered**:
- Two separate buttons "Select All" / "Deselect All" — rejected because it takes more space and is less discoverable than a single checkbox
- Clicking indeterminate always checks all — this IS the chosen behavior (simpler mental model)
