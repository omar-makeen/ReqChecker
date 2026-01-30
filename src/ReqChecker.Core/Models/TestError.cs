using ReqChecker.Core.Enums;

namespace ReqChecker.Core.Models;

/// <summary>
/// Error details for failed tests.
/// </summary>
public class TestError
{
    /// <summary>
    /// Error classification.
    /// </summary>
    public ErrorCategory Category { get; set; }

    /// <summary>
    /// User-friendly error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// .NET exception type name.
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Stack trace (debug builds only).
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Nested exception if any.
    /// </summary>
    public TestError? InnerError { get; set; }
}
