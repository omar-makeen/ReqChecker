# Tasks: Subtle Chip Badge

**Input**: Design documents from `/specs/064-subtle-chip-badge/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Tests**: Not required — this is a visual-only XAML restyling with no new logic or properties. Existing MainViewModel tests cover the TestCount/HasTests properties which remain unchanged.

**Organization**: Tasks are grouped by user story. No setup or foundational phase needed — all changes modify existing XAML properties in existing files.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: User Story 1 - Page Header Badge Restyling (Priority: P1) 🎯 MVP

**Goal**: Restyle the "X tests" badge on the Test Suite page header from accent-colored pill to a muted informational chip, so it no longer competes visually with the "Run All Tests" CTA button.

**Independent Test**: Load a profile with tests, navigate to the Test Suite page, and visually confirm the badge uses a muted surface background with secondary text — clearly distinct from the accent-colored "Run All Tests" button. Check in both dark and light themes.

### Implementation for User Story 1

- [X] T001 [US1] Restyle the page header test count badge in `src/ReqChecker.App/Views/TestListView.xaml` (lines 112-120): change `Background` from `AccentPrimary` to `BackgroundSurface`, add `BorderBrush="{DynamicResource BorderSubtle}" BorderThickness="1"`, change text `Foreground` from `White` to `TextSecondary`, change `FontWeight` from `SemiBold` to `Normal`, reduce `Padding` from `16,8` to `10,6`, reduce `CornerRadius` from `16` to `12`

**Checkpoint**: Page header badge appears as a muted chip. "Run All Tests" is the sole accent-colored element. Works in both themes.

---

## Phase 2: User Story 2 - Sidebar Badge Consistency (Priority: P2)

**Goal**: Restyle the sidebar navigation badge on the "Test Suite" nav item to match the muted chip style from US1, ensuring visual consistency.

**Independent Test**: Load a profile, observe the sidebar "Test Suite" nav item in expanded mode, confirm the badge uses the same muted chip style. Toggle theme to verify readability.

### Implementation for User Story 2

- [X] T002 [US2] Restyle the sidebar test count badge in `src/ReqChecker.App/MainWindow.xaml` (lines 93-108): change `Background` from `AccentPrimaryBrush` to `BackgroundSurface`, add `BorderBrush="{DynamicResource BorderSubtle}" BorderThickness="1"`, change text `Foreground` from `TextOnAccentFillColorPrimaryBrush` to `TextSecondary`, change `FontWeight` from `SemiBold` to `Normal`

**Checkpoint**: Sidebar badge matches the page header chip style. Both badges use consistent muted appearance.

---

## Phase 3: Polish & Cross-Cutting Concerns

**Purpose**: Final verification

- [X] T003 Run all existing unit tests to verify no regressions: `dotnet test tests/ReqChecker.App.Tests/`
- [X] T004 Verify both badges in dark and light themes via manual walkthrough per quickstart.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **US1 (Phase 1)**: No dependencies — can start immediately
- **US2 (Phase 2)**: No dependencies on US1 — can run in parallel (different file)
- **Polish (Phase 3)**: Depends on both US1 and US2 being complete

### Parallel Opportunities

- **T001 and T002 can run in parallel** — they modify different files (`TestListView.xaml` vs `MainWindow.xaml`)

---

## Parallel Example

```bash
# These can run simultaneously (different files):
Task T001: "Restyle page header badge in TestListView.xaml"
Task T002: "Restyle sidebar badge in MainWindow.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001: Restyle page header badge
2. **STOP and VALIDATE**: Load profile, verify badge is muted, CTA is prominent
3. Proceed to T002 for sidebar consistency

### Incremental Delivery

1. T001 → Page header badge restyled → Validate
2. T002 → Sidebar badge restyled → Validate
3. T003-T004 → Full regression test and theme verification

---

## Notes

- Both tasks are XAML property changes only — no C# code changes needed
- All existing bindings (TestCount, TestCountDisplay, HasTests, visibility logic) remain unchanged
- Theme resources (`BackgroundSurface`, `BorderSubtle`, `TextSecondary`) are already defined in the app's resource dictionaries
- Commit after each user story for clean git history
