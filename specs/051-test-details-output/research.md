# Research: Test Details Output for All Test Types

**Date**: 2026-02-23 | **Feature**: 051-test-details-output

## Research Task 1: Evidence Keys per Test Type

**Decision**: Use the exact evidence keys already emitted by each test implementation.

**Rationale**: All 10 test types already serialize evidence to `TestEvidence.ResponseData` as JSON. Tests using dictionary-based evidence emit camelCase keys; tests using POCO classes (UdpPortOpen, MtlsConnect, CertificateExpiry) emit PascalCase keys (default `System.Text.Json` serialization). No changes to test implementations are needed — only the converter needs to read these keys.

**Evidence Key Inventory**:

### Ping (PingTest.cs)
- `host` — target hostname
- `totalCount` — total ping attempts
- `successfulCount` — successful pings
- `failedCount` — failed pings
- `successRate` — percentage (e.g., "100%")
- `averageRoundtripTime` — average RTT string (e.g., "12ms")
- `pingResults` — JSON array of per-attempt results

### DnsResolve (DnsResolveTest.cs)
- `hostname` — queried hostname
- `addresses` — JSON array of resolved IP addresses
- `addressCount` — number of resolved addresses
- `addressFamily` — IPv4/IPv6
- `resolutionTimeMs` — DNS lookup time
- `expectedAddress` — expected address (if configured)
- `matchFound` — whether expected address was found

### TcpPortOpen (TcpPortOpenTest.cs)
- `host` — target host
- `port` — target port number
- `connected` — boolean, connection succeeded
- `connectTimeMs` — time to establish connection
- `remoteEndpoint` — resolved endpoint string

### UdpPortOpen (UdpPortOpenTest.cs)
- `Responded` — boolean, got a response
- `RoundTripTimeMs` — UDP round-trip time
- `PayloadSentBytes` — bytes sent
- `PayloadReceivedBytes` — bytes received
- `ResponseDataPreview` — first N bytes of response (if any)

### DiskSpace (DiskSpaceTest.cs)
- `path` — drive/path checked
- `totalSpaceGB` — total disk space
- `freeSpaceGB` — available free space
- `percentFree` — percentage free
- `minimumFreeGB` — configured threshold
- `thresholdMet` — boolean, meets minimum

### WindowsService (WindowsServiceTest.cs)
- `serviceName` — service name
- `displayName` — friendly display name
- `status` — current service status
- `expectedStatus` — expected status
- `startType` — service start type
- `statusMatch` — boolean, status matches expected

### MtlsConnect (MtlsConnectTest.cs)
- `Connected` — boolean, connection succeeded
- `HttpStatusCode` — HTTP status code
- `ResponseTimeMs` — response time
- `CertificateSubject` — certificate subject
- `CertificateIssuer` — certificate issuer
- `CertificateThumbprint` — certificate thumbprint
- `CertificateNotBefore` — validity start date
- `CertificateNotAfter` — validity end date
- `CertificateHasPrivateKey` — boolean, has private key
- `ServerCertValidationSkipped` — boolean, validation skipped

### CertificateExpiry (CertificateExpiryTest.cs)
- `Host` — target host
- `Port` — target port
- `Subject` — certificate subject
- `Issuer` — certificate issuer
- `Thumbprint` — certificate thumbprint
- `NotBefore` — validity start
- `NotAfter` — validity end / expiry date
- `DaysUntilExpiry` — days remaining
- `IsExpired` — boolean
- `IsNotYetValid` — boolean
- `ExpiresWithinWarningWindow` — boolean
- `SubjectAlternativeNames` — SAN list
- `TlsProtocolVersion` — TLS version

### FileExists (FileExistsTest.cs)
- `path` — file path
- `exists` — boolean, file exists
- `shouldExist` — expected existence
- `isPass` — boolean, test passed
- `size` — file size in bytes
- `lastModified` — last modification timestamp
- `creationTime` — creation timestamp
- `attributes` — file attributes string

### DirectoryExists (DirectoryExistsTest.cs)
- `path` — directory path
- `exists` — boolean, directory exists
- `shouldExist` — expected existence
- `isPass` — boolean, test passed
- `creationTime` — creation timestamp
- `lastModified` — last modification timestamp
- `attributes` — directory attributes string
- `fileCount` — number of files
- `directoryCount` — number of subdirectories

## Research Task 2: Detection Key Uniqueness

**Decision**: Each section uses a pair of evidence keys unique to that test type.

**Rationale**: Analyzed all 26 test types' evidence keys to confirm no collisions. Key observations:
- `successRate` + `pingResults` → only Ping
- `hostname` + `addresses` → only DnsResolve
- `host` + `port` + `connected` → only TcpPortOpen (MtlsConnect has `connected` but not `port`)
- `responded` + `payloadSentBytes` → only UdpPortOpen
- `totalSpaceGB` + `freeSpaceGB` → only DiskSpace
- `serviceName` + `expectedStatus` → only WindowsService
- `certificateSubject` + `certificateThumbprint` → only MtlsConnect (CertificateExpiry uses `subject` + `thumbprint`)
- `daysUntilExpiry` + `isExpired` → only CertificateExpiry
- `path` + `exists` + `size` → only FileExists (DirectoryExists has no `size`)
- `path` + `exists` + `directoryCount` → only DirectoryExists (FileExists has no `directoryCount`)

**Alternatives Considered**: Single key detection was rejected because keys like `host`, `path`, `connected` appear across multiple test types.

## Research Task 3: Existing Converter Patterns

**Decision**: Follow the exact same pattern used by existing dedicated sections (e.g., `[System RAM]`, `[WebSocket]`, `[Proxy]`, `[Traceroute]`).

**Rationale**: Consistency with existing code. Pattern is:
1. `if (evidenceData != null && evidenceData.ContainsKey("key1") && evidenceData.ContainsKey("key2"))`
2. `sections.Add("[SectionName]");`
3. For each field: `if (evidenceData.TryGetValue("key", out var obj) && obj != null)` → `sections.Add($"Label:  {obj}");`
4. Booleans: `obj.ToString() is "True" or "true" ? "yes" : "no"`
5. `sections.Add(string.Empty);` at end

**Alternatives Considered**: A table-driven approach with metadata arrays was considered but rejected — it would add abstraction complexity for a one-time operation with no reuse benefit.
