# Quickstart: Implementing Mutual TLS Client Certificate Authentication Test

**Feature**: `001-mtls-test`
**Date**: 2026-02-13
**Estimated Time**: 3-4 hours

## Overview

This guide walks through implementing the `MtlsConnect` test type from start to finish. Follow steps in order for a working implementation.

---

## Prerequisites

- Read [spec.md](spec.md) — Understand user scenarios and requirements
- Read [research.md](research.md) — Understand technical decisions (`HttpClientHandler`, PFX loading, etc.)
- Read [data-model.md](data-model.md) — Understand parameter and evidence schemas
- Read [contracts/README.md](contracts/README.md) — Understand interface contracts

---

## Step 1: Create Test Implementation Class (2 hours)

### 1.1 Create File

**Path**: `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs`

**Template**:

```csharp
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
/// Evidence captured during mTLS connection test execution.
/// </summary>
public class MtlsConnectTestEvidence
{
    public bool Connected { get; set; }
    public int? HttpStatusCode { get; set; }
    public int? ResponseTimeMs { get; set; }
    public string? CertificateSubject { get; set; }
    public string? CertificateIssuer { get; set; }
    public string? CertificateThumbprint { get; set; }
    public DateTime? CertificateNotBefore { get; set; }
    public DateTime? CertificateNotAfter { get; set; }
    public bool CertificateHasPrivateKey { get; set; }
    public bool ServerCertValidationSkipped { get; set; }
    public string? ServerCertificateSubject { get; set; }
    public string? ErrorDetail { get; set; }
}

/// <summary>
/// Tests mutual TLS (client certificate) authentication against an HTTPS endpoint.
/// Loads a PFX/PKCS#12 client certificate, performs TLS handshake, and verifies HTTP response.
/// </summary>
[TestType("MtlsConnect")]
public class MtlsConnectTest : ITest
{
    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(
        TestDefinition testDefinition,
        TestExecutionContext? context,
        CancellationToken cancellationToken = default)
    {
        // ... implementation in subsequent steps
    }
}
```

---

### 1.2 Implement ExecuteAsync Main Flow

```csharp
public async Task<TestResult> ExecuteAsync(
    TestDefinition testDefinition,
    TestExecutionContext? context,
    CancellationToken cancellationToken = default)
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
    MtlsConnectTestEvidence evidence = new();

    try
    {
        // 1. Extract parameters
        var parameters = ExtractParameters(testDefinition);

        // 2. Load and validate client certificate
        using var clientCert = LoadClientCertificate(
            parameters.ClientCertPath,
            context?.Password,
            evidence);

        // 3. Create HttpClient with client certificate
        using var handler = CreateHandler(clientCert, parameters.SkipServerCertValidation, evidence);
        using var httpClient = new HttpClient(handler);

        // 4. Execute HTTPS request with timeout
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(parameters.Timeout);

        using var request = new HttpRequestMessage(HttpMethod.Get, parameters.Url);
        var responseStart = stopwatch.ElapsedMilliseconds;
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        var responseTime = stopwatch.ElapsedMilliseconds - responseStart;

        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;

        // 5. Build success/validation result
        var statusCode = (int)response.StatusCode;
        evidence.Connected = true;
        evidence.HttpStatusCode = statusCode;
        evidence.ResponseTimeMs = (int)responseTime;

        if (statusCode == parameters.ExpectedStatus)
        {
            result.Status = TestStatus.Pass;
            result.HumanSummary = $"mTLS connection to {parameters.Url} succeeded — HTTP {statusCode} in {responseTime} ms (cert: {evidence.CertificateSubject})";
        }
        else
        {
            evidence.ErrorDetail = $"Expected status {parameters.ExpectedStatus}, received {statusCode} {response.StatusCode}";
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Validation,
                Message = $"Unexpected HTTP status: expected {parameters.ExpectedStatus}, got {statusCode}"
            };
            result.HumanSummary = $"mTLS handshake succeeded but server returned HTTP {statusCode} (expected {parameters.ExpectedStatus})";
        }

        result.TechnicalDetails = BuildTechnicalDetails(parameters.Url, evidence);
        result.Evidence = BuildEvidence(evidence, stopwatch.ElapsedMilliseconds, responseTime);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;
        result.Status = TestStatus.Skipped;
        result.Error = new TestError { Category = ErrorCategory.Unknown, Message = "Test was cancelled" };
        result.HumanSummary = "Test was cancelled by user";
    }
    catch (TaskCanceledException)
    {
        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;
        result.Error = new TestError { Category = ErrorCategory.Timeout, Message = "Connection timed out" };
        result.HumanSummary = $"Connection timed out";
        result.TechnicalDetails = BuildTechnicalDetails(null, evidence);
        result.Evidence = BuildEvidence(evidence, stopwatch.ElapsedMilliseconds, null);
    }
    catch (CryptographicException ex)
    {
        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;
        evidence.ErrorDetail = ex.Message;
        result.Error = new TestError
        {
            Category = ErrorCategory.Configuration,
            Message = $"Cannot load PFX file: {ex.Message}",
            ExceptionType = ex.GetType().Name,
            StackTrace = ex.StackTrace
        };
        result.HumanSummary = "Cannot load PFX file: incorrect password or corrupted file";
        result.TechnicalDetails = BuildTechnicalDetails(null, evidence);
        result.Evidence = BuildEvidence(evidence, stopwatch.ElapsedMilliseconds, null);
    }
    catch (FileNotFoundException ex)
    {
        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;
        evidence.ErrorDetail = ex.Message;
        result.Error = new TestError
        {
            Category = ErrorCategory.Configuration,
            Message = $"PFX file not found: {ex.FileName}",
            ExceptionType = ex.GetType().Name
        };
        result.HumanSummary = $"PFX file not found: {ex.FileName}";
        result.Evidence = BuildEvidence(evidence, stopwatch.ElapsedMilliseconds, null);
    }
    catch (ArgumentException ex)
    {
        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;
        result.Error = new TestError
        {
            Category = ErrorCategory.Configuration,
            Message = ex.Message,
            ExceptionType = ex.GetType().Name
        };
        result.HumanSummary = $"Configuration error: {ex.Message}";
        result.Evidence = BuildEvidence(evidence, stopwatch.ElapsedMilliseconds, null);
    }
    catch (HttpRequestException ex)
    {
        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;
        evidence.ErrorDetail = ex.InnerException?.Message ?? ex.Message;
        result.Error = new TestError
        {
            Category = ErrorCategory.Network,
            Message = $"TLS handshake failed: {ex.Message}",
            ExceptionType = ex.GetType().Name,
            StackTrace = ex.StackTrace,
            InnerError = ex.InnerException != null ? new TestError
            {
                Message = ex.InnerException.Message,
                ExceptionType = ex.InnerException.GetType().Name
            } : null
        };
        result.HumanSummary = $"mTLS handshake failed — server rejected client certificate";
        result.TechnicalDetails = BuildTechnicalDetails(null, evidence);
        result.Evidence = BuildEvidence(evidence, stopwatch.ElapsedMilliseconds, null);
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;
        evidence.ErrorDetail = ex.Message;
        result.Error = new TestError
        {
            Category = ErrorCategory.Network,
            Message = ex.Message,
            ExceptionType = ex.GetType().Name,
            StackTrace = ex.StackTrace
        };
        result.HumanSummary = $"Connection error: {ex.Message}";
        result.Evidence = BuildEvidence(evidence, stopwatch.ElapsedMilliseconds, null);
    }

    return result;
}
```

---

### 1.3 Implement Parameter Extraction

```csharp
private record MtlsParameters(
    string Url,
    string ClientCertPath,
    int ExpectedStatus,
    int Timeout,
    bool SkipServerCertValidation);

private static MtlsParameters ExtractParameters(TestDefinition testDefinition)
{
    var parameters = testDefinition.Parameters;

    var url = parameters["url"]?.ToString();
    if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL parameter is required", "url");

    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        throw new ArgumentException("URL must be a valid HTTPS URL", "url");

    var clientCertPath = parameters["clientCertPath"]?.ToString();
    if (string.IsNullOrWhiteSpace(clientCertPath))
        throw new ArgumentException("Client certificate path is required", "clientCertPath");

    var expectedStatus = parameters["expectedStatus"]?.GetValue<int>() ?? 200;
    var timeout = parameters["timeout"]?.GetValue<int>() ?? 30000;
    var skipServerCertValidation = parameters["skipServerCertValidation"]?.GetValue<bool>() ?? false;

    return new MtlsParameters(url, clientCertPath, expectedStatus, timeout, skipServerCertValidation);
}
```

---

### 1.4 Implement Certificate Loading

```csharp
private static X509Certificate2 LoadClientCertificate(
    string certPath,
    string? password,
    MtlsConnectTestEvidence evidence)
{
    if (!File.Exists(certPath))
        throw new FileNotFoundException("PFX file not found", certPath);

    var cert = new X509Certificate2(certPath, password);

    // Populate evidence with certificate metadata
    evidence.CertificateSubject = cert.Subject;
    evidence.CertificateIssuer = cert.Issuer;
    evidence.CertificateThumbprint = cert.Thumbprint;
    evidence.CertificateNotBefore = cert.NotBefore;
    evidence.CertificateNotAfter = cert.NotAfter;
    evidence.CertificateHasPrivateKey = cert.HasPrivateKey;

    if (!cert.HasPrivateKey)
    {
        cert.Dispose();
        throw new ArgumentException("PFX file does not contain a private key");
    }

    return cert;
}
```

---

### 1.5 Implement Handler Creation

```csharp
private static HttpClientHandler CreateHandler(
    X509Certificate2 clientCert,
    bool skipServerCertValidation,
    MtlsConnectTestEvidence evidence)
{
    var handler = new HttpClientHandler
    {
        ClientCertificateOptions = ClientCertificateOption.Manual
    };

    handler.ClientCertificates.Add(clientCert);

    evidence.ServerCertValidationSkipped = skipServerCertValidation;

    if (skipServerCertValidation)
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    else
    {
        // Capture server certificate subject for evidence
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert != null)
                evidence.ServerCertificateSubject = cert.Subject;
            return errors == SslPolicyErrors.None;
        };
    }

    return handler;
}
```

---

### 1.6 Implement Helper Methods

```csharp
private static string BuildTechnicalDetails(string? url, MtlsConnectTestEvidence evidence)
{
    var lines = new List<string>();

    if (url != null)
        lines.Add($"Connected to {url}");

    if (evidence.Connected && evidence.HttpStatusCode.HasValue)
        lines.Add($"HTTP GET → {evidence.HttpStatusCode} ({evidence.ResponseTimeMs} ms)");

    if (evidence.CertificateSubject != null)
    {
        lines.Add($"Client cert: {evidence.CertificateSubject}");
        lines.Add($"Issuer: {evidence.CertificateIssuer}");
        lines.Add($"Thumbprint: {evidence.CertificateThumbprint}");
        lines.Add($"Valid: {evidence.CertificateNotBefore:yyyy-MM-dd} to {evidence.CertificateNotAfter:yyyy-MM-dd}");
        lines.Add($"Has private key: {evidence.CertificateHasPrivateKey}");
    }

    if (evidence.ServerCertValidationSkipped)
        lines.Add("Server certificate validation: SKIPPED");

    if (evidence.ServerCertificateSubject != null)
        lines.Add($"Server cert: {evidence.ServerCertificateSubject}");

    if (evidence.ErrorDetail != null)
        lines.Add($"Error: {evidence.ErrorDetail}");

    return string.Join("\n", lines);
}

private static TestEvidence BuildEvidence(
    MtlsConnectTestEvidence evidence,
    long totalMs,
    long? responseTimeMs)
{
    return new TestEvidence
    {
        ResponseData = JsonSerializer.Serialize(evidence),
        ResponseCode = evidence.HttpStatusCode,
        Timing = new TimingBreakdown
        {
            TotalMs = (int)totalMs,
            ConnectMs = responseTimeMs.HasValue ? (int)responseTimeMs.Value : null
        }
    };
}
```

---

## Step 2: Update UI Converters (15 minutes)

### 2.1 Update Icon Converter

**File**: `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs`

Add case to the switch statement (after `UdpPortOpen`):

```csharp
"MtlsConnect" => SymbolRegular.ShieldKeyhole24,
```

### 2.2 Update Color Converter

**File**: `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs`

Add `"MtlsConnect"` to the network test category:

```csharp
"Ping" or "HttpGet" or "DnsLookup" or "DnsResolve" or "TcpPortOpen" or "UdpPortOpen" or "MtlsConnect" =>
```

---

## Step 3: Update Default Profile (15 minutes)

**File**: `src/ReqChecker.App/Profiles/default-profile.json`

Add mTLS test to the `tests` array:

```json
{
  "id": "test-mtls-001",
  "type": "MtlsConnect",
  "displayName": "Mutual TLS Authentication",
  "description": "Verify client certificate authentication to an HTTPS endpoint",
  "parameters": {
    "url": "https://your-mtls-endpoint.example.com/health",
    "clientCertPath": "C:\\path\\to\\client-certificate.pfx",
    "expectedStatus": 200,
    "timeout": 30000,
    "skipServerCertValidation": false,
    "credentialRef": "mtls-cert-password"
  },
  "fieldPolicy": {
    "url": "Editable",
    "clientCertPath": "Editable",
    "expectedStatus": "Editable",
    "timeout": "Editable",
    "skipServerCertValidation": "Editable",
    "credentialRef": "Hidden"
  }
}
```

---

## Step 4: Write Unit Tests (1 hour)

**File**: `tests/ReqChecker.Infrastructure.Tests/Tests/MtlsConnectTestTests.cs`

```csharp
using System.Text.Json.Nodes;
using FluentAssertions;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Tests;
using Xunit;

namespace ReqChecker.Infrastructure.Tests.Tests;

public class MtlsConnectTestTests
{
    [Fact]
    public async Task ExecuteAsync_MissingUrl_ReturnsConfigurationError()
    {
        var test = new MtlsConnectTest();
        var definition = CreateDefinition(new JsonObject
        {
            ["clientCertPath"] = "C:\\test.pfx"
        });

        var result = await test.ExecuteAsync(definition, null, CancellationToken.None);

        result.Status.Should().Be(TestStatus.Fail);
        result.Error!.Category.Should().Be(ErrorCategory.Configuration);
        result.HumanSummary.Should().Contain("URL");
    }

    [Fact]
    public async Task ExecuteAsync_MissingCertPath_ReturnsConfigurationError()
    {
        var test = new MtlsConnectTest();
        var definition = CreateDefinition(new JsonObject
        {
            ["url"] = "https://example.com"
        });

        var result = await test.ExecuteAsync(definition, null, CancellationToken.None);

        result.Status.Should().Be(TestStatus.Fail);
        result.Error!.Category.Should().Be(ErrorCategory.Configuration);
    }

    [Fact]
    public async Task ExecuteAsync_NonHttpsUrl_ReturnsConfigurationError()
    {
        var test = new MtlsConnectTest();
        var definition = CreateDefinition(new JsonObject
        {
            ["url"] = "http://example.com",
            ["clientCertPath"] = "C:\\test.pfx"
        });

        var result = await test.ExecuteAsync(definition, null, CancellationToken.None);

        result.Status.Should().Be(TestStatus.Fail);
        result.Error!.Category.Should().Be(ErrorCategory.Configuration);
        result.HumanSummary.Should().Contain("HTTPS");
    }

    [Fact]
    public async Task ExecuteAsync_PfxFileNotFound_ReturnsConfigurationError()
    {
        var test = new MtlsConnectTest();
        var definition = CreateDefinition(new JsonObject
        {
            ["url"] = "https://example.com",
            ["clientCertPath"] = "C:\\nonexistent\\missing.pfx"
        });

        var result = await test.ExecuteAsync(definition, null, CancellationToken.None);

        result.Status.Should().Be(TestStatus.Fail);
        result.Error!.Category.Should().Be(ErrorCategory.Configuration);
        result.HumanSummary.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_Cancelled_ReturnsSkipped()
    {
        var test = new MtlsConnectTest();
        var definition = CreateDefinition(new JsonObject
        {
            ["url"] = "https://example.com",
            ["clientCertPath"] = "C:\\nonexistent.pfx"
        });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await test.ExecuteAsync(definition, null, cts.Token);

        result.Status.Should().Be(TestStatus.Skipped);
    }

    private static TestDefinition CreateDefinition(JsonObject parameters)
    {
        return new TestDefinition
        {
            Id = "test-mtls-unit",
            Type = "MtlsConnect",
            DisplayName = "Unit Test mTLS",
            Parameters = parameters
        };
    }
}
```

---

## Step 5: Manual Testing (30 minutes)

### 5.1 Build Application

```bash
dotnet build src/ReqChecker.App/ReqChecker.App.csproj
```

### 5.2 Run Application

```bash
dotnet run --project src/ReqChecker.App/ReqChecker.App.csproj
```

### 5.3 Test Scenarios

1. **Load default profile** — should see "Mutual TLS Authentication" test
2. **Edit URL and cert path** to point to a real mTLS endpoint and PFX file
3. **Run test with valid cert** — should pass with HTTP 200 and certificate details in evidence
4. **Run test with wrong password** — should fail with "incorrect password" message
5. **Run test with missing PFX** — should fail with "file not found" message
6. **Run test against non-mTLS endpoint** — should succeed (server just ignores client cert)
7. **View Results** — should show certificate subject, issuer, thumbprint, and validity dates

---

## Step 6: Verification Checklist

- [ ] `MtlsConnectTest.cs` compiles without errors
- [ ] `[TestType("MtlsConnect")]` attribute present
- [ ] Parameter extraction validates required fields (`url`, `clientCertPath`)
- [ ] URL validated as HTTPS
- [ ] PFX loading handles wrong password, missing file, no private key
- [ ] Client certificate added to `HttpClientHandler.ClientCertificates`
- [ ] `skipServerCertValidation` controls server cert callback
- [ ] PFX password read from `context?.Password` (PromptAtRun pattern)
- [ ] Evidence includes all 12 certificate metadata fields
- [ ] Error categories correctly mapped (Configuration, Network, Validation, Timeout)
- [ ] Icon converter maps `MtlsConnect` → `ShieldKeyhole24`
- [ ] Color converter maps `MtlsConnect` → network test color
- [ ] Default profile includes sample mTLS test
- [ ] Unit tests cover configuration errors and cancellation
- [ ] `dotnet build` succeeds with zero errors

---

## Common Issues & Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| "The credentials supplied to the package were not recognized" | Wrong PFX password | Verify password at credential prompt |
| "The remote certificate is invalid" | Server cert untrusted | Set `skipServerCertValidation: true` for self-signed servers |
| "The SSL connection could not be established" | Server doesn't support mTLS | Verify endpoint actually requires client certificates |
| "PFX file does not contain a private key" | Certificate-only PFX (no key) | Re-export PFX with private key included |
| Build error: `ShieldKeyhole24` not found | WPF-UI version mismatch | Check available `SymbolRegular` values; use `ShieldLock24` as fallback |

---

## Next Steps

After implementation is complete:

1. Run full test suite: `dotnet test`
2. Generate tasks file: Run `/speckit.tasks` command
3. Begin implementation following task breakdown

---

**Estimated Total Time**: 3-4 hours
**Complexity**: Medium (combines HttpGet pattern with certificate handling from FTP tests)
