namespace ReqChecker.Core.Models;

/// <summary>
/// Captured environment information.
/// </summary>
public class MachineInfo
{
    /// <summary>
    /// Machine name.
    /// </summary>
    public string Hostname { get; set; } = string.Empty;

    /// <summary>
    /// Windows version string.
    /// </summary>
    public string OsVersion { get; set; } = string.Empty;

    /// <summary>
    /// Build number.
    /// </summary>
    public string OsBuild { get; set; } = string.Empty;

    /// <summary>
    /// CPU core count.
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// Total RAM in MB.
    /// </summary>
    public long TotalMemoryMB { get; set; }

    /// <summary>
    /// Running user (without domain).
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Running as admin.
    /// </summary>
    public bool IsElevated { get; set; }

    /// <summary>
    /// Active network interfaces.
    /// </summary>
    public List<NetworkInterfaceInfo> NetworkInterfaces { get; set; } = new();
}
