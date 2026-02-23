# Research: Bandwidth Test Type

**Date**: 2026-02-23 | **Feature**: 052-bandwidth-test

## Research Task 1: Evidence Key Selection

**Decision**: Use dictionary-based evidence with camelCase keys, matching the pattern used by Traceroute, Proxy, and most other test types.

**Rationale**: The majority of test types (Ping, DNS, TCP, DiskSpace, WindowsService, Traceroute, Proxy) use `Dictionary<string, object>` with camelCase keys serialized via `JsonSerializer.Serialize()`. Only three test types (UDP, mTLS, CertificateExpiry) use POCO classes that result in PascalCase. Following the majority pattern keeps the codebase consistent.

**Evidence Keys**:
- `url` — target download URL
- `measuredMbps` — measured throughput in Mbps (double, 2 decimal places)
- `minimumMbps` — configured minimum threshold in Mbps (double)
- `bytesDownloaded` — total bytes downloaded (long)
- `elapsedSeconds` — actual elapsed time in seconds (double)
- `thresholdMet` — boolean, whether measured >= minimum

**Alternatives Considered**: POCO class with `JsonNamingPolicy.CamelCase` (used by WebSocket test) was rejected — unnecessary for a simple flat evidence structure with no nested objects.

## Research Task 2: Converter Detection Key Uniqueness

**Decision**: Use `measuredMbps` + `bytesDownloaded` as the unique detection pair for the `[Bandwidth]` section.

**Rationale**: Analyzed all 26 existing test types' evidence keys. No other test type emits both `measuredMbps` and `bytesDownloaded`. The `url` key alone would collide with other test types that also have a URL parameter.

**Alternatives Considered**: Using `url` + `measuredMbps` was considered but `measuredMbps` alone is already unique. Using the two-key pair adds safety against future test types.

## Research Task 3: HTTP Download Implementation Pattern

**Decision**: Use `HttpClient` with `HttpCompletionOption.ResponseHeadersRead` and stream the response body with a `CancellationTokenSource` timeout linked to `durationSeconds`.

**Rationale**: This is the standard .NET pattern for bounded-time downloads:
1. `HttpCompletionOption.ResponseHeadersRead` — starts reading the response body as a stream without buffering the entire response.
2. `CancellationTokenSource` with `TimeSpan.FromSeconds(durationSeconds)` — cancels the download after the duration.
3. Read in a loop with `stream.ReadAsync()` into a buffer, accumulating `bytesDownloaded`.
4. Calculate throughput from `bytesDownloaded` and actual elapsed `Stopwatch` time.

This matches the project's existing `HttpClient` usage in `ProxyConnectivityTest.cs` and does not require any new packages.

**Alternatives Considered**:
- `WebClient.DownloadDataAsync()` — deprecated in .NET 8, no streaming support.
- `HttpClient.GetByteArrayAsync()` — buffers entire response in memory; doesn't support duration cap.

## Research Task 4: Details Converter Section Layout

**Decision**: Use concise aligned labels following the project's standard converter pattern.

**Layout**:
```
[Bandwidth]
URL:        https://example.com/testfile.bin
Speed:      25.47 Mbps
Minimum:    10.00 Mbps
Downloaded: 31.84 MB
Duration:   10.02 s
Threshold:  met
```

**Rationale**: Matches the label-alignment pattern used by all existing converter sections (`[Proxy]`, `[Traceroute]`, `[Ping]`, etc.). The `Downloaded` field reuses the existing `FormatBytes()` helper already present in the converter. The `Threshold` field uses `met` / `not met` wording consistent with the `[Disk Space]` section's `Threshold: met` pattern.

**Alternatives Considered**: Verbose labels (e.g., `Measured Throughput:`) were rejected — they break the concise, aligned visual style used throughout the converter.

## Research Task 5: Build Manifest Integration

**Decision**: Add `Bandwidth` to `TestManifest.props` following the exact pattern of the 25 existing test types (26th being the new one → total becomes 27).

**Steps**:
1. Add `<KnownTestType Include="Bandwidth" SourceFile="Tests\BandwidthTest.cs" />` to the registry (Step 2 section).
2. Add conditional `<ItemGroup>` block for `Bandwidth` (Step 4 section).
3. Update the comment `26 test types` → `27 test types` in the manifest and README.

**Rationale**: The manifest's validation targets (`ValidateManifestSync`, `ValidateIncludeTests`) will fail the build if the file exists without a registry entry, so both must be added together.
