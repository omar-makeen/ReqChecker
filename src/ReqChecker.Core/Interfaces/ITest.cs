using ReqChecker.Core.Execution;
using ReqChecker.Core.Models;
using System.Text.Json.Nodes;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Defines a test that can be executed.
/// </summary>
public interface ITest
{
    /// <summary>
    /// Executes the test asynchronously.
    /// </summary>
    /// <param name="definition">The test definition containing parameters.</param>
    /// <param name="context">The execution context containing transient credentials.</param>
    /// <param name="cancellationToken">Cancellation token for aborting the test.</param>
    /// <returns>The test result.</returns>
    Task<TestResult> ExecuteAsync(TestDefinition definition, TestExecutionContext? context, CancellationToken cancellationToken);
}
