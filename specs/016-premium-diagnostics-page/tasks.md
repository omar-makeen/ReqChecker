# Tasks: Premium System Diagnostics Page

**Input**: Design documents from `/specs/016-premium-diagnostics-page/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not required for this UI enhancement (manual verification only).

**Organization**: Tasks are grouped by user story. Since this is a UI-only enhancement with all styles going into one file, tasks are structured to enable incremental verification.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: `src/ReqChecker.App/`
- **Styles**: `src/ReqChecker.App/Resources/Styles/Controls.xaml`
- **Views**: `src/ReqChecker.App/Views/DiagnosticsView.xaml`

---

## Phase 1: Setup (Style Section Preparation)

**Purpose**: Prepare Controls.xaml for new diagnostic card styles

- [x] T001 Add DIAGNOSTICS CARD STYLES section comment after existing CARD STYLES section (~line 332) in `src/ReqChecker.App/Resources/Styles/Controls.xaml`

**Checkpoint**: Controls.xaml has a designated section for diagnostic styles

---

## Phase 2: User Story 1 - View System Diagnostics with Professional Design (Priority: P1) 🎯 MVP

**Goal**: Define all three missing card styles so the Diagnostics page renders with premium styling

**Independent Test**: Navigate to Diagnostics page, verify all cards render without XAML errors and display with professional styling

### Implementation for User Story 1

- [x] T002 [US1] Add DiagnosticCard style extending Card base with 24px padding, 12px corner radius, 16px bottom margin in `src/ReqChecker.App/Resources/Styles/Controls.xaml`
- [x] T003 [US1] Add DiagnosticCardHighlight style extending DiagnosticCard with AccentPrimary 2px border, BackgroundElevated background, enhanced glow effect in `src/ReqChecker.App/Resources/Styles/Controls.xaml`
- [x] T004 [US1] Add NetworkInterfaceCard style with BackgroundElevated background, BorderSubtle 1px border, 16px padding, 8px bottom margin in `src/ReqChecker.App/Resources/Styles/Controls.xaml`
- [x] T005 [US1] Build application with `dotnet build src/ReqChecker.App` to verify no XAML errors
- [x] T006 [US1] Run application and navigate to Diagnostics page to verify cards render correctly

**Checkpoint**: All diagnostic cards render with premium styling - MVP complete

---

## Phase 3: User Story 2 & 3 - Copy Details & Open Logs (Priority: P2)

**Goal**: Verify existing button functionality works with new styling

**Independent Test**: Click "Copy Details" and "Open Logs" buttons, verify they function correctly

**Note**: These features are already implemented in DiagnosticsViewModel. This phase verifies they work correctly with the new card styles.

### Verification for User Stories 2 & 3

- [x] T007 [US2] [US3] Verify "Copy Details" button is styled correctly and copies diagnostic data to clipboard
- [x] T008 [US2] [US3] Verify "Open Logs" button opens File Explorer to logs folder
- [x] T009 [US2] [US3] Verify status messages display correctly in the status banner area

**Checkpoint**: Button functionality verified with new styling

---

## Phase 4: User Story 4 - Visual Consistency Across Themes (Priority: P3)

**Goal**: Verify all card styles work correctly in both dark and light themes

**Independent Test**: Toggle between dark and light themes on the Diagnostics page

### Verification for User Story 4

- [x] T010 [US4] Verify DiagnosticCardHighlight displays correctly in dark theme (accent border visible, proper contrast)
- [x] T011 [US4] Verify DiagnosticCard displays correctly in dark theme
- [x] T012 [US4] Verify NetworkInterfaceCard displays correctly in dark theme
- [x] T013 [US4] Toggle to light theme and verify all cards adapt correctly
- [x] T014 [US4] Verify network interface status indicators (green/red) are distinguishable in both themes

**Checkpoint**: Theme compatibility verified

---

## Phase 5: Polish & Finalize

**Purpose**: Final verification and commit

- [x] T015 Verify card hover effects work for DiagnosticCard and DiagnosticCardHighlight
- [x] T016 Verify entrance animations still work (AnimatedDiagCard style in DiagnosticsView.xaml)
- [x] T017 Run full build verification with `dotnet build src/ReqChecker.App`
- [x] T018 Commit changes with descriptive message
- [x] T019 Push to feature branch

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies - start immediately
- **Phase 2 (US1)**: Depends on Phase 1 - defines all styles (MVP)
- **Phase 3 (US2/US3)**: Depends on Phase 2 - verification only
- **Phase 4 (US4)**: Depends on Phase 2 - theme verification
- **Phase 5 (Polish)**: Depends on Phases 2-4 completion

### Task Dependencies

```
T001 (Section comment)
  ↓
T002 → T003 → T004 (Styles must be added in order: base → highlight → interface)
  ↓
T005 (Build verification)
  ↓
T006 (Runtime verification)
  ↓
T007-T009 (Button verification - parallel)
T010-T014 (Theme verification - parallel)
  ↓
T015-T017 (Final verification)
  ↓
T018 → T019 (Commit and push)
```

### Parallel Opportunities

- T007, T008, T009 can be verified in parallel (different button functions)
- T010, T011, T012 can be verified in parallel (different card types)
- T015, T016 can be verified in parallel (different visual aspects)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Add section comment
2. Complete Phase 2: Add all three styles, build, verify
3. **STOP and VALIDATE**: Diagnostics page renders correctly with premium styling
4. This delivers the core fix (missing styles)

### Full Feature Delivery

1. Phase 1-2: Core styles (MVP)
2. Phase 3: Verify button functionality
3. Phase 4: Theme verification
4. Phase 5: Final polish and commit

### Estimated Scope

- **Total tasks**: 19
- **Style definitions**: 3 (T002-T004)
- **Build/verification**: 4 (T005-T006, T017)
- **Manual testing**: 10 (T007-T016)
- **Commit/push**: 2 (T018-T019)

---

## Notes

- All three styles must be added to the same file (Controls.xaml) in sequence
- Styles use DynamicResource for theme support - no StaticResource for colors
- DiagnosticCard inherits from Card, DiagnosticCardHighlight inherits from DiagnosticCard
- NetworkInterfaceCard is standalone (no inheritance) for simplicity
- The ViewModel and View structure are already correct - only styles are missing
- Commit after all verification passes to ensure the fix is complete
