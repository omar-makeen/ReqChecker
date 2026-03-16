# Quickstart: Scheduled Test Runs

**Feature Branch**: `067-scheduled-test-runs`
**Date**: 2026-03-16

## Prerequisites

- .NET 8.0 SDK installed
- Windows 10/11 (toast notifications require Windows 10+)
- ReqChecker project builds successfully: `dotnet build src/ReqChecker.App/`

## New NuGet Package

```bash
# Add toast notification support to ReqChecker.App
dotnet add src/ReqChecker.App/ package Microsoft.Toolkit.Uwp.Notifications
```

## New Files to Create

### Core Layer (`src/ReqChecker.Core/`)

| File | Purpose |
|------|---------|
| `Enums/ScheduleType.cs` | OneTime, Recurring |
| `Enums/ScheduleStatus.cs` | Active, Paused, Completed, Expired, Missed |
| `Enums/RecurrenceType.cs` | Hourly, Daily, Weekly, CustomInterval |
| `Enums/IntervalUnit.cs` | Minutes, Hours, Days |
| `Enums/ScheduleOutcome.cs` | Completed, Skipped, Missed, Failed |
| `Models/Schedule.cs` | Schedule entity |
| `Models/RecurrencePattern.cs` | Recurrence configuration |
| `Models/ScheduleExecutionRecord.cs` | Execution history record |
| `Interfaces/ISchedulerService.cs` | Scheduler operations interface |
| `Interfaces/ISchedulePersistenceService.cs` | Schedule storage interface |
| `Interfaces/IToastNotificationService.cs` | Toast notification interface |

### Infrastructure Layer (`src/ReqChecker.Infrastructure/`)

| File | Purpose |
|------|---------|
| `Scheduling/SchedulerService.cs` | Timer-based scheduler engine |
| `Scheduling/SchedulePersistenceService.cs` | JSON file persistence |
| `Scheduling/ScheduleCalculator.cs` | Next run time calculation logic |
| `Notifications/ToastNotificationService.cs` | Windows toast notifications |

### App Layer (`src/ReqChecker.App/`)

| File | Purpose |
|------|---------|
| `ViewModels/SchedulesViewModel.cs` | Schedules list page |
| `ViewModels/CreateScheduleViewModel.cs` | Create/edit schedule dialog |
| `Views/SchedulesView.xaml` | Schedules list page UI |
| `Views/CreateScheduleDialog.xaml` | Schedule creation dialog UI |
| `Views/MissedRunsDialog.xaml` | Missed runs notification dialog |

### Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/App.xaml.cs` | Register new services in DI, add system tray setup, missed run detection on startup |
| `src/ReqChecker.App/MainWindow.xaml` | Add "Schedules" navigation item, add NotifyIcon for system tray |
| `src/ReqChecker.App/MainWindow.xaml.cs` | Handle close-to-tray, add schedule nav handler |
| `src/ReqChecker.App/Services/NavigationService.cs` | Add `NavigateToSchedules()` method |
| `src/ReqChecker.App/Services/AppState.cs` | (Optional) Add scheduler status property |

## Key Integration Points

### 1. Test Execution (Background)

```csharp
// SchedulerService executes tests without UI
var profile = await _profileLoader.LoadFromFileAsync(schedule.ProfileFilePath);
var progress = new Progress<TestResult>(result => _logger.Information("Test {Name}: {Status}", result.Name, result.Status));
var report = await _testRunner.RunTestsAsync(profile, progress, cancellationToken);
await _historyService.SaveRunAsync(report);
```

### 2. System Tray (MainWindow.xaml)

```xml
<!-- WPF-UI NotifyIcon -->
<ui:TrayIcon x:Name="TrayIcon"
             Icon="/Assets/logo.ico"
             ToolTip="ReqChecker"
             MenuOnRightClick="True"
             LeftClick="TrayIcon_LeftClick">
    <ui:TrayIcon.Menu>
        <ContextMenu>
            <MenuItem Header="Open ReqChecker" Click="TrayOpen_Click"/>
            <MenuItem Header="Next Run: --" x:Name="TrayNextRun" IsEnabled="False"/>
            <Separator/>
            <MenuItem Header="Exit" Click="TrayExit_Click"/>
        </ContextMenu>
    </ui:TrayIcon.Menu>
</ui:TrayIcon>
```

### 3. Toast Notification

```csharp
// Using Microsoft.Toolkit.Uwp.Notifications
new ToastContentBuilder()
    .AddText($"Schedule: {schedule.Name}")
    .AddText($"{passCount}/{totalCount} tests passed")
    .AddButton(new ToastButton()
        .SetContent("View Results")
        .AddArgument("action", "viewResults")
        .AddArgument("runId", report.Id))
    .Show();
```

### 4. Close-to-Tray Override

```csharp
// MainWindow.xaml.cs
protected override void OnClosing(CancelEventArgs e)
{
    if (_schedulerService.HasActiveSchedules)
    {
        e.Cancel = true;
        Hide();
        _trayIcon.Visible = true;
    }
    else
    {
        base.OnClosing(e);
    }
}
```

## Build & Run

```bash
dotnet build src/ReqChecker.App/
dotnet run --project src/ReqChecker.App/
```

## Verification Steps

1. Load a profile → Create a one-time schedule 2 minutes from now → Verify it executes and results appear in history
2. Create a recurring schedule (every 5 minutes) → Verify at least 2 executions
3. Close the window → Verify app minimizes to tray → Verify tray context menu works
4. Schedule a run → Close via tray "Exit" → Reopen app → Verify missed run dialog appears
5. Verify toast notifications appear for both run start and completion
