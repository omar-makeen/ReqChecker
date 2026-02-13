# Data Model: Move mTLS Credentials to Test Configuration

**Feature**: 040-mtls-config-credentials
**Date**: 2026-02-13

## Entities

### TestDefinition.Parameters (modified)

**Change**: Add optional `pfxPassword` field for `MtlsConnect` test type.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| url | string | yes | Target HTTPS URL for mTLS connection |
| clientCertPath | string | yes | File path to PFX/P12 client certificate |
| pfxPassword | string | no | Passphrase for the PFX file (plaintext). If omitted or empty, certificate is loaded without a passphrase. |
| expectedStatus | integer | no | Expected HTTP status code (default: 200) |
| timeout | integer | no | Connection timeout in milliseconds |
| skipServerCertValidation | boolean | no | Whether to skip server certificate validation |
| credentialRef | string | no | (Deprecated) Legacy credential reference. Used only when `pfxPassword` is absent for backward compatibility. |

### TestDefinition.FieldPolicy (modified)

**Change**: Add `pfxPassword` as `Editable`. Remove or hide `credentialRef` in new profiles.

| Field | Policy | Description |
|-------|--------|-------------|
| pfxPassword | Editable | User can edit the PFX passphrase in config UI |
| credentialRef | Hidden or removed | Legacy field, hidden from UI in new profiles |

### TestExecutionContext (unchanged)

No structural changes. The runner creates a `TestExecutionContext` with `Password` set from `pfxPassword` parameter value instead of from the dialog prompt.

| Property | Type | Source (new flow) |
|----------|------|-------------------|
| Username | string? | Empty string (not used by mTLS) |
| Password | string? | Read from `pfxPassword` parameter |

## State Transitions

### Credential resolution order (per mTLS test execution)

```
1. Check: Does Parameters contain "pfxPassword"?
   ├─ YES → Create TestExecutionContext(password: pfxPassword) → Execute test
   └─ NO → Check: Does Parameters contain "credentialRef"?
           ├─ YES → Try ICredentialProvider → Try PromptForCredentials dialog → Execute test
           └─ NO → Execute test with null context (no passphrase)
```

## Profile JSON Example (new format)

```json
{
  "id": "test-013",
  "type": "MtlsConnect",
  "displayName": "mTLS — badssl.com (Happy Path)",
  "parameters": {
    "url": "https://client.badssl.com/",
    "clientCertPath": "C:\\temp\\badssl.com-client.p12",
    "pfxPassword": "badssl.com",
    "expectedStatus": 200,
    "timeout": 15000,
    "skipServerCertValidation": false
  },
  "fieldPolicy": {
    "url": "Editable",
    "clientCertPath": "Editable",
    "pfxPassword": "Editable",
    "expectedStatus": "Editable",
    "timeout": "Editable",
    "skipServerCertValidation": "Editable"
  }
}
```
