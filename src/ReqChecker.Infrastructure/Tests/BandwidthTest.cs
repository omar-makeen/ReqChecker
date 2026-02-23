using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests network bandwidth by downloading data from a URL for a configurable duration,
/// measuring download throughput in Mbps, and comparing against a minimum threshold.
/// </summary>
[TestType("Bandwidth")]
public class BandwidthTest : ITest
{
    private const int DefaultDurationSeconds = 10;

    /// <summary>
    /// Internal class to hold extracted and validated parameters.
    /// </summary>
    private class BandwidthTestParameters
    {
        public string Url { get; set; } = string.Empty;
        public double MinimumMbps { get; set; } = 0.0;
        public int DurationSeconds { get; set; } = DefaultDurationSeconds;
    }

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
        BandwidthTestParameters? parameters = null;
        HttpClientHandler? handler = null;
        HttpClient? httpClient = null;
        long totalBytesDownloaded = 0; // Declared outside try for access in catch blocks

        try
        {
            // Extract and validate parameters (T003)
            parameters = ExtractParameters(testDefinition);
            cancellationToken.ThrowIfCancellationRequested();

            // Create HttpClient with AllowAutoRedirect and infinite timeout (T004)
            // Timeout is controlled by CancellationTokenSource based on durationSeconds
            handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };
            httpClient = new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            // Create CancellationTokenSource for duration cap (T004)
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(parameters.DurationSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token);

            // Send GET request with ResponseHeadersRead (T004)
            using var request = new HttpRequestMessage(HttpMethod.Get, parameters.Url);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);
            response.EnsureSuccessStatusCode();

            // Get response stream (T004)
            await using var stream = await response.Content.ReadAsStreamAsync(linkedCts.Token);

            // Download loop with buffer (T004)
            var buffer = new byte[81920];
            totalBytesDownloaded = 0; // Reset before download loop
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, linkedCts.Token)) > 0)
            {
                totalBytesDownloaded += bytesRead;
            }

            stopwatch.Stop();

            // Calculate throughput (T005)
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            var measuredMbps = elapsedSeconds > 0
                ? Math.Round((totalBytesDownloaded * 8.0) / (elapsedSeconds * 1_000_000), 2)
                : 0.0;
            var thresholdMet = measuredMbps >= parameters.MinimumMbps;

            // Handle zero-bytes edge case (T005)
            if (totalBytesDownloaded == 0)
            {
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.HumanSummary = "Connection error: no data received";
                result.Error = new TestError
                {
                    Category = ErrorCategory.Network,
                    Message = "Connection error: no data received"
                };
                return result;
            }

            // Build evidence dictionary (T005)
            var evidence = new Dictionary<string, object>
            {
                ["url"] = parameters.Url,
                ["measuredMbps"] = measuredMbps,
                ["minimumMbps"] = parameters.MinimumMbps,
                ["bytesDownloaded"] = totalBytesDownloaded,
                ["elapsedSeconds"] = Math.Round(elapsedSeconds, 2),
                ["thresholdMet"] = thresholdMet
            };

            // Set result (T005)
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = thresholdMet ? TestStatus.Pass : TestStatus.Fail;
            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence)
            };
            result.HumanSummary = thresholdMet
                ? $"Download speed: {measuredMbps:F2} Mbps (minimum: {parameters.MinimumMbps:F2} Mbps) — threshold met"
                : $"Download speed: {measuredMbps:F2} Mbps (minimum: {parameters.MinimumMbps:F2} Mbps) — threshold not met";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // User cancellation (T006)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = "Test cancelled by user";
            result.Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = "Test cancelled by user"
            };
        }
        catch (OperationCanceledException)
        {
            // Duration timeout - this is expected behavior for time-bounded tests
            // Calculate throughput based on bytes downloaded during the duration
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Handle edge case where parameters wasn't initialized (shouldn't happen in practice)
            if (parameters == null)
            {
                result.Status = TestStatus.Fail;
                result.HumanSummary = "Test failed before parameters were initialized";
                result.Error = new TestError
                {
                    Category = ErrorCategory.Configuration,
                    Message = "Test failed before parameters were initialized"
                };
                return result;
            }

            // Handle zero-bytes edge case
            if (totalBytesDownloaded == 0)
            {
                result.Status = TestStatus.Fail;
                result.HumanSummary = "Connection error: no data received before timeout";
                result.Error = new TestError
                {
                    Category = ErrorCategory.Network,
                    Message = "Connection error: no data received before timeout"
                };
                return result;
            }

            // Calculate throughput from downloaded bytes
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            var measuredMbps = elapsedSeconds > 0
                ? Math.Round((totalBytesDownloaded * 8.0) / (elapsedSeconds * 1_000_000), 2)
                : 0.0;
            var thresholdMet = measuredMbps >= parameters.MinimumMbps;

            // Build evidence dictionary
            var evidence = new Dictionary<string, object>
            {
                ["url"] = parameters.Url,
                ["measuredMbps"] = measuredMbps,
                ["minimumMbps"] = parameters.MinimumMbps,
                ["bytesDownloaded"] = totalBytesDownloaded,
                ["elapsedSeconds"] = Math.Round(elapsedSeconds, 2),
                ["thresholdMet"] = thresholdMet
            };

            // Set result based on threshold
            result.Status = thresholdMet ? TestStatus.Pass : TestStatus.Fail;
            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence)
            };
            result.HumanSummary = thresholdMet
                ? $"Download speed: {measuredMbps:F2} Mbps (minimum: {parameters.MinimumMbps:F2} Mbps) — threshold met"
                : $"Download speed: {measuredMbps:F2} Mbps (minimum: {parameters.MinimumMbps:F2} Mbps) — threshold not met";
        }
        catch (HttpRequestException ex)
        {
            // HTTP error (T006)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;

            var message = ex.StatusCode.HasValue
                ? $"HTTP error: {(int)ex.StatusCode.Value} {ex.StatusCode.Value}"
                : $"HTTP request failed: {ex.Message}";
            result.HumanSummary = message;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = message
            };
        }
        catch (ArgumentException ex)
        {
            // Configuration error from ExtractParameters (T006)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = ex.Message;
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            // General error (T006)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = $"Unexpected error: {ex.Message}";
            result.Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = ex.Message
            };
        }
        finally
        {
            httpClient?.Dispose();
            handler?.Dispose();
        }

        return result;
    }

    /// <summary>
    /// Extracts and validates parameters from the test definition.
    /// </summary>
    private BandwidthTestParameters ExtractParameters(TestDefinition testDefinition)
    {
        var parameters = testDefinition.Parameters;

        // Extract url (required)
        var url = parameters["url"]?.ToString();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Parameter 'url' is required and cannot be empty");
        }

        // Validate URL scheme
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Parameter 'url' must start with http:// or https://");
        }

        // Extract minimumMbps (optional, default 0.0)
        var minimumMbps = 0.0;
        var minMbpsValue = parameters["minimumMbps"];
        if (minMbpsValue != null && !double.TryParse(minMbpsValue.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out minimumMbps))
        {
            throw new ArgumentException("Parameter 'minimumMbps' must be a valid number");
        }
        if (minimumMbps < 0)
        {
            throw new ArgumentException("Parameter 'minimumMbps' must be >= 0");
        }

        // Extract durationSeconds (optional, default 10)
        var durationSeconds = DefaultDurationSeconds;
        var durationValue = parameters["durationSeconds"];
        if (durationValue != null && !int.TryParse(durationValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out durationSeconds))
        {
            throw new ArgumentException("Parameter 'durationSeconds' must be a valid integer");
        }
        if (durationSeconds <= 0)
        {
            throw new ArgumentException("Parameter 'durationSeconds' must be > 0");
        }

        return new BandwidthTestParameters
        {
            Url = url,
            MinimumMbps = minimumMbps,
            DurationSeconds = durationSeconds
        };
    }
}
