# Test Type Parameter Contracts: 033-new-test-types

These are the JSON parameter schemas for each new test type as they appear in profile JSON files.

## DnsResolve (alias: DnsLookup)

```json
{
  "type": "DnsResolve",
  "parameters": {
    "hostname": "www.google.com",
    "expectedAddress": "142.250.80.46"
  },
  "fieldPolicy": {
    "hostname": "Editable",
    "expectedAddress": "Editable"
  }
}
```

**Required**: `hostname`
**Optional**: `expectedAddress`

## TcpPortOpen

```json
{
  "type": "TcpPortOpen",
  "parameters": {
    "host": "sqlserver.corp.local",
    "port": 1433,
    "connectTimeout": 5000
  },
  "fieldPolicy": {
    "host": "Editable",
    "port": "Editable",
    "connectTimeout": "Editable"
  }
}
```

**Required**: `host`, `port`
**Optional**: `connectTimeout` (default: 5000 ms)

## WindowsService

```json
{
  "type": "WindowsService",
  "parameters": {
    "serviceName": "MSSQLSERVER",
    "expectedStatus": "Running"
  },
  "fieldPolicy": {
    "serviceName": "Editable",
    "expectedStatus": "Editable"
  }
}
```

**Required**: `serviceName`
**Optional**: `expectedStatus` (default: `Running`)

## DiskSpace

```json
{
  "type": "DiskSpace",
  "parameters": {
    "path": "C:\\",
    "minimumFreeGB": 10.0
  },
  "fieldPolicy": {
    "path": "Editable",
    "minimumFreeGB": "Editable"
  }
}
```

**Required**: `path`, `minimumFreeGB`
