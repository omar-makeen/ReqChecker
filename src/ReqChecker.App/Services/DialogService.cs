using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Serilog;

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
    public virtual string? OpenProfileFileDialog()
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
    public virtual string? SaveFileDialog(string defaultFileName, string filter)
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

    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message to display.</param>
    /// <returns>True if user clicks Yes (discard), False if user clicks No (stay).</returns>
    public virtual bool ShowConfirmationDialog(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return result == MessageBoxResult.Yes;
    }

    /// <summary>
    /// Opens the given folder path in Windows Explorer (or the OS default file browser).
    /// </summary>
    /// <param name="path">Absolute folder path. Silently returns if the path is empty or does not exist.</param>
    public virtual void OpenInExplorer(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open path in Explorer: {Path}", path);
        }
    }
}
