# Tasks: Scheduled Test Runs

**Input**: Design documents from `/specs/067-scheduled-test-runs/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: No test framework in project. Manual verification only.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create all enums, models, and interfaces needed across stories. Add new NuGet package.

- [x] T001 Add `Microsoft.Toolkit.Uwp.Notifications` NuGet package to `src/ReqChecker.App/ReqChecker.App.csproj`
- [x] T002 [P] Create `ScheduleType` enum (OneTime, Recurring) in `src/ReqChecker.Core/Enums/ScheduleType.cs`
- [x] T003 [P] Create `ScheduleStatus` enum (Active, Paused, Completed, Expired, Missed) in `src/ReqChecker.Core/Enums/ScheduleStatus.cs`
- [x] T004 [P] Create `RecurrenceType` enum (Hourly, Daily, Weekly, CustomInterval) in `src/ReqChecker.Core/Enums/RecurrenceType.cs`
- [x] T005 [P] Create `IntervalUnit` enum (Minutes, Hours, Days) in `src/ReqChecker.Core/Enums/IntervalUnit.cs`
- [x] T006 [P] Create `ScheduleOutcome` enum (Completed, Skipped, Missed, Failed) in `src/ReqChecker.Core/Enums/ScheduleOutcome.cs`
- [x] T007 [P] Create `RecurrencePattern` model with FrequencyType, IntervalValue, IntervalUnit, DaysOfWeek, TimeOfDay, EndDate in `src/ReqChecker.Core/Models/RecurrencePattern.cs`
- [x] T008 [P] Create `Schedule` model with all fields from data-model.md in `src/ReqChecker.Core/Models/Schedule.cs`
- [x] T009 [P] Create `ScheduleExecutionRecord` model in `src/ReqChecker.Core/Models/ScheduleExecutionRecord.cs`
- [x] T010 [P] Create `ISchedulePersistenceService` interface (LoadAsync, SaveScheduleAsync, DeleteScheduleAsync, GetAllAsync) in `src/ReqChecker.Core/Interfaces/ISchedulePersistenceService.cs`
- [x] T011 [P] Create `ISchedulerService` interface (Start, Stop, GetSchedules, CreateSchedule, UpdateSchedule, DeleteSchedule, PauseSchedule, ResumeSchedule, GetMissedRuns, HasActiveSchedules) in `src/ReqChecker.Core/Interfaces/ISchedulerService.cs`
- [x] T012 [P] Create `IToastNotificationService` interface (ShowRunStarted, ShowRunCompleted) in `src/ReqChecker.Core/Interfaces/IToastNotificationService.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core scheduling infrastructure that MUST be complete before ANY user story can be implemented

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T013 Implement `ScheduleCalculator` with `CalculateNextRunTime(Schedule)` supporting one-time and recurring patterns (hourly, daily, weekly, custom interval) in `src/ReqChecker.Infrastructure/Scheduling/ScheduleCalculator.cs`
- [x] T014 Implement `SchedulePersistenceService` with JSON file persistence to `%APPDATA%/ReqChecker/schedules.json` using System.Text.Json source generation, including ScheduleStore model with version field, thread-safe read/write with SemaphoreSlim in `src/ReqChecker.Infrastructure/Scheduling/SchedulePersistenceService.cs`
- [x] T015 Implement `SchedulerService` core structure: constructor injection of ITestRunner, IProfileLoader, IHistoryService, ISchedulePersistenceService; System.Threading.Timer with 15-second interval; Start/Stop lifecycle; schedule loading on init in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Schedule a One-Time Test Run (Priority: P1) MVP

**Goal**: Users can create a one-time schedule, the scheduler executes tests at the scheduled time, and results appear in history.

**Independent Test**: Schedule a run 2 minutes from now, wait for it to execute, verify results appear in history.

### Implementation for User Story 1

- [x] T016 [US1] Add `CreateSchedule`, `DeleteSchedule`, `GetAllSchedules` methods to `SchedulerService` — create schedule validates future date/time, persists via `ISchedulePersistenceService`, and recalculates timer; for one-time only in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T017 [US1] Implement scheduled execution logic in `SchedulerService.OnTimerElapsed`: check all active schedules where NextRunTime <= now, load profile via `IProfileLoader.LoadFromFileAsync`, call `ITestRunner.RunTestsAsync` with no-op credential callback, save results via `IHistoryService.SaveRunAsync`, update schedule status to Completed (one-time), create ScheduleExecutionRecord, persist changes in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T018 [US1] Implement `CreateScheduleViewModel` with Name, ScheduledDate, ScheduledTime properties, validation (future date/time required, name required), SaveCommand that calls ISchedulerService.CreateSchedule for one-time schedules in `src/ReqChecker.App/ViewModels/CreateScheduleViewModel.cs`
- [x] T019 [US1] Create `CreateScheduleDialog.xaml` with schedule name TextBox, DatePicker, TimePicker, Save/Cancel buttons, validation error display — one-time schedule creation only in `src/ReqChecker.App/Views/CreateScheduleDialog.xaml` and `src/ReqChecker.App/Views/CreateScheduleDialog.xaml.cs`
- [x] T020 [US1] Implement `SchedulesViewModel` with ObservableCollection of schedules loaded from ISchedulerService.GetAllSchedules, auto-refresh via timer or event, display schedule name/type/status/next-run-time, CreateScheduleCommand that opens CreateScheduleDialog, DeleteCommand in `src/ReqChecker.App/ViewModels/SchedulesViewModel.cs`
- [x] T021 [US1] Create `SchedulesView.xaml` with AnimatedPageHeader (gradient accent, icon container, title "Schedules", subtitle), ItemsControl/ListView listing schedules with name, type badge, status, next run time, delete button; empty state message when no schedules exist in `src/ReqChecker.App/Views/SchedulesView.xaml` and `src/ReqChecker.App/Views/SchedulesView.xaml.cs`
- [x] T022 [US1] Add `NavigateToSchedules()` method to `NavigationService` in `src/ReqChecker.App/Services/NavigationService.cs`
- [x] T023 [US1] Add "Schedules" NavigationViewItem with CalendarClock24 icon to MainWindow sidebar between "Test History" and "System Diagnostics", wire NavItem_Click handler and SetNavigationSelection case in `src/ReqChecker.App/MainWindow.xaml` and `src/ReqChecker.App/MainWindow.xaml.cs`
- [x] T024 [US1] Register all new services in DI container in `App.xaml.cs` ConfigureServices: ISchedulePersistenceService as singleton, ISchedulerService as singleton (inject ITestRunner, IProfileLoader, IHistoryService, ISchedulePersistenceService), SchedulesViewModel as transient, CreateScheduleViewModel as transient; call SchedulerService.Start() after service provider build in `src/ReqChecker.App/App.xaml.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional — user can create a one-time schedule, it executes at the scheduled time, and results appear in history

---

## Phase 4: User Story 2 — Schedule Recurring Test Runs (Priority: P2)

**Goal**: Users can create recurring schedules (hourly, daily, weekly, custom interval) that execute repeatedly.

**Independent Test**: Create a recurring schedule with 5-minute interval, verify at least two consecutive executions occur, check both results appear in history.

### Implementation for User Story 2

- [x] T025 [US2] Extend `CreateScheduleViewModel` with ScheduleType toggle (OneTime/Recurring), RecurrenceType selection (Hourly/Daily/Weekly/CustomInterval), IntervalValue, IntervalUnit, DaysOfWeek multi-select, TimeOfDay picker, optional EndDate; add validation for 5-minute minimum interval in `src/ReqChecker.App/ViewModels/CreateScheduleViewModel.cs`
- [x] T026 [US2] Extend `CreateScheduleDialog.xaml` with recurring schedule UI: ScheduleType radio buttons, recurrence pattern panel (shown when Recurring selected) with frequency dropdown, interval input, day-of-week checkboxes (for Weekly), time picker (for Daily/Weekly), optional end date picker in `src/ReqChecker.App/Views/CreateScheduleDialog.xaml`
- [x] T027 [US2] Extend `SchedulerService.OnTimerElapsed` to handle recurring schedules: after execution, calculate next run time via ScheduleCalculator, update NextRunTime on the schedule, check end date expiration (set status to Expired if past), persist updated schedule in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T028 [US2] Add human-readable recurrence display method to `ScheduleCalculator` (e.g., "Every Monday at 9:00 AM", "Every 2 hours", "Every day at 22:00") used by SchedulesView in `src/ReqChecker.Infrastructure/Scheduling/ScheduleCalculator.cs`
- [x] T029 [US2] Update `SchedulesView.xaml` to display recurrence pattern description for recurring schedules, show "One-time" or recurrence text, display end date if set in `src/ReqChecker.App/Views/SchedulesView.xaml`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work — one-time and recurring schedules create and execute correctly

---

## Phase 5: User Story 3 — View and Manage Scheduled Runs (Priority: P2)

**Goal**: Users can view all schedules in a dedicated page and edit, pause, resume, or delete them.

**Independent Test**: Create multiple schedules (one-time and recurring), verify they all appear in the list with correct details, perform edit/delete/pause operations.

### Implementation for User Story 3

- [x] T030 [US3] Add `PauseSchedule` and `ResumeSchedule` methods to `SchedulerService`: pause sets status to Paused and clears NextRunTime, resume sets status to Active and recalculates NextRunTime via ScheduleCalculator, persist changes in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T031 [US3] Add `UpdateSchedule` method to `SchedulerService`: validate updated fields (future time for one-time, 5-min minimum for recurring), recalculate NextRunTime, persist changes in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T032 [US3] Extend `SchedulesViewModel` with PauseCommand, ResumeCommand, EditCommand (opens CreateScheduleDialog in edit mode pre-filled with schedule data), status-based command availability (e.g., can't pause a Completed schedule), confirmation dialog before delete in `src/ReqChecker.App/ViewModels/SchedulesViewModel.cs`
- [x] T033 [US3] Extend `CreateScheduleViewModel` with edit mode: accept existing Schedule object, pre-populate all fields, change Save button text to "Update", call ISchedulerService.UpdateSchedule instead of CreateSchedule in `src/ReqChecker.App/ViewModels/CreateScheduleViewModel.cs`
- [x] T034 [US3] Update `SchedulesView.xaml` with action buttons per schedule row: Pause/Resume toggle button (icon changes based on status), Edit button, Delete button with confirmation; status-colored badges (Active=green, Paused=yellow, Completed=gray, Expired=red) in `src/ReqChecker.App/Views/SchedulesView.xaml`

**Checkpoint**: All management operations work — users have full control over their schedules

---

## Phase 6: User Story 4 — Notifications and Missed Run Handling (Priority: P3)

**Goal**: Windows toast notifications for run start/completion, system tray support, and missed run detection on launch.

**Independent Test**: Schedule a run, close the app via tray Exit, reopen, verify missed run dialog appears. Also verify toasts appear for start and completion.

### Implementation for User Story 4

- [x] T035 [US4] Implement `ToastNotificationService` with ShowRunStarted (schedule name toast) and ShowRunCompleted (schedule name + "X/Y tests passed" + "View Results" action button) using Microsoft.Toolkit.Uwp.Notifications ToastContentBuilder in `src/ReqChecker.App/Services/ToastNotificationService.cs`
- [x] T036 [US4] Wire toast notifications into `SchedulerService`: raise events (ScheduleRunStarted, ScheduleRunCompleted) before/after test execution; register IToastNotificationService in DI and subscribe to events in App.xaml.cs in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs` and `src/ReqChecker.App/App.xaml.cs`
- [x] T037 [US4] Add WPF-UI `NotifyIcon` (TrayIcon) to `MainWindow.xaml` with app icon, tooltip "ReqChecker", right-click context menu: "Open ReqChecker", "Next Run: --" (info, disabled), separator, "Exit"; wire LeftClick to restore window, TrayOpen_Click to restore, TrayExit_Click to Application.Shutdown in `src/ReqChecker.App/MainWindow.xaml` and `src/ReqChecker.App/MainWindow.xaml.cs`
- [x] T038 [US4] Implement close-to-tray behavior in `MainWindow.xaml.cs`: override OnClosing, if ISchedulerService.HasActiveSchedules then cancel close + Hide() + show tray icon; update tray "Next Run" menu item text with nearest scheduled time; handle tray restore (Show + Activate + WindowState.Normal) in `src/ReqChecker.App/MainWindow.xaml.cs`
- [x] T039 [US4] Add `GetMissedRuns()` method to `SchedulerService`: on initialization, scan all Active schedules where NextRunTime < DateTime.Now, return list of missed Schedule objects, update their status in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T040 [US4] Create `MissedRunsDialog.xaml` showing list of missed schedules (name, profile, scheduled time), "Run Now" button (triggers immediate execution of all missed profiles sequentially), "Dismiss" button (marks missed as acknowledged, recalculates next run for recurring), styled with WPF-UI Fluent dialog pattern in `src/ReqChecker.App/Views/MissedRunsDialog.xaml` and `src/ReqChecker.App/Views/MissedRunsDialog.xaml.cs`
- [x] T041 [US4] Wire missed run detection on app startup in `App.xaml.cs`: after SchedulerService.Start(), call GetMissedRuns(), if any exist show MissedRunsDialog after MainWindow loads; handle "Run Now" by calling SchedulerService for each missed schedule in `src/ReqChecker.App/App.xaml.cs`
- [x] T042 [US4] Register `IToastNotificationService` as `ToastNotificationService` singleton in DI, handle toast activation (when user clicks "View Results" action) to navigate to results page in `src/ReqChecker.App/App.xaml.cs`

**Checkpoint**: Full notification lifecycle works — start toast, completion toast, tray icon, close-to-tray, missed run dialog

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Edge cases, robustness, and final verification

- [x] T043 Add overlap prevention to `SchedulerService.OnTimerElapsed`: track currently-executing schedule ID, skip any schedule whose timer fires while another is running, log ScheduleExecutionRecord with Outcome=Skipped in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T044 Add profile-not-found handling to `SchedulerService`: wrap profile loading in try-catch, if FileNotFoundException then create ScheduleExecutionRecord with Outcome=Failed and ErrorMessage, log error, continue to next schedule in `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T045 [P] Add schedule name tag to RunReport when saving via IHistoryService so scheduled runs are identifiable in history view — add a ScheduleName property or tag field to RunReport model if needed in `src/ReqChecker.Core/Models/RunReport.cs` and `src/ReqChecker.Infrastructure/Scheduling/SchedulerService.cs`
- [x] T046 [P] Add empty state for SchedulesView: when no schedules exist, show centered icon + "No schedules yet" message + "Create Schedule" primary button, matching existing empty state patterns (e.g., HistoryView) in `src/ReqChecker.App/Views/SchedulesView.xaml`
- [x] T047 Build verification: run `dotnet build src/ReqChecker.App/` and fix any compilation errors
- [ ] T048 Run quickstart.md end-to-end verification: load profile, create one-time schedule, verify execution, create recurring schedule, verify multiple executions, test tray minimize, test missed run detection, verify toast notifications

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — delivers MVP
- **US2 (Phase 4)**: Depends on Phase 3 (extends CreateScheduleDialog and SchedulerService)
- **US3 (Phase 5)**: Depends on Phase 3 (extends SchedulesViewModel and SchedulesView); can run in parallel with US2
- **US4 (Phase 6)**: Depends on Phase 3 (needs working scheduler); can run in parallel with US2/US3
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundational only — no story dependencies. Delivers working MVP.
- **User Story 2 (P2)**: Extends US1's CreateScheduleDialog and SchedulerService. Cannot start until US1's T016-T019 are complete.
- **User Story 3 (P2)**: Extends US1's SchedulesViewModel and SchedulesView. Can run in parallel with US2.
- **User Story 4 (P3)**: Needs working SchedulerService from US1. Can run in parallel with US2 and US3.

### Within Each User Story

- Models before services
- Services before ViewModels
- ViewModels before Views
- DI registration after all implementations
- Story complete before moving to next priority

### Parallel Opportunities

- T002-T012 (all Setup enums/models/interfaces) can run in parallel
- T013-T014 (ScheduleCalculator and SchedulePersistenceService) can run in parallel
- US3 and US4 can run in parallel after US1 is complete
- T043-T046 (all Polish tasks) can run in parallel

---

## Parallel Example: Phase 1 Setup

```
# Launch all enum/model/interface files together (12 tasks):
T002: Create ScheduleType enum
T003: Create ScheduleStatus enum
T004: Create RecurrenceType enum
T005: Create IntervalUnit enum
T006: Create ScheduleOutcome enum
T007: Create RecurrencePattern model
T008: Create Schedule model
T009: Create ScheduleExecutionRecord model
T010: Create ISchedulePersistenceService interface
T011: Create ISchedulerService interface
T012: Create IToastNotificationService interface
```

## Parallel Example: User Story 1

```
# After T016-T017 (service logic), launch VM + View in parallel:
T018: CreateScheduleViewModel
T020: SchedulesViewModel
# Then after VMs, launch Views in parallel:
T019: CreateScheduleDialog.xaml
T021: SchedulesView.xaml
T022: NavigationService update
T023: MainWindow nav item
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T012)
2. Complete Phase 2: Foundational (T013-T015)
3. Complete Phase 3: User Story 1 (T016-T024)
4. **STOP and VALIDATE**: Create a one-time schedule, verify it executes, check history
5. Demo-ready with core scheduling capability

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. Add User Story 1 → One-time scheduling works (MVP!)
3. Add User Story 2 → Recurring scheduling works
4. Add User Story 3 → Full management UI
5. Add User Story 4 → Notifications, tray, missed runs
6. Polish → Edge cases and robustness
7. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- No unit test framework exists in the project — verification is manual via quickstart.md steps
- ToastNotificationService placed in App project (not Infrastructure) because Microsoft.Toolkit.Uwp.Notifications requires net8.0-windows TFM, matching App project
- SchedulerService events bridge Infrastructure → App layer for toast notifications
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
