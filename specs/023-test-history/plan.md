# Implementation Plan: Test History

**Branch**: `023-test-history` | **Date**: 2026-02-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/023-test-history/spec.md`

## Summary

Implement a test history feature that persists test run reports to JSON files in AppData, provides a dedicated history view in the main navigation, displays pass rate trends via a line graph, identifies flaky tests, and allows users to manage (delete/clear) their test history.

**CRITICAL**: The History view MUST use premium/authentic UI styling consistent with ResultsView, DiagnosticsView, and other pages (AnimatedPageHeader, Card styles, accent gradients, status colors, entrance animations).

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, System.Text.Json
**Storage**: JSON files in `%APPDATA%/ReqChecker/history/`
**Testing**: Manual testing (existing project pattern)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single solution with App/Core/Infrastructure layers
**Performance Goals**: History view loads within 2 seconds for 100+ runs
**Constraints**: No external charting libraries (implement custom LineChart control like existing DonutChart)
**Scale/Scope**: Support 500+ historical runs with virtualized list

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The constitution template is unpopulated (placeholders only), so no specific gates apply. Proceeding with standard best practices:
- ✅ Follow existing project patterns (Services, ViewModels, Views structure)
- ✅ Use existing storage patterns (JSON in AppData, like PreferencesService)
- ✅ Implement custom controls (like DonutChart) rather than external dependencies
- ✅ Keep implementation simple and focused

## Project Structure

### Documentation (this feature)

```text
specs/023-test-history/
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
│   └── Models/
│       └── (existing RunReport.cs - reuse for history)
│
├── ReqChecker.Infrastructure/
│   └── History/
│       ├── IHistoryService.cs      # Interface for history operations
│       └── HistoryService.cs       # JSON file persistence
│
└── ReqChecker.App/
    ├── Controls/
    │   └── LineChart.xaml(.cs)     # New line graph control
    ├── ViewModels/
    │   └── HistoryViewModel.cs     # History view logic
    ├── Views/
    │   └── HistoryView.xaml(.cs)   # History UI
    └── Services/
        └── NavigationService.cs    # Update for history navigation
```

**Structure Decision**: Follows existing layered architecture (Core models, Infrastructure services, App UI). History service goes in Infrastructure layer. New HistoryView follows existing view patterns.

## UI/UX Requirements (Mandatory)

The History view MUST implement premium/authentic styling:

| Element | Style/Resource | Reference |
|---------|---------------|-----------|
| Page Header | `AnimatedPageHeader` | ResultsView.xaml |
| Content Cards | `Card` | DiagnosticsView.xaml |
| Primary Actions | `PrimaryButton` | Controls.xaml |
| Secondary Actions | `SecondaryButton` | Controls.xaml |
| Tertiary Actions | `GhostButton` | Controls.xaml |
| Filter Tabs | `FilterTab` | ResultsView.xaml |
| List Items | `AnimatedResultItem` pattern | ResultsView.xaml |
| Status Colors | `StatusPass`/`StatusFail`/`StatusSkip` | Colors.Dark/Light.xaml |
| Empty State | Premium empty state | ResultsView.xaml |

## Complexity Tracking

No constitution violations. Implementation follows existing patterns:
- Storage: Same as PreferencesService (JSON in AppData)
- UI: Same as other views (MVVM with CommunityToolkit) with PREMIUM styling
- Controls: Same as DonutChart (custom WPF control)
