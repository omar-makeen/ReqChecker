using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Globalization;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests network latency to a host using multiple ICMP ping samples.
/// Calculates average/min/max round-trip times and compares against a threshold.
/// </summary>
[TestType("Latency")]
public class LatencyTest : ITest
{
    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
    {
        var result = new TestResult
        {
            TestId = testDefinition.Id,
            TestType = testDefinition.Type,
            DisplayName = testDefinition.DisplayName,
            Status = TestStatus.Fail, // Default to fail, will update on success
            StartTime = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Extract and validate parameters
            var parameters = testDefinition.Parameters;

            // Extract host (required)
            var host = parameters["host"]?.ToString();
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Host parameter is required");
            }

            // Extract maxLatencyMs (optional, default 0.0)
            var maxThresholdMs = 0.0;
            var thresholdValue = parameters["maxLatencyMs"];
            if (thresholdValue != null)
            {
                if (!double.TryParse(thresholdValue.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out maxThresholdMs))
                {
                    throw new ArgumentException("Parameter 'maxLatencyMs' must be a valid number");
                }
            }
            if (maxThresholdMs < 0)
            {
                throw new ArgumentException("maxLatencyMs must be a non-negative number");
            }

            // Extract sampleCount (optional, default 4)
            var sampleCount = 4;
            var countValue = parameters["sampleCount"];
            if (countValue != null)
            {
                if (!int.TryParse(countValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out sampleCount))
                {
                    throw new ArgumentException("Parameter 'sampleCount' must be a valid integer");
                }
            }
            if (sampleCount <= 0)
            {
                throw new ArgumentException("sampleCount must be a positive integer");
            }

            // ICMP ping sampling loop
            using var ping = new Ping();
            var roundtripTimes = new List<long>();
            var successfulPings = 0;
            var failedPings = 0;
            const int timeoutMs = 5000;

            for (int i = 0; i < sampleCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var reply = await ping.SendPingAsync(host, timeoutMs);

                if (reply.Status == IPStatus.Success)
                {
                    roundtripTimes.Add(reply.RoundtripTime);
                    successfulPings++;
                }
                else
                {
                    failedPings++;
                }

                // Small delay between pings (skip after last)
                if (i < sampleCount - 1)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Calculate statistics and determine pass/fail
            if (roundtripTimes.Count == 0)
            {
                // All samples timed out
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Network,
                    Message = $"All {sampleCount} samples timed out — host {host} is unreachable"
                };
            }
            else
            {
                // Calculate latency statistics
                var averageLatencyMs = Math.Round(roundtripTimes.Average(), 2);
                var minLatencyMs = (double)roundtripTimes.Min();
                var maxLatencyMs = (double)roundtripTimes.Max();

                // Determine pass/fail
                bool thresholdMet;
                if (maxThresholdMs == 0)
                {
                    // Reachability mode: pass if any sample succeeded
                    thresholdMet = true;
                    result.Status = TestStatus.Pass;
                }
                else if (averageLatencyMs <= maxThresholdMs)
                {
                    thresholdMet = true;
                    result.Status = TestStatus.Pass;
                }
                else
                {
                    thresholdMet = false;
                    result.Status = TestStatus.Fail;
                    result.Error = new TestError
                    {
                        Category = ErrorCategory.Network,
                        Message = $"Average latency {averageLatencyMs}ms exceeds threshold {maxThresholdMs}ms"
                    };
                }

                // Build evidence
                var evidence = new Dictionary<string, object>
                {
                    ["host"] = host,
                    ["averageLatencyMs"] = averageLatencyMs,
                    ["minLatencyMs"] = minLatencyMs,
                    ["maxLatencyMs"] = maxLatencyMs,
                    ["maxThresholdMs"] = maxThresholdMs,
                    ["sampleCount"] = sampleCount,
                    ["successfulCount"] = successfulPings,
                    ["thresholdMet"] = thresholdMet
                };

                result.Evidence = new TestEvidence
                {
                    ResponseData = JsonSerializer.Serialize(evidence),
                    Timing = new TimingBreakdown
                    {
                        TotalMs = (int)stopwatch.ElapsedMilliseconds,
                        ExecuteMs = (int)averageLatencyMs
                    }
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
                Message = ex.Message
            };
        }
        catch (PingException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = ex.Message.Contains("could not be resolved", StringComparison.OrdinalIgnoreCase)
                    ? $"Hostname could not be resolved: {ex.Message}"
                    : ex.Message,
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
                Category = ErrorCategory.Network,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }

        return result;
    }
}
