using ReqChecker.Core.Models;

namespace ReqChecker.App.Services;

/// <summary>
/// Holds shared application state accessible across ViewModels.
/// </summary>
public class AppState : IAppState
{
    /// <inheritdoc />
    public RunReport? LastRunReport { get; private set; }

    /// <inheritdoc />
    public string LogsPath { get; private set; } = string.Empty;

    /// <inheritdoc />
    public Profile? CurrentProfile { get; private set; }

    /// <inheritdoc />
    public event EventHandler? LastRunReportChanged;

    /// <inheritdoc />
    public event EventHandler? CurrentProfileChanged;

    /// <inheritdoc />
    public void SetLastRunReport(RunReport report)
    {
        LastRunReport = report;
        LastRunReportChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SetLogsPath(string path)
    {
        LogsPath = path;
    }

    /// <inheritdoc />
    public void SetCurrentProfile(Profile profile)
    {
        CurrentProfile = profile;
        CurrentProfileChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Interface for shared application state.
/// </summary>
public interface IAppState
{
    /// <summary>
    /// Gets the last run report.
    /// </summary>
    RunReport? LastRunReport { get; }

    /// <summary>
    /// Gets the logs directory path.
    /// </summary>
    string LogsPath { get; }

    /// <summary>
    /// Gets the currently selected profile.
    /// </summary>
    Profile? CurrentProfile { get; }

    /// <summary>
    /// Event raised when last run report changes.
    /// </summary>
    event EventHandler? LastRunReportChanged;

    /// <summary>
    /// Event raised when current profile changes.
    /// </summary>
    event EventHandler? CurrentProfileChanged;

    /// <summary>
    /// Sets the last run report.
    /// </summary>
    void SetLastRunReport(RunReport report);

    /// <summary>
    /// Sets the logs directory path.
    /// </summary>
    void SetLogsPath(string path);

    /// <summary>
    /// Sets the current profile.
    /// </summary>
    void SetCurrentProfile(Profile profile);
}
