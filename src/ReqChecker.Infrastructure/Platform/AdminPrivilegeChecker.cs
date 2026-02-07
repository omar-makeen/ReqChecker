using System.Runtime.Versioning;

namespace ReqChecker.Infrastructure.Platform;

/// <summary>
/// Checks and manages administrator privileges.
/// </summary>
public class AdminPrivilegeChecker
{
    /// <summary>
    /// Checks if the current process is running with administrator privileges.
    /// </summary>
    /// <returns>True if running as administrator.</returns>
    public static bool IsAdministrator()
    {
#if WINDOWS
        return IsAdministratorWindows();
#else
        // On non-Windows platforms, we cannot easily check admin status
        return false;
#endif
    }

    /// <summary>
    /// Checks if the current process is running with administrator privileges on Windows.
    /// </summary>
    /// <returns>True if running as administrator.</returns>
    [SupportedOSPlatform("windows")]
    private static bool IsAdministratorWindows()
    {
        try
        {
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a user-friendly description of the current privilege level.
    /// </summary>
    /// <returns>A description of the privilege level.</returns>
    public static string GetPrivilegeDescription()
    {
        return IsAdministrator() ? "Administrator" : "Standard User";
    }
}
