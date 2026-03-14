# Tasks: Unsaved Changes Warning

**Input**: Design documents from `/specs/061-unsaved-changes-warning/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Tests are included — the spec defines measurable success criteria requiring validation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No new project setup needed — all changes are within existing project structure.

*No setup tasks required — feature modifies existing files only.*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add the confirmation dialog capability to DialogService — required by all user stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T001 Add `ShowConfirmationDialog(string title, string message, string confirmText, string cancelText)` method returning `bool` to `src/ReqChecker.App/Services/DialogService.cs` using `System.Windows.MessageBox` with `YesNo` buttons. Map Yes → true (discard), No → false (stay). Title: "Unsaved Changes", Message: "You have unsaved changes. Do you want to discard them?", ConfirmText maps to "Yes" button, CancelText maps to "No" button.

**Checkpoint**: Foundation ready — DialogService can now show confirmation dialogs

---

## Phase 3: User Story 1 — Warn on Unsaved Changes When Navigating Away (Priority: P1) 🎯 MVP

**Goal**: When user edits timeout, retries, or parameters and clicks Back, show a confirmation dialog offering Discard or Stay.

**Independent Test**: Open any test's configuration, change the timeout value, click Back — confirmation dialog appears with Discard/Stay options.

### Tests for User Story 1

- [X] T002 [P] [US1] Create test file `tests/ReqChecker.App.Tests/ViewModels/TestConfigViewModelTests.cs` with test: `HasUnsavedChanges_ShouldBeFalse_WhenNoChanges` — initialize ViewModel with a TestDefinition, verify `HasUnsavedChanges` is false
- [X] T003 [P] [US1] Add test `HasUnsavedChanges_ShouldBeTrue_WhenTimeoutChanged` — change Timeout value, verify `HasUnsavedChanges` is true
- [X] T004 [P] [US1] Add test `HasUnsavedChanges_ShouldBeTrue_WhenRetryCountChanged` — change RetryCount value, verify `HasUnsavedChanges` is true
- [X] T005 [P] [US1] Add test `HasUnsavedChanges_ShouldBeTrue_WhenParameterValueChanged` — change a parameter's Value, verify `HasUnsavedChanges` is true
- [X] T006 [P] [US1] Add test `HasUnsavedChanges_ShouldBeTrue_WhenPasswordParameterChanged` — change a password parameter's Value (field name ending in "Password"), verify `HasUnsavedChanges` is true
- [X] T007 [P] [US1] Add test `BackCommand_ShouldShowDialog_WhenHasUnsavedChanges` — mock DialogService, change a value, execute BackCommand, verify `ShowConfirmationDialog` was called
- [X] T008 [P] [US1] Add test `BackCommand_ShouldNavigateBack_WhenUserDiscardsChanges` — mock DialogService to return true (discard), verify `NavigationService.GoBack()` was called
- [X] T009 [P] [US1] Add test `BackCommand_ShouldStayOnPage_WhenUserChoosesStay` — mock DialogService to return false (stay), verify `NavigationService.GoBack()` was NOT called

### Implementation for User Story 1

- [X] T010 [US1] Add private `Dictionary<string, string?> _baseline` field and `CaptureBaseline()` method to `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`. `CaptureBaseline()` creates a dictionary with keys "Timeout" (→ `Timeout?.ToString()`), "RetryCount" (→ `RetryCount?.ToString()`), and each parameter name (→ `Value`). Call `CaptureBaseline()` at the end of `InitializeParameters()`.
- [X] T011 [US1] Add computed `HasUnsavedChanges` property to `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`. Iterates `_baseline` and compares each value against current ViewModel property/parameter value. Returns true on first mismatch.
- [X] T012 [US1] Inject `DialogService` into `TestConfigViewModel` constructor in `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`. Update `NavigationService.NavigateToTestConfig()` in `src/ReqChecker.App/Services/NavigationService.cs` to pass `DialogService` to the ViewModel constructor.
- [X] T013 [US1] Modify the `BackCommand` handler in `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs` to check `HasUnsavedChanges` before navigating. If true, call `DialogService.ShowConfirmationDialog()`. If dialog returns true (discard), call `GoBack()`. If false (stay), do nothing. If `HasUnsavedChanges` is false, call `GoBack()` directly.

**Checkpoint**: User Story 1 is fully functional — editing any field and clicking Back shows a confirmation dialog

---

## Phase 4: User Story 2 — No Warning When No Changes Made (Priority: P1)

**Goal**: When user opens config, makes no changes, and clicks Back — navigate immediately without any dialog.

**Independent Test**: Open any test's configuration, do not change anything, click Back — immediate navigation, no dialog.

### Tests for User Story 2

- [X] T014 [P] [US2] Add test `BackCommand_ShouldNavigateImmediately_WhenNoChanges` to `tests/ReqChecker.App.Tests/ViewModels/TestConfigViewModelTests.cs` — do not change any values, execute BackCommand, verify `GoBack()` called without `ShowConfirmationDialog`
- [X] T015 [P] [US2] Add test `HasUnsavedChanges_ShouldBeFalse_WhenValueRevertedToOriginal` — change Timeout, then change it back to original value, verify `HasUnsavedChanges` is false

### Implementation for User Story 2

*No additional implementation needed — US1 implementation already handles this:*
- `HasUnsavedChanges` returns false when no changes → `BackCommand` navigates directly (T013)
- Value-based comparison naturally handles edit-then-revert (T011)

**Checkpoint**: User Stories 1 AND 2 both work — dialog only appears when there are actual net changes

---

## Phase 5: User Story 3 — No Warning After Saving (Priority: P2)

**Goal**: After user clicks Save Changes, dirty state resets. Back navigates immediately unless new changes were made post-save.

**Independent Test**: Modify a value, click Save, then click Back — no dialog appears. Then modify again after save, click Back — dialog appears.

### Tests for User Story 3

- [X] T016 [P] [US3] Add test `HasUnsavedChanges_ShouldBeFalse_AfterSave` to `tests/ReqChecker.App.Tests/ViewModels/TestConfigViewModelTests.cs` — change Timeout, call SaveAsync, verify `HasUnsavedChanges` is false
- [X] T017 [P] [US3] Add test `HasUnsavedChanges_ShouldBeTrue_WhenChangedAfterSave` — change Timeout, call SaveAsync, change RetryCount, verify `HasUnsavedChanges` is true

### Implementation for User Story 3

- [X] T018 [US3] Add `CaptureBaseline()` call at the end of `SaveAsync()` in `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs` — after the save completes successfully, re-capture baseline from current values so post-save state is the new clean state.

**Checkpoint**: All user stories are independently functional — save resets dirty state correctly

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Keyboard accessibility and final validation

- [X] T019 Verify keyboard accessibility of the confirmation dialog in `src/ReqChecker.App/Services/DialogService.cs` — MessageBox natively supports Escape (cancel/No) and Enter (focused button). Document in quickstart.md test scenario 10 that this is handled by the OS-native dialog.
- [X] T020 Run all tests in `tests/ReqChecker.App.Tests/` to verify no regressions
- [X] T021 Run quickstart.md manual validation scenarios 1–10

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies — can start immediately
- **User Story 1 (Phase 3)**: Depends on Phase 2 (DialogService method)
- **User Story 2 (Phase 4)**: Depends on Phase 3 (tests validate US1 behavior for no-change case)
- **User Story 3 (Phase 5)**: Depends on Phase 3 (extends SaveAsync with baseline reset)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational only — core dirty tracking + dialog
- **User Story 2 (P1)**: Tests only — validates US1 handles no-change case (no new implementation)
- **User Story 3 (P2)**: Adds one line to SaveAsync — depends on baseline from US1

### Within Each User Story

- Tests written first → verify they fail → then implement
- Models/data before services
- Services before UI integration

### Parallel Opportunities

- All US1 tests (T002–T009) can run in parallel — different test methods, same file
- US2 tests (T014–T015) can run in parallel
- US3 tests (T016–T017) can run in parallel
- T001 (DialogService) and T002–T009 (test stubs) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all US1 tests together (they target the same test file but different methods):
Task T002: "HasUnsavedChanges_ShouldBeFalse_WhenNoChanges"
Task T003: "HasUnsavedChanges_ShouldBeTrue_WhenTimeoutChanged"
Task T004: "HasUnsavedChanges_ShouldBeTrue_WhenRetryCountChanged"
Task T005: "HasUnsavedChanges_ShouldBeTrue_WhenParameterValueChanged"
Task T006: "HasUnsavedChanges_ShouldBeTrue_WhenPasswordParameterChanged"
Task T007: "BackCommand_ShouldShowDialog_WhenHasUnsavedChanges"
Task T008: "BackCommand_ShouldNavigateBack_WhenUserDiscardsChanges"
Task T009: "BackCommand_ShouldStayOnPage_WhenUserChoosesStay"

# Then implementation sequentially:
Task T010: Baseline + CaptureBaseline()
Task T011: HasUnsavedChanges computed property
Task T012: Inject DialogService
Task T013: Modify BackCommand
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational (T001 — DialogService method)
2. Complete Phase 3: User Story 1 (T002–T013)
3. **STOP and VALIDATE**: Test US1 independently — edit a field, click Back, verify dialog
4. This delivers the core value: unsaved changes are never silently discarded

### Incremental Delivery

1. Add User Story 1 → dirty tracking + dialog on Back (MVP!)
2. Add User Story 2 → validate no false positives (tests only, no new code)
3. Add User Story 3 → save resets baseline (1 line addition)
4. Polish → keyboard verification + full regression

---

## Notes

- [P] tasks = different files or independent test methods, no dependencies
- [Story] label maps task to specific user story for traceability
- US2 requires no new implementation — it validates that US1's value-based comparison handles the no-change and revert cases correctly
- All editable fields use string comparison: `Timeout?.ToString()`, `RetryCount?.ToString()`, `parameter.Value`
- PasswordBox changes are already synced to `TestParameterViewModel.Value` via code-behind handler — no special dirty tracking needed
- MessageBox provides native keyboard accessibility (Escape/Enter) — no custom keyboard handling required
