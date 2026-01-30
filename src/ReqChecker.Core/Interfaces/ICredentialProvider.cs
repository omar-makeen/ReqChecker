namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Provides secure credential storage and retrieval.
/// </summary>
public interface ICredentialProvider
{
    /// <summary>
    /// Retrieves credentials for a given reference ID.
    /// </summary>
    /// <param name="credentialRef">The credential reference identifier.</param>
    /// <returns>A tuple of username and password, or null if not found.</returns>
    Task<(string? Username, string? Password)?> GetCredentialsAsync(string credentialRef);

    /// <summary>
    /// Stores credentials for a given reference ID.
    /// </summary>
    /// <param name="credentialRef">The credential reference identifier.</param>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    Task StoreCredentialsAsync(string credentialRef, string username, string password);

    /// <summary>
    /// Removes stored credentials for a given reference ID.
    /// </summary>
    /// <param name="credentialRef">The credential reference identifier.</param>
    Task RemoveCredentialsAsync(string credentialRef);
}
