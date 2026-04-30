# Implementation Plan: Profile Manager List Redesign (Premium UI/UX)

**Branch**: `068-profile-list-redesign` | **Date**: 2026-04-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/068-profile-list-redesign/spec.md`

## Summary

Replace the Profile Manager's wrapping grid of fixed-width cards (`ItemsControl` + `WrapPanel` + 320 px tiles) with a vertical, virtualized list of full-width rows that mirrors the canonical row pattern already used by `HistoryView` (and similar in `TestListView` / `SchedulesView`). The redesign also collapses the recommended-profile signal from three competing decorations to a single labeled badge, restyles the source label as a quiet metadata chip, adds a calm last-modified recency indicator, removes the redundant per-row "Select Profile" button, persists a visual selected/active state for the currently-loaded profile, and exposes proper list/listbox semantics to assistive technologies. All visual tokens (Card, CardSelected, RecommendedBadge, FocusVisualStyle, AccentSubtle, FriendlyDateConverter) are reused from the existing design system; no new tokens, packages, or persisted data are introduced.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (`net8.0-windows` TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2 (all existing — no new packages)
**Storage**: N/A (UI-only feature; profile data continues to be loaded from embedded resources for bundled profiles and from `%APPDATA%/ReqChecker/profiles/*.json` for user profiles via existing `IProfileStorageService`)
**Testing**: xUnit + Moq (existing patterns in `tests/ReqChecker.App.Tests/ViewModels/`)
**Target Platform**: Windows desktop (Windows 10 1809+ / Windows 11)
**Project Type**: Desktop application (single project tree under `src/`)
**Performance Goals**: 60 fps target for the list (≥ 55 fps measured with 50 profiles per SC-006); state transitions ≤ 200 ms (FR-017, SC-005); entrance staggers ≤ 300 ms total (SC-005)
**Constraints**: No layout shift on hover/focus/select (FR-017); no horizontal scrolling (FR-003); rows always full content width; accessible names + list semantics required (FR-019a/b); no new design tokens beyond the existing system (Out of Scope)
**Scale/Scope**: Typical user has < 20 profiles; redesign must remain smooth at 50; one screen (`ProfileSelectorView`) and one ViewModel (`ProfileSelectorViewModel`) plus a new per-row presentation VM

All required information is resolved against the existing codebase — no `NEEDS CLARIFICATION` items remain after Phase 0 research.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

`.specify/memory/constitution.md` is the unfilled template (placeholders, no ratified principles). No constitution gates apply to this feature; the file is treated as a no-op pass per its current state. This will be re-evaluated automatically once the project constitution is ratified.

**Initial Constitution Check**: PASS (no gates defined).
**Post-Design Constitution Check**: PASS (no gates defined; design uses only existing libraries and design tokens).

## Project Structure

### Documentation (this feature)

```text
specs/068-profile-list-redesign/
├── plan.md                         # This file
├── research.md                     # Phase 0 — decisions grounded in existing codebase patterns
├── data-model.md                   # Phase 1 — Profile presentation entity + per-row VM shape
├── quickstart.md                   # Phase 1 — how to run, verify, and review the redesign
├── contracts/
│   ├── view-model-contract.md      # Phase 1 — what the View binds to (per-row + page-level)
│   └── visual-contract.md          # Phase 1 — row anatomy, states, design tokens used
└── checklists/
    └── requirements.md             # Already produced by /speckit.specify
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Views/
│   │   ├── ProfileSelectorView.xaml          # REWRITE — list of full-width rows, virtualized
│   │   └── ProfileSelectorView.xaml.cs       # MINOR — keyboard activation handler if needed
│   ├── ViewModels/
│   │   ├── ProfileSelectorViewModel.cs       # EXTEND — bind to active-profile change events; expose row VMs
│   │   └── ProfileListItemViewModel.cs       # NEW — per-row presentation VM (name, source, count, version, modified, IsRecommended, IsActive)
│   ├── Converters/
│   │   ├── ProfileRecommendedConverter.cs    # KEEP — used by row VM (or moved into VM, see plan below)
│   │   └── FriendlyDateConverter.cs          # REUSE — already produces "3 days ago" / "Apr 28"
│   └── Resources/Styles/
│       └── Controls.xaml                      # REUSE only — Card, CardSelected, FocusVisualStyle, RecommendedBadge, AccentSubtle (no new keys added)
├── ReqChecker.Core/
│   └── Models/Profile.cs                     # NO CHANGE
└── ReqChecker.Infrastructure/                # NO CHANGE

tests/
└── ReqChecker.App.Tests/
    └── ViewModels/
        ├── ProfileSelectorViewModelTests.cs  # EXTEND — active-profile sync, row VM emission
        └── ProfileListItemViewModelTests.cs  # NEW — per-row computed properties, IsActive sync
```

**Structure Decision**: Single-project desktop layout, matching the rest of the repository. Only the App project (View + ViewModels + tests) is touched. No changes to Core or Infrastructure projects, no new project, no new package.

## Phase 0 — Outline & Research

Detailed findings live in [`research.md`](./research.md). Summary of the key decisions made during research:

1. **Canonical row pattern is `HistoryView`'s virtualized `ListBox`.** It already uses `VirtualizingPanel.IsVirtualizing="True"` + `VirtualizationMode="Recycling"` + `ScrollUnit="Pixel"`, plus a stripped-down `ListBoxItem` template wired to `FocusVisualStyle`, `AutomationProperties.Name` for the list, and `Style="{StaticResource Card}"` on each row's inner Border. Adopting this pattern verbatim is what produces SC-001 (consistency) and SC-006 (smooth scroll at 50+).
2. **Selected state is already a token.** `CardSelected` (accent border, accent glow) is the existing visual contract for "selected." Bind `ListBox.SelectedItem` to a row VM whose container picks `CardSelected` vs. `Card` via a style trigger on `IsSelected`.
3. **Active profile sync uses `IAppState.CurrentProfileChanged`.** When the user navigates back to Profile Manager after selecting, every row VM's `IsActive` is recomputed from `_appState.CurrentProfile?.Id == this.Profile.Id`. The `ListBox.SelectedItem` is initialized to that row so FR-009a is satisfied without a custom event flow.
4. **Recency indicator uses file `LastWriteTime` for user profiles; bundled profiles omit the field.** `IProfileStorageService.GetProfileFilePaths()` provides the path for user profiles. Bundled profiles are embedded resources with no on-disk timestamp; the spec already requires graceful omission (FR-013).
5. **Redundant decorations on Recommended are removed.** Today's view stacks: `RecommendedBadge` + a `BorderThickness=2` accent border + a 6px gradient header strip. The badge is kept; the border and gradient are removed for the recommended state. Per FR-015/FR-016, every row gets identical baseline chrome.
6. **Source chip becomes quiet metadata.** Instead of solid `AccentSecondary` background + white text, use `Background=Transparent`, `BorderBrush=BorderSubtle`, `Foreground=TextSecondary` — purely outlined, calmer than primary actions on the page.
7. **Header/banner duplication is resolved by removing the banner's gradient accent line** (the page header keeps it; the banner becomes a calmer rounded callout). No content change to the welcome banner — it stays dismissible.
8. **No new design tokens introduced.** All hover/focus/selection visuals compose existing brushes (`AccentPrimary`, `AccentSecondary`, `BorderDefault`, `BackgroundSurface`, `BackgroundElevated`, `BorderSubtle`, `TextPrimary`, `TextSecondary`, `TextTertiary`).

**Output**: `research.md` (created in this phase).

## Phase 1 — Design & Contracts

**Prerequisites**: `research.md` complete (above).

### Data model

Detailed in [`data-model.md`](./data-model.md). Summary:

- **No persisted-model changes.** `Profile` (in `ReqChecker.Core.Models`) is unchanged.
- **New presentation entity**: `ProfileListItemViewModel` wraps a `Profile` + the row's runtime state (`IsActive`, `IsRecommended`, `LastModifiedUtc?`) and exposes the formatted strings the row binds to (`SourceLabel`, `TestCountLabel`, `SchemaVersionLabel`, `ModifiedLabel`, `AccessibleName`).
- **Page-level VM** (`ProfileSelectorViewModel`) gains:
  - `ObservableCollection<ProfileListItemViewModel> Items` (replaces direct `Profiles` binding for the list, while keeping `Profiles` for back-compat in tests if needed),
  - `ProfileListItemViewModel? SelectedItem` (two-way bound to the `ListBox.SelectedItem`; setting it triggers the existing `SelectProfile(profile)` flow),
  - subscription to `IAppState.CurrentProfileChanged` to keep `IsActive` in sync.

### Contracts

For a UI-only feature there are no HTTP/RPC contracts. Two artifacts in `contracts/` capture the boundaries that this feature MUST honor:

1. [`view-model-contract.md`](./contracts/view-model-contract.md) — the binding surface: every property and command the view binds to, with type, semantics, and acceptance behavior. This is the "API" of the redesign.
2. [`visual-contract.md`](./contracts/visual-contract.md) — the visual state machine: row anatomy, the four runtime states (default / hover / focus / selected-active), tokens used per state, badge composition rules, and the explicit decorations that MUST NOT appear (e.g., per-row Select button, gradient stripe on recommended).

### Agent context update

After this plan is in place, the project-level agent context (`CLAUDE.md` at the repo root) is updated by `update-agent-context.ps1` to add this feature's tech context line. No new technology to register — all listed dependencies already exist in the project — so the script's job is to stamp the entry. Run:

```powershell
.\.specify\scripts\powershell\update-agent-context.ps1 -AgentType claude
```

### Quickstart

[`quickstart.md`](./quickstart.md) — minimal local steps to (a) check out the branch, (b) build, (c) run the app, (d) reach Profile Manager, (e) verify the eight Success Criteria visually and via the existing unit-test command.

**Output**: `data-model.md`, `contracts/view-model-contract.md`, `contracts/visual-contract.md`, `quickstart.md`, agent context stamped.

## Complexity Tracking

No constitution gates are defined; no violations to track. The implementation reuses existing design tokens and existing infrastructure (ListBox virtualization, IAppState events, IProfileStorageService paths, FriendlyDateConverter), and introduces exactly one new file (`ProfileListItemViewModel.cs`) plus its test. Complexity remains within "small UI feature" budget.
