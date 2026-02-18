using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace ReqChecker.Infrastructure.Tests;

// T001: Evidence class

/// <summary>
/// Evidence captured during SSL/TLS certificate expiry test execution.
/// </summary>
public class CertificateExpiryTestEvidence
{
    /// <summary>Target hostname used for the TLS connection.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Target port used for the TLS connection.</summary>
    public int Port { get; set; }

    /// <summary>Certificate Subject Distinguished Name.</summary>
    public string? Subject { get; set; }

    /// <summary>Certificate Issuer Distinguished Name.</summary>
    public string? Issuer { get; set; }

    /// <summary>Certificate SHA-1 thumbprint (hex, uppercase).</summary>
    public string? Thumbprint { get; set; }

    /// <summary>Certificate validity start date (UTC).</summary>
    public DateTime? NotBefore { get; set; }

    /// <summary>Certificate validity end date (UTC).</summary>
    public DateTime? NotAfter { get; set; }

    /// <summary>Days from now until NotAfter. Negative means already expired.</summary>
    public int? DaysUntilExpiry { get; set; }

    /// <summary>True if NotAfter is in the past.</summary>
    public bool IsExpired { get; set; }

    /// <summary>True if NotBefore is in the future.</summary>
    public bool IsNotYetValid { get; set; }

    /// <summary>True if the certificate expires within the configured warning window.</summary>
    public bool ExpiresWithinWarningWindow { get; set; }

    /// <summary>DNS names from the Subject Alternative Names (SAN) extension.</summary>
    public string[]? SubjectAlternativeNames { get; set; }

    /// <summary>Negotiated TLS protocol version (e.g., "Tls13").</summary>
    public string? TlsProtocolVersion { get; set; }

    /// <summary>TLS handshake time in milliseconds.</summary>
    public int? ResponseTimeMs { get; set; }

    /// <summary>Whether certificate chain validation was bypassed.</summary>
    public bool ChainValidationSkipped { get; set; }

    /// <summary>Additional error or diagnostic information.</summary>
    public string? ErrorDetail { get; set; }
}

// T002: Parameters class

/// <summary>
/// Internal container for extracted and validated test parameters.
/// </summary>
internal class CertificateExpiryParameters
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 443;
    public int WarningDaysBeforeExpiry { get; set; } = 30;
    public int Timeout { get; set; } = 10000;
    public bool SkipChainValidation { get; set; }
    public string? ExpectedSubject { get; set; }
    public string? ExpectedIssuer { get; set; }
    public string? ExpectedThumbprint { get; set; }
}

/// <summary>
/// Tests SSL/TLS certificate validity on a remote endpoint.
/// Connects via direct TLS (not STARTTLS), retrieves the server's leaf certificate,
/// and validates its expiry window and optionally its identity attributes.
/// </summary>
[TestType("CertificateExpiry")]
public class CertificateExpiryTest : ITest
{
    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
    {
        // T007: ExecuteAsync orchestration
        var result = new TestResult
        {
            TestId = testDefinition.Id,
            TestType = testDefinition.Type,
            DisplayName = testDefinition.DisplayName,
            Status = TestStatus.Fail,
            StartTime = DateTimeOffset.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();
        CertificateExpiryParameters? parameters = null;
        CertificateExpiryTestEvidence? evidence = null;

        try
        {
            parameters = ExtractParameters(testDefinition);
            cancellationToken.ThrowIfCancellationRequested();

            evidence = new CertificateExpiryTestEvidence
            {
                Host = parameters.Host,
                Port = parameters.Port,
                ChainValidationSkipped = parameters.SkipChainValidation
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(parameters.Timeout);

            var cert = await ConnectAndGetCertificateAsync(parameters, evidence, cts.Token);

            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Populate evidence from certificate
            PopulateEvidenceFromCertificate(cert, parameters, evidence, stopwatch.ElapsedMilliseconds);

            // Evaluate certificate
            var failReason = EvaluateCertificate(cert, parameters, evidence);
            if (failReason != null)
            {
                result = BuildValidationFailResult(result, parameters, evidence, failReason, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                result = BuildSuccessResult(result, parameters, evidence, stopwatch.ElapsedMilliseconds);
            }

            cert.Dispose();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildCancelledResult(result, stopwatch.Elapsed);
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildTimeoutResult(result, parameters, evidence, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildTimeoutResult(result, parameters, evidence, stopwatch.ElapsedMilliseconds);
        }
        catch (ArgumentException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildConfigurationErrorResult(result, ex, evidence, stopwatch.ElapsedMilliseconds);
        }
        catch (AuthenticationException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildNetworkErrorResult(result, ex, evidence, stopwatch.ElapsedMilliseconds);
        }
        catch (SocketException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildNetworkErrorResult(result, ex, evidence, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildNetworkErrorResult(result, ex, evidence, stopwatch.ElapsedMilliseconds);
        }

        return result;
    }

    #region T003: Parameter Extraction

    /// <summary>
    /// Extracts and validates parameters from the test definition.
    /// </summary>
    private static CertificateExpiryParameters ExtractParameters(TestDefinition testDefinition)
    {
        var p = testDefinition.Parameters;

        // Host (required)
        var host = p["host"]?.ToString();
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host parameter is required and cannot be empty", "host");

        // Port (optional, default 443, range 1-65535)
        int port = 443;
        var portValue = p["port"];
        if (portValue != null)
        {
            if (!int.TryParse(portValue.ToString(), out int parsedPort) || parsedPort < 1 || parsedPort > 65535)
                throw new ArgumentException("Port must be a valid integer between 1 and 65535", "port");
            port = parsedPort;
        }

        // WarningDaysBeforeExpiry (optional, default 30, >= 0)
        int warningDays = 30;
        var warningValue = p["warningDaysBeforeExpiry"];
        if (warningValue != null)
        {
            if (!int.TryParse(warningValue.ToString(), out int parsedDays) || parsedDays < 0)
                throw new ArgumentException("warningDaysBeforeExpiry must be a non-negative integer", "warningDaysBeforeExpiry");
            warningDays = parsedDays;
        }

        // Timeout (optional, default 10000ms, > 0)
        int timeout = 10000;
        var timeoutValue = p["timeout"];
        if (timeoutValue != null)
        {
            if (!int.TryParse(timeoutValue.ToString(), out int parsedTimeout) || parsedTimeout <= 0)
                throw new ArgumentException("Timeout must be a positive integer", "timeout");
            timeout = parsedTimeout;
        }

        // SkipChainValidation (optional, default false)
        bool skipChain = false;
        var skipValue = p["skipChainValidation"];
        if (skipValue != null && bool.TryParse(skipValue.ToString(), out bool parsedSkip))
            skipChain = parsedSkip;

        // Optional identity fields
        var expectedSubject = p["expectedSubject"]?.ToString();
        if (expectedSubject != null && string.IsNullOrWhiteSpace(expectedSubject))
            expectedSubject = null;

        var expectedIssuer = p["expectedIssuer"]?.ToString();
        if (expectedIssuer != null && string.IsNullOrWhiteSpace(expectedIssuer))
            expectedIssuer = null;

        var expectedThumbprint = p["expectedThumbprint"]?.ToString();
        if (expectedThumbprint != null && string.IsNullOrWhiteSpace(expectedThumbprint))
            expectedThumbprint = null;

        return new CertificateExpiryParameters
        {
            Host = host,
            Port = port,
            WarningDaysBeforeExpiry = warningDays,
            Timeout = timeout,
            SkipChainValidation = skipChain,
            ExpectedSubject = expectedSubject,
            ExpectedIssuer = expectedIssuer,
            ExpectedThumbprint = expectedThumbprint
        };
    }

    #endregion

    #region T005: TLS Connection (US1)

    /// <summary>
    /// Connects to the remote host via TcpClient + SslStream and retrieves the server's leaf certificate.
    /// Uses host as SNI hostname automatically. Respects skipChainValidation.
    /// </summary>
    private static async Task<X509Certificate2> ConnectAndGetCertificateAsync(
        CertificateExpiryParameters parameters,
        CertificateExpiryTestEvidence evidence,
        CancellationToken cancellationToken)
    {
        X509Certificate2? capturedCert = null;
        SslProtocols? negotiatedProtocol = null;

        var tcpClient = new TcpClient();
        try
        {
            await tcpClient.ConnectAsync(parameters.Host, parameters.Port, cancellationToken);

            var networkStream = tcpClient.GetStream();
            var sslStream = new SslStream(
                networkStream,
                leaveInnerStreamOpen: false,
                userCertificateValidationCallback: (sender, cert, chain, sslPolicyErrors) =>
                {
                    if (cert != null)
                        capturedCert = new X509Certificate2(cert);
                    if (parameters.SkipChainValidation)
                        return true;
                    if (sslPolicyErrors == SslPolicyErrors.None)
                        return true;
                    // Allow time-validity errors through so EvaluateCertificate can produce
                    // the correct Validation failure instead of a Network error (FR-004)
                    if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && chain != null)
                    {
                        const X509ChainStatusFlags timeFlags =
                            X509ChainStatusFlags.NotTimeValid | X509ChainStatusFlags.NotTimeNested;
                        if (chain.ChainStatus.Length > 0 &&
                            chain.ChainStatus.All(s => (s.Status & ~timeFlags) == X509ChainStatusFlags.NoError))
                            return true;
                    }
                    return false;
                });

            try
            {
                var sslOptions = new SslClientAuthenticationOptions
                {
                    TargetHost = parameters.Host,
                    EnabledSslProtocols = SslProtocols.None // Let OS choose best available
                };

                await sslStream.AuthenticateAsClientAsync(sslOptions, cancellationToken);
                negotiatedProtocol = sslStream.SslProtocol;
                evidence.TlsProtocolVersion = sslStream.SslProtocol.ToString();
            }
            finally
            {
                await sslStream.DisposeAsync();
            }
        }
        finally
        {
            tcpClient.Dispose();
        }

        if (capturedCert == null)
            throw new AuthenticationException($"TLS handshake completed but no server certificate was captured from {parameters.Host}:{parameters.Port}");

        return capturedCert;
    }

    #endregion

    #region T006 + T008 + T009: Certificate Evaluation (US1 + US2)

    /// <summary>
    /// Populates evidence fields from the retrieved certificate.
    /// </summary>
    private static void PopulateEvidenceFromCertificate(
        X509Certificate2 cert,
        CertificateExpiryParameters parameters,
        CertificateExpiryTestEvidence evidence,
        long elapsedMs)
    {
        var now = DateTime.UtcNow;
        var notAfterUtc = cert.NotAfter.ToUniversalTime();
        var notBeforeUtc = cert.NotBefore.ToUniversalTime();
        var daysUntilExpiry = (int)(notAfterUtc - now).TotalDays;

        evidence.Subject = cert.Subject;
        evidence.Issuer = cert.Issuer;
        evidence.Thumbprint = cert.Thumbprint;
        evidence.NotBefore = notBeforeUtc;
        evidence.NotAfter = notAfterUtc;
        evidence.DaysUntilExpiry = daysUntilExpiry;
        evidence.IsExpired = notAfterUtc < now;
        evidence.IsNotYetValid = notBeforeUtc > now;
        evidence.ExpiresWithinWarningWindow = !evidence.IsExpired && daysUntilExpiry < parameters.WarningDaysBeforeExpiry;
        evidence.SubjectAlternativeNames = ExtractSanEntries(cert);
        evidence.ResponseTimeMs = (int)elapsedMs;
    }

    /// <summary>
    /// T006: Evaluates the certificate against validity window and identity assertions.
    /// Returns null on pass, or a failure reason string on fail.
    /// </summary>
    private static string? EvaluateCertificate(
        X509Certificate2 cert,
        CertificateExpiryParameters parameters,
        CertificateExpiryTestEvidence evidence)
    {
        var now = DateTime.UtcNow;
        var notAfterUtc = cert.NotAfter.ToUniversalTime();
        var notBeforeUtc = cert.NotBefore.ToUniversalTime();
        var daysUntilExpiry = evidence.DaysUntilExpiry ?? (int)(notAfterUtc - now).TotalDays;

        // Validity window checks (US1)
        if (notAfterUtc < now)
        {
            var daysAgo = Math.Abs(daysUntilExpiry);
            return $"Certificate expired {daysAgo} day{(daysAgo == 1 ? "" : "s")} ago ({notAfterUtc:yyyy-MM-dd})";
        }

        if (notBeforeUtc > now)
        {
            return $"Certificate is not yet valid (valid from {notBeforeUtc:yyyy-MM-dd})";
        }

        if (daysUntilExpiry < parameters.WarningDaysBeforeExpiry)
        {
            return $"Certificate expires in {daysUntilExpiry} day{(daysUntilExpiry == 1 ? "" : "s")} ({notAfterUtc:yyyy-MM-dd}), within the {parameters.WarningDaysBeforeExpiry}-day warning window";
        }

        // Identity assertions (US2)
        if (parameters.ExpectedSubject != null)
        {
            var subjectMatch = cert.Subject.Contains(parameters.ExpectedSubject, StringComparison.OrdinalIgnoreCase);
            var sanMatch = (evidence.SubjectAlternativeNames ?? Array.Empty<string>())
                .Any(san => san.Equals(parameters.ExpectedSubject, StringComparison.OrdinalIgnoreCase));

            if (!subjectMatch && !sanMatch)
            {
                var sans = evidence.SubjectAlternativeNames?.Length > 0
                    ? string.Join(", ", evidence.SubjectAlternativeNames)
                    : "(none)";
                return $"Subject/SAN mismatch: expected '{parameters.ExpectedSubject}' — Subject: '{cert.Subject}', SANs: [{sans}]";
            }
        }

        if (parameters.ExpectedIssuer != null)
        {
            if (!cert.Issuer.Contains(parameters.ExpectedIssuer, StringComparison.OrdinalIgnoreCase))
                return $"Issuer mismatch: expected '{parameters.ExpectedIssuer}' — actual: '{cert.Issuer}'";
        }

        if (parameters.ExpectedThumbprint != null)
        {
            if (!cert.Thumbprint.Equals(parameters.ExpectedThumbprint, StringComparison.OrdinalIgnoreCase))
                return $"Thumbprint mismatch: expected '{parameters.ExpectedThumbprint.ToUpperInvariant()}' — actual: '{cert.Thumbprint}'";
        }

        return null; // All checks passed
    }

    /// <summary>
    /// T008: Extracts DNS Subject Alternative Names from the certificate's SAN extension.
    /// </summary>
    private static string[] ExtractSanEntries(X509Certificate2 cert)
    {
        var sanExtension = cert.Extensions
            .OfType<X509SubjectAlternativeNameExtension>()
            .FirstOrDefault();

        if (sanExtension == null)
            return Array.Empty<string>();

        return sanExtension.EnumerateDnsNames().ToArray();
    }

    #endregion

    #region T004: Result Building

    /// <summary>Builds a successful test result.</summary>
    private static TestResult BuildSuccessResult(
        TestResult result,
        CertificateExpiryParameters parameters,
        CertificateExpiryTestEvidence evidence,
        long elapsedMs)
    {
        var days = evidence.DaysUntilExpiry ?? 0;
        var expiry = evidence.NotAfter?.ToString("yyyy-MM-dd") ?? "unknown";

        result.Status = TestStatus.Pass;
        result.HumanSummary = $"Certificate for {parameters.Host} is valid — expires in {days} day{(days == 1 ? "" : "s")} ({expiry})";
        result.TechnicalDetails = BuildTechnicalDetails(parameters, evidence, elapsedMs);
        result.Evidence = BuildEvidence(evidence, elapsedMs);
        return result;
    }

    /// <summary>Builds a validation failure result with a specific reason.</summary>
    private static TestResult BuildValidationFailResult(
        TestResult result,
        CertificateExpiryParameters parameters,
        CertificateExpiryTestEvidence evidence,
        string reason,
        long elapsedMs)
    {
        result.Status = TestStatus.Fail;
        result.HumanSummary = $"Certificate check failed for {parameters.Host}:{parameters.Port} — {reason}";
        result.TechnicalDetails = BuildTechnicalDetails(parameters, evidence, elapsedMs);
        result.Evidence = BuildEvidence(evidence, elapsedMs);
        result.Error = new TestError
        {
            Category = ErrorCategory.Validation,
            Message = reason
        };
        return result;
    }

    /// <summary>Builds a cancelled (Skipped) result.</summary>
    private static TestResult BuildCancelledResult(TestResult result, TimeSpan elapsed)
    {
        result.Status = TestStatus.Skipped;
        result.HumanSummary = "Test was cancelled";
        result.TechnicalDetails = $"Test cancelled after {elapsed.TotalMilliseconds:F0} ms";
        result.Error = new TestError
        {
            Category = ErrorCategory.Unknown,
            Message = "Test was cancelled"
        };
        return result;
    }

    /// <summary>Builds a timeout failure result.</summary>
    private static TestResult BuildTimeoutResult(
        TestResult result,
        CertificateExpiryParameters? parameters,
        CertificateExpiryTestEvidence? evidence,
        long elapsedMs)
    {
        var endpoint = parameters != null ? $"{parameters.Host}:{parameters.Port}" : "the endpoint";
        result.Status = TestStatus.Fail;
        result.HumanSummary = $"TLS connection to {endpoint} timed out after {elapsedMs} ms";
        result.TechnicalDetails = evidence != null
            ? BuildTechnicalDetails(parameters!, evidence, elapsedMs)
            : $"Connection timed out after {elapsedMs} ms";
        result.Error = new TestError
        {
            Category = ErrorCategory.Timeout,
            Message = $"TLS connection timed out after {elapsedMs} ms"
        };
        if (evidence != null)
            result.Evidence = BuildEvidence(evidence, elapsedMs);
        return result;
    }

    /// <summary>Builds a network error result (TLS failure, DNS, connection refused).</summary>
    private static TestResult BuildNetworkErrorResult(
        TestResult result,
        Exception ex,
        CertificateExpiryTestEvidence? evidence,
        long elapsedMs)
    {
        var host = evidence?.Host ?? "the endpoint";
        var port = evidence?.Port;
        var endpoint = port.HasValue ? $"{host}:{port}" : host;

        result.Status = TestStatus.Fail;
        result.HumanSummary = ex is AuthenticationException
            ? $"TLS handshake failed for {endpoint} — {ex.Message}"
            : $"Network error connecting to {endpoint} — {ex.Message}";
        result.TechnicalDetails = evidence != null && evidence.Host.Length > 0
            ? $"Network error for {endpoint}\nError: {ex.GetType().Name} — {ex.Message}" +
              (ex.InnerException != null ? $"\nInner: {ex.InnerException.Message}" : "")
            : $"Network error: {ex.GetType().Name} — {ex.Message}";
        result.Error = new TestError
        {
            Category = ErrorCategory.Network,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            ExceptionType = ex.GetType().Name
        };
        if (evidence != null)
            result.Evidence = BuildEvidence(evidence, elapsedMs);
        return result;
    }

    /// <summary>Builds a configuration error result (invalid parameters).</summary>
    private static TestResult BuildConfigurationErrorResult(
        TestResult result,
        Exception ex,
        CertificateExpiryTestEvidence? evidence,
        long elapsedMs)
    {
        result.Status = TestStatus.Fail;
        result.HumanSummary = $"Configuration error: {ex.Message}";
        result.TechnicalDetails = ex.StackTrace ?? ex.Message;
        result.Error = new TestError
        {
            Category = ErrorCategory.Configuration,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            ExceptionType = ex.GetType().Name
        };
        if (evidence != null)
            result.Evidence = BuildEvidence(evidence, elapsedMs);
        return result;
    }

    /// <summary>Builds a multi-line technical details string from all evidence fields.</summary>
    private static string BuildTechnicalDetails(
        CertificateExpiryParameters parameters,
        CertificateExpiryTestEvidence evidence,
        long elapsedMs)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Host: {parameters.Host}:{parameters.Port}");
        if (evidence.TlsProtocolVersion != null)
            sb.AppendLine($"TLS: {evidence.TlsProtocolVersion}");
        if (evidence.ResponseTimeMs.HasValue)
            sb.AppendLine($"Handshake time: {evidence.ResponseTimeMs} ms");
        if (evidence.Subject != null)
            sb.AppendLine($"Subject: {evidence.Subject}");
        if (evidence.Issuer != null)
            sb.AppendLine($"Issuer: {evidence.Issuer}");
        if (evidence.Thumbprint != null)
            sb.AppendLine($"Thumbprint: {evidence.Thumbprint}");
        if (evidence.NotBefore.HasValue && evidence.NotAfter.HasValue)
            sb.AppendLine($"Validity: {evidence.NotBefore:yyyy-MM-dd} to {evidence.NotAfter:yyyy-MM-dd}");
        if (evidence.DaysUntilExpiry.HasValue)
            sb.AppendLine($"Days until expiry: {evidence.DaysUntilExpiry}");
        if (evidence.SubjectAlternativeNames?.Length > 0)
            sb.AppendLine($"SANs: {string.Join(", ", evidence.SubjectAlternativeNames)}");
        if (evidence.ChainValidationSkipped)
            sb.AppendLine("Chain validation: skipped");
        if (!string.IsNullOrEmpty(evidence.ErrorDetail))
            sb.AppendLine($"Detail: {evidence.ErrorDetail}");
        return sb.ToString().TrimEnd();
    }

    /// <summary>Builds a TestEvidence with serialized evidence JSON and timing.</summary>
    private static TestEvidence BuildEvidence(CertificateExpiryTestEvidence evidence, long elapsedMs)
    {
        return new TestEvidence
        {
            ResponseData = JsonSerializer.Serialize(evidence),
            Timing = new TimingBreakdown
            {
                TotalMs = (int)elapsedMs,
                ConnectMs = evidence.ResponseTimeMs ?? (int)elapsedMs
            }
        };
    }

    #endregion
}
