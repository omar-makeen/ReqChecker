using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Interfaces;
using ReqChecker.App.Services;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for test list view.
/// </summary>
public partial class TestListViewModel : ObservableObject, IDisposable
{
    private readonly IAppState _appState;
    private readonly NavigationService _navigationService;
    private readonly ITestRunner _testRunner;

    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private TestDefinition? _selectedTest;

    [ObservableProperty]
    private DialogService? _dialogService;

    [ObservableProperty]
    private bool _isRunning;

    public TestListViewModel(IAppState appState, NavigationService navigationService, ITestRunner testRunner)
    {
        _appState = appState;
        _navigationService = navigationService;
        _testRunner = testRunner;

        // Get current profile from shared state
        CurrentProfile = _appState.CurrentProfile;

        // Subscribe to profile changes
        _appState.CurrentProfileChanged += OnCurrentProfileChanged;
    }

    private void OnCurrentProfileChanged(object? sender, EventArgs e)
    {
        CurrentProfile = _appState.CurrentProfile;
    }

    /// <summary>
    /// Runs all tests in current profile.
    /// </summary>
    [RelayCommand]
    private void RunAllTests()
    {
        if (CurrentProfile == null)
        {
            return;
        }

        // Navigate to run progress view - it will handle the actual execution
        _navigationService.NavigateToRunProgress();
    }

    /// <summary>
    /// Navigates to test configuration for the selected test.
    /// </summary>
    [RelayCommand]
    private void NavigateToTestConfig(TestDefinition? test)
    {
        if (CurrentProfile == null || test == null)
            return;

        // Navigate to TestConfigView
        _navigationService.NavigateToTestConfig(test);
    }

    /// <summary>
    /// Navigates to the profile selector view.
    /// </summary>
    [RelayCommand]
    private void NavigateToProfiles()
    {
        _navigationService.NavigateToProfileSelector();
    }

    /// <summary>
    /// Disposes resources and unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        _appState.CurrentProfileChanged -= OnCurrentProfileChanged;
    }
}
