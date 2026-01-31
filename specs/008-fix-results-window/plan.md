# Implementation Plan: Fix Test Results Window

**Branch**: `008-fix-results-window` | **Date**: 2026-01-31 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/008-fix-results-window/spec.md`

## Summary

Fix multiple bugs in the Test Results window that prevent users from viewing and exporting test results. The core issue is that `NavigationService.NavigateToResults()` creates a new `ResultsViewModel` but never loads data from `AppState.LastRunReport`. Secondary fixes include enabling/disabling export buttons based on data availability and highlighting the Results menu item when active.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection
**Storage**: In-memory (session-only via IAppState)
**Testing**: xUnit, Moq
**Target Platform**: Windows (WPF desktop application)
**Project Type**: Desktop WPF application with MVVM architecture
**Performance Goals**: Results view loads within 1 second, filter operations respond within 100ms
**Constraints**: No silent failures - all errors must display user-friendly messages
**Scale/Scope**: Single-user desktop app, single test run stored at a time

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: PASS - No project constitution defined (template only). Following existing codebase conventions:
- MVVM pattern with CommunityToolkit.Mvvm
- Dependency injection via IServiceProvider
- xUnit for unit tests with Moq for mocking

## Project Structure

### Documentation (this feature)

```text
specs/008-fix-results-window/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (N/A - no new entities)
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Services/
│   │   ├── NavigationService.cs    # MODIFY: Load Report from AppState
│   │   └── AppState.cs             # READ: IAppState interface
│   ├── ViewModels/
│   │   └── ResultsViewModel.cs     # MODIFY: Add CanExport property
│   ├── Views/
│   │   └── ResultsView.xaml        # MODIFY: Bind export button IsEnabled
│   └── MainWindow.xaml.cs          # MODIFY: Set NavResults.IsActive
├── ReqChecker.Core/
│   └── Models/
│       └── RunReport.cs            # READ: Data model reference
└── ReqChecker.Infrastructure/
    └── Export/
        ├── JsonExporter.cs         # READ: Existing export logic
        └── CsvExporter.cs          # READ: Existing export logic

tests/
└── ReqChecker.App.Tests/
    └── ViewModels/
        └── ResultsViewModelTests.cs  # MODIFY: Add tests for data loading
```

**Structure Decision**: Existing WPF MVVM structure. No new projects or major restructuring needed - this is a bug fix that modifies existing files.

## Complexity Tracking

No constitution violations. This is a straightforward bug fix with minimal complexity:
- 3-4 files modified
- No new dependencies
- No architectural changes
- Follows existing patterns

## Implementation Approach

### Fix 1: Load Data in NavigationService (FR-001, FR-002, FR-003)

**Location**: `src/ReqChecker.App/Services/NavigationService.cs`

**Change**: In `NavigateToResults()`, after creating the ViewModel, set `viewModel.Report = appState.LastRunReport`

**Pattern**: Follows existing pattern in `NavigateToTestListWithProfile()` which gets AppState from DI and sets properties.

### Fix 2: Enable/Disable Export Buttons (FR-004, FR-005)

**Location**: `src/ReqChecker.App/ViewModels/ResultsViewModel.cs`

**Change**: Add computed property `CanExport => Report != null` and bind export button `IsEnabled` to it.

**Alternative considered**: Disable at command level with `CanExecute`. Rejected because button should visually indicate state, and current commands already check for null Report.

### Fix 3: Highlight Results Menu Item (FR-008)

**Location**: `src/ReqChecker.App/MainWindow.xaml.cs`

**Change**: In `NavigateWithAnimation()` case "Results", add `NavResults.IsActive = true` after navigation.

**Pattern**: Follows existing pattern used in `OnWindowLoaded()` for `NavTests.IsActive` and `NavProfiles.IsActive`.

### Fix 4: Verify Filters Work (FR-009, FR-010)

**Action**: Manual verification after Fix 1. Filter logic already exists in `ResultsViewModel.FilterTestResult()` and binds to `FilteredResults`. If data loads correctly, filters should work.

### Fix 5: Empty State Handling (FR-011)

**Location**: `src/ReqChecker.App/Views/ResultsView.xaml`

**Change**: Ensure empty state message displays when `Report` is null (no prior test run). Current view may already have this - verify and add if missing.

## Test Strategy

### Unit Tests

1. **ResultsViewModelTests**: Test that setting `Report` property:
   - Updates `FilteredResults` collection view
   - Stores report in AppState
   - `CanExport` returns true when Report is set, false when null

2. **NavigationService tests**: Verify `NavigateToResults()` sets Report from LastRunReport

### Manual Testing

1. Run tests → Click "Results" in sidebar → Verify data displays
2. Navigate away → Click "Results" again → Verify data persists
3. Click JSON button → Verify save dialog appears
4. Click CSV button → Verify save dialog appears
5. Click filter tabs → Verify correct filtering
6. Fresh app launch → Click "Results" → Verify empty state message

## Files to Modify

| File | Change Type | Purpose |
|------|-------------|---------|
| `NavigationService.cs` | Modify | Load Report from AppState in NavigateToResults() |
| `ResultsViewModel.cs` | Modify | Add CanExport property |
| `ResultsView.xaml` | Modify | Bind export button IsEnabled to CanExport |
| `MainWindow.xaml.cs` | Modify | Set NavResults.IsActive in navigation |
| `ResultsViewModelTests.cs` | Modify | Add tests for data loading and CanExport |

## Risk Assessment

**Low Risk**:
- All changes are localized to existing files
- Follows established patterns in codebase
- No database/storage changes
- No new dependencies
- Bug fixes with clear expected behavior

**Testing Coverage**:
- Existing `ResultsViewModelTests.cs` provides foundation
- Manual verification confirms UI behavior
