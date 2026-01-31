using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Export;
using ReqChecker.App.Services;
using Serilog;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// Filter options for test results.
/// </summary>
public enum ResultsFilter
{
    All,
    Passed,
    Failed,
    Skipped
}

/// <summary>
/// View model for results view.
/// </summary>
public partial class ResultsViewModel : ObservableObject
{
    [ObservableProperty]
    private RunReport? _report;

    [ObservableProperty]
    private ResultsFilter _activeFilter = ResultsFilter.All;

    [ObservableProperty]
    private ICollectionView? _filteredResults;

    [ObservableProperty]
    private NavigationService? _navigationService;

    [ObservableProperty]
    private DialogService? _dialogService;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isStatusError;

    [ObservableProperty]
    private bool _isExporting;

    /// <summary>
    /// Gets whether export is available (when Report is set).
    /// </summary>
    public bool CanExport => Report != null;

    private readonly JsonExporter _jsonExporter;
    private readonly CsvExporter _csvExporter;
    private readonly IAppState _appState;

    public ResultsViewModel(
        JsonExporter jsonExporter,
        CsvExporter csvExporter,
        IAppState appState,
        NavigationService? navigationService = null,
        DialogService? dialogService = null)
    {
        _jsonExporter = jsonExporter;
        _csvExporter = csvExporter;
        _appState = appState;
        NavigationService = navigationService;
        DialogService = dialogService;
    }

    /// <summary>
    /// Called when Report property changes. Stores the report in app state.
    /// </summary>
    partial void OnReportChanged(RunReport? value)
    {
        if (value != null)
        {
            _appState.SetLastRunReport(value);
            SetupFilteredResults();
        }
    }

    /// <summary>
    /// Called when ActiveFilter changes. Updates the filtered results.
    /// </summary>
    partial void OnActiveFilterChanged(ResultsFilter value)
    {
        FilteredResults?.Refresh();
    }

    private void SetupFilteredResults()
    {
        if (Report?.Results == null)
        {
            FilteredResults = null;
            return;
        }

        var view = CollectionViewSource.GetDefaultView(Report.Results);
        view.Filter = FilterTestResult;
        FilteredResults = view;
    }

    private bool FilterTestResult(object obj)
    {
        if (obj is not TestResult result)
            return false;

        return ActiveFilter switch
        {
            ResultsFilter.All => true,
            ResultsFilter.Passed => result.Status == TestStatus.Pass,
            ResultsFilter.Failed => result.Status == TestStatus.Fail,
            ResultsFilter.Skipped => result.Status == TestStatus.Skipped,
            _ => true
        };
    }

    /// <summary>
    /// Sets the active filter.
    /// </summary>
    [RelayCommand]
    private void SetFilter(ResultsFilter filter)
    {
        ActiveFilter = filter;
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
