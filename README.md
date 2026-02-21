# ReqChecker

**ReqChecker** is a Windows desktop application for validating system readiness through configurable test profiles. It helps IT administrators, DevOps engineers, and QA teams verify that systems meet required specifications before software deployment or environment setup.

## Overview

ReqChecker runs automated tests across network connectivity, file systems, system configuration, security certificates, and hardware specifications. Tests are defined in JSON profiles that can be versioned, shared, and customized for different environments.

### Key Features

- **24 Built-in Test Types** — Network, file system, system, security, FTP, and hardware tests
- **JSON Profile Configuration** — Define test suites in version-controlled JSON files
- **Test Dependencies** — Chain tests with `dependsOn` to create sequential validation flows
- **Field Policies** — Control parameter visibility and editability (Locked, Editable, Hidden, PromptAtRun)
- **Retry Logic** — Configurable timeout, retry count, and backoff strategies
- **mTLS Support** — Client certificate authentication for secure endpoints
- **Modern WPF UI** — Clean, professional interface with light/dark theme support
- **Run History** — Track test results over time with detailed evidence capture

### Technology Stack

| Component | Technology |
|-----------|------------|
| Language | C# 12 |
| Framework | .NET 8 |
| UI Framework | WPF with WPF-UI library |
| Architecture | MVVM pattern |
| JSON Processing | System.Text.Json |
| FTP Client | FluentFTP |
| Build System | MSBuild with conditional compilation |

---

## Table of Contents

- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Build & Run](#build--run)
  - [Conditional Build](#conditional-build)
- [Application Pages](#application-pages)
- [Test Types](#test-types)
- [Test Type Reference](#test-type-reference)
  - [Network Tests](#network-tests)
  - [File System Tests](#file-system-tests)
  - [System Tests](#system-tests)
  - [Security & Certificate Tests](#security--certificate-tests)
  - [FTP Tests](#ftp-tests)
  - [Hardware Tests](#hardware-tests)
- [Profile Configuration](#profile-configuration)
  - [Profile Schema](#profile-schema)
  - [RunSettings](#runsettings)
  - [Test Definition Structure](#test-definition-structure)
  - [Field Policies](#field-policies)
  - [Test Dependencies](#test-dependencies)
  - [Minimal Profile Example](#minimal-profile-example)
- [Project Structure](#project-structure)
- [License](#license)

---

## Getting Started

### Prerequisites

- **Windows 10/11** (x64)
- **.NET 8 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** (optional) — For development and debugging

### Build & Run

```bash
# Clone the repository
git clone https://github.com/omar-makeen/ReqChecker.git
cd ReqChecker

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run --project src/ReqChecker.App
```

### Conditional Build

ReqChecker supports conditional compilation to include only specific test types in the build:

```bash
# Build with all 24 test types (default)
dotnet build

# Build with only specific test types
dotnet build /p:IncludeTests="Ping;HttpGet;DnsResolve"

# Build with a single test type
dotnet build /p:IncludeTests="Ping"
```

The `IncludeTests` parameter accepts a semicolon-delimited list of test type names. When omitted, all 24 test types are compiled. When specified, only the listed types are included. The build will fail if an unknown type name is provided. See the [Test Types](#test-types) table for valid type names.

---

## Application Pages

ReqChecker organizes functionality into six main navigation pages:

| Page | Description |
|------|-------------|
| **Profiles** | Load, create, and manage test profiles. Profiles define which tests to run and their parameters. |
| **Tests** | View and configure individual tests within the loaded profile. Edit parameters based on field policies. |
| **Results** | View detailed results of the last test run, including pass/fail status, timing, and evidence. |
| **History** | Browse historical test runs. Compare results over time to identify trends or regressions. |
| **Diagnostics** | System information and diagnostic tools. View environment details relevant to test execution. |
| **Settings** | Application preferences including theme, default paths, and behavior options. |

---

## Test Types

ReqChecker includes 24 built-in test types organized into 6 categories:

| Category | Type | Description |
|----------|------|-------------|
| Network | Ping | ICMP echo to verify host reachability |
| Network | HttpGet | HTTP GET request with status code validation |
| Network | HttpPost | HTTP POST request with body and status validation |
| Network | DnsResolve | DNS hostname resolution with optional address match |
| Network | TcpPortOpen | TCP port connectivity check |
| Network | UdpPortOpen | UDP port reachability with optional response validation |
| Network | WebSocket | WebSocket handshake and optional message exchange |
| File System | FileExists | Verify a file exists (or does not exist) at a path |
| File System | DirectoryExists | Verify a directory exists (or does not exist) at a path |
| File System | FileRead | Read file content with optional content matching |
| File System | FileWrite | Write content to a file to test write permissions |
| File System | DiskSpace | Verify minimum free disk space on a drive |
| System | ProcessList | Check if specific processes are running |
| System | RegistryRead | Read and validate Windows registry values |
| System | WindowsService | Check Windows service status (Running, Stopped, etc.) |
| System | OsVersion | Validate Windows version and build number |
| System | InstalledSoftware | Detect installed software via registry with version check |
| System | EnvironmentVariable | Verify environment variable existence and value |
| Security | MtlsConnect | Mutual TLS client certificate authentication test |
| Security | CertificateExpiry | SSL/TLS certificate validity and expiry check |
| FTP | FtpRead | Read a file from an FTP server |
| FTP | FtpWrite | Write a file to an FTP server |
| Hardware | SystemRam | Detect total physical RAM with optional minimum threshold |
| Hardware | CpuCores | Detect logical processor count with optional minimum threshold |

---

## Test Type Reference

### Network Tests

#### Ping

Tests network connectivity to a host using ICMP ping.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| host | string | Yes | — | Hostname or IP address to ping |
| timeout | int | No | 5000 | Timeout per ping in milliseconds |
| count | int | No | 4 | Number of ping attempts |

**Example:**
```json
{
  "type": "Ping",
  "parameters": {
    "host": "8.8.8.8",
    "timeout": 5000,
    "count": 4
  }
}
```

---

#### HttpGet

Tests HTTP GET endpoint connectivity and response.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| url | string | Yes | — | Full URL to request |
| timeout | int | No | 30000 | Request timeout in milliseconds |
| expectedStatus | int | No | — | Expected HTTP status code |
| includeHeaders | bool | No | false | Include response headers in evidence |
| includeBody | bool | No | true | Include response body in evidence |
| maxBodyLength | int | No | 10240 | Maximum body length to capture |
| headers | array | No | [] | Custom headers as `{ "name": "...", "value": "..." }` |

**Example:**
```json
{
  "type": "HttpGet",
  "parameters": {
    "url": "https://www.example.com",
    "expectedStatus": 200,
    "timeout": 30000,
    "headers": [
      { "name": "Authorization", "value": "Bearer token123" }
    ]
  }
}
```

---

#### HttpPost

Tests HTTP POST endpoint connectivity with request body.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| url | string | Yes | — | Full URL to request |
| timeout | int | No | 30000 | Request timeout in milliseconds |
| expectedStatus | int | No | — | Expected HTTP status code |
| body | string | No | "" | Request body content |
| contentType | string | No | application/json | Content-Type header value |
| includeHeaders | bool | No | false | Include response headers in evidence |
| includeBody | bool | No | true | Include response body in evidence |
| maxBodyLength | int | No | 10240 | Maximum body length to capture |
| headers | array | No | [] | Custom headers |

**Example:**
```json
{
  "type": "HttpPost",
  "parameters": {
    "url": "https://api.example.com/data",
    "body": "{\"key\": \"value\"}",
    "contentType": "application/json",
    "expectedStatus": 201
  }
}
```

---

#### DnsResolve

Tests DNS resolution for a hostname and optionally validates the resolved address.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| hostname | string | Yes | — | Hostname to resolve |
| expectedAddress | string | No | — | Expected IP address to validate |

**Example:**
```json
{
  "type": "DnsResolve",
  "parameters": {
    "hostname": "www.google.com",
    "expectedAddress": "142.250.185.68"
  }
}
```

---

#### TcpPortOpen

Tests TCP port connectivity by attempting to establish a connection.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| host | string | Yes | — | Hostname or IP address |
| port | int | Yes | — | Port number (1-65535) |
| connectTimeout | int | No | 5000 | Connection timeout in milliseconds |

**Example:**
```json
{
  "type": "TcpPortOpen",
  "parameters": {
    "host": "www.example.com",
    "port": 443,
    "connectTimeout": 5000
  }
}
```

---

#### UdpPortOpen

Tests UDP port reachability by sending a datagram and waiting for a response.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| host | string | Yes | — | Hostname or IP address |
| port | int | Yes | — | Port number (1-65535) |
| timeout | int | No | 5000 | Response timeout in milliseconds |
| payload | string | No | "0x00" | Payload to send (hex, base64, or UTF-8) |
| encoding | string | No | auto | Payload encoding: `hex`, `base64`, `utf8`, `auto` |
| expectedResponse | string | No | — | Expected response pattern for validation |

**Example:**
```json
{
  "type": "UdpPortOpen",
  "parameters": {
    "host": "8.8.8.8",
    "port": 53,
    "timeout": 5000,
    "payload": "00010100000100000000000003777777076578616d706c6503636f6d0000010001",
    "encoding": "hex"
  }
}
```

---

#### WebSocket

Tests WebSocket connectivity by performing a handshake and optional message exchange.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| url | string | Yes | — | WebSocket URL (must start with `ws://` or `wss://`) |
| timeout | int | No | 10000 | Timeout in milliseconds for connection and message exchange |
| message | string | No | — | Message to send after connection (triggers message exchange) |
| expectedResponse | string | No | — | Expected response for exact match validation |
| headers | array | No | [] | Custom headers as `[{ "name": "...", "value": "..." }]` |
| subprotocol | string | No | — | WebSocket subprotocol to negotiate |

**Example (handshake only):**
```json
{
  "type": "WebSocket",
  "parameters": {
    "url": "wss://echo.websocket.events",
    "timeout": 10000
  }
}
```

**Example (echo test with validation):**
```json
{
  "type": "WebSocket",
  "parameters": {
    "url": "wss://echo.websocket.events",
    "timeout": 10000,
    "message": "hello",
    "expectedResponse": "hello"
  }
}
```

---

### File System Tests

#### FileExists

Tests if a file exists at the specified path.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| path | string | Yes | — | Full file path |
| shouldExist | bool | No | true | Whether the file should exist |

**Example:**
```json
{
  "type": "FileExists",
  "parameters": {
    "path": "C:\\Windows\\System32\\drivers\\etc\\hosts",
    "shouldExist": true
  }
}
```

---

#### DirectoryExists

Tests if a directory exists at the specified path.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| path | string | Yes | — | Full directory path |
| shouldExist | bool | No | true | Whether the directory should exist |

**Example:**
```json
{
  "type": "DirectoryExists",
  "parameters": {
    "path": "C:\\Program Files",
    "shouldExist": true
  }
}
```

---

#### FileRead

Tests reading content from a file.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| path | string | Yes | — | Full file path |
| encoding | string | No | utf-8 | Text encoding |
| maxLength | int | No | 10240 | Maximum content length to capture |
| expectedContent | string | No | — | Content to search for in file |

**Example:**
```json
{
  "type": "FileRead",
  "parameters": {
    "path": "C:\\config\\settings.json",
    "encoding": "utf-8",
    "expectedContent": "\"enabled\": true"
  }
}
```

---

#### FileWrite

Tests writing content to a file.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| path | string | Yes | — | Full file path |
| content | string | No | "" | Content to write |
| encoding | string | No | utf-8 | Text encoding |
| createDirectory | bool | No | false | Create parent directory if needed |
| overwrite | bool | No | false | Overwrite existing file |

**Example:**
```json
{
  "type": "FileWrite",
  "parameters": {
    "path": "C:\\temp\\test.txt",
    "content": "Hello, World!",
    "createDirectory": true,
    "overwrite": true
  }
}
```

---

#### DiskSpace

Tests if a drive has at least a specified amount of free space.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| path | string | Yes | — | Drive path (e.g., `C:\`) |
| minimumFreeGB | decimal | Yes | — | Minimum free space in GB |

**Example:**
```json
{
  "type": "DiskSpace",
  "parameters": {
    "path": "C:\\",
    "minimumFreeGB": 10.0
  }
}
```

---

### System Tests

#### ProcessList

Tests if specific processes are running.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| processNames | array | Yes | — | List of process names to check |
| requireAll | bool | No | true | Require all processes to be running |
| includeDetails | bool | No | false | Include process details in evidence |

**Example:**
```json
{
  "type": "ProcessList",
  "parameters": {
    "processNames": ["explorer", "svchost"],
    "requireAll": false,
    "includeDetails": true
  }
}
```

---

#### RegistryRead

Tests reading values from the Windows registry.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| hive | string | No | LocalMachine | Registry hive: `ClassesRoot`, `CurrentConfig`, `CurrentUser`, `LocalMachine`, `Users` |
| subKeyPath | string | Yes | — | Registry key path |
| valueName | string | No | — | Value name (null for default) |
| expectedValue | string | No | — | Expected value content |
| expectedType | string | No | — | Expected value type: `String`, `DWord`, `QWord`, `MultiString`, `Binary`, `ExpandString` |

**Example:**
```json
{
  "type": "RegistryRead",
  "parameters": {
    "hive": "LocalMachine",
    "subKeyPath": "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion",
    "valueName": "ProductName",
    "expectedValue": "Windows 10 Pro"
  }
}
```

---

#### WindowsService

Tests Windows service status.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| serviceName | string | Yes | — | Service name (not display name) |
| expectedStatus | string | No | Running | Expected status: `Running`, `Stopped`, `Paused`, etc. |

**Example:**
```json
{
  "type": "WindowsService",
  "parameters": {
    "serviceName": "wuauserv",
    "expectedStatus": "Running"
  }
}
```

---

#### OsVersion

Validates Windows version and build number.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| minimumBuild | int | No | — | Minimum build number required |
| expectedVersion | string | No | — | Exact version match (`major.minor.build`) |

**Note:** Specify either `minimumBuild` or `expectedVersion`, not both.

**Example:**
```json
{
  "type": "OsVersion",
  "parameters": {
    "minimumBuild": 19045
  }
}
```

---

#### InstalledSoftware

Checks if software is installed via Windows registry.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| softwareName | string | Yes | — | Software name to search for (partial match) |
| minimumVersion | string | No | — | Minimum version required |

**Example:**
```json
{
  "type": "InstalledSoftware",
  "parameters": {
    "softwareName": "Microsoft Edge",
    "minimumVersion": "100.0"
  }
}
```

---

#### EnvironmentVariable

Verifies environment variable existence and value.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| variableName | string | Yes | — | Environment variable name |
| expectedValue | string | No | — | Expected value for comparison |
| matchType | string | No | existence | Match type: `existence`, `exact`, `contains`, `regex`, `pathContains` |

**Example:**
```json
{
  "type": "EnvironmentVariable",
  "parameters": {
    "variableName": "PATH",
    "expectedValue": "C:\\Windows\\System32",
    "matchType": "pathContains"
  }
}
```

---

### Security & Certificate Tests

#### MtlsConnect

Tests mutual TLS (client certificate) authentication.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| url | string | Yes | — | HTTPS URL (must use https://) |
| clientCertPath | string | Yes | — | Path to PFX/PKCS#12 client certificate |
| pfxPassword | string | No | — | Certificate password (can use PromptAtRun) |
| expectedStatus | int | No | 200 | Expected HTTP status code |
| timeout | int | No | 30000 | Request timeout in milliseconds |
| skipServerCertValidation | bool | No | false | Skip server certificate validation |

**Example:**
```json
{
  "type": "MtlsConnect",
  "parameters": {
    "url": "https://client.badssl.com/",
    "clientCertPath": "C:\\certs\\client.p12",
    "pfxPassword": "badssl.com",
    "expectedStatus": 200
  }
}
```

---

#### CertificateExpiry

Tests SSL/TLS certificate validity on a remote endpoint.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| host | string | Yes | — | Hostname to check |
| port | int | No | 443 | Port number |
| warningDaysBeforeExpiry | int | No | 30 | Days before expiry to trigger warning |
| timeout | int | No | 10000 | Connection timeout in milliseconds |
| skipChainValidation | bool | No | false | Skip certificate chain validation |
| expectedSubject | string | No | — | Expected subject/SAN pattern |
| expectedIssuer | string | No | — | Expected issuer pattern |
| expectedThumbprint | string | No | — | Expected SHA-1 thumbprint |

**Example:**
```json
{
  "type": "CertificateExpiry",
  "parameters": {
    "host": "www.google.com",
    "port": 443,
    "warningDaysBeforeExpiry": 30
  }
}
```

---

### FTP Tests

#### FtpRead

Tests reading a file from an FTP server.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| host | string | Yes | — | FTP server hostname |
| port | int | No | 21 | FTP port |
| username | string | No | anonymous | FTP username (can use PromptAtRun) |
| password | string | No | anonymous@ | FTP password (can use PromptAtRun) |
| remotePath | string | Yes | — | Remote file path |
| useSsl | bool | No | false | Use FTPS (explicit SSL/TLS) |
| validateCertificate | bool | No | true | Validate server certificate |
| maxLength | int | No | 10240 | Maximum content length to capture |

**Example:**
```json
{
  "type": "FtpRead",
  "parameters": {
    "host": "ftp.example.com",
    "port": 21,
    "username": "user",
    "password": "pass",
    "remotePath": "/data/file.txt",
    "useSsl": true
  }
}
```

---

#### FtpWrite

Tests writing a file to an FTP server.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| host | string | Yes | — | FTP server hostname |
| port | int | No | 21 | FTP port |
| username | string | No | anonymous | FTP username |
| password | string | No | anonymous@ | FTP password |
| remotePath | string | Yes | — | Remote file path |
| content | string | No | "" | Content to write |
| encoding | string | No | utf-8 | Text encoding |
| useSsl | bool | No | false | Use FTPS |
| validateCertificate | bool | No | true | Validate server certificate |
| createDirectory | bool | No | false | Create parent directory if needed |
| overwrite | bool | No | false | Overwrite existing file |

**Example:**
```json
{
  "type": "FtpWrite",
  "parameters": {
    "host": "ftp.example.com",
    "remotePath": "/uploads/test.txt",
    "content": "Test content",
    "createDirectory": true
  }
}
```

---

### Hardware Tests

#### SystemRam

Tests system RAM by detecting total physical memory.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| minimumGB | double | No | — | Minimum RAM required in GB |

**Example:**
```json
{
  "type": "SystemRam",
  "parameters": {
    "minimumGB": 8.0
  }
}
```

---

#### CpuCores

Tests CPU cores by detecting logical processor count.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| minimumCores | int | No | — | Minimum logical processors required |

**Example:**
```json
{
  "type": "CpuCores",
  "parameters": {
    "minimumCores": 4
  }
}
```

---

## Profile Configuration

Profiles are JSON files that define test suites. They contain metadata, global settings, and a list of test definitions.

### Profile Schema

```json
{
  "id": "unique-profile-id",
  "name": "Profile Display Name",
  "schemaVersion": 3,
  "source": "Bundled",
  "runSettings": { ... },
  "tests": [ ... ],
  "signature": null
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | string | Yes | Unique profile identifier (GUID recommended) |
| name | string | Yes | Human-readable profile name |
| schemaVersion | int | Yes | Schema version (current: 3) |
| source | string | No | Profile source: `Bundled` or `UserProvided` |
| runSettings | object | No | Global execution settings |
| tests | array | Yes | List of test definitions |
| signature | string | No | HMAC-SHA256 signature for bundled profiles |

### RunSettings

Global defaults that can be overridden per-test:

```json
{
  "runSettings": {
    "defaultTimeout": 30000,
    "defaultRetryCount": 3,
    "retryBackoff": "Exponential",
    "retryDelayMs": 5000,
    "adminBehavior": "SkipWithReason",
    "interTestDelayMs": 500
  }
}
```

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| defaultTimeout | int | 30000 | Default test timeout in milliseconds |
| defaultRetryCount | int | 0 | Default retry attempts |
| retryBackoff | string | None | Backoff strategy: `None`, `Linear`, `Exponential` |
| retryDelayMs | int | 5000 | Base delay between retries |
| adminBehavior | string | SkipWithReason | Admin handling: `SkipWithReason`, `PromptForElevation`, `FailImmediately` |
| interTestDelayMs | int | 500 | Delay between tests |

### Test Definition Structure

```json
{
  "id": "test-001",
  "type": "Ping",
  "displayName": "Check Network Connectivity",
  "description": "Pings a host to verify connectivity.",
  "parameters": {
    "host": "8.8.8.8",
    "timeout": 5000
  },
  "fieldPolicy": {
    "host": "Editable",
    "timeout": "Hidden"
  },
  "timeout": null,
  "retryCount": null,
  "requiresAdmin": false,
  "dependsOn": []
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | string | Yes | Unique test identifier within profile |
| type | string | Yes | Test type (e.g., `Ping`, `HttpGet`) |
| displayName | string | Yes | User-facing test name |
| description | string | No | Help text for the test |
| parameters | object | Yes | Type-specific test parameters |
| fieldPolicy | object | No | Per-field editability rules |
| timeout | int | No | Override default timeout (ms) |
| retryCount | int | No | Override default retry count |
| requiresAdmin | bool | No | Whether elevation is needed |
| dependsOn | array | No | IDs of prerequisite tests |

### Field Policies

Control how parameters appear in the UI:

| Policy | Behavior |
|--------|----------|
| `Locked` | Read-only with lock icon |
| `Editable` | Normal editable input field |
| `Hidden` | Not rendered in UI |
| `PromptAtRun` | Prompts user for input before execution |

**Example:**
```json
{
  "fieldPolicy": {
    "host": "Editable",
    "password": "PromptAtRun",
    "internalId": "Hidden"
  }
}
```

### Test Dependencies

Use `dependsOn` to create sequential test flows. A test will only run after all its dependencies complete successfully.

```json
{
  "id": "test-002",
  "type": "HttpGet",
  "dependsOn": ["test-001"],
  ...
}
```

### Minimal Profile Example

```json
{
  "id": "my-profile",
  "name": "My Test Profile",
  "schemaVersion": 3,
  "runSettings": {
    "defaultTimeout": 30000
  },
  "tests": [
    {
      "id": "ping-test",
      "type": "Ping",
      "displayName": "Internet Connectivity",
      "parameters": {
        "host": "8.8.8.8"
      },
      "fieldPolicy": {
        "host": "Editable"
      },
      "dependsOn": []
    },
    {
      "id": "http-test",
      "type": "HttpGet",
      "displayName": "Web Server Check",
      "parameters": {
        "url": "https://www.example.com",
        "expectedStatus": 200
      },
      "fieldPolicy": {
        "url": "Editable",
        "expectedStatus": "Editable"
      },
      "dependsOn": ["ping-test"]
    }
  ]
}
```

---

## Project Structure

```
ReqChecker/
├── src/
│   ├── ReqChecker.App/           # WPF application (views, view models, App.xaml)
│   │   ├── Profiles/             # Bundled JSON profiles
│   │   ├── Views/                # XAML views
│   │   └── ViewModels/           # MVVM view models
│   ├── ReqChecker.Core/          # Core business logic (models, interfaces, enums)
│   │   ├── Models/               # Profile, TestDefinition, RunSettings, etc.
│   │   ├── Enums/                # TestStatus, FieldPolicyType, etc.
│   │   └── Interfaces/           # ITest interface
│   └── ReqChecker.Infrastructure/ # Test implementations and services
│       └── Tests/                # Individual test classes (*Test.cs)
├── specs/                        # Feature specifications
├── ReqChecker.sln                # Visual Studio solution
├── Directory.Build.props         # MSBuild configuration
└── global.json                   # .NET SDK version
```

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
