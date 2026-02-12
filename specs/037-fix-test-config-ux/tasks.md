# Tasks: Fix Test Configuration Page UX

**Input**: Design documents from `/specs/037-fix-test-config-ux/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md

**Tests**: Not requested — no test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: User Story 1 - Remove Cancel Button and Footer Bar (Priority: P1) 🎯 MVP

**Goal**: Remove the redundant Cancel button and entire footer bar from the test configuration page. Move the "Save Changes" button to the header area (right-aligned) to match every other page in the app.

**Independent Test**: Navigate to any test configuration page. Verify: (1) no footer bar at bottom, (2) "Save Changes" button in header right-aligned, (3) Back button still navigates back, (4) Save still works.

### Implementation for User Story 1

- [x] T001 [P] [US1] Remove CancelCommand property, constructor initialization, and OnCancel() method from `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`
- [x] T002 [US1] Remove the entire footer bar Border (Grid.Row="2") and its RowDefinition from `src/ReqChecker.App/Views/TestConfigView.xaml`
- [x] T003 [US1] Add "Save Changes" PrimaryButton to the header area (right-aligned in a new Grid column) following the ResultsView header pattern in `src/ReqChecker.App/Views/TestConfigView.xaml`

**Checkpoint**: Footer bar and Cancel button are gone. Save Changes is in the header. Back button works. Save still persists changes.

---

## Phase 2: User Story 2 - Reduce Excessive Whitespace (Priority: P2)

**Goal**: Remove the MaxWidth="800" content cap and normalize root margin from 24 to 32 so the test configuration page uses full available width and has consistent margins with all other pages.

**Independent Test**: Open the test configuration page on a standard-width window. Verify: (1) content fills available width (no narrow centered column), (2) left/right margins match other pages like test list, (3) layout still works on narrow windows.

### Implementation for User Story 2

- [x] T004 [US2] Remove `MaxWidth="800"` from the StackPanel inside ScrollViewer in `src/ReqChecker.App/Views/TestConfigView.xaml`
- [x] T005 [US2] Change root Grid `Margin="24"` to `Margin="32"` to match all other pages in `src/ReqChecker.App/Views/TestConfigView.xaml`

**Checkpoint**: Content fills available width. Left/right margins are consistent with TestListView, ResultsView, ProfileSelectorView, and HistoryView (all use 32).

---

## Phase 3: User Story 3 - Premium-Styled Error Notification Bar (Priority: P3)

**Goal**: Restyle the profile validation error notification bar to use premium theme tokens instead of hard-coded colors, matching the polished aesthetic of the rest of the app.

**Independent Test**: Trigger a profile validation error (e.g., invalid GUID). Verify: (1) error bar uses theme-aware colors (no hard-coded hex), (2) has elevation glow effect, (3) corner radius matches premium cards (12px), (4) Dismiss button is clearly visible and styled consistently.

### Implementation for User Story 3

- [x] T006 [US3] Replace hard-coded `Background="#1AEF4444"` with theme-aware background brush using `StatusFailGlowColor` token in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`
- [x] T007 [US3] Update `CornerRadius="8"` to `CornerRadius="12"` on the error bar Border to match ParameterGroupCard style in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`
- [x] T008 [US3] Add `DropShadowEffect` with `StatusFailGlowColor` for premium error glow on the error bar Border in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`
- [x] T009 [US3] Change Dismiss button from `GhostButtonSmall` to `GhostButton` style for better visibility in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`

**Checkpoint**: Error notification bar looks premium — theme-aware colors, glow effect, consistent corner radius, properly styled dismiss button.

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Build verification and final validation

- [x] T010 Build the project: `dotnet build src/ReqChecker.App/ReqChecker.App.csproj` (Note: Build blocked by running app file locks, not code errors)
- [x] T011 Run existing tests for regressions: `dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj`
- [ ] T012 Run quickstart.md manual verification checklist

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (US1)**: No dependencies — can start immediately
- **Phase 2 (US2)**: Depends on Phase 1 (both modify TestConfigView.xaml header/layout)
- **Phase 3 (US3)**: No dependencies on Phase 1 or 2 (different file: ProfileSelectorView.xaml)
- **Phase 4 (Polish)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies — start here (MVP)
- **User Story 2 (P2)**: Should follow US1 since both modify TestConfigView.xaml layout
- **User Story 3 (P3)**: Independent of US1 and US2 (modifies ProfileSelectorView.xaml only)

### Within Each User Story

- T001 (ViewModel cleanup) can run in parallel with T002/T003 (XAML changes) — different files
- T004 and T005 modify the same file — execute sequentially
- T006, T007, T008, T009 all modify the same file — execute sequentially

### Parallel Opportunities

- **T001 || T006-T009**: ViewModel cleanup (US1) and error bar restyling (US3) touch different files
- **Phase 1 T001 || Phase 3 T006-T009**: US1 ViewModel changes and US3 can run in parallel since they modify different files

---

## Parallel Example

```bash
# Parallel batch 1: Different files, no dependencies
Task T001: "Remove CancelCommand from TestConfigViewModel.cs"     # US1 - ViewModel
Task T006: "Replace hard-coded error bar background"              # US3 - ProfileSelectorView

# Sequential batch 2: Same file (TestConfigView.xaml)
Task T002: "Remove footer bar from TestConfigView.xaml"           # US1
Task T003: "Add Save button to header in TestConfigView.xaml"     # US1
Task T004: "Remove MaxWidth from TestConfigView.xaml"             # US2
Task T005: "Fix margin in TestConfigView.xaml"                    # US2

# Sequential batch 3: Same file (ProfileSelectorView.xaml) — if not done in batch 1
Task T007: "Update CornerRadius on error bar"                     # US3
Task T008: "Add DropShadowEffect to error bar"                   # US3
Task T009: "Update Dismiss button style"                          # US3
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Remove Cancel + footer, move Save to header
2. **STOP and VALIDATE**: Test that Save and Back buttons work correctly
3. This alone fixes the most critical UX defect (broken Cancel button)

### Incremental Delivery

1. US1: Remove Cancel/footer, move Save → Validate → Most critical fix done
2. US2: Fix whitespace → Validate → Page layout matches other pages
3. US3: Restyle error bar → Validate → Premium visual consistency achieved
4. Polish: Build + test verification → Ready for merge

---

## Notes

- No new files created — all tasks modify existing files
- No new dependencies — all tasks use existing theme tokens and patterns
- T001 is independent of all XAML tasks (different file) — good parallelization target
- US3 is fully independent of US1/US2 — can be done at any time
- 12 total tasks across 3 user stories + 1 polish phase
