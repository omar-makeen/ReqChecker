# Research: Premium Light Theme

**Feature**: 021-premium-light-theme
**Date**: 2026-02-02

## Research Summary

This document captures research findings for fixing the light theme navigation visibility issue and achieving premium light theme styling.

---

## Investigation 1: Root Cause Analysis

### Problem Statement
Navigation menu item text is invisible in light mode. The navigation panel appears with a dark background while text colors appear to be light/white.

### Findings

**Current Implementation:**
- `Colors.Light.xaml` defines correct light theme colors:
  - `BackgroundBaseColor`: `#f8f9fa` (light gray)
  - `TextPrimaryColor`: `#1a1a2e` (dark blue-black)
  - `NavigationViewDefaultPaneBackground`: Uses `BackgroundBaseColor`

- `ThemeService.cs` applies themes by:
  1. Calling `ApplicationThemeManager.Apply()` for WPF-UI built-in theme
  2. Swapping `ThemesDictionary` in merged dictionaries
  3. Adding our custom `Colors.Light.xaml` at the END to override

**Root Cause Identified:**
The WPF-UI `NavigationView` component uses its own internal text foreground resources that are NOT being overridden by our light theme. Specifically:
- `NavigationViewItemForeground`
- `NavigationViewItemForegroundSelected`
- `NavigationViewItemForegroundPointerOver`
- `TextFillColorPrimaryBrush`
- `TextFillColorSecondaryBrush`

These WPF-UI resources use light text colors (white/light gray) designed for dark backgrounds, and our `Colors.Light.xaml` does not override them for light mode.

### Decision
Add WPF-UI NavigationView text foreground overrides to `Colors.Light.xaml`

### Rationale
This approach:
1. Maintains the existing architecture (theme switching via resource dictionaries)
2. Follows the established pattern of overriding WPF-UI defaults in our color files
3. Requires minimal code changes
4. Is consistent with how we already override `NavigationViewDefaultPaneBackground`

### Alternatives Considered
1. **Custom NavigationView template**: Too invasive, requires maintaining large template
2. **Inline Foreground bindings in XAML**: Would break when WPF-UI updates, not DRY
3. **CSS-like global styling**: Not available in WPF

---

## Investigation 2: WPF-UI Light Theme Text Resources

### Research Question
Which WPF-UI resources need to be overridden for proper light theme text visibility?

### Findings

WPF-UI uses these key resources for navigation text (from WPF-UI source code analysis):

**Primary Text Resources:**
```
TextFillColorPrimaryBrush      - Main text color
TextFillColorSecondaryBrush    - Secondary/dimmed text
TextFillColorTertiaryBrush     - Tertiary/muted text
TextFillColorDisabledBrush     - Disabled state text
```

**NavigationView Specific:**
```
NavigationViewItemForeground                    - Default item text
NavigationViewItemForegroundSelected            - Selected item text
NavigationViewItemForegroundPointerOver         - Hover state text
NavigationViewItemForegroundDisabled            - Disabled item text
NavigationViewItemHeaderForeground              - Section header text
```

**Header/Title Resources:**
```
TextOnAccentFillColorPrimaryBrush   - Text on accent-colored backgrounds
```

### Decision
Override all text-related WPF-UI resources in `Colors.Light.xaml` to use our light theme text colors.

### Rationale
Complete override ensures consistent text visibility across all navigation states and prevents future WPF-UI updates from breaking our theme.

---

## Investigation 3: Card Shadow Styling for Light Theme

### Research Question
How should card shadows differ between dark and light themes?

### Findings

**Current Dark Theme:**
- Uses cyan glow effect (`ElevationGlowColor`: `#0D00d9ff`)
- Creates "floating" premium feel on dark backgrounds

**Current Light Theme:**
- Uses black shadow (`ElevationShadowColor`: `#1A000000`)
- Appropriate for light backgrounds

**Analysis:**
The light theme already defines appropriate shadow colors. However, the Controls.xaml styles may be using `ElevationGlowColor` which doesn't exist in the light theme file.

### Decision
1. Ensure `ElevationGlowColor` and `ElevationGlowHoverColor` are defined in `Colors.Light.xaml` with appropriate values
2. Use semi-transparent black shadows for light theme instead of colored glows

### Rationale
- Light backgrounds benefit from subtle dark shadows (not colored glows)
- This matches modern light theme design patterns (Apple, Material Design)

---

## Investigation 4: Icon Visibility in Navigation Panel

### Research Question
Why are navigation icons potentially not visible in light mode?

### Findings

**Icon Resource Analysis:**
WPF-UI icons use `SymbolIcon` which inherits foreground from parent. The navigation item icon visibility depends on:
- `NavigationViewItemForeground` for the icon color
- Parent `Foreground` property on NavigationViewItem

**Current State:**
MainWindow.xaml sets `Foreground="{DynamicResource TextPrimary}"` on NavigationView, but WPF-UI may override this for child NavigationViewItems.

### Decision
Icons should inherit text foreground colors once NavigationViewItem foreground resources are properly overridden.

### Rationale
No separate icon color overrides needed - fixing the text foreground fixes icon visibility.

---

## Technical Approach Summary

### Phase 1: Critical Fix (Navigation Visibility)

Add these resources to `Colors.Light.xaml`:

```xml
<!-- WPF-UI Text Fill Colors -->
<SolidColorBrush x:Key="TextFillColorPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
<SolidColorBrush x:Key="TextFillColorSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
<SolidColorBrush x:Key="TextFillColorTertiaryBrush" Color="{StaticResource TextTertiaryColor}"/>
<SolidColorBrush x:Key="TextFillColorDisabledBrush" Color="{StaticResource TextTertiaryColor}"/>

<!-- NavigationView Item Foreground Colors -->
<SolidColorBrush x:Key="NavigationViewItemForeground" Color="{StaticResource TextSecondaryColor}"/>
<SolidColorBrush x:Key="NavigationViewItemForegroundPointerOver" Color="{StaticResource TextPrimaryColor}"/>
<SolidColorBrush x:Key="NavigationViewItemForegroundSelected" Color="{StaticResource AccentPrimaryColor}"/>
<SolidColorBrush x:Key="NavigationViewItemForegroundSelectedPointerOver" Color="{StaticResource AccentPrimaryColor}"/>
<SolidColorBrush x:Key="NavigationViewItemForegroundPressed" Color="{StaticResource TextPrimaryColor}"/>
<SolidColorBrush x:Key="NavigationViewItemForegroundDisabled" Color="{StaticResource TextTertiaryColor}"/>
```

### Phase 2: Elevation Colors Parity

Ensure light theme has `ElevationGlowColor` defined (using shadow-style values):

```xml
<!-- Elevation Glow/Shadow (for light theme - use subtle black shadows) -->
<Color x:Key="ElevationGlowColor">#1A000000</Color>
<Color x:Key="ElevationGlowHoverColor">#1A000000</Color>
<Color x:Key="ElevationGlowModalColor">#26000000</Color>
```

### Phase 3: Verification

1. Toggle between themes - all text visible
2. Check all navigation states (normal, hover, selected, disabled)
3. Review all pages for card/shadow consistency
4. Verify WCAG AA contrast compliance (already defined in Colors.Light.xaml comments)

---

## Files to Modify

| File | Changes |
|------|---------|
| `src/ReqChecker.App/Resources/Styles/Colors.Light.xaml` | Add WPF-UI text and navigation resource overrides |
| (No other files needed) | |

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| WPF-UI version update breaks overrides | Document which resources are overridden; pin WPF-UI version |
| Incomplete resource coverage | Test all navigation states thoroughly |
| Color contrast issues | Already verified WCAG AA compliance in Colors.Light.xaml |
