using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.History;
using Serilog;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.Scheduling;

/// <summary>
/// Background scheduler that uses System.Threading.Timer (15-second interval) to execute
/// scheduled test runs independently of the UI thread.
/// </summary>
public class SchedulerService : ISchedulerService
{
    // ── Events ───────────────────────────────────────────────────────
    public event EventHandler<Schedule>? ScheduleRunStarted;
    public event EventHandler<ScheduleCompletedEventArgs>? ScheduleRunCompleted;

    // ── Dependencies ─────────────────────────────────────────────────
    private readonly ITestRunner _testRunner;
    private readonly IProfileLoader _profileLoader;
    private readonly IHistoryService _historyService;
    private readonly ISchedulePersistenceService _persistence;
    private readonly ScheduleCalculator _calculator;

    // ── State ─────────────────────────────────────────────────────────
    private Timer? _timer;
    private readonly SemaphoreSlim _executionLock = new(1, 1);
    private string? _currentlyExecutingScheduleId;

    private bool _hasActiveSchedules;

    public bool HasActiveSchedules => _hasActiveSchedules;

    public SchedulerService(
        ITestRunner testRunner,
        IProfileLoader profileLoader,
        IHistoryService historyService,
        ISchedulePersistenceService persistence)
    {
        _testRunner = testRunner;
        _profileLoader = profileLoader;
        _historyService = historyService;
        _persistence = persistence;
        _calculator = new ScheduleCalculator();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────

    public async void Start()
    {
        Log.Information("SchedulerService: Starting with 15-second polling interval");
        await RefreshHasActiveSchedulesAsync();
        _timer = new Timer(OnTimerElapsed, null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
    }

    private async Task RefreshHasActiveSchedulesAsync()
    {
        try
        {
            var schedules = await _persistence.GetAllAsync();
            _hasActiveSchedules = schedules.Any(s => s.Status == ScheduleStatus.Active);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "SchedulerService: Failed to refresh active schedules cache");
        }
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
        Log.Information("SchedulerService: Stopped");
    }

    // ── Schedule CRUD ─────────────────────────────────────────────────

    public async Task<List<Schedule>> GetSchedulesAsync()
        => await _persistence.GetAllAsync();

    public async Task<Schedule> CreateScheduleAsync(Schedule schedule)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(schedule.Name))
            throw new ArgumentException("Schedule name is required.");

        if (schedule.ScheduleType == ScheduleType.OneTime)
        {
            if (schedule.ScheduledTime <= DateTime.Now)
                throw new ArgumentException("Scheduled time must be in the future.");
            schedule.NextRunTime = schedule.ScheduledTime;
        }
        else
        {
            ValidateRecurrence(schedule);
            schedule.NextRunTime = _calculator.CalculateNextRunTime(schedule);
        }

        schedule.Id = Guid.NewGuid().ToString();
        schedule.Status = ScheduleStatus.Active;
        schedule.CreatedAt = DateTime.UtcNow;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _persistence.SaveScheduleAsync(schedule);
        await RefreshHasActiveSchedulesAsync();
        Log.Information("SchedulerService: Created schedule '{Name}' (Id={Id})", schedule.Name, schedule.Id);
        return schedule;
    }

    public async Task UpdateScheduleAsync(Schedule schedule)
    {
        if (schedule.ScheduleType == ScheduleType.OneTime)
        {
            if (schedule.ScheduledTime <= DateTime.Now)
                throw new ArgumentException("Scheduled time must be in the future.");
            schedule.NextRunTime = schedule.ScheduledTime;
        }
        else
        {
            ValidateRecurrence(schedule);
            schedule.NextRunTime = _calculator.CalculateNextRunTime(schedule);
        }

        schedule.UpdatedAt = DateTime.UtcNow;
        await _persistence.SaveScheduleAsync(schedule);
    }

    public async Task DeleteScheduleAsync(string scheduleId)
    {
        await _persistence.DeleteScheduleAsync(scheduleId);
        await RefreshHasActiveSchedulesAsync();
        Log.Information("SchedulerService: Deleted schedule Id={Id}", scheduleId);
    }

    public async Task PauseScheduleAsync(string scheduleId)
    {
        var schedules = await _persistence.GetAllAsync();
        var schedule = schedules.FirstOrDefault(s => s.Id == scheduleId)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found.");

        schedule.Status = ScheduleStatus.Paused;
        schedule.NextRunTime = null;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _persistence.SaveScheduleAsync(schedule);
        await RefreshHasActiveSchedulesAsync();
    }

    public async Task ResumeScheduleAsync(string scheduleId)
    {
        var schedules = await _persistence.GetAllAsync();
        var schedule = schedules.FirstOrDefault(s => s.Id == scheduleId)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found.");

        schedule.Status = ScheduleStatus.Active;
        schedule.NextRunTime = _calculator.CalculateNextRunTime(schedule);
        schedule.UpdatedAt = DateTime.UtcNow;
        await _persistence.SaveScheduleAsync(schedule);
        await RefreshHasActiveSchedulesAsync();
    }

    public async Task<List<Schedule>> GetMissedRunsAsync()
    {
        var now = DateTime.Now;
        var schedules = await _persistence.GetAllAsync();
        var missed = new List<Schedule>();

        foreach (var schedule in schedules.Where(s => s.Status == ScheduleStatus.Active))
        {
            if (schedule.NextRunTime.HasValue && schedule.NextRunTime.Value < now)
            {
                missed.Add(schedule);
                // Mark as missed (will recalculate next run time after acknowledgement)
                schedule.Status = ScheduleStatus.Missed;
                schedule.UpdatedAt = DateTime.UtcNow;
                await _persistence.SaveScheduleAsync(schedule);

                await _persistence.SaveExecutionRecordAsync(new ScheduleExecutionRecord
                {
                    ScheduleId = schedule.Id,
                    ExecutionTime = schedule.NextRunTime.Value,
                    Outcome = ScheduleOutcome.Missed
                });
            }
        }

        return missed;
    }

    public async Task RunNowAsync(string scheduleId)
    {
        var schedules = await _persistence.GetAllAsync();
        var schedule = schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule == null) return;

        // Re-activate before running
        schedule.Status = ScheduleStatus.Active;
        await _persistence.SaveScheduleAsync(schedule);

        await ExecuteScheduleAsync(schedule);
    }

    // ── Timer callback ────────────────────────────────────────────────

    private async void OnTimerElapsed(object? state)
    {
        var now = DateTime.Now;

        try
        {
            var schedules = await _persistence.GetAllAsync();
            var due = schedules
                .Where(s => s.Status == ScheduleStatus.Active
                         && s.NextRunTime.HasValue
                         && s.NextRunTime.Value <= now)
                .OrderBy(s => s.CreatedAt) // creation order for overlapping
                .ToList();

            foreach (var schedule in due)
            {
                // Overlap prevention
                if (_currentlyExecutingScheduleId != null)
                {
                    Log.Information("SchedulerService: Skipping schedule '{Name}' — previous run still in progress", schedule.Name);
                    await _persistence.SaveExecutionRecordAsync(new ScheduleExecutionRecord
                    {
                        ScheduleId = schedule.Id,
                        ExecutionTime = DateTime.UtcNow,
                        Outcome = ScheduleOutcome.Skipped
                    });
                    continue;
                }

                await ExecuteScheduleAsync(schedule);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SchedulerService: Unhandled error in timer callback");
        }
    }

    // ── Execution ─────────────────────────────────────────────────────

    private async Task ExecuteScheduleAsync(Schedule schedule)
    {
        if (!await _executionLock.WaitAsync(0))
        {
            Log.Information("SchedulerService: Skipping '{Name}' — execution lock held", schedule.Name);
            return;
        }

        _currentlyExecutingScheduleId = schedule.Id;
        Log.Information("SchedulerService: Starting scheduled run for '{Name}'", schedule.Name);
        ScheduleRunStarted?.Invoke(this, schedule);

        // Save the original credential callback so we can restore it after the scheduled run
        var sequentialRunner = _testRunner as ReqChecker.Infrastructure.Execution.SequentialTestRunner;
        var originalPrompt = sequentialRunner?.PromptForCredentials;

        try
        {
            // Load the profile associated with this schedule
            ProfileModel profile;
            try
            {
                profile = await _profileLoader.LoadFromFileAsync(schedule.ProfileFilePath);
            }
            catch (FileNotFoundException)
            {
                Log.Warning("SchedulerService: Profile not found for schedule '{Name}': {Path}", schedule.Name, schedule.ProfileFilePath);
                await RecordFailure(schedule, $"Profile file not found: {schedule.ProfileFilePath}");
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SchedulerService: Failed to load profile for schedule '{Name}'", schedule.Name);
                await RecordFailure(schedule, ex.Message);
                return;
            }

            // Tag the profile run with the schedule name
            var runSettings = new RunSettings();

            // Use no-op credential callback — scheduled runs are unattended
            if (sequentialRunner != null)
            {
                sequentialRunner.PromptForCredentials = (_, __, ___) =>
                    Task.FromResult<(string?, string?, bool)>((null, null, false));
            }

            // Execute tests
            var progress = new Progress<TestResult>(r =>
                Log.Debug("SchedulerService: [{Schedule}] Test '{Name}': {Status}", schedule.Name, r.DisplayName, r.Status));

            RunReport report;
            using var cts = new CancellationTokenSource(TimeSpan.FromHours(2));
            report = await _testRunner.RunTestsAsync(profile, progress, cts.Token, runSettings);

            // Tag with schedule name
            report.ScheduleName = schedule.Name;

            // Save to history
            await _historyService.SaveRunAsync(report);

            // Update schedule state
            schedule.LastRunTime = DateTime.Now;
            schedule.LastRunOutcome = ScheduleOutcome.Completed;

            if (schedule.ScheduleType == ScheduleType.OneTime)
            {
                schedule.Status = ScheduleStatus.Completed;
                schedule.NextRunTime = null;
            }
            else
            {
                // Check end date expiration
                if (schedule.Recurrence?.EndDate.HasValue == true &&
                    DateTime.Now.Date > schedule.Recurrence.EndDate.Value.Date)
                {
                    schedule.Status = ScheduleStatus.Expired;
                    schedule.NextRunTime = null;
                }
                else
                {
                    schedule.NextRunTime = _calculator.CalculateNextRunTime(schedule);
                }
            }

            schedule.UpdatedAt = DateTime.UtcNow;
            await _persistence.SaveScheduleAsync(schedule);
            await RefreshHasActiveSchedulesAsync();

            // Save execution record
            var passCount = report.Summary?.Passed ?? report.Results.Count(r => r.Status == TestStatus.Pass);
            var totalCount = report.Results.Count;

            await _persistence.SaveExecutionRecordAsync(new ScheduleExecutionRecord
            {
                ScheduleId = schedule.Id,
                ExecutionTime = DateTime.UtcNow,
                Outcome = ScheduleOutcome.Completed,
                HistoryRunId = report.RunId
            });

            Log.Information("SchedulerService: Completed '{Name}' — {Pass}/{Total} passed", schedule.Name, passCount, totalCount);
            ScheduleRunCompleted?.Invoke(this, new ScheduleCompletedEventArgs(schedule, passCount, totalCount, report.RunId));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SchedulerService: Error executing schedule '{Name}'", schedule.Name);
            await RecordFailure(schedule, ex.Message);
        }
        finally
        {
            // Restore the original credential callback
            if (sequentialRunner != null)
                sequentialRunner.PromptForCredentials = originalPrompt;

            _currentlyExecutingScheduleId = null;
            _executionLock.Release();
        }
    }

    private async Task RecordFailure(Schedule schedule, string errorMessage)
    {
        schedule.LastRunOutcome = ScheduleOutcome.Failed;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _persistence.SaveScheduleAsync(schedule);

        await _persistence.SaveExecutionRecordAsync(new ScheduleExecutionRecord
        {
            ScheduleId = schedule.Id,
            ExecutionTime = DateTime.UtcNow,
            Outcome = ScheduleOutcome.Failed,
            ErrorMessage = errorMessage
        });
    }

    private static void ValidateRecurrence(Schedule schedule)
    {
        var r = schedule.Recurrence;
        if (r == null)
            throw new ArgumentException("Recurrence pattern is required for recurring schedules.");

        if (r.FrequencyType == RecurrenceType.CustomInterval)
        {
            var unit = r.IntervalUnit ?? IntervalUnit.Minutes;
            var minMinutes = unit switch
            {
                IntervalUnit.Minutes => r.IntervalValue,
                IntervalUnit.Hours => r.IntervalValue * 60,
                IntervalUnit.Days => r.IntervalValue * 60 * 24,
                _ => r.IntervalValue
            };

            if (minMinutes < 5)
                throw new ArgumentException("Minimum recurrence interval is 5 minutes.");
        }

        if (r.FrequencyType == RecurrenceType.Weekly &&
            (r.DaysOfWeek == null || r.DaysOfWeek.Count == 0))
        {
            throw new ArgumentException("At least one day must be selected for weekly recurrence.");
        }
    }
}
