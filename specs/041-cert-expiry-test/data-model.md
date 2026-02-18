# Data Model: SSL Certificate Expiry Test

**Branch**: `041-cert-expiry-test` | **Date**: 2026-02-18

## Entities

### CertificateExpiryTestEvidence

Typed evidence class serialized to `TestEvidence.ResponseData` as JSON.

| Field                        | Type       | Nullable | Description                                                    |
|------------------------------|------------|----------|----------------------------------------------------------------|
| Host                         | string     | No       | Target hostname used for the TLS connection                    |
| Port                         | int        | No       | Target port used for the TLS connection                        |
| Subject                      | string     | Yes      | Certificate Subject DN (e.g., "CN=www.example.com, O=Example") |
| Issuer                       | string     | Yes      | Certificate Issuer DN                                          |
| Thumbprint                   | string     | Yes      | Certificate SHA-1 thumbprint (hex, uppercase)                  |
| NotBefore                    | DateTime   | Yes      | Certificate validity start date (UTC)                          |
| NotAfter                     | DateTime   | Yes      | Certificate validity end date (UTC)                            |
| DaysUntilExpiry              | int        | Yes      | Days from now until NotAfter (negative = already expired)      |
| IsExpired                    | bool       | No       | True if NotAfter < UtcNow                                      |
| IsNotYetValid                | bool       | No       | True if NotBefore > UtcNow                                     |
| ExpiresWithinWarningWindow   | bool       | No       | True if DaysUntilExpiry < warningDaysBeforeExpiry              |
| SubjectAlternativeNames      | string[]   | Yes      | DNS names from the SAN extension                               |
| TlsProtocolVersion           | string     | Yes      | Negotiated TLS version (e.g., "Tls13")                         |
| ResponseTimeMs               | int        | Yes      | TLS handshake time in milliseconds                             |
| ChainValidationSkipped       | bool       | No       | Whether chain validation was bypassed                          |
| ErrorDetail                  | string     | Yes      | Additional error or diagnostic information                     |

### CertificateExpiryParameters (internal)

Extracted and validated parameters from `TestDefinition.Parameters`.

| Field                    | Type   | Required | Default | Validation                                    |
|--------------------------|--------|----------|---------|-----------------------------------------------|
| Host                     | string | Yes      | —       | Non-empty                                     |
| Port                     | int    | No       | 443     | 1–65535                                       |
| WarningDaysBeforeExpiry  | int    | No       | 30      | >= 0                                          |
| Timeout                  | int    | No       | 10000   | > 0                                           |
| SkipChainValidation      | bool   | No       | false   | —                                             |
| ExpectedSubject          | string | No       | null    | Non-empty if provided                         |
| ExpectedIssuer           | string | No       | null    | Non-empty if provided                         |
| ExpectedThumbprint       | string | No       | null    | Non-empty if provided                         |

### Profile JSON Schema (test entry)

```json
{
  "id": "test-xxx",
  "type": "CertificateExpiry",
  "displayName": "Check TLS Certificate — example.com",
  "description": "Verifies the TLS certificate on example.com:443 is valid and does not expire within 30 days.",
  "parameters": {
    "host": "www.example.com",
    "port": 443,
    "warningDaysBeforeExpiry": 30,
    "timeout": 10000,
    "skipChainValidation": false,
    "expectedSubject": null,
    "expectedIssuer": null,
    "expectedThumbprint": null
  },
  "fieldPolicy": {
    "host": "Editable",
    "port": "Editable",
    "warningDaysBeforeExpiry": "Editable",
    "timeout": "Editable",
    "skipChainValidation": "Editable",
    "expectedSubject": "Editable",
    "expectedIssuer": "Editable",
    "expectedThumbprint": "Editable"
  },
  "timeout": null,
  "retryCount": null,
  "requiresAdmin": false,
  "dependsOn": []
}
```

## State Transitions

The test result follows the standard `TestStatus` flow:

```
[Start] → ExecuteAsync called
  ├── Parameter extraction fails → Fail (Configuration)
  ├── CancellationToken triggered → Skipped
  ├── TLS connection fails → Fail (Network)
  ├── TLS handshake times out → Fail (Timeout)
  ├── Certificate retrieved, evaluate:
  │   ├── Expired (NotAfter < UtcNow) → Fail (Validation)
  │   ├── Not yet valid (NotBefore > UtcNow) → Fail (Validation)
  │   ├── Expires within warning window → Fail (Validation)
  │   ├── expectedSubject set and no match in Subject DN or SANs → Fail (Validation)
  │   ├── expectedIssuer set and no match → Fail (Validation)
  │   ├── expectedThumbprint set and no match → Fail (Validation)
  │   └── All checks pass → Pass
```

## Relationships

- `CertificateExpiryTest` implements `ITest` interface
- Decorated with `[TestType("CertificateExpiry")]` for auto-discovery via DI
- Consumes `TestDefinition` (profile JSON model), produces `TestResult`
- `CertificateExpiryTestEvidence` serialized into `TestEvidence.ResponseData`
- No credentials required (`TestExecutionContext` is null)
