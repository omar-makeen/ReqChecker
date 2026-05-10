# Theme Token Contract

The "API surface" of this feature is the set of `x:Key`-named WPF resources (Color values and SolidColorBrush instances) that views and controls bind to. This document is the contract: what's added, kept, removed, and renamed.

A consumer (any XAML file in `ReqChecker.App`) is correct after this change if and only if every `{StaticResource <Key>}` and `{DynamicResource <Key>}` reference resolves to a key in the **Kept** or **Renamed → new name** rows below — and no reference points at a **Removed** key.

## Removed keys

These resource keys exist today and MUST NOT exist after this feature ships. Any straggler reference fails at XAML resource lookup and must be replaced.

| Key | Was used for | Replaced by |
|---|---|---|
| `AccentGradient` | LinearGradientBrush (cyan→indigo, diagonal) on primary surfaces | `AccentPrimary` brush (flat) |
| `AccentGradientHorizontal` | LinearGradientBrush (cyan→indigo, horizontal) on view headers and dialog headers | `AccentPrimary` brush (flat) |
| `AccentSecondaryColor` | Second stop of the deleted gradient | None — drop the reference, or substitute with `BorderStrong` / `StatusInfo` based on visual context (audit during slice 1) |
| `AccentSecondary` | (brush wrapper of above) | Same as above |
| `AccentSecondaryBrush` | (alias) | Same as above |
| `ElevationGlowColor` | Cyan-tinted glow shadow (dark theme) | `ElevationShadowColor` (renamed) |
| `ElevationGlowHoverColor` | Cyan-tinted glow shadow on hover | `ElevationShadowHoverColor` (renamed) |
| `ElevationGlowModalColor` | Cyan-tinted glow shadow on modals | `ElevationShadowModalColor` (renamed) |

**Verification**: `Grep "AccentGradient|AccentSecondary|ElevationGlow" src/` returns zero matches after the feature ships (FR-014, SC-004).

## Renamed keys

| Old key | New key | Reason |
|---|---|---|
| `ElevationGlowColor` | `ElevationShadowColor` | Glow → shadow accurately describes a neutral black-opacity drop shadow. |
| `ElevationGlowHoverColor` | `ElevationShadowHoverColor` | Same. |
| `ElevationGlowModalColor` | `ElevationShadowModalColor` | Same. |

The renames are mechanical — every `DropShadowEffect Color="{DynamicResource ElevationGlow*Color}"` site is updated to point at `ElevationShadow*Color`. 9 known sites: 4 in `Controls.xaml`, 4 in `Views/SettingsView.xaml`, 1 in `Views/ResultsView.xaml` (line 218 references `ElevationGlowHoverColor`).

## Kept keys (token names) — values change, names don't

Every key in the table below exists in both `Colors.Dark.xaml` and `Colors.Light.xaml` before and after this feature. Only the bound color values change. Consumers do not need to update their XAML for these.

**Background tier**: `BackgroundBaseColor`, `BackgroundBase`, `BackgroundSurfaceColor`, `BackgroundSurface`, `BackgroundElevatedColor`, `BackgroundElevated`, `BackgroundOverlayColor`, `BackgroundOverlay`.

**Text tier**: `TextPrimaryColor`, `TextPrimary`, `TextSecondaryColor`, `TextSecondary`, `TextTertiaryColor`, `TextTertiary`, `TextDisabled`.

**Border tier**: `BorderSubtleColor`, `BorderSubtle`, `BorderDefaultColor`, `BorderDefault`, `BorderStrongColor`, `BorderStrong`.

**Accent**: `AccentPrimaryColor`, `AccentPrimary`, `AccentPrimaryBrush`, `AccentSubtle`.

**Status**: `StatusPassColor`, `StatusPass`, `StatusFailColor`, `StatusFail`, `StatusSkipColor`, `StatusSkip`, `StatusInfoColor`, `StatusInfo`, `StatusPassGlowColor`, `StatusFailGlowColor`, `StatusSkipGlowColor`.

**Focus**: `FocusRingColor`, `FocusRing`.

**WPF-UI overrides** (all kept): `NavigationViewDefaultPaneBackground`, `NavigationViewExpandedPaneBackground`, `NavigationViewTopPaneBackground`, `ControlFillColorDefaultBrush`, `ControlFillColorSecondaryBrush`, `ControlFillColorTertiaryBrush`, `SubtleFillColorSecondaryBrush`, `SubtleFillColorTertiaryBrush`, `ApplicationBackgroundBrush`, `SolidBackgroundFillColorBaseBrush`, `SolidBackgroundFillColorSecondaryBrush`, `SolidBackgroundFillColorTertiaryBrush`, `LayerFillColorDefaultBrush`, `TextFillColorPrimaryBrush`, `TextFillColorSecondaryBrush`, `TextFillColorTertiaryBrush`, `TextFillColorDisabledBrush`, `NavigationViewItemForeground`, `NavigationViewItemForegroundPointerOver`, `NavigationViewItemForegroundSelected`, `NavigationViewItemForegroundSelectedPointerOver`, `NavigationViewItemForegroundPressed`, `NavigationViewItemForegroundDisabled`, `NavigationViewItemHeaderForeground`, `ToggleButtonForeground`, `ToggleButtonForegroundPointerOver`, `ToggleButtonForegroundPressed`, `ToggleButtonForegroundDisabled`, `NavigationViewItemBackgroundPointerOver`, `NavigationViewItemBackgroundSelected`.

## Added keys

None. This feature does not introduce new tokens; it removes 8 (`AccentGradient`, `AccentGradientHorizontal`, the 3 `AccentSecondary*`, the 3 `ElevationGlow*Color` — net of renames). Keeping the public surface lean is a deliberate decision aligned with FR-014.

## Consumer migration summary

A view or control consumer is one of three things in this feature:

| Consumer kind | Action required |
|---|---|
| Uses only **Kept** keys | None — the new values pick up automatically through `DynamicResource` re-resolution. |
| References a **Removed** key | Edit the XAML site to point at the replacement key (per "Removed keys" table). |
| References a **Renamed** key | Mechanical search-and-replace: `ElevationGlow*Color` → `ElevationShadow*Color`. |
| Hardcodes a hex value (`#00d9ff`, `#6366f1`, etc.) | Replace with the appropriate `DynamicResource` brush. (3 sites: `ProgressRing.xaml`, `SummaryCard.xaml`, `RunProgressView.xaml`.) |

## Verification commands (post-implementation)

Run from the repo root:

```powershell
# 1. No removed keys remain anywhere in source
Grep "AccentGradient|AccentSecondary|ElevationGlow" src/  # → 0 matches

# 2. No hardcoded violet/cyan/indigo hex remains
Grep "#0f0f1a|#1a1a2e|#252542|#2f2f52|#00d9ff|#6366f1" src/ | grep -v "specs/"  # → 0 matches (excluding spec docs)

# 3. No "Premium" wording in source
Grep -i "premium" src/  # → 0 matches

# 4. Both new accent values are present
Grep "#4f7cff" src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml  # → ≥1 match
Grep "#2c4cb8" src/ReqChecker.App/Resources/Styles/Colors.Light.xaml  # → ≥1 match
```

Any of these returning unexpected results blocks the slice from merging.
