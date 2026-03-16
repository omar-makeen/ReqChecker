using ReqChecker.Core.Enums;

namespace ReqChecker.Core.Models;

/// <summary>
/// Defines the repeat behavior for a recurring schedule.
/// </summary>
public class RecurrencePattern
{
    /// <summary>The frequency pattern type.</summary>
    public RecurrenceType FrequencyType { get; set; }

    /// <summary>
    /// Repeat every N units.
    /// For Hourly: hours between runs.
    /// For CustomInterval: value in IntervalUnit units (minimum 5 minutes).
    /// For Daily/Weekly: always 1.
    /// </summary>
    public int IntervalValue { get; set; } = 1;

    /// <summary>Time unit for CustomInterval. Null for other frequency types.</summary>
    public IntervalUnit? IntervalUnit { get; set; }

    /// <summary>Selected days of the week for Weekly recurrence.</summary>
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();

    /// <summary>Time of day for Daily and Weekly recurrence.</summary>
    public TimeSpan TimeOfDay { get; set; }

    /// <summary>Optional end date. Schedule expires after this date.</summary>
    public DateTime? EndDate { get; set; }
}
