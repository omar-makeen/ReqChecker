# Implementation Plan: Selective Test Run

**Branch**: `030-selective-test-run` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/030-selective-test-run/spec.md`

## Summary

Add checkbox-based test selection to the test list page so users can choose which tests to run instead of always running all. The approach is UI-only: a `SelectableTestItem` wrapper ViewModel provides per-test `IsSelected` state, the existing `SequentialTestRunner` receives a filtered `Profile` containing only selected tests, and no changes are needed to the runner interface or profile JSON schema. Visual treatment includes accent-styled checkboxes inside each card, reduced opacity on deselected cards with 150-200ms animated transitions, a "Select All" checkbox in the header, and a dynamic run-button label showing selection count.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection
**Storage**: N/A (in-memory session-only selection state; no persistence)
**Testing**: xUnit + Moq (existing test infrastructure)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Multi-project solution (Core / Infrastructure / App / Tests)
**Performance Goals**: Selection toggle must feel instant (<50ms UI response); opacity animation 150-200ms
**Constraints**: No changes to profile JSON schema; no changes to ITestRunner interface; selection resets on navigation/profile reload
**Scale/Scope**: Profiles with up to ~50 tests; single-user desktop app

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is not customized for this project (blank template). No gates to enforce. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/030-selective-test-run/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Core/
│   └── Interfaces/
│       └── IAppState.cs                    # Add SelectedTestIds + SetSelectedTestIds()
├── ReqChecker.App/
│   ├── ViewModels/
│   │   ├── SelectableTestItem.cs           # NEW — wrapper ViewModel for per-test selection
│   │   └── TestListViewModel.cs            # Add selection state, Select All logic, dynamic button label
│   ├── Views/
│   │   └── TestListView.xaml               # Add checkboxes, Select All, opacity binding, dynamic button
│   ├── Services/
│   │   └── AppState.cs                     # Add SelectedTestIds property
│   └── Resources/Styles/
│       └── Controls.xaml                   # Add AccentCheckBox style
tests/
├── ReqChecker.App.Tests/
│   └── ViewModels/
│       └── TestListViewModelTests.cs       # NEW — selection logic unit tests
```

**Structure Decision**: Follows existing multi-project layout. All changes are in App layer (ViewModel + View + Styles) with a minimal IAppState extension in Core. No Infrastructure changes needed since the runner already works with any Profile.Tests list.

## Complexity Tracking

No constitution violations. Feature touches 7 files (1 new ViewModel, 1 new test file, 5 modified) with no new dependencies or patterns beyond what already exists.
