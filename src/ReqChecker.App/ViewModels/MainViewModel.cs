using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Interfaces;
using ReqChecker.App.Services;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// Main view model for application shell with sidebar navigation state.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IPreferencesService _preferencesService;

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

    [ObservableProperty]
    private bool _isSidebarExpanded;

    public MainViewModel(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
        _isSidebarExpanded = _preferencesService.SidebarExpanded;
    }

    /// <summary>
    /// Gets whether a profile is currently loaded.
    /// </summary>
    public bool HasProfile => CurrentProfile != null;

    /// <summary>
    /// Gets the current profile name for display.
    /// </summary>
    public string ProfileName => CurrentProfile?.Name ?? "No profile loaded";

    /// <summary>
    /// Gets the theme toggle label based on current theme.
    /// </summary>
    public string ThemeLabel => ThemeService?.CurrentTheme == AppTheme.Dark ? "Light Mode" : "Dark Mode";

    partial void OnCurrentProfileChanged(Profile? value)
    {
        OnPropertyChanged(nameof(HasProfile));
        OnPropertyChanged(nameof(ProfileName));
    }

    partial void OnIsSidebarExpandedChanged(bool value)
    {
        // Persist sidebar state
        _preferencesService.SidebarExpanded = value;
    }

    partial void OnThemeServiceChanged(ThemeService? value)
    {
        if (value != null)
        {
            value.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ThemeService.CurrentTheme))
                {
                    OnPropertyChanged(nameof(ThemeLabel));
                }
            };
        }
        OnPropertyChanged(nameof(ThemeLabel));
    }

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

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
    }
}
