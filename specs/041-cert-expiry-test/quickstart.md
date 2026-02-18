# Quickstart: SSL Certificate Expiry Test

**Branch**: `041-cert-expiry-test` | **Date**: 2026-02-18

## Overview

Add a new `CertificateExpiry` test type to ReqChecker that connects to a remote TLS endpoint, retrieves the server certificate, and validates its expiry window.

## Prerequisites

- .NET 8.0 SDK
- No new NuGet packages required — uses existing `System.Net.Security` and `System.Security.Cryptography.X509Certificates`

## Files to Create

| File | Purpose |
|------|---------|
| `src/ReqChecker.Infrastructure/Tests/CertificateExpiryTest.cs` | Test implementation + evidence class + parameters class |
| `tests/ReqChecker.Infrastructure.Tests/Tests/CertificateExpiryTestTests.cs` | Unit tests for parameter validation, cancellation, result properties |

## Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs` | Add `"CertificateExpiry"` → icon mapping |
| `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs` | Add `"CertificateExpiry"` to network/security color group |
| `src/ReqChecker.App/Profiles/default-profile.json` | Add sample CertificateExpiry test entries |

## No Changes Required

- **DI registration**: Tests are auto-discovered from `ReqChecker.Infrastructure` assembly via `[TestType]` attribute reflection in `App.xaml.cs`
- **Test runner**: `SequentialTestRunner` dispatches to any registered `ITest` by type string — no changes needed
- **Core models**: Uses existing `TestDefinition`, `TestResult`, `TestEvidence`, `TestError`, `ErrorCategory`

## Build & Test

```bash
# Build
dotnet build src/ReqChecker.App/ReqChecker.App.csproj

# Run unit tests
dotnet test tests/ReqChecker.Infrastructure.Tests/

# Run app (manual smoke test)
dotnet run --project src/ReqChecker.App/
```

## Verification Checklist

1. Add a CertificateExpiry test in a profile pointing to `www.google.com:443` → should Pass
2. Add a CertificateExpiry test pointing to `expired.badssl.com:443` → should Fail with Validation error
3. Set `warningDaysBeforeExpiry` to 99999 for a valid cert → should Fail (expires within warning window)
4. Set `host` to a non-existent hostname → should Fail with Network error
5. Set `port` to 0 → should Fail with Configuration error
6. Cancel mid-execution → should show Skipped
7. Verify test icon and color render correctly in UI test list
