namespace ReqChecker.Core.Models;

/// <summary>
/// The output of a test execution session.
/// </summary>
public class RunReport
{
    /// <summary>
    /// Unique run identifier.
    /// </summary>
    public string RunId { get; set; } = string.Empty;

    /// <summary>
    /// Source profile ID.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Source profile name.
    /// </summary>
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>
    /// Run start timestamp.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Run end timestamp.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Total execution time.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Environment details.
    /// </summary>
    public MachineInfo MachineInfo { get; set; } = new();

    /// <summary>
    /// Per-test results.
    /// </summary>
    public List<TestResult> Results { get; set; } = new();

    /// <summary>
    /// Aggregate statistics.
    /// </summary>
    public RunSummary Summary { get; set; } = new();
}
