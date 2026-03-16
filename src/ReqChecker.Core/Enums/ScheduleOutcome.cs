namespace ReqChecker.Core.Enums;

/// <summary>
/// Result of a single schedule execution attempt.
/// </summary>
public enum ScheduleOutcome
{
    /// <summary>Tests ran successfully (individual tests may still have failed).</summary>
    Completed,

    /// <summary>Skipped due to an overlapping execution still in progress.</summary>
    Skipped,

    /// <summary>Application was not running at the scheduled time.</summary>
    Missed,

    /// <summary>Execution error such as profile not found or unhandled exception.</summary>
    Failed
}
