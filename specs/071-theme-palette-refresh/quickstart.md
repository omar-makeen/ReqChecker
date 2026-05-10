# Quickstart: Verifying the Theme Palette Refresh

This is the verification walkthrough that proves the feature meets its acceptance criteria. Use it after each slice and once more at the end. Roughly 15 minutes for a full pass.

## Prerequisites

- Repo built locally (`dotnet build src/ReqChecker.App/ReqChecker.App.csproj`).
- App runs (`dotnet run --project src/ReqChecker.App`).
- A WCAG contrast checker (e.g., webaim.org/resources/contrastchecker, or any browser DevTools color picker showing contrast).
- A color-blindness simulator (Color Oracle, free desktop tool — Windows/Mac/Linux).

## Slice 1 verification — Dark register (P1)

1. **Build and launch in dark mode.** If currently set to light, switch via **Settings → Theme → Dark**.
2. **Sweep every primary view** in this order: Profile Selector → Test List → Test Config → Run Progress → Results → History → Diagnostics → Schedules → Settings. Open each dialog: Credential Prompt (trigger via an mTLS test), Create Schedule, Missed Runs.
3. **Visual check** for each view:
   - The page background is a neutral dark gray/slate (no purple/violet cast). Hold a known-neutral reference (any GitHub dark theme tab) up to confirm the hue is comparable, not bluer-purpler.
   - View headers, dialog headers, primary buttons, focus rings: all use a single flat cobalt — no gradient, no two-color stops.
   - Cards and panels: any depth/elevation reads as a neutral shadow, not a cyan or indigo halo.
   - Status badges (in Run Progress / Results) keep their semantic hue (green pass / red fail / amber skip / blue info) but feel calmer than before.
4. **Run the verification commands** from `contracts/theme-token-contract.md`:
   ```powershell
   Grep "AccentGradient|AccentSecondary|ElevationGlow" src/  # MUST return 0 matches
   Grep "#0f0f1a|#1a1a2e|#252542|#2f2f52|#00d9ff|#6366f1" src/ | grep -v "specs/"  # MUST return 0
   Grep -i "premium" src/  # MUST return 0
   ```
5. **WCAG contrast spot-check** (3 minimum):
   - Page header text on base background → expect ≥ 14:1.
   - Body copy on a card → expect ≥ 4.5:1.
   - "Run Tests" button label on accent → expect ≥ 4.5:1.
6. **Pass criterion**: every visual check passes, all three Grep commands return 0, and the contrast checker reports ≥ 4.5:1 on every text pairing.

## Slice 2 verification — Light theme hierarchy (P2)

1. **Switch to light mode.** Settings → Theme → Light. Verify no app restart needed and that all currently-open views update.
2. **Open a list-on-cards view (Profile Selector or Test List)** and stand 1 metre back from the screen.
   - Cards must remain individually distinguishable from the page background. If a card "blends into the page," Slice 2 is incomplete.
3. **Open Settings.** Each grouped section should sit visibly on the page like a card, not a flat region.
4. **Open a dialog** (e.g., Credential Prompt). The dialog must read as the highest tier — visibly above any card behind it, via deeper shadow and a slightly cooler-white surface.
5. **Hover over a card.** The hover shadow grows; the resting shadow is subtle but real. Neither feels like a colored halo.
6. **WCAG contrast spot-check** (same 3 pairings as slice 1, in light mode).
7. **Pass criterion**: 100% of cards distinct from page background in 1-second scan; three tiers (page / card / dialog) clearly distinct; all WCAG AA.

## Slice 3 verification — Status calmness + collision (P3)

1. **In dark or light mode**, navigate to **Results** with a recent run that has mixed pass/fail/skip rows.
2. **Time test**: locate a specific failed test (have someone tell you the test name, then start a stopwatch). Should take < 2 seconds in a 40-row table.
3. **Side-by-side check**: place a "Run Tests" primary button next to a "Status: Info" badge.
   - The button should clearly read as "actionable" — louder than the badge.
   - The colors should be in different hue families (cobalt vs sky), not just different shades of the same blue.
4. **Color-blind simulation** (Color Oracle):
   - Open the Results view.
   - Trigger Color Oracle's "Deuteranopia" filter.
   - Verify: pass/fail/skip/info badges remain distinguishable from each other and from the primary button accent. None of the five colors should collapse into another.
   - Repeat with "Protanopia" filter.
   - Tritanopia is **not** required (per spec clarification Q2).
5. **Pass criterion**: failed-test scan time < 2 sec; primary action and StatusInfo unambiguously distinct; deuteranopia + protanopia simulations pass without any pair collapsing.

## End-to-end final pass

After all three slices are merged:

1. Toggle theme light↔dark several times. Every view updates without restart, no broken brushes, no warnings in the WPF debug output.
2. Close and relaunch the app. The previously-selected theme is honored from `preferences.json` (no re-onboarding).
3. Run all four `Grep` verification commands from the contract — all return 0 matches.
4. Take screenshots of each primary view (dark + light). Compare against the prior build.
   - The new screenshots should read as "professional infrastructure tooling" (Grafana / Datadog / Server Manager category) rather than "AI demo / marketing site."
   - This is the SC-001 acceptance check.

## Rollback

This feature is a single-PR change. To revert:

```powershell
git revert <merge-commit-sha>
```

There is no migration step, no persisted state to restore, and no parallel "legacy palette" toggle (out of scope per spec).
