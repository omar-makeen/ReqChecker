namespace ReqChecker.Core.Enums;

/// <summary>
/// Frequency pattern for recurring schedules.
/// </summary>
public enum RecurrenceType
{
    /// <summary>Repeats every N hours.</summary>
    Hourly,

    /// <summary>Repeats every day at a specific time.</summary>
    Daily,

    /// <summary>Repeats on selected days of the week at a specific time.</summary>
    Weekly,

    /// <summary>Repeats every N minutes, hours, or days.</summary>
    CustomInterval
}
