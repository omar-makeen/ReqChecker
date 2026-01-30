using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Collections.ObjectModel;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for the run progress view.
/// </summary>
public partial class RunProgressViewModel : ObservableObject
{
    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private int _currentTestIndex;

    [ObservableProperty]
    private int _totalTests;

    [ObservableProperty]
    private int _completedTests;

    [ObservableProperty]
    private int _failedTests;

    [ObservableProperty]
    private int _skippedTests;

    [ObservableProperty]
    private TestStatus? _currentStatus;

    [ObservableProperty]
    private string? _currentTestName;

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private ObservableCollection<TestResult> _testResults = new();

    /// <summary>
    /// Cancels the test run.
    /// </summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        Cts?.Cancel();
        IsComplete = true;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Navigates back to the test list view.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToTestListAsync()
    {
        // TODO: Implement navigation
        await Task.CompletedTask;
    }
}
