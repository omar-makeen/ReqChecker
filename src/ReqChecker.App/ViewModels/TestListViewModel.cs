using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Interfaces;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for test list view.
/// </summary>
public partial class TestListViewModel : ObservableObject
{
    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private ITestRunner? _testRunner;

    [ObservableProperty]
    private Services.NavigationService? _navigationService;

    [ObservableProperty]
    private Services.DialogService? _dialogService;

    /// <summary>
    /// Runs all tests in current profile.
    /// </summary>
    [RelayCommand]
    private async Task RunAllTestsAsync()
    {
        if (CurrentProfile == null || TestRunner == null)
        {
            return;
        }

        // TODO: Implement test execution and navigation
        await Task.CompletedTask;
    }

    /// <summary>
    /// Navigates to test configuration for the selected test.
    /// </summary>
    [RelayCommand]
    private void NavigateToTestConfig()
    {
        if (CurrentProfile == null)
            return;

        // Navigate to TestConfigView
        NavigationService?.NavigateToTestConfig(CurrentProfile);
    }
}
