# Implementation Plan: Subtle Chip Badge

**Branch**: `064-subtle-chip-badge` | **Date**: 2026-03-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/064-subtle-chip-badge/spec.md`

## Summary

Restyle the test count badges (page header and sidebar) from accent-colored pill to a muted "informational chip" style — surface background, secondary text, regular weight, subtle border, smaller padding. This creates clear visual hierarchy where the "Run All Tests" CTA is the sole accent-colored element.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2
**Storage**: N/A (UI-only change)
**Testing**: dotnet test (xUnit + Moq)
**Target Platform**: Windows desktop (WPF)
**Project Type**: desktop-app
**Performance Goals**: N/A (visual-only change, no performance impact)
**Constraints**: Must work in both dark and light WPF-UI themes
**Scale/Scope**: 2 XAML property changes across 2 files

## Constitution Check

*GATE: Constitution is a placeholder template — no project-specific gates defined. No violations.*

## Project Structure

### Documentation (this feature)

```text
specs/064-subtle-chip-badge/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (minimal — no data changes)
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── MainWindow.xaml              # Sidebar badge (lines 93-108)
│   ├── Views/
│   │   └── TestListView.xaml        # Page header badge (lines 112-120)
│   └── ViewModels/
│       ├── MainViewModel.cs         # TestCount/HasTests properties (no changes)
│       └── TestListViewModel.cs     # TestCountDisplay property (no changes)
tests/
└── ReqChecker.App.Tests/
    └── ViewModels/
        └── MainViewModelTests.cs    # Existing tests (no changes expected)
```

**Structure Decision**: No new files. Only XAML property modifications in two existing files.

## Complexity Tracking

No constitution violations to justify.
