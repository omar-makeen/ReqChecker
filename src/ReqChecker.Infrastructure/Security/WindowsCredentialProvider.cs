using ReqChecker.Core.Interfaces;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using System.Text;

namespace ReqChecker.Infrastructure.Security;

/// <summary>
/// Provides secure credential storage and retrieval using Windows Credential Manager.
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsCredentialProvider : ICredentialProvider
{
    private const string TargetPrefix = "ReqChecker:";

    /// <summary>
    /// Retrieves credentials for a given reference ID.
    /// </summary>
    public async Task<(string? Username, string? Password)?> GetCredentialsAsync(string credentialRef)
    {
        return await Task.Run<(string? Username, string? Password)?>(() =>
        {
            string targetName = $"{TargetPrefix}{credentialRef}";

            bool success = NativeMethods.CredRead(
                targetName,
                NativeMethods.CRED_TYPE_GENERIC,
                0,
                out IntPtr credentialPtr);

            if (!success || credentialPtr == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                NativeMethods.CREDENTIAL credential = Marshal.PtrToStructure<NativeMethods.CREDENTIAL>(credentialPtr);

                string username = credential.UserName != IntPtr.Zero
                    ? Marshal.PtrToStringUni(credential.UserName) ?? string.Empty
                    : string.Empty;
                string password = string.Empty;

                if (credential.CredentialBlobSize > 0 && credential.CredentialBlob != IntPtr.Zero)
                {
                    byte[] passwordBytes = new byte[credential.CredentialBlobSize];
                    try
                    {
                        Marshal.Copy(credential.CredentialBlob, passwordBytes, 0, (int)credential.CredentialBlobSize);
                        password = Encoding.Unicode.GetString(passwordBytes);
                    }
                    finally
                    {
                        // Zero out password bytes from memory after use
                        Array.Clear(passwordBytes, 0, passwordBytes.Length);
                    }
                }

                return (username, password);
            }
            finally
            {
                if (credentialPtr != IntPtr.Zero)
                {
                    NativeMethods.CredFree(credentialPtr);
                }
            }
        });
    }

    /// <summary>
    /// Stores credentials for a given reference ID.
    /// </summary>
    public async Task StoreCredentialsAsync(string credentialRef, string username, string password)
    {
        await Task.Run(() =>
        {
            string targetName = $"{TargetPrefix}{credentialRef}";
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);

            var credential = new NativeMethods.CREDENTIAL
            {
                Flags = 0,
                Type = NativeMethods.CRED_TYPE_GENERIC,
                TargetName = Marshal.StringToHGlobalUni(targetName),
                Comment = Marshal.StringToHGlobalUni("Stored by ReqChecker application"),
                LastWritten = DateTime.Now.ToFileTime(),
                CredentialBlobSize = (uint)passwordBytes.Length,
                CredentialBlob = Marshal.AllocHGlobal(passwordBytes.Length),
                Persist = NativeMethods.CRED_PERSIST_ENTERPRISE,
                UserName = Marshal.StringToHGlobalUni(username)
            };

            try
            {
                Marshal.Copy(passwordBytes, 0, credential.CredentialBlob, passwordBytes.Length);

                bool success = NativeMethods.CredWrite(ref credential, 0);
                if (!success)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException(
                        $"Failed to store credentials. Windows error code: {error}");
                }
            }
            finally
            {
                // Zero out password bytes from memory after use
                Array.Clear(passwordBytes, 0, passwordBytes.Length);

                if (credential.TargetName != IntPtr.Zero)
                    Marshal.FreeHGlobal(credential.TargetName);
                if (credential.Comment != IntPtr.Zero)
                    Marshal.FreeHGlobal(credential.Comment);
                if (credential.CredentialBlob != IntPtr.Zero)
                    Marshal.FreeHGlobal(credential.CredentialBlob);
                if (credential.UserName != IntPtr.Zero)
                    Marshal.FreeHGlobal(credential.UserName);
            }
        });
    }

    /// <summary>
    /// Removes stored credentials for a given reference ID.
    /// </summary>
    public async Task RemoveCredentialsAsync(string credentialRef)
    {
        await Task.Run(() =>
        {
            string targetName = $"{TargetPrefix}{credentialRef}";

            bool success = NativeMethods.CredDelete(
                targetName,
                NativeMethods.CRED_TYPE_GENERIC,
                0);

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                // ERROR_NOT_FOUND is expected if credential doesn't exist
                if (error != 1168) // ERROR_NOT_FOUND
                {
                    throw new InvalidOperationException(
                        $"Failed to remove credentials. Windows error code: {error}");
                }
            }
        });
    }

    /// <summary>
    /// Native Windows Credential Manager API methods.
    /// </summary>
    private static class NativeMethods
    {
        public const uint CRED_TYPE_GENERIC = 0x00000001;
        public const uint CRED_PERSIST_LOCAL_MACHINE = 0x00000002;
        public const uint CRED_PERSIST_ENTERPRISE = 0x00000003;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredRead(
            string targetName,
            uint type,
            uint flags,
            out IntPtr credential);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredWrite(
            ref CREDENTIAL credential,
            uint flags);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredDelete(
            string targetName,
            uint type,
            uint flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CredFree(IntPtr credential);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CREDENTIAL
        {
            public uint Flags;
            public uint Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public long LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }
    }
}
