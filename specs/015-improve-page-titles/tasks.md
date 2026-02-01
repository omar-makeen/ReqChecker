# Tasks: Improve Page Titles and Icons

**Input**: Design documents from `/specs/015-improve-page-titles/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not required for this UI enhancement (manual verification only).

**Organization**: Tasks are grouped by user story. US1 and US2 are both P1 priority and can be done together. US3 and US4 are lower priority enhancements.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: `src/ReqChecker.App/`
- **Views**: `src/ReqChecker.App/Views/`
- **MainWindow**: `src/ReqChecker.App/MainWindow.xaml`

---

## Phase 1: User Story 1 & 2 - Premium Navigation & Consistent Headers (Priority: P1) 🎯 MVP

**Goal**: Update navigation sidebar with premium icons/labels and ensure page headers match

**Independent Test**: Open application, verify navigation icons/labels are updated, navigate to each page and verify header matches navigation

**⚠️ NOTE**: US1 and US2 share MainWindow.xaml changes, so they're combined in this phase

### Implementation for User Story 1 & 2

- [x] T001 [US1] [US2] Update window title bar icon from CheckmarkCircle24 to ShieldCheckmark24 in `src/ReqChecker.App/MainWindow.xaml` (line 36)
- [x] T002 [US1] [US2] Update Profiles navigation item: Icon to Folder24, Content to "Profile Manager", Tooltip to "Manage test profiles" in `src/ReqChecker.App/MainWindow.xaml` (lines 65-77)
- [x] T003 [US1] [US2] Update Tests navigation item: Icon to ClipboardTaskList24, Content to "Test Suite", keep tooltip in `src/ReqChecker.App/MainWindow.xaml` (lines 78-90)
- [x] T004 [US1] [US2] Update Results navigation item: Icon to Poll24, Content to "Results Dashboard", keep tooltip in `src/ReqChecker.App/MainWindow.xaml` (lines 91-103)
- [x] T005 [US1] [US2] Update Diagnostics navigation item: Icon to HeartPulse24, Content to "System Diagnostics", Tooltip to "View system diagnostics" in `src/ReqChecker.App/MainWindow.xaml` (lines 104-116)
- [x] T006 [P] [US2] Update ProfileSelectorView header: Icon to Folder24, Title to "Profile Manager" in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`
- [x] T007 [P] [US2] Update TestListView header: Icon to ClipboardTaskList24, Title to "Test Suite" in `src/ReqChecker.App/Views/TestListView.xaml`
- [x] T008 [P] [US2] Update ResultsView header: Icon to Poll24, Title to "Results Dashboard" in `src/ReqChecker.App/Views/ResultsView.xaml`
- [x] T009 [P] [US2] Update DiagnosticsView header: Icon to HeartPulse24, Title to "System Diagnostics" in `src/ReqChecker.App/Views/DiagnosticsView.xaml`
- [x] T010 [P] [US2] Update TestConfigView header icon to SettingsCog24 in `src/ReqChecker.App/Views/TestConfigView.xaml`
- [x] T011 [P] [US2] Update RunProgressView header icon to PlayCircle24 in `src/ReqChecker.App/Views/RunProgressView.xaml`

**Checkpoint**: Navigation sidebar and all page headers display premium icons and professional titles

---

## Phase 2: User Story 3 - Refined Window Branding (Priority: P2)

**Goal**: Ensure window branding is professional in taskbar and window switcher

**Independent Test**: Launch application, check taskbar icon, use Alt+Tab to verify distinguishable icon

### Implementation for User Story 3

- [ ] T012 [US3] Verify ShieldCheckmark24 displays correctly in window title bar (already done in T001)
- [ ] T013 [US3] Verify application icon is visible and distinguishable in Windows taskbar

**Checkpoint**: Application is easily identifiable in taskbar and Alt+Tab window switcher

---

## Phase 3: User Story 4 - Professional Empty States (Priority: P3)

**Goal**: Update empty state icons to match page themes

**Independent Test**: View each page with no data and verify empty state icons are contextually appropriate

### Implementation for User Story 4

- [x] T014 [P] [US4] Update TestListView empty state icon to ClipboardTaskList24 (if present) in `src/ReqChecker.App/Views/TestListView.xaml`
- [x] T015 [P] [US4] Update ResultsView empty state icon to Poll24 in `src/ReqChecker.App/Views/ResultsView.xaml`
- [x] T016 [P] [US4] Update ProfileSelectorView empty state icon to Folder24 (if present) in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`

**Checkpoint**: Empty states display professional, contextually appropriate icons

---

## Phase 4: Verification & Polish

**Purpose**: Build, verify all acceptance criteria, ensure theme support

### Build & Test

- [x] T017 Build application with `dotnet build src/ReqChecker.App`
- [ ] T018 Run application and verify all acceptance criteria from spec.md (requires manual testing)

### Verification Checklist - US1 (Navigation Icons) - Manual Testing Required

- [ ] T019 Verify Profiles nav shows Folder24 icon with "Profile Manager" label (manual testing)
- [ ] T020 Verify Tests nav shows ClipboardTaskList24 icon with "Test Suite" label (manual testing)
- [ ] T021 Verify Results nav shows Poll24 icon with "Results Dashboard" label (manual testing)
- [ ] T022 Verify Diagnostics nav shows HeartPulse24 icon with "System Diagnostics" label (manual testing)
- [ ] T023 Verify all navigation tooltips are professional and descriptive (manual testing)

### Verification Checklist - US2 (Page Headers) - Manual Testing Required

- [ ] T024 Verify ProfileSelectorView header matches navigation (Folder24, "Profile Manager") (manual testing)
- [ ] T025 Verify TestListView header matches navigation (ClipboardTaskList24, "Test Suite") (manual testing)
- [ ] T026 Verify ResultsView header matches navigation (Poll24, "Results Dashboard") (manual testing)
- [ ] T027 Verify DiagnosticsView header matches navigation (HeartPulse24, "System Diagnostics") (manual testing)
- [ ] T028 Verify TestConfigView header shows SettingsCog24 icon (manual testing)
- [ ] T029 Verify RunProgressView header shows PlayCircle24 icon (manual testing)

### Verification Checklist - US3 (Window Branding) - Manual Testing Required

- [ ] T030 Verify window title bar shows ShieldCheckmark24 icon (manual testing)
- [ ] T031 Verify application is distinguishable in Windows taskbar (manual testing)
- [ ] T032 Verify application is identifiable in Alt+Tab switcher (manual testing)

### Theme & Functionality - Manual Testing Required

- [ ] T033 Verify icons work correctly in dark theme (manual testing)
- [ ] T034 Verify icons work correctly in light theme (if available) (manual testing)
- [ ] T035 Verify existing navigation functionality still works (manual testing)
- [ ] T036 Verify existing keyboard navigation still works (manual testing)

### Finalize

- [x] T037 Commit changes with descriptive message
- [x] T038 Push to feature branch (commit: 004fc89)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (US1 & US2)**: No dependencies - start immediately (MVP)
- **Phase 2 (US3)**: Depends on T001 (window icon already done)
- **Phase 3 (US4)**: No dependencies on other phases (different XAML locations)
- **Phase 4 (Verification)**: Depends on Phases 1-3 completion

### Task Dependencies

```
T001-T005 (sequential - same file MainWindow.xaml)
    ↓
T006-T011 (parallel - different view files)
    ↓
T012-T013 (verification tasks)
T014-T016 (parallel - different view files, can run alongside T006-T011)
    ↓
T017 (build after all code changes)
    ↓
T018-T036 (verification - single testing session)
    ↓
T037 → T038 (commit then push)
```

### Parallel Opportunities

- T006-T011 can be done in parallel (different view XAML files)
- T014-T016 can be done in parallel (different view XAML files)
- T019-T036 (verification) can be performed in a single testing session

---

## Implementation Strategy

### MVP Delivery (User Stories 1 & 2)

1. Complete T001-T011 (navigation + page headers)
2. Build and verify (T017-T029)
3. Stop and validate - MVP complete

### Full Feature Delivery

1. Complete Phase 1 (T001-T011) - Navigation + Headers
2. Complete Phase 2 (T012-T013) - Window Branding verification
3. Complete Phase 3 (T014-T016) - Empty States
4. Complete Phase 4 (T017-T038) - Verification & Polish

### Recommended Order (Single Developer)

1. Complete T001-T005 (MainWindow.xaml changes)
2. Complete T006-T011 in parallel batches (view headers)
3. Complete T014-T016 (empty states)
4. Complete T017 (build)
5. Complete T018-T036 (verification)
6. Complete T037-T038 (commit and push)

---

## Icon Fallback Reference

If any icon is unavailable in WPF-UI SymbolRegular:

| Proposed | Fallback |
|----------|----------|
| ClipboardTaskList24 | TaskListLtr24 |
| Poll24 | DataBarVertical24 |
| HeartPulse24 | Stethoscope24 |
| SettingsCog24 | Settings24 |
| PlayCircle24 | Play24 |
| ShieldCheckmark24 | Shield24 |

**Note**: Final icon choices (after verifying availability in WPF-UI 4.2.0):
- Folder24 ✓ (available)
- ClipboardTaskList24 → Beaker24 (fallback - ClipboardTaskList24 not available)
- Poll24 ✓ (available)
- HeartPulse24 → Stethoscope24 (fallback - HeartPulse24 not available)
- SettingsCog24 → Settings24 (fallback - SettingsCog24 not available, Settings24 acceptable)
- PlayCircle24 → Play24 (fallback - PlayCircle24 not available, Play24 acceptable)
- ShieldCheckmark24 → Shield24 (fallback - ShieldCheckmark24 not available)

---

## Notes

- All changes are XAML-only - no C# code changes required
- US1 and US2 share MainWindow.xaml, so they're combined in Phase 1
- US3 (Window Branding) is mostly verification since icon change is in T001
- US4 (Empty States) may have minimal changes if empty states already exist
- No automated tests required - manual verification sufficient for UI changes
- Commit after all verification passes to ensure fix is complete
