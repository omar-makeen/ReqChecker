# Tasks: Improve Test History Empty State UI/UX

**Input**: Design documents from `/specs/024-history-empty-state/`
**Prerequisites**: plan.md, spec.md, quickstart.md

**Tests**: Not requested - this is a UI-only fix with manual visual testing.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

- **Project Type**: Desktop WPF application
- **Paths**: `src/ReqChecker.App/`

---

## Phase 1: Setup

**Purpose**: No setup needed - modifying existing files only

*No tasks - existing project structure is sufficient*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core changes that enable the empty state fix

- [x] T001 [P] Fix status message logic to only display when history count > 0 in `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`

**Checkpoint**: ViewModel logic ready - UI changes can proceed

---

## Phase 3: User Story 1 - First-time User Views Empty History (Priority: P1)

**Goal**: User navigates to Test History with no saved runs and sees a clean, prominent empty state

**Independent Test**: Navigate to History page with no history data, verify clean empty state with CTA button

### Implementation for User Story 1

- [x] T002 [P] [US1] Fix trend chart visibility binding to use IsHistoryEmpty in `src/ReqChecker.App/Views/HistoryView.xaml` (line 179)
- [x] T003 [US1] Redesign empty state with larger icon, prominent heading, and PrimaryButton CTA in `src/ReqChecker.App/Views/HistoryView.xaml` (lines 367-397)

**Checkpoint**: User Story 1 complete - empty state shows correctly for first-time users

---

## Phase 4: User Story 2 - User Returns After Clearing History (Priority: P2)

**Goal**: User who cleared history sees same clean empty state as first-time user

**Independent Test**: Clear all history, return to History page, verify empty state appears

### Implementation for User Story 2

*No additional tasks needed - US1 implementation covers this scenario*

- All visibility bindings (status banner, trend chart, filter tabs, empty state) work correctly whether user is first-time or returning after clearing

**Checkpoint**: User Story 2 verified - same behavior after history clear

---

## Phase 5: Polish & Verification

**Purpose**: Final validation and cleanup

- [x] T004 Build and verify no compilation errors with `dotnet build src/ReqChecker.App`
- [x] T005 Manual verification: Navigate to empty History page and confirm all acceptance criteria pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies - can start immediately
- **User Story 1 (Phase 3)**: Can run in parallel with T001 (different files)
- **User Story 2 (Phase 4)**: No additional tasks - covered by US1
- **Polish (Phase 5)**: Depends on all previous phases

### Task Dependencies

```
T001 (ViewModel) ─┐
                  ├──> T004 (Build) ──> T005 (Verify)
T002, T003 (XAML) ┘
```

### Parallel Opportunities

- T001 (ViewModel) and T002/T003 (XAML) can run in parallel - different files
- T002 and T003 are sequential within same file

---

## Parallel Example

```bash
# These tasks can run in parallel (different files):
Task: T001 - Fix status message logic in HistoryViewModel.cs
Task: T002 - Fix trend chart binding in HistoryView.xaml

# Then T003 must follow T002 (same file):
Task: T003 - Redesign empty state in HistoryView.xaml
```

---

## Implementation Strategy

### MVP (User Story 1 Only)

1. Complete T001 (ViewModel fix) - optional, can skip if not showing banner
2. Complete T002 (trend chart binding) - hides empty chart
3. Complete T003 (empty state redesign) - prominent CTA
4. Build and test

### Quick Path (All Changes)

Since this is a small fix:
1. T001, T002, T003 can all be done in one session
2. Build once at the end
3. Manual verify

---

## Summary

| Metric | Count |
|--------|-------|
| Total Tasks | 5 |
| US1 Tasks | 2 |
| US2 Tasks | 0 (covered by US1) |
| Parallel Opportunities | T001 + T002 |
| Files Modified | 2 |

**Suggested MVP Scope**: User Story 1 only (tasks T001-T003)
