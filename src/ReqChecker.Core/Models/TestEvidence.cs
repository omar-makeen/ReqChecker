namespace ReqChecker.Core.Models;

/// <summary>
/// Data captured during test execution.
/// </summary>
public class TestEvidence
{
    /// <summary>
    /// Response body/output (truncated to 4KB).
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// HTTP status code if applicable.
    /// </summary>
    public int? ResponseCode { get; set; }

    /// <summary>
    /// Response headers if applicable.
    /// </summary>
    public Dictionary<string, string>? ResponseHeaders { get; set; }

    /// <summary>
    /// File content sample if applicable.
    /// </summary>
    public string? FileContent { get; set; }

    /// <summary>
    /// Running processes if applicable.
    /// </summary>
    public string[]? ProcessList { get; set; }

    /// <summary>
    /// Registry value if applicable.
    /// </summary>
    public string? RegistryValue { get; set; }

    /// <summary>
    /// Detailed timing breakdown.
    /// </summary>
    public TimingBreakdown? Timing { get; set; }
}
