using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests if a directory exists at the specified path.
/// </summary>
[TestType("DirectoryExists")]
public class DirectoryExistsTest : ITest
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
            var shouldExist = testDefinition.Parameters["shouldExist"]?.GetValue<bool>() ?? true;

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path parameter is required", nameof(path));
            }

            // Check if directory exists
            var exists = Directory.Exists(path);
            var isPass = shouldExist ? exists : !exists;

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = isPass ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["path"] = path,
                ["exists"] = exists,
                ["shouldExist"] = shouldExist,
                ["isPass"] = isPass
            };

            // If directory exists, get additional info
            if (exists)
            {
                var dirInfo = new DirectoryInfo(path);
                evidence["creationTime"] = dirInfo.CreationTimeUtc;
                evidence["lastModified"] = dirInfo.LastWriteTimeUtc;
                evidence["attributes"] = dirInfo.Attributes.ToString();

                // Count files and subdirectories
                try
                {
                    var files = Directory.GetFiles(path);
                    var directories = Directory.GetDirectories(path);
                    evidence["fileCount"] = files.Length;
                    evidence["directoryCount"] = directories.Length;
                }
                catch (UnauthorizedAccessException)
                {
                    evidence["fileCount"] = -1;
                    evidence["directoryCount"] = -1;
                    evidence["accessDenied"] = true;
                }
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds
                }
            };
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
