using ReqChecker.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ReqChecker.Infrastructure.ProfileManagement;

/// <summary>
/// Verifies profile integrity using HMAC-SHA256 signatures.
/// </summary>
public class HmacIntegrityVerifier : IIntegrityVerifier
{
    private readonly byte[] _hmacKey;

    /// <summary>
    /// Initializes a new instance of the HmacIntegrityVerifier.
    /// </summary>
    /// <param name="hmacKey">The HMAC key for signature verification.</param>
    public HmacIntegrityVerifier(string hmacKey)
    {
        if (string.IsNullOrWhiteSpace(hmacKey))
        {
            throw new ArgumentException("HMAC key cannot be null or empty.", nameof(hmacKey));
        }

        _hmacKey = Convert.FromBase64String(hmacKey);
    }

    /// <inheritdoc/>
    public Task<bool> VerifyAsync(string profileJson, string signature)
    {
        if (string.IsNullOrWhiteSpace(profileJson))
        {
            throw new ArgumentException("Profile JSON cannot be null or empty.", nameof(profileJson));
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException("Signature cannot be null or empty.", nameof(signature));
        }

        // Normalize JSON for consistent signature computation
        var normalizedJson = NormalizeJson(profileJson);

        // Compute HMAC-SHA256
        var computedSignature = ComputeHmac(normalizedJson);

        // Compare with provided signature
        var isValid = computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(isValid);
    }

    /// <summary>
    /// Normalizes JSON by removing whitespace.
    /// </summary>
    private static string NormalizeJson(string json)
    {
        // Simple normalization: remove all whitespace
        // For production, consider using a JSON parser to canonicalize properly
        var normalized = System.Text.RegularExpressions.Regex.Replace(json, @"\s+", "");
        return normalized;
    }

    /// <summary>
    /// Computes HMAC-SHA256 signature for the given data.
    /// </summary>
    private string ComputeHmac(string data)
    {
        using var hmac = new HMACSHA256(_hmacKey);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
