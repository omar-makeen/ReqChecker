# Tasks: Test Progress Delay for User Visibility

**Input**: Design documents from `/specs/006-test-progress-delay/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Not explicitly requested in specification. Manual verification via quickstart.md.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project type**: Single solution with layered architecture (App → Infrastructure → Core)
- **Source**: `src/ReqChecker.App/`, `src/ReqChecker.Core/`, `src/ReqChecker.Infrastructure/`
- **Tests**: `tests/ReqChecker.Infrastructure.Tests/`

---

## Phase 1: Setup

**Purpose**: No setup tasks required - extending existing project structure

All infrastructure already exists:
- Project structure established
- DI container configured
- PreferencesService in place
- SequentialTestRunner in place

**Checkpoint**: Proceed directly to Foundational phase

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core model extension that MUST be complete before user story implementation

**⚠️ CRITICAL**: User Story 1 depends on RunSettings having the InterTestDelayMs property

- [X] T001 Add `InterTestDelayMs` property (int, default 0) to RunSettings model in `src/ReqChecker.Core/Models/RunSettings.cs`

**Checkpoint**: Foundation ready - RunSettings can now carry delay configuration

---

## Phase 3: User Story 1 - Observable Test Execution (Priority: P1) 🎯 MVP

**Goal**: Add a 500ms delay between test completions so users can observe which test is currently running

**Independent Test**: Run a profile with 4+ tests and verify each test is visible in the "Currently Running" section for at least 500ms before the next test starts

### Implementation for User Story 1

- [X] T002 [US1] Extend UserPreferences class with `TestProgressDelayEnabled` (bool, default true) and `TestProgressDelayMs` (int, default 500) in `src/ReqChecker.App/Services/PreferencesService.cs`

- [X] T003 [US1] Add observable properties `_testProgressDelayEnabled` and `_testProgressDelayMs` with auto-save partial methods to PreferencesService in `src/ReqChecker.App/Services/PreferencesService.cs`

- [X] T004 [US1] Update PreferencesService `Load()` method to read delay settings (with clamp 0-3000) and `Save()` method to persist them in `src/ReqChecker.App/Services/PreferencesService.cs`

- [X] T005 [US1] Insert `Task.Delay(runSettings.InterTestDelayMs, cancellationToken)` after `progress?.Report()` in SequentialTestRunner, skipping delay after final test, in `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs`

- [X] T006 [US1] Add delay setting properties to RunProgressViewModel that delegate to PreferencesService and pass `InterTestDelayMs` to RunSettings when starting tests in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`

**Checkpoint**: At this point, tests should pause 500ms between each execution. Hardcoded default delay works.

---

## Phase 4: User Story 2 - Configurable Delay Duration (Priority: P2)

**Goal**: Allow users to adjust delay duration between 0ms and 3000ms via a slider control

**Independent Test**: Change the delay slider to 1500ms and run tests; verify each test pauses for approximately 1.5 seconds

### Implementation for User Story 2

- [X] T007 [US2] Add delay slider control (0-3000ms range) with value binding to `TestProgressDelayMs` in `src/ReqChecker.App/Views/RunProgressView.xaml`

- [X] T008 [US2] Add TextBlock showing current delay value (e.g., "500 ms") bound to `TestProgressDelayMs` in `src/ReqChecker.App/Views/RunProgressView.xaml`

**Checkpoint**: Users can now adjust delay duration. Slider changes take effect on next test run.

---

## Phase 5: User Story 3 - Delay Toggle Control (Priority: P2)

**Goal**: Allow users to quickly enable/disable delay without adjusting the slider value

**Independent Test**: Toggle delay OFF and run tests - verify no artificial delay; toggle back ON and verify delay resumes with preserved slider value

### Implementation for User Story 3

- [X] T009 [US3] Add ToggleSwitch control labeled "Demo Mode" bound to `TestProgressDelayEnabled` in `src/ReqChecker.App/Views/RunProgressView.xaml`

- [X] T010 [US3] Bind slider's `IsEnabled` property to `TestProgressDelayEnabled` so slider is disabled when toggle is OFF in `src/ReqChecker.App/Views/RunProgressView.xaml`

- [X] T011 [US3] Update RunProgressViewModel to pass 0 for `InterTestDelayMs` when `TestProgressDelayEnabled` is false in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`

**Checkpoint**: Users can toggle delay on/off. Slider value preserved when toggling.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validation, edge cases, and documentation

- [X] T012 Verify cancellation during delay completes within 200ms (SC-004) - manual test per quickstart.md

- [X] T013 Verify delay settings persist after app restart (SC-005) - check preferences.json

- [X] T014 Verify delay skipped after final test (FR-007) - observe no trailing pause

- [X] T015 [P] Build and run full solution to verify no compilation errors: `dotnet build src/ReqChecker.App`

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 1: Setup (skipped - nothing needed)
    ↓
Phase 2: Foundational (T001)
    ↓ BLOCKS ALL USER STORIES
    ├──→ Phase 3: User Story 1 (T002-T006) - MVP
    │         ↓
    ├──→ Phase 4: User Story 2 (T007-T008) - depends on US1
    │         ↓
    └──→ Phase 5: User Story 3 (T009-T011) - depends on US1 & US2
              ↓
Phase 6: Polish (T012-T015) - after all user stories
```

### User Story Dependencies

- **User Story 1 (P1)**: Depends on T001 (Foundational). No other story dependencies.
- **User Story 2 (P2)**: Depends on User Story 1 (slider needs ViewModel properties from US1)
- **User Story 3 (P3)**: Depends on User Story 1 and User Story 2 (toggle controls slider, which needs to exist)

### Within Each User Story

- T002-T004: PreferencesService changes (sequential, same file)
- T005: SequentialTestRunner (independent file)
- T006: ViewModel integration (depends on T002-T004)
- T007-T011: View changes (sequential, same file)

### Parallel Opportunities

Within Phase 3 (User Story 1):
- T002-T004 can run in parallel with T005 (different files)

Within Phase 6 (Polish):
- T012-T014 are manual tests (can run in parallel)
- T015 is final build verification

---

## Parallel Example: User Story 1

```bash
# These can run in parallel (different files):
Task T002-T004: "PreferencesService changes in src/ReqChecker.App/Services/PreferencesService.cs"
Task T005: "SequentialTestRunner delay in src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs"

# Then sequential (depends on above):
Task T006: "ViewModel integration in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 (Foundational) - Add InterTestDelayMs to RunSettings
2. Complete T002-T006 (User Story 1) - Delay works with hardcoded 500ms default
3. **STOP and VALIDATE**: Run tests, verify 500ms pause between each
4. This is a working MVP - delay feature functional without UI controls

### Incremental Delivery

1. Foundation (T001) → Delay property ready
2. User Story 1 (T002-T006) → Delay works with defaults → **MVP Complete**
3. User Story 2 (T007-T008) → Slider control added → Users can adjust duration
4. User Story 3 (T009-T011) → Toggle control added → Users can enable/disable
5. Polish (T012-T015) → Verify edge cases and final build

### Single Developer Strategy

Execute in order: T001 → T002 → T003 → T004 → T005 → T006 → T007 → T008 → T009 → T010 → T011 → T012 → T013 → T014 → T015

---

## Files Modified Summary

| File | Tasks | Lines Changed (est.) |
|------|-------|---------------------|
| `src/ReqChecker.Core/Models/RunSettings.cs` | T001 | +3 |
| `src/ReqChecker.App/Services/PreferencesService.cs` | T002, T003, T004 | +20 |
| `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs` | T005 | +5 |
| `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` | T006, T011 | +15 |
| `src/ReqChecker.App/Views/RunProgressView.xaml` | T007, T008, T009, T010 | +15 |
| **Total** | 15 tasks | ~58 lines |

---

## Notes

- No test tasks included (not explicitly requested in spec)
- Manual verification via quickstart.md acceptance tests
- T005 must pass CancellationToken to Task.Delay for immediate cancellation support
- T006/T011 coordinate toggle state → delay value passed to runner
- All UI controls in same StackPanel for visual grouping
