# Implementation Plan: Re-run Failed Tests Only

**Branch**: `034-rerun-failed-tests` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/034-rerun-failed-tests/spec.md`

## Summary

Add a "Re-run Failed" PrimaryButton to the Results page that collects all failed test IDs (plus dependency-skipped test IDs) from the current run report, stores them via the existing selective test execution mechanism (`IAppState.SetSelectedTestIds`), and navigates to the Run Progress page. Only two files are modified: `ResultsViewModel.cs` (add command + computed property) and `ResultsView.xaml` (add button).

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (in-memory session-only; reuses IAppState.SelectedTestIds)
**Testing**: Manual verification (run app, trigger failures, click re-run button)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop WPF application
**Performance Goals**: N/A (button click triggers existing test execution flow)
**Constraints**: Must reuse existing selective test execution infrastructure; no new services or interfaces
**Scale/Scope**: 2 files modified, ~30 lines of ViewModel code + ~15 lines of XAML

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is a blank template — no project-specific gates defined. No violations possible.

**Post-Phase 1 re-check**: No new projects, no new packages, no new abstractions introduced. Feature is a minimal addition to existing ViewModel + View.

## Project Structure

### Documentation (this feature)

```text
specs/034-rerun-failed-tests/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output — research decisions
├── data-model.md        # Phase 1 output — entity usage analysis
├── quickstart.md        # Phase 1 output — implementation guide
├── contracts/           # Phase 1 output — command contract
│   └── test-rerun-selection.md
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (files modified)

```text
src/ReqChecker.App/
├── ViewModels/
│   └── ResultsViewModel.cs    # Add HasFailedTests property + RerunFailedTestsCommand
└── Views/
    └── ResultsView.xaml        # Add PrimaryButton with ArrowRepeatAll24 icon
```

**Structure Decision**: This is a minimal 2-file change within the existing WPF application structure. No new files, projects, or directories are created.

## Implementation Details

### Phase 1: ViewModel Logic (ResultsViewModel.cs)

**Add computed property**:
- `HasFailedTests` (bool): Returns true when `Report?.Results` contains at least one result with `Status == TestStatus.Fail`
- Must notify on change when `Report` property changes

**Add command**:
- `RerunFailedTestsCommand` (RelayCommand): Collects re-run test IDs and navigates
- Logic:
  1. Collect `TestId` from results where `Status == TestStatus.Fail`
  2. Collect `TestId` from results where `Status == TestStatus.Skipped` AND `Error?.Category == ErrorCategory.Dependency`
  3. Union both sets into a deduplicated list
  4. Call `_appState.SetSelectedTestIds(rerunIds)`
  5. Call `_navigationService.NavigateToRunProgress()`
- CanExecute: `HasFailedTests && _navigationService != null`

### Phase 2: View Layer (ResultsView.xaml)

**Add button in header action bar** (after "Back to Tests", before export buttons):
- Style: `{StaticResource PrimaryButton}` (gradient background, white text)
- Icon: `ArrowRepeatAll24` with `FontSize="16"` and `Margin="0,0,8,0"`
- Text: "Re-run Failed"
- Visibility: Bound to `HasFailedTests` via `BooleanToVisibilityConverter`
- Command: `{Binding RerunFailedTestsCommand}`
- Margin: `0,0,8,0` (consistent spacing with adjacent buttons)
- TabIndex: Between Back (1) and JSON (2) — use TabIndex="2", shift others +1
- Animation: Inherits from `AnimatedPageHeader` parent (300ms fade + translateY)

### Key Design Decisions (from research.md)

1. **Reuse existing selective execution** — no new runner, service, or interface
2. **Dependency-skipped detection** — use `ErrorCategory.Dependency` (already set by SequentialTestRunner)
3. **Hide vs disable** — hide button entirely when no failures (disabled gradient button is visually confusing)
4. **Profile staleness** — existing RunProgressViewModel filter handles missing test IDs gracefully

## Complexity Tracking

No constitution violations. No complexity justifications needed.

| Aspect | Assessment |
|--------|-----------|
| New files | 0 |
| Modified files | 2 |
| New packages | 0 |
| New abstractions | 0 |
| Lines of code (estimated) | ~45 |
