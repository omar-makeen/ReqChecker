namespace ReqChecker.App.Services;

/// <summary>
/// Interface for managing user preferences persistence.
/// </summary>
public interface IPreferencesService
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
    /// Loads preferences from storage.
    /// </summary>
    void Load();

    /// <summary>
    /// Saves current preferences to storage.
    /// </summary>
    void Save();
}
