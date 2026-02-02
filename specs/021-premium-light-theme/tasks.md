# Tasks: Premium Light Theme

**Input**: Design documents from `/specs/021-premium-light-theme/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Manual visual verification only (UI-only feature - no automated tests)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Project structure**: `src/ReqChecker.App/` (WPF application)
- Styles: `src/ReqChecker.App/Resources/Styles/`
- Primary file to modify: `Colors.Light.xaml`

---

## Phase 1: Setup (Verification)

**Purpose**: Verify existing infrastructure is ready for feature implementation

- [x] T001 Verify Colors.Light.xaml exists in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T002 Verify Colors.Dark.xaml exists for reference in src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml
- [x] T003 Verify ThemeService.cs theme switching logic in src/ReqChecker.App/Services/ThemeService.cs

---

## Phase 2: Foundational (WPF-UI Text Resource Overrides)

**Purpose**: Add core WPF-UI text color resource overrides that all user stories depend on

**⚠️ CRITICAL**: Navigation visibility fix requires these text resource overrides

- [x] T004 Add TextFillColorPrimaryBrush override (use TextPrimaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T005 Add TextFillColorSecondaryBrush override (use TextSecondaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T006 Add TextFillColorTertiaryBrush override (use TextTertiaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T007 Add TextFillColorDisabledBrush override (use TextTertiaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml

**Checkpoint**: WPF-UI text fill colors now use light theme values

---

## Phase 3: User Story 1 - Navigation Text Visibility (Priority: P1) 🎯 MVP

**Goal**: Fix invisible navigation text by adding NavigationViewItem foreground resource overrides

**Independent Test**: Switch to light mode and verify all navigation items (Profile Manager, Test Suite, Results Dashboard, System Diagnostics, Dark Mode toggle) are clearly readable

### Implementation for User Story 1

- [x] T008 [US1] Add NavigationViewItemForeground brush (use TextSecondaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T009 [US1] Add NavigationViewItemForegroundPointerOver brush (use TextPrimaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T010 [US1] Add NavigationViewItemForegroundSelected brush (use AccentPrimaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T011 [US1] Add NavigationViewItemForegroundSelectedPointerOver brush (use AccentPrimaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T012 [US1] Add NavigationViewItemForegroundPressed brush (use TextPrimaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T013 [US1] Add NavigationViewItemForegroundDisabled brush (use TextTertiaryColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T014 [US1] Add NavigationViewItemHeaderForeground brush (use TextPrimaryColor) for "Navigation" header in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml

**Checkpoint**: All navigation items have visible text in light mode with proper contrast

---

## Phase 4: User Story 2 - Premium Navigation Panel Styling (Priority: P2)

**Goal**: Ensure navigation panel visual quality matches dark mode polish level

**Independent Test**: Compare navigation panel appearance between dark and light modes - both should feel equally polished

### Implementation for User Story 2

- [x] T015 [US2] Verify NavigationViewDefaultPaneBackground uses BackgroundBaseColor in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T016 [US2] Add NavigationViewItemBackgroundPointerOver brush (use BackgroundElevatedColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T017 [US2] Add NavigationViewItemBackgroundSelected brush (use BackgroundSurfaceColor) in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml

**Checkpoint**: Navigation panel has premium appearance in light mode

---

## Phase 5: User Story 3 - Content Area Light Theme Polish (Priority: P2)

**Goal**: Ensure cards, shadows, and content areas display correctly in light mode

**Independent Test**: View all pages (Test Suite, Profile Manager, Results Dashboard, System Diagnostics) in light mode and verify visual quality

### Implementation for User Story 3

- [x] T018 [US3] Add ElevationGlowColor token (#1A000000) for card shadows in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T019 [US3] Add ElevationGlowHoverColor token (#1A000000) for card hover shadows in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [x] T020 [US3] Add ElevationGlowModalColor token (#26000000) for modal shadows in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml

**Checkpoint**: Cards and elevated content display with appropriate shadows in light mode

---

## Phase 6: User Story 4 - Theme Toggle Reliability (Priority: P3)

**Goal**: Verify theme switching works correctly with all new resource overrides

**Independent Test**: Toggle between light and dark modes multiple times, verifying all UI elements update correctly

### Implementation for User Story 4

- [x] T021 [US4] Build application to verify no XAML errors (dotnet build src/ReqChecker.App)
- [ ] T022 [US4] Run application and test theme toggle from dark to light
- [ ] T023 [US4] Test theme toggle from light to dark
- [ ] T024 [US4] Verify theme persists after app restart

**Checkpoint**: Theme toggle works reliably with full visual updates

---

## Phase 7: Polish & Verification

**Purpose**: Final validation and cleanup

- [ ] T025 Run quickstart.md validation checklist against running application
- [ ] T026 Verify all navigation states (normal, hover, selected, disabled) in light mode
- [ ] T027 Check all pages for visual consistency in light mode
- [ ] T028 Compare light mode screenshots before/after fix
- [ ] T029 Verify WCAG AA contrast compliance (4.5:1 minimum) for navigation text

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - verification only
- **Foundational (Phase 2)**: No dependencies - can start after Setup
- **User Story 1 (Phase 3)**: Depends on Phase 2 (text fill colors must be defined first)
- **User Story 2 (Phase 4)**: Can start after Phase 2 - independent of Phase 3
- **User Story 3 (Phase 5)**: Can start after Phase 1 - independent of Phases 3-4
- **User Story 4 (Phase 6)**: Depends on all user stories being complete
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational phase - Navigation visibility requires text fill overrides
- **User Story 2 (P2)**: Can run in parallel with US1 after Foundational phase
- **User Story 3 (P2)**: Can run in parallel with US1, US2 - independent elevation color changes
- **User Story 4 (P3)**: Depends on US1, US2, US3 - verification story

### Within Each User Story

- All tasks within a user story are sequential (same file modifications)
- US1: T008 → T009 → T010 → T011 → T012 → T013 → T014
- US2: T015 → T016 → T017
- US3: T018 → T019 → T020
- US4: T021 → T022 → T023 → T024

### Parallel Opportunities

- After Phase 2 (Foundational) completes:
  - US1 can begin (navigation foreground colors)
  - US2 can begin in parallel (navigation background colors)
  - US3 can begin in parallel (elevation colors)

---

## Parallel Example: After Foundational Phase

```bash
# These can run in parallel (different resource categories in same file):
Developer A: T008-T014 (User Story 1 - Navigation foreground colors)
Developer B: T015-T017 (User Story 2 - Navigation background colors)
Developer C: T018-T020 (User Story 3 - Elevation colors)

# However, since it's a single file, in practice one developer does all sequentially
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (verify files exist)
2. Complete Phase 2: Foundational (add text fill color overrides)
3. Complete Phase 3: User Story 1 (add navigation foreground overrides)
4. **STOP and VALIDATE**: Navigation text should now be visible
5. This MVP delivers the critical P1 fix

### Full Delivery

1. MVP (US1) → Navigation visibility fixed
2. Add User Story 2 → Navigation panel premium styling
3. Add User Story 3 → Card shadow consistency
4. Add User Story 4 → Theme toggle verification
5. Phase 7: Final polish and verification

### Single Developer Strategy

Recommended order for single developer (all changes in one file):
1. T001-T003 (Setup verification)
2. T004-T007 (Foundational - text fill colors)
3. T008-T014 (US1 - navigation foregrounds)
4. T015-T017 (US2 - navigation backgrounds)
5. T018-T020 (US3 - elevation colors)
6. T021-T024 (US4 - verification)
7. T025-T029 (Polish)

---

## Notes

- All XAML changes are in a single file: `Colors.Light.xaml`
- Tasks are sequential within the file (can't truly parallelize same-file edits)
- Manual visual testing required - no automated tests for UI theming
- Build and test after each user story phase
- Reference `Colors.Dark.xaml` for existing resource patterns
- Use `{StaticResource ColorName}` for color references in brush definitions
