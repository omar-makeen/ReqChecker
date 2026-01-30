namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Verifies profile integrity using HMAC signatures.
/// </summary>
public interface IIntegrityVerifier
{
    /// <summary>
    /// Verifies a profile's signature.
    /// </summary>
    /// <param name="profileJson">The profile JSON content.</param>
    /// <param name="signature">The signature to verify against.</param>
    /// <returns>True if signature is valid.</returns>
    Task<bool> VerifyAsync(string profileJson, string signature);
}
