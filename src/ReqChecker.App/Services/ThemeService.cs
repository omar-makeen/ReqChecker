using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace ReqChecker.App.Services;

/// <summary>
/// Application theme types.
/// </summary>
public enum AppTheme
{
    Light,
    Dark
}

/// <summary>
/// Service for managing application theme with resource dictionary switching.
/// </summary>
public partial class ThemeService : ObservableObject
{
    private readonly IPreferencesService _preferencesService;
    private ResourceDictionary? _currentThemeDictionary;

    [ObservableProperty]
    private AppTheme _currentTheme;

    [ObservableProperty]
    private bool _isReducedMotionEnabled;

    public ThemeService(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;

        // Load theme from preferences
        CurrentTheme = _preferencesService.Theme;

        // Detect reduced motion setting
        DetectReducedMotion();

        // Apply theme on startup
        ApplyTheme();
    }

    /// <summary>
    /// Toggles between light and dark theme.
    /// </summary>
    [RelayCommand]
    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        _preferencesService.Theme = CurrentTheme;
        ApplyTheme();
    }

    /// <summary>
    /// Sets the theme to the specified value.
    /// </summary>
    public void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        _preferencesService.Theme = theme;
        ApplyTheme();
    }

    /// <summary>
    /// Detects if the system has reduced motion enabled.
    /// </summary>
    private void DetectReducedMotion()
    {
        // SystemParameters.ClientAreaAnimation reflects the Windows "Show animations in Windows" setting
        IsReducedMotionEnabled = !SystemParameters.ClientAreaAnimation;
    }

    /// <summary>
    /// Applies the current theme by switching resource dictionaries.
    /// </summary>
    private void ApplyTheme()
    {
        var app = Application.Current;
        if (app == null) return;

        var resources = app.Resources;
        if (resources == null) return;

        try
        {
            // Remove old theme dictionary if exists
            if (_currentThemeDictionary != null && resources.MergedDictionaries.Contains(_currentThemeDictionary))
            {
                resources.MergedDictionaries.Remove(_currentThemeDictionary);
            }

            // Create new theme dictionary
            var themeUri = CurrentTheme == AppTheme.Dark
                ? new Uri("pack://application:,,,/Resources/Styles/Colors.Dark.xaml", UriKind.Absolute)
                : new Uri("pack://application:,,,/Resources/Styles/Colors.Light.xaml", UriKind.Absolute);

            _currentThemeDictionary = new ResourceDictionary { Source = themeUri };

            // Add new theme dictionary at the beginning so it can be overridden by controls
            resources.MergedDictionaries.Insert(0, _currentThemeDictionary);

            // Update WPF-UI theme if available
            UpdateWpfUiTheme();
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to apply theme {Theme}", CurrentTheme);
        }
    }

    /// <summary>
    /// Updates WPF-UI's built-in theme to match our theme.
    /// </summary>
    private void UpdateWpfUiTheme()
    {
        try
        {
            var wpfUiTheme = CurrentTheme == AppTheme.Dark
                ? Wpf.Ui.Appearance.ApplicationTheme.Dark
                : Wpf.Ui.Appearance.ApplicationTheme.Light;

            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(wpfUiTheme);
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to update WPF-UI theme");
        }
    }

    /// <summary>
    /// Gets the animation duration based on reduced motion setting.
    /// Returns TimeSpan.Zero if reduced motion is enabled.
    /// </summary>
    public TimeSpan GetAnimationDuration(TimeSpan normalDuration)
    {
        return IsReducedMotionEnabled ? TimeSpan.Zero : normalDuration;
    }

    /// <summary>
    /// Gets whether decorative animations should be enabled.
    /// Decorative animations are disabled when reduced motion is enabled.
    /// </summary>
    public bool ShouldAnimateDecorative => !IsReducedMotionEnabled;

    /// <summary>
    /// Gets whether essential feedback animations should be enabled.
    /// Essential animations (button press, status changes) are always enabled.
    /// </summary>
    public bool ShouldAnimateEssential => true;
}
