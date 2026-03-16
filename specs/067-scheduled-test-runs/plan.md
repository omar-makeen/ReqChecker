# Implementation Plan: Scheduled Test Runs

**Branch**: `067-scheduled-test-runs` | **Date**: 2026-03-16 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/067-scheduled-test-runs/spec.md`

## Summary

Add the ability to schedule test runs for execution at specific times (one-time or recurring). The scheduler runs in-process using `System.Threading.Timer`, executing tests via the existing `ITestRunner` pipeline. The app minimizes to the system tray on close to keep schedules active. Windows toast notifications inform the user of run starts and completions. Missed runs (due to full exit) are detected on next launch via timestamp comparison. Schedule definitions persist in `%APPDATA%/ReqChecker/schedules.json`.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0 (existing), CommunityToolkit.Mvvm 8.4.0 (existing), Microsoft.Toolkit.Uwp.Notifications (new — toast notifications)
**Storage**: `%APPDATA%/ReqChecker/schedules.json` (JSON via System.Text.Json with source generation)
**Testing**: Manual verification (existing pattern — no unit test framework in project)
**Target Platform**: Windows 10/11 desktop
**Project Type**: Desktop app (WPF)
**Performance Goals**: Execute within 30 seconds of scheduled time (15-second polling interval); support 50+ active schedules
**Constraints**: App must be running (in tray or foreground) for execution; no background Windows service
**Scale/Scope**: Up to 50 concurrent active schedules; single-user local machine

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is unconfigured (template placeholders only). No gates to enforce. Proceeding.

**Post-Phase 1 Re-check**: No violations. The design follows the existing three-layer architecture (Core → Infrastructure → App), adds no unnecessary abstractions, and reuses existing services (ITestRunner, IProfileLoader, IHistoryService).

## Project Structure

### Documentation (this feature)

```text
specs/067-scheduled-test-runs/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0: technical decisions
├── data-model.md        # Phase 1: entity models
├── quickstart.md        # Phase 1: integration guide
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Core/                          # Pure domain layer (net8.0)
│   ├── Enums/
│   │   ├── ScheduleType.cs                   # NEW: OneTime, Recurring
│   │   ├── ScheduleStatus.cs                 # NEW: Active, Paused, Completed, Expired, Missed
│   │   ├── RecurrenceType.cs                 # NEW: Hourly, Daily, Weekly, CustomInterval
│   │   ├── IntervalUnit.cs                   # NEW: Minutes, Hours, Days
│   │   └── ScheduleOutcome.cs                # NEW: Completed, Skipped, Missed, Failed
│   ├── Models/
│   │   ├── Schedule.cs                       # NEW: Schedule entity
│   │   ├── RecurrencePattern.cs              # NEW: Recurrence config
│   │   └── ScheduleExecutionRecord.cs        # NEW: Execution history
│   └── Interfaces/
│       ├── ISchedulerService.cs              # NEW: Scheduler operations
│       ├── ISchedulePersistenceService.cs    # NEW: Schedule CRUD + persistence
│       └── IToastNotificationService.cs      # NEW: Toast notification abstraction
│
├── ReqChecker.Infrastructure/                # Implementations (net8.0)
│   ├── Scheduling/
│   │   ├── SchedulerService.cs               # NEW: Timer-based scheduler engine
│   │   ├── SchedulePersistenceService.cs     # NEW: JSON file persistence
│   │   └── ScheduleCalculator.cs             # NEW: Next run time calculation
│   └── Notifications/
│       └── ToastNotificationService.cs       # NEW: Windows toast notifications
│
└── ReqChecker.App/                           # WPF presentation (net8.0-windows)
    ├── ViewModels/
    │   ├── SchedulesViewModel.cs             # NEW: Schedules list page VM
    │   └── CreateScheduleViewModel.cs        # NEW: Create/edit schedule dialog VM
    ├── Views/
    │   ├── SchedulesView.xaml(.cs)            # NEW: Schedules list page
    │   ├── CreateScheduleDialog.xaml(.cs)     # NEW: Schedule creation dialog
    │   └── MissedRunsDialog.xaml(.cs)         # NEW: Missed runs notification dialog
    ├── Services/
    │   ├── NavigationService.cs              # MODIFIED: Add NavigateToSchedules()
    │   └── AppState.cs                       # MODIFIED: (optional) scheduler status
    ├── App.xaml.cs                            # MODIFIED: Register services, tray setup, missed run check
    └── MainWindow.xaml(.cs)                   # MODIFIED: Add nav item, NotifyIcon, close-to-tray
```

**Structure Decision**: Follows existing three-layer architecture. New scheduling code goes into `Scheduling/` subdirectories in Core and Infrastructure. Notifications get their own `Notifications/` subdirectory in Infrastructure. UI components follow existing patterns (ViewModel + View pairs).

**Note on Infrastructure TFM**: `ReqChecker.Infrastructure` targets `net8.0` (not `net8.0-windows`). The `ToastNotificationService` uses `Microsoft.Toolkit.Uwp.Notifications` which supports `net8.0` but requires Windows. The package will be added to the Infrastructure project. If build issues arise, the notification service can be moved to the App project instead.

## Complexity Tracking

No constitution violations to justify — constitution is unconfigured.
