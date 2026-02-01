# Tasks: Improve Test Configuration View

**Input**: Design documents from `/specs/013-improve-test-config/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not required for this bug fix (manual verification only).

**Organization**: Tasks are grouped by user story. US1 and US2 are both P1 priority and share foundational changes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: `src/ReqChecker.App/`
- **ViewModels**: `src/ReqChecker.App/ViewModels/`
- **Views**: `src/ReqChecker.App/Views/`
- **Services**: `src/ReqChecker.App/Services/`

---

## Phase 1: Foundational Changes (Blocking Prerequisites)

**Purpose**: Core changes required by multiple user stories - MUST complete before story-specific work

**⚠️ CRITICAL**: Both US1 (Back Navigation) and US2 (Remove Duplicates) require changes to TestConfigViewModel

### ViewModel Foundation

- [ ] T001 Add `NavigationService` field and constructor parameter to `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`
- [ ] T002 Update `src/ReqChecker.App/Services/NavigationService.cs` NavigateToTestConfig method to pass `this` to ViewModel constructor

**Checkpoint**: TestConfigViewModel can now access NavigationService for navigation operations

---

## Phase 2: User Story 1 - Navigate Back from Test Configuration (Priority: P1) 🎯 MVP

**Goal**: Back button navigates to Tests list instead of just resetting form values

**Independent Test**: Open Tests view → click any test → click Back button → returns to Tests list

### Implementation for User Story 1

- [ ] T003 [US1] Add `BackCommand` property and `OnBack()` method to `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`
- [ ] T004 [US1] Wire Back button to `BackCommand` in `src/ReqChecker.App/Views/TestConfigView.xaml` (change line 67 from CancelCommand to BackCommand)

**Checkpoint**: Back button navigates to Tests list. Cancel button still resets form (unchanged).

---

## Phase 3: User Story 2 - Remove Duplicate Fields (Priority: P1) 🎯 MVP

**Goal**: Timeout/Retries appear ONLY in Execution Settings section, NOT in Test Parameters section

**Independent Test**: Open any test configuration → verify Test Parameters shows only custom parameters from profile, NOT Timeout/RetryCount/RequiresAdmin

### Implementation for User Story 2

- [ ] T005 [US2] Remove hardcoded Timeout/RetryCount/RequiresAdmin additions from `InitializeParameters()` in `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs` (remove lines 68-79)
- [ ] T006 [US2] Simplify `SaveAsync()` in `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs` - remove special handling for Timeout/RetryCount/RequiresAdmin in the foreach loop (they're saved from ViewModel properties, not Parameters)

**Checkpoint**: Test Parameters section shows only profile-defined parameters. Empty state shows when no parameters exist.

---

## Phase 4: User Story 3 - Verify Field Policy Support (Priority: P2)

**Goal**: Verify that parameters respect their defined field policies (Editable, Locked, Hidden, PromptAtRun)

**Independent Test**: Load a test with various field policies and verify each renders correctly

### Verification (No Code Changes - Already Implemented)

- [ ] T007 [US3] Verify Editable parameters show as editable text fields
- [ ] T008 [US3] Verify Locked parameters show with locked field control
- [ ] T009 [US3] Verify Hidden parameters are not visible
- [ ] T010 [US3] Verify PromptAtRun parameters show "Will be prompted during test execution" indicator

**Checkpoint**: All four field policy types render correctly

---

## Phase 5: Verification & Polish

**Purpose**: Build, test, and finalize the fix

### Build & Test

- [ ] T011 Build application with `dotnet build src/ReqChecker.App`
- [ ] T012 Run application and verify all acceptance criteria from spec.md

### Verification Checklist

- [ ] T013 Verify Back button returns to Tests list
- [ ] T014 Verify Cancel button resets form (does NOT navigate)
- [ ] T015 Verify Timeout appears ONLY in Execution Settings section
- [ ] T016 Verify Retries appears ONLY in Execution Settings section
- [ ] T017 Verify Test Parameters shows only profile-defined custom parameters
- [ ] T018 Verify empty state displays when test has no custom parameters
- [ ] T019 Verify Save button persists changes to Timeout/Retries from Execution Settings

### Finalize

- [ ] T020 Commit changes with descriptive message
- [ ] T021 Push to feature branch

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Foundational)**: No dependencies - start immediately
- **Phase 2 (US1)**: Depends on Phase 1 completion (needs NavigationService in ViewModel)
- **Phase 3 (US2)**: Depends on Phase 1 completion (modifies same ViewModel)
- **Phase 4 (US3)**: Depends on Phase 3 completion (parameters must be correct to verify policies)
- **Phase 5 (Verification)**: Depends on all implementation phases

### Task Dependencies

```
T001 → T002 (constructor change needed before NavigationService update)
    ↓
T003 → T004 (BackCommand must exist before wiring in XAML)
T005 → T006 (remove params before simplifying SaveAsync)
    ↓
T007-T010 (verification - sequential testing session)
    ↓
T011 → T012-T019 (build before testing)
    ↓
T020 → T021 (commit then push)
```

### Parallel Opportunities

- T003 and T005 can be done in parallel (different methods in same file)
- T004 and T006 can be done in parallel (different files)
- T007-T010 (verification) can be performed in a single testing session
- T013-T019 (verification checklist) can be performed in a single testing session

---

## Implementation Strategy

### Recommended Order (Single Developer)

1. Complete T001-T002 (Foundational - NavigationService injection)
2. Complete T003-T004 (US1 - Back navigation)
3. Complete T005-T006 (US2 - Remove duplicates)
4. Complete T007-T010 (US3 - Verify field policies)
5. Complete T011-T019 (Build and verification)
6. Complete T020-T021 (Commit and push)

### MVP Delivery

After completing T001-T004, US1 (Back Navigation) is independently testable.
After completing T005-T006, US2 (Remove Duplicates) is independently testable.
US3 is verification-only - no code changes required.

---

## User Story Mapping

| Story | Priority | Tasks | Description |
|-------|----------|-------|-------------|
| Foundation | - | T001, T002 | NavigationService injection (required by US1) |
| US1 | P1 | T003, T004 | Back button navigation |
| US2 | P1 | T005, T006 | Remove duplicate fields |
| US3 | P2 | T007-T010 | Verify field policy support (no code changes) |
| Polish | - | T011-T021 | Build, verify, commit, push |

---

## Notes

- This is a bug fix with primarily code removal and wiring changes
- No automated tests required - manual verification is sufficient for UI fixes
- US1 and US2 are both P1 and can be completed in parallel after foundational phase
- US3 requires no code changes - field policy support already exists, just needs verification
- Commit after all verification passes to ensure fix is complete
