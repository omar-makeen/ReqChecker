using ReqChecker.Core.Models;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Executes a sequence of tests with progress reporting.
/// </summary>
public interface ITestRunner
{
    /// <summary>
    /// Executes all tests in a profile sequentially.
    /// </summary>
    /// <param name="profile">The profile containing tests to execute.</param>
    /// <param name="progress">Progress reporting callback.</param>
    /// <param name="cancellationToken">Cancellation token for aborting the run.</param>
    /// <returns>The complete run report.</returns>
    Task<RunReport> RunTestsAsync(Profile profile, IProgress<TestResult> progress, CancellationToken cancellationToken);
}
