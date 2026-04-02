using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.App.Services;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using Serilog;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// ViewModel for the Schedules page. Manages the list of schedules and all CRUD operations.
/// </summary>
public partial class SchedulesViewModel : ObservableObject, IDisposable
{
    private readonly ISchedulerService _schedulerService;
    private readonly NavigationService _navigationService;
    private readonly EventHandler<ScheduleCompletedEventArgs> _runCompletedHandler;

    [ObservableProperty]
    private ObservableCollection<ScheduleItemViewModel> _schedules = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isStatusError;

    public bool IsEmpty => Schedules.Count == 0;

    public SchedulesViewModel(ISchedulerService schedulerService, NavigationService navigationService)
    {
        _schedulerService = schedulerService;
        _navigationService = navigationService;

        Schedules.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsEmpty));

        // Refresh when a scheduled run completes
        _runCompletedHandler = (_, _) => RefreshSchedulesOnUiThread();
        _schedulerService.ScheduleRunCompleted += _runCompletedHandler;
    }

    public void Dispose()
    {
        _schedulerService.ScheduleRunCompleted -= _runCompletedHandler;
    }

    // ── Load ──────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadSchedulesAsync()
    {
        IsLoading = true;
        StatusMessage = null;

        try
        {
            var list = await _schedulerService.GetSchedulesAsync();
            Schedules.Clear();
            foreach (var s in list.OrderByDescending(s => s.CreatedAt))
                Schedules.Add(new ScheduleItemViewModel(s));
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to load schedules";
            IsStatusError = true;
            Log.Error(ex, "Failed to load schedules");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Create ────────────────────────────────────────────────────────

    [RelayCommand]
    private void CreateSchedule()
    {
        // Signal View to open dialog — View subscribes to this event
        CreateScheduleRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>View subscribes to open the CreateScheduleDialog.</summary>
    public event EventHandler? CreateScheduleRequested;

    // ── Edit ──────────────────────────────────────────────────────────

    [RelayCommand]
    private void EditSchedule(ScheduleItemViewModel? item)
    {
        if (item == null) return;
        EditScheduleRequested?.Invoke(this, item.Schedule);
    }

    /// <summary>View subscribes to open the CreateScheduleDialog in edit mode.</summary>
    public event EventHandler<Schedule>? EditScheduleRequested;

    // ── Pause / Resume ────────────────────────────────────────────────

    [RelayCommand]
    private async Task TogglePauseAsync(ScheduleItemViewModel? item)
    {
        if (item == null) return;

        try
        {
            if (item.Schedule.Status == ScheduleStatus.Active)
            {
                await _schedulerService.PauseScheduleAsync(item.Schedule.Id);
            }
            else if (item.Schedule.Status == ScheduleStatus.Paused)
            {
                await _schedulerService.ResumeScheduleAsync(item.Schedule.Id);
            }

            await LoadSchedulesAsync();
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to update schedule: {ex.Message}";
            IsStatusError = true;
            Log.Error(ex, "Failed to toggle pause for schedule {Id}", item.Schedule.Id);
        }
    }

    // ── Delete ────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task DeleteScheduleAsync(ScheduleItemViewModel? item)
    {
        if (item == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Delete schedule \"{item.Schedule.Name}\"? This cannot be undone.",
            "Delete Schedule",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            await _schedulerService.DeleteScheduleAsync(item.Schedule.Id);
            Schedules.Remove(item);
            StatusMessage = $"Deleted \"{item.Schedule.Name}\"";
            IsStatusError = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to delete schedule: {ex.Message}";
            IsStatusError = true;
            Log.Error(ex, "Failed to delete schedule {Id}", item.Schedule.Id);
        }
    }

    // ── Navigation ────────────────────────────────────────────────────

    [RelayCommand]
    private void NavigateBack() => _navigationService.NavigateToTestList();

    // ── Helpers ───────────────────────────────────────────────────────

    private void RefreshSchedulesOnUiThread()
    {
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            await LoadSchedulesAsync());
    }
}

/// <summary>
/// Wraps a Schedule for display in the schedules list, providing computed display strings.
/// </summary>
public partial class ScheduleItemViewModel : ObservableObject
{
    public Schedule Schedule { get; }

    public ScheduleItemViewModel(Schedule schedule)
    {
        Schedule = schedule;
    }

    public string Name => Schedule.Name;
    public string ProfileName => Schedule.ProfileName;
    public ScheduleStatus Status => Schedule.Status;
    public ScheduleType ScheduleType => Schedule.ScheduleType;

    public string TypeLabel => Schedule.ScheduleType == ScheduleType.OneTime ? "One-time" : "Recurring";

    public string StatusLabel => Schedule.Status switch
    {
        ScheduleStatus.Active => "Active",
        ScheduleStatus.Paused => "Paused",
        ScheduleStatus.Completed => "Completed",
        ScheduleStatus.Expired => "Expired",
        ScheduleStatus.Missed => "Missed",
        _ => Schedule.Status.ToString()
    };

    public string NextRunLabel => Schedule.NextRunTime.HasValue
        ? Schedule.NextRunTime.Value.ToString("MMM d, yyyy h:mm tt")
        : Schedule.Status == ScheduleStatus.Completed ? "—" : "Not scheduled";

    public string LastRunLabel => Schedule.LastRunTime.HasValue
        ? Schedule.LastRunTime.Value.ToString("MMM d, yyyy h:mm tt")
        : "Never";

    public bool CanPause => Schedule.Status == ScheduleStatus.Active;
    public bool CanResume => Schedule.Status == ScheduleStatus.Paused;
    public bool CanPauseOrResume => CanPause || CanResume;
    public bool CanEdit => Schedule.Status != ScheduleStatus.Completed && Schedule.Status != ScheduleStatus.Expired;

    public string PauseResumeLabel => CanResume ? "Resume" : "Pause";

    public void Refresh()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusLabel));
        OnPropertyChanged(nameof(NextRunLabel));
        OnPropertyChanged(nameof(CanPause));
        OnPropertyChanged(nameof(CanResume));
        OnPropertyChanged(nameof(CanPauseOrResume));
        OnPropertyChanged(nameof(PauseResumeLabel));
    }
}
