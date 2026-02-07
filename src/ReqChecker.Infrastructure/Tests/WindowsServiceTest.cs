using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests Windows service status by checking if a named service is installed and in the expected state.
/// </summary>
[TestType("WindowsService")]
[SupportedOSPlatform("windows")]
public class WindowsServiceTest : ITest
{
    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
    {
        var result = new TestResult
        {
            TestId = testDefinition.Id,
            TestType = testDefinition.Type,
            DisplayName = testDefinition.DisplayName,
            Status = TestStatus.Fail,
            StartTime = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
#if WINDOWS
            // Get parameters
            var serviceName = testDefinition.Parameters["serviceName"]?.ToString() ?? string.Empty;
            var expectedStatusValue = testDefinition.Parameters["expectedStatus"]?.ToString();

            // Validate required parameters
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException("serviceName parameter is required", nameof(serviceName));
            }

            // Default to Running if not specified
            string expectedStatus = expectedStatusValue ?? "Running";

            cancellationToken.ThrowIfCancellationRequested();

            // Get the service controller
            using var serviceController = new System.ServiceProcess.ServiceController(serviceName);

            // Check if service exists
            try
            {
                // Refresh to get current status
                serviceController.Refresh();
            }
            catch (InvalidOperationException ex)
            {
                // Service not found
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Configuration,
                    Message = $"Windows service '{serviceName}' not found",
                    StackTrace = ex.StackTrace
                };
                return await Task.FromResult(result);
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Get service details
            string displayName = serviceController.DisplayName;
            string currentStatus = serviceController.Status.ToString();
            string startType = serviceController.StartType.ToString();

            // Check if status matches
            bool statusMatch = string.Equals(currentStatus, expectedStatus, StringComparison.OrdinalIgnoreCase);

            // Determine test status
            result.Status = statusMatch ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["serviceName"] = serviceName,
                ["displayName"] = displayName,
                ["status"] = currentStatus,
                ["expectedStatus"] = expectedStatus,
                ["startType"] = startType,
                ["statusMatch"] = statusMatch
            };

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds,
                    ExecuteMs = (int)stopwatch.ElapsedMilliseconds
                }
            };

            // Set error if status doesn't match
            if (!statusMatch)
            {
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = $"Service '{serviceName}' status is '{currentStatus}', expected '{expectedStatus}'"
                };
            }
#else
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Skipped;
            result.Error = new TestError
            {
                Category = ErrorCategory.Permission,
                Message = "Windows Service tests are only supported on Windows"
            };
#endif
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Skipped;
            result.Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = "Test was cancelled"
            };
        }
        catch (ArgumentException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Validation,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }

        return await Task.FromResult(result);
    }
}
