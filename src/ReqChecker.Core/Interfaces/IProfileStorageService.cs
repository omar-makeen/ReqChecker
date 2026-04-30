namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Interface for profile storage operations.
/// </summary>
public interface IProfileStorageService
{
    /// <summary>
    /// Gets the path to the user profiles directory.
    /// </summary>
    /// <returns>The full path to the user profiles directory.</returns>
    string GetUserProfilesDirectory();

    /// <summary>
    /// Ensures the user profiles directory exists.
    /// </summary>
    void EnsureUserProfilesDirectory();

    /// <summary>
    /// Gets all profile file paths in the user profiles directory.
    /// </summary>
    /// <returns>An array of profile file paths.</returns>
    string[] GetProfileFilePaths();

    /// <summary>
    /// Copies a profile file to the user profiles directory.
    /// </summary>
    /// <param name="sourcePath">The source file path.</param>
    /// <param name="overwrite">Whether to overwrite existing files.</param>
    /// <returns>The destination file path.</returns>
    string CopyProfileToUserDirectory(string sourcePath, bool overwrite = true);

    /// <summary>
    /// Deletes a profile file from the user profiles directory.
    /// </summary>
    /// <param name="fileName">The file name to delete.</param>
    void DeleteProfile(string fileName);

    /// <summary>
    /// Checks if a profile file exists in the user profiles directory.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    bool ProfileExists(string fileName);

    /// <summary>
    /// Reads the JSON at <paramref name="sourcePath"/>, replaces its top-level "id" field
    /// with <paramref name="newId"/> (case-insensitive match preserved), and writes the
    /// result to the user profiles directory using the source file's name. Used to resolve
    /// profile-ID collisions at import time without modifying the user's original source file.
    /// </summary>
    /// <param name="sourcePath">Absolute path to the source JSON file.</param>
    /// <param name="newId">The new GUID to write into the file's id field.</param>
    /// <returns>The absolute destination path inside the user profiles directory.</returns>
    Task<string> SaveProfileWithRegeneratedIdAsync(string sourcePath, string newId);
}
