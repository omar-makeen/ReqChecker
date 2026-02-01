# Implementation Plan: Diagnostics Auto-Load

**Branch**: `017-diagnostics-auto-load` | **Date**: 2026-02-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/017-diagnostics-auto-load/spec.md`

## Summary

The System Diagnostics page currently displays Machine Information and Network Interfaces only after test runs complete, relying on `LastRunReport.MachineInfo`. This implementation adds a separate `CurrentMachineInfo` property that loads fresh system data on every page navigation, independent of test execution.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection
**Storage**: N/A (in-memory only, refreshed on navigation)
**Testing**: xUnit (existing test infrastructure)
**Target Platform**: Windows 10/11 (win-x64)
**Project Type**: Desktop WPF application
**Performance Goals**: Machine info visible within 2 seconds of page load
**Constraints**: Must not block UI thread; graceful error handling
**Scale/Scope**: Single page enhancement, ~50 lines ViewModel changes + XAML bindings

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **No constitution defined** - Project uses template constitution without custom rules.

## Project Structure

### Documentation (this feature)

```text
specs/017-diagnostics-auto-load/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── ViewModels/
│   │   └── DiagnosticsViewModel.cs  # Modified: add CurrentMachineInfo
│   ├── Views/
│   │   ├── DiagnosticsView.xaml     # Modified: bind to CurrentMachineInfo
│   │   └── DiagnosticsView.xaml.cs  # Modified: trigger refresh on navigation
│   └── Services/
│       └── AppState.cs              # Unchanged
├── ReqChecker.Core/
│   └── Models/
│       ├── MachineInfo.cs           # Unchanged
│       └── NetworkInterfaceInfo.cs  # Unchanged
└── ReqChecker.Infrastructure/
    └── Platform/
        └── MachineInfoCollector.cs  # Unchanged (already supports standalone collection)

tests/
└── ReqChecker.App.Tests/
    └── ViewModels/
        └── DiagnosticsViewModelTests.cs  # Added: test auto-load behavior
```

**Structure Decision**: Minimal changes to existing single-project structure. Only modifying DiagnosticsViewModel/View to add CurrentMachineInfo property and load on navigation.

## Complexity Tracking

No violations. This is a straightforward UI enhancement:
- Reuses existing `MachineInfoCollector.Collect()` method
- Adds one new property to ViewModel
- Updates XAML bindings to use new property
- No new dependencies or architectural changes
