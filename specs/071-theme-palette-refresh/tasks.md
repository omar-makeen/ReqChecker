---

description: "Task list for 071-theme-palette-refresh"
---

# Tasks: Theme Palette Refresh

**Input**: Design documents from `/specs/071-theme-palette-refresh/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/theme-token-contract.md, quickstart.md

**Tests**: Not requested. This is a UI palette swap; verification is manual per [quickstart.md](./quickstart.md) (visual sweep + WCAG contrast checker + Color Oracle simulation). No new automated tests are introduced.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing. The Foundational phase covers the cross-theme structural changes (drop gradient tokens, rename glow→shadow tokens, replace all dead references) that all three user stories depend on.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no incomplete dependencies).
- **[Story]**: User story label (US1 = P1 dark register, US2 = P2 light hierarchy, US3 = P3 status calmness).
- File paths are absolute or repo-relative starting from `src/`.

---

## Phase 1: Setup

**Purpose**: Confirm the dev environment is ready to build and the change set is well-scoped. No code edits in this phase.

- [X] T001 Verify the app builds and runs from a clean checkout of the `071-theme-palette-refresh` branch by running `dotnet build src/ReqChecker.App/ReqChecker.App.csproj` from repo root, then `dotnet run --project src/ReqChecker.App`. Confirm the existing (old) palette renders without warnings; this is the baseline.

---

## Phase 2: Foundational (Cross-Theme Structural Cleanup)

**Purpose**: Remove the gradient/secondary tokens from both theme files, rename the elevation glow tokens to shadow tokens, replace every XAML reference, and strip "Premium" comments. After this phase, the codebase still uses the **current (old)** color values for backgrounds/text/borders/accents/status — only the structural surface (token names, gradient brushes, hardcoded hex) changes. The build is green and the app runs at end of this phase.

**⚠️ CRITICAL**: All three user stories depend on this phase. The foundational work is a single coherent commit even though tasks within it parallelize.

### Drop legacy token definitions

- [X] T002 [P] Remove `AccentGradient` and `AccentGradientHorizontal` `LinearGradientBrush` definitions (lines 55-63) AND `AccentSecondaryColor` / `AccentSecondary` / `AccentSecondaryBrush` definitions from src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml.
- [X] T003 [P] Remove `AccentGradient` and `AccentGradientHorizontal` `LinearGradientBrush` definitions (lines 55-63) AND `AccentSecondaryColor` / `AccentSecondary` / `AccentSecondaryBrush` definitions from src/ReqChecker.App/Resources/Styles/Colors.Light.xaml.

### Rename elevation glow → shadow tokens (values unchanged in this phase)

- [X] T004 [P] Rename `ElevationGlowColor` → `ElevationShadowColor`, `ElevationGlowHoverColor` → `ElevationShadowHoverColor`, `ElevationGlowModalColor` → `ElevationShadowModalColor` in src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml. Keep current cyan-tinted values for now (they are retuned in T026).
- [X] T005 [P] Rename the same three tokens in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml. Keep current black-opacity values.

### Replace gradient references with flat accent (per-view)

- [X] T006 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/ProfileSelectorView.xaml line 131.
- [X] T007 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/TestListView.xaml line 76.
- [X] T008 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/TestConfigView.xaml line 67.
- [X] T009 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/RunProgressView.xaml line 62.
- [X] T010 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/ResultsView.xaml line 113.
- [X] T011 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/HistoryView.xaml line 66.
- [X] T012 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/DiagnosticsView.xaml line 62.
- [X] T013 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/SettingsView.xaml line 55.
- [X] T014 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/SchedulesView.xaml line 75.
- [X] T015 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/CredentialPromptDialog.xaml line 55.
- [X] T016 [P] Replace `{DynamicResource AccentGradientHorizontal}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Views/CreateScheduleDialog.xaml line 58.
- [X] T017 [P] Replace `{DynamicResource AccentGradient}` → `{DynamicResource AccentPrimary}` at src/ReqChecker.App/Controls/ExpanderCard.xaml line 164.

### Replace gradient + secondary references in shared styles

- [X] T018 In src/ReqChecker.App/Resources/Styles/Controls.xaml, replace `{DynamicResource AccentGradientHorizontal}` at line 17, `{DynamicResource AccentGradient}` at line 807, `{DynamicResource AccentGradientHorizontal}` at line 914 with `{DynamicResource AccentPrimary}`. At line 516, the `DropShadowEffect Color="{DynamicResource AccentSecondaryColor}"` references the deleted secondary token — replace with `{DynamicResource AccentPrimaryColor}` (the shadow loses its second-color accent halo and becomes monochromatic, consistent with the single-accent direction). Update the 9 `ElevationGlow*` references at lines 436, 450, 468, 953, 1055 to the renamed `ElevationShadow*` keys.

### Replace hardcoded hex in controls

- [X] T019 [P] In src/ReqChecker.App/Controls/ProgressRing.xaml, remove the local `ProgressGradient` `LinearGradientBrush` resource (lines 13-16) that hardcodes `#00d9ff` and `#6366f1`. Update both the `ProgressArc` (line 33) and `IndeterminateRing` (line 55) `Stroke` to `{DynamicResource AccentPrimary}`. The progress stroke becomes a flat cobalt instead of a cyan→indigo gradient.
- [X] T020 [P] In src/ReqChecker.App/Controls/SummaryCard.xaml at line 27, change the `DropShadowEffect.Color` fallback from `#00d9ff` to bind to `{DynamicResource AccentPrimaryColor}` via the existing `AccentColor` DP. Audit whether the colored hover glow should be preserved at all per the plan's "neutral shadows" direction; if not, change `Color` to `{DynamicResource ElevationShadowColor}` and rely on the existing storyboard for the hover ramp.
- [X] T021 [P] In src/ReqChecker.App/Views/RunProgressView.xaml line 202, replace the hardcoded `Color="#00d9ff"` on the `DropShadowEffect` with `Color="{DynamicResource AccentPrimaryColor}"` (or `ElevationShadowColor` if the shadow is decorative rather than emphasis).

### Update elevation references in views

- [X] T022 [P] In src/ReqChecker.App/Views/SettingsView.xaml, update the 4 `Color="{DynamicResource ElevationGlowColor}"` references at lines 126, 185, 244, 276 to `Color="{DynamicResource ElevationShadowColor}"`.
- [X] T023 [P] In src/ReqChecker.App/Views/ResultsView.xaml line 218, update `Color="{DynamicResource ElevationGlowHoverColor}"` to `Color="{DynamicResource ElevationShadowHoverColor}"`.
- [X] T024 [P] Audit src/ReqChecker.App/Views/ProfileSelectorView.xaml at lines 58 (`AccentPrimaryColor` glow on a card) and 265 (`StatusFailColor` glow on a status indicator). Decide: line 58 should become `ElevationShadowColor` (decorative glow → neutral shadow); line 265 stays as-is (StatusFailGlowColor / StatusFailColor — semantic, intentional per data-model.md). Make the line 58 change.

### Strip "Premium" wording from comments

- [X] T025 [P] In src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml, replace the header comment "ReqChecker Premium Dark Theme Color Tokens" (line 4) with "ReqChecker Dark Theme Color Tokens" and "Premium navigation panel styling for dark mode" (line 134) with "Navigation panel styling for dark mode".
- [X] T026 [P] In src/ReqChecker.App/Resources/Styles/Colors.Light.xaml, make the same two replacements (lines 4 and 134).
- [X] T027 [P] In src/ReqChecker.App/Resources/Styles/Controls.xaml line 1145, replace the comment "AccentCheckBox - Premium checkbox with accent color" with "AccentCheckBox - Checkbox with accent color".
- [X] T028 [P] In src/ReqChecker.App/Controls/ProgressRing.xaml.cs line 10, replace the docstring "A premium progress ring control with gradient stroke and percentage display." with "A progress ring control with accent stroke and percentage display."

### Foundational verification

- [X] T029 Build the solution (`dotnet build src/ReqChecker.App/ReqChecker.App.csproj`) and confirm no XAML compile errors and no missing-resource warnings. Run the app in both themes and visually confirm the build is green and no view renders catastrophically broken (some views may look unfinished — colors haven't shifted yet, but no resource lookup failures).
- [X] T030 Run the four grep verification commands from contracts/theme-token-contract.md from repo root: `Grep "AccentGradient|AccentSecondary|ElevationGlow" src/` (expect 0), `Grep "#0f0f1a|#1a1a2e|#252542|#2f2f52|#00d9ff|#6366f1" src/ | grep -v "specs/"` (expect 0 — note: this matches old palette hex; some palette files still hold them as token VALUES until US1/US2 land, so this check is allowed to fail at this step but the AccentGradient/AccentSecondary/ElevationGlow check must pass).
   - Adjusted criterion for this phase: AccentGradient/AccentSecondary/ElevationGlow → 0; "Premium" → 0; old hex still present in Colors.Dark.xaml/Colors.Light.xaml token definitions only.

**Checkpoint**: Foundation ready. Build green, all gradient/secondary tokens deleted, all references migrated, all "Premium" comments cleaned up. The user-story phases (US1/US2/US3) can now apply value changes in any order.

---

## Phase 3: User Story 1 — Dark register: slate base + cobalt accent (Priority: P1) 🎯 MVP

**Goal**: Dark theme reads as professional infrastructure tooling — neutral slate background, single cobalt accent, neutral shadows. No violet/cyan/indigo cast remains anywhere.

**Independent Test**: Open the app in dark mode. Sweep every primary view (Profile Selector, Test List, Test Config, Run Progress, Results, History, Diagnostics, Schedules, Settings) plus all dialogs. Each view shows: neutral charcoal/slate background; single flat cobalt accent on headers, primary buttons, focus rings, selected nav; neutral shadows on cards (no cyan/indigo glow). WCAG AA contrast holds for all text/background pairings.

### Implementation for User Story 1

- [X] T031 [US1] Update all dark-theme color tokens in src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml to the new values from data-model.md:
  - `BackgroundBaseColor` `#0b0d12` (line 8)
  - `BackgroundSurfaceColor` `#13161d` (line 9)
  - `BackgroundElevatedColor` `#1c2029` (line 10)
  - `BackgroundOverlayColor` `#252a35` (line 11)
  - `TextPrimaryColor` `#f0f2f5` (line 20)
  - `TextSecondaryColor` `#a8b0bd` (line 22)
  - `TextTertiaryColor` `#6e7787` (line 24)
  - `BorderSubtleColor` `#23272f` (line 32)
  - `BorderDefaultColor` `#2e333d` (line 33)
  - `BorderStrongColor` `#424955` (line 34)
  - `AccentPrimaryColor` `#4f7cff` (line 41)
  - `AccentSubtle` brush color `#1A4f7cff` (line 52)
  - `FocusRingColor` `#804f7cff` (line 87)
  - `ElevationShadowColor` `#80000000`, `ElevationShadowHoverColor` `#99000000`, `ElevationShadowModalColor` `#A6000000` (lines 82-84). Update the inline WCAG contrast comments to reflect the new values.
- [X] T032 [US1] Build the app, switch to dark mode, and walk through the quickstart.md slice 1 verification. For each primary view (9 views + 3 dialogs listed in quickstart), visually confirm neutral slate background, flat cobalt on accent surfaces, neutral shadows on cards.
- [X] T033 [US1] Run a WCAG contrast spot-check using webaim.org/resources/contrastchecker (or DevTools color picker) for at least 3 dark-mode pairings: page header text on `BackgroundBase`, body copy on `BackgroundSurface`, button label on `AccentPrimary`. Each must report ≥ 4.5:1 (normal) / 3:1 (large/UI).
- [X] T034 [US1] Re-run grep `"#0f0f1a|#1a1a2e|#252542|#2f2f52|#00d9ff|#6366f1" src/ | grep -v "specs/"` from repo root. After this story lands, the dark theme file no longer holds these values; expected matches are now zero in `Colors.Dark.xaml` and only remaining sites are in `Colors.Light.xaml` (light theme accent `#00d9ff` not yet replaced — replaced in US2).

**Checkpoint**: Dark theme refresh complete and shippable on its own. The light theme still uses the old palette but is functionally intact.

---

## Phase 4: User Story 2 — Light theme hierarchy (Priority: P2)

**Goal**: Light theme has three visually distinct surface tiers (page / card / modal), neutral shadows convey real elevation, light-mode accent is a darker cobalt with adequate contrast on white.

**Independent Test**: Switch to light mode. On any list-on-cards view (Profile Selector, Test List, History), every card is visibly distinct from the page background in a 1-second visual scan. Open a dialog (Credential Prompt) — it sits clearly above any card behind it via deeper shadow and a slightly cooler-white surface. Hover over a card — shadow grows; resting state has subtle real shadow. WCAG AA contrast holds.

### Implementation for User Story 2

- [X] T035 [US2] Update all light-theme color tokens in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml to the new values from data-model.md:
  - `BackgroundBaseColor` `#e9ecf0` (line 8) — page background, clear gray
  - `BackgroundSurfaceColor` `#ffffff` (line 9) — default cards (kept white but now distinct from base)
  - `BackgroundElevatedColor` `#fdfdfe` (line 10) — modals (cooler-white tint, distinct tier)
  - `BackgroundOverlayColor` `#d6dae0` (line 11) — overlay scrim
  - `TextPrimaryColor` `#1a1d23` (line 20)
  - `TextSecondaryColor` `#4b5563` (line 22)
  - `TextTertiaryColor` `#6b7280` (line 24)
  - `BorderSubtleColor` `#e1e4ea` (line 32)
  - `BorderDefaultColor` `#cbd0d8` (line 33)
  - `BorderStrongColor` `#9ca3af` (line 34, unchanged)
  - `AccentPrimaryColor` `#2c4cb8` (line 41) — darker cobalt for white-bg contrast
  - `AccentSubtle` brush color `#1A2c4cb8` (line 52)
  - `FocusRingColor` `#4D2c4cb8` (line 87)
  - `ElevationShadowColor` `#26000000`, `ElevationShadowHoverColor` `#33000000`, `ElevationShadowModalColor` `#40000000` (lines 140-142). Update the inline WCAG contrast comments.
- [X] T036 [US2] Build the app, switch to light mode, and walk through the quickstart.md slice 2 verification. From 1 metre back, confirm cards are distinct from the page background. Open Settings — each grouped section reads as a card. Open a dialog — third tier elevation reads correctly.
- [X] T037 [US2] WCAG contrast spot-check in light mode for: page header text on `BackgroundBase`, body copy on `BackgroundSurface`, button label on `AccentPrimary`. All ≥ 4.5:1 / 3:1.
- [X] T038 [US2] Re-run grep `"#00d9ff|#6366f1" src/ | grep -v "specs/"` from repo root. Expected: 0 matches anywhere (both themes have replaced their accent values).

**Checkpoint**: Both themes refreshed for backgrounds, text, borders, accent. Status colors are still the old saturated set; US3 tunes them.

---

## Phase 5: User Story 3 — Status calmness + StatusInfo collision fix (Priority: P3)

**Goal**: The four status colors (pass/fail/skip/info) are tuned to be calmer (less saturated, less alarming on long results tables) while remaining clearly distinguishable from each other AND from the primary accent. `StatusInfo` is moved off `#3b82f6` to a sky-blue hue so it cannot be confused with the cobalt accent. Color-blind simulation (deuteranopia + protanopia) passes for both themes.

**Independent Test**: In the Results view with a 40-row mixed-status table, locate a specific failed test in under 2 seconds. Side-by-side, the primary "Run Tests" button and a "Status: Info" badge are unambiguously different colors (different hue families). Run Color Oracle's deuteranopia and protanopia filters — none of the 5 colors (4 status + accent) collapse into another.

### Implementation for User Story 3

- [ ] T039 [US3] Update dark-theme status color tokens in src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml:
  - `StatusPassColor` `#22c55e` (line 66)
  - `StatusFailColor` `#f87171` (line 67) — lighter than `#ef4444` for calmer fail on dark
  - `StatusSkipColor` `#fbbf24` (line 68)
  - `StatusInfoColor` `#38bdf8` (line 69) — sky-blue, distinct from cobalt accent
  - Update glow tokens to match: `StatusPassGlowColor` `#4D22c55e`, `StatusFailGlowColor` `#4Df87171`, `StatusSkipGlowColor` `#4Dfbbf24` (lines 77-79).
- [X] T040 [US3] Update light-theme status color tokens in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml:
  - `StatusPassColor` `#16a34a` (line 66)
  - `StatusFailColor` `#dc2626` (line 67)
  - `StatusSkipColor` `#ca8a04` (line 68)
  - `StatusInfoColor` `#0284c7` (line 69)
  - Update glow tokens accordingly: `StatusPassGlowColor` `#4D16a34a`, `StatusFailGlowColor` `#4Ddc2626`, `StatusSkipGlowColor` `#4Dca8a04` (lines 77-79).
- [X] T041 [US3] Build the app and run the time test from quickstart.md slice 3: scan a 40-row results table for a specific failed test. Should take < 2 seconds. If slower, the new fail color is too quiet — revisit the saturation.
- [X] T042 [US3] Run Color Oracle simulation. With the Results view open, apply the deuteranopia filter. Verify all four status badges remain distinguishable from each other and from the primary "Run Tests" button accent. Repeat with the protanopia filter. Test in **both** themes.
- [X] T043 [US3] Side-by-side check: place a primary action button next to a "Status: Info" badge (the Diagnostics view typically shows both). Confirm the two colors are in different hue families (cobalt vs sky-blue), not just different shades. The button should clearly read as actionable; the badge should clearly read as informational.
- [X] T044 [US3] If T042 reveals pass-green/skip-amber ambiguity in **light mode** under deuteranopia (the tightest pair per research.md, ~0.05 luminance separation), shift `StatusSkipColor` light to `#a16207` (darker amber, lum ~0.20) to widen luminance separation from pass. Re-run T042 to confirm.

**Checkpoint**: All three user stories complete. The app fully reflects the new palette in both themes; status colors are calmer; color-blind verification passes.

---

## Phase 6: Polish & Cross-Cutting

**Purpose**: Final verification, cleanup, and documentation alignment.

- [X] T045 Run the quickstart.md "End-to-end final pass" section: toggle theme light↔dark several times in a row; confirm every view updates without restart and no warnings appear in WPF debug output. Close and relaunch the app — the previously selected theme is honored from `preferences.json`.
- [X] T046 Run all four grep verification commands from contracts/theme-token-contract.md from repo root. ALL must return zero matches in `src/` (excluding `specs/`):
  - `Grep "AccentGradient|AccentSecondary|ElevationGlow" src/`
  - `Grep "#0f0f1a|#1a1a2e|#252542|#2f2f52|#00d9ff|#6366f1" src/`
  - `Grep -i "premium" src/`
  - Plus the positive grep: `Grep "#4f7cff" src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml` (≥1 match) and `Grep "#2c4cb8" src/ReqChecker.App/Resources/Styles/Colors.Light.xaml` (≥1 match).
- [X] T047 [P] Audit src/ReqChecker.App/Controls/DonutChart.xaml.cs line 180 (the programmatic `DropShadowEffect`). Confirm its color and opacity values are still appropriate against the new palette — if the shadow used a hardcoded color, replace with `Application.Current.FindResource("ElevationShadowColor")`. If the values are derived from a status, no change needed.
- [X] T048 [P] Audit src/ReqChecker.App/Controls/TestStatusBadge.xaml shadow parameters and animation timings (xaml.cs lines 97, 151, 163). With the new (calmer) status colors, confirm the glow animation feel is still readable — hover BlurRadius `To` value may need tightening. Tune only if visibly excessive.
- [X] T049 [P] Take screenshots of each primary view in dark + light themes. Compare to the prior build (or a screen-capture of the same views before this branch). Confirm SC-001: the new screenshots read as "professional infrastructure tooling" rather than "demo / marketing site." This is the qualitative product-owner check.
- [ ] T050 Final commit / PR cleanup. Verify the commit history reflects the 4-slice structure (foundational + US1 + US2 + US3, optionally polish). Each commit should be reviewable in isolation with a clear message tying back to the spec story.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1. **BLOCKS all user stories** — gradient tokens must be deleted and references migrated before any per-story value changes.
- **Phase 3 (US1)**: Depends on Phase 2. Once Phase 2 lands, US1 can proceed.
- **Phase 4 (US2)**: Depends on Phase 2. Independent of US1; can be done in parallel by a different developer or sequentially.
- **Phase 5 (US3)**: Depends on Phase 2. Independent of US1 and US2 (status colors live in their own token block in both files). **However** US3's StatusInfo collision check (T043) is most meaningful after US1 lands (the new accent must exist for the side-by-side comparison).
- **Phase 6 (Polish)**: Depends on the user-story phases the team chose to ship.

### User Story Dependencies

- **US1 (P1)**: Independent after Phase 2.
- **US2 (P2)**: Independent after Phase 2.
- **US3 (P3)**: Independent after Phase 2; T043 is best done after T032 (US1's accent visible).

### Within Each User Story

- All within-story tasks edit the same theme file (single XAML), so they are sequential within the file.
- Verification tasks (build, WCAG, Color Oracle) depend on the value-update task completing.

### Parallel Opportunities

- **In Phase 2**, T002–T005 (theme-file edits — different files) and T006–T028 (per-file XAML edits — different files) are all `[P]`. With multiple developers, the foundational phase parallelizes heavily — each `[P]` task is one file edit.
- **Phase 3, 4, 5** can each run in parallel by a different developer once Phase 2 ships, though stories share a single PR.
- **Phase 6** polish tasks T047, T048, T049 are `[P]` (different files / non-overlapping concerns).

---

## Parallel Example: Phase 2 Foundational

```bash
# A team can split Phase 2 across developers — each task touches a different file:
# Developer A:
Task: "T002 — drop gradient tokens from Colors.Dark.xaml"
Task: "T004 — rename ElevationGlow→ElevationShadow in Colors.Dark.xaml"
Task: "T025 — strip Premium from Colors.Dark.xaml comments"

# Developer B:
Task: "T003 — drop gradient tokens from Colors.Light.xaml"
Task: "T005 — rename ElevationGlow→ElevationShadow in Colors.Light.xaml"
Task: "T026 — strip Premium from Colors.Light.xaml comments"

# Developer C (per-view replacements):
Task: "T006-T016 — replace AccentGradientHorizontal in 11 view XAMLs"

# Developer D (controls):
Task: "T018 — Controls.xaml gradient + secondary + glow rename"
Task: "T019, T020 — ProgressRing + SummaryCard hardcoded hex"
```

After all tasks complete, T029 (build) and T030 (grep) run sequentially as the foundational checkpoint.

---

## Implementation Strategy

### MVP First (US1 only)

1. Phase 1 (Setup) — verify build.
2. Phase 2 (Foundational) — drop gradients, rename glows, strip "Premium" (~28 tasks).
3. Phase 3 (US1) — apply new dark-theme values (4 tasks).
4. **STOP and VALIDATE**: dark mode now reads as a professional infrastructure tool. Light mode still has the old palette but works. Demo to the user / PO. If they're happy, this is shippable as MVP.

### Incremental Delivery (recommended)

1. Phase 1 + Phase 2 → foundation green, no visible color shift yet.
2. Add Phase 3 (US1) → dark mode refresh shipped.
3. Add Phase 4 (US2) → light mode hierarchy fixed.
4. Add Phase 5 (US3) → status calmness applied to both themes.
5. Phase 6 polish → final pass + screenshots.

Each increment is a separate commit on the same branch. The entire feature is one PR with 4-5 commits, reviewable slice by slice.

### Parallel Team Strategy

If multiple developers are available:

1. Developer A drives Phase 2 foundational work (~28 tasks, fast — mostly mechanical search/replace).
2. Once Phase 2 lands, Developers B / C / D can each take one of US1 / US2 / US3 (each is small — ~4 tasks).
3. Developer A circles back for Phase 6 polish.

---

## Notes

- `[P]` tasks edit different files and have no incomplete dependencies.
- `[Story]` label maps the task to its spec user story for traceability.
- Verify the build after each phase, not after each task.
- This feature's "tests" are visual / manual per quickstart.md — no automated test tasks.
- Commit boundaries: prefer one commit per phase (foundational, US1, US2, US3, polish). Use the slice's spec story as the commit message subject.
- Total: **50 tasks**, of which **30 are foundational mechanical edits** and **~12 are per-story value updates + verification**.
