# Implementation Plan: Premium Page Headers

**Branch**: `018-premium-page-headers` | **Date**: 2026-02-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/018-premium-page-headers/spec.md`

## Summary

Enhance the page headers across all 4 main pages (Profile Manager, Test Suite, Results Dashboard, System Diagnostics) with a premium visual treatment. The current headers show a basic icon + title pattern. The new design adds gradient accent lines, icon containers with accent backgrounds, refined typography with subtitles, enhanced count badges, and subtle entrance animations.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only enhancement)
**Testing**: Visual verification, existing test infrastructure
**Target Platform**: Windows 10/11 (win-x64)
**Project Type**: Desktop WPF application
**Performance Goals**: Header animation completes within 300ms
**Constraints**: Must work in both light and dark themes
**Scale/Scope**: 4 pages, ~100 lines XAML per page header modification

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **No constitution defined** - Project uses template constitution without custom rules.

## Project Structure

### Documentation (this feature)

```text
specs/018-premium-page-headers/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Resources/
│   │   └── Styles/
│   │       └── Controls.xaml        # Add PremiumPageHeader style
│   └── Views/
│       ├── ProfileSelectorView.xaml # Update header
│       ├── TestListView.xaml        # Update header
│       ├── ResultsView.xaml         # Update header
│       └── DiagnosticsView.xaml     # Update header
```

**Structure Decision**: UI-only changes to existing XAML files. A new reusable `PremiumPageHeader` style will be added to Controls.xaml for consistency.

## Complexity Tracking

No violations. This is a straightforward UI enhancement:
- Reuses existing design tokens (AccentPrimary, AccentGradient, etc.)
- Modifies existing XAML structure, no new files needed
- Animation pattern already exists in the codebase
