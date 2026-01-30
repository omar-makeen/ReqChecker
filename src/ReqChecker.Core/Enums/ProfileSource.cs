namespace ReqChecker.Core.Enums;

/// <summary>
/// Indicates where a profile originated from.
/// </summary>
public enum ProfileSource
{
    /// <summary>
    /// Profile is bundled with the application (company-managed).
    /// </summary>
    Bundled,

    /// <summary>
    /// Profile was imported by the user.
    /// </summary>
    UserProvided
}
