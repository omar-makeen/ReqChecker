using Microsoft.Toolkit.Uwp.Notifications;
using ReqChecker.Core.Interfaces;
using Serilog;

namespace ReqChecker.App.Services;

/// <summary>
/// Sends Windows toast notifications for scheduled run events using Microsoft.Toolkit.Uwp.Notifications.
/// </summary>
public class ToastNotificationService : IToastNotificationService
{
    private const string AppId = "ReqChecker";

    public void ShowRunStarted(string scheduleName)
    {
        try
        {
            new ToastContentBuilder()
                .AddArgument("action", "runStarted")
                .AddArgument("scheduleName", scheduleName)
                .AddText($"Schedule Running: {scheduleName}")
                .AddText("Tests are now running in the background.")
                .Show();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to show run-started toast for '{ScheduleName}'", scheduleName);
        }
    }

    public void ShowRunCompleted(string scheduleName, int passCount, int totalCount, string runId)
    {
        try
        {
            var allPassed = passCount == totalCount;
            var summary = $"{passCount}/{totalCount} tests passed";

            var builder = new ToastContentBuilder()
                .AddArgument("action", "viewResults")
                .AddArgument("runId", runId)
                .AddText($"Schedule Complete: {scheduleName}")
                .AddText(summary)
                .AddButton(new ToastButton()
                    .SetContent("View Results")
                    .AddArgument("action", "viewResults")
                    .AddArgument("runId", runId));

            builder.Show();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to show run-completed toast for '{ScheduleName}'", scheduleName);
        }
    }
}
