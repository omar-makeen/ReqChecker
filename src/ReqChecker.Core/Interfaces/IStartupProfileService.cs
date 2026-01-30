using ReqChecker.Core.Models;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Service for detecting and loading startup profiles.
/// </summary>
public interface IStartupProfileService
{
    /// <summary>
    /// Attempts to load a startup profile from the application directory.
    /// </summary>
    /// <returns>Result containing the profile or error information.</returns>
    Task<StartupProfileResult> TryLoadStartupProfileAsync();

    /// <summary>
    /// Gets the expected path for the startup profile.
    /// </summary>
    string GetStartupProfilePath();
}
