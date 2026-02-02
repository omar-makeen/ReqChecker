namespace ReqChecker.Core.Models;

/// <summary>
/// Represents the persisted history collection.
/// </summary>
public class HistoryStore
{
    /// <summary>
    /// Schema version for future migrations.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// All historical runs (reuses existing RunReport model).
    /// </summary>
    public List<RunReport> Runs { get; set; } = new();
}
