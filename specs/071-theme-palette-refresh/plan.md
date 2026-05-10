# Implementation Plan: Theme Palette Refresh

**Branch**: `071-theme-palette-refresh` | **Date**: 2026-05-10 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/071-theme-palette-refresh/spec.md`

## Summary

Replace ReqChecker's "AI-demo" cyan/indigo gradient + violet-navy palette with a credible IT-tooling palette: **neutral slate dark theme + cobalt single accent**, **light theme with three real elevation tiers**, **flat accents** (no gradients) and **neutral shadows** (no colored glows). Keep the existing token structure and brush-key contract; change only token *values* and the gradient/glow definitions, then sweep every XAML reference that hardcodes the old palette or relies on deleted gradient tokens.

Approach: pick concrete hex values that satisfy WCAG AA contrast and pass deuteranopia/protanopia simulation, then make the change in three independently shippable slices that map to the spec's three user stories (P1 register / P2 light hierarchy / P3 status calmness). The change is scoped to the WPF `ReqChecker.App` project — no service, persistence, or domain code is touched.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (`net8.0-windows` TFM)
**Primary Dependencies**: WPF-UI 4.2.0 (existing), CommunityToolkit.Mvvm 8.4.0 (existing), Microsoft.Extensions.DependencyInjection 10.0.2 (existing). **No new packages.**
**Storage**: N/A — UI-only change. `IPreferencesService` reads existing `preferences.json` for theme choice (read-only dependency).
**Testing**: Manual visual verification per `quickstart.md`. Automated contrast verification optional (off-the-shelf WCAG calculator). No new unit/integration tests; existing tests must continue to pass (build + run).
**Target Platform**: Windows 10 / Windows 11 desktop (existing target).
**Project Type**: Single WPF desktop application (`src/ReqChecker.App`).
**Performance Goals**: No regression. Theme switch latency ≤ existing baseline (token swap is `DynamicResource` re-resolution, identical cost).
**Constraints**:
- WCAG AA contrast (4.5:1 normal text, 3:1 large/UI) for every text-on-background and icon-on-background pairing in both themes (FR-005).
- Pass deuteranopia + protanopia simulation for status colors and accent (SC-009, FR-006).
- Zero gradient brushes referenced in primary accent surfaces; `AccentGradient` / `AccentGradientHorizontal` token definitions removed (FR-014, SC-004).
- No detection of Windows High-Contrast mode — palette is forced (FR-013).
- Existing user theme preference (light/dark) honored without migration (FR-009, SC-008).
**Scale/Scope**: 2 theme files, 1 control style file (~1100 LOC), 11 view XAMLs, 6 reusable controls, 1 animations file. **22 source files** identified by inventory grep (see [research.md](./research.md) §Inventory).

## Constitution Check

The repository's `.specify/memory/constitution.md` is the unfilled scaffold (placeholder principles only). No project-specific governance gates apply. The change is consistent with the implicit norms visible across prior `ReqChecker` features (existing tokens-as-resource-dictionary pattern, no new packages without justification, UI-only changes contained to `ReqChecker.App`).

**Gate result**: PASS (no violations).

## Project Structure

### Documentation (this feature)

```text
specs/071-theme-palette-refresh/
├── plan.md              # This file
├── research.md          # Phase 0 — inventory, palette values, shadow values, contrast/CB verification
├── data-model.md        # Phase 1 — palette token catalog (the "data model" here is the token set)
├── contracts/
│   └── theme-token-contract.md   # The brush keys consumers depend on; what's added/changed/removed
├── quickstart.md        # Phase 1 — verification walkthrough (build, navigate, audit)
└── checklists/
    └── requirements.md  # From /speckit.specify + /speckit.clarify
```

### Source Code (repository root)

This is a single WPF desktop application. The plan touches a small, well-scoped subset of `ReqChecker.App`:

```text
src/ReqChecker.App/
├── Resources/Styles/
│   ├── Colors.Dark.xaml          # REWRITE: new dark palette tokens, drop gradients/glows
│   ├── Colors.Light.xaml         # REWRITE: new light palette tokens with 3 surface tiers
│   ├── Controls.xaml             # EDIT: replace AccentGradient brushes; replace ElevationGlow shadows; rename AccentCheckBox comment
│   └── Animations.xaml           # AUDIT: keep storyboards but verify they still target valid effects after glow → shadow swap
├── Controls/
│   ├── ProgressRing.xaml         # EDIT: replace local ProgressGradient (cyan/indigo hex) with flat accent brush
│   ├── ProgressRing.xaml.cs      # EDIT: drop "premium" comment
│   ├── SummaryCard.xaml          # EDIT: replace fallback #00d9ff color with new accent token; cyan glow → neutral shadow
│   ├── TestStatusBadge.xaml      # EDIT: glow effect parameters tuned for new (calmer) status colors
│   ├── TestStatusBadge.xaml.cs   # AUDIT: glow animation params still correct
│   ├── DonutChart.xaml.cs        # AUDIT: DropShadowEffect params still appropriate
│   └── ExpanderCard.xaml         # EDIT: drop AccentGradient on indicator
├── Views/
│   ├── ProfileSelectorView.xaml  # EDIT: AccentGradientHorizontal header → flat accent; AccentPrimaryColor glow → shadow
│   ├── TestListView.xaml         # EDIT: header gradient → flat accent
│   ├── TestConfigView.xaml       # EDIT: header gradient → flat accent
│   ├── RunProgressView.xaml      # EDIT: header gradient + hardcoded #00d9ff drop shadow
│   ├── ResultsView.xaml          # EDIT: header gradient + ElevationGlowHover shadow
│   ├── HistoryView.xaml          # EDIT: header gradient → flat accent
│   ├── DiagnosticsView.xaml      # EDIT: header gradient → flat accent
│   ├── SettingsView.xaml         # EDIT: header gradient + 4× ElevationGlow shadows
│   ├── SchedulesView.xaml        # EDIT: header gradient → flat accent
│   ├── CredentialPromptDialog.xaml  # EDIT: dialog header gradient → flat accent
│   └── CreateScheduleDialog.xaml    # EDIT: dialog header gradient → flat accent
└── ...                           # (no other files touched)

CLAUDE.md                         # UPDATE via update-agent-context.ps1 (record feature 071)
```

**Structure Decision**: Single WPF desktop project. The change is contained entirely within `src/ReqChecker.App/Resources/Styles/` and `src/ReqChecker.App/Views/` + `Controls/`. No new directories, no new files. The plan splits the work along the spec's user-story boundaries so each slice (US1 dark register / US2 light hierarchy / US3 status tuning) is independently testable.

## Phasing & Slicing

The spec has 3 prioritized user stories. The plan implements them in that order with **independent commits**, each shippable on its own:

### Slice 1 (P1) — Dark register: slate base + cobalt accent + drop gradients/glows

- Rewrite `Colors.Dark.xaml` token values (background tiers, borders, accent, status, focus, elevation).
- Delete `AccentGradient` / `AccentGradientHorizontal` definitions in **both** theme files.
- Replace every `AccentGradient`/`AccentGradientHorizontal` reference across views/controls/Controls.xaml with a flat accent brush (`AccentPrimary`).
- Replace every cyan-glow `DropShadowEffect` reference with a neutral-shadow `DropShadowEffect` in dark mode (uses new `ElevationShadowDark` color).
- Remove hardcoded `#00d9ff` / `#6366f1` in `ProgressRing.xaml`, `SummaryCard.xaml`, `RunProgressView.xaml`.
- Remove "Premium" wording from comments and the one `AccentCheckBox` comment.

**Acceptance** (matches US1 acceptance scenarios in spec): every primary view in dark mode shows neutral slate background, single cobalt accent, no gradients, no cyan glows. WCAG AA verified. Build + run; navigate every view; audit by grep that all 4 problem hex values and `AccentGradient` keys return zero matches.

### Slice 2 (P2) — Light theme hierarchy

- Rewrite `Colors.Light.xaml` token values so `BackgroundBase`, `BackgroundSurface`, and `BackgroundElevated` are three distinct values.
- Adjust `BorderSubtle` / `BorderDefault` for the new background tiers.
- Adjust the light-theme accent (cobalt with slightly higher contrast on white) to match its dark counterpart in role.
- Verify shadow opacity in light mode reads as elevation (not as muddy halo) on the new whiter surfaces.

**Acceptance** (matches US2): in light mode, every card and panel surface is visually distinguishable from the page background in a 1-second scan; modals/dialogs sit at a third tier above panels.

### Slice 3 (P3) — Status calmness + collision fix

- Tune the four status colors in **both** theme files to be slightly less saturated.
- Move `StatusInfo` to a non-cobalt blue (sky/teal) so it cannot collide with the new primary accent.
- Update `StatusPassGlow` / `StatusFailGlow` / `StatusSkipGlow` color values in step with the new status colors (still used by `TestStatusBadge`).
- Run color-blind simulation pass (deuteranopia, protanopia) on the final 5-color set (4 status + accent); adjust if any pair collapses.

**Acceptance** (matches US3 + SC-009): scanning a 40-row mixed-status results table locates a specific failed row in under 2 seconds; primary accent and StatusInfo are independently identifiable; deuteranopia/protanopia simulation passes.

Each slice is one commit. The PR can be opened after Slice 1 if needed and the remaining slices added; preferred is one PR with three commits.

## Complexity Tracking

No constitution violations to justify. Notable design decisions worth flagging for review:

| Decision | Why | Simpler Alternative Rejected Because |
|----------|-----|-------------------------------------|
| Delete `AccentGradient` token outright (vs. redefine as solid) | FR-014; clarification Q3 — make demo-register impossible to reintroduce by accident | "Redefine as solid" leaves a token whose name lies, perpetuating the marketing register at code-review level |
| Force palette regardless of Windows HC mode | FR-013; clarification Q1 — HC users use OS tools | "Detect HC and defer" doubles the test surface for an audience the app doesn't serve |
| 3-slice commit structure (matches user-story priorities) | Each slice ships value alone; reduces review surface | Single mega-commit makes review noisier and rollback granularity worse |
| Keep `DropShadowEffect` (just change colors/opacity) for elevation rather than introducing a new elevation primitive | Existing storyboards in `Animations.xaml` already target `DropShadowEffect.BlurRadius` / `.Opacity`; changing the primitive would require rewriting those storyboards | Out of scope; orthogonal cleanup |

---

**Phase 0 output**: see [research.md](./research.md)
**Phase 1 outputs**: see [data-model.md](./data-model.md), [contracts/theme-token-contract.md](./contracts/theme-token-contract.md), [quickstart.md](./quickstart.md)
