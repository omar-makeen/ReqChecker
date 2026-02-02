using ReqChecker.Core.Enums;

namespace ReqChecker.Core.Models;

/// <summary>
/// Computed at runtime for flaky test detection. Not persisted.
/// </summary>
public class TestTrendData
{
    /// <summary>
    /// Test identifier.
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string TestName { get; set; } = string.Empty;

    /// <summary>
    /// Profile scope.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Number of runs analyzed.
    /// </summary>
    public int TotalRuns { get; set; }

    /// <summary>
    /// Times passed.
    /// </summary>
    public int PassCount { get; set; }

    /// <summary>
    /// Times failed.
    /// </summary>
    public int FailCount { get; set; }

    /// <summary>
    /// Times skipped.
    /// </summary>
    public int SkipCount { get; set; }

    /// <summary>
    /// PassCount / (TotalRuns - SkipCount).
    /// </summary>
    public double PassRate { get; set; }

    /// <summary>
    /// Has both pass and fail results.
    /// </summary>
    public bool IsFlaky { get; set; }

    /// <summary>
    /// Last N results for display.
    /// </summary>
    public List<TestStatus> RecentResults { get; set; } = new();
}
