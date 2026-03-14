# Implementation Plan: UX Quick Wins

**Branch**: `063-ux-quick-wins` | **Date**: 2026-03-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/063-ux-quick-wins/spec.md`

## Summary

Five independent UI/UX polish items: display profile name in RunProgress header, add test count badge to sidebar navigation, add Ctrl+E export shortcut on Results page, animate filter tab transitions, and complete tooltip coverage audit. All changes are UI-layer only with no new data persistence, services, or external dependencies.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2
**Storage**: N/A (in-memory session-only; no persistence changes)
**Testing**: xUnit + existing test infrastructure (`dotnet test tests/ReqChecker.App.Tests/`)
**Target Platform**: Windows 10/11 desktop (WPF)
**Project Type**: Desktop app (WPF)
**Performance Goals**: Animations complete in ≤200ms; no perceptible UI lag
**Constraints**: WPF-UI 4.2.0 NavigationViewItem has no native InfoBadge — custom overlay required
**Scale/Scope**: 5 independent changes touching ~8 files; no new files needed

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is unconfigured (template placeholders only). No gates to enforce. Proceeding.

**Post-Phase 1 re-check**: No violations. All changes are minimal, UI-layer only, follow existing patterns, and introduce no new dependencies or abstractions.

## Project Structure

### Documentation (this feature)

```text
specs/063-ux-quick-wins/
├── plan.md              # This file
├── research.md          # Phase 0: Technology decisions
├── data-model.md        # Phase 1: ViewModel property additions
├── quickstart.md        # Phase 1: Build/test/file guide
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/ReqChecker.App/
├── ViewModels/
│   ├── MainViewModel.cs           # Add TestCount/HasTests properties (P2)
│   ├── RunProgressViewModel.cs    # Add ProfileName property (P1)
│   └── ResultsViewModel.cs        # No changes (existing commands sufficient)
├── Views/
│   ├── MainWindow.xaml            # Add badge overlay on NavTests (P2)
│   ├── RunProgressView.xaml       # Add profile name TextBlock in header (P1)
│   ├── ResultsView.xaml           # Add InputBinding Ctrl+E (P3), fade Storyboard (P4)
│   └── ResultsView.xaml.cs        # Add filter transition animation helper (P4)
├── Controls/                      # Tooltip audit only (P5)
└── Resources/Styles/              # No changes expected

tests/ReqChecker.App.Tests/
└── ViewModels/
    ├── MainViewModelTests.cs      # Test TestCount/HasTests (P2)
    └── RunProgressViewModelTests.cs # Test ProfileName property (P1)
```

**Structure Decision**: All changes fit within existing project structure. No new files or directories needed (except possibly test additions).
