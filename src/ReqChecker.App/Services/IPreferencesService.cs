using System.ComponentModel;

namespace ReqChecker.App.Services;

/// <summary>
/// Interface for managing user preferences persistence.
/// </summary>
public interface IPreferencesService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets the current theme preference.
    /// </summary>
    AppTheme Theme { get; set; }

    /// <summary>
    /// Gets or sets whether the sidebar is expanded.
    /// </summary>
    bool SidebarExpanded { get; set; }

    /// <summary>
    /// Gets or sets whether the user has seen the onboarding welcome banner.
    /// </summary>
    bool HasSeenOnboarding { get; set; }

    /// <summary>
    /// Loads preferences from storage.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves current preferences to storage.
    /// </summary>
    void Save();

    /// <summary>
    /// Resets all preferences to their default values.
    /// </summary>
    void ResetToDefaults();
}
