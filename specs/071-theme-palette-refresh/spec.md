# Feature Specification: Theme Palette Refresh

**Feature Branch**: `071-theme-palette-refresh`
**Created**: 2026-05-10
**Status**: Draft
**Input**: User description: "Replace the AI-demo cyan/indigo gradient and violet-navy palette with a credible IT-tooling slate-and-cobalt single-accent palette across both dark and light themes; drop the cyan elevation glows in favor of real shadows; restore visual hierarchy in the light theme."

## Clarifications

### Session 2026-05-10

- Q: Should the application detect Windows High-Contrast mode and defer to Windows system colors, force the ReqChecker palette regardless, or ship a third in-app "high-contrast" theme variant? → A: Always force the ReqChecker palette regardless of Windows HC mode (no HC detection, no third theme). The palette must meet WCAG AA on its own; users who need HC rely on OS-level tools (magnifier, inversion).
- Q: How rigorously must color-blind safety be verified for the new palette? → A: Verify the four status colors (pass/fail/skip/info) AND the primary accent remain mutually distinguishable under deuteranopia and protanopia simulation (the two common red-green deficiencies, ~7% of males combined). Tritanopia coverage is not required. Verification uses a color-blindness simulation tool (e.g., Color Oracle, Sim Daltonism).
- Q: What happens to the existing `AccentGradient` / `AccentGradientHorizontal` resource tokens in `Colors.Dark.xaml` / `Colors.Light.xaml`? → A: Delete the gradient token definitions outright. Every XAML reference to them must be removed as part of this change; there is no compatibility shim, no redefined-as-solid alias, and no replacement gradient token. Any straggler reference will fail at resource resolution, which is the desired enforcement.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Professional visual register for IT tooling (Priority: P1)

An IT pro, sysadmin, or deployment engineer opens ReqChecker — often in front of stakeholders, on a screen-share, or while gating a production rollout. They need the application's visual register to read as **professional infrastructure tooling** (in the same family as Grafana, Wireshark, Datadog, Server Manager) rather than as a consumer SaaS marketing demo or AI-generated mockup. The current cyan→indigo gradient on a violet-navy ground undermines that credibility.

**Why this priority**: This is the core problem the user raised. ReqChecker gates production deployments; the UI must convey trust. Without this story shipping, the "AI-generated" perception persists no matter what other improvements ship later.

**Independent Test**: Open the app side-by-side with the current build. A user familiar with IT tooling categorizes each as either "professional tool" or "demo/marketing site." The new palette is consistently categorized as "professional tool."

**Acceptance Scenarios**:

1. **Given** a user opens ReqChecker in dark mode for the first time, **When** they view any primary view (Profile Selector, Test List, Run Progress, Results, History, Diagnostics, Settings, Schedules), **Then** the dominant visual elements use a neutral slate/charcoal background (no violet or purple cast) and a single cobalt accent — no cyan→indigo gradients are visible anywhere.
2. **Given** a user is on a Run Progress, Test Config, or Results view, **When** they look at primary action buttons, headers, selected navigation items, and focus rings, **Then** all primary action emphasis uses a single flat accent color (not a multi-stop gradient).
3. **Given** a user has the app open during a screen-share with stakeholders, **When** stakeholders see the UI, **Then** no element looks decorative-only (cyan glows around cards, gradient hero badges, "Premium" treatment) — every visual treatment conveys structure or status.

---

### User Story 2 — Light theme has real visual hierarchy (Priority: P2)

A user on light theme can visually distinguish cards, panels, and elevated surfaces from the page background at a glance. Currently the light theme defines `BackgroundSurface`, `BackgroundElevated`, and `BackgroundOverlay` all as the same `#ffffff`, so a card sitting on a white-ish page has no perceivable elevation; the layout reads as a flat document rather than a structured tool.

**Why this priority**: The light theme is functionally broken for hierarchy. This is a higher-impact fix than the dark-mode refresh from a usability standpoint, but lower urgency than the overall register problem. A user on light mode benefits immediately and unambiguously.

**Independent Test**: Open any list-on-card view (Profile Selector, Test List, History) in light mode. Cards are clearly distinguishable from the page background by surface color and shadow without any user effort.

**Acceptance Scenarios**:

1. **Given** a user is in light mode on any view containing cards or panels, **When** they look at the screen, **Then** every card has a visibly different surface color from the page background and casts a subtle real shadow (not a colored glow).
2. **Given** a user opens the Settings or Diagnostics view in light mode, **When** they scan the layout, **Then** they can identify each grouped section as a distinct elevated container without squinting or relying solely on borders.

---

### User Story 3 — Status signals remain calm and effective (Priority: P3)

A user reviewing a 40-row results table can quickly locate failed/skipped tests because status colors (pass/fail/skip/info) remain distinguishable, but the colors are calm enough that a long table doesn't feel like a fire alarm. Status colors do not visually compete with primary action emphasis (the "Run Tests" button is louder than any single status badge), and status-info does not collide with the primary accent.

**Why this priority**: Status colors are already functional in the current palette; this story refines them. It must be done as part of this change because the new accent could otherwise collide with `StatusInfo` (both are blue), and the existing saturated greens/reds become disproportionately loud against a calmer neutral ground.

**Independent Test**: A reviewer scans a 40-row results table containing a mix of pass/fail/skip/info statuses and locates a specific failed test in under 2 seconds; the same reviewer reports that the table feels "scannable" rather than "loud."

**Acceptance Scenarios**:

1. **Given** a results view with mixed pass/fail/skip/info statuses, **When** the user scans the list, **Then** each status is unambiguously distinguishable from the others at a glance and from the primary accent.
2. **Given** the new primary accent is a blue/cobalt, **When** a row contains a "Status: Info" badge alongside a primary action button, **Then** the user can tell which is the actionable element.
3. **Given** a results table with many failed rows, **When** the user views it, **Then** the visual loudness is proportionate (failed rows are noticeable but the table is not painful to look at).

---

### Edge Cases

- A user toggling theme (light ↔ dark) via Settings mid-session: the new palette must apply across every open view without restart and without leaving any control rendered with a stale token reference.
- A user with mild color-vision deficiency: under deuteranopia and protanopia simulation, pass/fail/skip/info MUST remain pairwise distinguishable from each other AND from the primary accent (verified with a simulation tool such as Color Oracle). Tritanopia is not part of the verification surface.
- A user on a low-quality projector during a presentation: muted status colors and a calmer accent must still read clearly under washed-out display conditions.
- A user who previously customized a theme preference in `preferences.json`: the existing preference (light/dark) is honored without requiring re-onboarding or migration.
- A user viewing a chart (donut, summary card): chart fills must use the new accent and status colors, not orphaned gradient or glow references.
- A view that previously used a gradient for visual interest (e.g., navigation selection rail, "premium" buttons): after this change, that view must still read as intentional and finished — not as if a gradient was deleted and forgotten.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST present a neutral slate/charcoal dark theme with no violet or purple color cast in any background, surface, border, or elevation layer.
- **FR-002**: The application MUST present a light theme in which the page background, card surfaces, and elevated surfaces are three visually distinguishable tiers (no two tiers may be the same color).
- **FR-003**: The application MUST use a single flat accent color for primary action emphasis (primary buttons, focus rings, selected navigation items, brand-ish header marks) — no multi-stop gradients on primary surfaces.
- **FR-004**: The application MUST convey card/panel elevation through neutral shadows in both themes (a subtle dark shadow in light mode; a slightly stronger shadow or thin border treatment in dark mode) — no colored glows.
- **FR-005**: The application MUST maintain WCAG AA contrast (4.5:1 for normal text, 3:1 for large text and UI elements) for every text-on-background and icon-on-background pairing in both themes.
- **FR-006**: The four status colors (pass/fail/skip/info) MUST remain visually distinguishable from one another AND from the primary accent in both themes; specifically, `StatusInfo` MUST NOT collide with the primary accent (currently both are blue at `#3b82f6`-ish).
- **FR-007**: Status colors MUST be tuned so that a single status badge in a row is visually quieter than a primary action button — primary actions remain the loudest element on any view.
- **FR-008**: The application MUST apply the new palette to every view and every reusable control (buttons, cards, badges, navigation, progress indicators, charts, dialogs) so no surface retains the prior gradient/glow/violet treatment.
- **FR-009**: The application MUST honor the user's existing theme preference (light/dark) from `preferences.json` without requiring re-onboarding, migration, or manual reset.
- **FR-010**: Theme switching at runtime MUST continue to update every visible view without an application restart.
- **FR-011**: All chart and data-visualization fills (donut chart, summary cards, progress rings) MUST source their colors from the new palette tokens — no hardcoded gradient or cyan references may remain.
- **FR-012**: Resource and style names that signal a marketing register (e.g., "Premium" in comments and resource group labels) MUST be revised so the codebase no longer reinforces the demo aesthetic at a code-review level.
- **FR-013**: The application MUST always render with the ReqChecker palette regardless of the Windows High-Contrast accessibility setting — Windows HC mode is NOT detected, and no separate in-app "high-contrast" theme variant is provided. Users requiring high-contrast rely on OS-level accessibility tools (magnifier, color inversion).
- **FR-014**: The `AccentGradient` and `AccentGradientHorizontal` resource definitions MUST be removed from both `Colors.Dark.xaml` and `Colors.Light.xaml`. Every XAML reference to those keys MUST be removed or replaced with a flat accent / status / surface brush as appropriate. No compatibility alias (e.g., a `SolidColorBrush` reusing the gradient key name) is permitted. After this change, a repository-wide search for `AccentGradient` returns zero matches.

### Key Entities

- **Theme palette**: The set of named color tokens that compose a single theme — background tiers (base / surface / elevated / overlay), text tiers (primary / secondary / tertiary / disabled), border tiers (subtle / default / strong), accent (single), status colors (pass / fail / skip / info), focus ring, elevation (shadow values). Two variants exist: a dark variant and a light variant.
- **Elevation system**: The visual treatment that conveys "this surface is above the page" — replaces the prior cyan/indigo glow approach with neutral shadows in both themes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: When the user (acting as PO) reviews the new build, they describe the visual register as "professional infrastructure tooling" (or equivalent — "looks like a real tool", "doesn't look AI-generated") rather than "demo," "marketing," or "AI-made."
- **SC-002**: In light theme, 100% of card and panel surfaces are visually distinguishable from the page background in a 1-second visual scan (no card blends into the page).
- **SC-003**: 100% of text/background and icon/background pairings in both themes meet WCAG AA contrast (4.5:1 normal, 3:1 large/UI).
- **SC-004**: Zero gradient brushes are referenced anywhere in the application's XAML. A repository-wide search for `AccentGradient`, `AccentGradientHorizontal`, and any inline `LinearGradientBrush`/`RadialGradientBrush` used as a primary accent surface returns zero matches; the token definitions themselves are removed from both theme files.
- **SC-005**: In a 40-row mixed-status results table, a user locates a specific failed test in under 2 seconds (status colors remain effective signals).
- **SC-006**: The primary accent color and `StatusInfo` color are independently identifiable in side-by-side comparison; users do not confuse a primary action button with a "status: info" badge.
- **SC-007**: Switching theme at runtime via Settings (light ↔ dark) updates every open view with no missing or broken color references and no app restart required.
- **SC-008**: The user's existing theme preference is preserved without prompting after the update is installed (no re-onboarding required).
- **SC-009**: Under deuteranopia and protanopia simulation (verified with Color Oracle or equivalent tool), the four status colors (pass / fail / skip / info) and the primary accent remain pairwise distinguishable in both light and dark themes — no two of the five colors collapse to a perceptually identical hue.

## Assumptions

- The existing token structure in `Colors.Dark.xaml` and `Colors.Light.xaml` (token names, file layout, WPF-UI override mechanism) is sound and remains; only token *values* and gradient/glow definitions change.
- The user has approved direction "(a) — slate + cobalt single-accent palette swap" from the earlier discussion; no preview/A-B toggle is required.
- Existing controls and views pick up token changes via their existing brush bindings; no architectural refactor of styles is needed.
- "Premium" naming in source comments and resource group labels is in scope for cleanup, since it reinforces the demo register at the code-review level. No new naming convention is introduced beyond removing the marketing register.
- No new NuGet packages are required.
- Onboarding, preferences persistence, and the existing theme-toggle flow are out of scope and remain functionally unchanged.
- Typography, spacing, layout, and component structure are out of scope.
- The four semantic status meanings (pass / fail / skip / info) and their roles in the UI are unchanged; only their saturation/luminance is tuned.

## Dependencies

- Existing `IPreferencesService` / `PreferencesService` — read-only dependency for honoring theme preference.
- Existing WPF-UI brush override mechanism (the named brush keys overridden in `Colors.Dark.xaml` / `Colors.Light.xaml`).

## Out of Scope

- Typography or font changes.
- Spacing, layout, or grid changes.
- New themes beyond the existing light/dark pair (e.g., a separate high-contrast variant, custom user themes).
- Detection of or deference to Windows High-Contrast accessibility mode (the app forces its own palette in all modes — see FR-013).
- New components or views.
- Branding changes (app name, logo, splash screen content).
- Backwards-compatibility shims for the prior palette (no parallel "legacy theme" toggle).
