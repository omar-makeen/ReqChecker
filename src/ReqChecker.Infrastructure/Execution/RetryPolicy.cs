using ReqChecker.Core.Enums;
using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;

namespace ReqChecker.Infrastructure.Execution;

/// <summary>
/// Provides retry logic with configurable backoff strategies.
/// </summary>
public static class RetryPolicy
{
    /// <summary>
    /// Executes a test with retry logic based on the test definition.
    /// </summary>
    /// <param name="test">The test to execute.</param>
    /// <param name="testDefinition">The test definition containing retry configuration.</param>
    /// <param name="runSettings">The run settings containing default retry configuration.</param>
    /// <param name="context">Optional execution context containing credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The test result.</returns>
    public static async Task<TestResult> ExecuteWithRetryAsync(
        ITest test,
        TestDefinition testDefinition,
        RunSettings runSettings,
        TestExecutionContext? context,
        CancellationToken cancellationToken = default)
    {
        var retryCount = testDefinition.RetryCount ?? runSettings.DefaultRetryCount;
        var retryDelayMs = runSettings.RetryDelayMs;
        var backoffStrategy = runSettings.RetryBackoff;

        TestResult? lastResult = null;

        for (int attempt = 0; attempt <= retryCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Execute the test
            var result = await test.ExecuteAsync(testDefinition, context, cancellationToken);
            lastResult = result;

            // If test passed, return immediately
            if (result.Status == TestStatus.Pass)
            {
                return result;
            }

            // If this was the last attempt, return the result
            if (attempt >= retryCount)
            {
                break;
            }

            // Calculate delay based on backoff strategy
            var delay = CalculateDelay(attempt, retryDelayMs, backoffStrategy);

            // Wait before retry
            await Task.Delay(delay, cancellationToken);
        }

        // Return the last result (which should be a failure)
        return lastResult ?? new TestResult
        {
            TestId = testDefinition.Id,
            Status = TestStatus.Fail,
            Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = "No result returned from test execution"
            }
        };
    }

    /// <summary>
    /// Calculates the delay before the next retry attempt.
    /// </summary>
    /// <param name="attempt">The current attempt number (0-based).</param>
    /// <param name="baseDelayMs">The base delay in milliseconds.</param>
    /// <param name="strategy">The backoff strategy to use.</param>
    /// <returns>The delay in milliseconds.</returns>
    private static int CalculateDelay(int attempt, int baseDelayMs, BackoffStrategy strategy)
    {
        return strategy switch
        {
            BackoffStrategy.Linear => baseDelayMs * (attempt + 1),
            BackoffStrategy.Exponential => (int)(baseDelayMs * Math.Pow(2, attempt)),
            BackoffStrategy.None => baseDelayMs,
            _ => baseDelayMs
        };
    }

    /// <summary>
    /// Determines if a test should be retried based on its result.
    /// </summary>
    /// <param name="result">The test result.</param>
    /// <returns>True if the test should be retried, false otherwise.</returns>
    public static bool ShouldRetry(TestResult result)
    {
        // Retry on failure, but not on skipped or passed tests
        return result.Status == TestStatus.Fail;
    }
}
