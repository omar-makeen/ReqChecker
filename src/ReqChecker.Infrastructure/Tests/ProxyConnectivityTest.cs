using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests HTTP/SOCKS proxy connectivity by connecting to a target URL through a specified proxy server.
/// Supports HTTP, HTTPS, SOCKS4, and SOCKS5 proxies with optional authentication.
/// </summary>
[TestType("ProxyConnectivity")]
public class ProxyConnectivityTest : ITest
{
    private const int DefaultTimeoutMs = 30000;

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
        ProxyTestParameters? parameters = null;
        HttpClientHandler? handler = null;
        HttpClient? httpClient = null;

        try
        {
            // Extract and validate parameters (T002)
            parameters = ExtractParameters(testDefinition);
            cancellationToken.ThrowIfCancellationRequested();

            // Configure proxy (T003)
            var webProxy = new WebProxy(parameters.ProxyUrl, false);

            // Add credentials if provided (T009)
            if (!string.IsNullOrEmpty(parameters.ProxyUsername) && parameters.ProxyPassword != null)
            {
                webProxy.Credentials = new NetworkCredential(parameters.ProxyUsername, parameters.ProxyPassword);
            }

            // Create HttpClientHandler with proxy (T003)
            handler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = webProxy,
                AllowAutoRedirect = true,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            // Create HttpClient (T003)
            httpClient = new HttpClient(handler);

            // Create linked CancellationTokenSource for timeout (T003)
            using var timeoutCts = new CancellationTokenSource(parameters.Timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token);

            // Send request through proxy (T003)
            var requestStopwatch = Stopwatch.StartNew();
            using var request = new HttpRequestMessage(HttpMethod.Get, parameters.TestUrl);
            using var response = await httpClient.SendAsync(request, linkedCts.Token);
            requestStopwatch.Stop();

            // Capture evidence (T003)
            var evidence = new Dictionary<string, object>
            {
                ["proxyUrl"] = parameters.ProxyUrl,
                ["testUrl"] = parameters.TestUrl,
                ["proxyType"] = InferProxyType(parameters.ProxyUrl),
                ["proxyReached"] = true,
                ["targetReached"] = true,
                ["connectTimeMs"] = requestStopwatch.ElapsedMilliseconds,
                ["statusCode"] = (int)response.StatusCode
            };

            // Check for proxy authentication required (407) - T009
            // HttpClient returns 407 as a response, not an exception
            if (response.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;

                string errorMessage;
                if (string.IsNullOrEmpty(parameters.ProxyUsername))
                {
                    errorMessage = "Proxy requires authentication — provide proxyUsername and proxyPassword parameters";
                    evidence["authSucceeded"] = false;
                }
                else
                {
                    errorMessage = "Proxy authentication failed — verify credentials";
                    evidence["authSucceeded"] = false;
                    evidence["proxyUsername"] = parameters.ProxyUsername;
                }

                result.HumanSummary = errorMessage;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Permission,
                    Message = errorMessage
                };
                result.Evidence = new TestEvidence
                {
                    ResponseData = JsonSerializer.Serialize(evidence)
                };
                return result;
            }

            // Add authentication evidence if credentials were provided (T009)
            // Only set authSucceeded = true if we didn't get a 407 response
            if (!string.IsNullOrEmpty(parameters.ProxyUsername))
            {
                evidence["proxyUsername"] = parameters.ProxyUsername;
                evidence["authSucceeded"] = true;
            }

            // Check for expected status (T004)
            var statusCode = (int)response.StatusCode;
            if (parameters.ExpectedStatus.HasValue && statusCode != parameters.ExpectedStatus.Value)
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.HumanSummary = $"Status mismatch: expected {parameters.ExpectedStatus.Value}, got {statusCode}";
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = result.HumanSummary
                };
                result.Evidence = new TestEvidence
                {
                    ResponseData = JsonSerializer.Serialize(evidence)
                };
                return result;
            }

            // When expectedStatus is not specified, default behavior is to pass only on 2xx/3xx
            if (!parameters.ExpectedStatus.HasValue && (statusCode < 200 || statusCode >= 400))
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.HumanSummary = $"Unexpected status code: got {statusCode}, expected 2xx or 3xx (default behavior when expectedStatus is not specified)";
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = result.HumanSummary
                };
                result.Evidence = new TestEvidence
                {
                    ResponseData = JsonSerializer.Serialize(evidence)
                };
                return result;
            }

            // Success (T003)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Pass;
            result.HumanSummary = BuildSuccessSummary(evidence, parameters, statusCode, response.StatusCode.ToString());
            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence)
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // User cancellation (T004)
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
            // Timeout (T004)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = $"Proxy connection timed out after {parameters?.Timeout ?? DefaultTimeoutMs}ms";
            result.Error = new TestError
            {
                Category = ErrorCategory.Timeout,
                Message = result.HumanSummary
            };
        }
        catch (HttpRequestException ex)
        {
            // HTTP/Proxy errors (T004)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;

            var evidence = CreateErrorEvidence(parameters);
            string errorMessage;

            // Check for proxy authentication required (T009)
            if (ex.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
            {
                if (string.IsNullOrEmpty(parameters?.ProxyUsername))
                {
                    errorMessage = "Proxy requires authentication — provide proxyUsername and proxyPassword parameters";
                    evidence["authSucceeded"] = false;
                }
                else
                {
                    errorMessage = "Proxy authentication failed — verify credentials";
                    evidence["authSucceeded"] = false;
                    evidence["proxyUsername"] = parameters.ProxyUsername;
                }
                result.Error = new TestError
                {
                    Category = ErrorCategory.Permission,
                    Message = errorMessage
                };
            }
            // Check for SOCKS-specific errors (T008)
            else if (IsSocksProxy(parameters?.ProxyUrl) && IsSocksProtocolError(ex))
            {
                var proxyType = InferProxyType(parameters?.ProxyUrl ?? "");
                errorMessage = $"SOCKS connection failed — verify the proxy supports {proxyType.ToUpperInvariant()} protocol";
                result.Error = new TestError
                {
                    Category = ErrorCategory.Network,
                    Message = errorMessage
                };
            }
            // Distinguish proxy vs target errors (T004)
            else if (IsProxyConnectionError(ex))
            {
                evidence["proxyReached"] = false;
                evidence["targetReached"] = false;
                errorMessage = $"Failed to connect to proxy server: {GetInnermostMessage(ex)}";
                result.Error = new TestError
                {
                    Category = ErrorCategory.Network,
                    Message = errorMessage
                };
            }
            else
            {
                evidence["proxyReached"] = true;
                evidence["targetReached"] = false;
                errorMessage = $"Failed to reach target through proxy: {GetInnermostMessage(ex)}";
                result.Error = new TestError
                {
                    Category = ErrorCategory.Network,
                    Message = errorMessage
                };
            }

            result.HumanSummary = errorMessage;
            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence)
            };
        }
        catch (SocketException ex)
        {
            // Network-level proxy errors (T004)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;

            var evidence = CreateErrorEvidence(parameters);
            evidence["proxyReached"] = false;
            evidence["targetReached"] = false;

            result.HumanSummary = $"Network error connecting to proxy: {GetSocketErrorMessage(ex)}";
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = result.HumanSummary,
                ExceptionType = ex.GetType().FullName
            };
            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence)
            };
        }
        catch (UriFormatException ex)
        {
            // Malformed URLs (T004)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = "Invalid URL format: URLs must be properly formatted";
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = result.HumanSummary,
                ExceptionType = ex.GetType().FullName
            };
        }
        catch (ArgumentException ex)
        {
            // Parameter validation errors (T004)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = ex.Message;
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = ex.Message,
                ExceptionType = ex.GetType().FullName
            };
        }
        catch (Exception ex)
        {
            // General errors (T004)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = $"Unexpected error: {ex.Message}";
            result.Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = ex.Message,
                ExceptionType = ex.GetType().FullName
            };
        }
        finally
        {
            httpClient?.Dispose();
            handler?.Dispose();
        }

        return result;
    }

    #region Parameter Extraction

    /// <summary>
    /// Extracts and validates parameters from the test definition.
    /// </summary>
    private ProxyTestParameters ExtractParameters(TestDefinition testDefinition)
    {
        var parameters = testDefinition.Parameters;

        // Proxy URL (required)
        var proxyUrl = parameters["proxyUrl"]?.ToString();
        if (string.IsNullOrWhiteSpace(proxyUrl))
        {
            throw new ArgumentException("proxyUrl parameter is required and cannot be empty", "proxyUrl");
        }

        // Validate proxy URL scheme
        if (!proxyUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !proxyUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
            !proxyUrl.StartsWith("socks4://", StringComparison.OrdinalIgnoreCase) &&
            !proxyUrl.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid proxy URL: must start with http://, https://, socks4://, or socks5://", "proxyUrl");
        }

        // Test URL (required)
        var testUrl = parameters["testUrl"]?.ToString();
        if (string.IsNullOrWhiteSpace(testUrl))
        {
            throw new ArgumentException("testUrl parameter is required and cannot be empty", "testUrl");
        }

        // Validate test URL scheme
        if (!testUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !testUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid test URL: must start with http:// or https://", "testUrl");
        }

        // Timeout (optional, default 30000ms)
        int timeout = DefaultTimeoutMs;
        var timeoutValue = parameters["timeout"];
        if (timeoutValue != null && int.TryParse(timeoutValue.ToString(), out int parsedTimeout))
        {
            if (parsedTimeout <= 0)
            {
                throw new ArgumentException("Timeout must be a positive integer", "timeout");
            }
            timeout = parsedTimeout;
        }

        // Expected status (optional)
        int? expectedStatus = null;
        var expectedStatusValue = parameters["expectedStatus"];
        if (expectedStatusValue != null && int.TryParse(expectedStatusValue.ToString(), out int parsedStatus))
        {
            if (parsedStatus < 100 || parsedStatus > 599)
            {
                throw new ArgumentException("Expected status must be a valid HTTP status code (100-599)", "expectedStatus");
            }
            expectedStatus = parsedStatus;
        }

        // Proxy username (optional)
        string? proxyUsername = parameters["proxyUsername"]?.ToString();
        if (!string.IsNullOrEmpty(proxyUsername))
        {
            proxyUsername = proxyUsername.Trim();
        }

        // Proxy password (optional)
        string? proxyPassword = parameters["proxyPassword"]?.ToString();

        return new ProxyTestParameters
        {
            ProxyUrl = proxyUrl,
            TestUrl = testUrl,
            Timeout = timeout,
            ExpectedStatus = expectedStatus,
            ProxyUsername = proxyUsername,
            ProxyPassword = proxyPassword
        };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Infers the proxy type from the proxy URL scheme.
    /// </summary>
    private static string InferProxyType(string proxyUrl)
    {
        if (string.IsNullOrEmpty(proxyUrl))
            return "unknown";

        if (proxyUrl.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase))
            return "socks5";
        if (proxyUrl.StartsWith("socks4://", StringComparison.OrdinalIgnoreCase))
            return "socks4";
        if (proxyUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return "https";
        if (proxyUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return "http";

        return "unknown";
    }

    /// <summary>
    /// Checks if the proxy URL uses SOCKS protocol.
    /// </summary>
    private static bool IsSocksProxy(string? proxyUrl)
    {
        if (string.IsNullOrEmpty(proxyUrl))
            return false;

        return proxyUrl.StartsWith("socks4://", StringComparison.OrdinalIgnoreCase) ||
               proxyUrl.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the exception indicates a SOCKS protocol mismatch or error.
    /// </summary>
    private static bool IsSocksProtocolError(HttpRequestException ex)
    {
        var message = ex.Message.ToLowerInvariant();
        var innerMessage = ex.InnerException?.Message?.ToLowerInvariant() ?? "";

        return message.Contains("socks") ||
               innerMessage.Contains("socks") ||
               message.Contains("protocol") ||
               innerMessage.Contains("protocol") ||
               message.Contains("invalid response") ||
               innerMessage.Contains("invalid response");
    }

    /// <summary>
    /// Determines if the error is a proxy connection error vs target error.
    /// </summary>
    private static bool IsProxyConnectionError(HttpRequestException ex)
    {
        var message = ex.Message.ToLowerInvariant();
        var innerMessage = ex.InnerException?.Message?.ToLowerInvariant() ?? "";

        // Connection refused, no route to host, etc. indicate proxy is unreachable
        return message.Contains("connection refused") ||
               message.Contains("no route to host") ||
               message.Contains("network is unreachable") ||
               message.Contains("the remote name could not be resolved") ||
               innerMessage.Contains("connection refused") ||
               innerMessage.Contains("no route to host") ||
               innerMessage.Contains("network is unreachable") ||
               innerMessage.Contains("the remote name could not be resolved") ||
               ex.InnerException is SocketException;
    }

    /// <summary>
    /// Gets the innermost exception message.
    /// </summary>
    private static string GetInnermostMessage(Exception ex)
    {
        var current = ex;
        while (current.InnerException != null)
        {
            current = current.InnerException;
        }
        return current.Message;
    }

    /// <summary>
    /// Gets a user-friendly socket error message.
    /// </summary>
    private static string GetSocketErrorMessage(SocketException ex)
    {
        return ex.SocketErrorCode switch
        {
            SocketError.HostNotFound => "Proxy host not found — check the proxy URL hostname",
            SocketError.HostUnreachable => "Proxy host unreachable — check network connectivity",
            SocketError.NetworkUnreachable => "Network unreachable — check network configuration",
            SocketError.ConnectionRefused => "Connection refused — proxy server may not be running or port is incorrect",
            SocketError.TimedOut => "Connection timed out — proxy server may be overloaded or unreachable",
            SocketError.ConnectionReset => "Connection reset by proxy — proxy may have rejected the connection",
            _ => $"{ex.Message} (socket error: {ex.SocketErrorCode})"
        };
    }

    /// <summary>
    /// Creates an evidence dictionary for error cases.
    /// </summary>
    private static Dictionary<string, object> CreateErrorEvidence(ProxyTestParameters? parameters)
    {
        var evidence = new Dictionary<string, object>();

        if (parameters != null)
        {
            evidence["proxyUrl"] = parameters.ProxyUrl;
            evidence["testUrl"] = parameters.TestUrl;
            evidence["proxyType"] = InferProxyType(parameters.ProxyUrl);
            evidence["proxyReached"] = false;
            evidence["targetReached"] = false;
            evidence["connectTimeMs"] = 0L;
        }

        return evidence;
    }

    /// <summary>
    /// Builds a human-readable success summary.
    /// </summary>
    private static string BuildSuccessSummary(Dictionary<string, object> evidence, ProxyTestParameters parameters, int statusCode, string statusText)
    {
        var proxyType = evidence["proxyType"].ToString();
        var connectTime = evidence["connectTimeMs"];

        var authPart = !string.IsNullOrEmpty(parameters.ProxyUsername) ? " with authentication" : "";
        var statusPart = $"{statusCode} {statusText}";

        return $"Connected to {parameters.TestUrl} via {proxyType} proxy{authPart} in {connectTime}ms ({statusPart})";
    }

    #endregion

    #region Nested Types

    /// <summary>
    /// Holds extracted and validated parameters for the proxy test.
    /// </summary>
    private class ProxyTestParameters
    {
        public string ProxyUrl { get; set; } = string.Empty;
        public string TestUrl { get; set; } = string.Empty;
        public int Timeout { get; set; } = DefaultTimeoutMs;
        public int? ExpectedStatus { get; set; }
        public string? ProxyUsername { get; set; }
        public string? ProxyPassword { get; set; }
    }

    #endregion
}
