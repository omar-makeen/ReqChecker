namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Sends Windows toast notifications for scheduled run events.
/// </summary>
public interface IToastNotificationService
{
    /// <summary>Shows a toast when a scheduled run starts.</summary>
    /// <param name="scheduleName">Name of the schedule that started.</param>
    void ShowRunStarted(string scheduleName);

    /// <summary>Shows a toast when a scheduled run completes.</summary>
    /// <param name="scheduleName">Name of the schedule that completed.</param>
    /// <param name="passCount">Number of tests that passed.</param>
    /// <param name="totalCount">Total number of tests executed.</param>
    /// <param name="runId">Run ID for the "View Results" action.</param>
    void ShowRunCompleted(string scheduleName, int passCount, int totalCount, string runId);
}
