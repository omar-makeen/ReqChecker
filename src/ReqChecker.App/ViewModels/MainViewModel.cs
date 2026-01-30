using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Interfaces;
using ReqChecker.App.Services;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// Main view model for application.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private ITestRunner? _testRunner;

    [ObservableProperty]
    private NavigationService? _navigationService;

    [ObservableProperty]
    private DialogService? _dialogService;

    [ObservableProperty]
    private ThemeService? _themeService;

    [RelayCommand]
    private void NavigateToProfiles()
    {
        NavigationService?.NavigateToProfileSelector();
    }

    [RelayCommand]
    private void NavigateToTests()
    {
        NavigationService?.NavigateToTestList();
    }

    [RelayCommand]
    private void NavigateToResults()
    {
        NavigationService?.NavigateToResults();
    }

    [RelayCommand]
    private void NavigateToDiagnostics()
    {
        NavigationService?.NavigateToDiagnostics();
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeService?.ToggleTheme();
    }
}
