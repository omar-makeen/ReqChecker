# Implementation Plan: Unsaved Changes Warning

**Branch**: `061-unsaved-changes-warning` | **Date**: 2026-03-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/061-unsaved-changes-warning/spec.md`

## Summary

Add dirty-state tracking to the Test Configuration page so that clicking Back with unsaved changes shows a confirmation dialog (Discard / Stay). Dirty state is computed by value-based comparison of current field values against a captured baseline. No dialog appears when values match the baseline (including edit-then-revert). After Save, the baseline resets to saved values.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2
**Storage**: N/A (in-memory session-only dirty tracking; no persistence)
**Testing**: xUnit + Moq (existing test infrastructure)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop application
**Performance Goals**: Dialog dismiss < 1 second (SC-003)
**Constraints**: Value-based comparison (not event-based); keyboard-accessible dialog (Escape to stay, Enter to confirm)
**Scale/Scope**: Single page (TestConfigView), ~3 tracked fields + N dynamic parameters

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is template-only (no project-specific gates defined). No violations to evaluate. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/061-unsaved-changes-warning/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── ViewModels/
│   │   └── TestConfigViewModel.cs       # Add dirty tracking, baseline capture, HasUnsavedChanges property
│   ├── Views/
│   │   └── TestConfigView.xaml.cs       # Wire PasswordBox changes to dirty tracking
│   └── Services/
│       └── DialogService.cs             # Add ShowConfirmationDialog method
tests/
└── ReqChecker.App.Tests/
    └── ViewModels/
        └── TestConfigViewModelTests.cs  # New: dirty state + dialog tests
```

**Structure Decision**: All changes fit within the existing single-project WPF app structure. No new projects or architectural changes needed. The feature touches 3-4 existing files and adds 1 test file.

## Key Design Decisions

### 1. Dirty Tracking Location: ViewModel-only

Dirty state tracking lives entirely in `TestConfigViewModel`. A dictionary-based baseline snapshot is captured when parameters are initialized. On each property change, current values are compared against the baseline.

**Why**: Keeps logic testable without UI dependencies. No need for a separate service — scope is limited to one page.

### 2. Baseline Snapshot as Dictionary

Capture `Dictionary<string, string?>` mapping field names → string values at load time. Keys: `"Timeout"`, `"RetryCount"`, plus each parameter's name. Comparison is string equality (all values are ultimately strings in the UI).

**Why**: Simple, flat structure. Nullable int (Timeout/RetryCount) converts to string for uniform comparison. Handles edit-then-revert naturally via value equality.

### 3. Confirmation Dialog via DialogService

Add `ShowConfirmationDialog(string title, string message, string confirmText, string cancelText)` → `bool` to `DialogService`. Uses WPF `MessageBox` with `YesNo` buttons (or a custom styled dialog matching WPF-UI theme). The Back command checks `HasUnsavedChanges` before navigating.

**Why**: Centralizes dialog logic for reuse. Keeps ViewModel testable (DialogService is injected and mockable).

### 4. PasswordBox Dirty Tracking

PasswordBox values are already synced to `TestParameterViewModel.Value` via the `ParameterPasswordBox_PasswordChanged` code-behind handler. Dirty tracking hooks into the same `Value` property, so password changes are automatically included.

**Why**: No special handling needed — the existing sync mechanism already updates the bindable property.

### 5. HasUnsavedChanges Computation

Computed property (not stored boolean). Iterates baseline dictionary and compares each key's baseline value against the current value. Returns `true` if any mismatch found.

**Why**: Value-based comparison handles edit-then-revert correctly (FR-007). No risk of stale boolean state.

### 6. Save Resets Baseline

After `SaveAsync()` completes successfully, re-capture the baseline from current values. This ensures post-save modifications trigger the dialog correctly.

**Why**: Satisfies FR-006 and US3-AS2 (save, edit more, then Back should warn).

## Complexity Tracking

No constitution violations to justify. Feature is a straightforward ViewModel enhancement with no new projects, patterns, or dependencies.
