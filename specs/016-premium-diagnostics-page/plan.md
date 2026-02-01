# Implementation Plan: Premium System Diagnostics Page

**Branch**: `016-premium-diagnostics-page` | **Date**: 2026-02-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/016-premium-diagnostics-page/spec.md`

## Summary

Fix and enhance the System Diagnostics page by defining missing XAML card styles (DiagnosticCard, DiagnosticCardHighlight, NetworkInterfaceCard) in Controls.xaml. The page already has the correct structure and ViewModel - only the visual styling is incomplete due to undefined styles being referenced in DiagnosticsView.xaml.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only enhancement, uses existing in-memory data from IAppState)
**Testing**: Manual verification (UI changes)
**Target Platform**: Windows 10/11 desktop
**Project Type**: Single WPF application
**Performance Goals**: Page loads within 500ms, animations smooth at 60fps
**Constraints**: Must support both dark and light themes via DynamicResource bindings
**Scale/Scope**: Single page enhancement, 3 new styles to define

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Simplicity | PASS | Adding only necessary styles, no over-engineering |
| Consistency | PASS | Following existing Card style patterns in Controls.xaml |
| Theme Support | PASS | Using DynamicResource for all colors/brushes |

No violations - proceeding with design.

## Project Structure

### Documentation (this feature)

```text
specs/016-premium-diagnostics-page/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (N/A - UI only)
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (files to modify)

```text
src/ReqChecker.App/
├── Resources/
│   └── Styles/
│       └── Controls.xaml          # ADD: DiagnosticCard, DiagnosticCardHighlight, NetworkInterfaceCard styles
└── Views/
    └── DiagnosticsView.xaml       # VERIFY: Styles referenced correctly (already done)
```

**Structure Decision**: UI-only enhancement modifying existing style file. No new files needed beyond style additions.

## Complexity Tracking

No complexity violations - this is a straightforward style definition task following existing patterns.

## Design Decisions

### Style Inheritance Strategy

The new diagnostic card styles will extend the existing `Card` base style pattern:

| Style Name | Base | Purpose | Key Differences |
|------------|------|---------|-----------------|
| `DiagnosticCard` | `Card` | Standard diagnostic info card | Larger padding (24px), margin for spacing |
| `DiagnosticCardHighlight` | `DiagnosticCard` | Primary info card (Last Run) | Accent border, enhanced glow effect |
| `NetworkInterfaceCard` | None | Individual network interface item | Compact padding, subtle background |

### Theme Support

All styles will use these existing DynamicResource keys:
- `BackgroundSurface` / `BackgroundElevated` - card backgrounds
- `BorderDefault` / `BorderStrong` - card borders
- `AccentPrimary` / `AccentSecondary` - accent colors
- `ElevationGlowColor` / `ElevationGlowHoverColor` - shadow effects
- `StatusPass` / `StatusFail` - status indicators (already defined)

### Animation Integration

The existing `AnimatedDiagCard` style (defined in DiagnosticsView.xaml) handles entrance animations. The new card styles only define visual appearance - no animation changes needed.
