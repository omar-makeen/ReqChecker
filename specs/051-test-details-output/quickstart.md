# Quickstart: Test Details Output for All Test Types

**Date**: 2026-02-23 | **Feature**: 051-test-details-output

## Manual Test Scenarios

### Scenario 1: Network Tests (US1)

1. Load a profile with Ping, DnsResolve, TcpPortOpen, and UdpPortOpen tests
2. Run all four tests
3. Click on each result to view details
4. **Verify**:
   - Ping result shows `[Ping]` section with host, success count, rate, avg RTT
   - DnsResolve result shows `[DNS]` section with hostname, addresses listed individually, count, resolution time
   - TcpPortOpen result shows `[TCP]` section with host, port, connected status, connect time
   - UdpPortOpen result shows `[UDP]` section with response status, RTT, payload sizes

### Scenario 2: System & Security Tests (US2)

1. Load a profile with DiskSpace, WindowsService, MtlsConnect, and CertificateExpiry tests
2. Run all four tests
3. Click on each result to view details
4. **Verify**:
   - DiskSpace result shows `[Disk Space]` section with path, total/free space, percent, threshold
   - WindowsService result shows `[Service]` section with service name, status, expected status, start type
   - MtlsConnect result shows `[mTLS]` section with connection status, certificate details
   - CertificateExpiry result shows `[Certificate]` section with host, expiry date, days left, expired status

### Scenario 3: File System Tests (US3)

1. Load a profile with FileExists and DirectoryExists tests
2. Run both tests
3. Click on each result to view details
4. **Verify**:
   - FileExists result shows `[File]` section with path, exists status, size, last modified
   - DirectoryExists result shows `[Directory]` section with path, exists status, file/directory counts

### Scenario 4: Null/Missing Fields

1. Run a test where some evidence fields are null (e.g., UdpPortOpen with no response)
2. View the details
3. **Verify**: Missing fields are silently omitted — no "null" or empty lines appear

### Scenario 5: Existing Sections Unaffected (SC-004)

1. Run OsVersion, InstalledSoftware, EnvironmentVariable, SystemRam, CpuCores, WebSocket, Proxy, Traceroute tests
2. View each result's details
3. **Verify**: Output is identical to before this change — no regressions

### Scenario 6: Error Case

1. Run a test that fails with an exception (e.g., Ping to unreachable host with short timeout)
2. View the details
3. **Verify**: No dedicated section appears (evidenceData is null), only `[General]` and error info shown

## Build & Run

```bash
dotnet build src/ReqChecker.App/ReqChecker.App.csproj
dotnet run --project src/ReqChecker.App
```

Load `src/ReqChecker.App/Profiles/sample-diagnostics.json` for a profile containing most test types.
