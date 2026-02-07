using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests DNS resolution for a hostname and optionally validates an expected IP address.
/// </summary>
[TestType("DnsResolve")]
public class DnsResolveTest : ITest
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
            // Get parameters
            var hostname = testDefinition.Parameters["hostname"]?.ToString() ?? string.Empty;
            var expectedAddress = testDefinition.Parameters["expectedAddress"]?.ToString();

            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentException("Hostname parameter is required", nameof(hostname));
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Resolve the hostname
            var addresses = await Dns.GetHostAddressesAsync(hostname, cancellationToken);

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Determine address family
            var hasIPv4 = addresses.Any(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            var hasIPv6 = addresses.Any(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
            string addressFamily;
            if (hasIPv4 && hasIPv6)
            {
                addressFamily = "Mixed";
            }
            else if (hasIPv4)
            {
                addressFamily = "IPv4";
            }
            else if (hasIPv6)
            {
                addressFamily = "IPv6";
            }
            else
            {
                addressFamily = "Unknown";
            }

            // Validate expected address if provided
            bool expectedAddressMatch = true;
            string? validationMessage = null;
            if (!string.IsNullOrEmpty(expectedAddress))
            {
                var normalizedExpected = expectedAddress.Trim();
                expectedAddressMatch = addresses.Any(a => a.ToString().Equals(normalizedExpected, StringComparison.OrdinalIgnoreCase));
                if (!expectedAddressMatch)
                {
                    validationMessage = $"Expected address '{expectedAddress}' not found in resolved addresses";
                }
            }

            // Determine status
            result.Status = expectedAddressMatch ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var resolvedAddressList = addresses.Select(a => a.ToString()).ToList();
            var evidence = new Dictionary<string, object>
            {
                ["hostname"] = hostname,
                ["addresses"] = resolvedAddressList,
                ["addressCount"] = resolvedAddressList.Count,
                ["addressFamily"] = addressFamily,
                ["resolutionTimeMs"] = stopwatch.ElapsedMilliseconds
            };

            if (!string.IsNullOrEmpty(expectedAddress))
            {
                evidence["expectedAddress"] = expectedAddress;
                evidence["matchFound"] = expectedAddressMatch;
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    evidence["validationMessage"] = validationMessage;
                }
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds,
                    ExecuteMs = (int)stopwatch.ElapsedMilliseconds
                }
            };

            // Set error if validation failed
            if (!expectedAddressMatch)
            {
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = validationMessage ?? "Expected address not found in resolved addresses"
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
                Category = ErrorCategory.Network,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }

        return result;
    }
}
