# Phase 1 Data Model: Theme Palette Token Catalog

This feature has no domain data model. The "data model" here is the catalog of named theme tokens that compose a palette. Both `Colors.Dark.xaml` and `Colors.Light.xaml` define the same set of token names (a contract — see [contracts/theme-token-contract.md](./contracts/theme-token-contract.md)); only the values differ between themes.

## Token Categories

### Background tier

The vertical stack of surfaces, from page bottom to topmost.

| Token (Color + Brush key) | Role | Dark value | Light value |
|---|---|---|---|
| `BackgroundBaseColor` / `BackgroundBase` | Page background — the lowest visual tier | `#0b0d12` | `#e9ecf0` |
| `BackgroundSurfaceColor` / `BackgroundSurface` | Default card and panel surface | `#13161d` | `#ffffff` |
| `BackgroundElevatedColor` / `BackgroundElevated` | Modal, popover, menu — sits above surface | `#1c2029` | `#fdfdfe` |
| `BackgroundOverlayColor` / `BackgroundOverlay` | Disabled/overlay tint, scrim base | `#252a35` | `#d6dae0` |

**Validation rule**: All four values within a theme MUST be distinct (FR-002). Going up the stack, dark theme increases lightness; light theme decreases base lightness toward gray and pushes elevated whiter.

### Text tier

| Token | Role | Dark value | Light value |
|---|---|---|---|
| `TextPrimaryColor` / `TextPrimary` | Headings, primary body text | `#f0f2f5` | `#1a1d23` |
| `TextSecondaryColor` / `TextSecondary` | Subtitle, helper text | `#a8b0bd` | `#4b5563` |
| `TextTertiaryColor` / `TextTertiary` | De-emphasized labels | `#6e7787` | `#6b7280` |
| `TextDisabled` (brush only — aliases tertiary) | Disabled text | (= TextTertiary) | (= TextTertiary) |

**Validation rule**: Every text-tier on every reachable background-tier MUST meet WCAG AA — 4.5:1 normal text, 3:1 large/UI (FR-005). Disabled text is exempt per WCAG.

### Border tier

| Token | Role | Dark value | Light value |
|---|---|---|---|
| `BorderSubtleColor` / `BorderSubtle` | Internal dividers, faint card outlines | `#23272f` | `#e1e4ea` |
| `BorderDefaultColor` / `BorderDefault` | Standard input borders, card outlines | `#2e333d` | `#cbd0d8` |
| `BorderStrongColor` / `BorderStrong` | Emphasis borders, focus-adjacent | `#424955` | `#9ca3af` |

### Accent (single, non-gradient)

| Token | Role | Dark value | Light value |
|---|---|---|---|
| `AccentPrimaryColor` / `AccentPrimary` / `AccentPrimaryBrush` | Primary action emphasis: buttons, focus rings, selected nav, brand marks | `#4f7cff` | `#2c4cb8` |
| `AccentSubtle` (brush only) | 10% accent tint — selection backgrounds, faint highlights | `#1A4f7cff` | `#1A2c4cb8` |

**Validation rule**: A single accent applies to all primary action surfaces (FR-003). The legacy gradient tokens (`AccentGradient`, `AccentGradientHorizontal`) are **removed** in this feature (FR-014).

**Removed tokens** (no longer defined in either theme file):
- `AccentGradient`
- `AccentGradientHorizontal`
- `AccentSecondaryColor` / `AccentSecondary` / `AccentSecondaryBrush` — was the second gradient stop. Removed because there is no semantic role for "secondary accent" in a single-accent palette. Any remaining XAML references must be replaced with `AccentPrimary` or `BorderStrong`/`StatusInfo` depending on context.

### Status (semantic)

| Token | Role | Dark value | Light value |
|---|---|---|---|
| `StatusPassColor` / `StatusPass` | Test passed | `#22c55e` | `#16a34a` |
| `StatusFailColor` / `StatusFail` | Test failed | `#f87171` | `#dc2626` |
| `StatusSkipColor` / `StatusSkip` | Test skipped | `#fbbf24` | `#ca8a04` |
| `StatusInfoColor` / `StatusInfo` | Informational | `#38bdf8` | `#0284c7` |

**Validation rule** (FR-006, FR-007, SC-009):
- The four status colors AND the primary accent (5 colors total) MUST be pairwise distinct under deuteranopia and protanopia simulation.
- A single status badge MUST be visually quieter than a primary action button (saturation/lightness, not size).
- `StatusInfo` MUST NOT collide with `AccentPrimary` (forced apart by hue family — sky vs. cobalt).

### Status glow (badge effect color)

| Token | Role | Dark value | Light value |
|---|---|---|---|
| `StatusPassGlowColor` | Glow color used by `TestStatusBadge` for pass state (existing pattern) | `#4D22c55e` | `#4D16a34a` |
| `StatusFailGlowColor` | | `#4Df87171` | `#4Ddc2626` |
| `StatusSkipGlowColor` | | `#4Dfbbf24` | `#4Dca8a04` |

These exist because `TestStatusBadge` deliberately uses a **status-colored glow** to draw attention to a fail/skip in a long results list. This is the one place where colored glows are retained — they convey semantic meaning, not just decoration. Tuning is to keep them at 30% opacity (`4D` alpha) of the status hue.

### Focus ring

| Token | Role | Dark value | Light value |
|---|---|---|---|
| `FocusRingColor` / `FocusRing` | Keyboard focus halo | `#804f7cff` (50% accent) | `#4D2c4cb8` (30% accent) |

### Elevation (shadow, replaces glow)

The previous palette used cyan/indigo `ElevationGlow*` tokens consumed by `DropShadowEffect.Color`. Renamed to `ElevationShadow*` and recolored to neutral black-with-opacity. Animation storyboards continue to target the same `DropShadowEffect` properties, so they don't need editing.

| Token | Role | Dark value | Light value |
|---|---|---|---|
| `ElevationShadowColor` (renamed from `ElevationGlowColor`) | Resting card/panel shadow | `#80000000` (50% black) | `#26000000` (15% black) |
| `ElevationShadowHoverColor` (renamed from `ElevationGlowHoverColor`) | Card hover state | `#99000000` (60% black) | `#33000000` (20% black) |
| `ElevationShadowModalColor` (renamed from `ElevationGlowModalColor`) | Modal/dialog elevation | `#A6000000` (65% black) | `#40000000` (25% black) |

**Shadow geometry parameters** (set on `DropShadowEffect` directly, not as tokens — shadow-color is the only theme-varying element):
- Resting: BlurRadius=12 (dark) / 8 (light), ShadowDepth=2 (dark) / 1 (light), Opacity=1.0 (color carries opacity).
- Hover: BlurRadius=20 / 16, ShadowDepth=4 / 2.
- Modal: BlurRadius=32, ShadowDepth=8 / 4.

**Migration note**: existing `DropShadowEffect Color="{DynamicResource ElevationGlowColor}"` references must be updated to `ElevationShadowColor`. The token rename is a search-and-replace across `Controls.xaml` (4 references) and `Views/SettingsView.xaml` (4 references) and `Views/ResultsView.xaml` (1 reference).

### WPF-UI override brushes (unchanged structure, new values)

These brushes override WPF-UI's library defaults so the framework's `NavigationView`, button, etc. controls pick up the ReqChecker palette. The keys (`ApplicationBackgroundBrush`, `ControlFillColorDefaultBrush`, `TextFillColorPrimaryBrush`, `NavigationViewItemForeground*`, `NavigationViewItemBackgroundSelected`, etc.) stay defined; their values point at the new tokens above. No new override keys; no removed override keys.

## Token Lifecycle

This feature is a **single migration event**:

1. Old palette is in production today.
2. New palette is committed in 3 slices (P1 / P2 / P3 from spec).
3. After this feature merges, no parallel palette exists. Reverting requires a `git revert`.

There is no runtime token version, no theme schema version, and no fallback path to the old palette (FR-014, "Out of Scope: backwards-compatibility shims").

## State Transitions

Theme switching (light ↔ dark via Settings) continues to work via WPF resource-dictionary swap. The flow is unchanged:

```
User selects theme
  → IPreferencesService writes preferences.json
  → ThemeManager swaps loaded resource dictionary (Colors.Dark.xaml ↔ Colors.Light.xaml)
  → All DynamicResource bindings re-resolve
  → All views update without restart (FR-010)
```

The resource dictionary swap path is the existing mechanism — no change in this feature.
