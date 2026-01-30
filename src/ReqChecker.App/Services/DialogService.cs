using Microsoft.Win32;

namespace ReqChecker.App.Services;

/// <summary>
/// Manages dialog interactions.
/// </summary>
public class DialogService
{
    /// <summary>
    /// Initializes dialog service.
    /// </summary>
    public DialogService()
    {
    }

    /// <summary>
    /// Opens a file picker dialog for profile import.
    /// </summary>
    /// <returns>The selected file path, or null if cancelled.</returns>
    public string? OpenProfileFileDialog()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import Profile",
            Filter = "Profile Files (*.json)|*.json|All Files (*.*)|*.*",
            FilterIndex = 1,
            CheckFileExists = true,
            CheckPathExists = true
        };

        var result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }

    /// <summary>
    /// Opens a save file dialog for export.
    /// </summary>
    /// <param name="defaultFileName">The default file name.</param>
    /// <param name="filter">The file filter (e.g., "JSON Files (*.json)|*.json").</param>
    /// <returns>The selected file path, or null if cancelled.</returns>
    public string? SaveFileDialog(string defaultFileName, string filter)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Export",
            FileName = defaultFileName,
            Filter = filter,
            FilterIndex = 1,
            AddExtension = true
        };

        var result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }
}
