# Research: Mutual TLS Client Certificate Authentication Test

**Feature**: `001-mtls-test`
**Date**: 2026-02-13

## Executive Summary

This document captures the technical research and decisions for implementing mTLS (mutual TLS) client certificate authentication testing in ReqChecker. All decisions align with existing architectural patterns (no new dependencies, follows `ITest` interface, uses built-in .NET APIs).

---

## R1: TLS Client Certificate API Selection

**Decision**: Use `HttpClientHandler.ClientCertificates` with `X509Certificate2` loaded from PFX file

**Rationale**:
- `HttpClientHandler` is the standard .NET mechanism for configuring client certificates on `HttpClient`
- `X509Certificate2` natively loads PFX/PKCS#12 files via constructor: `new X509Certificate2(path, password)`
- No new NuGet dependencies required — all APIs are in `System.Security.Cryptography.X509Certificates` and `System.Net.Http`
- Consistent with the `HttpGetTest` pattern which also uses `HttpClient`
- `HttpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual` ensures the certificate is only sent when explicitly configured

**Alternatives Considered**:
1. **`SslStream` with raw TCP**: Lower-level control but requires manual HTTP protocol handling. Too complex for this use case where `HttpClient` already supports client certificates natively.
2. **`WebRequest` (deprecated)**: Legacy API, not recommended for .NET 8+
3. **Third-party TLS libraries** (e.g., `BouncyCastle`): Unnecessary — .NET 8's built-in TLS stack fully supports client certificates with PFX files

**Implementation Notes**:
- Cannot reuse a static `HttpClient` (unlike `HttpGetTest`) because each test may use a different client certificate. Create a new `HttpClientHandler` + `HttpClient` per test execution.
- Dispose both handler and client after each test to release certificate resources
- Use `HttpClientHandler.ServerCertificateCustomValidationCallback` for server certificate validation skip option

---

## R2: PFX File Loading Strategy

**Decision**: Load PFX via `X509Certificate2(string fileName, string? password)` constructor with pre-validation

**Rationale**:
- Direct file path loading is the simplest approach and matches the existing file-based parameter pattern (e.g., `FileExists`, `FileRead` tests)
- Password parameter comes from `TestExecutionContext.Password` (PromptAtRun field policy)
- Pre-validation before connection catches invalid files early with clear error messages

**Pre-Validation Steps**:
1. Check file exists at configured path
2. Try loading PFX with provided password — catches wrong password, corrupted files, invalid format
3. Verify loaded certificate has a private key (`cert.HasPrivateKey`)
4. Optionally check certificate validity dates (warn if expired, but still attempt connection — let the server decide)

**Alternatives Considered**:
1. **Windows Certificate Store (by thumbprint)**: Deferred to future release per spec clarification. Would use `X509Store` + `Certificates.Find(X509FindType.FindByThumbprint, ...)`
2. **PEM file loading**: Deferred — PFX is the Windows standard. .NET 8 supports PEM via `X509Certificate2.CreateFromPemFile()` but this adds complexity without clear user value on Windows.

**Implementation Notes**:
- `X509KeyStorageFlags.DefaultKeySet` is sufficient; no need for `MachineKeySet` or `EphemeralKeySet` for test-scoped certificates
- Handle `CryptographicException` for wrong password / corrupted PFX with `ErrorCategory.Configuration`

---

## R3: Server Certificate Validation

**Decision**: Use `HttpClientHandler.ServerCertificateCustomValidationCallback` for skip-validation opt-in

**Rationale**:
- Default behavior: standard server certificate validation (trusts Windows certificate store)
- When `skipServerCertValidation` parameter is `true`: bypass server certificate validation entirely using `HttpClientHandler.DangerousAcceptAnyServerCertificateValidator`
- This matches the spec requirement for explicit opt-in to skip server validation
- Consistent with FTP tests' `validateCertificate` parameter pattern

**Implementation**:
```csharp
if (skipServerCertValidation)
{
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
}
```

**Alternatives Considered**:
1. **Custom CA certificate bundle parameter**: The spec includes FR-010 for optional CA certificate. Implementation: load additional CA cert as `X509Certificate2`, add to `handler.ClientCertificates` or use custom validation callback that checks against the provided CA. Deferred to P2 — skip-validation covers the primary self-signed cert use case.
2. **Certificate pinning**: Out of scope — this is a diagnostic tool, not a production client

---

## R4: PFX Password Credential Flow

**Decision**: Use existing `credentialRef` + `PromptAtRun` field policy pattern for PFX password

**Rationale**:
- Exact same pattern as FTP tests (`FtpReadTest`, `FtpWriteTest`)
- `SequentialTestRunner.PromptForCredentialsIfNeededAsync()` already handles credential prompting
- Password is retrieved via `context?.Password` at test execution time
- Password is never persisted in profile JSON (secure by design)
- Users can optionally store credentials in Windows Credential Manager via the "Remember" checkbox

**Flow**:
1. Profile JSON has `"credentialRef": "mtls-cert-password"` in parameters
2. `SequentialTestRunner` detects `credentialRef` and prompts user for password
3. `TestExecutionContext.Password` is populated with the PFX password
4. `MtlsConnectTest.ExecuteAsync()` reads `context?.Password` and uses it to load PFX
5. If no `credentialRef` is set (password-less PFX), no prompt occurs and `context` may be null

**Implementation Notes**:
- The credential prompt label should clearly indicate this is a "PFX certificate password" (via `credentialRef` naming convention)
- If the PFX file has no password, users can leave the prompt empty or the profile can omit `credentialRef` entirely

---

## R5: HTTP Request Method and Expected Status

**Decision**: Use HTTP GET with configurable expected status code (default 200)

**Rationale**:
- HTTP GET is the simplest verification method and aligns with `HttpGetTest` pattern
- Expected status code allows users to validate specific responses (e.g., 200 OK, 204 No Content, 403 Forbidden for negative testing)
- Default 200 covers the most common success case
- The test verifies end-to-end: TLS handshake with client cert → HTTP request → status code validation

**Alternatives Considered**:
1. **HEAD request**: Lighter weight but some servers don't support HEAD on all endpoints
2. **Configurable HTTP method**: Over-engineered for a certificate authentication test; users who need POST with body should use `HttpPost` test with a client certificate (future enhancement)
3. **Response body validation**: Not needed for certificate authentication testing — status code is sufficient

---

## R6: Error Categorization Strategy

**Decision**: Map specific .NET exception types to `ErrorCategory` values with detailed human-readable messages

**Mapping**:

| Exception / Condition | ErrorCategory | Human Summary |
|----------------------|---------------|---------------|
| `CryptographicException` (PFX load) | `Configuration` | "Cannot load PFX file: incorrect password or corrupted file" |
| `FileNotFoundException` | `Configuration` | "PFX file not found: {path}" |
| PFX loaded but `!HasPrivateKey` | `Configuration` | "PFX file does not contain a private key" |
| `HttpRequestException` (TLS handshake failure) | `Network` | "TLS handshake failed: server rejected client certificate" |
| `AuthenticationException` | `Permission` | "Client certificate authentication failed" |
| Status code mismatch | `Validation` | "Unexpected HTTP status: expected {expected}, got {actual}" |
| `OperationCanceledException` (user cancel) | `Unknown` | "Test was cancelled" |
| `TaskCanceledException` (timeout) | `Timeout` | "Connection timed out after {timeout}ms" |
| Other `Exception` | `Network` | "Connection error: {message}" |

**Implementation Notes**:
- Extract inner exception messages for TLS errors — .NET wraps TLS failures in multiple exception layers
- Include certificate subject, issuer, and thumbprint in `TechnicalDetails` when certificate is successfully loaded
- Include server error details when available (e.g., HTTP 403 response body)

---

## R7: Test Evidence Structure

**Decision**: Capture structured evidence with certificate metadata, connection details, and HTTP response info

**Evidence Schema**:
```csharp
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
```

**Rationale**:
- Certificate metadata (subject, issuer, thumbprint, dates) provides diagnostic value without exposing sensitive key material
- `Connected` flag distinguishes TLS handshake success from HTTP-level verification
- `ServerCertValidationSkipped` documents the security posture for audit trail

---

## R8: Icon and Color Mapping

**Decision**: Use `SymbolRegular.ShieldKeyhole24` icon and network-category color (same as HttpGet, Ping, etc.)

**Rationale**:
- `ShieldKeyhole24` represents security/authentication — appropriate for certificate-based auth testing
- Network category color (blue/teal) — mTLS is a network connectivity test with a security layer
- Consistent with existing test type visual hierarchy

**Converter Updates**:
- `TestTypeToIconConverter.cs`: Add case `"MtlsConnect" => SymbolRegular.ShieldKeyhole24`
- `TestTypeToColorConverter.cs`: Add `"MtlsConnect"` to the network test category group

---

## R9: Default Profile Sample Test

**Decision**: Include a commented/placeholder mTLS test in the default profile (disabled by default)

**Rationale**:
- Unlike Ping or DNS tests, mTLS requires endpoint-specific configuration (URL + PFX file) that cannot have a universal default
- Including a sample test definition shows users the correct parameter format
- The test should target a placeholder URL that clearly indicates it needs customization

**Sample Test Configuration**:
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

## Summary Table

| Research Area | Decision | Key Trade-off |
|---------------|----------|---------------|
| **TLS API** | `HttpClientHandler.ClientCertificates` | Built-in (no deps) vs. raw SslStream |
| **Certificate Format** | PFX/PKCS#12 only | Windows-native UX vs. cross-platform PEM support |
| **Server Cert Validation** | `DangerousAcceptAnyServerCertificateValidator` opt-in | Security default vs. dev/test convenience |
| **Password Handling** | `credentialRef` + PromptAtRun | Never persisted vs. profile convenience |
| **HTTP Method** | GET with expected status | Simple verification vs. configurable method |
| **Error Mapping** | Exception type → ErrorCategory | Detailed diagnostics vs. simple pass/fail |
| **Evidence Schema** | 12-field structured JSON | Diagnostic depth vs. data volume |
| **Icon** | `ShieldKeyhole24` | Security-focused vs. network-focused icon |
| **Default Profile** | Placeholder sample test | User guidance vs. working out-of-box |

---

**Research Complete**: All technical unknowns resolved. Ready for Phase 1 (data model and contracts).
