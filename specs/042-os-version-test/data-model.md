# Data Model: OS Version Validation Test

**Branch**: `042-os-version-test` | **Date**: 2026-02-19

## Entities

### OsVersion Test Parameters

Configuration extracted from `TestDefinition.Parameters` (JsonObject).

| Field            | Type     | Required | Default | Description                                                            |
|------------------|----------|----------|---------|------------------------------------------------------------------------|
| minimumBuild     | int?     | No       | null    | Minimum Windows build number. Test passes if detected build >= value.  |
| expectedVersion  | string?  | No       | null    | Exact version string (format: "major.minor.build"). Test passes only on exact match. |

**Validation Rules**:
- `minimumBuild` and `expectedVersion` MUST NOT both be set → `ErrorCategory.Configuration`
- `minimumBuild` must be a positive integer if set → `ErrorCategory.Configuration`
- `expectedVersion` must match format `\d+\.\d+\.\d+` if set → `ErrorCategory.Configuration`
- Both null → informational mode (always Pass)

**State Transitions**: None. Parameters are immutable after profile load.

### OS Evidence

Captured during test execution and serialized to `TestEvidence.ResponseData` as JSON.

| Field         | Type   | Description                                          |
|---------------|--------|------------------------------------------------------|
| productName   | string | Human-friendly OS name (e.g., "Windows 11 Pro")      |
| version       | string | Full version string (e.g., "10.0.22631")             |
| buildNumber   | int    | Build component of the version (e.g., 22631)         |
| architecture  | string | Processor architecture (e.g., "X64", "Arm64")        |

### Relationships

```text
TestDefinition (existing)
  └── Parameters: JsonObject
        └── Parsed as: OsVersion Test Parameters

TestResult (existing)
  ├── Evidence: TestEvidence
  │     └── ResponseData: JSON string of OS Evidence
  ├── Error?: TestError
  │     └── Category: Configuration | Validation | Unknown
  ├── HumanSummary: string
  └── Status: Pass | Fail | Skipped
```

### Result Outcomes by Mode

| Mode           | Condition                        | Status | ErrorCategory | HumanSummary Example                                                |
|----------------|----------------------------------|--------|---------------|----------------------------------------------------------------------|
| Informational  | No constraints configured        | Pass   | —             | "Windows 11 Pro — version 10.0.22631 (X64)"                         |
| Minimum Build  | Detected build >= minimumBuild   | Pass   | —             | "OS build 22631 meets minimum requirement of 19045"                  |
| Minimum Build  | Detected build < minimumBuild    | Fail   | Validation    | "OS build 19045 does not meet minimum requirement of 22631"          |
| Exact Match    | Detected version == expected     | Pass   | —             | "OS version 10.0.22631 matches expected version"                     |
| Exact Match    | Detected version != expected     | Fail   | Validation    | "OS version 10.0.19045 does not match expected version 10.0.22631"   |
| Config Error   | Both params set                  | Fail   | Configuration | "Invalid configuration: specify either minimumBuild or expectedVersion, not both" |
| Config Error   | Invalid version format           | Fail   | Configuration | "Invalid expectedVersion format 'abc': expected major.minor.build"   |
| Cancelled      | CancellationToken triggered      | Skipped| Unknown       | "Test was cancelled"                                                 |
