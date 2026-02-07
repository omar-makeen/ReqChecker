using ReqChecker.Core.Models;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;

namespace ReqChecker.Infrastructure.Platform;

/// <summary>
/// Collects machine information for diagnostics.
/// </summary>
public class MachineInfoCollector
{
    /// <summary>
    /// Collects current machine information.
    /// </summary>
    /// <returns>The collected machine information.</returns>
    public static MachineInfo Collect()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Select(ni => new NetworkInterfaceInfo
            {
                Name = ni.Name,
                Description = ni.Description,
                MacAddress = ni.GetPhysicalAddress().ToString(),
                IpAddresses = ni.GetIPProperties().UnicastAddresses
                    .Select(ip => ip.Address?.ToString() ?? string.Empty)
                    .Where(ip => !string.IsNullOrEmpty(ip))
                    .ToList(),
                Status = ni.OperationalStatus.ToString()
            })
            .ToList();

        return new MachineInfo
        {
            Hostname = Environment.MachineName,
            OsVersion = Environment.OSVersion.VersionString,
            OsBuild = Environment.OSVersion.Version.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            TotalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024),
            UserName = Environment.UserName,
            IsElevated = IsRunningAsAdministrator(),
            NetworkInterfaces = interfaces
        };
    }

#if WINDOWS
    /// <summary>
    /// Checks if the current process is running with administrator privileges on Windows.
    /// </summary>
    /// <returns>True if running as administrator.</returns>
    [SupportedOSPlatform("windows")]
    private static bool IsRunningAsAdministrator()
    {
        using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }
#else
    /// <summary>
    /// Checks if the current process is running with administrator privileges (non-Windows).
    /// </summary>
    /// <returns>False on non-Windows platforms.</returns>
    private static bool IsRunningAsAdministrator()
    {
        // On non-Windows platforms, we cannot easily check admin status
        return false;
    }
#endif
}
