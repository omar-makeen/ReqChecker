# Tasks: Premium Page Headers

**Input**: Design documents from `/specs/018-premium-page-headers/`
**Prerequisites**: plan.md, spec.md, research.md, quickstart.md

**Tests**: Not requested in spec. Manual visual verification via quickstart.md.

**Organization**: Tasks grouped by user story for independent implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No setup required - this feature modifies existing files only.

*No tasks - existing project structure is used.*

---

## Phase 2: Foundational (Animation Style)

**Purpose**: Add AnimatedPageHeader style that ALL pages will use.

**⚠️ CRITICAL**: Page header updates cannot use animation until this style is defined.

- [x] T001 Add AnimatedPageHeader style with fade + translateY animation (300ms, CubicEase) in src/ReqChecker.App/Resources/Styles/Controls.xaml

**Checkpoint**: Animation style ready for use in all page headers.

---

## Phase 3: User Story 1 - Premium Header Visual Treatment (Priority: P1) 🎯 MVP

**Goal**: Update all 4 pages with premium header design including gradient accent line, icon container, and refined typography.

**Independent Test**:
1. Launch app and navigate to each page
2. Verify gradient accent line (4px) at top of header
3. Verify icon in 48px accent-colored container
4. Verify larger title text (24px, SemiBold)

### Implementation for User Story 1

- [x] T002 [P] [US1] Update TestListView header with gradient accent line, icon container, and larger title in src/ReqChecker.App/Views/TestListView.xaml
- [x] T003 [P] [US1] Update ProfileSelectorView header with gradient accent line, icon container, and larger title in src/ReqChecker.App/Views/ProfileSelectorView.xaml
- [x] T004 [P] [US1] Update ResultsView header with gradient accent line, icon container, and larger title in src/ReqChecker.App/Views/ResultsView.xaml
- [x] T005 [P] [US1] Update DiagnosticsView header with gradient accent line, icon container, and larger title in src/ReqChecker.App/Views/DiagnosticsView.xaml

**Checkpoint**: All 4 pages have premium header visual treatment. US1 acceptance scenarios pass.

---

## Phase 4: User Story 2 - Contextual Subtitle and Metadata (Priority: P1)

**Goal**: Add subtitles and enhanced count badges to page headers.

**Independent Test**:
1. Navigate to Test Suite - verify "N tests" badge with accent styling
2. Navigate to Profile Manager - verify subtitle "Manage and import test profiles"
3. Navigate to Results Dashboard - verify subtitle "View test execution results"
4. Navigate to System Diagnostics - verify subtitle "System information and logs"

### Implementation for User Story 2

- [x] T006 [P] [US2] Add subtitle "Run diagnostics and verify requirements" and enhanced count badge to TestListView header in src/ReqChecker.App/Views/TestListView.xaml
- [x] T007 [P] [US2] Add subtitle "Manage and import test profiles" to ProfileSelectorView header in src/ReqChecker.App/Views/ProfileSelectorView.xaml
- [x] T008 [P] [US2] Add subtitle "View test execution results" to ResultsView header in src/ReqChecker.App/Views/ResultsView.xaml
- [x] T009 [P] [US2] Add subtitle "System information and logs" to DiagnosticsView header in src/ReqChecker.App/Views/DiagnosticsView.xaml

**Checkpoint**: All 4 pages have contextual subtitles. Test Suite has enhanced count badge. US2 acceptance scenarios pass.

---

## Phase 5: User Story 3 - Animated Entrance Effect (Priority: P2)

**Goal**: Apply AnimatedPageHeader style to all page headers for smooth entrance animation.

**Independent Test**:
1. Navigate between pages
2. Verify header animates in with fade + slide effect
3. Verify animation completes within 300ms

### Implementation for User Story 3

- [x] T010 [P] [US3] Apply AnimatedPageHeader style to TestListView header border in src/ReqChecker.App/Views/TestListView.xaml
- [x] T011 [P] [US3] Apply AnimatedPageHeader style to ProfileSelectorView header border in src/ReqChecker.App/Views/ProfileSelectorView.xaml
- [x] T012 [P] [US3] Apply AnimatedPageHeader style to ResultsView header border in src/ReqChecker.App/Views/ResultsView.xaml
- [x] T013 [P] [US3] Apply AnimatedPageHeader style to DiagnosticsView header border in src/ReqChecker.App/Views/DiagnosticsView.xaml

**Checkpoint**: All 4 page headers animate on entrance. US3 acceptance scenarios pass.

---

## Phase 6: Polish & Verification

**Purpose**: Final verification and theme testing

- [x] T014 [P] Build solution and verify no compiler errors via dotnet build in src/ReqChecker.App
- [x] T015 [P] Verify headers display correctly in light theme
- [x] T016 [P] Verify headers display correctly in dark theme
- [x] T017 Run quickstart.md verification checklist manually

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: N/A - no setup tasks
- **Phase 2 (Foundational)**: Must complete before Phase 5 (animation)
- **Phase 3 (US1)**: Can start immediately - visual treatment only
- **Phase 4 (US2)**: Can run parallel with Phase 3 - subtitles/badges
- **Phase 5 (US3)**: Depends on Phase 2 - applies animation style
- **Phase 6 (Polish)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies - can start immediately
- **User Story 2 (P1)**: No dependencies - can run parallel with US1
- **User Story 3 (P2)**: Depends on T001 (AnimatedPageHeader style)

### Task Dependencies Within Phases

**Phase 3 (US1)**:
- T002, T003, T004, T005 are all independent - different files, can run in parallel

**Phase 4 (US2)**:
- T006, T007, T008, T009 are all independent - different files, can run in parallel

**Phase 5 (US3)**:
- T010, T011, T012, T013 are all independent - different files, can run in parallel
- All depend on T001 (animation style must exist)

### Parallel Opportunities

- **Within US1**: All 4 page updates can run in parallel (different .xaml files)
- **Within US2**: All 4 subtitle additions can run in parallel
- **Within US3**: All 4 animation applications can run in parallel
- **Across stories**: US1 and US2 can run in parallel
- **Polish phase**: T014, T015, T016 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all US1 tasks in parallel (different files):
Task: "Update TestListView header in src/ReqChecker.App/Views/TestListView.xaml"
Task: "Update ProfileSelectorView header in src/ReqChecker.App/Views/ProfileSelectorView.xaml"
Task: "Update ResultsView header in src/ReqChecker.App/Views/ResultsView.xaml"
Task: "Update DiagnosticsView header in src/ReqChecker.App/Views/DiagnosticsView.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Add AnimatedPageHeader style
2. Complete Phase 3: Update all 4 pages with premium visual treatment
3. **STOP and VALIDATE**: Verify gradient lines, icon containers, larger titles
4. All pages look premium = MVP delivered

### Incremental Delivery

1. Foundational → Animation style ready
2. US1 → Premium visual treatment on all pages (MVP!)
3. US2 → Subtitles and enhanced badges (refinement)
4. US3 → Entrance animations (polish)
5. Each story adds perceived quality without breaking previous work

### Recommended Execution Order

For single developer:
1. T001 (animation style)
2. T002-T005 in parallel (US1 - visual treatment)
3. T006-T009 in parallel (US2 - subtitles)
4. T010-T013 in parallel (US3 - animation)
5. T014-T017 (verification)

---

## Notes

- This is a UI-only feature - 17 tasks total
- All changes are in XAML files - no C# code changes
- US1 and US2 can be combined if modifying same file section
- Action buttons remain in same position (right side of header)
- Use DynamicResource for all colors to support theme switching
