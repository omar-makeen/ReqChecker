using System.Windows;

namespace ReqChecker.App.Services;

/// <summary>
/// Service for clipboard operations.
/// </summary>
public class ClipboardService : IClipboardService
{
    /// <summary>
    /// Copies text to the system clipboard.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    public void SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            Clipboard.SetText(text);
        });
    }

    /// <summary>
    /// Gets text from the system clipboard.
    /// </summary>
    /// <returns>The text from the clipboard, or null if unavailable.</returns>
    public string? GetText()
    {
        string? result = null;
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (Clipboard.ContainsText())
            {
                result = Clipboard.GetText();
            }
        });
        return result;
    }

    /// <summary>
    /// Clears the system clipboard.
    /// </summary>
    public void Clear()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Clipboard.Clear();
        });
    }
}

/// <summary>
/// Interface for clipboard operations.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Copies text to the system clipboard.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    void SetText(string text);

    /// <summary>
    /// Gets text from the system clipboard.
    /// </summary>
    /// <returns>The text from the clipboard, or null if unavailable.</returns>
    string? GetText();

    /// <summary>
    /// Clears the system clipboard.
    /// </summary>
    void Clear();
}
