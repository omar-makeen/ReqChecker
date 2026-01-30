using ReqChecker.Core.Enums;

namespace ReqChecker.Core.Models;

/// <summary>
/// Global defaults for test execution, can be overridden per-test.
/// </summary>
public class RunSettings
{
    /// <summary>
    /// Default timeout in milliseconds (default: 30000).
    /// </summary>
    public int DefaultTimeout { get; set; } = 30000;

    /// <summary>
    /// Default retry attempts (default: 0).
    /// </summary>
    public int DefaultRetryCount { get; set; } = 0;

    /// <summary>
    /// Default retry delay in milliseconds (default: 5000).
    /// </summary>
    public int RetryDelayMs { get; set; } = 5000;

    /// <summary>
    /// Retry delay strategy (default: None).
    /// </summary>
    public BackoffStrategy RetryBackoff { get; set; } = BackoffStrategy.None;

    /// <summary>
    /// How to handle tests requiring admin privileges (default: SkipWithReason).
    /// </summary>
    public AdminBehavior AdminBehavior { get; set; } = AdminBehavior.SkipWithReason;
}
