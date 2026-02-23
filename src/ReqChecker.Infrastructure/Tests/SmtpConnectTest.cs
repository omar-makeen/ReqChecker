using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests SMTP mail server connectivity by performing a TCP connection,
/// reading the server greeting banner, optionally negotiating TLS (STARTTLS or implicit TLS),
/// and optionally authenticating with stored credentials.
/// </summary>
[TestType("SmtpConnect")]
public class SmtpConnectTest : ITest
{
    private const int DefaultPort = 25;
    private const int ImplicitTlsPort = 465;
    private const int TimeoutMs = 10000;

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
        TcpClient? tcpClient = null;
        Stream? stream = null;
        StreamReader? reader = null;
        StreamWriter? writer = null;
        SslStream? sslStream = null;

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

            // Extract port (optional, default 25)
            var port = DefaultPort;
            var portValue = parameters["port"];
            if (portValue != null)
            {
                if (!int.TryParse(portValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
                {
                    throw new ArgumentException("Parameter 'port' must be a valid integer");
                }
            }
            if (port < 1 || port > 65535)
            {
                throw new ArgumentException("Port must be between 1 and 65535");
            }

            // Extract useTls (optional, default false)
            var useTls = false;
            var useTlsValue = parameters["useTls"];
            if (useTlsValue != null)
            {
                var useTlsStr = useTlsValue.ToString();
                if (useTlsStr != null && (useTlsStr.Equals("true", StringComparison.OrdinalIgnoreCase) || useTlsStr.Equals("1")))
                {
                    useTls = true;
                }
            }

            // credentialRef is optional - resolved by SequentialTestRunner and passed via context

            // Create cancellation token with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeoutMs);

            // Connect to SMTP server
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cts.Token);

            stream = tcpClient.GetStream();

            // For implicit TLS (port 465), wrap in SslStream before reading banner
            if (useTls && port == ImplicitTlsPort)
            {
                sslStream = new SslStream(stream, false, (sender, certificate, chain, errors) => true);
                await sslStream.AuthenticateAsClientAsync(host, null, SslProtocols.None, false);
                stream = sslStream;
            }

            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Read banner
            var banner = await reader.ReadLineAsync(cts.Token);
            if (string.IsNullOrEmpty(banner))
            {
                throw new InvalidOperationException("Server did not respond with a valid SMTP banner: (null)");
            }

            // Check for 4xx/5xx error responses
            if (banner.StartsWith("4") || banner.StartsWith("5"))
            {
                throw new InvalidOperationException($"SMTP server returned error: {banner}");
            }

            if (!banner.StartsWith("220"))
            {
                throw new InvalidOperationException($"Server did not respond with a valid SMTP banner: {banner}");
            }

            // Send EHLO
            await writer.WriteLineAsync("EHLO ReqChecker");
            var ehloResponse = await ReadMultilineResponseAsync(reader, cts.Token);
            var ehloCapabilities = ParseEhloCapabilities(ehloResponse);

            // Handle STARTTLS for non-465 ports when useTls is true
            var tlsNegotiated = false;
            var tlsVersion = string.Empty;

            if (useTls && port != ImplicitTlsPort)
            {
                // Check if STARTTLS is supported
                if (!ehloCapabilities.Contains("STARTTLS"))
                {
                    throw new InvalidOperationException("TLS negotiation failed: server does not support STARTTLS");
                }

                // Send STARTTLS
                await writer.WriteLineAsync("STARTTLS");
                var startTlsResponse = await reader.ReadLineAsync(cts.Token);
                if (string.IsNullOrEmpty(startTlsResponse) || !startTlsResponse.StartsWith("220"))
                {
                    throw new InvalidOperationException($"TLS negotiation failed: server rejected STARTTLS - {startTlsResponse}");
                }

                // Wrap stream in SslStream
                sslStream = new SslStream(stream, false, (sender, certificate, chain, errors) => true);
                await sslStream.AuthenticateAsClientAsync(host, null, SslProtocols.None, false);
                stream = sslStream;

                // Old reader/writer are intentionally not disposed here — disposing them
                // would close the underlying stream that SslStream now wraps. They become
                // unreferenced and will be collected by the GC.
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                // Re-send EHLO over encrypted connection
                await writer.WriteLineAsync("EHLO ReqChecker");
                ehloResponse = await ReadMultilineResponseAsync(reader, cts.Token);
                ehloCapabilities = ParseEhloCapabilities(ehloResponse);

                tlsNegotiated = true;
                tlsVersion = sslStream.SslProtocol.ToString();
            }
            else if (useTls && port == ImplicitTlsPort)
            {
                tlsNegotiated = true;
                tlsVersion = sslStream!.SslProtocol.ToString();
            }

            // Handle SMTP authentication if credentials are provided
            var authenticated = false;

            if (context?.Username != null)
            {
                var password = context.Password ?? string.Empty;

                // Prefer AUTH LOGIN, fall back to AUTH PLAIN
                if (ehloCapabilities.Contains("AUTH LOGIN") || ehloCapabilities.Any(c => c.Contains("AUTH") && c.Contains("LOGIN")))
                {
                    // AUTH LOGIN sequence
                    await writer.WriteLineAsync("AUTH LOGIN");
                    var authPrompt1 = await reader.ReadLineAsync(cts.Token);
                    if (string.IsNullOrEmpty(authPrompt1) || !authPrompt1.StartsWith("334"))
                    {
                        throw new InvalidOperationException($"SMTP authentication failed: unexpected response to AUTH LOGIN - {authPrompt1}");
                    }

                    // Send Base64-encoded username
                    var usernameB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(context.Username));
                    await writer.WriteLineAsync(usernameB64);
                    var authPrompt2 = await reader.ReadLineAsync(cts.Token);
                    if (string.IsNullOrEmpty(authPrompt2) || !authPrompt2.StartsWith("334"))
                    {
                        throw new InvalidOperationException($"SMTP authentication failed: unexpected response to username - {authPrompt2}");
                    }

                    // Send Base64-encoded password
                    var passwordB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
                    await writer.WriteLineAsync(passwordB64);
                    var authResponse = await reader.ReadLineAsync(cts.Token);

                    if (string.IsNullOrEmpty(authResponse) || !authResponse.StartsWith("235"))
                    {
                        throw new InvalidOperationException($"SMTP authentication failed: {authResponse}");
                    }

                    authenticated = true;
                }
                else if (ehloCapabilities.Contains("AUTH PLAIN") || ehloCapabilities.Any(c => c.Contains("AUTH") && c.Contains("PLAIN")))
                {
                    // AUTH PLAIN - single message with \0username\0password
                    var authString = $"\0{context.Username}\0{password}";
                    var authB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
                    await writer.WriteLineAsync($"AUTH PLAIN {authB64}");
                    var authResponse = await reader.ReadLineAsync(cts.Token);

                    if (string.IsNullOrEmpty(authResponse) || !authResponse.StartsWith("235"))
                    {
                        throw new InvalidOperationException($"SMTP authentication failed: {authResponse}");
                    }

                    authenticated = true;
                }
                else
                {
                    throw new InvalidOperationException("SMTP authentication failed: server does not advertise AUTH LOGIN or AUTH PLAIN");
                }
            }

            // Send QUIT
            await writer.WriteLineAsync("QUIT");
            _ = await reader.ReadLineAsync(cts.Token);

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Pass;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["host"] = host,
                ["port"] = port,
                ["useTls"] = useTls,
                ["serverBanner"] = banner,
                ["responseTimeMs"] = stopwatch.ElapsedMilliseconds
            };

            if (tlsNegotiated)
            {
                evidence["tlsNegotiated"] = true;
                evidence["tlsVersion"] = tlsVersion;
            }

            if (context?.Username != null)
            {
                evidence["authenticated"] = authenticated;

                // Warn if credentials were sent without TLS encryption
                if (!useTls)
                {
                    evidence["warning"] = "Credentials were sent in cleartext without TLS encryption";
                }
            }

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
            result.Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = "Test was cancelled or timed out"
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
        catch (SocketException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = $"SMTP server unreachable: {ex.Message}"
            };
        }
        catch (IOException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = $"Connection reset by server: {ex.Message}"
            };
        }
        catch (AuthenticationException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = $"TLS negotiation failed: {ex.Message}"
            };
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("TLS negotiation failed") || ex.Message.Contains("SMTP authentication failed"))
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            var category = ex.Message.Contains("TLS") ? ErrorCategory.Network : ErrorCategory.Validation;
            result.Error = new TestError
            {
                Category = category,
                Message = ex.Message
            };
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("SMTP banner") || ex.Message.Contains("SMTP server returned error"))
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Validation,
                Message = ex.Message
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
        finally
        {
            writer?.Dispose();
            reader?.Dispose();
            stream?.Dispose();
            tcpClient?.Dispose();
        }

        return result;
    }

    /// <summary>
    /// Reads a multi-line SMTP response (lines starting with code followed by "-" continue, " " terminates).
    /// </summary>
    private static async Task<string> ReadMultilineResponseAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var response = new StringBuilder();
        string? line;
        var firstLine = true;
        var code = string.Empty;

        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (firstLine)
            {
                if (line.Length >= 3)
                {
                    code = line.Substring(0, 3);
                }
                firstLine = false;
            }

            response.AppendLine(line);

            // Check if this is the last line (code followed by space, not dash)
            if (line.Length >= 4 && line.Substring(0, 3) == code && line[3] == ' ')
            {
                break;
            }

            // Also break if line is too short to have continuation marker
            if (line.Length < 4)
            {
                break;
            }
        }

        return response.ToString();
    }

    /// <summary>
    /// Parses EHLO response to extract advertised capabilities.
    /// </summary>
    private static HashSet<string> ParseEhloCapabilities(string ehloResponse)
    {
        var capabilities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = ehloResponse.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Skip the first line (220 greeting) and any line that's just a status code
            if (line.Length > 4)
            {
                // Format: "250-SIZE 35882577" or "250 SIZE 35882577"
                var capability = line.Substring(4).Trim().ToUpperInvariant();
                if (!string.IsNullOrEmpty(capability))
                {
                    capabilities.Add(capability);
                }
            }
        }

        return capabilities;
    }
}
