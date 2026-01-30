namespace ReqChecker.Core.Models;

/// <summary>
/// Information about a network interface.
/// </summary>
public class NetworkInterfaceInfo
{
    /// <summary>
    /// Interface name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Interface description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// MAC address.
    /// </summary>
    public string MacAddress { get; set; } = string.Empty;

    /// <summary>
    /// IP addresses.
    /// </summary>
    public List<string> IpAddresses { get; set; } = new();

    /// <summary>
    /// Interface status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
