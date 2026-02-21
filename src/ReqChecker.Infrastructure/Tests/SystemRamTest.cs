using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests system RAM by detecting total physical memory and optionally enforcing a minimum threshold.
/// </summary>
[TestType("SystemRam")]
[SupportedOSPlatform("windows")]
public class SystemRamTest : ITest
{
#if WINDOWS
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicallyInstalledSystemMemory(out ulong TotalMemoryInKilobytes);
#endif

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
            // Get optional minimumGB parameter
            double? minimumGB = null;
            var minimumGBValue = testDefinition.Parameters["minimumGB"];
            
            if (minimumGBValue != null)
            {
                if (!double.TryParse(minimumGBValue.ToString(), out var parsedGB))
                {
                    throw new ArgumentException("minimumGB must be a valid numeric value", "minimumGB");
                }
                minimumGB = parsedGB;
            }

            // Validate minimumGB is non-negative
            if (minimumGB.HasValue && minimumGB.Value < 0)
            {
                throw new ArgumentException("minimumGB must be a non-negative value", "minimumGB");
            }

            cancellationToken.ThrowIfCancellationRequested();

#if WINDOWS
            // Detect total physical RAM using Windows API
            ulong totalBytes;
            try
            {
                if (!GetPhysicallyInstalledSystemMemory(out ulong totalMemoryKB))
                {
                    throw new InvalidOperationException("Failed to retrieve system RAM information from Windows API");
                }
                totalBytes = totalMemoryKB * 1024; // Convert KB to bytes
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
                    Message = $"Failed to retrieve system RAM information: {ex.Message}",
                    StackTrace = ex.StackTrace
                };
                return await Task.FromResult(result);
            }

            // Convert to GB - use unrounded value for comparison, rounded for display
            double detectedGBRaw = totalBytes / 1073741824.0;
            double detectedGB = Math.Round(detectedGBRaw, 1);

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Determine if threshold is met (compare against unrounded value)
            bool? thresholdMet = null;
            if (minimumGB.HasValue)
            {
                thresholdMet = detectedGBRaw >= minimumGB.Value;
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
                ["detectedGB"] = detectedGB,
                ["detectedBytes"] = totalBytes,
                ["minimumGB"] = minimumGB,
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
            if (minimumGB.HasValue)
            {
                result.HumanSummary = thresholdMet == true
                    ? $"{detectedGB:F1} GB RAM detected (minimum: {minimumGB.Value:F1} GB) - Pass"
                    : $"{detectedGB:F1} GB RAM detected (minimum: {minimumGB.Value:F1} GB) - Fail";
            }
            else
            {
                result.HumanSummary = $"{detectedGB:F1} GB RAM detected";
            }

            // Set error if threshold not met
            if (minimumGB.HasValue && thresholdMet == false)
            {
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = $"System has {detectedGB:F1} GB RAM, which is less than the required {minimumGB.Value:F1} GB"
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
                Message = "System RAM tests are only supported on Windows"
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
