# Implementation Plan: Premium History Cards

**Branch**: `026-premium-history-cards` | **Date**: 2026-02-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/026-premium-history-cards/spec.md`

## Summary

Improve the Test History result cards to be self-explanatory: friendly relative dates with 12-hour clock, labeled duration with timer icon, color-coded pass rate badge with "Pass Rate:" label, and text-labeled status indicators ("Passed"/"Failed"/"Skipped") alongside colored dots. All changes are UI-only within the existing card template in `HistoryView.xaml`, plus two new WPF value converters and one updated converter.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only changes)
**Testing**: Manual visual verification (WPF desktop app)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single desktop application
**Performance Goals**: N/A (static card rendering, no measurable perf impact)
**Constraints**: Must work in both light and dark themes; must not break existing card hover/animation behavior
**Scale/Scope**: 1 XAML view, 2 new converters, 1 updated converter, App.xaml registration

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is not configured for this project (template placeholders only). No gates apply. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/026-premium-history-cards/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/ReqChecker.App/
├── Converters/
│   ├── DurationFormatConverter.cs      # MODIFY: add minutes+seconds format for ≥60s
│   ├── FriendlyDateConverter.cs        # NEW: "Today at", "Yesterday at", full date
│   └── PassRateToBrushConverter.cs     # NEW: green/amber/red brush based on thresholds
├── Views/
│   └── HistoryView.xaml                # MODIFY: update card item template
└── App.xaml                            # MODIFY: register 2 new converters
```

**Structure Decision**: Single WPF desktop project. All changes are in the presentation layer (`ReqChecker.App`). No new projects, services, or models needed.

## Complexity Tracking

No violations. This is a minimal UI refinement touching 5 files with no architectural changes.
