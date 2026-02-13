# Data Model: Mutual TLS Client Certificate Authentication Test

**Feature**: `001-mtls-test`
**Date**: 2026-02-13

## Overview

This document defines the data structures for the `MtlsConnect` test type, including test parameters (stored in profile JSON), evidence output (captured during execution), and validation rules.

---

## Test Parameters

### Schema

Parameters are stored in the `TestDefinition.Parameters` JSON object within profile files.

| Parameter | Type | Required | Default | Validation | Description |
|-----------|------|----------|---------|------------|-------------|
| `url` | string | Yes | — | Non-empty, valid HTTPS URL | Target HTTPS endpoint URL |
| `clientCertPath` | string | Yes | — | Non-empty, valid file path | Path to PFX/PKCS#12 client certificate file |
| `expectedStatus` | integer | No | 200 | 100-599 | Expected HTTP response status code |
| `timeout` | integer | No | 30000 | > 0 | Connection timeout in milliseconds |
| `skipServerCertValidation` | boolean | No | false | — | Skip server certificate validation (for self-signed certs) |
| `credentialRef` | string | No | null | — | Credential reference key for PFX password (triggers PromptAtRun) |

### Example: Basic mTLS Test

```json
{
  "id": "test-mtls-001",
  "type": "MtlsConnect",
  "displayName": "API Gateway mTLS Check",
  "description": "Verify client certificate auth to API gateway",
  "parameters": {
    "url": "https://api.internal.corp.com/health",
    "clientCertPath": "C:\\certs\\api-client.pfx",
    "expectedStatus": 200,
    "timeout": 30000,
    "credentialRef": "api-client-cert-password"
  },
  "fieldPolicy": {
    "url": "Editable",
    "clientCertPath": "Editable",
    "expectedStatus": "Editable",
    "timeout": "Editable",
    "skipServerCertValidation": "Hidden",
    "credentialRef": "Hidden"
  }
}
```

**Behavior**: Loads PFX from `C:\certs\api-client.pfx`, prompts for password at runtime, connects to `https://api.internal.corp.com/health` with client certificate, expects HTTP 200.

---

### Example: Self-Signed Server Certificate

```json
{
  "id": "test-mtls-002",
  "type": "MtlsConnect",
  "displayName": "Dev Environment mTLS",
  "description": "Verify mTLS to dev server (self-signed cert)",
  "parameters": {
    "url": "https://dev-api.local:8443/status",
    "clientCertPath": "C:\\certs\\dev-client.pfx",
    "expectedStatus": 200,
    "timeout": 10000,
    "skipServerCertValidation": true
  },
  "fieldPolicy": {
    "url": "Editable",
    "clientCertPath": "Editable",
    "expectedStatus": "Editable",
    "timeout": "Editable",
    "skipServerCertValidation": "Locked"
  }
}
```

**Behavior**: Connects with client certificate and skips server certificate validation (for self-signed dev servers). No password prompt (password-less PFX or no `credentialRef`).

---

### Example: Locked Corporate Endpoint

```json
{
  "id": "test-mtls-003",
  "type": "MtlsConnect",
  "displayName": "Corporate Service Auth",
  "description": "Verify mTLS to corporate service endpoint",
  "parameters": {
    "url": "https://service.corp.internal/api/ping",
    "clientCertPath": "C:\\corporate\\client.pfx",
    "expectedStatus": 200,
    "timeout": 30000,
    "skipServerCertValidation": false,
    "credentialRef": "corp-service-cert"
  },
  "fieldPolicy": {
    "url": "Locked",
    "clientCertPath": "Locked",
    "expectedStatus": "Locked",
    "timeout": "Editable",
    "skipServerCertValidation": "Locked",
    "credentialRef": "Hidden"
  }
}
```

---

## Field-Level Policies

All parameters support the standard policy types:

| Policy | Behavior | Use Case |
|--------|----------|----------|
| `Locked` | Read-only with lock icon + tooltip | Company-managed endpoints and cert paths |
| `Editable` | Normal input field | User-customizable values |
| `Hidden` | Not rendered in UI | Internal parameters (e.g., `credentialRef`) |
| `PromptAtRun` | Dialog prompt before execution | Dynamic values |

---

## Evidence Output

### Schema

Evidence is captured during test execution and stored in `TestResult.Evidence.ResponseData` as serialized JSON.

```csharp
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
```

### Example: Successful Connection

```json
{
  "connected": true,
  "httpStatusCode": 200,
  "responseTimeMs": 145,
  "certificateSubject": "CN=api-client, O=Corp, C=US",
  "certificateIssuer": "CN=Corp Internal CA, O=Corp, C=US",
  "certificateThumbprint": "A1B2C3D4E5F6789012345678901234567890ABCD",
  "certificateNotBefore": "2025-01-01T00:00:00Z",
  "certificateNotAfter": "2027-01-01T00:00:00Z",
  "certificateHasPrivateKey": true,
  "serverCertValidationSkipped": false,
  "serverCertificateSubject": "CN=api.internal.corp.com, O=Corp",
  "errorDetail": null
}
```

### Example: Certificate Rejected

```json
{
  "connected": false,
  "httpStatusCode": null,
  "responseTimeMs": 52,
  "certificateSubject": "CN=expired-client, O=Corp",
  "certificateIssuer": "CN=Corp Internal CA, O=Corp",
  "certificateThumbprint": "DEADBEEF1234567890ABCDEF1234567890ABCDEF",
  "certificateNotBefore": "2023-01-01T00:00:00Z",
  "certificateNotAfter": "2024-01-01T00:00:00Z",
  "certificateHasPrivateKey": true,
  "serverCertValidationSkipped": false,
  "serverCertificateSubject": null,
  "errorDetail": "The remote certificate was rejected by the provided RemoteCertificateValidationCallback"
}
```

### Example: Unexpected HTTP Status

```json
{
  "connected": true,
  "httpStatusCode": 403,
  "responseTimeMs": 89,
  "certificateSubject": "CN=limited-client, O=Corp",
  "certificateIssuer": "CN=Corp Internal CA, O=Corp",
  "certificateThumbprint": "CAFE1234BEEF5678DEAD9012FACE3456BABE7890",
  "certificateNotBefore": "2025-06-01T00:00:00Z",
  "certificateNotAfter": "2026-06-01T00:00:00Z",
  "certificateHasPrivateKey": true,
  "serverCertValidationSkipped": false,
  "serverCertificateSubject": "CN=api.internal.corp.com, O=Corp",
  "errorDetail": "Expected status 200, received 403 Forbidden"
}
```

---

## Human Summary Messages

The `TestResult.HumanSummary` field provides user-friendly result descriptions:

| Scenario | Example Summary |
|----------|-----------------|
| **Success** | `"mTLS connection to https://api.corp.com/health succeeded — HTTP 200 in 145 ms (cert: CN=api-client)"` |
| **Status mismatch** | `"mTLS handshake succeeded but server returned HTTP 403 (expected 200)"` |
| **TLS rejected** | `"mTLS handshake failed — server rejected client certificate (cert: CN=api-client)"` |
| **Certificate expired** | `"Client certificate is expired (expired 2024-01-01) — connection not attempted"` |
| **Invalid PFX** | `"Cannot load PFX file: incorrect password or corrupted file"` |
| **PFX not found** | `"PFX file not found: C:\certs\missing.pfx"` |
| **Timeout** | `"Connection timed out after 30000 ms"` |

---

## Technical Details Messages

The `TestResult.TechnicalDetails` field provides diagnostic information:

| Scenario | Example Details |
|----------|-----------------|
| **Success** | `"Connected to https://api.corp.com/health\nHTTP GET → 200 OK (145 ms)\nClient cert: CN=api-client, O=Corp\nIssuer: CN=Corp CA\nThumbprint: A1B2C3...\nValid: 2025-01-01 to 2027-01-01"` |
| **TLS rejected** | `"TLS handshake to https://api.corp.com failed\nClient cert: CN=api-client (thumbprint: A1B2C3...)\nError: AuthenticationException — The remote certificate was rejected\nInner: The credentials supplied to the package were not recognized"` |
| **Invalid PFX** | `"Cannot load PFX from C:\certs\bad.pfx\nError: CryptographicException — The specified network password is not correct"` |

---

## Validation Rules

### Parameter Validation (Pre-Execution)

1. **url**: Non-null, non-empty, valid URI with HTTPS scheme
2. **clientCertPath**: Non-null, non-empty, file must exist on disk
3. **expectedStatus**: Integer in range `[100, 599]` (HTTP status codes)
4. **timeout**: Positive integer `> 0`
5. **skipServerCertValidation**: Boolean (default `false`)

### Certificate Validation (Pre-Connection)

1. **File access**: PFX file readable at configured path
2. **Password**: PFX loads successfully with provided password (or null for password-less)
3. **Private key**: Loaded certificate has private key (`cert.HasPrivateKey`)
4. **Validity dates**: Warn in evidence if certificate is expired or not yet valid (but still attempt connection)

### Execution Validation

1. **TLS handshake**: Client certificate presented during handshake; server accepts or rejects
2. **HTTP response**: Compare actual status code against `expectedStatus`
3. **Timeout enforcement**: Use `CancellationTokenSource.CancelAfter(timeout)`

---

## Error Categories

| Category | Trigger Condition |
|----------|------------------|
| `Configuration` | PFX file not found, wrong password, corrupted PFX, missing private key, invalid URL |
| `Network` | TLS handshake failure, connection refused, DNS resolution failure |
| `Permission` | Server explicitly rejected client certificate (HTTP 401/403 after handshake) |
| `Validation` | HTTP status code does not match expected value |
| `Timeout` | Connection or response exceeded configured timeout |
| `Unknown` | Test cancelled by user |

---

## State Transitions

mTLS tests follow the standard `TestStatus` state machine:

```
[Pending] → [Pass]      # TLS handshake succeeded AND HTTP status matches expected
         → [Fail]       # PFX load error, TLS rejection, status mismatch, timeout
         → [Skipped]    # Dependency failed, user cancelled, admin elevation blocked
```

---

## Integration Points

### Profile Loader

- `JsonProfileLoader` deserializes `parameters` into `JsonObject`
- `MtlsConnectTest.ExecuteAsync()` extracts parameters using `.GetValue<T>()`
- Field policies loaded from `TestDefinition.FieldPolicy` dictionary

### Credential Prompting

- `SequentialTestRunner.PromptForCredentialsIfNeededAsync()` detects `credentialRef` in parameters
- Prompts user for PFX password at test execution time
- Password available via `TestExecutionContext.Password`
- Optional: User can store in Windows Credential Manager via "Remember" checkbox

### UI Converters

- `TestTypeToIconConverter`: Maps `"MtlsConnect"` → `SymbolRegular.ShieldKeyhole24`
- `TestTypeToColorConverter`: Maps `"MtlsConnect"` → Network test color (`StatusInfo` brush)

### Evidence Serialization

```csharp
var evidence = new MtlsConnectTestEvidence
{
    Connected = true,
    HttpStatusCode = 200,
    ResponseTimeMs = 145,
    CertificateSubject = cert.Subject,
    CertificateIssuer = cert.Issuer,
    CertificateThumbprint = cert.Thumbprint,
    CertificateNotBefore = cert.NotBefore,
    CertificateNotAfter = cert.NotAfter,
    CertificateHasPrivateKey = cert.HasPrivateKey,
    ServerCertValidationSkipped = skipServerCertValidation
};

var json = JsonSerializer.Serialize(evidence);
// Store in TestResult.Evidence.ResponseData
```

---

## Summary

- **6 parameters**: `url`, `clientCertPath`, `expectedStatus`, `timeout`, `skipServerCertValidation`, `credentialRef`
- **12 evidence fields**: Certificate metadata, connection details, HTTP response, error info
- **6 error categories**: Configuration, Network, Permission, Validation, Timeout, Unknown
- **Field-level policies**: Full support for Locked/Editable/Hidden/PromptAtRun
- **Credential integration**: PFX password via existing `credentialRef` + PromptAtRun pattern

**Data model complete** — ready for contract generation and quickstart guide.
