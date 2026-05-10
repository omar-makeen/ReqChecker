---
description: "Task list for 073-design-language-refresh"
---

# Tasks: Design Language Refresh — Reduce "AI-Built" Aesthetic

**Input**: Design documents from `/specs/073-design-language-refresh/`
**Prerequisites**: plan.md, spec.md, research.md, quickstart.md (all present)

**Tests**: No automated tests are required. This is a UI/UX refresh; verification is manual per [quickstart.md](./quickstart.md), consistent with prior UI features (071 palette refresh, 072 auto-scroll).

**Organization**: Tasks are grouped by user story to enable independent implementation and verification. Each story is a coherent slice of the redesign and can be merged independently if needed.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: US1 / US2 / US3 / US4 / US5 / US5a — maps to user stories in spec.md
- Setup, Foundational, and Polish phases carry no `[Story]` label

## Path Conventions

- All paths are relative to the repo root: `C:\workspace\pulsar\ReqChecker\`
- Source root: `src/ReqChecker.App/`
- Views: `src/ReqChecker.App/Views/`
- Styles: `src/ReqChecker.App/Resources/Styles/`

## Pre-flight scope inventory (informational)

A grep pass on `master` HEAD established the actual scope of each pattern. Numbers below are the ground truth for task sizing — they correct the plan.md estimates:

| Pattern | Defined in | Used in (files) | Total occurrences |
|---|---|---|---|
| `AnimatedPageHeader` style | `Controls.xaml` | 9 views | 9 |
| Icon-in-rounded-square 48×48 + colored SymbolIcon (inline pattern) | inline in views | 12 views (all incl. dialogs) | 12 |
| `ParameterGroupCard` style | `Controls.xaml` | `TestConfigView.xaml` only | 3 |
| `PromptAtRunIndicator` style | `Controls.xaml` | `TestConfigView.xaml` only | 1 |
| `AnimatedSection` (inline `<Style>`) | `TestConfigView.xaml` only | `TestConfigView.xaml` | 3 |

Implication: US2 (section chrome) is concentrated in `TestConfigView.xaml`, but each *other* configuration view (`SettingsView`, `DiagnosticsView`, `SchedulesView`, `ResultsView`, `ProfileSelectorView`) likely has its own hand-rolled card/border grouping that doesn't use the shared style — a per-view audit task is included for each.

---

## Phase 1: Setup

**Purpose**: Capture before-state and align everyone on what's about to change.

- [X] T001 Capture pre-refresh visual baseline by running the app on master and screenshotting each view (Light + Dark themes) per [quickstart.md](./quickstart.md) Pass 0. Save into `specs/073-design-language-refresh/baseline/` (do not commit). 24 screenshots total: 12 views × 2 themes.
- [X] T002 Verify the baseline build is clean: run `dotnet build src/ReqChecker.App/ReqChecker.App.csproj -c Debug` and confirm 0 errors and 0 warnings unrelated to existing third-party packages.
- [X] T003 [P] Skim each XAML in [src/ReqChecker.App/Views](src/ReqChecker.App/Views) to confirm no manual additions to `Storyboard`/`EventTrigger` blocks have appeared since the spec was written. Any new decorative motion not catalogued in plan.md gets added to the deletion list before Foundational starts.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Edit the shared style dictionaries so per-view edits later can rely on the new tokens/styles. **Must complete before any user story phase begins** — all stories consume tokens added here.

**⚠️ CRITICAL**: No user story work begins until T010 completes.

- [X] T004 Add semantic spacing tokens to [src/ReqChecker.App/Resources/Styles/Spacing.xaml](src/ReqChecker.App/Resources/Styles/Spacing.xaml): `SpacingLabelField` (`0,0,0,4`), `SpacingFieldField` (`0,0,0,16`), `SpacingSectionSection` (`0,0,0,32`), `SpacingHeaderToBody` (`0,0,0,24`). Place them in a dedicated `<!-- Semantic Spacing Tokens -->` section after the existing primitive tokens, before the `Border Radius Values` block.
- [X] T005 Add a `SectionHeadingStyle` to [src/ReqChecker.App/Resources/Styles/Typography.xaml](src/ReqChecker.App/Resources/Styles/Typography.xaml) based on `H2TextStyle` (20 px SemiBold) with a default `Margin` of `0,0,0,16` (heading-to-first-field). Place after `H3TextStyle`.
- [X] T006 Add a `PageTitleStyle` to [src/ReqChecker.App/Resources/Styles/Typography.xaml](src/ReqChecker.App/Resources/Styles/Typography.xaml) based on `H1TextStyle` (24 px SemiBold) with `VerticalAlignment=Center` and `TextTrimming=CharacterEllipsis`, for use in the new page header. Place after `H1TextStyle`.
- [X] T007 Delete the `AnimatedPageHeader` style (entire `<Style x:Key="AnimatedPageHeader" …>` block, ~28 lines) from [src/ReqChecker.App/Resources/Styles/Controls.xaml](src/ReqChecker.App/Resources/Styles/Controls.xaml) starting at line 1112. The build will break with `StaticResource AnimatedPageHeader not found` until all 9 view references are updated in Phase 3 — that is expected and gated by T010.
- [X] T008 Delete the `PromptAtRunIndicator` style (entire `<Style x:Key="PromptAtRunIndicator" …>` block) from [src/ReqChecker.App/Resources/Styles/Controls.xaml](src/ReqChecker.App/Resources/Styles/Controls.xaml) starting at line 510. Build breaks until `TestConfigView.xaml` line 332 is updated in Phase 8 — accept the break, fix in order.
- [X] T009 Delete the `ParameterGroupCard` style (entire `<Style x:Key="ParameterGroupCard" …>` block, lines ~500-507) from [src/ReqChecker.App/Resources/Styles/Controls.xaml](src/ReqChecker.App/Resources/Styles/Controls.xaml). Build breaks until `TestConfigView.xaml` lines 142, 209, 272 are updated in Phase 4.
- [ ] T010 **Checkpoint**: Confirm the working tree compiles only after Phase 3 begins. Do NOT attempt to build between T007–T009 and the start of Phase 3; the broken-reference state is by design. Commit T004–T009 as a single atomic change titled "refactor(styles): remove decorative chrome styles and add semantic spacing tokens" — but do not push until at least one view (US1) is fixed and the app builds again.

---

## Phase 3: User Story 1 — Headers (Priority: P1) 🎯 MVP

**Goal**: Every page header — across 9 top-level views and 3 dialogs — drops the icon-in-rounded-square, the 4-px accent-line strip, and the restated subtitle. Title-only with flanking nav/action affordances.

**Independent Test**: After Phase 3, every header is a single typographic title with no decoration. Open every view; verify no icon container, no accent line, no restated subtitle. Functional affordances (Back, Save, Run, Export) all still work. App builds and runs.

**MVP scope**: Completing Phase 1 + Phase 2 + Phase 3 alone is a shippable change. Pages still have their existing section chrome and motion (those land in later phases), but the headers — the most universal AI tell — are gone everywhere.

### Top-level views (parallel — different files)

- [X] T011 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/ProfileSelectorView.xaml](src/ReqChecker.App/Views/ProfileSelectorView.xaml) at line ~122: replace the `<Border Style="{StaticResource AnimatedPageHeader}">…</Border>` block with a 3-column `Grid` (`Auto,*,Auto`) containing existing Back button (col 0), `PageTitleStyle` title `TextBlock` (col 1), primary action(s) (col 2). Remove icon container, accent-line strip, and any subtitle that restates the title. Preserve all `Command` bindings, `TabIndex` values, `ToolTip`, and `AutomationProperties`.
- [X] T012 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/TestListView.xaml](src/ReqChecker.App/Views/TestListView.xaml) at line ~67. Same recipe as T011. Preserve search/filter affordances if they sit in the header.
- [X] T013 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/TestConfigView.xaml](src/ReqChecker.App/Views/TestConfigView.xaml) at line ~58. Same recipe. Preserve `BackCommand`, `SaveCommand`, `BackButton` `x:Name`, focus-element binding (`FocusManager.FocusedElement={Binding ElementName=BackButton}`).
- [X] T014 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/RunProgressView.xaml](src/ReqChecker.App/Views/RunProgressView.xaml) at line ~53. Same recipe. **Critical**: do NOT touch the Completed Tests `ScrollViewer` (`x:Name="CompletedTestsScrollViewer"`) or its `ScrollChanged="OnCompletedTestsScrollChanged"` handler — 072 auto-scroll depends on them.
- [X] T015 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/ResultsView.xaml](src/ReqChecker.App/Views/ResultsView.xaml) at line ~104. Same recipe. Preserve export-dropdown affordance if in header.
- [X] T016 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/HistoryView.xaml](src/ReqChecker.App/Views/HistoryView.xaml) at line ~57. Same recipe.
- [X] T017 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/DiagnosticsView.xaml](src/ReqChecker.App/Views/DiagnosticsView.xaml) at line ~53. Same recipe.
- [X] T018 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/SchedulesView.xaml](src/ReqChecker.App/Views/SchedulesView.xaml) at line ~66. Same recipe. Preserve "New schedule" / "Cancel run" affordances.
- [X] T019 [P] [US1] Rewrite the page header in [src/ReqChecker.App/Views/SettingsView.xaml](src/ReqChecker.App/Views/SettingsView.xaml) at line ~46. Same recipe.

### Dialogs (parallel — different files, Q1-decided "header chrome only")

- [X] T020 [P] [US1] Rewrite ONLY the dialog header in [src/ReqChecker.App/Views/CredentialPromptDialog.xaml](src/ReqChecker.App/Views/CredentialPromptDialog.xaml): remove icon-in-rounded-square, accent-line strip, and restated subtitle. Do NOT change the dialog body's internal layout (cards/sections inside the dialog are intentionally preserved per Q1).
- [X] T021 [P] [US1] Rewrite ONLY the dialog header in [src/ReqChecker.App/Views/CreateScheduleDialog.xaml](src/ReqChecker.App/Views/CreateScheduleDialog.xaml). Same scope rule as T020.
- [X] T022 [P] [US1] Rewrite ONLY the dialog header in [src/ReqChecker.App/Views/MissedRunsDialog.xaml](src/ReqChecker.App/Views/MissedRunsDialog.xaml). Same scope rule as T020.

### Verification

- [X] T023 [US1] Build the app: `dotnet build src/ReqChecker.App/ReqChecker.App.csproj -c Debug`. Confirm 0 errors. Resolve any leftover `AnimatedPageHeader` references not caught above.
- [ ] T024 [US1] Run the app and walk every primary view + dialog. Visually confirm: no icon-in-square decoration, no accent-line strip, no restated subtitle. Back/primary action work. Header heights look consistent. Tick the US1 boxes in [quickstart.md](./quickstart.md) Pass 1.

**Checkpoint**: At this point, US1 is fully functional. The app is shippable as an MVP — all the biggest "AI tells" are gone from headers across the entire surface. Other story work can begin in parallel from here.

---

## Phase 4: User Story 2 — Sections (Priority: P1)

**Goal**: Configuration views drop bordered/elevated `<Border>` card chrome around groups of fields. Sections become typographic headings + whitespace.

**Independent Test**: After Phase 4, every configuration view groups its fields by section heading + whitespace. No surrounding rounded-border card. No icon-in-rounded-square next to section headings.

### TestConfigView.xaml — the largest single edit

- [X] T025 [US2] In [src/ReqChecker.App/Views/TestConfigView.xaml](src/ReqChecker.App/Views/TestConfigView.xaml) lines 141–204 ("Basic Information" section): replace the outer `<Border Style="{StaticResource AnimatedSection}">` wrapping the inner `<Border Style="{StaticResource ParameterGroupCard}">` with a single `<StackPanel Margin="{StaticResource SpacingSectionSection}">`. Replace the section-header sub-tree (icon-in-square + `TextH3` title) with a single `<TextBlock Style="{StaticResource SectionHeadingStyle}" Text="Basic Information"/>`.
- [X] T026 [US2] In [src/ReqChecker.App/Views/TestConfigView.xaml](src/ReqChecker.App/Views/TestConfigView.xaml) lines 208–268 ("Execution Settings" section): same recipe as T025.
- [X] T027 [US2] In [src/ReqChecker.App/Views/TestConfigView.xaml](src/ReqChecker.App/Views/TestConfigView.xaml) lines 271–370 ("Test Parameters" section): same recipe as T025. Keep the inner `ItemsControl` and its `DataTemplate` as-is; only the section wrapper changes.

### Other configuration views — per-view audit + edit (parallel)

- [X] T028 [P] [US2] Audit [src/ReqChecker.App/Views/SettingsView.xaml](src/ReqChecker.App/Views/SettingsView.xaml) for hand-rolled card/border wrappers around field groups. For each found: replace with `StackPanel` + `SectionHeadingStyle` heading per the research.md Decision 2 recipe. Note in the commit message which line ranges were touched.
- [X] T029 [P] [US2] Audit [src/ReqChecker.App/Views/DiagnosticsView.xaml](src/ReqChecker.App/Views/DiagnosticsView.xaml) for `DiagnosticCard` / `DiagnosticCardHighlight` / `NetworkInterfaceCard` style usage. These DiagnosticCard styles in [Controls.xaml](src/ReqChecker.App/Resources/Styles/Controls.xaml) lines 530–560 *may* survive as legitimate "data card" containers (they represent discrete pieces of diagnostic data, not form sections) — assess per-card. Replace any used purely as section-grouping chrome.
- [X] T030 [P] [US2] Audit [src/ReqChecker.App/Views/SchedulesView.xaml](src/ReqChecker.App/Views/SchedulesView.xaml) for grouping chrome. Apply the recipe where applicable. Schedule items in a list are *data cards* (each schedule is a discrete entity) — keep those.
- [X] T031 [P] [US2] Audit [src/ReqChecker.App/Views/ResultsView.xaml](src/ReqChecker.App/Views/ResultsView.xaml) for grouping chrome. Apply recipe to summary/header sections. Per-test result cards in the list are data cards — keep those.
- [X] T032 [P] [US2] Audit [src/ReqChecker.App/Views/ProfileSelectorView.xaml](src/ReqChecker.App/Views/ProfileSelectorView.xaml) for form-grouping chrome. Apply recipe. Per-profile cards in the profile list are data cards — keep those.
- [X] T033 [P] [US2] Audit [src/ReqChecker.App/Views/HistoryView.xaml](src/ReqChecker.App/Views/HistoryView.xaml) for form-grouping chrome (filter/summary sections). Apply recipe. Per-run history cards are data cards — keep those.

### Verification

- [ ] T034 [US2] Build the app. Confirm 0 errors. Resolve any leftover `ParameterGroupCard` references not caught above.
- [ ] T035 [US2] Run the app, open every configuration view. Confirm: section chrome gone, headings are typographic, whitespace separates sections. Functional affordances unchanged. Tick the US2 boxes in [quickstart.md](./quickstart.md) Pass 1.

**Checkpoint**: At this point, US1 + US2 are complete. Configuration views read as documents, not card collages.

---

## Phase 5: User Story 3 — Motion (Priority: P2)

**Goal**: Zero decorative entrance animation across all views. Pages appear instantly. Only purposeful motion (072 auto-scroll, native focus/hover/press, ScrollViewer inertia) remains.

**Independent Test**: Navigate between every view. Pages appear with no fade, no slide, no per-section stagger. 072 auto-scroll on RunProgressView still works.

- [X] T036 [US3] Delete the inline `<Style x:Key="AnimatedSection" TargetType="Border">…</Style>` block (lines 19–48) from `<Page.Resources>` in [src/ReqChecker.App/Views/TestConfigView.xaml](src/ReqChecker.App/Views/TestConfigView.xaml). All three consumers (lines 141, 208, 271) were already removed in Phase 4 (T025–T027), so this deletes orphaned style declaration.
- [X] T037 [P] [US3] Sweep [src/ReqChecker.App/Views](src/ReqChecker.App/Views) for any other inline `<Style>` blocks containing `EventTrigger RoutedEvent="Loaded"` with `Opacity`/`TranslateTransform` animations. For each found, delete the style declaration AND any `Style=` reference in the same file. (Likely candidates discovered during US1 work that weren't catalogued upfront.)
- [X] T038 [P] [US3] Sweep [src/ReqChecker.App/Resources/Styles/Controls.xaml](src/ReqChecker.App/Resources/Styles/Controls.xaml) for any remaining decorative entrance Storyboards (search for `EventTrigger RoutedEvent="Loaded"` with `DoubleAnimation` on `Opacity`). Confirm `AnimatedPageHeader` (already deleted in T007) is the only such style, or delete the others.
- [ ] T039 [P] [US3] Confirm [src/ReqChecker.App/Resources/Styles/Animations.xaml](src/ReqChecker.App/Resources/Styles/Animations.xaml) contains only timing tokens (`DurationFast`, etc.) and visual-state Storyboards for native controls — NOT page-level entrance animations. If any decorative storyboards exist, delete them.
- [ ] T040 [US3] Build and run. Navigate between every view. Confirm pages appear instantly with no entrance animation.
- [ ] T041 [US3] **072 auto-scroll regression check**: Start a run with the Completed Tests panel visible. Verify: (a) at-bottom → new completions auto-scroll, (b) scroll up → follow-mode pauses, (c) scroll back to bottom → follow-mode resumes, (d) start a new run → list clears and follow-mode resumes. Tick the US3 + 072 boxes in [quickstart.md](./quickstart.md) Pass 1 and Pass 2.

**Checkpoint**: Motion is calm and purposeful. App feels confidently desktop-native rather than "presented."

---

## Phase 6: User Story 4 — Spacing (Priority: P2)

**Goal**: Apply the semantic spacing tokens added in Phase 2 (T004) across views. Label-field gaps tight, field-field gaps medium, section-section gaps large.

**Independent Test**: Pick a label/field pair in any configuration view; gap is visibly tighter than the gap to the next field. Sections separate visibly more than fields within a section. Verifies at 1024×720 too.

- [X] T042 [P] [US4] In [src/ReqChecker.App/Views/TestConfigView.xaml](src/ReqChecker.App/Views/TestConfigView.xaml), audit every `Margin="0,0,0,12"` and `Margin="0,0,0,16"` on Grid/StackPanel rows. Replace with the appropriate semantic token (`SpacingFieldField` for between fields in a section; `SpacingSectionSection` is already applied by Phase 4's StackPanel margins).
- [X] T043 [P] [US4] In [src/ReqChecker.App/Views/SettingsView.xaml](src/ReqChecker.App/Views/SettingsView.xaml), apply the same spacing audit. Use semantic tokens for label-field and field-field gaps. **Audit result**: One legitimate field-to-field row found in the Reset section (description-to-button gap) — converted to `SpacingFieldField`. Theme picker, About section, etc. are not label/field forms.
- [X] T044 [P] [US4] In [src/ReqChecker.App/Views/DiagnosticsView.xaml](src/ReqChecker.App/Views/DiagnosticsView.xaml), apply the spacing audit. **Audit result**: No form-style label/field rows. Existing `0,0,0,12` and `0,0,0,16` margins are for status messages, decorative icon spacing, and section-header gaps — none are field-to-field gaps. Left unchanged.
- [X] T045 [P] [US4] In [src/ReqChecker.App/Views/SchedulesView.xaml](src/ReqChecker.App/Views/SchedulesView.xaml), apply the spacing audit, especially around the new-schedule form areas. **Audit result**: Schedule creation form lives in `CreateScheduleDialog` (out of scope per Q1). Top-level `SchedulesView` has only schedule data cards in a list — list-item spacing, not form spacing. Left unchanged.
- [X] T046 [P] [US4] In [src/ReqChecker.App/Views/ProfileSelectorView.xaml](src/ReqChecker.App/Views/ProfileSelectorView.xaml), apply the spacing audit to any form/filter areas. **Audit result**: ProfileSelectorView has profile data cards in a list, no form areas. Left unchanged.
- [X] T047 [P] [US4] In every view modified in Phase 3 (US1), add `Margin="{StaticResource SpacingHeaderToBody}"` to the header `Grid`/`StackPanel` so the header separates from the page body by a consistent 24 px. **Done across all 9 top-level views.**
- [ ] T048 [US4] Resize the running app to 1024×720 and walk every configuration view. Confirm the rhythm survives: label-field tight, field-field medium, section-section large. Tick the US4 boxes in [quickstart.md](./quickstart.md) Pass 1.

**Checkpoint**: Spacing now *means* something. The whitespace does real work for grouping.

---

## Phase 7: User Story 5a — Navigation Rail (Priority: P2)

**Goal**: Quiet the left navigation rail so it reads as part of the new design language. Icons preserved (functional); chrome (active background, icon color, spacing) audited.

**Independent Test**: Navigate between destinations. Active destination is indicated by a subtle leading accent stripe (3 px) or a tinted background — not by an oversized chrome treatment. Hover/focus visuals are clear and subtle. Keyboard nav still works.

- [X] T049 [US5a] In [src/ReqChecker.App/MainWindow.xaml](src/ReqChecker.App/MainWindow.xaml), locate the `NavigationView` (or equivalent rail control) and its item template. Identify the active-state visual treatment (likely a `Trigger`/`VisualState` on `IsActive`/`IsSelected`). **Done**: applied `Style="{StaticResource NavigationViewItemStyle}"` to all 7 nav items.
- [X] T050 [US5a] Replace the active-state background with a 3-px leading accent stripe drawn via a leading `Border` (width = `SidebarActiveIndicatorWidth` from `Spacing.xaml`, `Background={DynamicResource AccentPrimary}`). Optionally pair with a subtle `BackgroundSurface` tint on the row. **Done**: `NavigationViewItemStyle` now sets `BorderThickness="3,0,0,0"` with `BorderBrush=Transparent` by default and `BorderBrush=AccentPrimary` on `IsActive=True`.
- [X] T051 [US5a] Set inactive icon foreground to `{DynamicResource TextSecondary}`. Set hover icon foreground to `{DynamicResource TextPrimary}`. Set active icon foreground to `{DynamicResource AccentPrimary}`. **Done** in `NavigationViewItemStyle` triggers.
- [X] T052 [US5a] Remove any oversized icon containers (filled circles, rounded squares behind icons) if they exist on rail items. The icon glyph sits on the rail row directly. **Audit result**: no oversized containers existed — WPF-UI's `NavigationViewItem` already uses inline `Icon="{ui:SymbolIcon …}"`.
- [ ] T053 [US5a] Verify keyboard navigation (Tab into rail, arrow keys to move, Enter to activate) still works and shows a WCAG-AA focus visual.

**Checkpoint**: The nav rail now feels like the same product as the redesigned pages.

---

## Phase 8: User Story 5 — Status Indicators (Priority: P3)

**Goal**: Replace the colored-pill-with-italic-white-text status indicators with typography-led inline compositions.

**Independent Test**: On Test Configuration, find a prompt-at-run parameter. The "Will be prompted during test execution" indicator is plain text (not italic) with a small leading icon — no pill background, no drop shadow.

- [X] T054 [US5] In [src/ReqChecker.App/Views/TestConfigView.xaml](src/ReqChecker.App/Views/TestConfigView.xaml) at line ~331–344, replace the `<Border Style="{StaticResource PromptAtRunIndicator}">` block with the inline composition from research.md Decision 5:

  ```xml
  <StackPanel Orientation="Horizontal" Margin="0,4,0,0"
              Visibility="{Binding IsPromptAtRun, Converter={StaticResource BoolToVisibilityConverter}}">
      <ui:SymbolIcon Symbol="Key24" FontSize="14"
                     Foreground="{DynamicResource AccentPrimary}"
                     Margin="0,0,8,0" VerticalAlignment="Center"/>
      <TextBlock Text="Will be prompted during test execution"
                 Style="{StaticResource CaptionTextStyle}"
                 Foreground="{DynamicResource TextSecondary}"
                 VerticalAlignment="Center"/>
  </StackPanel>
  ```

  Preserve the `Grid.Column="1"` placement and the `Visibility` binding to `IsPromptAtRun`.

- [ ] T055 [P] [US5] Audit every XAML in [src/ReqChecker.App/Views](src/ReqChecker.App/Views) for `Border` elements with `CornerRadius="6"` AND `Background={DynamicResource Accent*}` (chip/pill style). Each is a candidate for the typography-led treatment. For each: assess whether the element is an *interactive chip/filter* (preserve, possibly slightly de-saturate) or an *ambient status indicator* (convert to typography-led).
- [ ] T056 [US5] Build and run. Confirm the prompt-at-run indicator reads as text with a leading glyph. Confirm no remaining "white italic text on accent pill" patterns exist anywhere. Tick the US5 boxes in [quickstart.md](./quickstart.md) Pass 1.

**Checkpoint**: Status text reads as text. All decorative pill chrome is gone.

---

## Phase 9: Polish & Cross-Cutting Concerns

- [ ] T057 [P] Light/Dark theme parity: toggle between themes on every view. Verify nothing is illegible or visually broken. Resolve any per-theme regressions (likely candidates: hairline rules with insufficient contrast in Dark).
- [ ] T058 [P] WCAG AA contrast audit: spot-check title text, body text, secondary text, focus visuals, hairline rules. Use Color Oracle or WebAIM contrast checker. Document any near-misses in the PR description.
- [ ] T059 [P] Accessibility regression check: Tab through every view, run NVDA/Narrator on RunProgressView during a live run, verify mnemonics (Alt+T, Alt+R) on Test Config. Tick the accessibility checklist in [quickstart.md](./quickstart.md) Pass 2.
- [ ] T060 [P] Small-window check: resize app to 1024×720 and walk every view. Confirm nothing clips, scrollbars appear only where expected, spacing rhythm survives.
- [ ] T061 Functional regression sweep: walk the full app loop per [quickstart.md](./quickstart.md) Pass 2 (Open profile → edit test → save → run → view results → view history → settings → schedules). Confirm no functional regressions.
- [ ] T062 072 auto-scroll regression: re-verify auto-scroll behavior one more time on the final build (this was already verified in T041 but is repeated as a final gate because RunProgressView is the most-likely-to-regress view).
- [ ] T063 Product-owner subjective verdict (SC-001): user rates UI on the "AI-built" scale. Target: ≤ 4/10. Document in the PR description.
- [ ] T064 Capture post-refresh screenshots matching the T001 baseline set (12 views × 2 themes = 24 screenshots). Save into `specs/073-design-language-refresh/post-refresh/` for before/after comparison. (Do not commit.)
- [ ] T065 Update `CLAUDE.md` notes if any new shared style or token requires documenting beyond what `update-agent-context.ps1` already wrote.
- [ ] T066 Commit, push, and merge to master following the 071/072 pattern: `git push -u origin 073-design-language-refresh`, then `git checkout master && git merge --no-ff 073-design-language-refresh -m "Merge branch '073-design-language-refresh'" && git push origin master`. (Do NOT execute without explicit user confirmation; this is a destructive-to-history operation worth confirming.)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1. **Blocks ALL user stories** because user stories consume tokens/styles added here.
- **Phase 3 (US1 — Headers)**: Depends on Phase 2. MVP-shippable on its own.
- **Phase 4 (US2 — Sections)**: Depends on Phase 2. Can start in parallel with Phase 3 if staffed by a different developer, since the files affected (configuration views) overlap with Phase 3 files but at different XAML elements (header vs body). Practical sequencing: complete Phase 3 first to avoid merge conflicts.
- **Phase 5 (US3 — Motion)**: Depends on Phase 2 (T007 already deletes `AnimatedPageHeader`). The remaining `AnimatedSection` deletion (T036) depends on Phase 4 having removed its consumers (T025–T027). Practical sequencing: after Phase 4.
- **Phase 6 (US4 — Spacing)**: Depends on Phase 2 (semantic tokens added in T004) and on Phases 3 & 4 (headers and sections need to exist in their new form before spacing is applied to them).
- **Phase 7 (US5a — Nav Rail)**: Depends on Phase 2 only. Can run fully in parallel with Phases 3–6.
- **Phase 8 (US5 — Status Indicators)**: Depends on Phase 2 (T008 deletes `PromptAtRunIndicator`). Practical sequencing: after Phase 6 (lowest priority, least blast radius).
- **Phase 9 (Polish)**: Depends on all desired user stories being complete.

### User Story Independence

Each story phase ends with a usable, releasable increment:

- After **US1 (Phase 3)**: app is shippable as MVP — header chrome removed everywhere.
- After **US2 (Phase 4)**: configuration views read as documents.
- After **US3 (Phase 5)**: motion is calm.
- After **US4 (Phase 6)**: spacing has hierarchy.
- After **US5a (Phase 7)**: nav rail matches the new look.
- After **US5 (Phase 8)**: status indicators are typography-led.

A stop after any phase is acceptable. If review feedback in any phase is heavy, the previous phase is still an integrated, shippable state.

### Parallel Opportunities

- **Within Phase 3**: T011–T022 are all `[P]` — 12 different files, no inter-dependencies. A single developer can edit them sequentially or batch-edit them all then verify.
- **Within Phase 4**: T028–T033 are all `[P]` (different files). T025–T027 are sequential (same file, `TestConfigView.xaml`).
- **Within Phase 5**: T037–T039 are `[P]` (different files).
- **Within Phase 6**: T042–T047 are all `[P]` (different files).
- **Within Phase 9**: T057–T060 are all `[P]` (independent checks).
- **Across phases**: Phase 7 (US5a, nav rail in `MainWindow.xaml`) can run fully in parallel with Phases 3–6 since it touches an entirely different file.

---

## Parallel Example: Phase 3 (User Story 1 — Headers)

```text
# Launch all 12 header rewrites together (different files, no shared state):
T011 [US1] ProfileSelectorView.xaml header
T012 [US1] TestListView.xaml header
T013 [US1] TestConfigView.xaml header
T014 [US1] RunProgressView.xaml header (preserve auto-scroll)
T015 [US1] ResultsView.xaml header
T016 [US1] HistoryView.xaml header
T017 [US1] DiagnosticsView.xaml header
T018 [US1] SchedulesView.xaml header
T019 [US1] SettingsView.xaml header
T020 [US1] CredentialPromptDialog.xaml header
T021 [US1] CreateScheduleDialog.xaml header
T022 [US1] MissedRunsDialog.xaml header
```

Then verify together via T023 (build) and T024 (manual walkthrough).

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 (Setup): screenshots + baseline build.
2. Complete Phase 2 (Foundational): style edits + semantic tokens.
3. Complete Phase 3 (US1 — Headers): all 12 view header rewrites.
4. **STOP and validate**: app builds, all functional affordances work, headers are de-chromed everywhere.
5. Ship as MVP. The single biggest "AI tell" is gone.

### Incremental Delivery

1. Phase 1 + 2 → foundation ready.
2. Phase 3 (US1) → ship MVP.
3. Phase 4 (US2) → ship "configuration views as documents."
4. Phase 5 (US3) → ship "calm motion."
5. Phase 6 (US4) → ship "intentional spacing."
6. Phase 7 (US5a) → ship "nav rail matches."
7. Phase 8 (US5) → ship "typography-led status."
8. Phase 9 (Polish) → final verification + merge.

Each ship point is a coherent step toward the SC-001 target. A subjective rating after each step would let you stop early if the goal is already met.

### Single-developer strategy

Sequential phases as listed. Estimated effort:

- Phase 1–2: ~1 hour (style edits, no per-view work).
- Phase 3: ~2 hours (12 file edits, mostly mechanical).
- Phase 4: ~2 hours (TestConfigView is the bulk; audits of others may be quick).
- Phases 5–6: ~1 hour combined (deletions + token applications).
- Phase 7: ~1 hour (MainWindow audit + edit).
- Phase 8: ~30 minutes (one element to convert; sweep for others).
- Phase 9: ~1.5 hours (verification + product-owner verdict + merge).

Total: ~9 hours for one developer, or one focused day.

---

## Notes

- [P] tasks = different files, no dependencies — safe to fan out.
- [Story] label maps each task to its user story for traceability and partial-ship decisions.
- The plan's scope estimate ("approximately 20 occurrences of `ParameterGroupCard`") was wrong — the pre-flight inventory at the top of this file replaces those numbers with ground truth from grep.
- Tasks T007–T009 deliberately break the build until corresponding view edits land. This is captured in T010 (commit as atomic, don't push until at least US1 lands).
- T066 (merge to master) is the only task that requires explicit user confirmation per the project's prior pattern (071 and 072 were both merged via explicit user request).
