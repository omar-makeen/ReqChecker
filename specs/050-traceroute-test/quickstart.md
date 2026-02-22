# Quickstart: Traceroute Test Type

**Feature Branch**: `050-traceroute-test`
**Date**: 2026-02-22

## What This Feature Does

Adds a `Traceroute` test type that traces the network path to a target host by sending ICMP echo requests with incrementing TTL values. Each hop's responding IP address and round-trip time are recorded. Pass when the target is reached; Fail otherwise.

## Files to Create/Modify

| File | Action | Purpose |
|------|--------|---------|
| `src/ReqChecker.Infrastructure/Tests/TracerouteTest.cs` | Create | Test implementation |
| `src/ReqChecker.Infrastructure/TestManifest.props` | Modify | Register for conditional builds |
| `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs` | Modify | Add `[Traceroute]` evidence display |
| `src/ReqChecker.App/Profiles/default-profile.json` | Modify | Add sample Traceroute test |
| `src/ReqChecker.App/Profiles/sample-diagnostics.json` | Modify | Add sample Traceroute test |
| `README.md` | Modify | Update test count (25→26) and add docs |
| `CLAUDE.md` | Modify | Add 050-traceroute-test entry |

## Profile Configuration Example

```json
{
  "id": "traceroute-check",
  "type": "Traceroute",
  "displayName": "Trace Route to Gateway",
  "description": "Traces the network path to the gateway server.",
  "parameters": {
    "host": "gateway.example.com",
    "maxHops": 30,
    "timeout": 5000
  },
  "fieldPolicy": {
    "host": "Editable",
    "maxHops": "Editable",
    "timeout": "Editable"
  },
  "dependsOn": []
}
```

## Minimal Example (Defaults)

```json
{
  "id": "trace-dns",
  "type": "Traceroute",
  "displayName": "Trace to DNS",
  "description": "Traces the route to Google DNS.",
  "parameters": {
    "host": "8.8.8.8"
  },
  "fieldPolicy": {
    "host": "Editable"
  },
  "dependsOn": []
}
```

## Build Commands

```bash
# Build with all test types (includes Traceroute)
dotnet build

# Build with only Traceroute
dotnet build /p:IncludeTests="Traceroute"

# Build with Traceroute and other network tests
dotnet build /p:IncludeTests="Ping;Traceroute;HttpGet"
```

## Evidence Output

When viewing test results, the `[Traceroute]` section displays:

```
[Traceroute]
Host:       gateway.example.com
Resolved:   93.184.216.34
Hops:       8 / 30
Reached:    yes

  1     1ms  192.168.1.1
  2     5ms  10.0.0.1
  3       *  *
  4    12ms  172.16.0.1
  5    18ms  203.0.113.5
  6    25ms  198.51.100.2
  7    31ms  93.184.216.1
  8    34ms  93.184.216.34
```
