using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Text;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests writing content to a file.
/// </summary>
[TestType("FileWrite")]
public class FileWriteTest : ITest
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
            var content = testDefinition.Parameters["content"]?.ToString() ?? string.Empty;
            var encoding = testDefinition.Parameters["encoding"]?.ToString() ?? "utf-8";
            var createDirectory = testDefinition.Parameters["createDirectory"]?.GetValue<bool>() ?? false;
            var overwrite = testDefinition.Parameters["overwrite"]?.GetValue<bool>() ?? false;

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path parameter is required", nameof(path));
            }

            // Check if file exists and overwrite is false
            if (File.Exists(path) && !overwrite)
            {
                throw new IOException($"File already exists and overwrite is false: {path}");
            }

            // Create directory if needed
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && createDirectory && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write file content
            var encodingObj = Encoding.GetEncoding(encoding);
            await File.WriteAllTextAsync(path, content, encodingObj, cancellationToken);

            // Verify write by reading back
            var readContent = await File.ReadAllTextAsync(path, encodingObj, cancellationToken);
            var isPass = readContent == content;

            // Get file info
            var fileInfo = new FileInfo(path);

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = isPass ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["path"] = path,
                ["encoding"] = encoding,
                ["contentLength"] = content.Length,
                ["size"] = fileInfo.Length,
                ["createDirectory"] = createDirectory,
                ["overwrite"] = overwrite,
                ["isPass"] = isPass
            };

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                FileContent = content,
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

        return result;
    }
}
