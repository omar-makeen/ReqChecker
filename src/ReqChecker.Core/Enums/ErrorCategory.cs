namespace ReqChecker.Core.Enums;

/// <summary>
/// Classification of test errors for better user understanding.
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// Network-related errors (connection, DNS, socket errors).
    /// </summary>
    Network,

    /// <summary>
    /// Operation timed out.
    /// </summary>
    Timeout,

    /// <summary>
    /// Permission or access denied errors, elevation required.
    /// </summary>
    Permission,

    /// <summary>
    /// Unexpected response, assertion failed.
    /// </summary>
    Validation,

    /// <summary>
    /// Invalid parameters or configuration.
    /// </summary>
    Configuration,

    /// <summary>
    /// Test skipped because a prerequisite test failed or was skipped.
    /// </summary>
    Dependency,

    /// <summary>
    /// Unclassified error.
    /// </summary>
    Unknown
}
