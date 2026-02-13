# Contracts: UDP Port Reachability Test

**Feature**: `039-udp-port-test`
**Date**: 2026-02-13

## Overview

This directory documents the interface contracts for the `UdpPortOpen` test type. ReqChecker uses an **internal plugin architecture** (not REST/GraphQL APIs), so contracts define:

1. **ITest Interface Contract** — The execution interface all test types implement
2. **Test Discovery Contract** — How the test registers itself via `[TestType]` attribute
3. **Parameter Contract** — JSON schema for test configuration
4. **Evidence Contract** — JSON schema for test results

---

## 1. ITest Interface Contract

### Interface Definition

All test types must implement `ReqChecker.Core.Interfaces.ITest`:

```csharp
public interface ITest
{
    /// <summary>
    /// Executes the test asynchronously.
    /// </summary>
    /// <param name="definition">Test configuration including parameters and policies.</param>
    /// <param name="context">Execution context (optional, may be null).</param>
    /// <param name="cancellationToken">Cancellation token for timeout/user cancel.</param>
    /// <returns>Test result with status, evidence, and error details.</returns>
    Task<TestResult> ExecuteAsync(
        TestDefinition definition,
        TestExecutionContext? context,
        CancellationToken cancellationToken);
}
```

### Implementation Signature

```csharp
[TestType("UdpPortOpen")]
public class UdpPortOpenTest : ITest
{
    public async Task<TestResult> ExecuteAsync(
        TestDefinition definition,
        TestExecutionContext? context,
        CancellationToken cancellationToken)
    {
        // Implementation details
    }
}
```

### Contract Guarantees

**Inputs**:
- `definition.Parameters`: JSON object containing `host`, `port`, `timeout`, etc.
- `definition.Timeout`: Optional override for global timeout (nullable)
- `cancellationToken`: Must be respected; throw `OperationCanceledException` when cancelled

**Outputs**:
- `TestResult.Status`: One of `Pass`, `Fail`, `Skipped`
- `TestResult.ErrorCategory`: One of `Network`, `Timeout`, `Validation`, `Configuration`, `Unknown` (only for `Fail` status)
- `TestResult.ResponseData`: Serialized JSON evidence (see Evidence Contract)
- `TestResult.HumanSummary`: User-friendly message (1-2 sentences)
- `TestResult.TechnicalDetails`: Diagnostic information (multi-line, detailed)

**Error Handling**:
- Exceptions must be caught and converted to `Fail` result with appropriate error category
- Timeout via cancellation token → `ErrorCategory.Timeout`
- Network errors → `ErrorCategory.Network`
- Validation failures → `ErrorCategory.Validation`
- Invalid parameters → `ErrorCategory.Configuration`

---

## 2. Test Discovery Contract

### Attribute Registration

```csharp
[TestType("UdpPortOpen")]
public class UdpPortOpenTest : ITest
{
    // Implementation
}
```

**Contract**:
- The `[TestType("UdpPortOpen")]` attribute enables auto-discovery by the DI container
- Type name must be unique across all tests
- Multiple aliases NOT supported in this implementation (unlike `DnsResolve`/`DnsLookup`)
- The test runner looks up implementations via `IServiceProvider.GetKeyedService<ITest>("UdpPortOpen")`

### Dependency Injection

```csharp
// Registration (auto-scanned by startup)
services.AddKeyedTransient<ITest, UdpPortOpenTest>("UdpPortOpen");

// Resolution
var test = serviceProvider.GetRequiredKeyedService<ITest>("UdpPortOpen");
```

---

## 3. Parameter Contract (JSON Schema)

### Profile JSON Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "description": "Unique test identifier within profile"
    },
    "name": {
      "type": "string",
      "description": "Display name shown in UI"
    },
    "type": {
      "type": "string",
      "enum": ["UdpPortOpen"],
      "description": "Test type identifier"
    },
    "description": {
      "type": "string",
      "description": "Optional description of what this test validates"
    },
    "timeout": {
      "type": "integer",
      "minimum": 1,
      "description": "Optional test-specific timeout override (milliseconds)"
    },
    "parameters": {
      "type": "object",
      "properties": {
        "host": {
          "type": "string",
          "minLength": 1,
          "description": "Target hostname, IPv4, or IPv6 address"
        },
        "port": {
          "type": "integer",
          "minimum": 1,
          "maximum": 65535,
          "description": "Target UDP port number"
        },
        "timeout": {
          "type": "integer",
          "minimum": 1,
          "default": 5000,
          "description": "Timeout in milliseconds (default 5000)"
        },
        "payload": {
          "type": "string",
          "description": "Hex or base64-encoded UDP datagram payload (default: single null byte)"
        },
        "expectedResponse": {
          "type": ["string", "null"],
          "description": "Expected response pattern as hex or base64 (null = any response accepted)"
        },
        "encoding": {
          "type": "string",
          "enum": ["auto", "hex", "base64", "utf8"],
          "default": "auto",
          "description": "Payload encoding hint (auto-detect if not specified)"
        }
      },
      "required": ["host", "port"],
      "additionalProperties": false
    },
    "fieldPolicy": {
      "type": "object",
      "description": "Field-level visibility/editability policies",
      "properties": {
        "host": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "port": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "timeout": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "payload": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "expectedResponse": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "encoding": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] }
      },
      "additionalProperties": false
    }
  },
  "required": ["id", "name", "type", "parameters"]
}
```

### Parameter Extraction Contract

```csharp
// In UdpPortOpenTest.ExecuteAsync()
var parameters = definition.Parameters;

string host = parameters["host"]?.ToString() ?? throw new ArgumentException("host required");
int port = parameters["port"]?.GetValue<int>() ?? throw new ArgumentException("port required");
int timeout = parameters["timeout"]?.GetValue<int>() ?? 5000;
string? payload = parameters["payload"]?.ToString();
string? expectedResponse = parameters["expectedResponse"]?.ToString();
string encoding = parameters["encoding"]?.ToString() ?? "auto";
```

**Contract Rules**:
- `host` and `port` are mandatory; throw `ArgumentException` if missing
- `timeout`, `payload`, `expectedResponse`, `encoding` are optional with defaults
- Invalid values (e.g., port out of range) → return `Fail` result with `ErrorCategory.Configuration`

---

## 4. Evidence Contract (JSON Schema)

### Evidence Output Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "responded": {
      "type": "boolean",
      "description": "Whether a UDP response was received"
    },
    "roundTripTimeMs": {
      "type": ["integer", "null"],
      "minimum": 0,
      "description": "Round-trip time in milliseconds (null if no response)"
    },
    "remoteEndpoint": {
      "type": "string",
      "pattern": "^.+:\\d+$",
      "description": "Remote endpoint in 'IP:Port' format (e.g., '8.8.8.8:53')"
    },
    "payloadSentBytes": {
      "type": "integer",
      "minimum": 0,
      "description": "Size of sent UDP datagram in bytes"
    },
    "payloadReceivedBytes": {
      "type": ["integer", "null"],
      "minimum": 0,
      "description": "Size of received UDP datagram in bytes (null if no response)"
    },
    "responseDataPreview": {
      "type": ["string", "null"],
      "description": "First 64 bytes of response as hex string (null if no response)"
    }
  },
  "required": ["responded", "remoteEndpoint", "payloadSentBytes"],
  "additionalProperties": false
}
```

### Evidence Serialization Contract

```csharp
// Create evidence object
var evidence = new UdpPortTestEvidence
{
    Responded = true,
    RoundTripTimeMs = 12,
    RemoteEndpoint = "8.8.8.8:53",
    PayloadSentBytes = 32,
    PayloadReceivedBytes = 64,
    ResponseDataPreview = "3082014502010004067075626c6963a036..."
};

// Serialize to JSON
var options = new JsonSerializerOptions { WriteIndented = false };
string json = JsonSerializer.Serialize(evidence, options);

// Store in TestResult
return new TestResult
{
    Status = TestStatus.Pass,
    ResponseData = json,
    HumanSummary = "UDP port 53 on 8.8.8.8 responded in 12 ms",
    TechnicalDetails = "Sent 32-byte datagram...",
    // ... other fields
};
```

**Contract Rules**:
- Evidence must serialize to valid JSON matching the schema above
- `responded` is always present (boolean)
- `roundTripTimeMs`, `payloadReceivedBytes`, `responseDataPreview` are nullable (null when no response)
- `responseDataPreview` is truncated to first 64 bytes (128 hex characters) for readability

---

## 5. TestResult Contract

### Output Schema

```csharp
public class TestResult
{
    public string TestId { get; set; }              // From TestDefinition.Id
    public TestStatus Status { get; set; }          // Pass | Fail | Skipped
    public ErrorCategory? ErrorCategory { get; set; } // Network | Timeout | Validation | Configuration (null for Pass)
    public string HumanSummary { get; set; }        // User-friendly 1-2 sentence summary
    public string TechnicalDetails { get; set; }    // Multi-line diagnostic details
    public string ResponseData { get; set; }        // Serialized UdpPortTestEvidence JSON
    public DateTime StartTime { get; set; }         // Test start timestamp
    public DateTime EndTime { get; set; }           // Test end timestamp
    public int TotalMs { get; set; }                // Total execution time (EndTime - StartTime)
}
```

### Status-Specific Contracts

| Status | ErrorCategory | HumanSummary Format | TechnicalDetails Content |
|--------|---------------|---------------------|-------------------------|
| `Pass` | `null` | "UDP port {port} on {host} responded in {ms} ms" | Sent/received byte counts, response preview |
| `Fail` (timeout) | `Timeout` | "UDP port {port} on {host} did not respond within {timeout} ms" | Sent byte count, possible causes |
| `Fail` (ICMP) | `Network` | "UDP port {port} on {host} is unreachable (ICMP ...)" | ICMP error details, socket error code |
| `Fail` (validation) | `Validation` | "UDP port {port} on {host} responded but data did not match" | Expected vs. actual hex comparison |
| `Fail` (DNS) | `Network` | "Could not resolve hostname '{host}'" | DNS error message |
| `Fail` (config) | `Configuration` | "Invalid parameter: {parameter}" | Validation error details |
| `Skipped` | `null` | "Skipped: dependency {dep} failed" | Dependency name and reason |

---

## 6. UI Integration Contracts

### Icon Converter Contract

```csharp
// TestTypeToIconConverter.cs
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is string testType)
    {
        return testType switch
        {
            "UdpPortOpen" => Symbol.ConnectedNetwork24,
            // ... other mappings
            _ => Symbol.Question24
        };
    }
    return Symbol.Question24;
}
```

### Color Converter Contract

```csharp
// TestTypeToColorConverter.cs
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is string testType)
    {
        // Network test category (same as Ping, TcpPortOpen, DnsResolve)
        if (testType == "UdpPortOpen")
        {
            return Application.Current.Resources["NetworkTestBrush"] as Brush ?? Brushes.Blue;
        }
        // ... other mappings
    }
    return Brushes.Gray;
}
```

---

## 7. Backward Compatibility

**Breaking Changes**: None (new test type, existing tests unaffected)

**Profile Schema Version**: Remains at version 3 (no schema changes, just new test type)

**Migration**: Not required; existing profiles continue to work

---

## Summary

- **ITest interface**: Standard async execution contract with cancellation support
- **Discovery**: `[TestType("UdpPortOpen")]` attribute for DI registration
- **Parameters**: 2 required (`host`, `port`), 4 optional with defaults
- **Evidence**: 6-field structured JSON with nullable fields for error cases
- **TestResult**: Standard status/error/summary/details contract
- **UI Integration**: Icon (`ConnectedNetwork24`) and color (network category)

**All contracts defined** — ready for quickstart implementation guide.
