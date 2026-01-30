# Tasks: Auto-Load Bundled Configuration

**Input**: Design documents from `/specs/004-auto-load-config/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Core interfaces/models**: `src/ReqChecker.Core/`
- **Infrastructure implementations**: `src/ReqChecker.Infrastructure/`
- **App layer (WPF)**: `src/ReqChecker.App/`
- **Tests**: `tests/ReqChecker.*.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create new interfaces and models required by multiple user stories

- [X] T001 [P] Create StartupProfileResult model in src/ReqChecker.Core/Models/StartupProfileResult.cs
- [X] T002 [P] Create IStartupProfileService interface in src/ReqChecker.Core/Interfaces/IStartupProfileService.cs

**Checkpoint**: Core contracts defined, ready for infrastructure implementation

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement the startup profile service that all user stories depend on

**⚠️ CRITICAL**: User stories 1-3 cannot proceed without the service implementation

- [X] T003 Implement StartupProfileService in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs
- [X] T004 Register IStartupProfileService in DI container in src/ReqChecker.App/App.xaml.cs

**Checkpoint**: Foundation ready - StartupProfileService available for injection

---

## Phase 3: User Story 1 - Auto-Load Bundled Configuration on Startup (Priority: P1) 🎯 MVP

**Goal**: When `startup-profile.json` exists alongside the executable, automatically load it and navigate directly to the test list, bypassing the profile selector.

**Independent Test**: Place a valid `startup-profile.json` in the app directory, launch the app, verify tests appear immediately without any dialogs.

### Implementation for User Story 1

- [X] T005 [US1] Add startup profile check logic in App.OnStartup in src/ReqChecker.App/App.xaml.cs
- [X] T006 [US1] Add NavigateToTestListWithProfile method to NavigationService in src/ReqChecker.App/Services/NavigationService.cs
- [X] T007 [US1] Update MainWindow to check AppState.CurrentProfile on load and navigate accordingly in src/ReqChecker.App/MainWindow.xaml.cs
- [X] T008 [US1] Add Serilog logging for successful startup profile load in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs

**Checkpoint**: App auto-loads startup-profile.json and shows test list immediately

---

## Phase 4: User Story 2 - Graceful Handling When No Bundled Config Exists (Priority: P2)

**Goal**: When no `startup-profile.json` exists, the app starts normally showing the profile selector without any error messages.

**Independent Test**: Launch the app without `startup-profile.json` in the directory, verify normal profile selector appears with no errors.

### Implementation for User Story 2

- [X] T009 [US2] Ensure StartupProfileService.TryLoadStartupProfileAsync returns NotFound (not error) when file missing in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs
- [X] T010 [US2] Verify App.OnStartup falls through to profile selector when result.Success is false and result.FileFound is false in src/ReqChecker.App/App.xaml.cs
- [X] T011 [US2] Add Serilog logging (Information level) when startup profile not found in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs

**Checkpoint**: App starts normally with profile selector when no startup-profile.json exists

---

## Phase 5: User Story 3 - Handle Invalid or Corrupted Bundled Config (Priority: P3)

**Goal**: When `startup-profile.json` exists but is invalid, show a clear error message and allow the user to proceed to the profile selector.

**Independent Test**: Place an invalid JSON file as `startup-profile.json`, launch the app, verify error dialog appears with option to continue to profile selector.

### Implementation for User Story 3

- [X] T012 [US3] Add error handling for invalid JSON in StartupProfileService.TryLoadStartupProfileAsync in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs
- [X] T013 [US3] Add error handling for schema validation failures in StartupProfileService in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs
- [X] T014 [US3] Add error handling for empty files (0 bytes) - treat as NotFound in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs
- [X] T015 [US3] Add error handling for files with no tests - treat as NotFound in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs
- [X] T016 [US3] Create ShowStartupProfileError method in App.xaml.cs with Continue button in src/ReqChecker.App/App.xaml.cs
- [X] T017 [US3] Add Serilog logging (Warning level) for startup profile errors in src/ReqChecker.Infrastructure/Profile/StartupProfileService.cs

**Checkpoint**: Invalid startup profiles show user-friendly error with path to continue

---

## Phase 6: User Story 4 - Sample Diagnostic Profile Included with Application (Priority: P2)

**Goal**: Include a pre-configured "Sample Diagnostics" profile as an embedded resource that support teams can export and use as a template.

**Independent Test**: Launch the app (without startup-profile.json), verify "Sample Diagnostics" appears in bundled profiles list with 4 working tests.

### Implementation for User Story 4

- [X] T018 [US4] Create sample-diagnostics.json with 4 test definitions in src/ReqChecker.App/Profiles/sample-diagnostics.json
- [X] T019 [US4] Add sample-diagnostics.json as EmbeddedResource in src/ReqChecker.App/ReqChecker.App.csproj
- [X] T020 [US4] Verify ProfileSelectorViewModel loads sample-diagnostics.json from embedded resources (existing LoadBundledProfilesAsync should work)

**Checkpoint**: Sample Diagnostics profile visible and exportable for client distribution

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and documentation updates

- [X] T021 [P] Update quickstart.md with final testing instructions in specs/004-auto-load-config/quickstart.md
- [X] T022 [P] Verify all logging messages are consistent and useful for troubleshooting
- [X] T023 Run manual end-to-end test: valid startup-profile.json auto-loads
- [X] T024 Run manual end-to-end test: missing startup-profile.json shows profile selector
- [X] T025 Run manual end-to-end test: invalid startup-profile.json shows error then profile selector
- [X] T026 Run manual end-to-end test: Sample Diagnostics appears in bundled profiles

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - US1 (Phase 3): Core auto-load - MVP
  - US2 (Phase 4): No-file fallback - Can run parallel with US1
  - US3 (Phase 5): Error handling - Can run parallel with US1, US2
  - US4 (Phase 6): Sample profile - Independent, can run parallel with US1-3
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Phase 2 only - No dependencies on other stories
- **User Story 2 (P2)**: Depends on Phase 2 only - Independent of US1
- **User Story 3 (P3)**: Depends on Phase 2 only - Independent of US1, US2
- **User Story 4 (P2)**: Depends only on existing embedded resource pattern - Independent of US1-3

### Within Each User Story

- Implementation flows: Service → App integration → Logging
- All tasks within a story should be completed before checkpoint

### Parallel Opportunities

- T001, T002 can run in parallel (different files in Core)
- US1, US2, US3, US4 implementation phases can run in parallel after Phase 2
- T021, T022 can run in parallel (different concerns)

---

## Parallel Example: Phase 1 Setup

```bash
# Launch both Core tasks together:
Task: "Create StartupProfileResult model in src/ReqChecker.Core/Models/StartupProfileResult.cs"
Task: "Create IStartupProfileService interface in src/ReqChecker.Core/Interfaces/IStartupProfileService.cs"
```

## Parallel Example: User Stories After Phase 2

```bash
# After Foundational phase, these can run in parallel:
# Developer A: User Story 1 (T005-T008)
# Developer B: User Story 4 (T018-T020) - completely independent
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T004)
3. Complete Phase 3: User Story 1 (T005-T008)
4. **STOP and VALIDATE**: Test with valid startup-profile.json
5. Deploy/demo if ready - clients can now receive pre-configured apps

### Incremental Delivery

1. Complete Setup + Foundational → Service ready
2. Add User Story 1 → Auto-load works (MVP!)
3. Add User Story 2 → No-file fallback clean
4. Add User Story 3 → Error handling complete
5. Add User Story 4 → Sample profile for support teams
6. Each story adds value without breaking previous stories

### Single Developer Strategy (Sequential)

1. Phase 1 → Phase 2 → Phase 3 (MVP)
2. Then Phase 4 → Phase 5 → Phase 6
3. Finally Phase 7 (Polish)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each phase or logical group
- Stop at any checkpoint to validate story independently
- All error messages should be user-friendly (no technical jargon)
- Logging should help support teams troubleshoot without exposing sensitive info
