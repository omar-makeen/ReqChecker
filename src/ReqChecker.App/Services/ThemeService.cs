using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.IO;

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
/// Service for managing application theme.
/// </summary>
public partial class ThemeService : ObservableObject
{
    [ObservableProperty]
    private AppTheme _currentTheme;

    private readonly string _settingsPath;

    public ThemeService()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ReqChecker",
            "theme-settings.json");

        LoadTheme();
    }

    /// <summary>
    /// Toggles between light and dark theme.
    /// </summary>
    [RelayCommand]
    public void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        ApplyTheme();
        SaveTheme();
    }

    /// <summary>
    /// Sets the theme to the specified value.
    /// </summary>
    public void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        ApplyTheme();
        SaveTheme();
    }

    private void ApplyTheme()
    {
        var app = Application.Current;
        if (app == null) return;

        var resources = app.Resources;
        if (resources == null) return;

        // Apply theme to application
        resources["ThemeMode"] = CurrentTheme.ToString();

        // Update window backgrounds
        if (app.MainWindow != null)
        {
            app.MainWindow.Background = CurrentTheme == AppTheme.Dark
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 32, 32))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
        }
    }

    private void LoadTheme()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var themeValue = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                if (Enum.TryParse<AppTheme>(themeValue, out var theme))
                {
                    CurrentTheme = theme;
                    ApplyTheme();
                }
            }
        }
        catch
        {
            // Default to light theme if loading fails
            CurrentTheme = AppTheme.Light;
        }
    }

    private void SaveTheme()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(CurrentTheme.ToString());
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently fail on save error
        }
    }
}
