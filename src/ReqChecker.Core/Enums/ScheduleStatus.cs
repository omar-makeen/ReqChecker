namespace ReqChecker.Core.Enums;

/// <summary>
/// Lifecycle state of a schedule.
/// </summary>
public enum ScheduleStatus
{
    /// <summary>Scheduled and waiting for the next run time.</summary>
    Active,

    /// <summary>Paused by the user; will not execute until resumed.</summary>
    Paused,

    /// <summary>One-time schedule that has finished executing.</summary>
    Completed,

    /// <summary>Recurring schedule that has passed its end date.</summary>
    Expired,

    /// <summary>Schedule was due but the application was not running.</summary>
    Missed
}
