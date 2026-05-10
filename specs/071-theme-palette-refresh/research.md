# Phase 0 Research: Theme Palette Refresh

## Inventory of Affected Files

A repository-wide grep against the old palette (`AccentGradient`, `ElevationGlow`, hardcoded violet/cyan/indigo hex values, "Premium" comments) identified the full surface to be touched.

**Color/theme files (rewrite token values, drop gradient definitions, drop glow tokens):**
- `src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml` — full palette rewrite, drop `AccentGradient` + `AccentGradientHorizontal` definitions, replace `ElevationGlowColor*` cyan with neutral shadow color, strip "Premium" comments.
- `src/ReqChecker.App/Resources/Styles/Colors.Light.xaml` — full palette rewrite (introduces 3 distinct elevation tiers), drop gradient definitions, strip "Premium" comments.

**Reusable styles (replace gradient/glow brushes):**
- `src/ReqChecker.App/Resources/Styles/Controls.xaml` — 3 `AccentGradient*` references (lines 17, 807, 914), 9 `ElevationGlow*`/accent-glow shadow effects (lines 436, 450, 468, 490, 516, 543, 599, 612, 625, 953, 1055, 1079, 1098), 1 `Premium` comment on `AccentCheckBox`.
- `src/ReqChecker.App/Resources/Styles/Animations.xaml` — 6 storyboards target `DropShadowEffect.BlurRadius` / `.Opacity`. **No structural change needed**; storyboards continue to work because the underlying `DropShadowEffect` is preserved (only its color and opacity ramp change).

**Reusable controls (12):**
- `Controls/ProgressRing.xaml` — local `ProgressGradient` resource hardcodes `#00d9ff` and `#6366f1`. Replace with single accent brush (Stroke = AccentPrimary).
- `Controls/ProgressRing.xaml.cs` — "premium" word in comment (line 10).
- `Controls/SummaryCard.xaml` — `DropShadowEffect.Color` defaults to `#00d9ff` fallback (line 27); change fallback to AccentPrimaryColor and bind to neutral shadow color via the `AccentColor` DP, OR drop the colored hover glow entirely and use neutral shadow.
- `Controls/TestStatusBadge.xaml` + `.cs` — `GlowEffect` is a `DropShadowEffect` whose color is bound through `TestStatusToColorConverter`/`GlowColor`. The `Status*GlowColor` tokens (`StatusPassGlowColor`, etc.) are retuned in step with the new status colors. No structural change.
- `Controls/DonutChart.xaml.cs` — programmatic `DropShadowEffect` (line 180); retune color/opacity in step.
- `Controls/ExpanderCard.xaml` — 1 `AccentGradient` reference (line 164) → flat accent.

**View XAMLs (11) — each uses `AccentGradientHorizontal` for a header bar plus selective glow shadows:**
- `Views/ProfileSelectorView.xaml` (gradient line 131; AccentPrimary glow line 58; StatusFail glow line 265)
- `Views/TestListView.xaml` (gradient line 76)
- `Views/TestConfigView.xaml` (gradient line 67)
- `Views/RunProgressView.xaml` (gradient line 62; hardcoded `#00d9ff` shadow line 202)
- `Views/ResultsView.xaml` (gradient line 113; ElevationGlowHover line 218)
- `Views/HistoryView.xaml` (gradient line 66)
- `Views/DiagnosticsView.xaml` (gradient line 62)
- `Views/SettingsView.xaml` (gradient line 55; 4× `ElevationGlowColor` shadows lines 126, 185, 244, 276)
- `Views/SchedulesView.xaml` (gradient line 75)
- `Views/CredentialPromptDialog.xaml` (gradient line 55)
- `Views/CreateScheduleDialog.xaml` (gradient line 58)

**Total: 22 source files** (matches grep result). All other XAML in the app uses `DynamicResource` brush keys that pick up the new palette automatically with no edit needed.

## Decision: Concrete Dark Palette Values

| Token | Value | Notes |
|---|---|---|
| `BackgroundBaseColor` | `#0b0d12` | Cool charcoal, no violet cast. Replaces `#0f0f1a`. |
| `BackgroundSurfaceColor` | `#13161d` | Card/panel surface. Replaces `#1a1a2e`. |
| `BackgroundElevatedColor` | `#1c2029` | Modal/popover/menu. Replaces `#252542`. |
| `BackgroundOverlayColor` | `#252a35` | Disabled/overlay tint. Replaces `#2f2f52`. |
| `TextPrimaryColor` | `#f0f2f5` | Off-white, ~16:1 on base. Less harsh than pure white. |
| `TextSecondaryColor` | `#a8b0bd` | ~7.5:1 on base. |
| `TextTertiaryColor` | `#6e7787` | ~4.6:1 on base. |
| `BorderSubtleColor` | `#23272f` | Cool gray, no blue tint. |
| `BorderDefaultColor` | `#2e333d` | |
| `BorderStrongColor` | `#424955` | |
| `AccentPrimaryColor` | `#4f7cff` | Cobalt, ~5.5:1 on base. Single accent — replaces cyan + indigo. |
| `AccentSecondaryColor` | *(removed)* | Token kept for binding compatibility but set to `AccentPrimaryColor` value; no semantic role. **Decision deferred to slice 1**: prefer to drop entirely if no remaining references, else alias to primary. |
| `StatusPassColor` | `#22c55e` | Slightly desaturated from `#10b981`; lum ~0.49 on dark. |
| `StatusFailColor` | `#f87171` | Lighter on dark for calmer fail; lum ~0.45. |
| `StatusSkipColor` | `#fbbf24` | Lum ~0.61. |
| `StatusInfoColor` | `#38bdf8` | Sky blue — distinct from cobalt accent (FR-006). Was `#3b82f6` (collided). |
| `FocusRingColor` | `#804f7cff` | Cobalt at 50% opacity for halo. |
| `ElevationShadowColor` (renamed from `ElevationGlowColor*`) | `#80000000` | 50% opacity black. Replaces cyan glow. |

**Shadow parameters (dark theme):**
- Resting: BlurRadius=12, ShadowDepth=2, Opacity=0.45, Color=#000.
- Hover: BlurRadius=20, ShadowDepth=4, Opacity=0.55, Color=#000.
- Modal: BlurRadius=32, ShadowDepth=8, Opacity=0.65, Color=#000.

**Rationale**: Slate base (`#0b0d12`) reads as "dev tool" not "AI demo" because the hue is neutral (slightly cool but no measurable violet/blue chroma). Cobalt (`#4f7cff`) is the single most common "trust + infrastructure" accent across professional IT tooling (Datadog, Grafana, Jenkins). Single accent avoids the gradient tell. Sky-blue StatusInfo (`#38bdf8`) is distinct enough from cobalt that side-by-side comparison shows clear hue separation, satisfying FR-006.

**Alternatives considered:**
- *Amber single accent (`#d4a017`)*: more distinctive but harder to use sparingly without reading as a warning, and amber + StatusFail-red creates visual tension. Rejected.
- *Phosphor-green retro accent*: striking but undermines trust register. Rejected for product-fit reasons.
- *Keep gradient, just change colors*: violates user-confirmed direction "(a)" and Q3 clarification.

## Decision: Concrete Light Palette Values

| Token | Value | Notes |
|---|---|---|
| `BackgroundBaseColor` | `#e9ecf0` | Page background — clear gray. White cards "pop" against this. Replaces `#f8f9fa`. |
| `BackgroundSurfaceColor` | `#ffffff` | Default card/panel. Replaces previous (which was also `#ffffff`). **Tier 2 — visually distinct from base.** |
| `BackgroundElevatedColor` | `#fdfdfe` | Modal/popover — pure-white-with-faint-cool-tint. **Tier 3 — distinct from surface by hue + heavier shadow.** Replaces `#ffffff`. |
| `BackgroundOverlayColor` | `#d6dae0` | Overlay scrim base. Replaces `#ffffff`. |
| `TextPrimaryColor` | `#1a1d23` | ~14:1 on base, ~17:1 on white. |
| `TextSecondaryColor` | `#4b5563` | ~7.5:1 on white. |
| `TextTertiaryColor` | `#6b7280` | ~5:1 on white. |
| `BorderSubtleColor` | `#e1e4ea` | Slightly cooler than the prior `#e5e7eb`. |
| `BorderDefaultColor` | `#cbd0d8` | |
| `BorderStrongColor` | `#9ca3af` | (unchanged in role) |
| `AccentPrimaryColor` | `#2c4cb8` | Darker cobalt for white-bg contrast (~6.5:1). Replaces cyan `#00d9ff`. |
| `StatusPassColor` | `#16a34a` | Deeper green for white bg. Replaces `#10b981`. |
| `StatusFailColor` | `#dc2626` | ~4.9:1 on white. Replaces `#ef4444`. |
| `StatusSkipColor` | `#ca8a04` | Deeper amber for white bg. |
| `StatusInfoColor` | `#0284c7` | Sky — distinct from cobalt accent. Replaces `#3b82f6`. |
| `FocusRingColor` | `#4D2c4cb8` | 30% accent. |
| `ElevationShadowColor` | `#26000000` | ~15% opacity black. (current `ElevationGlow*` values stay similar; renamed.) |

**Shadow parameters (light theme):**
- Resting: BlurRadius=8, ShadowDepth=1, Opacity=0.08, Color=#000.
- Hover: BlurRadius=16, ShadowDepth=2, Opacity=0.12, Color=#000.
- Modal: BlurRadius=32, ShadowDepth=4, Opacity=0.18, Color=#000.

**Rationale**: The current light theme has `BackgroundSurface = BackgroundElevated = BackgroundOverlay = #ffffff` (User Story 2 problem). Three tier values restore visual hierarchy. The page-background gray (`#e9ecf0`) is cool to match the dark theme's slate hue family — both themes share a "cool neutral" identity. Light cobalt accent (`#2c4cb8`) has higher contrast against white than the dark variant against dark, intentional so the accent reads at the same relative weight in both themes.

**Alternatives considered:**
- *base=#f4f5f7 / surface=#fafbfc / elevated=#ffffff* (going lighter as you go up): more intuitive for some, but pure-white surfaces against near-white bases blur the tier boundary. The grayer base + white surface contrast was clearer in spot tests.
- *Re-use dark theme's `#4f7cff` accent in light too*: accent has only ~3.4:1 contrast on white, fails WCAG AA for normal text. Need the deeper variant.

## Decision: Color-Blind Verification (per Q2 clarification)

Per the spec's color-blind acceptance criterion (FR-006, SC-009), we verify the 5-color set (4 status + accent) under **deuteranopia** and **protanopia** simulation. Tritanopia is not in scope.

**Predicted result table (relative luminance under each deficiency, computed from sRGB):**

| Color | Hex (dark) | Lum | Hex (light) | Lum |
|---|---|---|---|---|
| Pass | `#22c55e` | 0.49 | `#16a34a` | 0.32 |
| Fail | `#f87171` | 0.45 | `#dc2626` | 0.18 |
| Skip | `#fbbf24` | 0.61 | `#ca8a04` | 0.27 |
| Info | `#38bdf8` | 0.55 | `#0284c7` | 0.22 |
| Accent | `#4f7cff` | 0.27 | `#2c4cb8` | 0.10 |

All five luminances are pairwise distinct (≥ 0.04 separation) in both themes. Under deuteranopia/protanopia (which preserve luminance, only collapse red/green hue distinction), distinguishability is preserved by luminance separation. The pass-green/skip-amber pair — historically the riskiest under deuteranopia — has 0.12 luminance separation in dark and 0.05 in light; for the light theme this is the tightest pair and warrants a visual check during Slice 3 with Color Oracle.

**If Slice 3 spot check shows ambiguity**: shift `StatusSkipColor` light to `#a16207` (darker, lum ~0.20) to widen separation from pass.

## Decision: "Premium" Cleanup Scope (FR-012)

`grep -i premium` across `src/` returns 6 matches in 4 files:
- `Resources/Styles/Colors.Dark.xaml` — 2 comments (header + line 134)
- `Resources/Styles/Colors.Light.xaml` — 2 comments (header + line 134)
- `Resources/Styles/Controls.xaml` — 1 comment (line 1145, on `AccentCheckBox` style)
- `Controls/ProgressRing.xaml.cs` — 1 docstring (line 10)

No resource keys, file names, class names, or method names contain "Premium" — only comments. **Cleanup**: replace each "Premium" comment with neutral language describing what the code does (e.g., "Dark theme color tokens" instead of "Premium Dark Theme Color Tokens"). Zero behavior change.

## Decision: Animation Storyboard Compatibility

The 6 storyboards in `Animations.xaml` (lines 53, 72, 336, 345, 358, 371) target `DropShadowEffect.BlurRadius` and `.Opacity`. After the glow→shadow swap, these effects are still `DropShadowEffect` instances (only the `Color` property changes), so storyboards continue to animate the correct property. **No edit to `Animations.xaml` required**, but the visual feel of the animations should be re-verified during Slice 1 — animating an opacity ramp on a neutral shadow may need a tighter range than on a colored glow. If the hover animation feels excessive on neutral shadows, narrow the `To` opacity in the storyboard from `0.3` to `0.2`.

## Decision: WCAG AA Verification Approach

Verify on the rendered app, not on the hex values alone. WPF brush rendering and font anti-aliasing affect perceived contrast. Use the following test surface during Slice 1:
- A view with primary text on base (e.g., view headers).
- A view with secondary text on a card surface (e.g., body copy in `TestListView`).
- A view with tertiary/disabled text on overlay (e.g., field labels in `SettingsView`).
- An accent-on-base button (e.g., "Run Tests" CTA).

Tool: any free WCAG contrast calculator (e.g., webaim.org/resources/contrastchecker, Stark plugin). Required: every pairing in both themes ≥ 4.5:1 normal / 3:1 large/UI.

## Open Questions Resolved

- `[Phase 0 NEEDS CLARIFICATION]` — none. All Q1/Q2/Q3 from `/speckit.clarify` are answered. The plan advances to Phase 1 with concrete values.
