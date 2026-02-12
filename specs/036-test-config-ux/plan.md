# Implementation Plan: Improve Test Configuration Page UI/UX

**Branch**: `036-test-config-ux` | **Date**: 2026-02-12 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/036-test-config-ux/spec.md`

## Summary

Improve the Test Configuration page's visual density and clarity by reducing excessive spacing, simplifying locked field indicators, lightening card decoration, and ensuring visual consistency — all while preserving existing functionality. This is a UI-only change affecting 3 files with low blast radius.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0 (all existing — no new packages)
**Storage**: N/A (UI-only enhancement, no data persistence changes)
**Testing**: XUnit with Moq (dotnet test); primarily visual verification for this feature
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single desktop application
**Performance Goals**: Smooth 60fps rendering; removing DropShadowEffect improves GPU load
**Constraints**: Must fit all 3 sections on 1080p without scrolling (for tests with up to 5 parameters)
**Scale/Scope**: 3 files modified, 0 new files, 0 ViewModel changes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution is a blank template with no project-specific principles defined. No gates to enforce. **PASS**.

**Post-Phase 1 re-check**: No new dependencies, no new projects, no architectural changes. **PASS**.

## Project Structure

### Documentation (this feature)

```text
specs/036-test-config-ux/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: design decisions and rationale
├── data-model.md        # Phase 1: no data changes (UI-only)
├── quickstart.md        # Phase 1: build and verification guide
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Controls/
│   │   ├── LockedFieldControl.xaml        # MODIFY: simplify to inline layout
│   │   └── LockedFieldControl.xaml.cs     # NO CHANGE (DependencyProperty unchanged)
│   ├── Resources/
│   │   └── Styles/
│   │       ├── Controls.xaml              # MODIFY: ParameterGroupCard style
│   │       └── Spacing.xaml               # NO CHANGE (use existing tokens)
│   ├── ViewModels/
│   │   └── TestConfigViewModel.cs         # NO CHANGE
│   └── Views/
│       └── TestConfigView.xaml            # MODIFY: spacing, sizing, layout
├── ReqChecker.Core/                       # NO CHANGES
└── ReqChecker.Infrastructure/             # NO CHANGES

tests/                                     # NO CHANGES (visual verification only)
```

**Structure Decision**: Existing single-project WPF desktop application structure. No structural changes needed — this feature modifies 3 existing files in-place.

## Detailed Change Plan

### Change 1: ParameterGroupCard Style (Controls.xaml)

**Current**:
- Padding: 20
- Margin: 0,0,0,16
- DropShadowEffect: BlurRadius=10, Opacity=1, ElevationGlowColor
- BorderBrush: BorderDefault

**Target**:
- Padding: 16 (use existing CardPadding token value)
- Margin: 0,0,0,12 (use existing CardMargin token value)
- DropShadowEffect: **Remove entirely**
- BorderBrush: BorderSubtle (lighter than BorderDefault)

**Rationale**: See research.md R4, R7. Style is only used in TestConfigView — safe to modify.

### Change 2: LockedFieldControl.xaml

**Current**: Two-column layout
- Column 0: Bordered container (Background=Elevated, Border, Padding=10) with LockClosed24 icon (16px)
- Column 1: Bordered container (Background=Elevated, Border, Padding=12,10) with read-only TextBlock
- Root opacity: 0.7

**Target**: Single-row layout
- Single Border container (Background=Elevated, BorderBrush=BorderSubtle, Padding=10,8)
- Inline StackPanel: LockClosed24 icon (14px, TextTertiary) + TextBlock value
- Root opacity: 0.7 (unchanged)
- ToolTip preserved on container

**Rationale**: See research.md R2. Removes ~50px horizontal waste from the separate icon container.

### Change 3: TestConfigView.xaml

**Spacing reductions**:
- Page outer margin: 32 → 24 (Grid Margin)
- Header bottom margin: 24 → 16
- Section header bottom margin (within cards): 20 → 12
- Field row bottom margin: 16 → 12
- Footer margin adjustment: -32 → -24 (matches new page margin)
- Footer padding: 32,16 → 24,12

**Label column width**:
- All Grid ColumnDefinitions: 140 → 120

**Section header icons**:
- Icon badge Border: Width/Height 32 → 24
- Inner SymbolIcon: FontSize 16 → 14

**"Requires Admin" field (FR-008)**:
- Replace custom Badge+Shield markup with `<controls:LockedFieldControl Value="{Binding RequiresAdmin, StringFormat='{}{0}'}"/>` or bind to a "Yes"/"No" string computed from RequiresAdmin bool
- This may require adding a `RequiresAdminText` property to ViewModel or using a BoolToStringConverter

**Label truncation (FR-012)**:
- Add `TextTrimming="CharacterEllipsis"` and `ToolTip="{Binding Label}"` to parameter label TextBlocks in the ItemsControl DataTemplate

**Preserved**:
- AnimatedSection staggered entrance animations (no timing changes)
- All data bindings, commands, tab navigation
- PromptAtRunIndicator styling (only used here, but no changes needed)
- Save/Cancel/Back button functionality

### Change 3b: "Requires Admin" Display

**Option chosen**: Add a simple BoolToTextConverter or use a computed property. The simplest approach is to add `StringFormat` or use the existing bool-to-string conversion that WPF provides, displaying "Yes" or "No" through the LockedFieldControl.

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| ParameterGroupCard style change affects other views | None | N/A | Verified: only used in TestConfigView |
| InputField style accidentally modified | Low | Medium | Plan explicitly excludes InputField changes |
| Locked field simplification breaks data binding | Low | Medium | DependencyProperty (Value) unchanged in code-behind |
| Spacing too tight on lower resolutions | Low | Low | 1080p baseline; ScrollViewer still present |
| "Requires Admin" bool-to-text display | Low | Low | Simple string conversion, well-tested WPF pattern |

## Complexity Tracking

No constitution violations. No complexity justifications needed.
