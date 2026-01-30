namespace ReqChecker.Core.Enums;

/// <summary>
/// Defines how to handle tests that require administrator privileges.
/// </summary>
public enum AdminBehavior
{
    /// <summary>
    /// Skip the test and record the reason.
    /// </summary>
    SkipWithReason,

    /// <summary>
    /// Prompt the user to elevate privileges.
    /// </summary>
    PromptForElevation,

    /// <summary>
    /// Fail the test immediately if not elevated.
    /// </summary>
    FailImmediately
}
