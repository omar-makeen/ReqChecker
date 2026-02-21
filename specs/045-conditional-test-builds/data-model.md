# Data Model: Conditional Test Builds

## Entities

### Test Manifest Entry

Defined in `TestManifest.props` as MSBuild items. Each entry maps a test type identifier to its source file.

| Field | Type | Description |
|-------|------|-------------|
| TestTypeId | string | Unique identifier (e.g., "Http", "Ping", "TcpPortOpen") |
| SourceFile | string | Relative path to the test class file (e.g., "Tests/HttpGetTest.cs") |

### Current Test Type Registry (22 types)

| TestTypeId | Source File | Class Name |
|------------|------------|------------|
| Ping | Tests/PingTest.cs | PingTest |
| HttpGet | Tests/HttpGetTest.cs | HttpGetTest |
| HttpPost | Tests/HttpPostTest.cs | HttpPostTest |
| DnsResolve | Tests/DnsResolveTest.cs | DnsResolveTest |
| TcpPortOpen | Tests/TcpPortOpenTest.cs | TcpPortOpenTest |
| UdpPortOpen | Tests/UdpPortOpenTest.cs | UdpPortOpenTest |
| MtlsConnect | Tests/MtlsConnectTest.cs | MtlsConnectTest |
| CertificateExpiry | Tests/CertificateExpiryTest.cs | CertificateExpiryTest |
| FileExists | Tests/FileExistsTest.cs | FileExistsTest |
| DirectoryExists | Tests/DirectoryExistsTest.cs | DirectoryExistsTest |
| FileRead | Tests/FileReadTest.cs | FileReadTest |
| FileWrite | Tests/FileWriteTest.cs | FileWriteTest |
| DiskSpace | Tests/DiskSpaceTest.cs | DiskSpaceTest |
| ProcessList | Tests/ProcessListTest.cs | ProcessListTest |
| RegistryRead | Tests/RegistryReadTest.cs | RegistryReadTest |
| WindowsService | Tests/WindowsServiceTest.cs | WindowsServiceTest |
| FtpRead | Tests/FtpReadTest.cs | FtpReadTest |
| FtpWrite | Tests/FtpWriteTest.cs | FtpWriteTest |
| OsVersion | Tests/OsVersionTest.cs | OsVersionTest |
| InstalledSoftware | Tests/InstalledSoftwareTest.cs | InstalledSoftwareTest |
| EnvironmentVariable | Tests/EnvironmentVariableTest.cs | EnvironmentVariableTest |

### Shared Infrastructure Files (always included)

| File | Purpose |
|------|---------|
| Tests/TestTypeAttribute.cs | Attribute for marking test implementations |

### Build Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| IncludeTests | semicolon-separated string | (empty = all) | Test type IDs to include |
| CustomerName | string | (empty) | Optional customer name for artifact naming |

## Relationships

- **TestDefinition.Type** (in profile JSON) → **TestTypeId** (in manifest): The profile's `type` field must match a manifest entry's TestTypeId for that test to appear in a filtered build.
- **[TestType("X")] attribute** (on test class) → **TestTypeId** (in manifest): The attribute value must match the manifest ID. This is enforced by convention (same string).
- **ITest implementation** (runtime) → **Compiled test classes**: Only test classes included by the manifest are discoverable via reflection at runtime.
