using ReqChecker.Core.Enums;

namespace ReqChecker.Core.Models;

/// <summary>
/// Root configuration entity loaded from JSON files.
/// </summary>
public class Profile
{
    /// <summary>
    /// Unique identifier for the profile.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable profile name for display.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Schema version for migration.
    /// </summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// Bundled (company-managed) or UserProvided.
    /// </summary>
    public ProfileSource Source { get; set; }

    /// <summary>
    /// Global defaults for test execution.
    /// </summary>
    public RunSettings RunSettings { get; set; } = new();

    /// <summary>
    /// Ordered list of tests to execute.
    /// </summary>
    public List<TestDefinition> Tests { get; set; } = new();

    /// <summary>
    /// HMAC-SHA256 signature for integrity (bundled only).
    /// </summary>
    public string? Signature { get; set; }
}
