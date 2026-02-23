# Quickstart: Bandwidth Test Type

**Feature**: 052-bandwidth-test | **Date**: 2026-02-23

## Profile Configuration

Add a Bandwidth test to any profile JSON:

```json
{
  "id": "bandwidth-check",
  "type": "Bandwidth",
  "displayName": "Download Speed Check",
  "parameters": {
    "url": "https://speed.cloudflare.com/__down?bytes=10000000",
    "minimumMbps": 10,
    "durationSeconds": 10
  }
}
```

### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| url | string | Yes | — | HTTP/HTTPS URL to download from |
| minimumMbps | number | No | 0 | Minimum acceptable throughput in Mbps |
| durationSeconds | int | No | 10 | Maximum download duration in seconds |

## Expected Output

### Pass Result
```
[Bandwidth]
URL:        https://speed.cloudflare.com/__down?bytes=10000000
Speed:      25.47 Mbps
Minimum:    10.00 Mbps
Downloaded: 31.84 MB
Duration:   10.02 s
Threshold:  met
```

### Fail Result (below threshold)
```
[Bandwidth]
URL:        https://example.com/testfile.bin
Speed:      4.82 Mbps
Minimum:    10.00 Mbps
Downloaded: 6.03 MB
Duration:   10.01 s
Threshold:  not met
```

## Files to Create/Modify

| Action | File | Purpose |
|--------|------|---------|
| Create | `src/ReqChecker.Infrastructure/Tests/BandwidthTest.cs` | Test implementation |
| Modify | `src/ReqChecker.Infrastructure/TestManifest.props` | Register in build manifest |
| Modify | `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs` | Add `[Bandwidth]` details section |
| Modify | `README.md` | Document test type and parameters |

## Build & Verify

```bash
# Full build (includes all test types)
dotnet build src/ReqChecker.App/ReqChecker.App.csproj

# Selective build (Bandwidth only)
dotnet build src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj /p:IncludeTests="Bandwidth"
```
