using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Interfaces;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// Main view model for the application.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private ITestRunner? _testRunner;

    [ObservableProperty]
    private Services.NavigationService? _navigationService;

    [ObservableProperty]
    private Services.DialogService? _dialogService;

    [RelayCommand]
    private void NavigateToProfiles()
    {
        // TODO: Implement navigation
    }

    [RelayCommand]
    private void NavigateToTests()
    {
        // TODO: Implement navigation
    }

    [RelayCommand]
    private void NavigateToResults()
    {
        // TODO: Implement navigation
    }

    [RelayCommand]
    private void NavigateToDiagnostics()
    {
        // TODO: Implement navigation
    }
}
