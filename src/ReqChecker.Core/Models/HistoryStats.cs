namespace ReqChecker.Core.Models;

/// <summary>
/// Storage statistics for test history.
/// </summary>
public record HistoryStats
{
    /// <summary>
    /// Total number of runs in history.
    /// </summary>
    public int TotalRuns { get; init; }

    /// <summary>
    /// Approximate file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// Oldest run timestamp (null if empty).
    /// </summary>
    public DateTimeOffset? OldestRun { get; init; }

    /// <summary>
    /// Newest run timestamp (null if empty).
    /// </summary>
    public DateTimeOffset? NewestRun { get; init; }

    /// <summary>
    /// Creates an empty stats instance.
    /// </summary>
    public static HistoryStats Empty => new()
    {
        TotalRuns = 0,
        FileSizeBytes = 0,
        OldestRun = null,
        NewestRun = null
    };
}
