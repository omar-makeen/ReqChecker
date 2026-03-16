using ReqChecker.Core.Models;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Persists schedule definitions and execution records to storage.
/// </summary>
public interface ISchedulePersistenceService
{
    /// <summary>Loads all schedules from storage.</summary>
    Task<List<Schedule>> GetAllAsync();

    /// <summary>Saves a new or updated schedule.</summary>
    Task SaveScheduleAsync(Schedule schedule);

    /// <summary>Removes a schedule by ID.</summary>
    Task DeleteScheduleAsync(string scheduleId);

    /// <summary>Appends an execution record.</summary>
    Task SaveExecutionRecordAsync(ScheduleExecutionRecord record);

    /// <summary>Returns all execution records for the given schedule ID.</summary>
    Task<List<ScheduleExecutionRecord>> GetExecutionRecordsAsync(string scheduleId);
}
