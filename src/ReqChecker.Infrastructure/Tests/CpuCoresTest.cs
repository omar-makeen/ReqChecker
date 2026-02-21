using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests CPU cores by detecting logical processor count and optionally enforcing a minimum threshold.
/// </summary>
[TestType("CpuCores")]
public class CpuCoresTest : ITest
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
            // Get optional minimumCores parameter
            int? minimumCores = null;
            var minimumCoresValue = testDefinition.Parameters["minimumCores"];
            
            if (minimumCoresValue != null)
            {
                if (!int.TryParse(minimumCoresValue.ToString(), out var parsedCores))
                {
                    throw new ArgumentException("minimumCores must be a valid integer value", "minimumCores");
                }
                minimumCores = parsedCores;
            }

            // Validate minimumCores is non-negative
            if (minimumCores.HasValue && minimumCores.Value < 0)
            {
                throw new ArgumentException("minimumCores must be a non-negative value", "minimumCores");
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Detect logical processor count
            int detectedCores = Environment.ProcessorCount;

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Determine if threshold is met
            bool? thresholdMet = null;
            if (minimumCores.HasValue)
            {
                thresholdMet = detectedCores >= minimumCores.Value;
                result.Status = thresholdMet.Value ? TestStatus.Pass : TestStatus.Fail;
            }
            else
            {
                // Informational mode - always pass
                result.Status = TestStatus.Pass;
            }

            // Build evidence
            var evidence = new Dictionary<string, object?>
            {
                ["detectedCores"] = detectedCores,
                ["minimumCores"] = minimumCores,
                ["thresholdMet"] = thresholdMet
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

            // Build human-readable summary
            if (minimumCores.HasValue)
            {
                result.HumanSummary = thresholdMet == true
                    ? $"{detectedCores} logical processors detected (minimum: {minimumCores.Value}) - Pass"
                    : $"{detectedCores} logical processors detected (minimum: {minimumCores.Value}) - Fail";
            }
            else
            {
                result.HumanSummary = $"{detectedCores} logical processors detected";
            }

            // Set error if threshold not met
            if (minimumCores.HasValue && thresholdMet == false)
            {
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = $"System has {detectedCores} logical processors, which is less than the required {minimumCores.Value}"
                };
            }
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
