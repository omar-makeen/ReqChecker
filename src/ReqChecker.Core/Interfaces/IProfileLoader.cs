using ReqChecker.Core.Models;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Loads profiles from files or embedded resources.
/// </summary>
public interface IProfileLoader
{
    /// <summary>
    /// Loads a profile from a file path.
    /// </summary>
    /// <param name="filePath">The path to the profile JSON file.</param>
    /// <returns>The loaded profile.</returns>
    Task<Profile> LoadFromFileAsync(string filePath);

    /// <summary>
    /// Loads a profile from an embedded resource stream.
    /// </summary>
    /// <param name="stream">The stream containing the profile JSON.</param>
    /// <returns>The loaded profile.</returns>
    Task<Profile> LoadFromStreamAsync(Stream stream);
}
