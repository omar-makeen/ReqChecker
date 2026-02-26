# Data Model: LdapBind Test Type

**Feature**: 057-ldap-bind-test | **Date**: 2026-02-26

## Entities

### LdapBind Test Parameters (Input — from profile JSON)

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `server` | string | Yes | — | LDAP server hostname or IP address |
| `port` | int | No | 389 (plain) / 636 (SSL) | LDAP server port (1–65535) |
| `useSsl` | bool | No | `false` | Whether to use implicit SSL/TLS (LDAPS) |
| `credentialRef` | string | No | — | Reference to stored username/password credentials |

**Validation rules**:
- `server` must not be null or empty
- `port` must be 1–65535 (inclusive) when provided
- Default port depends on `useSsl`: 389 when false, 636 when true

### LdapBind Evidence (Output — serialized to `TestEvidence.ResponseData`)

| Key | Type | Presence | Description |
|-----|------|----------|-------------|
| `server` | string | Always | Target server hostname/IP |
| `port` | int | Always | Target port number |
| `useSsl` | bool | Always | Whether SSL/TLS was requested |
| `bindType` | string | Always | `"anonymous"` or `"authenticated"` |
| `responseTimeMs` | long | Always | Total operation time in ms (n/a → `-1`) |
| `tlsNegotiated` | bool | Conditional | Only when `useSsl` is true |
| `tlsVersion` | string | Conditional | Only when TLS was negotiated (e.g., `"Tls12"`, `"Tls13"`) |
| `authenticated` | bool | Conditional | Only when `credentialRef` was provided |
| `warning` | string | Conditional | Only when credentials sent without TLS |

**Failure-path behavior**: All "always" fields are populated on every outcome. When a value cannot be determined (e.g., `responseTimeMs` on connection timeout before timing completes), the field is still present with a sentinel value (e.g., `-1` for time, rendered as `n/a` in the converter).

### Details View Layout (`[LdapBind]` section)

```
[LdapBind]
  Server:     dc.example.com
  Port:       636
  Bind:       authenticated
  Time:       85 ms
  TLS:        Tls12
  Auth:       yes
```

| Label | Source Key | Always/Conditional | Failure Value |
|-------|-----------|-------------------|---------------|
| `Server` | `server` | Always | Value always known |
| `Port` | `port` | Always | Value always known |
| `Bind` | `bindType` | Always | `n/a` if bind never attempted |
| `Time` | `responseTimeMs` | Always | `n/a` if < 0 |
| `TLS` | `tlsVersion` | Conditional (useSsl) | Omitted if useSsl is false |
| `Auth` | `authenticated` | Conditional (credentialRef) | Omitted if no credentialRef |

## Relationships

```
TestDefinition (profile JSON)
  └─ Parameters: { server, port, useSsl, credentialRef }
  └─ fieldPolicy: { server: Editable, port: Editable, useSsl: Editable, credentialRef: Editable }

TestExecutionContext (runtime, from SequentialTestRunner)
  └─ Username / Password (resolved from credentialRef)

TestResult (output)
  └─ Evidence.ResponseData → JSON { server, port, useSsl, bindType, responseTimeMs, ... }
  └─ Error → { Category, Message } (on failure)
```
