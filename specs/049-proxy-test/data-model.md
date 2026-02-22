# Data Model: ProxyConnectivity Test Type

**Feature Branch**: `049-proxy-test`
**Date**: 2026-02-22

## Entities

### ProxyConnectivity Test Parameters

Parameters extracted from `TestDefinition.Parameters` (JsonObject):

| Field            | Type   | Required | Default | Validation                                                        |
|------------------|--------|----------|---------|-------------------------------------------------------------------|
| `proxyUrl`       | string | Yes      | —       | Must start with `http://`, `https://`, `socks4://`, or `socks5://` |
| `testUrl`        | string | Yes      | —       | Must start with `http://` or `https://`                           |
| `proxyUsername`   | string | No       | null    | Free text; only used when provided                                |
| `proxyPassword`  | string | No       | null    | Free text; supports `PromptAtRun` field policy                    |
| `timeout`        | int    | No       | 30000   | Must be positive integer (milliseconds)                           |
| `expectedStatus` | int    | No       | null    | Valid HTTP status code (100-599); when null, any 2xx/3xx passes   |

### ProxyConnectivity Evidence

Evidence captured as `Dictionary<string, object>` and serialized to `TestEvidence.ResponseData`:

| Key              | Type   | Always Present | Description                                          |
|------------------|--------|----------------|------------------------------------------------------|
| `proxyUrl`       | string | Yes            | The proxy URL that was configured                     |
| `testUrl`        | string | Yes            | The target URL that was requested                     |
| `proxyType`      | string | Yes            | Inferred proxy type: `http`, `socks4`, or `socks5`   |
| `proxyReached`   | bool   | Yes            | Whether the proxy server was contacted                |
| `targetReached`  | bool   | Yes            | Whether the target URL responded through the proxy    |
| `connectTimeMs`  | long   | Yes            | Total elapsed time in milliseconds                    |
| `statusCode`     | int    | When target reached | HTTP status code from the target response        |
| `proxyUsername`   | string | When auth attempted | Username used for proxy auth (password redacted) |
| `authSucceeded`  | bool   | When auth attempted | Whether proxy authentication succeeded           |

## Relationships

- **TestDefinition → ProxyConnectivity Parameters**: One TestDefinition contains one set of parameters in its `Parameters` JsonObject.
- **TestResult → ProxyConnectivity Evidence**: One TestResult contains one Evidence payload serialized as JSON in `Evidence.ResponseData`.
- **TestResultDetailsConverter → Evidence Keys**: The converter reads evidence keys by name to render the `[Proxy]` details section.

## State Transitions

The test execution follows a linear flow:

```
[Validate Parameters] → [Configure Proxy Handler] → [Send Request Through Proxy] → [Evaluate Result]
        ↓ fail                    ↓ fail                      ↓ fail                    ↓ fail
   ConfigError              ConfigError                  NetworkError            StatusMismatch
```

No persistent state — all data is in-memory per test execution.
