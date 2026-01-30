namespace ReqChecker.Core.Enums;

/// <summary>
/// Retry delay strategy for failed test executions.
/// </summary>
public enum BackoffStrategy
{
    /// <summary>
    /// No delay between retries.
    /// </summary>
    None,

    /// <summary>
    /// Fixed delay between retries.
    /// </summary>
    Linear,

    /// <summary>
    /// Exponential backoff (delay doubles each retry).
    /// </summary>
    Exponential
}
