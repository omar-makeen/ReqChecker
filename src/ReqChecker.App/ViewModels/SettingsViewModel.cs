using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.App.Services;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// ViewModel for the Settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IPreferencesService _preferencesService;
    private readonly ThemeService _themeService;

    [ObservableProperty]
    private AppTheme _currentTheme;

    [ObservableProperty]
    private string _appVersion;

    public SettingsViewModel(IPreferencesService preferencesService, ThemeService themeService)
    {
        _preferencesService = preferencesService;
        _themeService = themeService;

        // Initialize current theme from ThemeService
        CurrentTheme = _themeService.CurrentTheme;

        // Get application version from assembly
        AppVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Command to select the dark theme.
    /// </summary>
    [RelayCommand]
    private void SelectDarkTheme()
    {
        _themeService.SetTheme(AppTheme.Dark);
        CurrentTheme = AppTheme.Dark;
    }

    /// <summary>
    /// Command to select the light theme.
    /// </summary>
    [RelayCommand]
    private void SelectLightTheme()
    {
        _themeService.SetTheme(AppTheme.Light);
        CurrentTheme = AppTheme.Light;
    }

    /// <summary>
    /// Command to reset all preferences to defaults.
    /// </summary>
    [RelayCommand]
    private void ResetToDefaults()
    {
        var result = System.Windows.MessageBox.Show(
            "Reset all settings to defaults?",
            "Reset Settings",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _preferencesService.ResetToDefaults();
            _themeService.SetTheme(AppTheme.Dark);
            CurrentTheme = AppTheme.Dark;
        }
    }
}
