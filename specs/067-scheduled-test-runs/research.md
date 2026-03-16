# Research: Scheduled Test Runs

**Feature Branch**: `067-scheduled-test-runs`
**Date**: 2026-03-16

## Decision 1: Scheduling Engine

**Decision**: Use `System.Threading.Timer` with a 15-second check interval in a singleton `SchedulerService`.

**Rationale**: The scheduler must run when the app is minimized to the system tray (no visible window). `DispatcherTimer` is tied to the UI thread and would be inappropriate for background execution. `System.Threading.Timer` is lightweight, built-in, and runs on a ThreadPool thread. A 15-second polling interval balances responsiveness (SC-002 requires execution within 30 seconds of scheduled time) against CPU overhead.

**Alternatives considered**:
- `DispatcherTimer`: Tied to UI thread, unsuitable for tray-minimized background work.
- `Task.Delay` loop: More complex lifecycle management, no advantage over Timer.
- Quartz.NET: Full-featured scheduler library, but heavyweight for this use case. Adds external dependency for functionality that can be built with ~200 lines of code.
- Windows Task Scheduler (COM interop): Would work when app is closed, but adds significant complexity and requires elevated permissions for programmatic task creation.

## Decision 2: System Tray Implementation

**Decision**: Use WPF-UI's `NotifyIcon` control (part of WPF-UI 4.2.0, already a dependency).

**Rationale**: The project already depends on WPF-UI 4.2.0 which includes `Wpf.Ui.Tray.NotifyIcon`. Using the existing library avoids adding `System.Windows.Forms` as a dependency and integrates cleanly with the existing Fluent design system. The `NotifyIcon` supports context menus, icon management, and click events.

**Alternatives considered**:
- `System.Windows.Forms.NotifyIcon`: The classic approach, but requires adding WindowsForms as a dependency and mixing frameworks.
- Hardcodet.NotifyIcon.Wpf: Popular third-party library, but unnecessary given WPF-UI already provides this.

## Decision 3: Windows Toast Notifications

**Decision**: Use `Microsoft.Toolkit.Uwp.Notifications` NuGet package (Community Toolkit).

**Rationale**: This is the standard library for Windows 10/11 toast notifications from desktop apps. It provides a fluent builder API, supports action buttons ("View Results"), persists in Action Center, and works with both packaged and unpackaged apps. The app targets Windows (.NET 8.0-windows TFM), so platform compatibility is guaranteed.

**Alternatives considered**:
- Raw Windows API via COM interop: Complex, error-prone, and hard to maintain.
- WPF-UI notification panel: In-app only, won't show when minimized to tray.
- Custom WPF popup window: Doesn't integrate with Windows notification center.

## Decision 4: Schedule Persistence

**Decision**: Store schedules in `%APPDATA%/ReqChecker/schedules.json` using `System.Text.Json` with source generation.

**Rationale**: Consistent with existing persistence patterns (`preferences.json` in `%APPDATA%`, `history.json` in `%LOCALAPPDATA%`). Using `%APPDATA%` (not `%LOCALAPPDATA%`) since schedules are user configuration (like preferences), not generated data (like history). Source-generated JSON matches the pattern used by HistoryService for performance and AOT compatibility.

**Alternatives considered**:
- SQLite: Overkill for a list of up to 50 schedule records.
- Individual JSON files per schedule: Unnecessary complexity for the expected scale.
- Embedding in preferences.json: Would bloat the preferences file and mix concerns.

## Decision 5: Scheduled Execution Architecture

**Decision**: The `SchedulerService` directly uses `ITestRunner` and `IProfileLoader` to execute tests, bypassing the UI ViewModel layer entirely.

**Rationale**: Scheduled runs happen in the background — there's no UI to update during execution. The service loads the profile via `IProfileLoader.LoadFromFileAsync()`, calls `ITestRunner.RunTestsAsync()` with its own `IProgress<TestResult>` callback (for logging), and saves results via `IHistoryService.SaveRunAsync()`. This avoids coupling to `RunProgressViewModel` and the navigation system.

**Alternatives considered**:
- Routing through RunProgressViewModel: Would require navigating to the RunProgress page, which disrupts the user's current view and doesn't work when minimized to tray.
- Creating a headless ViewModel: Unnecessary abstraction layer when the service can call the same infrastructure directly.

## Decision 6: Credential Handling for Scheduled Runs

**Decision**: Scheduled runs use stored credentials from `ICredentialProvider` (Windows Credential Manager). If credentials are not stored and are required, the test is skipped with a "Missing credentials" error.

**Rationale**: Scheduled runs are unattended by nature. The `PromptForCredentials` callback used by `SequentialTestRunner` shows a UI dialog, which can't work when the app is in the tray. Instead, the scheduler sets a no-op credential callback that returns null, causing the runner to skip tests that need unstored credentials. The user is informed in the run results.

**Alternatives considered**:
- Storing credentials at schedule creation time: Security risk — credentials would be serialized to disk outside the secure Windows Credential Manager.
- Showing a tray popup for credentials: Poor UX for unattended runs, defeats the purpose of scheduling.

## Decision 7: Missed Run Detection

**Decision**: On `SchedulerService` initialization (app startup), compare each active schedule's `NextRunTime` against `DateTime.Now`. If `NextRunTime < Now`, mark as missed and update the `LastMissedAt` timestamp.

**Rationale**: Simple and reliable. The scheduler loads persisted schedules, checks timestamps, and identifies missed runs before starting the timer. Missed runs are surfaced through a dialog triggered in `App.xaml.cs` after the main window loads.

**Alternatives considered**:
- Persisting a "last checked" timestamp: More complex, same result.
- Relying on execution records: Requires checking history, which is slower and indirect.

## Decision 8: Where to Place New Code

**Decision**:
- `ISchedulerService` interface → `ReqChecker.Core.Interfaces`
- `Schedule`, `RecurrencePattern`, `ScheduleExecutionRecord` models → `ReqChecker.Core.Models`
- `ScheduleStatus`, `RecurrenceType` enums → `ReqChecker.Core.Enums`
- `SchedulerService`, `SchedulePersistenceService` → `ReqChecker.Infrastructure.Scheduling`
- `ToastNotificationService` → `ReqChecker.Infrastructure.Notifications`
- `SchedulesViewModel`, `CreateScheduleViewModel` → `ReqChecker.App.ViewModels`
- `SchedulesView`, `CreateScheduleDialog` → `ReqChecker.App.Views`
- System tray setup → `ReqChecker.App` (App.xaml.cs / MainWindow.xaml.cs)

**Rationale**: Follows the existing three-layer architecture. Core holds interfaces and models (no platform dependencies). Infrastructure holds implementations (can use System.Threading.Timer). App holds WPF-specific UI code.
