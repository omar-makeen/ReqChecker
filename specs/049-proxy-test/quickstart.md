# Quickstart: ProxyConnectivity Test Type

**Feature Branch**: `049-proxy-test`
**Date**: 2026-02-22

## What This Feature Does

Adds a `ProxyConnectivity` test type that validates HTTP and SOCKS proxy reachability by connecting to a target URL through a specified proxy server. Supports HTTP, SOCKS4, and SOCKS5 proxies with optional authentication.

## Files to Create/Modify

| File | Action | Purpose |
|------|--------|---------|
| `src/ReqChecker.Infrastructure/Tests/ProxyConnectivityTest.cs` | Create | Test implementation |
| `src/ReqChecker.Infrastructure/TestManifest.props` | Modify | Register for conditional builds |
| `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs` | Modify | Add `[Proxy]` evidence display |
| `src/ReqChecker.App/Profiles/default-profile.json` | Modify | Add sample ProxyConnectivity test |
| `src/ReqChecker.App/Profiles/sample-diagnostics.json` | Modify | Add sample ProxyConnectivity test |
| `README.md` | Modify | Update test count (24→25) and add docs |

## Profile Configuration Example

```json
{
  "id": "proxy-check",
  "type": "ProxyConnectivity",
  "displayName": "Corporate Proxy Check",
  "description": "Validates connectivity through the corporate HTTP proxy.",
  "parameters": {
    "proxyUrl": "http://proxy.corp.com:8080",
    "testUrl": "https://www.example.com",
    "timeout": 30000,
    "expectedStatus": 200
  },
  "fieldPolicy": {
    "proxyUrl": "Editable",
    "testUrl": "Editable",
    "timeout": "Editable",
    "expectedStatus": "Editable"
  },
  "dependsOn": []
}
```

## Authenticated Proxy Example

```json
{
  "id": "proxy-auth-check",
  "type": "ProxyConnectivity",
  "displayName": "Authenticated Proxy Check",
  "parameters": {
    "proxyUrl": "http://proxy.corp.com:8080",
    "testUrl": "https://www.example.com",
    "proxyUsername": "serviceaccount",
    "proxyPassword": ""
  },
  "fieldPolicy": {
    "proxyUrl": "Locked",
    "testUrl": "Editable",
    "proxyUsername": "Locked",
    "proxyPassword": "PromptAtRun"
  },
  "dependsOn": []
}
```

## SOCKS5 Proxy Example

```json
{
  "id": "socks5-check",
  "type": "ProxyConnectivity",
  "displayName": "SOCKS5 Tunnel Check",
  "parameters": {
    "proxyUrl": "socks5://10.0.0.1:1080",
    "testUrl": "https://www.example.com"
  },
  "fieldPolicy": {
    "proxyUrl": "Editable",
    "testUrl": "Editable"
  },
  "dependsOn": []
}
```

## Build Commands

```bash
# Build with all test types (includes ProxyConnectivity)
dotnet build

# Build with only ProxyConnectivity
dotnet build /p:IncludeTests="ProxyConnectivity"

# Build with ProxyConnectivity and network tests
dotnet build /p:IncludeTests="Ping;HttpGet;ProxyConnectivity"
```

## Evidence Output

When viewing test results, the `[Proxy]` section displays:

```
[Proxy]
Proxy:      http://proxy.corp.com:8080
Target:     https://www.example.com
Type:       http
Connected:  yes
Status:     200 OK
Connect:    142 ms
```
