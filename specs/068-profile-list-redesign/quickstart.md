# Quickstart — Profile Manager List Redesign

**Branch**: `068-profile-list-redesign`
**Date**: 2026-04-30

---

## 1. Prerequisites

- Windows 10 (1809+) or Windows 11
- .NET 8 SDK
- The branch is already checked out: `068-profile-list-redesign`

## 2. Build and run

```powershell
dotnet build src/ReqChecker.App/ReqChecker.App.csproj -c Debug
dotnet run --project src/ReqChecker.App/ReqChecker.App.csproj
```

If you also want to run tests for this feature only:

```powershell
dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj --filter FullyQualifiedName~ProfileSelector --filter FullyQualifiedName~ProfileListItem
```

## 3. Reach the Profile Manager

- On first run, the navigation menu opens on Profiles. Otherwise click **Profiles** in the left navigation.

## 4. Visual verification (against `contracts/visual-contract.md`)

Walk through this checklist; each line maps to a Functional Requirement.

- [ ] Profiles render as a single vertical column of full-width rows (FR-001 / FR-003).
- [ ] Resizing the window from narrow → wide keeps rows full width and never wraps into two columns (FR-003).
- [ ] Side-by-side with the Test History page, the row pattern (height, padding, gaps, hover treatment) reads as the same component family (FR-002, SC-001).
- [ ] No per-row **Select Profile** button is visible (FR-006).
- [ ] Clicking anywhere on a row loads that profile and navigates to Test List (FR-005).
- [ ] The Refresh and Import Profile buttons remain in the page header (Assumptions).

### Recommended profile

- [ ] The recommended profile shows a **Recommended** labeled badge — and *only* the badge (FR-015). No accent border, no gradient strip on its row.
- [ ] If you toggle to a state with no recommended profile, all rows look identical (FR-016).

### Information display

- [ ] Each row shows: profile **Name**, an outlined **Source** chip, **N tests**, **vN** (when version present), and **modified …** (for user profiles).
- [ ] Long names truncate with an ellipsis; hovering the row shows the full name as a tooltip (FR-010).
- [ ] Bundled profiles do NOT display a "modified" segment (FR-013).

### Selected / active state

- [ ] After clicking a profile and navigating to Test List, return to Profiles. That profile's row is rendered in the **selected** state (accent border + glow), distinct from hover and from focus (FR-009 / FR-009a).
- [ ] If you click a different profile, the previous selected state moves to the new row instantly.

### Keyboard parity

- [ ] Tab into the list — the focused row shows a clear ring (FR-019).
- [ ] Up/Down arrow keys move focus between rows; the focused row scrolls into view if needed (FR-008).
- [ ] Pressing **Enter** or **Space** on the focused row loads that profile (FR-007).
- [ ] You can complete the full selection task without touching the mouse (SC-004).

### Motion

- [ ] Hover transitions feel quick and quiet — no color flash, no layout shift (FR-017).
- [ ] Entrance animation is subtle and ends within ~300 ms across the first visible rows (SC-005).
- [ ] If you toggle Windows Settings → Accessibility → Visual effects → **Animation effects = Off**, the entrance and hover animations stop (FR-018).

### Header / banner

- [ ] The page header still has its gradient accent line. The welcome banner does NOT (FR-004).
- [ ] Dismissing the banner removes it without leaving an empty gap above the list.

### Edge / loading / error states

- [ ] When loading, only the centered progress ring is visible — no list shell flicker (FR-022).
- [ ] When no profiles exist, the empty state is centered and intact (FR-021).
- [ ] When an error occurs (you can simulate by deleting your user-profiles directory and clicking Refresh), the inline error banner appears and does not stack with the page header decoration (FR-023).

### Performance

- [ ] With 50 profiles loaded (you can copy a bundled profile JSON 50 times into `%APPDATA%/ReqChecker/profiles/` with unique names/IDs and click Refresh), scrolling stays smooth (≥ 55 fps subjectively — SC-006).

### Accessibility (light pass)

- [ ] Open Windows Narrator (`Win + Ctrl + Enter`). Navigate into the list.
- [ ] Narrator announces the container as a list (FR-019a).
- [ ] Narrator announces each row by the profile name; recommended row is announced with the "(recommended)" suffix (FR-019b).
- [ ] When a profile is the active one, Narrator announces it as selected on focus (FR-009b).

## 5. Reset between runs

- The active-profile state is in memory (`IAppState`); restarting the app clears it.
- Welcome-banner state lives in `%APPDATA%/ReqChecker/preferences.json`. Delete that file to see the banner again.
- User profiles live in `%APPDATA%/ReqChecker/profiles/`. Delete or restore as needed.

## 6. If something looks off

- Cross-check the [`contracts/visual-contract.md`](./contracts/visual-contract.md) "Decorations explicitly forbidden" section first — most regressions surface as one of those reappearing.
- If the row visuals desync from `HistoryView`, diff `ProfileSelectorView.xaml`'s `<ListBox>` block against `HistoryView.xaml:209–245`; they should match structurally except for the row template.
