# Tasks: Premium Button Styles & Consistency

**Input**: Design documents from `/specs/019-premium-buttons/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Manual visual testing only (no automated tests - UI-only feature)

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

All changes target `src/ReqChecker.App/` project:
- Styles: `src/ReqChecker.App/Resources/Styles/Controls.xaml`
- Views: `src/ReqChecker.App/Views/*.xaml`
- Main window: `src/ReqChecker.App/MainWindow.xaml`

---

## Phase 1: Setup (Analysis & Preparation)

**Purpose**: Analyze existing styles and establish baseline

- [x] T001 Review existing button styles in src/ReqChecker.App/Resources/Styles/Controls.xaml to understand current ControlTemplate structure
- [x] T002 [P] Review local FilterTabButton style in src/ReqChecker.App/Views/ResultsView.xaml to document differences from global FilterTab
- [x] T003 [P] Audit all button usages across views to identify icon margin inconsistencies

**Checkpoint**: Baseline established - ready to begin style enhancements

---

## Phase 2: Foundational (Style Infrastructure)

**Purpose**: Add core infrastructure to Controls.xaml that all button styles will use

**CRITICAL**: These changes establish the foundation for all user story implementations

- [x] T004 Add TextDisabled brush resource to Controls.xaml if not already present (for disabled state foreground)
- [x] T005 Verify AccentPrimary resource exists for focus ring color in Controls.xaml

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 & 2 - Visual Hierarchy & Interaction Feedback (Priority: P1) MVP

**Goal**: Establish consistent button styles with smooth hover/press transitions across all button types

**Independent Test**: Navigate through all pages, verify primary/secondary/ghost buttons have distinct visual hierarchy and smooth hover transitions

### Implementation for User Stories 1 & 2

- [x] T006 [US1][US2] Update PrimaryButton style with VisualStateManager transitions (150ms hover, instant press) in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T007 [P] [US1][US2] Update SecondaryButton style with VisualStateManager transitions in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T008 [P] [US1][US2] Update GhostButton style with VisualStateManager transitions in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T009 [P] [US1][US2] Update IconButton style with VisualStateManager transitions in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T010 [US1][US2] Update PrimaryButtonSmall and PrimaryButtonLarge to inherit transitions from base PrimaryButton in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T011 [P] [US1][US2] Update SecondaryButtonSmall to inherit transitions from base SecondaryButton in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T012 [P] [US1][US2] Update GhostButtonSmall to inherit transitions from base GhostButton in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T013 [US1] Build and manually verify hover transitions are smooth on Test Suite page buttons

**Checkpoint**: All buttons have consistent visual hierarchy and smooth 150ms hover transitions

---

## Phase 4: User Story 3 - Keyboard Accessibility (Priority: P2)

**Goal**: Add visible focus indicators for keyboard navigation

**Independent Test**: Tab through all buttons on any page, verify focus ring appears on each focused button in both themes

### Implementation for User Story 3

- [x] T014 [US3] Add IsKeyboardFocused trigger with FocusRing element to PrimaryButton ControlTemplate in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T015 [P] [US3] Add IsKeyboardFocused trigger with FocusRing to SecondaryButton ControlTemplate in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T016 [P] [US3] Add IsKeyboardFocused trigger with FocusRing to GhostButton ControlTemplate in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T017 [P] [US3] Add IsKeyboardFocused trigger with FocusRing to IconButton ControlTemplate in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T018 [US3] Verify focus ring visibility in light theme via manual testing
- [x] T019 [US3] Verify focus ring visibility in dark theme via manual testing

**Checkpoint**: All buttons show visible focus indicators when tabbed to via keyboard

---

## Phase 5: User Story 4 - Disabled State Clarity (Priority: P2)

**Goal**: Enhance disabled button visibility with opacity, color, and cursor changes

**Independent Test**: View disabled "Run All Tests" button (when no profile loaded), verify opacity reduction, muted color, and not-allowed cursor

### Implementation for User Story 4

- [x] T020 [US4] Update PrimaryButton disabled trigger to add Cursor="No" and Foreground=TextDisabled in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T021 [P] [US4] Update SecondaryButton disabled trigger with enhanced visibility in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T022 [P] [US4] Update GhostButton disabled trigger with enhanced visibility in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T023 [P] [US4] Update IconButton disabled trigger with enhanced visibility in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T024 [US4] Verify disabled buttons suppress hover/focus states in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T025 [US4] Manual test: Verify disabled buttons show not-allowed cursor on hover

**Checkpoint**: All disabled buttons are clearly distinguishable with multiple visual cues

---

## Phase 6: User Story 5 - Consistent Icon-Text Spacing (Priority: P3)

**Goal**: Standardize 8px spacing between icons and text in all buttons

**Independent Test**: Compare all icon-text buttons across pages, verify spacing is identical (8px)

### Implementation for User Story 5

- [x] T026 [P] [US5] Standardize icon margins in src/ReqChecker.App/Views/TestListView.xaml (ensure Margin="0,0,8,0")
- [x] T027 [P] [US5] Standardize icon margins in src/ReqChecker.App/Views/ResultsView.xaml
- [x] T028 [P] [US5] Standardize icon margins in src/ReqChecker.App/Views/ProfileSelectorView.xaml
- [x] T029 [P] [US5] Standardize icon margins in src/ReqChecker.App/Views/DiagnosticsView.xaml
- [x] T030 [P] [US5] Standardize icon margins in src/ReqChecker.App/Views/TestConfigView.xaml
- [x] T031 [P] [US5] Standardize icon margins in src/ReqChecker.App/Views/RunProgressView.xaml

**Checkpoint**: All icon-text buttons have consistent 8px spacing

---

## Phase 7: Style Consolidation & Button Width Fixes

**Purpose**: Remove duplicate styles and fix responsive width issues

- [x] T032 Remove local FilterTabButton style definition from src/ReqChecker.App/Views/ResultsView.xaml
- [x] T033 Update filter RadioButtons in ResultsView to use global FilterTab style in src/ReqChecker.App/Views/ResultsView.xaml
- [x] T034 [P] Enhance global FilterTab style with any missing properties from local FilterTabButton in src/ReqChecker.App/Resources/Styles/Controls.xaml
- [x] T035 [P] Remove fixed Width="100px" from buttons in src/ReqChecker.App/Views/CredentialPromptDialog.xaml, replace with MinWidth

**Checkpoint**: No duplicate styles, all buttons use responsive widths

---

## Phase 8: Polish & Verification

**Purpose**: Final validation and cross-cutting fixes

- [x] T036 Run `dotnet build src/ReqChecker.App` to verify no compilation errors
- [x] T037 Manual visual test: Verify hover transitions on all button types across all pages
- [x] T038 Manual visual test: Verify press feedback (0.98 scale) on button clicks
- [x] T039 Manual visual test: Verify focus ring visibility via Tab navigation
- [x] T040 Manual visual test: Verify disabled state appearance and cursor
- [x] T041 Manual visual test: Verify FilterTab styling in ResultsView matches global style
- [x] T042 Manual visual test: Verify icon-text spacing consistency across all pages

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion
- **US1 & US2 (Phase 3)**: Depends on Foundational - Core MVP functionality
- **US3 (Phase 4)**: Depends on Phase 3 (needs ControlTemplate structure in place)
- **US4 (Phase 5)**: Depends on Phase 3 (needs ControlTemplate structure in place)
- **US5 (Phase 6)**: Can run in parallel with Phases 4 & 5 (different files)
- **Consolidation (Phase 7)**: Can run in parallel with Phases 4-6
- **Polish (Phase 8)**: Depends on all previous phases

### User Story Dependencies

- **User Stories 1 & 2 (P1)**: Combined as MVP - smooth transitions establish premium feel
- **User Story 3 (P2)**: Can start after US1/US2 - needs ControlTemplate changes
- **User Story 4 (P2)**: Can start after US1/US2 - needs ControlTemplate changes
- **User Story 5 (P3)**: Independent - different files (view XAMLs vs Controls.xaml)

### Parallel Opportunities

- T002, T003 can run in parallel (different analysis tasks)
- T007, T008, T009 can run in parallel (different button styles, same file but different sections)
- T015, T016, T017 can run in parallel (different button styles)
- T021, T022, T023 can run in parallel (different button styles)
- T026-T031 can ALL run in parallel (different view files)
- T034, T035 can run in parallel (different files)

---

## Parallel Example: User Story 5 (Icon Spacing)

```bash
# All these tasks can run simultaneously (different files):
Task: "Standardize icon margins in TestListView.xaml"
Task: "Standardize icon margins in ResultsView.xaml"
Task: "Standardize icon margins in ProfileSelectorView.xaml"
Task: "Standardize icon margins in DiagnosticsView.xaml"
Task: "Standardize icon margins in TestConfigView.xaml"
Task: "Standardize icon margins in RunProgressView.xaml"
```

---

## Implementation Strategy

### MVP First (User Stories 1 & 2)

1. Complete Phase 1: Setup (analyze existing)
2. Complete Phase 2: Foundational (add resources)
3. Complete Phase 3: User Stories 1 & 2 (hover transitions)
4. **STOP and VALIDATE**: Test smooth transitions on all pages
5. This delivers the core "premium feel" improvement

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 & US2 → Test transitions → Premium hover feel achieved (MVP!)
3. Add US3 → Test keyboard focus → Accessibility improved
4. Add US4 → Test disabled states → Full state visibility
5. Add US5 + Consolidation → Test spacing & styles → Polished consistency
6. Polish → Final validation → Feature complete

---

## Notes

- [P] tasks = different files or independent sections, no dependencies
- [Story] label maps task to specific user story for traceability
- US1 and US2 are combined because they both modify the same ControlTemplates
- All style changes are in Controls.xaml; spacing changes are in individual view files
- Build verification (T036) should run after each phase for safety
- Manual visual testing is required as this is a UI-only feature
