# Data Model: Traceroute Test Type

**Feature Branch**: `050-traceroute-test`
**Date**: 2026-02-22

## Entities

### Traceroute Test Parameters

Parameters extracted from `TestDefinition.Parameters` (JsonObject):

| Field     | Type   | Required | Default | Validation                          |
|-----------|--------|----------|---------|-------------------------------------|
| `host`    | string | Yes      | —       | Non-empty hostname or IP address    |
| `maxHops` | int    | No       | 30      | Must be positive integer (1–128)    |
| `timeout` | int    | No       | 5000    | Must be positive integer (milliseconds, per hop) |

### Traceroute Evidence

Evidence captured as `Dictionary<string, object>` and serialized to `TestEvidence.ResponseData`:

| Key             | Type            | Always Present | Description                                           |
|-----------------|-----------------|----------------|-------------------------------------------------------|
| `host`          | string          | Yes            | The original host parameter (hostname or IP)          |
| `resolvedIp`    | string          | Yes            | The resolved target IP address                        |
| `maxHops`       | int             | Yes            | The configured maximum hop count                      |
| `hopCount`      | int             | Yes            | Number of hops actually traced (including timeouts)   |
| `reachedTarget` | bool            | Yes            | Whether the trace reached the target host             |
| `hops`          | array of object | Yes            | Ordered list of hop entries (see Hop Entry below)     |

### Hop Entry

Each element in the `hops` array:

| Key          | Type   | Always Present | Description                                      |
|--------------|--------|----------------|--------------------------------------------------|
| `hop`        | int    | Yes            | Hop number (1-based)                             |
| `address`    | string | Yes            | Responding IP address, or `*` if timed out       |
| `roundtripMs`| int?   | Yes            | Round-trip time in ms, or null if timed out      |

## Relationships

- **TestDefinition → Traceroute Parameters**: One TestDefinition contains one set of parameters in its `Parameters` JsonObject.
- **TestResult → Traceroute Evidence**: One TestResult contains one Evidence payload serialized as JSON in `Evidence.ResponseData`.
- **TestResultDetailsConverter → Evidence Keys**: The converter reads evidence keys by name to render the `[Traceroute]` details section with `tracert`-style compact lines.

## State Transitions

The test execution follows a linear flow:

```
[Validate Parameters] → [Resolve DNS] → [Trace Hops (TTL 1..maxHops)] → [Evaluate Result]
        ↓ fail              ↓ fail              ↓ cancel                    ↓
   ConfigError          DnsError           Skipped (partial)        Pass/Fail
```

No persistent state — all data is in-memory per test execution.
