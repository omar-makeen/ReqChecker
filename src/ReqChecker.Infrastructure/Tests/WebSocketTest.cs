using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Evidence captured during WebSocket test execution.
/// </summary>
public class WebSocketTestEvidence
{
    /// <summary>
    /// The WebSocket URL that was tested.
    /// </summary>
    public string WsUrl { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the WebSocket handshake completed successfully.
    /// </summary>
    public bool Connected { get; set; }

    /// <summary>
    /// Time to complete the handshake in milliseconds.
    /// </summary>
    public long ConnectTimeMs { get; set; }

    /// <summary>
    /// Negotiated subprotocol (if requested and accepted by server).
    /// </summary>
    public string? Subprotocol { get; set; }

    /// <summary>
    /// The message sent to the server (only when message parameter is set).
    /// </summary>
    public string? MessageSent { get; set; }

    /// <summary>
    /// The response received from the server (text as-is, binary as hex).
    /// </summary>
    public string? MessageReceived { get; set; }

    /// <summary>
    /// Indicates whether the response matched the expected response.
    /// Only present when both message and expectedResponse parameters are set.
    /// </summary>
    public bool? ResponseMatched { get; set; }

    /// <summary>
    /// Type of message received: "text" or "binary".
    /// Only present when message exchange occurred.
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    /// WebSocket close status code name (e.g., "NormalClosure").
    /// </summary>
    public string? CloseStatus { get; set; }
}

/// <summary>
/// Tests WebSocket connectivity by performing a handshake and optional message exchange.
/// Supports ws:// and wss:// URLs, custom headers, and subprotocol negotiation.
/// </summary>
[TestType("WebSocket")]
public class WebSocketTest : ITest
{
    private const int DefaultTimeoutMs = 10000;
    private const int CloseTimeoutMs = 2000;
    private const int MaxReceiveBufferBytes = 8192;
    private const int MaxMessageDisplayLength = 500;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
        WebSocketTestParameters? parameters = null;
        ClientWebSocket? webSocket = null;

        try
        {
            // Extract and validate parameters
            parameters = ExtractParameters(testDefinition);
            cancellationToken.ThrowIfCancellationRequested();

            // Create WebSocket client
            webSocket = new ClientWebSocket();

            // Configure custom headers (T012)
            foreach (var (name, value) in parameters.Headers)
            {
                webSocket.Options.SetRequestHeader(name, value);
            }

            // Configure subprotocol (T013)
            if (!string.IsNullOrEmpty(parameters.Subprotocol))
            {
                webSocket.Options.AddSubProtocol(parameters.Subprotocol);
            }

            // Create linked CancellationTokenSource for timeout
            using var timeoutCts = new CancellationTokenSource(parameters.Timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token);

            // Connect to WebSocket server (T003)
            var connectStopwatch = Stopwatch.StartNew();
            var uri = new Uri(parameters.Url);
            await webSocket.ConnectAsync(uri, linkedCts.Token);
            connectStopwatch.Stop();

            var evidence = new WebSocketTestEvidence
            {
                WsUrl = parameters.Url,
                Connected = true,
                ConnectTimeMs = connectStopwatch.ElapsedMilliseconds,
                Subprotocol = webSocket.SubProtocol
            };

            // Message exchange (T008)
            if (!string.IsNullOrEmpty(parameters.Message))
            {
                var remainingTimeout = parameters.Timeout - (int)connectStopwatch.ElapsedMilliseconds;
                if (remainingTimeout > 0)
                {
                    var messageBytes = Encoding.UTF8.GetBytes(parameters.Message);
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(messageBytes),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        linkedCts.Token);

                    evidence.MessageSent = parameters.Message;

                    // Receive response - handle fragmented messages by looping until EndOfMessage
                    var buffer = new byte[MaxReceiveBufferBytes];
                    using var messageBuffer = new MemoryStream();
                    WebSocketReceiveResult receiveResult;
                    WebSocketMessageType receivedMessageType = WebSocketMessageType.Text;

                    do
                    {
                        receiveResult = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            linkedCts.Token);

                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                        {
                            receivedMessageType = receiveResult.MessageType;
                            messageBuffer.Write(buffer, 0, receiveResult.Count);
                        }
                    } while (!receiveResult.EndOfMessage && receiveResult.MessageType != WebSocketMessageType.Close);

                    // Check if server closed without sending any response data (P2 fix)
                    if (receiveResult.MessageType == WebSocketMessageType.Close && messageBuffer.Length == 0)
                    {
                        await CloseWebSocketAsync(webSocket);
                        stopwatch.Stop();
                        result.EndTime = DateTime.UtcNow;
                        result.Duration = stopwatch.Elapsed;
                        result.Status = TestStatus.Fail;
                        result.HumanSummary = "Server closed the connection without sending any response data";
                        result.Error = new TestError
                        {
                            Category = ErrorCategory.Network,
                            Message = "Server sent a Close frame without any data response"
                        };
                        result.Evidence = new TestEvidence
                        {
                            ResponseData = JsonSerializer.Serialize(evidence, s_jsonOptions)
                        };
                        return result;
                    }

                    // Store the full untruncated message for comparison
                    string? fullMessageReceived = null;
                    if (receivedMessageType == WebSocketMessageType.Text)
                    {
                        fullMessageReceived = Encoding.UTF8.GetString(messageBuffer.ToArray());
                        evidence.MessageType = "text";
                    }
                    else if (receivedMessageType == WebSocketMessageType.Binary)
                    {
                        // Display binary as hex
                        fullMessageReceived = BitConverter.ToString(messageBuffer.ToArray()).Replace("-", " ");
                        evidence.MessageType = "binary";
                    }

                    // Expected response validation (T009) - compare against FULL untruncated message
                    if (!string.IsNullOrEmpty(parameters.ExpectedResponse))
                    {
                        if (evidence.MessageType == "text")
                        {
                            evidence.ResponseMatched = fullMessageReceived == parameters.ExpectedResponse;

                            if (!evidence.ResponseMatched.Value)
                            {
                                // Truncate for display in error message
                                var displayMessage = fullMessageReceived?.Length > MaxMessageDisplayLength
                                    ? fullMessageReceived.Substring(0, MaxMessageDisplayLength) + "..."
                                    : fullMessageReceived;

                                // Close gracefully before returning failure
                                await CloseWebSocketAsync(webSocket);
                                stopwatch.Stop();
                                result.EndTime = DateTime.UtcNow;
                                result.Duration = stopwatch.Elapsed;
                                result.Status = TestStatus.Fail;
                                result.HumanSummary = $"Response mismatch: expected '{parameters.ExpectedResponse}', got '{displayMessage}'";
                                // Store truncated version in evidence for display
                                evidence.MessageReceived = displayMessage;
                                result.Evidence = new TestEvidence
                                {
                                    ResponseData = JsonSerializer.Serialize(evidence, s_jsonOptions)
                                };
                                return result;
                            }
                        }
                        else
                        {
                            // Binary response when text was expected - this is a mismatch
                            await CloseWebSocketAsync(webSocket);
                            stopwatch.Stop();
                            result.EndTime = DateTime.UtcNow;
                            result.Duration = stopwatch.Elapsed;
                            result.Status = TestStatus.Fail;
                            evidence.ResponseMatched = false;
                            result.HumanSummary = $"Expected text response but received binary frame";
                            result.Evidence = new TestEvidence
                            {
                                ResponseData = JsonSerializer.Serialize(evidence, s_jsonOptions)
                            };
                            return result;
                        }
                    }

                    // Truncate for display in evidence (after validation)
                    evidence.MessageReceived = fullMessageReceived?.Length > MaxMessageDisplayLength
                        ? fullMessageReceived.Substring(0, MaxMessageDisplayLength) + "..."
                        : fullMessageReceived;
                }
                else
                {
                    // Timeout exhausted before message exchange could start (P2 fix)
                    await CloseWebSocketAsync(webSocket);
                    stopwatch.Stop();
                    result.EndTime = DateTime.UtcNow;
                    result.Duration = stopwatch.Elapsed;
                    result.Status = TestStatus.Fail;
                    result.HumanSummary = $"Timeout exhausted before message exchange could start (handshake took {connectStopwatch.ElapsedMilliseconds}ms of {parameters.Timeout}ms timeout)";
                    result.Error = new TestError
                    {
                        Category = ErrorCategory.Timeout,
                        Message = "Handshake consumed the full timeout, leaving no time for message exchange"
                    };
                    result.Evidence = new TestEvidence
                    {
                        ResponseData = JsonSerializer.Serialize(evidence, s_jsonOptions)
                    };
                    return result;
                }
            }

            // Close gracefully
            var closeStatus = await CloseWebSocketAsync(webSocket);
            evidence.CloseStatus = closeStatus;

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Pass;
            result.HumanSummary = BuildSuccessSummary(evidence, parameters);
            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence, s_jsonOptions)
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = "Test cancelled by user";
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = "Test cancelled by user"
            };
        }
        catch (OperationCanceledException)
        {
            // Timeout (not user cancellation)
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = $"WebSocket connection timed out after {parameters?.Timeout ?? DefaultTimeoutMs}ms";
            result.Error = new TestError
            {
                Category = ErrorCategory.Timeout,
                Message = result.HumanSummary
            };
        }
        catch (WebSocketException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = $"WebSocket connection failed: {GetWebSocketErrorMessage(ex)}";
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = result.HumanSummary,
                ExceptionType = ex.GetType().FullName
            };
        }
        catch (SocketException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = GetSocketErrorMessage(ex);
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = result.HumanSummary,
                ExceptionType = ex.GetType().FullName
            };
        }
        catch (HttpRequestException ex)
        {
            // TLS/SSL errors
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            var host = ExtractHost(parameters?.Url);
            result.HumanSummary = $"TLS/SSL error connecting to {host}: {ex.Message}";
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = result.HumanSummary,
                ExceptionType = ex.GetType().FullName
            };
        }
        catch (UriFormatException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = "Invalid WebSocket URL: must start with ws:// or wss://";
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = result.HumanSummary,
                ExceptionType = ex.GetType().FullName
            };
        }
        catch (ArgumentException ex)
        {
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
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.HumanSummary = $"Unexpected error: {ex.Message}";
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = ex.Message,
                ExceptionType = ex.GetType().FullName
            };
        }
        finally
        {
            webSocket?.Dispose();
        }

        return result;
    }

    #region Parameter Extraction

    /// <summary>
    /// Extracts and validates parameters from the test definition.
    /// </summary>
    private WebSocketTestParameters ExtractParameters(TestDefinition testDefinition)
    {
        var parameters = testDefinition.Parameters;

        // URL (required)
        var url = parameters["url"]?.ToString();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL parameter is required and cannot be empty", "url");
        }

        // Validate URL scheme
        if (!url.StartsWith("ws://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid WebSocket URL: must start with ws:// or wss://", "url");
        }

        // Timeout (optional, default 10000ms)
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

        // Message (optional)
        string? message = parameters["message"]?.ToString();
        if (!string.IsNullOrEmpty(message) && message.Length > MaxReceiveBufferBytes)
        {
            throw new ArgumentException($"Message length exceeds maximum of {MaxReceiveBufferBytes} bytes", "message");
        }

        // Expected response (optional)
        string? expectedResponse = parameters["expectedResponse"]?.ToString();

        // Validate: expectedResponse requires message to be set
        if (!string.IsNullOrEmpty(expectedResponse) && string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("expectedResponse requires message to be set. Without a message to send, no response can be received and validated.", "expectedResponse");
        }

        // Headers (optional, JSON array of {name, value} objects)
        var headers = new List<(string Name, string Value)>();
        if (parameters["headers"] is JsonArray headersArray)
        {
            foreach (var header in headersArray)
            {
                if (header is JsonObject headerObj)
                {
                    var name = headerObj["name"]?.ToString();
                    var value = headerObj["value"]?.ToString();
                    if (!string.IsNullOrEmpty(name) && value != null)
                    {
                        headers.Add((name, value));
                    }
                }
            }
        }

        // Subprotocol (optional)
        string? subprotocol = parameters["subprotocol"]?.ToString();

        return new WebSocketTestParameters
        {
            Url = url,
            Timeout = timeout,
            Message = message,
            ExpectedResponse = expectedResponse,
            Headers = headers,
            Subprotocol = subprotocol
        };
    }

    /// <summary>
    /// Internal container for extracted test parameters.
    /// </summary>
    private class WebSocketTestParameters
    {
        public string Url { get; set; } = string.Empty;
        public int Timeout { get; set; }
        public string? Message { get; set; }
        public string? ExpectedResponse { get; set; }
        public List<(string Name, string Value)> Headers { get; set; } = new();
        public string? Subprotocol { get; set; }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Closes the WebSocket gracefully with a timeout.
    /// </summary>
    private async Task<string?> CloseWebSocketAsync(ClientWebSocket webSocket)
    {
        if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
        {
            try
            {
                using var closeCts = new CancellationTokenSource(CloseTimeoutMs);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", closeCts.Token);
                return webSocket.CloseStatus?.ToString() ?? "NormalClosure";
            }
            catch
            {
                // If graceful close fails, abort
                webSocket.Abort();
                return "Aborted";
            }
        }
        return null;
    }

    /// <summary>
    /// Builds a human-readable success summary.
    /// </summary>
    private static string BuildSuccessSummary(WebSocketTestEvidence evidence, WebSocketTestParameters parameters)
    {
        var sb = new StringBuilder();
        sb.Append($"Connected to {evidence.WsUrl} in {evidence.ConnectTimeMs}ms");

        if (!string.IsNullOrEmpty(evidence.Subprotocol))
        {
            sb.Append($" (subprotocol: {evidence.Subprotocol})");
        }

        if (!string.IsNullOrEmpty(evidence.MessageSent))
        {
            sb.Append($", sent message, received {evidence.MessageType} response");
            if (evidence.ResponseMatched == true)
            {
                sb.Append(" (matched)");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets a user-friendly error message for WebSocket exceptions.
    /// </summary>
    private static string GetWebSocketErrorMessage(WebSocketException ex)
    {
        return ex.WebSocketErrorCode switch
        {
            WebSocketError.ConnectionClosedPrematurely => "Connection closed prematurely by server",
            WebSocketError.InvalidMessageType => "Invalid message type received",
            WebSocketError.UnsupportedProtocol => "Unsupported WebSocket protocol",
            WebSocketError.UnsupportedVersion => "Unsupported WebSocket version",
            WebSocketError.HeaderError => "Invalid handshake headers from server",
            WebSocketError.Faulted => $"WebSocket faulted: {ex.Message}",
            _ => ex.Message
        };
    }

    /// <summary>
    /// Gets a user-friendly error message for socket exceptions.
    /// </summary>
    private static string GetSocketErrorMessage(SocketException ex)
    {
        return ex.SocketErrorCode switch
        {
            SocketError.ConnectionRefused => "Connection refused - server is not accepting connections",
            SocketError.HostNotFound => "Host not found - check the hostname",
            SocketError.HostUnreachable => "Host unreachable - network connectivity issue",
            SocketError.NetworkUnreachable => "Network unreachable",
            SocketError.TimedOut => "Connection timed out",
            SocketError.ConnectionReset => "Connection reset by server",
            SocketError.NotConnected => "Socket not connected",
            _ => $"Network error: {ex.Message}"
        };
    }

    /// <summary>
    /// Extracts the host from a URL for error messages.
    /// </summary>
    private static string? ExtractHost(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return url;
        }
    }

    #endregion
}
