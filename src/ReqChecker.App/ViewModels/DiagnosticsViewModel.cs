using System.Diagnostics;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.App.Services;
using Serilog;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for diagnostics view.
/// </summary>
public partial class DiagnosticsViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isStatusError;

    [ObservableProperty]
    private bool _hasLastRun;

    [ObservableProperty]
    private string? _lastRunSummary;

    [ObservableProperty]
    private string? _machineInfoSummary;

    [ObservableProperty]
    private RunReport? _lastRunReport;

    private readonly IClipboardService _clipboardService;
    private readonly IAppState _appState;

    public DiagnosticsViewModel(IClipboardService clipboardService, IAppState appState)
    {
        _clipboardService = clipboardService;
        _appState = appState;

        // Subscribe to changes
        _appState.LastRunReportChanged += OnLastRunReportChanged;

        // Initialize from current state
        RefreshFromAppState();
    }

    private void OnLastRunReportChanged(object? sender, EventArgs e)
    {
        RefreshFromAppState();
    }

    /// <summary>
    /// Refreshes the view model from current app state.
    /// </summary>
    public void RefreshFromAppState()
    {
        LastRunReport = _appState.LastRunReport;
        HasLastRun = _appState.LastRunReport != null;
        UpdateSummaries();
    }

    private void UpdateSummaries()
    {
        if (LastRunReport == null)
        {
            LastRunSummary = "No test runs have been performed yet.";
            MachineInfoSummary = null;
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Run ID: {LastRunReport.RunId}");
        sb.AppendLine($"Profile: {LastRunReport.ProfileName}");
        sb.AppendLine($"Started: {LastRunReport.StartTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Duration: {LastRunReport.Duration.TotalSeconds:F2} seconds");
        sb.AppendLine();
        sb.AppendLine($"Total Tests: {LastRunReport.Summary.TotalTests}");
        sb.AppendLine($"Passed: {LastRunReport.Summary.Passed}");
        sb.AppendLine($"Failed: {LastRunReport.Summary.Failed}");
        sb.AppendLine($"Skipped: {LastRunReport.Summary.Skipped}");
        sb.AppendLine($"Pass Rate: {LastRunReport.Summary.PassRate:F1}%");

        LastRunSummary = sb.ToString();

        sb.Clear();
        sb.AppendLine($"Hostname: {LastRunReport.MachineInfo.Hostname}");
        sb.AppendLine($"OS: {LastRunReport.MachineInfo.OsVersion} (Build {LastRunReport.MachineInfo.OsBuild})");
        sb.AppendLine($"CPU Cores: {LastRunReport.MachineInfo.ProcessorCount}");
        sb.AppendLine($"Total RAM: {LastRunReport.MachineInfo.TotalMemoryMB} MB");
        sb.AppendLine($"User: {LastRunReport.MachineInfo.UserName}");
        sb.AppendLine($"Elevated: {(LastRunReport.MachineInfo.IsElevated ? "Yes" : "No")}");
        sb.AppendLine($"Network Interfaces: {LastRunReport.MachineInfo.NetworkInterfaces.Count}");

        MachineInfoSummary = sb.ToString();
    }

    /// <summary>
    /// Opens the logs folder in Windows Explorer.
    /// </summary>
    [RelayCommand]
    private void OpenLogsFolder()
    {
        var logsPath = _appState.LogsPath;

        if (string.IsNullOrEmpty(logsPath))
        {
            StatusMessage = "Logs path is not configured.";
            IsStatusError = true;
            Log.Warning("OpenLogsFolder called but logs path is null");
            return;
        }

        if (!Directory.Exists(logsPath))
        {
            StatusMessage = $"Logs folder does not exist: {logsPath}";
            IsStatusError = true;
            Log.Warning("Logs folder does not exist: {Path}", logsPath);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = logsPath,
                UseShellExecute = true
            });

            StatusMessage = "Opened logs folder.";
            IsStatusError = false;
            Log.Information("Opened logs folder: {Path}", logsPath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open logs folder: {ex.Message}";
            IsStatusError = true;
            Log.Error(ex, "Failed to open logs folder: {Path}", logsPath);
        }
    }

    /// <summary>
    /// Copies diagnostic details to the clipboard.
    /// </summary>
    [RelayCommand]
    private void CopyDetails()
    {
        if (LastRunReport == null)
        {
            StatusMessage = "No diagnostic details available to copy.";
            IsStatusError = true;
            Log.Warning("CopyDetails called but no last run report is available");
            return;
        }

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ReqChecker Diagnostics ===");
            sb.AppendLine();
            sb.AppendLine(LastRunSummary);
            sb.AppendLine();
            sb.AppendLine("=== Machine Information ===");
            sb.AppendLine();
            sb.AppendLine(MachineInfoSummary);
            sb.AppendLine();
            sb.AppendLine("=== Network Interfaces ===");
            sb.AppendLine();

            foreach (var nic in LastRunReport.MachineInfo.NetworkInterfaces)
            {
                sb.AppendLine($"- {nic.Name}");
                sb.AppendLine($"  Description: {nic.Description}");
                sb.AppendLine($"  Status: {nic.Status}");
                sb.AppendLine($"  MAC: {nic.MacAddress}");
                if (nic.IpAddresses.Count > 0)
                {
                    sb.AppendLine($"  IPs: {string.Join(", ", nic.IpAddresses)}");
                }
                sb.AppendLine();
            }

            var details = sb.ToString();
            _clipboardService.SetText(details);

            StatusMessage = "Diagnostic details copied to clipboard.";
            IsStatusError = false;
            Log.Information("Copied diagnostic details to clipboard for run {RunId}", LastRunReport.RunId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to copy details: {ex.Message}";
            IsStatusError = true;
            Log.Error(ex, "Failed to copy diagnostic details");
        }
    }
}
