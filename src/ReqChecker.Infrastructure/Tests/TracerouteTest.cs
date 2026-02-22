using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Traces the network path to a target host by sending ICMP echo requests with incrementing TTL values.
/// Records each hop's responding IP address and round-trip time.
/// </summary>
[TestType("Traceroute")]
public class TracerouteTest : ITest
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
        var hops = new List<Dictionary<string, object?>>();
        var reachedTarget = false;
        string? resolvedIp = null;

        try
        {
            // Get parameters
            var host = testDefinition.Parameters["host"]?.ToString() ?? string.Empty;
            var maxHops = testDefinition.Parameters["maxHops"]?.GetValue<int>() ?? 30;
            var timeoutMs = testDefinition.Parameters["timeout"]?.GetValue<int>() ?? 5000;

            // Validate parameters (FR-009)
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Host parameter is required", nameof(host));
            }

            if (maxHops <= 0)
            {
                throw new ArgumentException("maxHops must be a positive integer", nameof(maxHops));
            }

            // Resolve hostname to IP (FR-006)
            IPAddress[] addresses;
            try
            {
                addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"DNS resolution failed for host '{host}': {ex.Message}", ex);
            }

            if (addresses.Length == 0)
            {
                throw new InvalidOperationException($"DNS resolution returned no addresses for host '{host}'");
            }

            // Prefer IPv4 address since PingOptions only supports IPv4
            // This prevents PlatformNotSupportedException when IPv6 is returned first
            var targetAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            if (targetAddress == null)
            {
                // No IPv4 address available - check if IPv6 exists and provide clear error
                var ipv6Address = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6);
                if (ipv6Address != null)
                {
                    throw new InvalidOperationException(
                        $"Host '{host}' only has IPv6 addresses. Traceroute is not supported for IPv6 targets " +
                        "because .NET PingOptions does not support IPv6. Please use an IPv4 address or a hostname with IPv4 support.");
                }

                throw new InvalidOperationException($"DNS resolution returned no usable IP addresses for host '{host}'");
            }

            resolvedIp = targetAddress.ToString();

            // Trace route with incrementing TTL (FR-005, R1)
            using var ping = new Ping();
            var buffer = new byte[32]; // Standard ping buffer size

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var options = new PingOptions
                {
                    Ttl = ttl,
                    DontFragment = true
                };

                PingReply reply;
                try
                {
                    reply = await ping.SendPingAsync(targetAddress, timeoutMs, buffer, options);
                }
                catch (PingException)
                {
                    // Handle ping-specific exceptions
                    var hopEntry = new Dictionary<string, object?>
                    {
                        ["hop"] = ttl,
                        ["address"] = "*",
                        ["roundtripMs"] = null
                    };
                    hops.Add(hopEntry);
                    continue;
                }

                // Record hop data (FR-005)
                var hopData = new Dictionary<string, object?>
                {
                    ["hop"] = ttl,
                    ["address"] = reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired
                        ? reply.Address?.ToString() ?? "*"
                        : "*",
                    ["roundtripMs"] = reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired
                        ? (int?)reply.RoundtripTime
                        : null
                };

                // Handle timed-out hops (FR-010)
                if (reply.Status == IPStatus.TimedOut)
                {
                    hopData["address"] = "*";
                    hopData["roundtripMs"] = null;
                }

                hops.Add(hopData);

                // Check if target reached (FR-011)
                if (reply.Status == IPStatus.Success || reply.Address?.Equals(targetAddress) == true)
                {
                    reachedTarget = true;
                    break; // Stop early when target reached
                }
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Determine result: Pass when target reached, Fail otherwise (FR-008)
            result.Status = reachedTarget ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence dictionary with camelCase keys (per data-model.md)
            var evidence = new Dictionary<string, object?>
            {
                ["host"] = host,
                ["resolvedIp"] = resolvedIp,
                ["maxHops"] = maxHops,
                ["hopCount"] = hops.Count,
                ["reachedTarget"] = reachedTarget,
                ["hops"] = hops
            };

            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence),
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

            // Include partial hop data in evidence
            var evidence = new Dictionary<string, object?>
            {
                ["host"] = testDefinition.Parameters["host"]?.ToString() ?? string.Empty,
                ["resolvedIp"] = resolvedIp,
                ["maxHops"] = testDefinition.Parameters["maxHops"]?.GetValue<int>() ?? 30,
                ["hopCount"] = hops.Count,
                ["reachedTarget"] = reachedTarget,
                ["hops"] = hops
            };

            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds
                }
            };

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
        catch (InvalidOperationException ex)
        {
            // DNS resolution failures
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
