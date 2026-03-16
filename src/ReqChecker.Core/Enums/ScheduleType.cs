namespace ReqChecker.Core.Enums;

/// <summary>
/// Defines whether a schedule runs once or repeatedly.
/// </summary>
public enum ScheduleType
{
    /// <summary>Runs once at the specified date and time.</summary>
    OneTime,

    /// <summary>Repeats according to a recurrence pattern.</summary>
    Recurring
}
