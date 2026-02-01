# Tasks: Diagnostics Auto-Load

**Input**: Design documents from `/specs/017-diagnostics-auto-load/`
**Prerequisites**: plan.md, spec.md, research.md, quickstart.md

**Tests**: Not requested in spec. Manual verification via quickstart.md.

**Organization**: Tasks grouped by user story for independent implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No setup required - this feature modifies existing files only.

*No tasks - existing project structure is used.*

---

## Phase 2: Foundational (ViewModel Changes)

**Purpose**: Add CurrentMachineInfo property and LoadMachineInfo() method that ALL user stories depend on.

**⚠️ CRITICAL**: User story XAML changes cannot be made until these ViewModel changes are complete.

- [x] T001 Add CurrentMachineInfo observable property in src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs
- [x] T002 Add using directive for MachineInfoCollector in src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs
- [x] T003 Add LoadMachineInfo() method that calls MachineInfoCollector.Collect() in src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs
- [x] T004 Update UpdateSummaries() to compute MachineInfoSummary from CurrentMachineInfo in src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs
- [x] T005 Add Loaded event handler to trigger LoadMachineInfo() in src/ReqChecker.App/Views/DiagnosticsView.xaml.cs

**Checkpoint**: ViewModel now has CurrentMachineInfo property and auto-loads on page navigation. XAML updates can proceed.

---

## Phase 3: User Story 1 - View Machine Information Immediately (Priority: P1) 🎯 MVP

**Goal**: Display hostname, OS, CPU, RAM, username, and elevation status immediately on page load without requiring test runs.

**Independent Test**:
1. Launch app fresh (no prior test runs)
2. Navigate to System Diagnostics
3. Verify Machine Information section shows all system data

### Implementation for User Story 1

- [x] T006 [US1] Verify MachineInfoSummary now displays from CurrentMachineInfo (no code change - verify UpdateSummaries() works)

**Checkpoint**: Machine Information displays immediately on page load. US1 acceptance scenarios pass.

---

## Phase 4: User Story 2 - View Network Interfaces Immediately (Priority: P1)

**Goal**: Display all network adapters with name, status, MAC, and IP addresses immediately on page load.

**Independent Test**:
1. Launch app fresh (no prior test runs)
2. Navigate to System Diagnostics
3. Verify Network Interfaces section shows all adapters

### Implementation for User Story 2

- [x] T007 [US2] Update ItemsSource binding for network interfaces list from LastRunReport.MachineInfo.NetworkInterfaces to CurrentMachineInfo.NetworkInterfaces in src/ReqChecker.App/Views/DiagnosticsView.xaml
- [x] T008 [US2] Update network interface count badge binding from LastRunReport.MachineInfo.NetworkInterfaces.Count to CurrentMachineInfo.NetworkInterfaces.Count in src/ReqChecker.App/Views/DiagnosticsView.xaml
- [x] T009 [US2] Update empty state visibility binding from LastRunReport.MachineInfo.NetworkInterfaces.Count to CurrentMachineInfo.NetworkInterfaces.Count in src/ReqChecker.App/Views/DiagnosticsView.xaml

**Checkpoint**: Network Interfaces display immediately on page load. US2 acceptance scenarios pass.

---

## Phase 5: User Story 3 - Last Run Summary Remains Conditional (Priority: P2)

**Goal**: Ensure Last Run Summary continues to show "No test runs have been performed yet" until tests are actually run.

**Independent Test**:
1. Launch app fresh - verify Last Run Summary shows "No test runs have been performed yet"
2. Run tests - verify Last Run Summary shows test results

### Implementation for User Story 3

- [x] T010 [US3] Verify LastRunSummary logic unchanged in UpdateSummaries() (no code change - confirm existing behavior preserved)

**Checkpoint**: Last Run Summary still depends on test execution. US3 acceptance scenarios pass.

---

## Phase 6: Polish & Verification

**Purpose**: Final verification and cleanup

- [x] T011 [P] Build solution and verify no compiler errors via dotnet build in src/ReqChecker.App
- [x] T012 [P] Run quickstart.md verification steps manually
- [x] T013 Update CopyDetails() to use CurrentMachineInfo for network interfaces in src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: N/A - no setup tasks
- **Phase 2 (Foundational)**: Must complete before ANY user story
- **Phase 3 (US1)**: Depends on Phase 2 - verifies machine info works
- **Phase 4 (US2)**: Depends on Phase 2 - updates XAML bindings
- **Phase 5 (US3)**: Depends on Phase 2 - verifies existing behavior
- **Phase 6 (Polish)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends only on Foundational (Phase 2)
- **User Story 2 (P1)**: Depends only on Foundational (Phase 2) - can run parallel with US1
- **User Story 3 (P2)**: Depends only on Foundational (Phase 2) - can run parallel with US1/US2

### Task Dependencies Within Phases

**Phase 2 (Foundational)**:
- T001 → T002 → T003 → T004 (sequential - all modify same file)
- T005 can run after T003 (different file)

**Phase 4 (US2)**:
- T007, T008, T009 are all in same file but modify different sections - execute sequentially

### Parallel Opportunities

- After Phase 2: US1, US2, US3 can all proceed in parallel (different aspects)
- T011 and T012 in Polish phase can run in parallel

---

## Parallel Example: Foundational Phase

```bash
# T001-T004 must be sequential (same file)
# T005 can run after T003:
Task: "Add Loaded event handler in DiagnosticsView.xaml.cs" (after T003)
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Phase 2: Foundational ViewModel changes
2. Complete Phase 3: Verify US1 Machine Info works
3. Complete Phase 4: Update XAML bindings for US2 Network Interfaces
4. **STOP and VALIDATE**: Test both US1 and US2 independently
5. Both P1 stories complete = MVP delivered

### Incremental Delivery

1. Foundational → Core ViewModel ready
2. US1 + US2 → Machine info and network interfaces display on load (MVP!)
3. US3 → Verify Last Run Summary unchanged (regression check)
4. Polish → Final verification

---

## Notes

- This is a small feature - only 13 tasks total
- Most changes are in DiagnosticsViewModel.cs and DiagnosticsView.xaml
- No new files created - only existing files modified
- US3 is primarily a verification task (no code changes needed)
- CopyDetails() update (T013) ensures clipboard copy uses current data
