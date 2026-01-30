using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace ReqChecker.Infrastructure.Security;

/// <summary>
/// Provides data protection using Windows DPAPI (Data Protection API).
/// Encrypts data for the current user or machine context.
/// </summary>
[SupportedOSPlatform("windows")]
public class DpapiProtector
{
    /// <summary>
    /// Encrypts plaintext data using DPAPI for the current user.
    /// </summary>
    /// <param name="plaintext">The plaintext data to encrypt.</param>
    /// <returns>Base64-encoded encrypted data.</returns>
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentException("Plaintext cannot be null or empty.", nameof(plaintext));
        }

        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] encryptedBytes = ProtectedData.Protect(
            plaintextBytes,
            null,
            DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Encrypts plaintext data using DPAPI for the current machine.
    /// </summary>
    /// <param name="plaintext">The plaintext data to encrypt.</param>
    /// <returns>Base64-encoded encrypted data.</returns>
    public string EncryptForMachine(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentException("Plaintext cannot be null or empty.", nameof(plaintext));
        }

        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] encryptedBytes = ProtectedData.Protect(
            plaintextBytes,
            null,
            DataProtectionScope.LocalMachine);

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypts encrypted data using DPAPI for the current user.
    /// </summary>
    /// <param name="encryptedBase64">Base64-encoded encrypted data.</param>
    /// <returns>The decrypted plaintext.</returns>
    public string Decrypt(string encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64))
        {
            throw new ArgumentException("Encrypted data cannot be null or empty.", nameof(encryptedBase64));
        }

        byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
        byte[] decryptedBytes = ProtectedData.Unprotect(
            encryptedBytes,
            null,
            DataProtectionScope.CurrentUser);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// Decrypts encrypted data using DPAPI for the current machine.
    /// </summary>
    /// <param name="encryptedBase64">Base64-encoded encrypted data.</param>
    /// <returns>The decrypted plaintext.</returns>
    public string DecryptForMachine(string encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64))
        {
            throw new ArgumentException("Encrypted data cannot be null or empty.", nameof(encryptedBase64));
        }

        byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
        byte[] decryptedBytes = ProtectedData.Unprotect(
            encryptedBytes,
            null,
            DataProtectionScope.LocalMachine);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// Encrypts binary data using DPAPI for the current user.
    /// </summary>
    /// <param name="data">The binary data to encrypt.</param>
    /// <returns>Encrypted binary data.</returns>
    public byte[] EncryptBytes(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        }

        return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
    }

    /// <summary>
    /// Decrypts binary data using DPAPI for the current user.
    /// </summary>
    /// <param name="encryptedData">The encrypted binary data.</param>
    /// <returns>Decrypted binary data.</returns>
    public byte[] DecryptBytes(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
        {
            throw new ArgumentException("Encrypted data cannot be null or empty.", nameof(encryptedData));
        }

        return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
    }
}
