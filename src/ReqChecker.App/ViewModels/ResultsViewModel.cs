using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Export;
using Serilog;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for results view.
/// </summary>
public partial class ResultsViewModel : ObservableObject
{
    [ObservableProperty]
    private RunReport? _report;

    [ObservableProperty]
    private Services.NavigationService? _navigationService;

    [ObservableProperty]
    private Services.DialogService? _dialogService;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isStatusError;

    [ObservableProperty]
    private bool _isExporting;

    private readonly JsonExporter _jsonExporter;
    private readonly CsvExporter _csvExporter;

    public ResultsViewModel(JsonExporter jsonExporter, CsvExporter csvExporter)
    {
        _jsonExporter = jsonExporter;
        _csvExporter = csvExporter;
    }

    /// <summary>
    /// Navigates back to the test list view.
    /// </summary>
    [RelayCommand]
    private void NavigateToTestList()
    {
        NavigationService?.NavigateToTestList();
    }

    /// <summary>
    /// Exports the test results to JSON format.
    /// </summary>
    [RelayCommand]
    private async Task ExportToJsonAsync()
    {
        if (DialogService == null || Report == null)
        {
            return;
        }

        var filePath = DialogService.SaveFileDialog(
            $"report-{Report.RunId}.json",
            "JSON Files (*.json)|*.json|All Files (*.*)|*.*");

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        await ExportAsync(() => _jsonExporter.ExportAsync(Report, filePath), filePath);
    }

    /// <summary>
    /// Exports the test results to CSV format.
    /// </summary>
    [RelayCommand]
    private async Task ExportToCsvAsync()
    {
        if (DialogService == null || Report == null)
        {
            return;
        }

        var filePath = DialogService.SaveFileDialog(
            $"report-{Report.RunId}.csv",
            "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*");

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        await ExportAsync(() => _csvExporter.ExportAsync(Report, filePath), filePath);
    }

    private async Task ExportAsync(Func<Task> exportAction, string filePath)
    {
        IsExporting = true;
        StatusMessage = null;
        IsStatusError = false;

        try
        {
            await exportAction();
            StatusMessage = $"Exported to {Path.GetFileName(filePath)}";
            IsStatusError = false;
            Log.Information("Export successful: {FilePath}", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = "Export failed: Access denied. Check file permissions.";
            IsStatusError = true;
            Log.Error(ex, "Export failed due to access denied: {FilePath}", filePath);
        }
        catch (IOException ex)
        {
            StatusMessage = "Export failed: File may be in use or disk is full.";
            IsStatusError = true;
            Log.Error(ex, "Export failed due to IO error: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
            IsStatusError = true;
            Log.Error(ex, "Export failed: {FilePath}", filePath);
        }
        finally
        {
            IsExporting = false;
        }
    }
}
