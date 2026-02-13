using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReqChecker.App.Services;

/// <summary>
/// User preferences data transfer object for JSON serialization.
/// </summary>
public class UserPreferences
{
    public string Theme { get; set; } = "Dark";
    public bool SidebarExpanded { get; set; } = true;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Service for managing user preferences with persistence to %APPDATA%.
/// </summary>
public partial class PreferencesService : ObservableObject, IPreferencesService
{
    private static readonly string PreferencesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ReqChecker",
        "preferences.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private bool _suppressSave;

    [ObservableProperty]
    private AppTheme _theme = AppTheme.Dark;

    [ObservableProperty]
    private bool _sidebarExpanded = true;

    public PreferencesService()
    {
        Load();
    }

    /// <summary>
    /// Loads preferences from the JSON file.
    /// </summary>
    public void Load()
    {
        try
        {
            if (File.Exists(PreferencesPath))
            {
                var json = File.ReadAllText(PreferencesPath);
                var prefs = JsonSerializer.Deserialize<UserPreferences>(json, JsonOptions);

                if (prefs != null)
                {
                    if (Enum.TryParse<AppTheme>(prefs.Theme, out var theme))
                    {
                        Theme = theme;
                    }
                    SidebarExpanded = prefs.SidebarExpanded;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error and use defaults
            Serilog.Log.Warning(ex, "Failed to load user preferences, using defaults");
            Theme = AppTheme.Dark;
            SidebarExpanded = true;
        }
    }

    /// <summary>
    /// Saves current preferences to the JSON file.
    /// </summary>
    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(PreferencesPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var prefs = new UserPreferences
            {
                Theme = Theme.ToString(),
                SidebarExpanded = SidebarExpanded,
                LastUpdated = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(prefs, JsonOptions);
            File.WriteAllText(PreferencesPath, json);
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to save user preferences");
        }
    }

    partial void OnThemeChanged(AppTheme value)
    {
        if (!_suppressSave) Save();
    }

    partial void OnSidebarExpandedChanged(bool value)
    {
        if (!_suppressSave) Save();
    }

    /// <summary>
    /// Resets all preferences to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        _suppressSave = true;
        Theme = AppTheme.Dark;
        SidebarExpanded = true;
        _suppressSave = false;
        Save();
    }
}
