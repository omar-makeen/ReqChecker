# Implementation Plan: Export Test Results

**Branch**: `022-export-results` | **Date**: 2026-02-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/022-export-results/spec.md`

## Summary

Add PDF export capability to the existing export infrastructure. JSON and CSV exports are already implemented and functional. The main work involves:
1. Creating a `PdfExporter` service implementing `IExporter`
2. Adding PDF export button to Results Dashboard UI
3. Adding ReqChecker branding assets for PDF header
4. Enhancing logging for all export operations

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, QuestPDF (for PDF generation)
**Storage**: N/A (file export to user-selected location)
**Testing**: Manual testing (existing pattern - no automated UI tests)
**Target Platform**: Windows 10/11 x64
**Project Type**: WPF Desktop Application (single solution, 3 projects)
**Performance Goals**: PDF export < 5 seconds for 100 tests, CSV/JSON < 2 seconds
**Constraints**: Must match existing export UX patterns, maintain premium styling
**Scale/Scope**: Typical test runs 10-100 tests per report

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Constitution not configured** - Using default project patterns observed in codebase:
- ✅ Follow existing service/interface patterns (`IExporter` interface)
- ✅ Use dependency injection (existing `JsonExporter`, `CsvExporter` registration)
- ✅ Maintain MVVM architecture (ViewModel commands, View bindings)
- ✅ Premium UI styling (use existing button styles, status feedback)
- ✅ Logging via Serilog (existing pattern in `ResultsViewModel`)

## Project Structure

### Documentation (this feature)

```text
specs/022-export-results/
├── plan.md              # This file
├── research.md          # PDF library evaluation
├── data-model.md        # Export entities (minimal - extends existing)
├── quickstart.md        # Implementation guide
└── contracts/           # N/A (no API contracts - desktop app)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Core/
│   └── Interfaces/
│       └── IExporter.cs              # Existing - no changes needed
├── ReqChecker.Infrastructure/
│   └── Export/
│       ├── JsonExporter.cs           # Existing - add logging enhancement
│       ├── CsvExporter.cs            # Existing - add logging enhancement
│       └── PdfExporter.cs            # NEW - PDF generation
└── ReqChecker.App/
    ├── ViewModels/
    │   └── ResultsViewModel.cs       # Add ExportToPdfCommand
    ├── Views/
    │   └── ResultsView.xaml          # Add PDF export button
    └── Resources/
        └── Images/
            └── reqchecker-logo.png   # NEW - branding for PDF header
```

**Structure Decision**: Extends existing Infrastructure/Export pattern. No new projects needed.

## Complexity Tracking

No constitution violations - feature follows established patterns.

## Existing Implementation Analysis

### Already Implemented (No Changes Needed)
- `IExporter` interface with `ExportAsync(RunReport, filePath, maskCredentials)`
- `JsonExporter` - fully functional JSON export
- `CsvExporter` - fully functional CSV export with summary, machine info, and test results
- `ResultsViewModel` - export commands, file dialog, error handling, status messages
- `ResultsView.xaml` - export buttons in header, status message display
- `DialogService.SaveFileDialog()` - file picker

### New Implementation Required
1. **PdfExporter** - Implements `IExporter`, generates branded PDF reports
2. **PDF button in UI** - Add alongside existing JSON/CSV buttons
3. **Logo asset** - ReqChecker logo PNG for PDF header
4. **Export logging** - Add diagnostic logging with format, outcome, file size (FR-020)

## Implementation Phases

### Phase 1: PDF Exporter Service
- Create `PdfExporter.cs` in `ReqChecker.Infrastructure/Export/`
- Add QuestPDF NuGet package to Infrastructure project
- Implement professional PDF layout:
  - Header: ReqChecker logo, profile name, run date, summary stats
  - Machine info section
  - Test results table with status, name, duration
  - Failed test details (error messages)
- Register in DI container

### Phase 2: UI Integration
- Add PDF export button to `ResultsView.xaml` header
- Add `ExportToPdfCommand` to `ResultsViewModel`
- Wire up file dialog with PDF filter

### Phase 3: Logging Enhancement
- Add structured logging to all exporters (format, outcome, file size)
- Consistent log format for diagnostics

### Phase 4: Assets
- Add ReqChecker logo PNG to Resources/Images
- Embed as resource in App project
