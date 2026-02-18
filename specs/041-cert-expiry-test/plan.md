# Implementation Plan: SSL Certificate Expiry Test

**Branch**: `041-cert-expiry-test` | **Date**: 2026-02-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/041-cert-expiry-test/spec.md`

## Summary

Add a new `CertificateExpiry` test type that connects to a remote TLS endpoint via `TcpClient` + `SslStream`, retrieves the server's leaf certificate, and validates its expiry window against a configurable warning threshold. Optionally verifies certificate identity (Subject/SAN, Issuer, Thumbprint). Follows the established test type pattern (typed evidence class, inner parameters class, `[TestType]` attribute for auto-discovery).

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (in-memory test results; parameters persisted in profile JSON files)
**Testing**: xUnit (existing test project `ReqChecker.Infrastructure.Tests`)
**Target Platform**: Windows 10/11 desktop (WPF)
**Project Type**: Single solution with 3 projects (Core, Infrastructure, App)
**Performance Goals**: TLS handshake + certificate validation under 10 seconds per endpoint
**Constraints**: Direct TLS only (no STARTTLS); no client certificate presentation (use MtlsConnect for that)
**Scale/Scope**: Single test type addition — 1 new file, 3 modified files, 1 new test file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is unconfigured (template placeholders only). No gates to evaluate. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/041-cert-expiry-test/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 research findings
├── data-model.md        # Phase 1 data model
├── quickstart.md        # Phase 1 quickstart guide
└── checklists/
    └── requirements.md  # Spec quality checklist
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Core/
│   ├── Interfaces/ITest.cs                          # (existing) Interface implemented by CertificateExpiryTest
│   ├── Models/TestDefinition.cs                     # (existing) Profile JSON model consumed by test
│   ├── Models/TestResult.cs                         # (existing) Result model produced by test
│   ├── Models/TestEvidence.cs                       # (existing) Evidence container (ResponseData holds serialized JSON)
│   └── Enums/ErrorCategory.cs                       # (existing) Error categories used in test results
├── ReqChecker.Infrastructure/
│   └── Tests/
│       ├── TestTypeAttribute.cs                     # (existing) [TestType] attribute for auto-discovery
│       ├── MtlsConnectTest.cs                       # (existing) Reference pattern for TLS/certificate handling
│       ├── UdpPortOpenTest.cs                       # (existing) Reference pattern for typed evidence + params
│       └── CertificateExpiryTest.cs                 # *** NEW *** Test implementation
└── ReqChecker.App/
    ├── App.xaml.cs                                  # (existing) DI auto-discovers [TestType] classes — no change needed
    ├── Converters/
    │   ├── TestTypeToIconConverter.cs               # *** MODIFY *** Add CertificateExpiry icon
    │   └── TestTypeToColorConverter.cs              # *** MODIFY *** Add CertificateExpiry to network group
    └── Profiles/
        └── default-profile.json                     # *** MODIFY *** Add sample CertificateExpiry entries

tests/
└── ReqChecker.Infrastructure.Tests/
    └── Tests/
        └── CertificateExpiryTestTests.cs            # *** NEW *** Unit tests
```

**Structure Decision**: Follows existing single-solution structure. New test class goes in `Infrastructure/Tests/` alongside existing test types. No new projects or directories needed.

## Implementation Details

### File 1: `CertificateExpiryTest.cs` (NEW)

**Location**: `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs`

**Structure** (following MtlsConnectTest / UdpPortOpenTest pattern):

```
CertificateExpiryTestEvidence (public class)
  - All evidence fields per data-model.md

CertificateExpiryParameters (private class)
  - Extracted/validated parameters per data-model.md

[TestType("CertificateExpiry")]
CertificateExpiryTest : ITest
  - ExecuteAsync() — main entry point
  - ExtractParameters() — validate and extract from JsonObject
  - ConnectAndGetCertificateAsync() — TcpClient + SslStream handshake
  - ExtractSanEntries() — read SAN extension (OID 2.5.29.17)
  - EvaluateCertificate() — validity window + identity assertions
  - BuildSuccessResult()
  - BuildValidationFailResult()
  - BuildCancelledResult()
  - BuildTimeoutResult()
  - BuildNetworkErrorResult()
  - BuildConfigurationErrorResult()
  - BuildTechnicalDetails()
  - BuildEvidence()
```

**Key implementation details**:

1. **TLS handshake via SslStream**:
   ```
   TcpClient → ConnectAsync(host, port, linkedCts.Token)
   SslStream(networkStream, leaveInnerStreamOpen: false, validationCallback)
   SslStream.AuthenticateAsClientAsync(host) — sends SNI automatically
   Capture certificate from callback parameter
   ```

2. **Chain validation callback**:
   - When `skipChainValidation` = true: callback returns `true` (accept any chain)
   - When `skipChainValidation` = false: callback returns the default validation result
   - In both cases, capture the `X509Certificate2` from the callback

3. **SAN extraction**:
   ```
   cert.Extensions.OfType<X509SubjectAlternativeNameExtension>()
   → EnumerateDnsNames() for DNS SANs
   ```

4. **Identity matching** (FR-013 per clarification):
   - `expectedSubject`: substring match against Subject DN **OR** exact match against any SAN DNS entry
   - `expectedIssuer`: substring match against Issuer DN
   - `expectedThumbprint`: case-insensitive exact match against Thumbprint

5. **Pass/Fail logic**:
   ```
   PASS if ALL:
     - NotBefore <= UtcNow (not future-dated)
     - NotAfter > UtcNow (not expired)
     - DaysUntilExpiry >= warningDaysBeforeExpiry
     - (if expectedSubject set) Subject DN contains value OR any SAN matches
     - (if expectedIssuer set) Issuer DN contains value
     - (if expectedThumbprint set) Thumbprint matches (case-insensitive)
   ```

6. **Exception handling** (same pattern as MtlsConnectTest):
   - `OperationCanceledException` when `cancellationToken.IsCancellationRequested` → Skipped
   - `TaskCanceledException` / `OperationCanceledException` (timeout) → Fail/Timeout
   - `ArgumentException` → Fail/Configuration
   - `SocketException` → Fail/Network
   - `AuthenticationException` → Fail/Network (TLS handshake failure)
   - General `Exception` → Fail/Network

### File 2: `TestTypeToIconConverter.cs` (MODIFY)

Add to the switch expression:
```
"CertificateExpiry" => SymbolRegular.Certificate24,
```

Place after `"MtlsConnect"` entry for logical grouping.

### File 3: `TestTypeToColorConverter.cs` (MODIFY)

Add `"CertificateExpiry"` to the network/security group:
```
"Ping" or "HttpGet" or ... or "MtlsConnect" or "CertificateExpiry" =>
```

### File 4: `default-profile.json` (MODIFY)

Add two entries after the existing MtlsConnect entries:

1. **Happy path**: `test-017` targeting `www.google.com:443` with default 30-day warning
2. **Expected failure**: `test-018` targeting `expired.badssl.com:443` (known expired certificate)

### File 5: `CertificateExpiryTestTests.cs` (NEW)

**Location**: `tests/ReqChecker.Infrastructure.Tests/Tests/CertificateExpiryTestTests.cs`

**Test categories** (following MtlsConnectTestTests pattern):

1. **Parameter validation** (no network required):
   - Missing host → Configuration error
   - Empty host → Configuration error
   - Missing port uses default 443 → succeeds past validation (fails on network)
   - Invalid port (0, 65536, negative) → Configuration error
   - Invalid timeout (0, negative) → Configuration error
   - Invalid warningDaysBeforeExpiry (negative) → Configuration error

2. **Cancellation**:
   - Pre-cancelled token → Skipped

3. **Result properties**:
   - TestId, TestType, DisplayName populated correctly
   - StartTime, EndTime, Duration set
   - HumanSummary and TechnicalDetails non-empty on failure

## Complexity Tracking

No constitution violations to justify. The feature follows established patterns exactly.
