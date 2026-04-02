using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using System.Collections.ObjectModel;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// ViewModel for the Create/Edit Schedule dialog.
/// Supports one-time and recurring schedules, with edit mode for existing schedules.
/// </summary>
public partial class CreateScheduleViewModel : ObservableObject
{
    private readonly ISchedulerService _schedulerService;
    private Schedule? _editingSchedule;

    // ── Bound Properties ──────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _profileFilePath = string.Empty;

    [ObservableProperty]
    private string _profileName = string.Empty;

    // ── Schedule type ─────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOneTime))]
    [NotifyPropertyChangedFor(nameof(IsRecurring))]
    private ScheduleType _scheduleType = ScheduleType.OneTime;

    public bool IsOneTime => ScheduleType == ScheduleType.OneTime;
    public bool IsRecurring => ScheduleType == ScheduleType.Recurring;

    // ── One-time fields ───────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private DateTime _scheduledDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private TimeSpan _scheduledTime = TimeSpan.FromHours(9);

    public int ScheduledHour
    {
        get => ScheduledTime.Hours;
        set { ScheduledTime = new TimeSpan(value, ScheduledTime.Minutes, 0); OnPropertyChanged(); }
    }

    public int ScheduledMinute
    {
        get => ScheduledTime.Minutes;
        set { ScheduledTime = new TimeSpan(ScheduledTime.Hours, value, 0); OnPropertyChanged(); }
    }

    // ── Recurrence fields ─────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomInterval))]
    [NotifyPropertyChangedFor(nameof(IsDaily))]
    [NotifyPropertyChangedFor(nameof(IsWeekly))]
    [NotifyPropertyChangedFor(nameof(IsHourly))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private RecurrenceType _recurrenceType = RecurrenceType.Daily;

    public bool IsHourly => RecurrenceType == RecurrenceType.Hourly;
    public bool IsDaily => RecurrenceType == RecurrenceType.Daily;
    public bool IsWeekly => RecurrenceType == RecurrenceType.Weekly;
    public bool IsCustomInterval => RecurrenceType == RecurrenceType.CustomInterval;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int _intervalValue = 1;

    [ObservableProperty]
    private IntervalUnit _intervalUnit = IntervalUnit.Hours;

    [ObservableProperty]
    private TimeSpan _timeOfDay = TimeSpan.FromHours(9);

    public int TimeOfDayHour
    {
        get => TimeOfDay.Hours;
        set { TimeOfDay = new TimeSpan(value, TimeOfDay.Minutes, 0); OnPropertyChanged(); }
    }

    public int TimeOfDayMinute
    {
        get => TimeOfDay.Minutes;
        set { TimeOfDay = new TimeSpan(TimeOfDay.Hours, value, 0); OnPropertyChanged(); }
    }

    [ObservableProperty]
    private DateTime? _endDate;

    // Day-of-week checkboxes for Weekly recurrence
    public ObservableCollection<DayOfWeekItem> DaysOfWeek { get; } = new(
        Enum.GetValues<DayOfWeek>()
            .OrderBy(d => ((int)d + 6) % 7) // Monday first
            .Select(d => new DayOfWeekItem(d)));

    // ── Error / state ─────────────────────────────────────────────────

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    // ── Result ────────────────────────────────────────────────────────

    /// <summary>Set to true after a successful save; View should close.</summary>
    public bool Saved { get; private set; }

    /// <summary>True when editing an existing schedule; false when creating new.</summary>
    public bool IsEditMode => _editingSchedule != null;

    public string SaveButtonText => IsEditMode ? "Update" : "Create";

    // ── Constructor ───────────────────────────────────────────────────

    public CreateScheduleViewModel(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;

        // Subscribe to each DayOfWeekItem so CanSave re-evaluates when a day is toggled
        foreach (var day in DaysOfWeek)
        {
            day.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(DayOfWeekItem.IsSelected))
                    SaveCommand.NotifyCanExecuteChanged();
            };
        }
    }

    /// <summary>
    /// Pre-populate fields for editing an existing schedule.
    /// </summary>
    public void LoadSchedule(Schedule schedule)
    {
        _editingSchedule = schedule;

        Name = schedule.Name;
        ProfileFilePath = schedule.ProfileFilePath;
        ProfileName = schedule.ProfileName;
        ScheduleType = schedule.ScheduleType;

        if (schedule.ScheduleType == ScheduleType.OneTime)
        {
            ScheduledDate = schedule.ScheduledTime.Date;
            ScheduledTime = schedule.ScheduledTime.TimeOfDay;
            OnPropertyChanged(nameof(ScheduledHour));
            OnPropertyChanged(nameof(ScheduledMinute));
        }
        else if (schedule.Recurrence != null)
        {
            var r = schedule.Recurrence;
            RecurrenceType = r.FrequencyType;
            IntervalValue = r.IntervalValue;
            IntervalUnit = r.IntervalUnit ?? IntervalUnit.Hours;
            TimeOfDay = r.TimeOfDay;
            EndDate = r.EndDate;
            OnPropertyChanged(nameof(TimeOfDayHour));
            OnPropertyChanged(nameof(TimeOfDayMinute));

            if (r.DaysOfWeek != null)
            {
                foreach (var item in DaysOfWeek)
                    item.IsSelected = r.DaysOfWeek.Contains(item.Day);
            }
        }

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(SaveButtonText));
        SaveCommand.NotifyCanExecuteChanged();
    }

    // ── Commands ──────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;
        IsSaving = true;

        try
        {
            var schedule = BuildSchedule();

            if (IsEditMode)
                await _schedulerService.UpdateScheduleAsync(schedule);
            else
                await _schedulerService.CreateScheduleAsync(schedule);

            Saved = true;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to save schedule: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanSave()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (string.IsNullOrWhiteSpace(ProfileFilePath)) return false;
        if (IsSaving) return false;

        if (ScheduleType == ScheduleType.OneTime)
        {
            var scheduled = ScheduledDate.Date.Add(ScheduledTime);
            if (scheduled <= DateTime.Now) return false;
        }
        else
        {
            if (RecurrenceType == RecurrenceType.CustomInterval && IntervalValue < 1) return false;
            if (RecurrenceType == RecurrenceType.Weekly && !DaysOfWeek.Any(d => d.IsSelected)) return false;
        }

        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private Schedule BuildSchedule()
    {
        var schedule = _editingSchedule ?? new Schedule();

        schedule.Name = Name.Trim();
        schedule.ProfileFilePath = ProfileFilePath;
        schedule.ProfileName = ProfileName;
        schedule.ScheduleType = ScheduleType;

        if (ScheduleType == ScheduleType.OneTime)
        {
            schedule.ScheduledTime = ScheduledDate.Date.Add(ScheduledTime);
            schedule.Recurrence = null;
        }
        else
        {
            schedule.Recurrence = new RecurrencePattern
            {
                FrequencyType = RecurrenceType,
                IntervalValue = IntervalValue,
                IntervalUnit = RecurrenceType == RecurrenceType.CustomInterval ? IntervalUnit : null,
                TimeOfDay = TimeOfDay,
                EndDate = EndDate,
                DaysOfWeek = RecurrenceType == RecurrenceType.Weekly
                    ? DaysOfWeek.Where(d => d.IsSelected).Select(d => d.Day).ToList()
                    : new List<DayOfWeek>()
            };
        }

        return schedule;
    }
}

/// <summary>Wraps a DayOfWeek for checkbox binding in the weekly recurrence UI.</summary>
public partial class DayOfWeekItem : ObservableObject
{
    public DayOfWeek Day { get; }
    public string Label => Day.ToString()[..3]; // "Mon", "Tue", etc.

    [ObservableProperty]
    private bool _isSelected;

    public DayOfWeekItem(DayOfWeek day) => Day = day;
}
