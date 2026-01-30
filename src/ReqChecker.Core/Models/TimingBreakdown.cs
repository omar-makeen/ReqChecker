namespace ReqChecker.Core.Models;

/// <summary>
/// Detailed timing information for test execution.
/// </summary>
public class TimingBreakdown
{
    /// <summary>
    /// Connection/setup time in milliseconds.
    /// </summary>
    public int? ConnectMs { get; set; }

    /// <summary>
    /// Actual execution time in milliseconds.
    /// </summary>
    public int? ExecuteMs { get; set; }

    /// <summary>
    /// Total time in milliseconds.
    /// </summary>
    public int TotalMs { get; set; }
}
