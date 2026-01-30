using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests network connectivity to a host using ICMP ping.
/// </summary>
[TestType("Ping")]
public class PingTest : ITest
{
    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, CancellationToken cancellationToken = default)
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
            // Get parameters
            var host = testDefinition.Parameters["host"]?.ToString() ?? string.Empty;
            var timeoutMs = testDefinition.Parameters["timeout"]?.GetValue<int>() ?? 5000;
            var count = testDefinition.Parameters["count"]?.GetValue<int>() ?? 4;

            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Host parameter is required", nameof(host));
            }

            var ping = new Ping();
            var successfulPings = 0;
            var failedPings = 0;
            var totalRoundtripTime = 0L;
            var pingResults = new List<Dictionary<string, object>>();

            for (int i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var reply = await ping.SendPingAsync(host, timeoutMs);

                var pingResult = new Dictionary<string, object>
                {
                    ["attempt"] = i + 1,
                    ["address"] = reply.Address?.ToString() ?? "N/A",
                    ["status"] = reply.Status.ToString(),
                    ["roundtripTime"] = reply.RoundtripTime,
                    ["bufferLength"] = reply.Buffer?.Length ?? 0
                };

                pingResults.Add(pingResult);

                if (reply.Status == IPStatus.Success)
                {
                    successfulPings++;
                    totalRoundtripTime += reply.RoundtripTime;
                }
                else
                {
                    failedPings++;
                }

                // Small delay between pings
                if (i < count - 1)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Determine overall status
            result.Status = successfulPings > 0 ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["host"] = host,
                ["totalCount"] = count,
                ["successfulCount"] = successfulPings,
                ["failedCount"] = failedPings,
                ["successRate"] = count > 0 ? (double)successfulPings / count : 0,
                ["averageRoundtripTime"] = successfulPings > 0 ? totalRoundtripTime / successfulPings : 0,
                ["pingResults"] = pingResults
            };

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds,
                    ExecuteMs = successfulPings > 0 ? (int)(totalRoundtripTime / successfulPings) : 0
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
                Category = ErrorCategory.Network,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }

        return result;
    }
}
