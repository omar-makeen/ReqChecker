# Data Model: Bandwidth Test Type

**Feature**: 052-bandwidth-test | **Date**: 2026-02-23

## Entities

### BandwidthTestParameters (internal to BandwidthTest)

Extracted and validated configuration from `TestDefinition.Parameters`.

| Field | Type | Required | Default | Validation |
|-------|------|----------|---------|------------|
| Url | string | Yes | — | Must not be empty; must start with `http://` or `https://` |
| MinimumMbps | double | No | 0.0 | Must be >= 0 |
| DurationSeconds | int | No | 10 | Must be > 0 |

### Bandwidth Evidence (Dictionary<string, object>)

Runtime data captured during test execution, serialized to `TestEvidence.ResponseData` as JSON.

| Key | Type | Description |
|-----|------|-------------|
| `url` | string | Target download URL |
| `measuredMbps` | double | Measured throughput: `(bytesDownloaded * 8) / (elapsedSeconds * 1_000_000)` |
| `minimumMbps` | double | Configured minimum threshold |
| `bytesDownloaded` | long | Total bytes successfully downloaded |
| `elapsedSeconds` | double | Actual wall-clock elapsed time in seconds |
| `thresholdMet` | bool | `true` if `measuredMbps >= minimumMbps` |

### Pass/Fail Logic

```
IF measuredMbps >= minimumMbps THEN Pass
ELSE Fail
```

Special case: if zero bytes are downloaded (connection dropped before any data), the test Fails with a connection error rather than reporting 0 Mbps.

## Relationships

- **BandwidthTest** implements `ITest` interface from `ReqChecker.Core.Interfaces`
- **BandwidthTest** reads from `TestDefinition.Parameters` (JSON profile)
- **BandwidthTest** writes to `TestResult.Evidence.ResponseData` (JSON string)
- **TestResultDetailsConverter** reads evidence keys `measuredMbps` + `bytesDownloaded` to detect and render `[Bandwidth]` section
- **TestResultDetailsConverter** reuses existing `FormatBytes()` helper for the `Downloaded` field

## State Transitions

```
[Not Started] → ExecuteAsync called
  → [Validating] → Parameter extraction & validation
    → Invalid: Fail with configuration error (ArgumentException)
    → Valid: proceed
  → [Downloading] → HTTP GET with streaming read, bounded by durationSeconds
    → Connection error: Fail with network error
    → HTTP error (4xx/5xx): Fail with HTTP status error
    → Duration elapsed OR file complete: stop reading
  → [Calculating] → Compute throughput from bytes and elapsed time
    → thresholdMet: Pass
    → !thresholdMet: Fail
```
