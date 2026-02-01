# Research: Premium System Diagnostics Page

**Feature**: 016-premium-diagnostics-page
**Date**: 2026-02-01

## Research Summary

This feature requires defining missing XAML styles. Research focused on analyzing existing patterns in the codebase.

---

## R1: Existing Card Style Patterns

**Question**: What patterns do existing card styles follow in Controls.xaml?

**Findings**:

Analyzed `src/ReqChecker.App/Resources/Styles/Controls.xaml` lines 233-332.

**Decision**: Follow the established `Card` base style pattern with inheritance.

**Rationale**:
- Existing styles use a base `Card` style with `BasedOn` inheritance
- All card styles use DynamicResource for theme support
- Standard properties: Background, BorderBrush, BorderThickness, CornerRadius, Padding, Effect (DropShadowEffect)
- Hover effects use Style.Triggers with IsMouseOver

**Alternatives Considered**:
- Creating completely independent styles → Rejected (violates DRY, harder to maintain)
- Inline styles in DiagnosticsView.xaml → Rejected (not reusable, inconsistent with codebase)

---

## R2: Missing Style Analysis

**Question**: What styles are referenced in DiagnosticsView.xaml but not defined?

**Findings**:

Searched DiagnosticsView.xaml for StaticResource references:
- Line 150: `Style="{StaticResource DiagnosticCardHighlight}"` - NOT DEFINED
- Line 185: `Style="{StaticResource DiagnosticCard}"` - NOT DEFINED
- Line 220: `Style="{StaticResource DiagnosticCard}"` - NOT DEFINED
- Line 252: `Style="{StaticResource NetworkInterfaceCard}"` - NOT DEFINED

**Decision**: Define all three missing styles in Controls.xaml under a new "DIAGNOSTICS CARD STYLES" section.

**Rationale**:
- Keeping styles in Controls.xaml maintains consistency with other card styles
- Grouping under a section comment aids discoverability
- All three styles are needed for the page to render correctly

---

## R3: Visual Hierarchy Requirements

**Question**: How should the three card types differ visually to create premium hierarchy?

**Findings**:

Based on spec requirements:
- **DiagnosticCardHighlight** (Last Run Summary): Most prominent - accent border, enhanced shadow
- **DiagnosticCard** (Machine Info, Network): Standard prominence - normal border, standard shadow
- **NetworkInterfaceCard** (individual items): Subtle - minimal styling, used for list items

**Decision**:

| Style | Background | Border | Shadow | Padding | Margin |
|-------|------------|--------|--------|---------|--------|
| DiagnosticCardHighlight | BackgroundElevated | 2px AccentPrimary | Enhanced (blur 15) | 24px | 0,0,0,16 |
| DiagnosticCard | BackgroundSurface | 1px BorderDefault | Standard (blur 10) | 24px | 0,0,0,16 |
| NetworkInterfaceCard | BackgroundElevated | 1px BorderSubtle | None | 16px | 0,0,0,8 |

**Rationale**:
- Highlight card uses accent color to draw attention to latest run
- Standard cards use neutral styling for information density
- Interface cards are compact for list display

---

## R4: Theme Compatibility

**Question**: What DynamicResource keys are available for theme support?

**Findings**:

Checked Colors.Dark.xaml and Colors.Light.xaml for available resources:
- Backgrounds: BackgroundBase, BackgroundSurface, BackgroundElevated, BackgroundOverlay
- Borders: BorderSubtle, BorderDefault, BorderStrong
- Accents: AccentPrimary, AccentSecondary, AccentPrimaryColor (for shadows)
- Elevation: ElevationGlowColor, ElevationGlowHoverColor, ElevationGlowModalColor

**Decision**: Use only existing DynamicResource keys - no new color definitions needed.

**Rationale**:
- Existing resources provide sufficient variety
- Using established resources ensures automatic theme switching
- No custom colors means consistent design system adherence

---

## Implementation Notes

1. Add styles after the existing "CARD STYLES" section (after line ~332)
2. Use `<!-- DIAGNOSTICS CARD STYLES -->` section header
3. Follow exact property order used by existing styles
4. Include hover effects for DiagnosticCard and DiagnosticCardHighlight
5. NetworkInterfaceCard should be simple (no hover, used in ItemsControl)
