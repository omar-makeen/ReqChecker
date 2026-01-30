using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests HTTP GET endpoint connectivity and response.
/// </summary>
[TestType("HttpGet")]
public class HttpGetTest : ITest
{
    private static readonly HttpClient _httpClient = new();

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
            var url = testDefinition.Parameters["url"]?.ToString() ?? string.Empty;
            var timeoutMs = testDefinition.Parameters["timeout"]?.GetValue<int>() ?? 30000;
            var expectedStatus = testDefinition.Parameters["expectedStatus"]?.GetValue<int>();
            var includeHeaders = testDefinition.Parameters["includeHeaders"]?.GetValue<bool>() ?? false;
            var includeBody = testDefinition.Parameters["includeBody"]?.GetValue<bool>() ?? true;
            var maxBodyLength = testDefinition.Parameters["maxBodyLength"]?.GetValue<int>() ?? 10240;

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL parameter is required", nameof(url));
            }

            // Set timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            // Create request
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add custom headers if specified
            if (testDefinition.Parameters["headers"] is System.Text.Json.Nodes.JsonArray headersArray)
            {
                foreach (var headerNode in headersArray)
                {
                    if (headerNode is System.Text.Json.Nodes.JsonObject headerObj)
                    {
                        var name = headerObj["name"]?.ToString();
                        var value = headerObj["value"]?.ToString();
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                        {
                            request.Headers.TryAddWithoutValidation(name, value);
                        }
                    }
                }
            }

            // Execute request
            var responseStart = stopwatch.ElapsedMilliseconds;
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            var responseTime = stopwatch.ElapsedMilliseconds - responseStart;

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Read response
            var statusCode = (int)response.StatusCode;
            var isSuccess = response.IsSuccessStatusCode;

            // Check if status matches expected
            if (expectedStatus.HasValue && statusCode != expectedStatus.Value)
            {
                isSuccess = false;
            }

            result.Status = isSuccess ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["url"] = url,
                ["method"] = "GET",
                ["statusCode"] = statusCode,
                ["isSuccess"] = isSuccess,
                ["responseTimeMs"] = responseTime,
                ["contentType"] = response.Content.Headers.ContentType?.ToString() ?? "N/A",
                ["contentLength"] = response.Content.Headers.ContentLength ?? 0
            };

            // Include headers if requested
            if (includeHeaders)
            {
                var headers = new Dictionary<string, string>();
                foreach (var header in response.Headers)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }
                foreach (var header in response.Content.Headers)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }
                evidence["headers"] = headers;
            }

            // Include body if requested
            if (includeBody && isSuccess)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (body.Length > maxBodyLength)
                {
                    body = body.Substring(0, maxBodyLength) + "... (truncated)";
                }
                evidence["body"] = body;
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                ResponseCode = statusCode,
                ResponseHeaders = includeHeaders ? evidence["headers"] as Dictionary<string, string> : null,
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds,
                    ConnectMs = (int)responseTime
                }
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Timeout,
                Message = "Request timed out"
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
                Category = ErrorCategory.Network,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }

        return result;
    }
}
