using ReqChecker.Core.Models;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Validates profile structure and content.
/// </summary>
public interface IProfileValidator
{
    /// <summary>
    /// Validates a profile and returns any validation errors.
    /// </summary>
    /// <param name="profile">The profile to validate.</param>
    /// <returns>A collection of validation error messages (empty if valid).</returns>
    Task<IEnumerable<string>> ValidateAsync(Profile profile);
}
