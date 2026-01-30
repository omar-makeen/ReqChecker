namespace ReqChecker.Core.Models;

/// <summary>
/// Result of attempting to load a startup profile.
/// </summary>
public class StartupProfileResult
{
    /// <summary>
    /// Whether a startup profile was found and loaded successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The loaded profile, if successful.
    /// </summary>
    public Profile? Profile { get; init; }

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Whether a startup profile file was found (regardless of validity).
    /// </summary>
    public bool FileFound { get; init; }

    /// <summary>
    /// Creates a result indicating no startup profile file was found.
    /// </summary>
    public static StartupProfileResult NotFound() => new()
    {
        Success = false,
        FileFound = false
    };

    /// <summary>
    /// Creates a result indicating a profile was successfully loaded.
    /// </summary>
    /// <param name="profile">The loaded profile.</param>
    public static StartupProfileResult Loaded(Profile profile) => new()
    {
        Success = true,
        Profile = profile,
        FileFound = true
    };

    /// <summary>
    /// Creates a result indicating loading failed.
    /// </summary>
    /// <param name="error">The error message.</param>
    public static StartupProfileResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error,
        FileFound = true
    };
}
