# Tasks: Premium Test Execution Page

**Input**: Design documents from `/specs/020-premium-test-execution/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Manual visual verification only (UI-only feature - no automated tests)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Project structure**: `src/ReqChecker.App/` (WPF application)
- Views: `src/ReqChecker.App/Views/`
- ViewModels: `src/ReqChecker.App/ViewModels/`
- Converters: `src/ReqChecker.App/Converters/`
- Styles: `src/ReqChecker.App/Resources/Styles/`

---

## Phase 1: Setup (Verification)

**Purpose**: Verify existing infrastructure is ready for feature implementation

- [ ] T001 Verify AnimatedPageHeader style exists in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [ ] T002 Verify TestStatusToColorConverter is registered in src/ReqChecker.App/App.xaml
- [ ] T003 Verify TestStatusBadge control exists in src/ReqChecker.App/Controls/TestStatusBadge.xaml

---

## Phase 2: Foundational (ViewModel Support)

**Purpose**: Add ViewModel property that all user stories depend on

**⚠️ CRITICAL**: Header subtitle binding required before UI work can begin

- [ ] T004 Add HeaderSubtitle computed property to src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [ ] T005 Add OnPropertyChanged notifications for HeaderSubtitle in relevant partial methods in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs

**Checkpoint**: ViewModel ready - UI implementation can now begin

---

## Phase 3: User Story 1 - Consistent Premium Header (Priority: P1) 🎯 MVP

**Goal**: Replace the basic header with premium AnimatedPageHeader pattern matching other pages

**Independent Test**: Navigate to Test Execution page and visually compare header to Test Suite page - should have identical styling approach (gradient line, icon container, typography)

### Implementation for User Story 1

- [ ] T006 [US1] Remove existing basic header (StackPanel + icon + text) from src/ReqChecker.App/Views/RunProgressView.xaml lines 52-67
- [ ] T007 [US1] Add AnimatedPageHeader Border wrapper with style reference in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T008 [US1] Add gradient accent line (4px height Border) at top of header in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T009 [US1] Add icon container (48x48 Border with AccentPrimary background, CornerRadius=12) in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T010 [US1] Add Play24 SymbolIcon centered in icon container in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T011 [US1] Add title TextBlock "Test Execution" (24px, SemiBold) in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T012 [US1] Add subtitle TextBlock bound to HeaderSubtitle property in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T013 [US1] Preserve Cancel and navigation buttons in header right column in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: Header visually matches other pages with entrance animation

---

## Phase 4: User Story 2 - Visual Test Status Indicators (Priority: P1)

**Goal**: Ensure TestStatusBadge displays correctly on each completed test card

**Independent Test**: Run tests and verify each completed test shows a colored badge with icon (green checkmark for Pass, red X for Fail, amber for Skip)

### Implementation for User Story 2

- [ ] T014 [US2] Verify TestStatusBadge binding path is correct (Status="{Binding Status}") in src/ReqChecker.App/Views/RunProgressView.xaml line 344-347
- [ ] T015 [US2] Ensure TestStatusBadge has proper sizing and alignment (remove any constraints that might hide it) in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T016 [US2] Add HorizontalAlignment="Right" to TestStatusBadge if not present in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T017 [US2] Verify test result Grid column widths allow badge to display (Column 1 should be Width="Auto") in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: Status badges visible on all completed test cards

---

## Phase 5: User Story 3 - Animated Test Results List (Priority: P2)

**Goal**: Verify and refine entrance animations for test result items

**Independent Test**: Run tests and observe each completed test slides in from right with fade effect within 300ms

### Implementation for User Story 3

- [ ] T018 [US3] Verify AnimatedTestResultItem style is correctly applied to result card Border in src/ReqChecker.App/Views/RunProgressView.xaml line 316
- [ ] T019 [US3] Confirm animation duration is 300ms with CubicEase in AnimatedTestResultItem style in src/ReqChecker.App/Views/RunProgressView.xaml lines 14-43
- [ ] T020 [US3] Test animation performance with virtualized ItemsControl in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: Test results animate smoothly when added to list

---

## Phase 6: User Story 4 - Status-Colored Test Cards (Priority: P2)

**Goal**: Add colored left border to test result cards based on status

**Independent Test**: Run tests with mixed results (pass/fail/skip) and verify cards have colored left borders (green/red/amber)

### Implementation for User Story 4

- [ ] T021 [P] [US4] Create TestStatusToBorderBrushConverter in src/ReqChecker.App/Converters/TestStatusToBorderBrushConverter.cs
- [ ] T022 [US4] Register TestStatusToBorderBrushConverter in src/ReqChecker.App/App.xaml (if new converter created) OR use existing converter
- [ ] T023 [US4] Add BorderThickness="4,0,0,0" to test result card Border in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T024 [US4] Bind BorderBrush to Status using converter in test result card in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T025 [US4] Add subtle background tint DataTriggers based on Status in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: Test result cards have colored left borders matching status

---

## Phase 7: Polish & Verification

**Purpose**: Final validation and cleanup

- [ ] T026 Build and run application to verify all changes compile (dotnet build src/ReqChecker.App)
- [ ] T027 Run quickstart.md validation checklist against running application
- [ ] T028 Compare Test Execution page header side-by-side with Test Suite page
- [ ] T029 Verify header subtitle updates dynamically during test execution
- [ ] T030 Test with mixed pass/fail/skip results to verify all status colors display correctly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - verification only
- **Foundational (Phase 2)**: No dependencies - can run after/parallel with Phase 1
- **User Story 1 (Phase 3)**: Depends on Phase 2 (HeaderSubtitle property)
- **User Story 2 (Phase 4)**: Can start after Phase 1 - independent of Phase 3
- **User Story 3 (Phase 5)**: Can start after Phase 1 - independent of Phases 3-4
- **User Story 4 (Phase 6)**: Can start after Phase 1 - independent of Phases 3-5
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational phase - Header needs HeaderSubtitle binding
- **User Story 2 (P1)**: No dependencies - can run in parallel with US1
- **User Story 3 (P2)**: No dependencies - can run in parallel with US1, US2
- **User Story 4 (P2)**: No dependencies - can run in parallel with US1, US2, US3

### Within Each User Story

- Tasks within a story are sequential (same file modifications)
- US1: T006 → T007 → T008 → T009 → T010 → T011 → T012 → T013
- US2: T014 → T015 → T016 → T017
- US3: T018 → T019 → T020
- US4: T021 (converter) → T022 (register) → T023 → T024 → T025

### Parallel Opportunities

- After Phase 2 (Foundational) completes:
  - US1 can begin (header work)
  - US2 can begin in parallel (badge verification)
  - US3 can begin in parallel (animation verification)
  - US4 can begin T021 (new converter) in parallel with other stories

---

## Parallel Example: After Foundational Phase

```bash
# These can run in parallel (different concerns):
Developer A: T006-T013 (User Story 1 - Header)
Developer B: T014-T017 (User Story 2 - Badge visibility)
Developer C: T018-T020 (User Story 3 - Animations)
Developer D: T021 (User Story 4 - Converter creation)

# Then converge for US4 XAML changes after converter is ready
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (verify infrastructure)
2. Complete Phase 2: Foundational (add HeaderSubtitle)
3. Complete Phase 3: User Story 1 (premium header)
4. Complete Phase 4: User Story 2 (status badges)
5. **STOP and VALIDATE**: Test Execution page now has consistent header and visible status indicators
6. This MVP delivers the two P1 user stories

### Full Delivery

1. MVP (US1 + US2) → Visual consistency achieved
2. Add User Story 3 → Animations verified/refined
3. Add User Story 4 → Colored borders add extra polish
4. Phase 7: Final verification

### Single Developer Strategy

Recommended order for single developer:
1. T001-T003 (Setup)
2. T004-T005 (Foundational)
3. T006-T013 (US1 - header first, most visible change)
4. T014-T017 (US2 - badge fixes)
5. T021-T025 (US4 - colored borders)
6. T018-T020 (US3 - animation verification)
7. T026-T030 (Polish)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All changes are in RunProgressView.xaml except converter and ViewModel
- Manual visual testing required - no automated tests for UI-only feature
- Reference TestListView.xaml lines 64-120 for header pattern
- Commit after completing each user story phase
