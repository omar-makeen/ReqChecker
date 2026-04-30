using CommunityToolkit.Mvvm.ComponentModel;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using System.IO;

namespace ReqChecker.App.ViewModels;

public partial class ProfileListItemViewModel : ObservableObject
{
    public Profile Profile { get; }
    public string Name { get; }
    public string SourceLabel { get; }
    public string TestCountLabel { get; }
    public string? SchemaVersionLabel { get; }
    public string? ModifiedLabel { get; }
    public DateTime? LastModifiedUtc { get; }
    public bool IsRecommended { get; }
    public string AccessibleName { get; }

    /// <summary>
    /// Absolute path to the underlying JSON file for user-source profiles. Null for bundled profiles.
    /// Drives row-action visibility (delete / open file location are hidden for bundled rows).
    /// </summary>
    public string? SourceFilePath { get; }

    [ObservableProperty]
    private bool _isActive;

    public ProfileListItemViewModel(Profile profile, string? sourceFilePath, bool isRecommended)
    {
        Profile = profile;
        Name = profile.Name;
        SourceFilePath = sourceFilePath;
        SourceLabel = profile.Source == ProfileSource.Bundled ? "Bundled" : "User";
        IsRecommended = isRecommended;
        AccessibleName = isRecommended ? $"{profile.Name} (recommended)" : profile.Name;

        var count = profile.Tests?.Count ?? 0;
        TestCountLabel = count == 1 ? "1 test" : $"{count} tests";

        SchemaVersionLabel = profile.SchemaVersion >= 1 ? $"v{profile.SchemaVersion}" : null;

        if (sourceFilePath != null && File.Exists(sourceFilePath))
        {
            LastModifiedUtc = File.GetLastWriteTimeUtc(sourceFilePath);
            ModifiedLabel = $"modified {FormatFriendlyDate(LastModifiedUtc.Value)}";
        }
        else
        {
            LastModifiedUtc = null;
            ModifiedLabel = null;
        }
    }

    private static string FormatFriendlyDate(DateTime utcDate)
    {
        var localDate = utcDate.ToLocalTime().Date;
        var today = DateTime.Today;
        var diffDays = (today - localDate).Days;

        if (diffDays < 0)
            return localDate.ToString("MMM d, yyyy");
        if (diffDays == 0)
            return "today";
        if (diffDays == 1)
            return "yesterday";
        if (diffDays < 30)
            return $"{diffDays} days ago";
        return localDate.ToString("MMM d, yyyy");
    }
}
