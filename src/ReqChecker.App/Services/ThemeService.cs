using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
    private const int ThemeTransitionDurationMs = 300;

    [ObservableProperty]
    private AppTheme _currentTheme;

    [ObservableProperty]
    private bool _isReducedMotionEnabled;

    [ObservableProperty]
    private bool _isTransitioning;

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
    /// Applies the current theme by switching resource dictionaries with cross-fade transition.
    /// </summary>
    private void ApplyTheme()
    {
        var app = Application.Current;
        if (app == null) return;

        var mainWindow = app.MainWindow;
        var resources = app.Resources;
        if (resources == null) return;

        try
        {
            // Determine if we should animate (not on startup, and reduced motion not enabled)
            bool shouldAnimate = mainWindow != null &&
                                 mainWindow.IsLoaded &&
                                 !IsReducedMotionEnabled &&
                                 _currentThemeDictionary != null;

            if (shouldAnimate)
            {
                ApplyThemeWithTransition(mainWindow!, resources);
            }
            else
            {
                ApplyThemeImmediate(resources);
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to apply theme {Theme}", CurrentTheme);
        }
    }

    /// <summary>
    /// Applies theme immediately without animation (used on startup).
    /// </summary>
    private void ApplyThemeImmediate(ResourceDictionary resources)
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

    /// <summary>
    /// Applies theme with a smooth cross-fade transition.
    /// </summary>
    private void ApplyThemeWithTransition(Window mainWindow, ResourceDictionary resources)
    {
        IsTransitioning = true;

        // Create fade out animation
        var fadeOut = new DoubleAnimation
        {
            From = 1.0,
            To = 0.85,
            Duration = TimeSpan.FromMilliseconds(ThemeTransitionDurationMs / 2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        // Create fade in animation
        var fadeIn = new DoubleAnimation
        {
            From = 0.85,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(ThemeTransitionDurationMs / 2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        // When fade out completes, switch theme and fade in
        fadeOut.Completed += (s, e) =>
        {
            // Switch the theme
            ApplyThemeImmediate(resources);

            // Fade back in
            fadeIn.Completed += (s2, e2) =>
            {
                IsTransitioning = false;
            };

            mainWindow.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        };

        // Start fade out
        mainWindow.BeginAnimation(UIElement.OpacityProperty, fadeOut);
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
