using ReqChecker.Core.Models;

namespace ReqChecker.Infrastructure.History;

/// <summary>
/// Interface for history operations.
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// Load history from disk (call on startup).
    /// </summary>
    /// <returns>List of historical runs.</returns>
    Task<List<RunReport>> LoadHistoryAsync();

    /// <summary>
    /// Save a new run to history (call after test completion).
    /// </summary>
    /// <param name="report">The run report to save.</param>
    Task SaveRunAsync(RunReport report);

    /// <summary>
    /// Delete a specific run.
    /// </summary>
    /// <param name="runId">The run ID to delete.</param>
    Task DeleteRunAsync(string runId);

    /// <summary>
    /// Clear all history.
    /// </summary>
    Task ClearHistoryAsync();

    /// <summary>
    /// Get storage statistics.
    /// </summary>
    /// <returns>Storage statistics.</returns>
    HistoryStats GetStats();
}
