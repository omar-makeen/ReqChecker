using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

    /// <summary>
    /// Gets whether a report is available (not null).
    /// </summary>
    public bool HasReport => Report != null;

    /// <summary>
    /// Gets whether the report contains any failed tests.
    /// </summary>
    public bool HasFailedTests => Report?.Results?.Any(r => r.Status == TestStatus.Fail) ?? false;

    /// <summary>
    /// Gets the command to re-run failed tests.
    /// </summary>
    public IRelayCommand RerunFailedTestsCommand { get; }

    private readonly JsonExporter _jsonExporter;
    private readonly CsvExporter _csvExporter;
    private readonly PdfExporter _pdfExporter;
    private readonly IAppState _appState;

    public ResultsViewModel(
        JsonExporter jsonExporter,
        CsvExporter csvExporter,
        PdfExporter pdfExporter,
        IAppState appState,
        NavigationService? navigationService = null,
        DialogService? dialogService = null)
    {
        _jsonExporter = jsonExporter;
        _csvExporter = csvExporter;
        _pdfExporter = pdfExporter;
        _appState = appState;
        NavigationService = navigationService;
        DialogService = dialogService;

        RerunFailedTestsCommand = new RelayCommand(ExecuteRerunFailedTests, CanExecuteRerunFailedTests);
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
        // Notify that CanExport, HasReport, and HasFailedTests have changed (depend on Report)
        OnPropertyChanged(nameof(CanExport));
        OnPropertyChanged(nameof(HasReport));
        OnPropertyChanged(nameof(HasFailedTests));
        // Update the re-run failed tests command's CanExecute state
        RerunFailedTestsCommand.NotifyCanExecuteChanged();
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

    /// <summary>
    /// Exports test results to PDF format.
    /// </summary>
    [RelayCommand]
    private async Task ExportToPdfAsync()
    {
        if (DialogService == null || Report == null)
        {
            return;
        }

        var filePath = DialogService.SaveFileDialog(
            $"{Report.ProfileName}-{Report.StartTime:yyyy-MM-dd}.pdf",
            "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*");

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        await ExportAsync(() => _pdfExporter.ExportAsync(Report, filePath), filePath);
    }

    private async Task ExportAsync(Func<Task> exportAction, string filePath)
    {
        IsExporting = true;
        StatusMessage = null;
        IsStatusError = false;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var format = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant();

        Log.Information("Export started: {Format}, {TestCount} tests",
            format, Report?.Results?.Count ?? 0);

        try
        {
            await exportAction();

            var fileInfo = new FileInfo(filePath);
            var fileSize = fileInfo.Length;

            StatusMessage = $"Exported to {Path.GetFileName(filePath)}";
            IsStatusError = false;

            Log.Information("Export completed: {Format}, {FilePath}, {FileSize} bytes, {Duration}ms",
                format, filePath, fileSize, sw.ElapsedMilliseconds);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = "Export failed: Access denied. Check file permissions.";
            IsStatusError = true;
            Log.Error(ex, "Export failed: {Format}, {FilePath}, Outcome: AccessDenied",
                format, filePath);
        }
        catch (IOException ex)
        {
            StatusMessage = "Export failed: File may be in use or disk is full.";
            IsStatusError = true;
            Log.Error(ex, "Export failed: {Format}, {FilePath}, Outcome: IOError",
                format, filePath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
            IsStatusError = true;
            Log.Error(ex, "Export failed: {Format}, {FilePath}, Outcome: Error",
                format, filePath);
        }
        finally
        {
            sw.Stop();
            IsExporting = false;
        }
    }

    /// <summary>
    /// Executes the re-run failed tests command.
    /// </summary>
    private void ExecuteRerunFailedTests()
    {
        if (Report?.Results == null)
            return;

        // Collect failed test IDs
        var failedIds = Report.Results
            .Where(r => r.Status == TestStatus.Fail)
            .Select(r => r.TestId);

        // Collect dependency-skipped test IDs
        var depSkippedIds = Report.Results
            .Where(r => r.Status == TestStatus.Skipped && r.Error?.Category == ErrorCategory.Dependency)
            .Select(r => r.TestId);

        // Union both sets (deduplicated)
        var rerunIds = failedIds.Union(depSkippedIds).ToList();

        // Store the selected test IDs in app state
        _appState.SetSelectedTestIds(rerunIds);

        // Navigate to Run Progress page
        NavigationService?.NavigateToRunProgress();

        Log.Information("Re-run failed tests initiated: {TestCount} tests (failed: {FailedCount}, dep-skipped: {DepSkippedCount})",
            rerunIds.Count, failedIds.Count(), depSkippedIds.Count());
    }

    /// <summary>
    /// Determines whether the re-run failed tests command can execute.
    /// </summary>
    private bool CanExecuteRerunFailedTests()
    {
        return HasFailedTests && NavigationService != null;
    }
}
