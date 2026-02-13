# Contracts: Mutual TLS Client Certificate Authentication Test

**Feature**: `001-mtls-test`
**Date**: 2026-02-13

## Overview

This directory documents the interface contracts for the `MtlsConnect` test type. ReqChecker uses an **internal plugin architecture** (not REST/GraphQL APIs), so contracts define:

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
    Task<TestResult> ExecuteAsync(
        TestDefinition definition,
        TestExecutionContext? context,
        CancellationToken cancellationToken);
}
```

### Implementation Signature

```csharp
[TestType("MtlsConnect")]
public class MtlsConnectTest : ITest
{
    public async Task<TestResult> ExecuteAsync(
        TestDefinition testDefinition,
        TestExecutionContext? context,
        CancellationToken cancellationToken = default)
    {
        // Implementation details
    }
}
```

### Contract Guarantees

**Inputs**:
- `testDefinition.Parameters`: JSON object containing `url`, `clientCertPath`, `expectedStatus`, `timeout`, `skipServerCertValidation`, `credentialRef`
- `context?.Password`: PFX file password (provided by PromptAtRun credential flow, may be null for password-less PFX)
- `cancellationToken`: Must be respected; handle `OperationCanceledException` when cancelled

**Outputs**:
- `TestResult.Status`: One of `Pass`, `Fail`, `Skipped`
- `TestResult.Error.Category`: One of `Network`, `Timeout`, `Validation`, `Configuration`, `Permission`, `Unknown` (only for `Fail` status)
- `TestResult.Evidence.ResponseData`: Serialized `MtlsConnectTestEvidence` JSON
- `TestResult.HumanSummary`: User-friendly message (1-2 sentences)
- `TestResult.TechnicalDetails`: Diagnostic information (multi-line, detailed)

**Error Handling**:
- Exceptions must be caught and converted to `Fail` result with appropriate error category
- `CryptographicException` → `ErrorCategory.Configuration` (bad PFX password, corrupted file)
- `FileNotFoundException` → `ErrorCategory.Configuration` (PFX file missing)
- `HttpRequestException` / `AuthenticationException` → `ErrorCategory.Network` (TLS handshake failure)
- HTTP status mismatch → `ErrorCategory.Validation`
- `TaskCanceledException` (timeout) → `ErrorCategory.Timeout`
- `OperationCanceledException` (user cancel) → `TestStatus.Skipped`

---

## 2. Test Discovery Contract

### Attribute Registration

```csharp
[TestType("MtlsConnect")]
public class MtlsConnectTest : ITest
{
    // Implementation
}
```

**Contract**:
- The `[TestType("MtlsConnect")]` attribute enables auto-discovery by the DI container
- Type name must be unique across all tests
- The test runner looks up implementations via `IServiceProvider.GetKeyedService<ITest>("MtlsConnect")`

### Dependency Injection

```csharp
// Registration (auto-scanned by startup)
services.AddKeyedTransient<ITest, MtlsConnectTest>("MtlsConnect");

// Resolution
var test = serviceProvider.GetRequiredKeyedService<ITest>("MtlsConnect");
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
    "type": {
      "type": "string",
      "enum": ["MtlsConnect"],
      "description": "Test type identifier"
    },
    "displayName": {
      "type": "string",
      "description": "Display name shown in UI"
    },
    "description": {
      "type": "string",
      "description": "Optional description of what this test validates"
    },
    "parameters": {
      "type": "object",
      "properties": {
        "url": {
          "type": "string",
          "format": "uri",
          "pattern": "^https://",
          "description": "Target HTTPS endpoint URL"
        },
        "clientCertPath": {
          "type": "string",
          "minLength": 1,
          "description": "Path to PFX/PKCS#12 client certificate file"
        },
        "expectedStatus": {
          "type": "integer",
          "minimum": 100,
          "maximum": 599,
          "default": 200,
          "description": "Expected HTTP response status code"
        },
        "timeout": {
          "type": "integer",
          "minimum": 1,
          "default": 30000,
          "description": "Connection timeout in milliseconds"
        },
        "skipServerCertValidation": {
          "type": "boolean",
          "default": false,
          "description": "Skip server certificate validation (for self-signed certificates)"
        },
        "credentialRef": {
          "type": ["string", "null"],
          "description": "Credential reference key for PFX password (triggers PromptAtRun)"
        }
      },
      "required": ["url", "clientCertPath"],
      "additionalProperties": false
    },
    "fieldPolicy": {
      "type": "object",
      "properties": {
        "url": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "clientCertPath": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "expectedStatus": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "timeout": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "skipServerCertValidation": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] },
        "credentialRef": { "type": "string", "enum": ["Locked", "Editable", "Hidden", "PromptAtRun"] }
      },
      "additionalProperties": false
    }
  },
  "required": ["id", "type", "parameters"]
}
```

### Parameter Extraction Contract

```csharp
// In MtlsConnectTest.ExecuteAsync()
var parameters = testDefinition.Parameters;

string url = parameters["url"]?.ToString()
    ?? throw new ArgumentException("URL parameter is required");
string clientCertPath = parameters["clientCertPath"]?.ToString()
    ?? throw new ArgumentException("Client certificate path is required");
int expectedStatus = parameters["expectedStatus"]?.GetValue<int>() ?? 200;
int timeout = parameters["timeout"]?.GetValue<int>() ?? 30000;
bool skipServerCertValidation = parameters["skipServerCertValidation"]?.GetValue<bool>() ?? false;

// PFX password from credential flow (may be null for password-less PFX)
string? pfxPassword = context?.Password;
```

---

## 4. Evidence Contract (JSON Schema)

### Evidence Output Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "connected": {
      "type": "boolean",
      "description": "Whether TLS connection with client certificate was established"
    },
    "httpStatusCode": {
      "type": ["integer", "null"],
      "description": "HTTP status code received (null if connection failed)"
    },
    "responseTimeMs": {
      "type": ["integer", "null"],
      "minimum": 0,
      "description": "Response time in milliseconds (null if connection failed)"
    },
    "certificateSubject": {
      "type": ["string", "null"],
      "description": "Client certificate subject DN"
    },
    "certificateIssuer": {
      "type": ["string", "null"],
      "description": "Client certificate issuer DN"
    },
    "certificateThumbprint": {
      "type": ["string", "null"],
      "description": "Client certificate SHA-1 thumbprint"
    },
    "certificateNotBefore": {
      "type": ["string", "null"],
      "format": "date-time",
      "description": "Certificate validity start date"
    },
    "certificateNotAfter": {
      "type": ["string", "null"],
      "format": "date-time",
      "description": "Certificate validity end date"
    },
    "certificateHasPrivateKey": {
      "type": "boolean",
      "description": "Whether the certificate contains a private key"
    },
    "serverCertValidationSkipped": {
      "type": "boolean",
      "description": "Whether server certificate validation was disabled"
    },
    "serverCertificateSubject": {
      "type": ["string", "null"],
      "description": "Server certificate subject DN"
    },
    "errorDetail": {
      "type": ["string", "null"],
      "description": "Additional error or diagnostic detail"
    }
  },
  "required": ["connected", "certificateHasPrivateKey", "serverCertValidationSkipped"],
  "additionalProperties": false
}
```

---

## 5. UI Integration Contracts

### Icon Converter Contract

```csharp
// TestTypeToIconConverter.cs
"MtlsConnect" => SymbolRegular.ShieldKeyhole24,
```

### Color Converter Contract

```csharp
// TestTypeToColorConverter.cs — add to network test category
"Ping" or "HttpGet" or "DnsLookup" or "DnsResolve" or "TcpPortOpen" or "UdpPortOpen" or "MtlsConnect" =>
    Application.Current.FindResource("StatusInfo") as SolidColorBrush ?? FallbackBrush,
```

---

## 6. Backward Compatibility

**Breaking Changes**: None (new test type, existing tests unaffected)

**Profile Schema Version**: Remains at version 3 (no schema changes, just new test type)

**Migration**: Not required; existing profiles continue to work

---

## Summary

- **ITest interface**: Standard async execution contract with cancellation support
- **Discovery**: `[TestType("MtlsConnect")]` attribute for DI registration
- **Parameters**: 2 required (`url`, `clientCertPath`), 4 optional with defaults
- **Evidence**: 12-field structured JSON with certificate metadata and connection details
- **TestResult**: Standard status/error/summary/details contract
- **UI Integration**: Icon (`ShieldKeyhole24`) and color (network category)
- **Credential flow**: PFX password via `credentialRef` + `PromptAtRun` field policy

**All contracts defined** — ready for quickstart implementation guide.
