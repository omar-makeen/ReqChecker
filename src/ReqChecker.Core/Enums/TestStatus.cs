namespace ReqChecker.Core.Enums;

/// <summary>
/// Represents the status of a test execution.
/// </summary>
public enum TestStatus
{
    /// <summary>
    /// Test passed successfully.
    /// </summary>
    Pass,

    /// <summary>
    /// Test failed.
    /// </summary>
    Fail,

    /// <summary>
    /// Test was skipped (e.g., due to missing admin privileges).
    /// </summary>
    Skipped
}
