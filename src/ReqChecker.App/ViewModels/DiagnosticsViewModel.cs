using System.Diagnostics;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.App.Services;
using ReqChecker.Infrastructure.Platform;
using Serilog;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for diagnostics view.
/// </summary>
public partial class DiagnosticsViewModel : ObservableObject, IDisposable
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

    [ObservableProperty]
    private MachineInfo? _currentMachineInfo;

    partial void OnCurrentMachineInfoChanged(MachineInfo? value) => UpdateSummaries();

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
        }
        else
        {
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
        }

        // MachineInfoSummary now uses CurrentMachineInfo instead of LastRunReport.MachineInfo
        if (CurrentMachineInfo == null)
        {
            MachineInfoSummary = null;
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Hostname: {CurrentMachineInfo.Hostname}");
            sb.AppendLine($"OS: {CurrentMachineInfo.OsVersion} (Build {CurrentMachineInfo.OsBuild})");
            sb.AppendLine($"CPU Cores: {CurrentMachineInfo.ProcessorCount}");
            sb.AppendLine($"Total RAM: {CurrentMachineInfo.TotalMemoryMB} MB");
            sb.AppendLine($"User: {CurrentMachineInfo.UserName}");
            sb.AppendLine($"Elevated: {(CurrentMachineInfo.IsElevated ? "Yes" : "No")}");
            sb.AppendLine($"Network Interfaces: {CurrentMachineInfo.NetworkInterfaces.Count}");

            MachineInfoSummary = sb.ToString();
        }
    }

    /// <summary>
    /// Loads current machine information independent of test execution.
    /// </summary>
    public void LoadMachineInfo()
    {
        try
        {
            CurrentMachineInfo = MachineInfoCollector.Collect();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to collect machine info");
            CurrentMachineInfo = null;
        }
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

            // Use CurrentMachineInfo for network interfaces instead of LastRunReport.MachineInfo
            if (CurrentMachineInfo != null)
            {
                foreach (var nic in CurrentMachineInfo.NetworkInterfaces)
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

    /// <summary>
    /// Disposes resources and unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        _appState.LastRunReportChanged -= OnLastRunReportChanged;
    }
}
