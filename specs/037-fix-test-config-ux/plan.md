# Implementation Plan: Fix Test Configuration Page UX

**Branch**: `037-fix-test-config-ux` | **Date**: 2026-02-13 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/037-fix-test-config-ux/spec.md`

## Summary

Fix three UX issues on the test configuration page: (1) remove the redundant Cancel button and the entire footer bar, moving the "Save Changes" button to the header to match all other pages; (2) remove the MaxWidth="800" content cap and normalize the root margin from 24 to 32 to match other pages, eliminating excessive whitespace; (3) restyle the profile validation error notification bar to use premium theme tokens instead of hard-coded colors.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0 (all existing — no new packages)
**Storage**: N/A (UI-only enhancement, no data persistence changes)
**Testing**: dotnet test (xUnit) — existing test project at tests/ReqChecker.App.Tests/
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single desktop application
**Performance Goals**: N/A (UI-only changes)
**Constraints**: No new dependencies; must use existing theme token system
**Scale/Scope**: 4 files modified, 0 files created

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is a blank template — no project-specific gates defined. All changes are UI-only modifications to existing files using existing patterns. No violations possible.

**Pre-Phase 0**: PASS
**Post-Phase 1**: PASS

## Project Structure

### Documentation (this feature)

```text
specs/037-fix-test-config-ux/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (files to modify)

```text
src/ReqChecker.App/
├── Views/
│   ├── TestConfigView.xaml              # Remove footer, fix header, fix margins/MaxWidth
│   └── ProfileSelectorView.xaml         # Restyle error notification bar
├── ViewModels/
│   └── TestConfigViewModel.cs           # Remove CancelCommand and OnCancel()
└── Resources/Styles/
    └── (no changes — existing tokens are sufficient)
```

**Structure Decision**: Existing single-project WPF desktop application. All changes modify existing files in `src/ReqChecker.App/`. No new files or projects needed.

## Detailed Changes

### Change 1: Remove Cancel Button, Footer Bar, Move Save to Header

**Files**: `TestConfigView.xaml`, `TestConfigViewModel.cs`

**TestConfigView.xaml**:
- **Remove**: The entire footer `<Border Grid.Row="2" ...>` section (lines ~320-353) containing Cancel and Save buttons
- **Remove**: Grid.Row="2" RowDefinition (the `Auto` row for the footer)
- **Modify header** (Grid.Row="0"): Add a "Save Changes" `PrimaryButton` right-aligned in the header, following the same pattern used by ResultsView, HistoryView, and other pages (StackPanel with Orientation="Horizontal" in a right-aligned column)
- **Header pattern**: Back button (left) | Icon + Title (center-left) | Save Changes button (right)

**TestConfigViewModel.cs**:
- **Remove**: `public ICommand CancelCommand { get; }` property declaration
- **Remove**: `CancelCommand = new RelayCommand(OnCancel);` from constructor
- **Remove**: The entire `OnCancel()` private method
- **Keep**: SaveCommand, BackCommand, PromptForCredentialsCommand (unchanged)

### Change 2: Fix Whitespace (MaxWidth + Margins)

**File**: `TestConfigView.xaml`

- **Remove**: `MaxWidth="800"` from the StackPanel inside ScrollViewer (line ~91) — content should fill available width like all other pages
- **Change**: Root Grid `Margin="24"` → `Margin="32"` to match TestListView, ResultsView, ProfileSelectorView, HistoryView (all use 32)

### Change 3: Premium Error Notification Bar

**File**: `ProfileSelectorView.xaml`

- **Replace**: Hard-coded `Background="#1AEF4444"` with a theme-aware background using existing `StatusFailGlowColor` token or a DynamicResource brush
- **Update**: `CornerRadius="8"` → `CornerRadius="12"` to match ParameterGroupCard and other premium cards
- **Add**: `Effect` property with a subtle `DropShadowEffect` using the `StatusFailGlowColor` token for a premium error glow
- **Keep**: Existing `BorderBrush="{DynamicResource StatusFail}"` and text foreground (already using theme tokens)
- **Update**: Dismiss button from `GhostButtonSmall` to `GhostButton` for better visibility and consistency

## Complexity Tracking

No violations — all changes are straightforward UI modifications to 4 existing files using existing patterns and tokens.
