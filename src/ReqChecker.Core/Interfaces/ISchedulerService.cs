using ReqChecker.Core.Models;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Manages the scheduling lifecycle: creation, execution, and missed-run detection.
/// </summary>
public interface ISchedulerService
{
    /// <summary>Fired when a scheduled run begins.</summary>
    event EventHandler<Schedule>? ScheduleRunStarted;

    /// <summary>Fired when a scheduled run completes.</summary>
    event EventHandler<ScheduleCompletedEventArgs>? ScheduleRunCompleted;

    /// <summary>Starts the background scheduling timer.</summary>
    void Start();

    /// <summary>Stops the background scheduling timer.</summary>
    void Stop();

    /// <summary>Returns all schedules.</summary>
    Task<List<Schedule>> GetSchedulesAsync();

    /// <summary>Creates and persists a new schedule.</summary>
    Task<Schedule> CreateScheduleAsync(Schedule schedule);

    /// <summary>Updates an existing schedule and recalculates its next run time.</summary>
    Task UpdateScheduleAsync(Schedule schedule);

    /// <summary>Deletes a schedule by ID.</summary>
    Task DeleteScheduleAsync(string scheduleId);

    /// <summary>Pauses a schedule; clears its next run time.</summary>
    Task PauseScheduleAsync(string scheduleId);

    /// <summary>Resumes a paused schedule; recalculates its next run time.</summary>
    Task ResumeScheduleAsync(string scheduleId);

    /// <summary>Returns schedules that were due while the application was closed.</summary>
    Task<List<Schedule>> GetMissedRunsAsync();

    /// <summary>Immediately executes a schedule's tests (used for missed run "Run Now").</summary>
    Task RunNowAsync(string scheduleId);

    /// <summary>Whether any Active schedules exist.</summary>
    bool HasActiveSchedules { get; }
}

/// <summary>
/// Event data for a completed scheduled run.
/// </summary>
public class ScheduleCompletedEventArgs : EventArgs
{
    public Schedule Schedule { get; }
    public int PassCount { get; }
    public int TotalCount { get; }
    public string RunId { get; }

    public ScheduleCompletedEventArgs(Schedule schedule, int passCount, int totalCount, string runId)
    {
        Schedule = schedule;
        PassCount = passCount;
        TotalCount = totalCount;
        RunId = runId;
    }
}
