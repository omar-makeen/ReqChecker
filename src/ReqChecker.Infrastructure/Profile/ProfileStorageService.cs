using ReqChecker.Core.Interfaces;
using Serilog;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ReqChecker.Infrastructure.Profile;

/// <summary>
/// Service for profile storage operations.
/// </summary>
public class ProfileStorageService : IProfileStorageService
{
    private readonly string _profilesPath;

    /// <summary>
    /// Initializes a new instance of the ProfileStorageService class.
    /// </summary>
    public ProfileStorageService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _profilesPath = Path.Combine(appDataPath, "ReqChecker", "Profiles");
    }

    /// <summary>
    /// Gets the path to the user profiles directory.
    /// </summary>
    /// <returns>The full path to the user profiles directory.</returns>
    public string GetUserProfilesDirectory()
    {
        return _profilesPath;
    }

    /// <summary>
    /// Ensures the user profiles directory exists.
    /// </summary>
    public void EnsureUserProfilesDirectory()
    {
        if (!Directory.Exists(_profilesPath))
        {
            Directory.CreateDirectory(_profilesPath);
            Log.Information("Created user profiles directory: {ProfilesPath}", _profilesPath);
        }
    }

    /// <summary>
    /// Gets all profile file paths in the user profiles directory.
    /// </summary>
    /// <returns>An array of profile file paths.</returns>
    public string[] GetProfileFilePaths()
    {
        if (!Directory.Exists(_profilesPath))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(_profilesPath, "*.json");
    }

    /// <summary>
    /// Copies a profile file to the user profiles directory.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    /// <returns>The destination file path.</returns>
    public string CopyProfileToUserDirectory(string sourcePath, bool overwrite = true)
    {
        if (string.IsNullOrEmpty(sourcePath))
        {
            throw new ArgumentException("Source path cannot be null or empty.", nameof(sourcePath));
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Source file not found.", sourcePath);
        }

        EnsureUserProfilesDirectory();

        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.Combine(_profilesPath, fileName);

        File.Copy(sourcePath, destinationPath, overwrite);
        Log.Information("Copied profile to user directory: {DestinationPath}", destinationPath);

        return destinationPath;
    }

    /// <summary>
    /// Validates a file name to prevent path traversal attacks.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the file name is invalid.</exception>
    private void ValidateFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        // Check if the file name contains path separators
        if (Path.GetFileName(fileName) != fileName)
        {
            throw new ArgumentException("Invalid filename: must not contain path separators.", nameof(fileName));
        }

        // Check for path traversal sequences
        if (fileName.Contains(".."))
        {
            throw new ArgumentException("Invalid filename: must not contain path traversal sequences.", nameof(fileName));
        }
    }

    /// <summary>
    /// Deletes a profile file from the user profiles directory.
    /// </summary>
    /// <param name="fileName">The file name to delete.</param>
    public void DeleteProfile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        // Validate file name before processing
        ValidateFileName(fileName);

        var filePath = Path.Combine(_profilesPath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Log.Information("Deleted profile: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Checks if a profile file exists in the user profiles directory.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    public bool ProfileExists(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        // Validate file name before processing
        ValidateFileName(fileName);

        var filePath = Path.Combine(_profilesPath, fileName);
        return File.Exists(filePath);
    }

    /// <inheritdoc />
    public async Task<string> SaveProfileWithRegeneratedIdAsync(string sourcePath, string newId)
    {
        if (string.IsNullOrEmpty(sourcePath))
        {
            throw new ArgumentException("Source path cannot be null or empty.", nameof(sourcePath));
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Source file not found.", sourcePath);
        }

        if (string.IsNullOrEmpty(newId))
        {
            throw new ArgumentException("New ID cannot be null or empty.", nameof(newId));
        }

        EnsureUserProfilesDirectory();

        var json = await File.ReadAllTextAsync(sourcePath);
        var node = JsonNode.Parse(json);
        if (node is JsonObject obj)
        {
            // Remove any existing 'id' key (case-insensitive) so the canonical "id" is the only one.
            foreach (var key in obj.Select(kvp => kvp.Key).ToList())
            {
                if (string.Equals(key, "id", StringComparison.OrdinalIgnoreCase))
                {
                    obj.Remove(key);
                }
            }
            obj["id"] = newId;
        }

        // Filename = original-base + short-id suffix, so the new copy never overwrites an
        // existing user file (e.g., when the colliding profile was itself imported under the
        // same source filename). The 8-hex-char suffix is derived from the regenerated GUID.
        var originalBase = Path.GetFileNameWithoutExtension(sourcePath);
        if (string.IsNullOrEmpty(originalBase))
        {
            originalBase = "profile";
        }
        var hexId = newId.Replace("-", string.Empty);
        var shortId = hexId.Substring(0, Math.Min(8, hexId.Length));

        var destPath = Path.Combine(_profilesPath, $"{originalBase}-{shortId}.json");
        // Defensive: if a file at this path somehow already exists, append -2, -3, … to find
        // a free slot. Avoids any silent overwrite.
        var dedupeIndex = 2;
        while (File.Exists(destPath))
        {
            destPath = Path.Combine(_profilesPath, $"{originalBase}-{shortId}-{dedupeIndex}.json");
            dedupeIndex++;
        }

        var serialized = node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? json;
        await File.WriteAllTextAsync(destPath, serialized);

        Log.Information("Saved profile with regenerated ID to user directory: {DestPath}", destPath);
        return destPath;
    }
}
