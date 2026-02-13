# Quickstart: Implementing UDP Port Reachability Test

**Feature**: `039-udp-port-test`
**Date**: 2026-02-13
**Estimated Time**: 3-4 hours

## Overview

This guide walks through implementing the `UdpPortOpen` test type from start to finish. Follow steps in order for a working implementation.

---

## Prerequisites

✅ Read [spec.md](spec.md) — Understand user scenarios and requirements
✅ Read [research.md](research.md) — Understand technical decisions (`UdpClient`, timeout strategy, etc.)
✅ Read [data-model.md](data-model.md) — Understand parameter and evidence schemas
✅ Read [contracts/README.md](contracts/README.md) — Understand interface contracts

---

## Step 1: Create Test Implementation Class (1.5 hours)

### 1.1 Create File

**Path**: `src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs`

**Template**:

```csharp
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;

namespace ReqChecker.Infrastructure.Tests;

[TestType("UdpPortOpen")]
public class UdpPortOpenTest : ITest
{
    public async Task<TestResult> ExecuteAsync(
        TestDefinition definition,
        TestExecutionContext? context,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var testId = definition.Id;

        try
        {
            // Extract and validate parameters
            var (host, port, timeout, payload, expectedResponse) = ExtractParameters(definition);

            // Resolve host to IP address
            var ipAddress = await ResolveHostAsync(host, cancellationToken);

            // Execute UDP test
            var evidence = await ExecuteUdpTestAsync(ipAddress, port, timeout, payload, expectedResponse, cancellationToken);

            // Build result
            var endTime = DateTime.UtcNow;
            return BuildResult(testId, startTime, endTime, evidence, expectedResponse);
        }
        catch (OperationCanceledException)
        {
            return BuildCancelledResult(testId, startTime);
        }
        catch (Exception ex)
        {
            return BuildErrorResult(testId, startTime, ex);
        }
    }

    private (string host, int port, int timeout, byte[] payload, byte[]? expectedResponse) ExtractParameters(TestDefinition definition)
    {
        // Implementation in Step 1.2
    }

    private async Task<IPAddress> ResolveHostAsync(string host, CancellationToken cancellationToken)
    {
        // Implementation in Step 1.3
    }

    private async Task<UdpPortTestEvidence> ExecuteUdpTestAsync(
        IPAddress ipAddress,
        int port,
        int timeout,
        byte[] payload,
        byte[]? expectedResponse,
        CancellationToken cancellationToken)
    {
        // Implementation in Step 1.4
    }

    private TestResult BuildResult(string testId, DateTime startTime, DateTime endTime, UdpPortTestEvidence evidence, byte[]? expectedResponse)
    {
        // Implementation in Step 1.5
    }

    private TestResult BuildCancelledResult(string testId, DateTime startTime)
    {
        // Implementation in Step 1.6
    }

    private TestResult BuildErrorResult(string testId, DateTime startTime, Exception ex)
    {
        // Implementation in Step 1.7
    }

    private byte[] DecodePayload(string? payloadStr, string encoding)
    {
        // Implementation in Step 1.8
    }
}

public class UdpPortTestEvidence
{
    public bool Responded { get; set; }
    public int? RoundTripTimeMs { get; set; }
    public string RemoteEndpoint { get; set; }
    public int PayloadSentBytes { get; set; }
    public int? PayloadReceivedBytes { get; set; }
    public string? ResponseDataPreview { get; set; }
}
```

---

### 1.2 Implement Parameter Extraction

```csharp
private (string host, int port, int timeout, byte[] payload, byte[]? expectedResponse) ExtractParameters(TestDefinition definition)
{
    var parameters = definition.Parameters;

    // Required parameters
    string host = parameters["host"]?.ToString()
        ?? throw new ArgumentException("Parameter 'host' is required");

    int port = parameters["port"]?.GetValue<int>()
        ?? throw new ArgumentException("Parameter 'port' is required");

    if (port < 1 || port > 65535)
        throw new ArgumentException($"Port {port} is out of valid range (1-65535)");

    // Optional parameters with defaults
    int timeout = parameters["timeout"]?.GetValue<int>() ?? 5000;
    string? payloadStr = parameters["payload"]?.ToString();
    string encoding = parameters["encoding"]?.ToString() ?? "auto";
    string? expectedResponseStr = parameters["expectedResponse"]?.ToString();

    // Decode payload
    byte[] payload = DecodePayload(payloadStr, encoding);
    byte[]? expectedResponse = string.IsNullOrEmpty(expectedResponseStr)
        ? null
        : DecodePayload(expectedResponseStr, encoding);

    return (host, port, timeout, payload, expectedResponse);
}
```

---

### 1.3 Implement DNS Resolution

```csharp
private async Task<IPAddress> ResolveHostAsync(string host, CancellationToken cancellationToken)
{
    // Try parse as IP address first
    if (IPAddress.TryParse(host, out var ipAddress))
    {
        return ipAddress;
    }

    // DNS resolution
    try
    {
        var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);

        if (addresses.Length == 0)
            throw new Exception($"DNS resolution for '{host}' returned no addresses");

        // Prefer IPv4 for broad compatibility
        var ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        return ipv4 ?? addresses[0];
    }
    catch (SocketException ex)
    {
        throw new Exception($"DNS resolution failed for '{host}': {ex.Message}", ex);
    }
}
```

---

### 1.4 Implement UDP Test Execution

```csharp
private async Task<UdpPortTestEvidence> ExecuteUdpTestAsync(
    IPAddress ipAddress,
    int port,
    int timeout,
    byte[] payload,
    byte[]? expectedResponse,
    CancellationToken cancellationToken)
{
    var endpoint = new IPEndPoint(ipAddress, port);
    var sw = Stopwatch.StartNew();

    using var udpClient = new UdpClient(ipAddress.AddressFamily);
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(timeout);

    try
    {
        // Send UDP datagram
        udpClient.Connect(endpoint);
        await udpClient.SendAsync(payload, cts.Token);

        // Wait for response
        var receiveTask = udpClient.ReceiveAsync(cts.Token);
        var result = await receiveTask;

        sw.Stop();

        // Extract response data
        var responseData = result.Buffer;
        var responsePreview = Convert.ToHexString(responseData.Take(64).ToArray());

        // Validate response if pattern provided
        if (expectedResponse != null && !responseData.SequenceEqual(expectedResponse))
        {
            throw new ValidationException(
                $"Response mismatch: expected {expectedResponse.Length} bytes, got {responseData.Length} bytes");
        }

        return new UdpPortTestEvidence
        {
            Responded = true,
            RoundTripTimeMs = (int)sw.ElapsedMilliseconds,
            RemoteEndpoint = endpoint.ToString(),
            PayloadSentBytes = payload.Length,
            PayloadReceivedBytes = responseData.Length,
            ResponseDataPreview = responsePreview
        };
    }
    catch (OperationCanceledException)
    {
        // Timeout occurred
        return new UdpPortTestEvidence
        {
            Responded = false,
            RoundTripTimeMs = null,
            RemoteEndpoint = endpoint.ToString(),
            PayloadSentBytes = payload.Length,
            PayloadReceivedBytes = null,
            ResponseDataPreview = null
        };
    }
    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
    {
        // ICMP Port Unreachable
        throw new Exception($"Port unreachable (ICMP response): {ex.Message}", ex);
    }
}
```

---

### 1.5 Implement Result Building (Success)

```csharp
private TestResult BuildResult(string testId, DateTime startTime, DateTime endTime, UdpPortTestEvidence evidence, byte[]? expectedResponse)
{
    var totalMs = (int)(endTime - startTime).TotalMilliseconds;
    var evidenceJson = JsonSerializer.Serialize(evidence);

    if (evidence.Responded)
    {
        var summary = expectedResponse != null
            ? $"UDP port {GetPortFromEndpoint(evidence.RemoteEndpoint)} on {GetHostFromEndpoint(evidence.RemoteEndpoint)} responded in {evidence.RoundTripTimeMs} ms (validated)"
            : $"UDP port {GetPortFromEndpoint(evidence.RemoteEndpoint)} on {GetHostFromEndpoint(evidence.RemoteEndpoint)} responded in {evidence.RoundTripTimeMs} ms (sent {evidence.PayloadSentBytes} bytes, received {evidence.PayloadReceivedBytes} bytes)";

        return new TestResult
        {
            TestId = testId,
            Status = TestStatus.Pass,
            ErrorCategory = null,
            HumanSummary = summary,
            TechnicalDetails = $"Sent {evidence.PayloadSentBytes}-byte datagram to {evidence.RemoteEndpoint}\nReceived {evidence.PayloadReceivedBytes}-byte response in {evidence.RoundTripTimeMs} ms\nResponse (hex): {evidence.ResponseDataPreview}",
            ResponseData = evidenceJson,
            StartTime = startTime,
            EndTime = endTime,
            TotalMs = totalMs
        };
    }
    else
    {
        return new TestResult
        {
            TestId = testId,
            Status = TestStatus.Fail,
            ErrorCategory = ErrorCategory.Timeout,
            HumanSummary = $"UDP port {GetPortFromEndpoint(evidence.RemoteEndpoint)} on {GetHostFromEndpoint(evidence.RemoteEndpoint)} did not respond within timeout",
            TechnicalDetails = $"Sent {evidence.PayloadSentBytes}-byte datagram to {evidence.RemoteEndpoint}\nNo response received\nPossible causes: service not running, firewall blocking, incorrect port",
            ResponseData = evidenceJson,
            StartTime = startTime,
            EndTime = endTime,
            TotalMs = totalMs
        };
    }
}

private string GetHostFromEndpoint(string endpoint) => endpoint.Split(':')[0].Trim('[', ']');
private int GetPortFromEndpoint(string endpoint) => int.Parse(endpoint.Split(':')[1]);
```

---

### 1.6-1.8 Implement Helper Methods

```csharp
private TestResult BuildCancelledResult(string testId, DateTime startTime)
{
    var endTime = DateTime.UtcNow;
    return new TestResult
    {
        TestId = testId,
        Status = TestStatus.Skipped,
        HumanSummary = "Test cancelled by user",
        TechnicalDetails = "Operation was cancelled before completion",
        StartTime = startTime,
        EndTime = endTime,
        TotalMs = (int)(endTime - startTime).TotalMilliseconds
    };
}

private TestResult BuildErrorResult(string testId, DateTime startTime, Exception ex)
{
    var endTime = DateTime.UtcNow;
    var category = ex.Message.Contains("DNS") ? ErrorCategory.Network
        : ex.Message.Contains("unreachable") ? ErrorCategory.Network
        : ex is ValidationException ? ErrorCategory.Validation
        : ex is ArgumentException ? ErrorCategory.Configuration
        : ErrorCategory.Unknown;

    return new TestResult
    {
        TestId = testId,
        Status = TestStatus.Fail,
        ErrorCategory = category,
        HumanSummary = ex.Message,
        TechnicalDetails = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}",
        StartTime = startTime,
        EndTime = endTime,
        TotalMs = (int)(endTime - startTime).TotalMilliseconds
    };
}

private byte[] DecodePayload(string? payloadStr, string encoding)
{
    if (string.IsNullOrEmpty(payloadStr))
        return new byte[] { 0x00 }; // Default: single null byte

    try
    {
        return encoding.ToLower() switch
        {
            "hex" => Convert.FromHexString(payloadStr),
            "base64" => Convert.FromBase64String(payloadStr),
            "utf8" => Encoding.UTF8.GetBytes(payloadStr),
            "auto" => TryDecodeAuto(payloadStr),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }
    catch (FormatException ex)
    {
        throw new ArgumentException($"Invalid payload format for encoding '{encoding}': {ex.Message}", ex);
    }
}

private byte[] TryDecodeAuto(string str)
{
    // Try hex first (if all chars are hex and even length)
    if (str.Length % 2 == 0 && str.All(c => "0123456789ABCDEFabcdef".Contains(c)))
        return Convert.FromHexString(str);

    // Try base64
    try { return Convert.FromBase64String(str); }
    catch { }

    // Fallback to UTF-8
    return Encoding.UTF8.GetBytes(str);
}
```

Add `ValidationException` class:

```csharp
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

---

## Step 2: Update UI Converters (30 minutes)

### 2.1 Update Icon Converter

**File**: `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs`

Add case to the switch statement:

```csharp
return testType switch
{
    "Ping" => Symbol.Wifi124,
    "HttpGet" => Symbol.Globe24,
    "HttpPost" => Symbol.Globe24,
    "TcpPortOpen" => Symbol.PlugConnected24,
    "UdpPortOpen" => Symbol.ConnectedNetwork24,  // NEW
    "DnsResolve" => Symbol.Dns24,
    // ... other mappings
    _ => Symbol.Question24
};
```

---

### 2.2 Update Color Converter

**File**: `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs`

Add to network test category:

```csharp
// Network tests
if (testType is "Ping" or "HttpGet" or "HttpPost" or "TcpPortOpen" or "UdpPortOpen" or "DnsResolve")
{
    return Application.Current.Resources["NetworkTestBrush"] as Brush ?? Brushes.Blue;
}
```

---

## Step 3: Update Default Profile (15 minutes)

**File**: `src/ReqChecker.App/Profiles/default-profile.json`

Add UDP test to the `tests` array:

```json
{
  "id": "test-udp-001",
  "name": "DNS Server Reachability",
  "type": "UdpPortOpen",
  "description": "Verify DNS query to Google Public DNS succeeds",
  "parameters": {
    "host": "8.8.8.8",
    "port": 53,
    "timeout": 5000,
    "payload": "00010100000100000000000003777777076578616d706c6503636f6d0000010001",
    "encoding": "hex"
  },
  "fieldPolicy": {
    "host": "Editable",
    "port": "Editable",
    "timeout": "Editable",
    "payload": "Hidden",
    "expectedResponse": "Hidden",
    "encoding": "Hidden"
  }
}
```

---

## Step 4: Write Unit Tests (1-1.5 hours)

**File**: `tests/ReqChecker.Infrastructure.Tests/Tests/UdpPortOpenTestTests.cs`

```csharp
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Tests;
using Xunit;

namespace ReqChecker.Infrastructure.Tests.Tests;

public class UdpPortOpenTestTests
{
    [Fact]
    public async Task ExecuteAsync_ValidDnsQuery_ReturnsPass()
    {
        // Arrange
        var test = new UdpPortOpenTest();
        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "UdpPortOpen",
            Parameters = new Dictionary<string, object>
            {
                ["host"] = "8.8.8.8",
                ["port"] = 53,
                ["timeout"] = 5000,
                ["payload"] = "00010100000100000000000003777777076578616d706c6503636f6d0000010001",
                ["encoding"] = "hex"
            }
        };

        // Act
        var result = await test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        result.Status.Should().Be(TestStatus.Pass);
        result.ErrorCategory.Should().BeNull();
        result.HumanSummary.Should().Contain("responded");
        result.ResponseData.Should().Contain("\"responded\":true");
    }

    [Fact]
    public async Task ExecuteAsync_UnreachablePort_ReturnsFail()
    {
        // Arrange
        var test = new UdpPortOpenTest();
        var definition = new TestDefinition
        {
            Id = "test-002",
            Type = "UdpPortOpen",
            Parameters = new Dictionary<string, object>
            {
                ["host"] = "127.0.0.1",
                ["port"] = 9999,
                ["timeout"] = 1000
            }
        };

        // Act
        var result = await test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        result.Status.Should().Be(TestStatus.Fail);
        result.ErrorCategory.Should().Be(ErrorCategory.Timeout);
        result.HumanSummary.Should().Contain("did not respond");
    }

    [Fact]
    public async Task ExecuteAsync_InvalidPort_ReturnsConfigurationError()
    {
        // Arrange
        var test = new UdpPortOpenTest();
        var definition = new TestDefinition
        {
            Id = "test-003",
            Type = "UdpPortOpen",
            Parameters = new Dictionary<string, object>
            {
                ["host"] = "8.8.8.8",
                ["port"] = 99999  // Invalid
            }
        };

        // Act
        var result = await test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        result.Status.Should().Be(TestStatus.Fail);
        result.ErrorCategory.Should().Be(ErrorCategory.Configuration);
    }

    [Fact]
    public async Task ExecuteAsync_MissingHost_ReturnsConfigurationError()
    {
        // Arrange
        var test = new UdpPortOpenTest();
        var definition = new TestDefinition
        {
            Id = "test-004",
            Type = "UdpPortOpen",
            Parameters = new Dictionary<string, object>
            {
                ["port"] = 53  // Missing host
            }
        };

        // Act
        var result = await test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        result.Status.Should().Be(TestStatus.Fail);
        result.ErrorCategory.Should().Be(ErrorCategory.Configuration);
        result.HumanSummary.Should().Contain("host");
    }

    // Add more test cases as needed
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

1. **Load default profile** → Should see "DNS Server Reachability" test
2. **Run test** → Should pass with evidence showing response time
3. **Edit timeout to 1ms** → Should fail with timeout error
4. **Edit host to invalid hostname** → Should fail with DNS resolution error
5. **View Results** → Should show Pass/Fail with detailed evidence

---

## Step 6: Verification Checklist

- [ ] `UdpPortOpenTest.cs` compiles without errors
- [ ] `[TestType("UdpPortOpen")]` attribute present
- [ ] Parameter extraction validates required fields (`host`, `port`)
- [ ] DNS resolution handles both IP addresses and hostnames
- [ ] Timeout is enforced via `CancellationTokenSource`
- [ ] ICMP errors caught and categorized as `ErrorCategory.Network`
- [ ] Evidence includes all 6 required fields
- [ ] Icon converter maps `UdpPortOpen` → `ConnectedNetwork24`
- [ ] Color converter maps `UdpPortOpen` → network test color
- [ ] Default profile includes sample UDP test
- [ ] Unit tests cover success, timeout, and error cases
- [ ] Manual testing confirms UI displays test correctly
- [ ] Results page shows evidence with response preview

---

## Common Issues & Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| "host is required" exception | Missing parameter in profile JSON | Add `"host": "..."` to `parameters` |
| Timeout even for reachable service | Firewall blocking UDP | Check Windows Firewall, add exception if needed |
| ICMP errors not detected | Platform limitation (Linux/macOS) | Document as known limitation, focus on Windows testing |
| Response validation always fails | Encoding mismatch | Verify `expectedResponse` uses same encoding as `payload` |
| DNS resolution slow | Network latency | Increase timeout or use IP address instead of hostname |

---

## Next Steps

After implementation is complete:

1. Run full test suite: `dotnet test`
2. Commit changes: `git add . && git commit -m "feat: implement UdpPortOpen test type"`
3. Generate tasks file: Run `/speckit.tasks` command
4. Begin implementation following task breakdown

---

**Estimated Total Time**: 3-4 hours
**Complexity**: Medium (similar to `TcpPortOpenTest`, adds payload encoding logic)
