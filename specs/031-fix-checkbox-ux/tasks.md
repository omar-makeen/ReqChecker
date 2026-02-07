# Tasks: Fix Checkbox Visibility and Select All Placement

**Input**: Design documents from `/specs/031-fix-checkbox-ux/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)

**Tests**: No new tests required — both fixes are purely visual (XAML-only). Existing tests cover the ViewModel logic which is unchanged.

**Organization**: Tasks are grouped by user story. US1 and US2 are independent and can be implemented in parallel.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: User Story 1 - Checkbox Checkmark Visibility (Priority: P1) 🎯 MVP

**Goal**: Fix the invisible checkmark glyph so all checkboxes display a visible ✓ when checked.

**Independent Test**: Open the Test Suite page with a loaded profile. All checkboxes should show a visible checkmark. Toggle dark/light theme — checkmark remains visible in both.

### Implementation for User Story 1

- [X] T001 [P] [US1] Fix checkmark Path geometry in AccentCheckBox style — rescale `Data` from `M3.5,7.5 L6.5,10.5 L14.5,2.5` to coordinates that fit within the 18x18 container, and add explicit centering alignment in src/ReqChecker.App/Resources/Styles/Controls.xaml

**Checkpoint**: At this point, all checkboxes (Select All + per-test) display a visible checkmark when checked, in both themes.

---

## Phase 2: User Story 2 - Relocate Select All Checkbox (Priority: P2)

**Goal**: Move the "Select All" checkbox from the crowded header row to a dedicated toolbar row between the header and the test list.

**Independent Test**: Navigate to the Test Suite page. The "Select All" checkbox should appear in its own row directly above the test list, not in the header. Clicking it should toggle all test selections. When no profile is loaded, the toolbar row should be hidden.

### Implementation for User Story 2

- [X] T002 [P] [US2] Add a new Grid row (Auto height) between header and test list, move Select All checkbox + label from header StackPanel into the new toolbar row, update Grid.Row assignments for existing elements, and bind toolbar Visibility to CurrentProfile in src/ReqChecker.App/Views/TestListView.xaml

**Checkpoint**: Select All appears in a distinct toolbar row with proper visibility and toggle behavior. Header row now contains only the title, badge, and Run button.

---

## Phase 3: Polish & Verification

**Purpose**: Build verification and regression check.

- [X] T003 Verify `dotnet build` completes with zero errors and zero warnings
- [X] T004 Verify `dotnet test` — all existing tests pass (no regressions)

---

## Dependencies & Execution Order

### Phase Dependencies

- **US1 (Phase 1)**: No dependencies — can start immediately
- **US2 (Phase 2)**: No dependencies — can start immediately (different file than US1)
- **Polish (Phase 3)**: Depends on both US1 and US2 being complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent — modifies only Controls.xaml
- **User Story 2 (P2)**: Independent — modifies only TestListView.xaml

### Parallel Opportunities

- T001 and T002 can run in parallel (different files, no dependencies between them)

---

## Parallel Example

```bash
# Both user stories can be implemented simultaneously:
Task T001: "Fix checkmark Path geometry in src/ReqChecker.App/Resources/Styles/Controls.xaml"
Task T002: "Relocate Select All to toolbar row in src/ReqChecker.App/Views/TestListView.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001: Fix checkmark visibility
2. **STOP and VALIDATE**: Build and visually verify checkmarks appear
3. This alone resolves the most critical usability issue

### Full Delivery

1. Complete T001 + T002 in parallel (different files)
2. Complete T003 + T004: Build and test verification
3. All acceptance scenarios validated

---

## Notes

- Both tasks modify different files — fully parallelizable
- No ViewModel changes needed — all fixes are XAML-only
- Existing tests cover the ViewModel logic (IsAllSelected, HasSelectedTests, etc.) which is unchanged
- Visual verification required for both fixes (automated tests don't cover XAML rendering)
