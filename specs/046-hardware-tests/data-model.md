# Data Model: SystemRam & CpuCores Hardware Tests

**Feature**: 046-hardware-tests | **Date**: 2026-02-21

## Entities

### SystemRam Test Parameters

| Field      | Type      | Required | Default | Description                                  |
|------------|-----------|----------|---------|----------------------------------------------|
| minimumGB  | decimal?  | No       | null    | Minimum required RAM in GB. Null = info mode. |

**Validation rules**:
- `minimumGB < 0` → Configuration error
- `minimumGB == 0` → Always pass (trivially met)
- `minimumGB == null` → Informational mode (always pass)

### SystemRam Test Evidence

| Field         | Type    | Description                                       |
|---------------|---------|---------------------------------------------------|
| detectedGB    | double  | Total physical RAM in GB (1 decimal precision)     |
| detectedBytes | ulong   | Total physical RAM in bytes (raw value)            |
| minimumGB     | double? | Configured threshold, null if informational        |
| thresholdMet  | bool?   | True/false if threshold set, null if informational |

### CpuCores Test Parameters

| Field        | Type  | Required | Default | Description                                       |
|--------------|-------|----------|---------|---------------------------------------------------|
| minimumCores | int?  | No       | null    | Minimum logical processor count. Null = info mode. |

**Validation rules**:
- `minimumCores < 0` → Configuration error
- `minimumCores == 0` → Always pass (trivially met)
- `minimumCores == null` → Informational mode (always pass)

### CpuCores Test Evidence

| Field         | Type  | Description                                       |
|---------------|-------|---------------------------------------------------|
| detectedCores | int   | Logical processor count                            |
| minimumCores  | int?  | Configured threshold, null if informational        |
| thresholdMet  | bool? | True/false if threshold set, null if informational |

## Profile Entry Schema

Both test types follow the standard profile test entry schema. No new fields are introduced.

### SystemRam Profile Entries (3 per profile)

**Informational** (no threshold — always pass):
```json
{
  "id": "test-028",
  "type": "SystemRam",
  "displayName": "Check System RAM",
  "description": "Reports the machine's total installed physical RAM.",
  "parameters": { "minimumGB": null },
  "fieldPolicy": { "minimumGB": "Editable" },
  "timeout": null, "retryCount": null, "requiresAdmin": false, "dependsOn": []
}
```

**Low threshold** (expected pass on most machines):
```json
{
  "id": "test-029",
  "type": "SystemRam",
  "displayName": "Check Minimum RAM (4 GB)",
  "description": "Verifies that the machine has at least 4 GB of RAM.",
  "parameters": { "minimumGB": 4 },
  "fieldPolicy": { "minimumGB": "Editable" },
  "timeout": null, "retryCount": null, "requiresAdmin": false, "dependsOn": []
}
```

**Unreachable threshold** (expected fail):
```json
{
  "id": "test-030",
  "type": "SystemRam",
  "displayName": "Check RAM 1024 GB (Expected Fail)",
  "description": "Tests the failure path by requiring 1024 GB of RAM. Expected to fail on all machines.",
  "parameters": { "minimumGB": 1024 },
  "fieldPolicy": { "minimumGB": "Editable" },
  "timeout": null, "retryCount": null, "requiresAdmin": false, "dependsOn": []
}
```

### CpuCores Profile Entries (3 per profile)

**Informational** (no threshold — always pass):
```json
{
  "id": "test-031",
  "type": "CpuCores",
  "displayName": "Check CPU Cores",
  "description": "Reports the machine's logical processor count.",
  "parameters": { "minimumCores": null },
  "fieldPolicy": { "minimumCores": "Editable" },
  "timeout": null, "retryCount": null, "requiresAdmin": false, "dependsOn": []
}
```

**Low threshold** (expected pass on most machines):
```json
{
  "id": "test-032",
  "type": "CpuCores",
  "displayName": "Check Minimum CPU Cores (2)",
  "description": "Verifies that the machine has at least 2 logical processors.",
  "parameters": { "minimumCores": 2 },
  "fieldPolicy": { "minimumCores": "Editable" },
  "timeout": null, "retryCount": null, "requiresAdmin": false, "dependsOn": []
}
```

**Unreachable threshold** (expected fail):
```json
{
  "id": "test-033",
  "type": "CpuCores",
  "displayName": "Check CPU Cores 256 (Expected Fail)",
  "description": "Tests the failure path by requiring 256 logical processors. Expected to fail on most machines.",
  "parameters": { "minimumCores": 256 },
  "fieldPolicy": { "minimumCores": "Editable" },
  "timeout": null, "retryCount": null, "requiresAdmin": false, "dependsOn": []
}
```
