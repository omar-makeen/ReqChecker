namespace ReqChecker.Core.Models;

/// <summary>
/// Aggregate statistics for a test run.
/// </summary>
public class RunSummary
{
    /// <summary>
    /// Total number of tests.
    /// </summary>
    public int TotalTests { get; set; }

    /// <summary>
    /// Number of passed tests.
    /// </summary>
    public int Passed { get; set; }

    /// <summary>
    /// Number of failed tests.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Number of skipped tests.
    /// </summary>
    public int Skipped { get; set; }

    /// <summary>
    /// Pass percentage (passed / (passed + failed) * 100).
    /// </summary>
    public double PassRate { get; set; }
}
