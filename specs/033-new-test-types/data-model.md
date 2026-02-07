# Data Model: 033-new-test-types

**Date**: 2026-02-07
**Branch**: `033-new-test-types`

## Overview

No new entities or model changes are required. The four new test types plug into the existing `TestDefinition` → `ITest` → `TestResult` / `TestEvidence` pipeline. Each test type defines its own parameter schema within the existing flexible `JsonObject Parameters` property.

## Parameter Schemas (per test type)

### DnsResolve (alias: DnsLookup)

| Parameter        | Type    | Required | Default | Description                              |
|------------------|---------|----------|---------|------------------------------------------|
| `hostname`       | string  | Yes      | —       | Hostname to resolve (e.g., `google.com`) |
| `expectedAddress`| string  | No       | null    | Expected IP address in results           |

**Evidence output** (serialized in `TestEvidence.ResponseData`):

| Field             | Type     | Description                                  |
|-------------------|----------|----------------------------------------------|
| `hostname`        | string   | The queried hostname                         |
| `addresses`       | string[] | All resolved IP addresses                    |
| `addressFamily`   | string   | `IPv4`, `IPv6`, or `Mixed`                   |
| `expectedAddress` | string?  | The expected address (if specified)           |
| `matchFound`      | bool     | Whether expected address was in results       |
| `resolutionTimeMs`| int      | DNS resolution time in milliseconds           |

---

### TcpPortOpen

| Parameter        | Type    | Required | Default | Description                                   |
|------------------|---------|----------|---------|-----------------------------------------------|
| `host`           | string  | Yes      | —       | Target hostname or IP address                 |
| `port`           | int     | Yes      | —       | Target TCP port number (1–65535)              |
| `connectTimeout` | int     | No       | 5000    | Connection timeout in milliseconds            |

**Evidence output**:

| Field              | Type   | Description                              |
|--------------------|--------|------------------------------------------|
| `host`             | string | Target host                              |
| `port`             | int    | Target port                              |
| `connected`        | bool   | Whether connection succeeded              |
| `remoteEndpoint`   | string | Resolved remote endpoint (IP:port)        |
| `connectTimeMs`    | int    | Connection time in milliseconds           |

---

### WindowsService

| Parameter        | Type    | Required | Default     | Description                                |
|------------------|---------|----------|-------------|--------------------------------------------|
| `serviceName`    | string  | Yes      | —           | Internal (short) service name              |
| `expectedStatus` | string  | No       | `Running`   | Expected service status                    |

**Valid `expectedStatus` values**: `Running`, `Stopped`, `Paused`, `StartPending`, `StopPending`

**Evidence output**:

| Field            | Type   | Description                              |
|------------------|--------|------------------------------------------|
| `serviceName`    | string | Internal service name                    |
| `displayName`    | string | Human-friendly display name              |
| `status`         | string | Current status (e.g., `Running`)         |
| `expectedStatus` | string | The expected status                      |
| `startType`      | string | Start type (e.g., `Automatic`, `Manual`) |
| `statusMatch`    | bool   | Whether current matches expected          |

---

### DiskSpace

| Parameter       | Type    | Required | Default | Description                                    |
|-----------------|---------|----------|---------|------------------------------------------------|
| `path`          | string  | Yes      | —       | Drive letter or mount path (e.g., `C:\`, `/`)  |
| `minimumFreeGB` | decimal | Yes      | —       | Minimum required free space in GB              |

**Evidence output**:

| Field            | Type    | Description                              |
|------------------|---------|------------------------------------------|
| `path`           | string  | The queried path                         |
| `totalSpaceGB`   | decimal | Total drive/volume capacity in GB         |
| `freeSpaceGB`    | decimal | Available free space in GB                |
| `percentFree`    | decimal | Free space as percentage (0–100)          |
| `minimumFreeGB`  | decimal | The required threshold                    |
| `thresholdMet`   | bool    | Whether free space >= minimum              |

## Existing Model Dependencies (no changes)

- **TestDefinition**: Unchanged — new test types use the existing `Type`, `Parameters`, `FieldPolicy`, and `DependsOn` fields.
- **TestResult**: Unchanged — new tests populate `Status`, `Evidence`, `Error`, `HumanSummary`.
- **TestEvidence**: Unchanged — new tests serialize to `ResponseData` (JSON string) and populate `Timing`.
- **TimingBreakdown**: Unchanged — new tests set `TotalMs` and optionally `ConnectMs` / `ExecuteMs`.
- **TestError**: Unchanged — new tests use existing `ErrorCategory` values (`Network`, `Timeout`, `Validation`, `Configuration`, `Permission`).
