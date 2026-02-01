# Implementation Plan: Premium Button Styles & Consistency

**Branch**: `019-premium-buttons` | **Date**: 2026-02-01 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/019-premium-buttons/spec.md`

## Summary

This feature addresses button style and behavior inconsistencies across the ReqChecker WPF application. The implementation consolidates button patterns into a unified design system with smooth hover/press transitions, keyboard focus indicators, improved disabled states, and consistent icon-text spacing. All changes are UI-only, modifying XAML styles in Controls.xaml and updating button usages across view files.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only feature, no data persistence)
**Testing**: Manual visual testing, dotnet build verification
**Target Platform**: Windows 10+ (WPF desktop application)
**Project Type**: Desktop WPF application (3-project solution)
**Performance Goals**: Smooth 150-200ms hover transitions, 60fps animations
**Constraints**: Must work in both light and dark themes
**Scale/Scope**: 8 view files, 1 resource dictionary, ~18 button instances

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution is a template placeholder without project-specific rules. No gates to evaluate.

**Status**: PASS (no constitution violations)

## Project Structure

### Documentation (this feature)

```text
specs/019-premium-buttons/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # N/A (UI-only, no data model)
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/           # WPF application (target of changes)
│   ├── Resources/
│   │   └── Styles/
│   │       └── Controls.xaml # Button style definitions
│   ├── Views/
│   │   ├── TestListView.xaml
│   │   ├── ResultsView.xaml
│   │   ├── ProfileSelectorView.xaml
│   │   ├── DiagnosticsView.xaml
│   │   ├── TestConfigView.xaml
│   │   ├── RunProgressView.xaml
│   │   └── CredentialPromptDialog.xaml
│   └── MainWindow.xaml
├── ReqChecker.Core/          # Core library (no changes)
└── ReqChecker.Infrastructure/ # Infrastructure (no changes)

tests/
├── ReqChecker.App.Tests/     # App tests (no changes needed)
├── ReqChecker.Core.Tests/
└── ReqChecker.Infrastructure.Tests/
```

**Structure Decision**: Existing 3-project WPF solution structure. All changes target `ReqChecker.App` project only, specifically XAML resource dictionaries and view files.

## Complexity Tracking

No constitution violations to justify.

## Implementation Approach

### Phase 1: Style Enhancement (Controls.xaml)

1. **Add smooth transitions** to all button styles (PrimaryButton, SecondaryButton, GhostButton, IconButton)
   - Add Storyboard animations for hover state transitions (150ms duration)
   - Ensure press state remains instant (0.98 scale)

2. **Add keyboard focus indicators**
   - Add IsKeyboardFocused trigger to all button styles
   - Use 2px accent-colored border for focus ring
   - Ensure visibility in both light and dark themes

3. **Enhance disabled state**
   - Add not-allowed cursor (Cursors.No)
   - Add muted foreground color in addition to opacity 0.5
   - Suppress hover/focus states when disabled

### Phase 2: Style Consolidation

1. **Remove local FilterTabButton style** from ResultsView.xaml
2. **Update ResultsView** to use global FilterTab style from Controls.xaml
3. **Verify FilterTab style** has all necessary properties (or enhance it)

### Phase 3: Button Usage Fixes

1. **Standardize icon-text spacing** across all views
   - Ensure all icon margins are exactly "0,0,8,0"

2. **Remove fixed widths** from CredentialPromptDialog buttons
   - Replace Width="100px" with MinWidth="100"

3. **Audit button style assignments** across all views
   - Verify primary/secondary/ghost/icon usage is semantically correct

### Verification

1. `dotnet build src/ReqChecker.App` - Ensure compilation succeeds
2. Run application and manually verify:
   - Hover transitions are smooth on all button types
   - Press feedback is visible (scale effect)
   - Tab navigation shows focus rings on buttons
   - Disabled buttons show not-allowed cursor
   - Filter tabs in ResultsView use global style
   - Icon-text spacing is consistent across all pages

## Files to Modify

| File | Changes |
|------|---------|
| `src/ReqChecker.App/Resources/Styles/Controls.xaml` | Add transitions, focus indicators, enhanced disabled state |
| `src/ReqChecker.App/Views/ResultsView.xaml` | Remove local FilterTabButton, use global FilterTab |
| `src/ReqChecker.App/Views/CredentialPromptDialog.xaml` | Remove fixed button widths |
| Multiple view files | Standardize icon margins to 8px |

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Transition animations affect performance | Low | Medium | Use hardware-accelerated properties only |
| Focus ring not visible in some themes | Medium | Low | Test in both light and dark themes |
| FilterTab style migration breaks layout | Low | Medium | Compare old and new styles carefully |
