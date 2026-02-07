using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Interfaces;
using ReqChecker.App.Services;
using System.ComponentModel;
using Wpf.Ui.Controls;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// Main view model for application shell with sidebar navigation state.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IPreferencesService _preferencesService;
    private readonly IAppState _appState;

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

    private PropertyChangedEventHandler? _themeHandler;

    public MainViewModel(IPreferencesService preferencesService, IAppState appState)
    {
        _preferencesService = preferencesService;
        _appState = appState;
        _isSidebarExpanded = _preferencesService.SidebarExpanded;

        // Get current profile from shared state
        CurrentProfile = _appState.CurrentProfile;

        // Subscribe to profile changes
        _appState.CurrentProfileChanged += OnCurrentProfileChanged;

        // Initialize theme handler as a stored field to enable proper unsubscription
        _themeHandler = (s, e) =>
        {
            if (e.PropertyName == nameof(ThemeService.CurrentTheme))
            {
                OnPropertyChanged(nameof(ThemeLabel));
                OnPropertyChanged(nameof(ThemeIcon));
            }
        };
    }

    private void OnCurrentProfileChanged(object? sender, EventArgs e)
    {
        CurrentProfile = _appState.CurrentProfile;
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

    /// <summary>
    /// Gets the theme icon based on current theme.
    /// </summary>
    public SymbolRegular ThemeIcon => ThemeService?.CurrentTheme == AppTheme.Dark
        ? SymbolRegular.WeatherSunny24
        : SymbolRegular.WeatherMoon24;

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
        // Unsubscribe from old theme service if it exists
        if (ThemeService != null && _themeHandler != null)
        {
            ThemeService.PropertyChanged -= _themeHandler;
        }

        // Subscribe to new theme service if it exists
        if (value != null && _themeHandler != null)
        {
            value.PropertyChanged += _themeHandler;
        }

        OnPropertyChanged(nameof(ThemeLabel));
        OnPropertyChanged(nameof(ThemeIcon));
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

    /// <summary>
    /// Disposes resources and unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        _appState.CurrentProfileChanged -= OnCurrentProfileChanged;

        // Unsubscribe from theme service property changed
        if (ThemeService != null && _themeHandler != null)
        {
            ThemeService.PropertyChanged -= _themeHandler;
        }
    }
}
