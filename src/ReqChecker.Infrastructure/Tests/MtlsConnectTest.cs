using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Evidence captured during mTLS test execution.
/// </summary>
public class MtlsConnectTestEvidence
{
    /// <summary>
    /// Whether the TLS connection with client certificate was established.
    /// </summary>
    public bool Connected { get; set; }

    /// <summary>
    /// HTTP status code received from the server (null if connection failed).
    /// </summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>
    /// Response time in milliseconds from request start to response headers.
    /// </summary>
    public int? ResponseTimeMs { get; set; }

    /// <summary>
    /// Client certificate subject (e.g., "CN=api-client, O=Corp").
    /// </summary>
    public string? CertificateSubject { get; set; }

    /// <summary>
    /// Client certificate issuer (e.g., "CN=Corp CA, O=Corp").
    /// </summary>
    public string? CertificateIssuer { get; set; }

    /// <summary>
    /// Client certificate SHA-1 thumbprint.
    /// </summary>
    public string? CertificateThumbprint { get; set; }

    /// <summary>
    /// Client certificate validity start date.
    /// </summary>
    public DateTime? CertificateNotBefore { get; set; }

    /// <summary>
    /// Client certificate validity end date.
    /// </summary>
    public DateTime? CertificateNotAfter { get; set; }

    /// <summary>
    /// Whether the client certificate contains a private key.
    /// </summary>
    public bool CertificateHasPrivateKey { get; set; }

    /// <summary>
    /// Whether server certificate validation was skipped.
    /// </summary>
    public bool ServerCertValidationSkipped { get; set; }

    /// <summary>
    /// Server certificate subject (captured during TLS handshake).
    /// </summary>
    public string? ServerCertificateSubject { get; set; }

    /// <summary>
    /// Additional error or diagnostic detail.
    /// </summary>
    public string? ErrorDetail { get; set; }
}

/// <summary>
/// Internal container for extracted test parameters.
/// </summary>
internal class MtlsTestParameters
{
    public string Url { get; set; } = string.Empty;
    public string ClientCertPath { get; set; } = string.Empty;
    public int ExpectedStatus { get; set; } = 200;
    public int Timeout { get; set; } = 30000;
    public bool SkipServerCertValidation { get; set; }
}

/// <summary>
/// Tests mutual TLS (client certificate) authentication against HTTPS endpoints.
/// Loads a PFX/PKCS#12 client certificate, performs TLS handshake with client certificate
/// presentation, and verifies the HTTP response status code.
/// </summary>
[TestType("MtlsConnect")]
public class MtlsConnectTest : ITest
{
    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
    {
        var result = new TestResult
        {
            TestId = testDefinition.Id,
            TestType = testDefinition.Type,
            DisplayName = testDefinition.DisplayName,
            Status = TestStatus.Fail,
            StartTime = DateTimeOffset.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();
        MtlsTestParameters? parameters = null;
        MtlsConnectTestEvidence? evidence = null;
        X509Certificate2? clientCertificate = null;

        try
        {
            // T001: Extract and validate parameters
            parameters = ExtractParameters(testDefinition);
            cancellationToken.ThrowIfCancellationRequested();

            // T003: Load client certificate with password from context
            clientCertificate = LoadClientCertificate(parameters.ClientCertPath, context?.Password, out evidence);
            cancellationToken.ThrowIfCancellationRequested();

            // T004: Create handler with client certificate
            using var handler = CreateHandler(clientCertificate, parameters.SkipServerCertValidation);
            evidence.ServerCertValidationSkipped = parameters.SkipServerCertValidation;

            // T006: Execute HTTP request with timeout
            using var httpClient = new HttpClient(handler);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(parameters.Timeout);

            var responseStart = stopwatch.ElapsedMilliseconds;
            using var response = await httpClient.GetAsync(parameters.Url, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            var responseTime = stopwatch.ElapsedMilliseconds - responseStart;

            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;

            // Update evidence with response info
            evidence.Connected = true;
            evidence.HttpStatusCode = (int)response.StatusCode;
            evidence.ResponseTimeMs = (int)responseTime;

            // T008: Check if status matches expected
            var actualStatus = (int)response.StatusCode;
            if (actualStatus != parameters.ExpectedStatus)
            {
                evidence.ErrorDetail = $"Expected status {parameters.ExpectedStatus}, received {actualStatus} ({response.StatusCode})";
                result = BuildValidationFailResult(result, parameters, evidence, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                result = BuildSuccessResult(result, parameters, evidence, stopwatch.ElapsedMilliseconds);
            }
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
        catch (ArgumentException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildConfigurationErrorResult(result, ex, evidence, stopwatch.ElapsedMilliseconds);
        }
        catch (FileNotFoundException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildConfigurationErrorResult(result, ex, evidence, stopwatch.ElapsedMilliseconds);
        }
        catch (CryptographicException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTimeOffset.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result = BuildConfigurationErrorResult(result, ex, evidence, stopwatch.ElapsedMilliseconds);
        }
        catch (HttpRequestException ex)
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
        finally
        {
            clientCertificate?.Dispose();
        }

        return result;
    }

    #region T002: Parameter Extraction

    /// <summary>
    /// Extracts and validates parameters from the test definition.
    /// </summary>
    private MtlsTestParameters ExtractParameters(TestDefinition testDefinition)
    {
        var parameters = testDefinition.Parameters;

        // URL (required, HTTPS only)
        var url = parameters["url"]?.ToString();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL parameter is required and cannot be empty", "url");
        }
        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("URL must use HTTPS scheme for mTLS testing", "url");
        }

        // Client certificate path (required)
        var clientCertPath = parameters["clientCertPath"]?.ToString();
        if (string.IsNullOrWhiteSpace(clientCertPath))
        {
            throw new ArgumentException("Client certificate path parameter is required", "clientCertPath");
        }

        // Expected status (optional, default 200)
        int expectedStatus = 200;
        var expectedStatusValue = parameters["expectedStatus"];
        if (expectedStatusValue != null)
        {
            if (int.TryParse(expectedStatusValue.ToString(), out int parsedStatus))
            {
                if (parsedStatus < 100 || parsedStatus > 599)
                {
                    throw new ArgumentException("Expected status must be a valid HTTP status code (100-599)", "expectedStatus");
                }
                expectedStatus = parsedStatus;
            }
        }

        // Timeout (optional, default 30000ms)
        int timeout = 30000;
        var timeoutValue = parameters["timeout"];
        if (timeoutValue != null && int.TryParse(timeoutValue.ToString(), out int parsedTimeout))
        {
            if (parsedTimeout <= 0)
            {
                throw new ArgumentException("Timeout must be a positive integer", "timeout");
            }
            timeout = parsedTimeout;
        }

        // Skip server certificate validation (optional, default false)
        bool skipServerCertValidation = false;
        var skipValue = parameters["skipServerCertValidation"];
        if (skipValue != null && bool.TryParse(skipValue.ToString(), out bool parsedSkip))
        {
            skipServerCertValidation = parsedSkip;
        }

        return new MtlsTestParameters
        {
            Url = url,
            ClientCertPath = clientCertPath,
            ExpectedStatus = expectedStatus,
            Timeout = timeout,
            SkipServerCertValidation = skipServerCertValidation
        };
    }

    #endregion

    #region T003: Certificate Loading

    /// <summary>
    /// Loads a PFX client certificate and populates evidence with certificate metadata.
    /// </summary>
    /// <param name="certPath">Path to the PFX file</param>
    /// <param name="password">Optional password for the PFX file</param>
    /// <param name="evidence">Evidence object to populate with certificate metadata</param>
    /// <returns>The loaded X509Certificate2</returns>
    /// <exception cref="FileNotFoundException">PFX file not found</exception>
    /// <exception cref="CryptographicException">Invalid PFX or wrong password</exception>
    /// <exception cref="ArgumentException">Certificate missing private key or invalid dates</exception>
    private X509Certificate2 LoadClientCertificate(string certPath, string? password, out MtlsConnectTestEvidence evidence)
    {
        evidence = new MtlsConnectTestEvidence
        {
            CertificateHasPrivateKey = false,
            ServerCertValidationSkipped = false
        };

        // Check file exists
        if (!File.Exists(certPath))
        {
            evidence.ErrorDetail = $"PFX file not found: {certPath}";
            throw new FileNotFoundException($"PFX file not found: {certPath}", certPath);
        }

        // Load certificate
        X509Certificate2 cert;
        try
        {
            cert = new X509Certificate2(certPath, password, X509KeyStorageFlags.DefaultKeySet);
        }
        catch (CryptographicException ex)
        {
            evidence.ErrorDetail = $"Cannot load PFX file: incorrect passphrase or corrupted file. Check the pfxPassword parameter in test configuration. ({ex.Message})";
            throw;
        }

        // Populate evidence with certificate metadata
        evidence.CertificateSubject = cert.Subject;
        evidence.CertificateIssuer = cert.Issuer;
        evidence.CertificateThumbprint = cert.Thumbprint;
        evidence.CertificateNotBefore = cert.NotBefore;
        evidence.CertificateNotAfter = cert.NotAfter;
        evidence.CertificateHasPrivateKey = cert.HasPrivateKey;

        // Verify private key exists
        if (!cert.HasPrivateKey)
        {
            evidence.ErrorDetail = "PFX file does not contain a private key";
            cert.Dispose();
            throw new ArgumentException("PFX file does not contain a private key", "clientCertPath");
        }

        // T015: Check certificate validity dates (pre-connection validation)
        if (cert.NotAfter < DateTimeOffset.UtcNow)
        {
            evidence.ErrorDetail = $"Client certificate expired on {cert.NotAfter:yyyy-MM-dd} (subject: {cert.Subject})";
            cert.Dispose();
            throw new ArgumentException($"Client certificate expired on {cert.NotAfter:yyyy-MM-dd} (subject: {cert.Subject})", "clientCertPath");
        }

        if (cert.NotBefore > DateTimeOffset.UtcNow)
        {
            evidence.ErrorDetail = $"Client certificate is not yet valid until {cert.NotBefore:yyyy-MM-dd} (subject: {cert.Subject})";
            cert.Dispose();
            throw new ArgumentException($"Client certificate is not yet valid until {cert.NotBefore:yyyy-MM-dd} (subject: {cert.Subject})", "clientCertPath");
        }

        return cert;
    }

    #endregion

    #region T004: Handler Creation

    /// <summary>
    /// Creates an HttpClientHandler configured with client certificate and optional server cert validation skip.
    /// </summary>
    /// <param name="clientCertificate">The client certificate to use for mTLS</param>
    /// <param name="skipServerCertValidation">Whether to skip server certificate validation</param>
    /// <returns>Configured HttpClientHandler</returns>
    private HttpClientHandler CreateHandler(X509Certificate2 clientCertificate, bool skipServerCertValidation)
    {
        var handler = new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual
        };

        // Add client certificate
        handler.ClientCertificates.Add(clientCertificate);

        // Configure server certificate validation - use DangerousAcceptAnyServerCertificateValidator for skip
        if (skipServerCertValidation)
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        return handler;
    }

    #endregion

    #region T005: Result Building

    /// <summary>
    /// Builds multi-line technical details with cert info, connection details, and error info.
    /// </summary>
    private string BuildTechnicalDetails(MtlsTestParameters parameters, MtlsConnectTestEvidence evidence, long elapsedMs, Exception? error = null)
    {
        var sb = new System.Text.StringBuilder();

        if (evidence.Connected)
        {
            sb.AppendLine($"Connected to {parameters.Url}");
            sb.AppendLine($"HTTP GET → {evidence.HttpStatusCode} ({evidence.HttpStatusCode switch
            {
                200 => "OK",
                201 => "Created",
                204 => "No Content",
                301 => "Moved Permanently",
                302 => "Found",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                _ => "Unknown"
            }}) ({evidence.ResponseTimeMs} ms)");
        }
        else
        {
            sb.AppendLine($"TLS handshake to {parameters.Url} failed");
        }

        // Client certificate info
        if (!string.IsNullOrEmpty(evidence.CertificateSubject))
        {
            sb.AppendLine($"Client cert: {evidence.CertificateSubject}");
        }
        if (!string.IsNullOrEmpty(evidence.CertificateIssuer))
        {
            sb.AppendLine($"Issuer: {evidence.CertificateIssuer}");
        }
        if (!string.IsNullOrEmpty(evidence.CertificateThumbprint))
        {
            sb.AppendLine($"Thumbprint: {evidence.CertificateThumbprint}");
        }
        if (evidence.CertificateNotBefore.HasValue && evidence.CertificateNotAfter.HasValue)
        {
            sb.AppendLine($"Valid: {evidence.CertificateNotBefore:yyyy-MM-dd} to {evidence.CertificateNotAfter:yyyy-MM-dd}");
        }

        // Server certificate info
        if (!string.IsNullOrEmpty(evidence.ServerCertificateSubject))
        {
            sb.AppendLine($"Server cert: {evidence.ServerCertificateSubject}");
        }

        // Error info
        if (error != null)
        {
            sb.AppendLine($"Error: {error.GetType().Name} — {error.Message}");
            if (error.InnerException != null)
            {
                sb.AppendLine($"Inner: {error.InnerException.Message}");
            }
        }

        if (!string.IsNullOrEmpty(evidence.ErrorDetail))
        {
            sb.AppendLine($"Detail: {evidence.ErrorDetail}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Builds TestEvidence with ResponseData (serialized evidence JSON), ResponseCode, and TimingBreakdown.
    /// </summary>
    private TestEvidence BuildEvidence(MtlsConnectTestEvidence evidence, int? responseCode, long elapsedMs, long responseTimeMs)
    {
        return new TestEvidence
        {
            ResponseData = JsonSerializer.Serialize(evidence),
            ResponseCode = responseCode,
            Timing = new TimingBreakdown
            {
                TotalMs = (int)elapsedMs,
                ConnectMs = (int)responseTimeMs
            }
        };
    }

    /// <summary>
    /// Builds a successful test result with evidence.
    /// </summary>
    private TestResult BuildSuccessResult(TestResult result, MtlsTestParameters parameters, MtlsConnectTestEvidence evidence, long elapsedMs)
    {
        result.Status = TestStatus.Pass;
        result.HumanSummary = $"mTLS connection to {parameters.Url} succeeded — HTTP {evidence.HttpStatusCode} in {elapsedMs} ms (cert: {ExtractCommonName(evidence.CertificateSubject)})";
        result.TechnicalDetails = BuildTechnicalDetails(parameters, evidence, elapsedMs);
        result.Evidence = BuildEvidence(evidence, evidence.HttpStatusCode, elapsedMs, evidence.ResponseTimeMs ?? 0);

        return result;
    }

    /// <summary>
    /// Builds a validation failure result when HTTP status doesn't match expected.
    /// </summary>
    private TestResult BuildValidationFailResult(TestResult result, MtlsTestParameters parameters, MtlsConnectTestEvidence evidence, long elapsedMs)
    {
        result.Status = TestStatus.Fail;
        result.HumanSummary = $"mTLS handshake succeeded but server returned HTTP {evidence.HttpStatusCode} (expected {parameters.ExpectedStatus})";
        result.TechnicalDetails = BuildTechnicalDetails(parameters, evidence, elapsedMs);
        result.Evidence = BuildEvidence(evidence, evidence.HttpStatusCode, elapsedMs, evidence.ResponseTimeMs ?? 0);
        result.Error = new TestError
        {
            Category = ErrorCategory.Validation,
            Message = evidence.ErrorDetail ?? $"Expected status {parameters.ExpectedStatus}, received {evidence.HttpStatusCode}"
        };

        return result;
    }

    /// <summary>
    /// Builds a result for user-cancelled tests.
    /// </summary>
    private TestResult BuildCancelledResult(TestResult result, TimeSpan elapsed)
    {
        result.Status = TestStatus.Skipped;
        result.HumanSummary = "Test was cancelled";
        result.TechnicalDetails = $"Test cancelled after {elapsed.TotalMilliseconds} ms";
        result.Error = new TestError
        {
            Category = ErrorCategory.Unknown,
            Message = "Test was cancelled"
        };

        return result;
    }

    /// <summary>
    /// Builds a timeout result.
    /// </summary>
    private TestResult BuildTimeoutResult(TestResult result, MtlsTestParameters? parameters, MtlsConnectTestEvidence? evidence, long elapsedMs)
    {
        result.Status = TestStatus.Fail;
        result.HumanSummary = $"Connection timed out after {elapsedMs} ms";
        result.TechnicalDetails = parameters != null && evidence != null
            ? BuildTechnicalDetails(parameters, evidence, elapsedMs)
            : $"Connection timed out after {elapsedMs} ms";
        result.Error = new TestError
        {
            Category = ErrorCategory.Timeout,
            Message = $"Connection timed out after {elapsedMs} ms"
        };

        if (evidence != null && parameters != null)
        {
            result.Evidence = BuildEvidence(evidence, null, elapsedMs, 0);
        }

        return result;
    }

    /// <summary>
    /// Builds a configuration error result (invalid params, bad PFX, etc.).
    /// </summary>
    private TestResult BuildConfigurationErrorResult(TestResult result, Exception ex, MtlsConnectTestEvidence? evidence, long elapsedMs)
    {
        result.Status = TestStatus.Fail;

        string humanSummary;
        if (ex is FileNotFoundException)
            humanSummary = $"PFX file not found: {ex.Message}";
        else if (ex is CryptographicException)
            humanSummary = "Cannot load PFX file: incorrect password or corrupted file";
        else
            humanSummary = ex.Message;

        result.HumanSummary = humanSummary;
        result.TechnicalDetails = ex.StackTrace ?? ex.Message;
        result.Error = new TestError
        {
            Category = ErrorCategory.Configuration,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            ExceptionType = ex.GetType().Name
        };

        if (evidence != null)
        {
            result.Evidence = BuildEvidence(evidence, null, elapsedMs, 0);
        }

        return result;
    }

    /// <summary>
    /// Builds a network error result (TLS failure, connection refused, etc.).
    /// T013: Enhanced HttpRequestException error handling with chain trust detection.
    /// </summary>
    private TestResult BuildNetworkErrorResult(TestResult result, Exception ex, MtlsConnectTestEvidence? evidence, long elapsedMs)
    {
        result.Status = TestStatus.Fail;

        // T013: Check for chain trust indicators in exception
        string humanSummary = "Connection error";
        string errorDetail = ex.Message;

        if (ex is HttpRequestException httpEx)
        {
            // Check inner exception chain for chain trust indicators
            var innerEx = httpEx.InnerException;
            while (innerEx != null)
            {
                string innerMessage = innerEx.Message ?? "";
                if (innerMessage.Contains("chain", StringComparison.OrdinalIgnoreCase) ||
                    innerMessage.Contains("trust", StringComparison.OrdinalIgnoreCase) ||
                    innerMessage.Contains("RemoteCertificateChainErrors", StringComparison.OrdinalIgnoreCase) ||
                    innerEx is AuthenticationException)
                {
                    humanSummary = "mTLS handshake failed — certificate chain validation error (untrusted CA or incomplete chain)";
                    errorDetail = "Possible causes: certificate not signed by a trusted CA, intermediate certificates missing from PFX, root CA not in trusted store";
                    break;
                }
                innerEx = innerEx.InnerException;
            }

            if (humanSummary == "Connection error")
            {
                humanSummary = "mTLS handshake failed — server rejected client certificate";
            }
        }

        result.HumanSummary = humanSummary;
        result.TechnicalDetails = evidence != null && evidence.CertificateSubject != null
            ? $"TLS handshake failed\nClient cert: {evidence.CertificateSubject} (thumbprint: {evidence.CertificateThumbprint})\nError: {ex.GetType().Name} — {ex.Message}\n{(errorDetail != ex.Message ? $"Detail: {errorDetail}" : "")}"
            : $"Connection error: {ex.Message}";
        result.Error = new TestError
        {
            Category = ErrorCategory.Network,
            Message = errorDetail,
            StackTrace = ex.StackTrace,
            ExceptionType = ex.GetType().Name
        };

        if (evidence != null)
        {
            result.Evidence = BuildEvidence(evidence, null, elapsedMs, 0);
        }

        return result;
    }

    /// <summary>
    /// Extracts the Common Name (CN) from a certificate subject DN.
    /// </summary>
    private static string ExtractCommonName(string? subject)
    {
        if (string.IsNullOrEmpty(subject))
            return "unknown";

        var parts = subject.Split(',', StringSplitOptions.TrimEntries);
        var cnPart = parts.FirstOrDefault(p => p.StartsWith("CN=", StringComparison.OrdinalIgnoreCase));
        return cnPart != null ? cnPart.Substring(3) : subject;
    }

    #endregion
}
