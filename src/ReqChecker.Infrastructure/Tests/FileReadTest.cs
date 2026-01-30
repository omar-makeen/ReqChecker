using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Text;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests reading content from a file.
/// </summary>
[TestType("FileRead")]
public class FileReadTest : ITest
{
    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, CancellationToken cancellationToken = default)
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
            var encoding = testDefinition.Parameters["encoding"]?.ToString() ?? "utf-8";
            var maxLength = testDefinition.Parameters["maxLength"]?.GetValue<int>() ?? 10240;
            var expectedContent = testDefinition.Parameters["expectedContent"]?.ToString();

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path parameter is required", nameof(path));
            }

            // Check if file exists
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}", path);
            }

            // Read file content
            var encodingObj = Encoding.GetEncoding(encoding);
            var content = await File.ReadAllTextAsync(path, encodingObj, cancellationToken);

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Check if content matches expected
            var isPass = true;
            if (!string.IsNullOrEmpty(expectedContent))
            {
                isPass = content.Contains(expectedContent);
            }

            result.Status = isPass ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["path"] = path,
                ["encoding"] = encoding,
                ["length"] = content.Length,
                ["isPass"] = isPass
            };

            // Include expected content if specified
            if (!string.IsNullOrEmpty(expectedContent))
            {
                evidence["expectedContent"] = expectedContent;
                evidence["containsExpected"] = isPass;
            }

            // Truncate content if too long
            if (content.Length > maxLength)
            {
                evidence["content"] = content.Substring(0, maxLength) + "... (truncated)";
                evidence["truncated"] = true;
            }
            else
            {
                evidence["content"] = content;
                evidence["truncated"] = false;
            }

            // Get file info
            var fileInfo = new FileInfo(path);
            evidence["size"] = fileInfo.Length;
            evidence["lastModified"] = fileInfo.LastWriteTimeUtc;
            evidence["creationTime"] = fileInfo.CreationTimeUtc;

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                FileContent = evidence["content"] as string,
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
