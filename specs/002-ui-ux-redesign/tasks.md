# Tasks: UI/UX Premium Redesign

**Input**: Design documents from `/specs/002-ui-ux-redesign/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested - test tasks included only for critical services (PreferencesService, ThemeService).

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md, paths are:
- **App source**: `src/ReqChecker.App/`
- **Tests**: `tests/ReqChecker.App.Tests/`

---

## Phase 1: Setup (Theme Infrastructure)

**Purpose**: Create the foundational design system and resource dictionaries

- [X] T001 [P] Create dark theme color tokens in src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml
- [X] T002 [P] Create light theme color tokens in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [X] T003 [P] Create typography styles in src/ReqChecker.App/Resources/Styles/Typography.xaml
- [X] T004 [P] Create spacing values in src/ReqChecker.App/Resources/Styles/Spacing.xaml
- [X] T005 Create PreferencesService for user settings persistence in src/ReqChecker.App/Services/PreferencesService.cs
- [X] T006 Create IPreferencesService interface in src/ReqChecker.App/Services/IPreferencesService.cs
- [X] T007 Add unit tests for PreferencesService in tests/ReqChecker.App.Tests/Services/PreferencesServiceTests.cs
- [X] T008 Enhance ThemeService with persistence and new resource dictionaries in src/ReqChecker.App/Services/ThemeService.cs
- [X] T009 Add unit tests for ThemeService in tests/ReqChecker.App.Tests/Services/ThemeServiceTests.cs
- [X] T010 Update App.xaml to merge new resource dictionaries in src/ReqChecker.App/App.xaml
- [X] T011 Register PreferencesService in DI container in src/ReqChecker.App/App.xaml.cs

**Checkpoint**: Theme system complete - colors, typography, spacing resources available, theme persists

---

## Phase 2: Foundational (Main Window Shell)

**Purpose**: Convert MainWindow to FluentWindow with sidebar navigation - BLOCKS all view redesigns

**⚠️ CRITICAL**: No view redesign work can begin until this phase is complete

- [ ] T012 Convert MainWindow to FluentWindow base class in src/ReqChecker.App/MainWindow.xaml
- [ ] T013 Update MainWindow.xaml.cs to inherit from FluentWindow in src/ReqChecker.App/MainWindow.xaml.cs
- [ ] T014 Replace header navigation with NavigationView sidebar in src/ReqChecker.App/MainWindow.xaml
- [ ] T015 Add IsSidebarExpanded property to MainWindowViewModel in src/ReqChecker.App/ViewModels/MainWindowViewModel.cs
- [ ] T016 Implement sidebar collapse/expand toggle with persistence in src/ReqChecker.App/MainWindow.xaml
- [ ] T017 Configure Mica backdrop for title bar with Windows 10 fallback in src/ReqChecker.App/MainWindow.xaml
- [ ] T018 Add status bar with version, profile name, theme toggle in src/ReqChecker.App/MainWindow.xaml
- [ ] T019 Create view transition animations in src/ReqChecker.App/Resources/Styles/Animations.xaml
- [ ] T020 Implement frame navigation with fade transitions in src/ReqChecker.App/MainWindow.xaml.cs

**Checkpoint**: Sidebar navigation functional, collapse/expand works, state persists, Mica visible on Windows 11

---

## Phase 3: User Story 1 - First Impressions (Priority: P1) 🎯 MVP

**Goal**: Create striking dark interface with premium appearance on first launch

**Independent Test**: Launch app → observe fade-in animation, dark theme with glass effects, professional appearance

### Implementation for User Story 1

- [ ] T021 [P] [US1] Create button styles (Primary gradient, Secondary, Ghost) in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [ ] T022 [P] [US1] Create card base style with elevation in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [ ] T023 [P] [US1] Add hover animations for cards in src/ReqChecker.App/Resources/Styles/Animations.xaml
- [ ] T024 [US1] Add window fade-in animation on launch in src/ReqChecker.App/MainWindow.xaml
- [ ] T025 [US1] Implement reduced motion detection in src/ReqChecker.App/Services/ThemeService.cs
- [ ] T026 [US1] Add conditional animation logic based on system settings in src/ReqChecker.App/Resources/Styles/Animations.xaml

**Checkpoint**: App launches with smooth fade-in, dark theme looks premium, Mica effect visible

---

## Phase 4: User Story 2 - Seamless Navigation (Priority: P1)

**Goal**: Sidebar with animated transitions and clear visual feedback

**Independent Test**: Click each nav item → observe smooth transitions, hover effects, active indicator

### Implementation for User Story 2

- [ ] T027 [P] [US2] Style NavigationView items with icons in src/ReqChecker.App/MainWindow.xaml
- [ ] T028 [P] [US2] Add active item accent border indicator (3px left border) in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [ ] T029 [US2] Add hover effect with background highlight in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [ ] T030 [US2] Implement 200ms slide/fade view transitions in src/ReqChecker.App/Resources/Styles/Animations.xaml
- [ ] T031 [US2] Add navigation item tooltips when sidebar collapsed in src/ReqChecker.App/MainWindow.xaml

**Checkpoint**: Navigation feels smooth and responsive, active state clear, hover feedback visible

---

## Phase 5: User Story 3 - Engaging Test Execution (Priority: P1)

**Goal**: Progress ring with animations, pulse effects on test completion

**Independent Test**: Run tests → observe animated progress ring, pulse on each test completion, staggered results

### Implementation for User Story 3

- [ ] T032 [P] [US3] Enhance ProgressRing with gradient stroke in src/ReqChecker.App/Controls/ProgressRing.xaml
- [ ] T033 [P] [US3] Add ProgressRing.xaml.cs code-behind for gradient animation in src/ReqChecker.App/Controls/ProgressRing.xaml.cs
- [ ] T034 [US3] Add pulse animation for test completion in src/ReqChecker.App/Resources/Styles/Animations.xaml
- [ ] T035 [US3] Enhance TestStatusBadge with glow effects in src/ReqChecker.App/Controls/TestStatusBadge.xaml
- [ ] T036 [US3] Redesign RunProgressView with centered progress ring in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T037 [US3] Add current test card below progress ring in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T038 [US3] Add completed tests mini-list with scroll in src/ReqChecker.App/Views/RunProgressView.xaml
- [ ] T039 [US3] Add staggered entrance animations for test items in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: Test execution visually engaging, progress ring animates smoothly, pulse on completion

---

## Phase 6: User Story 4 - Delightful Results Visualization (Priority: P2)

**Goal**: Results with summary dashboard, expandable cards, filter tabs

**Independent Test**: View results → see animated stats, expand/collapse cards, filter by status

### Implementation for User Story 4

- [ ] T040 [P] [US4] Create SummaryCard control in src/ReqChecker.App/Controls/SummaryCard.xaml
- [ ] T041 [P] [US4] Create SummaryCard.xaml.cs code-behind in src/ReqChecker.App/Controls/SummaryCard.xaml.cs
- [ ] T042 [P] [US4] Create ExpanderCard control in src/ReqChecker.App/Controls/ExpanderCard.xaml
- [ ] T043 [P] [US4] Create ExpanderCard.xaml.cs code-behind in src/ReqChecker.App/Controls/ExpanderCard.xaml.cs
- [ ] T044 [US4] Create donut chart component for pass/fail/skip ratio in src/ReqChecker.App/Controls/DonutChart.xaml
- [ ] T045 [US4] Add DonutChart.xaml.cs with draw-in animation in src/ReqChecker.App/Controls/DonutChart.xaml.cs
- [ ] T046 [US4] Redesign ResultsView with summary dashboard at top in src/ReqChecker.App/Views/ResultsView.xaml
- [ ] T047 [US4] Add filter tabs (All/Passed/Failed/Skipped) to ResultsView in src/ReqChecker.App/Views/ResultsView.xaml
- [ ] T048 [US4] Add ResultsFilter enum and filtering logic to ResultsViewModel in src/ReqChecker.App/ViewModels/ResultsViewModel.cs
- [ ] T049 [US4] Replace result cards with ExpanderCard components in src/ReqChecker.App/Views/ResultsView.xaml
- [ ] T050 [US4] Style export buttons with icons in src/ReqChecker.App/Views/ResultsView.xaml

**Checkpoint**: Results view shows animated dashboard, cards expand/collapse smoothly, filtering works

---

## Phase 7: User Story 5 - Professional Theme System (Priority: P2)

**Goal**: Smooth theme transitions, WCAG AA compliance, persistence

**Independent Test**: Toggle theme → observe cross-fade animation, verify contrast, restart app → theme restored

### Implementation for User Story 5

- [ ] T051 [US5] Implement 300ms cross-fade theme transition in src/ReqChecker.App/Services/ThemeService.cs
- [ ] T052 [US5] Verify and adjust WCAG AA contrast for all text colors in src/ReqChecker.App/Resources/Styles/Colors.Dark.xaml
- [ ] T053 [US5] Verify and adjust WCAG AA contrast for light theme in src/ReqChecker.App/Resources/Styles/Colors.Light.xaml
- [ ] T054 [US5] Add theme toggle button styling in status bar in src/ReqChecker.App/MainWindow.xaml
- [ ] T055 [US5] Ensure all DynamicResource references for theme-aware colors in src/ReqChecker.App/Resources/Styles/Controls.xaml

**Checkpoint**: Theme switch is smooth, both themes accessible, preference persists

---

## Phase 8: Remaining Views Redesign

**Purpose**: Apply design system to all remaining views

### Profile Selector View

- [ ] T056 [P] Redesign ProfileSelectorView with large profile cards in src/ReqChecker.App/Views/ProfileSelectorView.xaml
- [ ] T057 [P] Add gradient header strip to profile cards in src/ReqChecker.App/Views/ProfileSelectorView.xaml
- [ ] T058 Add empty state with Fluent icon composition in src/ReqChecker.App/Views/ProfileSelectorView.xaml
- [ ] T059 Add drag-drop styling for import button in src/ReqChecker.App/Views/ProfileSelectorView.xaml

### Test List View

- [ ] T060 [P] Redesign TestListView with interactive cards in src/ReqChecker.App/Views/TestListView.xaml
- [ ] T061 [P] Add test type icons to test cards in src/ReqChecker.App/Views/TestListView.xaml
- [ ] T062 Style Run All Tests button with gradient accent in src/ReqChecker.App/Views/TestListView.xaml
- [ ] T063 Add staggered entrance animations for test cards in src/ReqChecker.App/Views/TestListView.xaml

### Test Config View

- [ ] T064 [P] Redesign TestConfigView with organized parameter sections in src/ReqChecker.App/Views/TestConfigView.xaml
- [ ] T065 [P] Update LockedFieldControl with lock icon and reduced opacity in src/ReqChecker.App/Controls/LockedFieldControl.xaml
- [ ] T066 Add floating label input styling in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [ ] T067 Add inline validation error animations in src/ReqChecker.App/Views/TestConfigView.xaml

### Diagnostics View

- [ ] T068 [P] Redesign DiagnosticsView with key-value card format in src/ReqChecker.App/Views/DiagnosticsView.xaml
- [ ] T069 Add visual status indicators for last run summary in src/ReqChecker.App/Views/DiagnosticsView.xaml
- [ ] T070 Style Copy/Open Logs action buttons in src/ReqChecker.App/Views/DiagnosticsView.xaml

### Credential Prompt Dialog

- [ ] T071 Restyle CredentialPromptDialog with design system in src/ReqChecker.App/Views/CredentialPromptDialog.xaml

**Checkpoint**: All views consistent with new design system

---

## Phase 9: Accessibility & Polish

**Purpose**: Ensure accessibility compliance and final polish

- [ ] T072 [P] Implement custom focus ring style (accent-colored outline) in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [ ] T073 Verify keyboard navigation for all interactive elements in all Views
- [ ] T074 Add screen reader announcements for navigation changes in src/ReqChecker.App/MainWindow.xaml.cs
- [ ] T075 Add screen reader announcements for status updates in src/ReqChecker.App/Views/RunProgressView.xaml.cs
- [ ] T076 [P] Add virtualization to test list and results list in src/ReqChecker.App/Views/TestListView.xaml
- [ ] T077 [P] Add virtualization to results list in src/ReqChecker.App/Views/ResultsView.xaml
- [ ] T078 Test reduced motion setting disables decorative animations
- [ ] T079 Test high DPI rendering (100%, 125%, 150%, 200%)
- [ ] T080 Test Windows 10 fallback (no Mica, solid colors)
- [ ] T081 Performance profiling for 60fps animations
- [ ] T082 Final visual review against mockups in specs/002-ui-ux-redesign/visual-mockups.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 (First Impressions): Can start immediately after Phase 2
  - US2 (Navigation): Can start immediately after Phase 2
  - US3 (Test Execution): Can start immediately after Phase 2
  - US4 (Results): Can start immediately after Phase 2
  - US5 (Theme): Can start immediately after Phase 2
- **Remaining Views (Phase 8)**: Depends on Phase 3 (core components)
- **Polish (Phase 9)**: Depends on all views being complete

### User Story Dependencies

All P1/P2 user stories are **independent** and can be worked in parallel once Phase 2 is complete:

```
Phase 1 (Setup) ────────────────────────────────────────────────────────────┐
         │                                                                   │
         v                                                                   │
Phase 2 (Foundational) ─────────────────────────────────────────────────────┤
         │                                                                   │
         ├──────────────────┬──────────────────┬──────────────────┐          │
         v                  v                  v                  v          │
   US1: First         US2: Navigation    US3: Test          US4: Results    │
   Impressions                           Execution                           │
         │                  │                  │                  │          │
         └──────────────────┴──────────────────┴──────────────────┘          │
                                    │                                        │
                                    v                                        │
                           US5: Theme System                                 │
                                    │                                        │
                                    v                                        │
                      Phase 8: Remaining Views                               │
                                    │                                        │
                                    v                                        │
                      Phase 9: Accessibility & Polish ───────────────────────┘
```

### Parallel Opportunities

Within Phase 1 (Setup):
- T001, T002, T003, T004 can all run in parallel (different resource files)

Within Phase 3 (US1):
- T021, T022, T023 can run in parallel (different style sections)

Within Phase 5 (US3):
- T032, T033 can run in parallel with T034, T035 (different controls)

Within Phase 6 (US4):
- T040, T041, T042, T043 can all run in parallel (new controls)

Within Phase 8 (Remaining Views):
- All view redesigns can run in parallel (different files)

---

## Parallel Example: User Story 1

```bash
# Launch all parallel tasks for User Story 1:
Task: "Create button styles in src/ReqChecker.App/Resources/Styles/Controls.xaml" [T021]
Task: "Create card base style in src/ReqChecker.App/Resources/Styles/Controls.xaml" [T022]
Task: "Add hover animations in src/ReqChecker.App/Resources/Styles/Animations.xaml" [T023]

# Then sequential tasks:
Task: "Add window fade-in animation" [T024]
Task: "Implement reduced motion detection" [T025]
Task: "Add conditional animation logic" [T026]
```

---

## Implementation Strategy

### MVP First (User Stories 1-3)

1. Complete Phase 1: Setup (theme infrastructure)
2. Complete Phase 2: Foundational (FluentWindow shell)
3. Complete Phase 3: User Story 1 (First Impressions)
4. Complete Phase 4: User Story 2 (Navigation)
5. Complete Phase 5: User Story 3 (Test Execution)
6. **STOP and VALIDATE**: Test US1-3 independently - core experience complete
7. Deploy/demo if ready

### Incremental Delivery

1. **Foundation**: Setup + Foundational → Shell with navigation working
2. **MVP+**: Add US1-3 → Premium first impressions, navigation, test execution
3. **Enhanced**: Add US4-5 → Results visualization, theme polish
4. **Complete**: Remaining views + Accessibility → Full redesign
5. Each increment adds visual value without breaking previous work

### Single Developer Strategy

Execute phases sequentially:
1. Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6 → Phase 7 → Phase 8 → Phase 9

### Parallel Team Strategy (2-3 developers)

After Phase 2 completes:
- Developer A: US1 + US2 (First Impressions + Navigation)
- Developer B: US3 + US4 (Test Execution + Results)
- Developer C: US5 + Remaining Views

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently testable after completion
- Commit after each task or logical group
- Test theme switching frequently during development
- Verify animations with and without reduced motion enabled
- Test on both Windows 10 and Windows 11 periodically
