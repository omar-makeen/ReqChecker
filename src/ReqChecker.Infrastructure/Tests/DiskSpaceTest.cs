using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests disk space by checking if a drive or mount point has at least a specified amount of free space.
/// </summary>
[TestType("DiskSpace")]
public class DiskSpaceTest : ITest
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
            // Get parameters
            var path = testDefinition.Parameters["path"]?.ToString() ?? string.Empty;
            var minimumFreeGBValue = testDefinition.Parameters["minimumFreeGB"];

            // Validate required parameters
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("path parameter is required", nameof(path));
            }

            if (minimumFreeGBValue == null)
            {
                throw new ArgumentException("minimumFreeGB parameter is required", "minimumFreeGB");
            }

            // Parse minimumFreeGB
            if (!decimal.TryParse(minimumFreeGBValue.ToString(), out decimal minimumFreeGB) || minimumFreeGB < 0)
            {
                throw new ArgumentException("minimumFreeGB must be a valid non-negative decimal value", "minimumFreeGB");
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Get drive info
            DriveInfo? driveInfo = null;
            try
            {
                // Try to get drive info for the path
                driveInfo = new DriveInfo(path);
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
                    Message = $"Invalid path '{path}': {ex.Message}",
                    StackTrace = ex.StackTrace
                };
                return await Task.FromResult(result);
            }

            // Check if drive is ready (e.g., removable media may be disconnected)
            if (!driveInfo.IsReady)
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Configuration,
                    Message = $"Drive '{path}' is not ready or not accessible"
                };
                return await Task.FromResult(result);
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Get drive details
            decimal totalSpaceGB = driveInfo.TotalSize > 0 ? (decimal)driveInfo.TotalSize / (1024 * 1024 * 1024) : 0;
            decimal freeSpaceGB = driveInfo.AvailableFreeSpace > 0 ? (decimal)driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024) : 0;
            decimal percentFree = totalSpaceGB > 0 ? (freeSpaceGB / totalSpaceGB) * 100 : 0;

            // Check if threshold is met (inclusive: free >= minimum)
            bool thresholdMet = freeSpaceGB >= minimumFreeGB;

            // Determine test status
            result.Status = thresholdMet ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["path"] = path,
                ["totalSpaceGB"] = Math.Round(totalSpaceGB, 2),
                ["freeSpaceGB"] = Math.Round(freeSpaceGB, 2),
                ["percentFree"] = Math.Round(percentFree, 2),
                ["minimumFreeGB"] = Math.Round(minimumFreeGB, 2),
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

            // Set error if threshold not met
            if (!thresholdMet)
            {
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = $"Drive '{path}' has {Math.Round(freeSpaceGB, 2)} GB free, which is less than the required {Math.Round(minimumFreeGB, 2)} GB"
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
