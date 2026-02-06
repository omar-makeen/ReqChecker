using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.App.Services;
using ReqChecker.Infrastructure.History;
using System.Collections.ObjectModel;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for the run progress view.
/// </summary>
public partial class RunProgressViewModel : ObservableObject
{
    private readonly IAppState _appState;
    private readonly ITestRunner _testRunner;
    private readonly NavigationService _navigationService;
    private readonly IPreferencesService _preferencesService;
    private readonly IHistoryService _historyService;

    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private int _currentTestIndex;

    [ObservableProperty]
    private int _totalTests;

    [ObservableProperty]
    private int _completedTests;

    /// <summary>
    /// Gets the progress percentage (0-100).
    /// </summary>
    public double ProgressPercentage => TotalTests > 0 ? (double)(CompletedTests + FailedTests + SkippedTests) / TotalTests * 100 : 0;

    partial void OnCompletedTestsChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressPercentage));
    }

    partial void OnFailedTestsChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressPercentage));
    }

    partial void OnSkippedTestsChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressPercentage));
    }

    partial void OnTotalTestsChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressPercentage));
        OnPropertyChanged(nameof(HeaderSubtitle));
    }

    partial void OnCurrentTestIndexChanged(int value)
    {
        OnPropertyChanged(nameof(HeaderSubtitle));
    }

    partial void OnIsRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(IsTestRunning));
        OnPropertyChanged(nameof(HeaderSubtitle));
    }

    partial void OnIsCompleteChanged(bool value)
    {
        OnPropertyChanged(nameof(IsTestRunning));
        OnPropertyChanged(nameof(HeaderSubtitle));
    }

    partial void OnIsCancellingChanged(bool value) => OnPropertyChanged(nameof(IsTestRunning));

    [ObservableProperty]
    private int _failedTests;

    [ObservableProperty]
    private int _skippedTests;

    /// <summary>
    /// Gets whether there are any test results to display.
    /// </summary>
    public bool HasResults => TestResults.Count > 0;

    /// <summary>
    /// Gets whether a test is actively executing (not just IsRunning).
    /// </summary>
    public bool IsTestRunning => IsRunning && !IsComplete && !IsCancelling;

    /// <summary>
    /// Gets the header subtitle showing current execution status.
    /// </summary>
    public string HeaderSubtitle => IsComplete
        ? $"{TotalTests} tests completed"
        : IsRunning
            ? $"Running {CurrentTestIndex + 1} of {TotalTests} tests"
            : "Ready to run";

    [ObservableProperty]
    private TestStatus? _currentStatus;

    [ObservableProperty]
    private string? _currentTestName;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private bool _isCancelling;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private ObservableCollection<TestResult> _testResults = new();

    [ObservableProperty]
    private RunReport? _runReport;

    public RunProgressViewModel(IAppState appState, ITestRunner testRunner, NavigationService navigationService, IPreferencesService preferencesService, IHistoryService historyService)
    {
        _appState = appState;
        _testRunner = testRunner;
        _navigationService = navigationService;
        _preferencesService = preferencesService;
        _historyService = historyService;

        // Get current profile from shared state
        CurrentProfile = _appState.CurrentProfile;

        if (CurrentProfile != null)
        {
            TotalTests = CurrentProfile.Tests.Count;
        }
    }

    /// <summary>
    /// Starts the test execution.
    /// </summary>
    public async Task StartTestsAsync()
    {
        if (CurrentProfile == null || IsRunning)
        {
            return;
        }

        IsRunning = true;
        IsComplete = false;
        Cts = new CancellationTokenSource();

        // Reset counters
        CompletedTests = 0;
        FailedTests = 0;
        SkippedTests = 0;
        CurrentTestIndex = 0;
        TestResults.Clear();
        CurrentTestName = "Preparing...";

        try
        {
            var progress = new Progress<TestResult>(OnTestCompleted);
            var runSettings = new RunSettings(); // Uses default 500ms delay
            RunReport = await _testRunner.RunTestsAsync(CurrentProfile, progress, Cts.Token, runSettings);
        }
        catch (OperationCanceledException)
        {
            // User cancelled - this is expected
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            Serilog.Log.Error(ex, "Error running tests");
        }
        finally
        {
            OnCompletion();
        }
    }

    private void OnTestCompleted(TestResult result)
    {
        // Update on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            TestResults.Add(result);
            CurrentTestIndex++;
            // Set CurrentTestName to NEXT test (if any) instead of completed test
            CurrentTestName = CurrentTestIndex < TotalTests
                ? CurrentProfile?.Tests[CurrentTestIndex].DisplayName
                : null;
            CurrentStatus = result.Status;

            switch (result.Status)
            {
                case TestStatus.Pass:
                    CompletedTests++;
                    break;
                case TestStatus.Fail:
                    FailedTests++;
                    break;
                case TestStatus.Skipped:
                    SkippedTests++;
                    break;
            }

            // Notify that HasResults has changed
            OnPropertyChanged(nameof(HasResults));
        });
    }

    /// <summary>
    /// Atomically updates all final state properties when test execution completes.
    /// </summary>
    private void OnCompletion()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            IsRunning = false;
            IsComplete = true;
            IsCancelling = false;
            CurrentTestName = null;
        });

        // Fire-and-forget with proper error handling
        _ = SaveToHistoryAsync();
    }

    /// <summary>
    /// Saves the run report to history asynchronously.
    /// </summary>
    private async Task SaveToHistoryAsync()
    {
        if (RunReport == null)
        {
            return;
        }

        try
        {
            await _historyService.SaveRunAsync(RunReport);
            Serilog.Log.Information("Auto-saved run {RunId} to history", RunReport.RunId);
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to auto-save run {RunId} to history", RunReport.RunId);
        }
    }

    /// <summary>
    /// Cancels the test run.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        if (!IsRunning || IsCancelling)
            return;

        IsCancelling = true;
        Cts?.Cancel();
    }

    /// <summary>
    /// Navigates back to the test list view.
    /// </summary>
    [RelayCommand]
    private void NavigateToTestList()
    {
        _navigationService.NavigateToTestList();
    }

    /// <summary>
    /// Navigates to the results view.
    /// </summary>
    [RelayCommand]
    private void ViewResults()
    {
        if (RunReport != null)
        {
            _appState.SetLastRunReport(RunReport);
            _navigationService.NavigateToResults();
        }
    }
}
