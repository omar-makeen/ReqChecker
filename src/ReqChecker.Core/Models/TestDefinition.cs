using ReqChecker.Core.Enums;
using System.Text.Json.Nodes;

namespace ReqChecker.Core.Models;

/// <summary>
/// Defines a single test to be executed.
/// </summary>
public class TestDefinition
{
    /// <summary>
    /// Unique identifier within profile.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Test type identifier (e.g., "HttpGet", "Ping").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// User-facing test name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional help text.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type-specific test parameters.
    /// </summary>
    public JsonObject Parameters { get; set; } = new();

    /// <summary>
    /// Per-field editability rules using dot-path keys.
    /// </summary>
    public Dictionary<string, FieldPolicyType> FieldPolicy { get; set; } = new();

    /// <summary>
    /// Override default timeout (ms).
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// Override default retry count.
    /// </summary>
    public int? RetryCount { get; set; }

    /// <summary>
    /// Whether test needs elevation.
    /// </summary>
    public bool RequiresAdmin { get; set; }

    /// <summary>
    /// IDs of tests that must complete successfully before this test runs. Empty list means no dependencies.
    /// </summary>
    public List<string> DependsOn { get; set; } = new();
}
