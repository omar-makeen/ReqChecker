using ReqChecker.Core.Enums;

namespace ReqChecker.Core.Models;

/// <summary>
/// Records a single execution attempt of a schedule.
/// </summary>
public class ScheduleExecutionRecord
{
    /// <summary>Unique identifier (GUID).</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Reference to the parent schedule.</summary>
    public string ScheduleId { get; set; } = string.Empty;

    /// <summary>When execution started (UTC).</summary>
    public DateTime ExecutionTime { get; set; } = DateTime.UtcNow;

    /// <summary>Outcome of this execution attempt.</summary>
    public ScheduleOutcome Outcome { get; set; }

    /// <summary>Reference to the RunReport in history. Set when Outcome is Completed.</summary>
    public string? HistoryRunId { get; set; }

    /// <summary>Error details. Set when Outcome is Failed.</summary>
    public string? ErrorMessage { get; set; }
}
