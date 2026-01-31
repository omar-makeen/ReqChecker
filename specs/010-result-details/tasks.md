# Tasks: Display Test Result Details in Expanded Card

**Input**: Design documents from `/specs/010-result-details/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested - test tasks are excluded.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/ReqChecker.App/`, `tests/ReqChecker.App.Tests/`
- Based on existing WPF application structure

---

## Phase 1: Setup

**Purpose**: Create converter infrastructure for generating display text from TestResult

- [X] T001 Create TestResultSummaryConverter class in src/ReqChecker.App/Converters/TestResultSummaryConverter.cs
- [X] T002 [P] Create TestResultDetailsConverter class in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T003 Register converters in App.xaml resources

**Checkpoint**: Converters are registered and available for XAML bindings

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core ExpanderCard property additions that ALL user stories depend on

**Note**: No foundational blockers - the ExpanderCard already has Summary, TechnicalDetails, ErrorMessage properties. Stories can proceed directly.

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - View Test Result Summary (Priority: P1)

**Goal**: Display human-readable summary text when user expands a test card

**Independent Test**: Run any test, expand result card, verify summary text displays explaining the outcome

### Implementation for User Story 1

- [X] T004 [US1] Implement summary generation logic in TestResultSummaryConverter for Pass status in src/ReqChecker.App/Converters/TestResultSummaryConverter.cs
- [X] T005 [US1] Implement summary generation logic in TestResultSummaryConverter for Fail status in src/ReqChecker.App/Converters/TestResultSummaryConverter.cs
- [X] T006 [US1] Implement summary generation logic in TestResultSummaryConverter for Skipped status in src/ReqChecker.App/Converters/TestResultSummaryConverter.cs
- [X] T007 [US1] Add test-type specific summary patterns (HttpGet, Ping, FileExists, etc.) in src/ReqChecker.App/Converters/TestResultSummaryConverter.cs
- [X] T008 [US1] Update ResultsView.xaml to use TestResultSummaryConverter for Summary binding in src/ReqChecker.App/Views/ResultsView.xaml
- [X] T009 [US1] Add fallback message when all content is empty ("Test completed with no additional details") in src/ReqChecker.App/Converters/TestResultSummaryConverter.cs

**Checkpoint**: User Story 1 complete - expanding any test card shows human-readable summary

---

## Phase 4: User Story 3 - View Error Information (Priority: P1)

**Goal**: Display error information prominently with red styling for failed tests

**Independent Test**: Run a test that fails (e.g., ping unreachable host), expand card, verify error shows with red styling

### Implementation for User Story 3 (Error Display)

- [X] T010 [US3] Update ExpanderCard.xaml error section with premium styling (elevated background, 4px red left border) in src/ReqChecker.App/Controls/ExpanderCard.xaml
- [X] T011 [US3] Add error icon and category display to error section in src/ReqChecker.App/Controls/ExpanderCard.xaml
- [X] T012 [US3] Verify ErrorMessage binding uses Error.Message from TestResult in src/ReqChecker.App/Views/ResultsView.xaml

**Checkpoint**: User Story 3 complete - failed tests show error with premium red styling

---

## Phase 5: User Story 2 - View Technical Details (Priority: P2)

**Goal**: Display formatted technical output (response data, headers, timing) in monospace font

**Independent Test**: Run HTTP GET test, expand card, verify technical details section shows response data, headers, timing

### Implementation for User Story 2

- [X] T013 [US2] Implement Evidence parsing logic in TestResultDetailsConverter in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T014 [US2] Add [General] section formatting (Duration, Attempts) in TestResultDetailsConverter in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T015 [US2] Add [Response] section formatting (status code, response time, content-type) in TestResultDetailsConverter in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T016 [US2] Add [Headers] section formatting in TestResultDetailsConverter in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T017 [US2] Add [Body] section formatting in TestResultDetailsConverter in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T018 [P] [US2] Add [File Content] section formatting in TestResultDetailsConverter in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T019 [P] [US2] Add [Process List] section formatting in TestResultDetailsConverter in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T020 [P] [US2] Add [Registry] section formatting in TestResultDetailsConverter in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs
- [X] T021 [US2] Update ExpanderCard.xaml technical details section with premium styling (elevated background, 4px accent gradient left border) in src/ReqChecker.App/Controls/ExpanderCard.xaml
- [X] T022 [US2] Update ResultsView.xaml to use TestResultDetailsConverter for TechnicalDetails binding in src/ReqChecker.App/Views/ResultsView.xaml
- [X] T023 [US2] Ensure CredentialMaskConverter is chained after TestResultDetailsConverter in binding in src/ReqChecker.App/Views/ResultsView.xaml

**Checkpoint**: User Story 2 complete - technical details display with formatted sections and premium styling

---

## Phase 6: User Story 4 - View Test Metadata (Priority: P3)

**Goal**: Display duration and retry attempt count in expanded card

**Independent Test**: Run any test, expand card, verify duration is shown; run test with retries, verify attempt count shown

### Implementation for User Story 4

- [X] T024 [US4] Add TestDuration dependency property to ExpanderCard in src/ReqChecker.App/Controls/ExpanderCard.xaml.cs
- [X] T025 [US4] Add AttemptCount dependency property to ExpanderCard in src/ReqChecker.App/Controls/ExpanderCard.xaml.cs
- [X] T026 [US4] Add metadata section to ExpanderCard.xaml with elevated background and accent left border in src/ReqChecker.App/Controls/ExpanderCard.xaml
- [X] T027 [US4] Display formatted duration ("Xms" for <1s, "X.Xs" for >=1s) in metadata section in src/ReqChecker.App/Controls/ExpanderCard.xaml
- [X] T028 [US4] Display retry count only when AttemptCount > 1 ("Retry: X attempts") in src/ReqChecker.App/Controls/ExpanderCard.xaml
- [X] T029 [US4] Update ResultsView.xaml to bind Duration and AttemptCount to ExpanderCard in src/ReqChecker.App/Views/ResultsView.xaml

**Checkpoint**: User Story 4 complete - metadata shows duration and retry count when applicable

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final integration and validation

- [X] T030 Verify all sections hide when data is null/empty (FR-007) in src/ReqChecker.App/Controls/ExpanderCard.xaml
- [X] T031 Verify expand/collapse animation is preserved (FR-008) in src/ReqChecker.App/Controls/ExpanderCard.xaml.cs
- [X] T032 Verify keyboard navigation works (Enter/Space) for expand/collapse (FR-009)
- [X] T033 Verify dark and light theme support for all new styling
- [X] T034 Run quickstart.md validation steps

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: N/A - no foundational blockers
- **User Stories (Phase 3-6)**: Depend on Setup (T001-T003) completion
  - User stories can proceed in parallel or sequentially in priority order
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1 - Summary)**: Depends on T001 (TestResultSummaryConverter class)
- **User Story 3 (P1 - Error)**: Independent - uses existing bindings, just styling updates
- **User Story 2 (P2 - Technical)**: Depends on T002 (TestResultDetailsConverter class)
- **User Story 4 (P3 - Metadata)**: Independent - new properties and styling

### Within Each User Story

- Converter logic before XAML binding updates
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- T001 and T002 can run in parallel (different converter files)
- T018, T019, T020 can run in parallel (independent sections in same converter)
- US1 and US3 can run in parallel after Setup
- US2 and US4 can run in parallel after their dependencies

---

## Parallel Example: Setup Phase

```bash
# Launch both converter stubs in parallel:
Task: "Create TestResultSummaryConverter class in src/ReqChecker.App/Converters/TestResultSummaryConverter.cs"
Task: "Create TestResultDetailsConverter class in src/ReqChecker.App/Converters/TestResultDetailsConverter.cs"
```

## Parallel Example: User Story 2

```bash
# Launch all optional section formatters in parallel:
Task: "Add [File Content] section formatting in TestResultDetailsConverter"
Task: "Add [Process List] section formatting in TestResultDetailsConverter"
Task: "Add [Registry] section formatting in TestResultDetailsConverter"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 3 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 3: User Story 1 - Summary (T004-T009)
3. Complete Phase 4: User Story 3 - Error Display (T010-T012)
4. **STOP and VALIDATE**: Test both stories independently
5. Deploy/demo if ready - core value delivered

### Incremental Delivery

1. Complete Setup → Converters available
2. Add User Story 1 + 3 (P1) → Test independently → MVP complete
3. Add User Story 2 (P2) → Test independently → Technical details available
4. Add User Story 4 (P3) → Test independently → Full feature complete
5. Complete Polish → Production ready

### Task Count by Story

| Story | Tasks | Priority |
|-------|-------|----------|
| Setup | 3 | - |
| US1 (Summary) | 6 | P1 |
| US3 (Error) | 3 | P1 |
| US2 (Technical) | 11 | P2 |
| US4 (Metadata) | 6 | P3 |
| Polish | 5 | - |
| **Total** | **34** | - |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- US1 and US3 are both P1 - complete both for MVP
