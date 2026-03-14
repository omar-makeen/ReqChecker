# Research: Unsaved Changes Warning

**Feature**: 061-unsaved-changes-warning | **Date**: 2026-03-14

## Decision 1: Dirty Tracking Strategy

**Decision**: Value-based comparison using dictionary baseline snapshot

**Rationale**: The spec explicitly requires value-based comparison (FR-007) so that editing a field and reverting it clears the dirty state. A dictionary `Dictionary<string, string?>` mapping field names to their string representations provides a simple, flat structure for comparison. All editable fields (Timeout, RetryCount, parameters) can be uniformly represented as strings.

**Alternatives considered**:
- Event-based tracking (set dirty on any PropertyChanged): Rejected — fails the edit-then-revert requirement (FR-007)
- Deep clone comparison (snapshot entire TestDefinition): Rejected — over-engineered for 2 fixed fields + N parameters; TestDefinition contains non-editable fields that would cause false positives
- HashCode comparison: Rejected — prone to collisions and harder to debug

## Decision 2: Confirmation Dialog Implementation

**Decision**: Add `ShowConfirmationDialog` method to existing `DialogService` using WPF `MessageBox`

**Rationale**: The app currently has no confirmation dialog pattern (no MessageBox usage found in codebase). `DialogService` already handles file dialogs, making it the natural home for confirmation dialogs. WPF `MessageBox.Show()` provides native OS-styled dialogs with built-in keyboard support (Enter/Escape). The spec requires matching "existing dialog style" (FR-008) — since no custom dialog system exists, a MessageBox is the simplest consistent option.

**Alternatives considered**:
- Custom WPF-UI styled dialog (ContentDialog): Could provide more on-brand styling, but adds complexity. MessageBox is sufficient for a simple two-option confirmation and is immediately keyboard-accessible (FR-009)
- Inline confirmation banner (non-modal): Rejected — spec explicitly says "confirmation dialog" (FR-003)
- Browser-style `beforeunload` pattern: N/A — this is a WPF desktop app with Frame-based navigation

## Decision 3: Integration Point for Back Navigation

**Decision**: Override the existing `BackCommand` in `TestConfigViewModel` to check `HasUnsavedChanges` before calling `NavigationService.GoBack()`

**Rationale**: The existing `BackCommand` is a simple `RelayCommand` that directly calls `GoBack()`. Adding the dirty check before navigation keeps the logic in the ViewModel (testable) and requires no changes to `NavigationService`. The dialog is shown via `DialogService` (injected dependency).

**Alternatives considered**:
- NavigationService-level guard (INavigationGuard pattern): Over-engineered for a single page; would require interface + registration mechanism
- View-level interception (Frame.Navigating event): Harder to test, mixes concerns between View and ViewModel

## Decision 4: PasswordBox Value Tracking

**Decision**: No special handling needed — existing `ParameterPasswordBox_PasswordChanged` code-behind handler already syncs PasswordBox.Password to `TestParameterViewModel.Value`

**Rationale**: Since the PasswordBox's value is already pushed into the same `Value` property as regular TextBox fields, dirty tracking automatically covers password parameters. The baseline captures Value at load time, and any change (including password field changes) is detected via the same comparison logic.

**Alternatives considered**: None needed — existing pattern handles this naturally.

## Decision 5: DialogService Testability

**Decision**: Add `ShowConfirmationDialog` as a virtual method on `DialogService` (mockable via Moq) and/or extract to `IDialogService` interface

**Rationale**: `TestConfigViewModel` currently receives `DialogService` (concrete class) but for unit testing the dirty-check-then-dialog flow, we need to mock the dialog response. Making the method virtual allows Moq to override it. If `IDialogService` doesn't exist yet, creating one follows the existing `IAppState`, `IPreferencesService` pattern.

**Alternatives considered**:
- Pass a `Func<bool>` confirmation callback: Simpler but breaks the established service injection pattern
- Test only via integration tests: Insufficient — unit tests should verify the ViewModel logic independently
