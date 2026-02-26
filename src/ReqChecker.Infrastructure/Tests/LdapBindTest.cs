using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests LDAP/Active Directory server connectivity by performing a bind operation.
/// Supports anonymous and authenticated simple binds, with optional implicit SSL/TLS (LDAPS).
/// Referral chasing is disabled — only the specified server is evaluated.
/// </summary>
[TestType("LdapBind")]
public class LdapBindTest : ITest
{
    private const int DefaultLdapPort = 389;
    private const int DefaultLdapsPort = 636;
    private const int TimeoutMs = 10000;

    /// <inheritdoc/>
    /// <remarks>
    /// LdapConnection.Bind() is synchronous and does not accept a CancellationToken.
    /// Cancellation relies on connection.Timeout (10 s). The cancellationToken parameter
    /// is checked only if already cancelled before entry.
    /// </remarks>
    public Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new TestResult
        {
            TestId = testDefinition.Id,
            TestType = testDefinition.Type,
            DisplayName = testDefinition.DisplayName,
            Status = TestStatus.Fail,
            StartTime = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        // Sentinel values for "always-present" evidence keys on failure paths
        var server = string.Empty;
        var port = 0;
        var useSsl = false;
        var bindType = "n/a";
        var responseTimeMs = -1L;

        try
        {
            var parameters = testDefinition.Parameters;

            // Extract server (required)
            server = parameters["server"]?.ToString();
            if (string.IsNullOrEmpty(server))
            {
                throw new ArgumentException("Server parameter is required");
            }

            // Extract useSsl (optional, default false) — needed before port default
            var useSslValue = parameters["useSsl"];
            if (useSslValue != null)
            {
                var useSslStr = useSslValue.ToString();
                if (useSslStr != null && (useSslStr.Equals("true", StringComparison.OrdinalIgnoreCase) || useSslStr.Equals("1")))
                {
                    useSsl = true;
                }
            }

            // Extract port (optional, default depends on useSsl)
            port = useSsl ? DefaultLdapsPort : DefaultLdapPort;
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

            // credentialRef is optional — resolved by SequentialTestRunner and passed via context
            var credentialRefValue = parameters["credentialRef"]?.ToString();
            var hasCredentialRef = !string.IsNullOrEmpty(credentialRefValue);

            // Determine bind type based on whether credentials are available
            bindType = (hasCredentialRef && context?.Username != null) ? "authenticated" : "anonymous";

            // Warn if credentials were requested but context is null (credentialRef not resolved)
            if (hasCredentialRef && context?.Username == null)
            {
                throw new ArgumentException($"Credential reference '{credentialRefValue}' could not be resolved from the credential store");
            }

            // Create LDAP identifier and connection
            var identifier = new LdapDirectoryIdentifier(server, port, false, false);
            using var connection = new LdapConnection(identifier);

            // Configure connection options
            connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
            connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
            connection.Timeout = TimeSpan.FromMilliseconds(TimeoutMs);

            if (useSsl)
            {
                connection.SessionOptions.SecureSocketLayer = true;
            }

            // Perform bind
            stopwatch.Restart();

            if (bindType == "authenticated")
            {
                connection.AuthType = AuthType.Basic;
                connection.Credential = new NetworkCredential(context!.Username, context.Password ?? string.Empty);
                connection.Bind();
            }
            else
            {
                connection.AuthType = AuthType.Anonymous;
                connection.Bind();
            }

            stopwatch.Stop();
            responseTimeMs = stopwatch.ElapsedMilliseconds;
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Pass;

            // Build evidence — always-present keys
            var evidence = new Dictionary<string, object>
            {
                ["server"] = server,
                ["port"] = port,
                ["useSsl"] = useSsl,
                ["bindType"] = bindType,
                ["responseTimeMs"] = responseTimeMs
            };

            // Conditional: TLS info (only when useSsl requested)
            if (useSsl)
            {
                evidence["tlsNegotiated"] = true;
                // Attempt to read negotiated TLS version from session options
                try
                {
                    var sslInfo = connection.SessionOptions.SslInformation;
                    if (sslInfo != null)
                    {
                        evidence["tlsVersion"] = sslInfo.Protocol.ToString();
                    }
                }
                catch
                {
                    // SslInformation not available on all platforms — tlsNegotiated already set
                }
            }

            // Conditional: auth result (only when credentialRef was provided)
            if (hasCredentialRef)
            {
                evidence["authenticated"] = true;

                // Warn if credentials sent without encryption
                if (!useSsl)
                {
                    evidence["warning"] = "Credentials sent without encryption";
                }
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)responseTimeMs
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
            SetFailureEvidence(result, server, port, useSsl, bindType, -1L);
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
            SetFailureEvidence(result, server, port, useSsl, bindType, -1L);
        }
        catch (LdapException ex) when (ex.ErrorCode == 49)
        {
            // LDAP error 49 = Invalid credentials
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Permission,
                Message = "Authentication failed: invalid credentials"
            };
            SetFailureEvidence(result, server, port, useSsl, "authenticated", stopwatch.ElapsedMilliseconds, authenticated: false);
        }
        catch (LdapException ex) when (ex.ErrorCode == 53 || ex.ErrorCode == 8 || ex.Message.Contains("unwilling") || ex.Message.Contains("anonymous"))
        {
            // Server unwilling to perform / anonymous bind rejected
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = "LDAP server rejected anonymous bind"
            };
            SetFailureEvidence(result, server, port, useSsl, "anonymous", stopwatch.ElapsedMilliseconds);
        }
        catch (LdapException ex) when (ex.Message.Contains("TLS") || ex.Message.Contains("SSL") || ex.Message.Contains("certificate") || ex.Message.Contains("handshake"))
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
            SetFailureEvidence(result, server, port, useSsl, bindType, -1L);
        }
        catch (LdapException ex) when (ex.ErrorCode == 85 || ex.Message.Contains("timed out") || ex.Message.Contains("timeout"))
        {
            // LDAP error 85 = LDAP_TIMEOUT
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Timeout,
                Message = $"LDAP server unreachable: connection timed out"
            };
            SetFailureEvidence(result, server, port, useSsl, bindType, -1L);
        }
        catch (LdapException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = $"LDAP bind failed: {ex.Message}"
            };
            SetFailureEvidence(result, server, port, useSsl, bindType, stopwatch.ElapsedMilliseconds);
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
                Message = $"LDAP server unreachable: {ex.Message}"
            };
            SetFailureEvidence(result, server, port, useSsl, bindType, -1L);
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
            SetFailureEvidence(result, server, port, useSsl, bindType, -1L);
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// Populates failure-path evidence with always-present fields.
    /// Per FR-014: all "always" fields must appear on every outcome; use -1 for responseTimeMs as sentinel (rendered as n/a).
    /// </summary>
    private static void SetFailureEvidence(TestResult result, string? server, int port, bool useSsl, string bindType, long responseTimeMs, bool? authenticated = null)
    {
        var evidence = new Dictionary<string, object>
        {
            ["server"] = string.IsNullOrEmpty(server) ? "n/a" : server,
            ["port"] = port > 0 ? port : 0,
            ["useSsl"] = useSsl,
            ["bindType"] = bindType,
            ["responseTimeMs"] = responseTimeMs
        };

        if (authenticated.HasValue)
        {
            evidence["authenticated"] = authenticated.Value;
        }

        result.Evidence = new TestEvidence
        {
            ResponseData = JsonSerializer.Serialize(evidence),
            Timing = new TimingBreakdown
            {
                TotalMs = responseTimeMs > 0 ? (int)responseTimeMs : 0
            }
        };
    }
}
