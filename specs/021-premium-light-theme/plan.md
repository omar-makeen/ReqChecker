# Implementation Plan: Premium Light Theme

**Branch**: `021-premium-light-theme` | **Date**: 2026-02-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/021-premium-light-theme/spec.md`

## Summary

Fix critical light theme visibility issue where navigation menu item text is invisible due to missing WPF-UI resource overrides. The navigation panel background color tokens are correctly defined for light mode, but WPF-UI's internal text foreground resources (NavigationViewItemForeground, TextFillColorPrimaryBrush, etc.) default to light colors designed for dark backgrounds. The fix requires adding these override resources to `Colors.Light.xaml`.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only fix, theme preference already persisted via IPreferencesService)
**Testing**: Manual visual verification (UI theming feature)
**Target Platform**: Windows 10/11 desktop application
**Project Type**: WPF desktop application (single project)
**Performance Goals**: Theme toggle completes within 500ms (already met)
**Constraints**: WCAG AA contrast compliance (4.5:1 minimum)
**Scale/Scope**: 4 navigation items, 4 pages to verify

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution is a template (not customized). No specific gates defined.

**Implicit Gates (Standard WPF Project):**
- [x] Changes are minimal and focused
- [x] No new dependencies required
- [x] Follows existing architecture (resource dictionary pattern)
- [x] No breaking changes to public APIs
- [x] Accessibility compliance maintained (WCAG AA)

## Project Structure

### Documentation (this feature)

```text
specs/021-premium-light-theme/
├── plan.md              # This file
├── research.md          # Root cause analysis and solution approach
├── data-model.md        # Color token documentation
├── quickstart.md        # Verification checklist
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Implementation tasks (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/ReqChecker.App/
├── Resources/
│   └── Styles/
│       ├── Colors.Light.xaml    # PRIMARY FILE TO MODIFY
│       ├── Colors.Dark.xaml     # Reference for parity
│       ├── Controls.xaml        # May need ElevationGlow reference check
│       └── Theme.xaml           # Theme configuration
├── Services/
│   └── ThemeService.cs          # Theme switching (no changes expected)
└── MainWindow.xaml              # Navigation structure (no changes expected)

tests/ReqChecker.App.Tests/
└── Services/
    └── ThemeServiceTests.cs     # Existing tests (verify still pass)
```

**Structure Decision**: Single WPF application project. All changes isolated to `Colors.Light.xaml` resource dictionary.

## Implementation Approach

### Phase 1: Critical Fix (P1)

Add WPF-UI text and navigation foreground resource overrides to `Colors.Light.xaml`:

**Text Fill Colors (WPF-UI global text resources):**
- `TextFillColorPrimaryBrush` → TextPrimaryColor
- `TextFillColorSecondaryBrush` → TextSecondaryColor
- `TextFillColorTertiaryBrush` → TextTertiaryColor
- `TextFillColorDisabledBrush` → TextTertiaryColor

**NavigationView Item Foregrounds:**
- `NavigationViewItemForeground` → TextSecondaryColor (normal state)
- `NavigationViewItemForegroundPointerOver` → TextPrimaryColor (hover)
- `NavigationViewItemForegroundSelected` → AccentPrimaryColor (selected)
- `NavigationViewItemForegroundSelectedPointerOver` → AccentPrimaryColor
- `NavigationViewItemForegroundPressed` → TextPrimaryColor
- `NavigationViewItemForegroundDisabled` → TextTertiaryColor

### Phase 2: Elevation Parity (P2)

Add missing `ElevationGlowColor` resources to `Colors.Light.xaml` (used by Controls.xaml):
- `ElevationGlowColor` → `#1A000000` (10% black shadow)
- `ElevationGlowHoverColor` → `#1A000000`
- `ElevationGlowModalColor` → `#26000000` (15% black shadow)

### Phase 3: Verification (P3)

- Build and run application
- Toggle between dark and light themes
- Verify all navigation states (normal, hover, selected)
- Check all pages for visual consistency
- Verify theme persistence on restart

## Files Modified

| File | Change Type | Description |
|------|-------------|-------------|
| `Colors.Light.xaml` | Modify | Add ~15 new resource brushes for WPF-UI overrides |

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| WPF-UI resource names incorrect | Low | Medium | Verify against WPF-UI source/docs |
| Incomplete resource coverage | Low | Low | Thorough testing of all nav states |
| Color contrast issues | Very Low | Medium | Already WCAG AA verified |

## Complexity Tracking

No complexity violations. This is a minimal, focused fix:
- Single file modification
- No new code files
- No architectural changes
- No new dependencies
