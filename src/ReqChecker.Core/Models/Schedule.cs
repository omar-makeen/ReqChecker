using ReqChecker.Core.Enums;

namespace ReqChecker.Core.Models;

/// <summary>
/// Represents a user-defined scheduled test run.
/// </summary>
public class Schedule
{
    /// <summary>Unique identifier (GUID).</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>User-given descriptive name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Absolute path to the profile JSON file (auto-loaded at execution time).</summary>
    public string ProfileFilePath { get; set; } = string.Empty;

    /// <summary>Display name of the profile (captured at creation time).</summary>
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>Whether this is a one-time or recurring schedule.</summary>
    public ScheduleType ScheduleType { get; set; }

    /// <summary>
    /// For one-time: the exact execution date/time.
    /// For recurring: the first run date/time.
    /// </summary>
    public DateTime ScheduledTime { get; set; }

    /// <summary>Recurrence configuration. Null for one-time schedules.</summary>
    public RecurrencePattern? Recurrence { get; set; }

    /// <summary>Current lifecycle state.</summary>
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Active;

    /// <summary>Computed next execution time. Null when Completed/Expired.</summary>
    public DateTime? NextRunTime { get; set; }

    /// <summary>When the schedule last executed. Null if never run.</summary>
    public DateTime? LastRunTime { get; set; }

    /// <summary>Outcome of the most recent execution. Null if never run.</summary>
    public ScheduleOutcome? LastRunOutcome { get; set; }

    /// <summary>When the schedule was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the schedule was last modified (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
