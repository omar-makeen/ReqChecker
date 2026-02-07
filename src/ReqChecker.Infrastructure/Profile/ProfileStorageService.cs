using ReqChecker.Core.Interfaces;
using Serilog;
using System.IO;

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
}
