using ReqChecker.Core.Enums;

namespace ReqChecker.Core.Models;

/// <summary>
/// Outcome of a single test execution.
/// </summary>
public class TestResult
{
    /// <summary>
    /// Test definition ID.
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Test type identifier.
    /// </summary>
    public string TestType { get; set; } = string.Empty;

    /// <summary>
    /// User-facing test name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Pass, Fail, or Skipped.
    /// </summary>
    public TestStatus Status { get; set; }

    /// <summary>
    /// Test start timestamp.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Test end timestamp.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Execution time.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Number of execution attempts (with retries).
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Captured data.
    /// </summary>
    public TestEvidence Evidence { get; set; } = new();

    /// <summary>
    /// Error details if failed.
    /// </summary>
    public TestError? Error { get; set; }

    /// <summary>
    /// User-friendly result message.
    /// </summary>
    public string HumanSummary { get; set; } = string.Empty;

    /// <summary>
    /// Detailed technical output.
    /// </summary>
    public string TechnicalDetails { get; set; } = string.Empty;
}
