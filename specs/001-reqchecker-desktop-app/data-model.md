# Data Model: ReqChecker Desktop Application

**Branch**: `001-reqchecker-desktop-app` | **Date**: 2026-01-30

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              PROFILE                                     │
├─────────────────────────────────────────────────────────────────────────┤
│ + Id: string (GUID)                                                      │
│ + Name: string                                                           │
│ + SchemaVersion: int                                                     │
│ + Source: ProfileSource (Bundled | UserProvided)                         │
│ + RunSettings: RunSettings                                               │
│ + Tests: List<TestDefinition>                                            │
│ + Signature?: string (HMAC, bundled only)                                │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ 1:N
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           TEST_DEFINITION                                │
├─────────────────────────────────────────────────────────────────────────┤
│ + Id: string                                                             │
│ + Type: string (e.g., "HttpGet", "Ping")                                 │
│ + DisplayName: string                                                    │
│ + Description?: string                                                   │
│ + Parameters: Dictionary<string, JsonElement>                            │
│ + FieldPolicy: Dictionary<string, FieldPolicyType>                       │
│ + Timeout?: int (ms, overrides RunSettings)                              │
│ + RetryCount?: int (overrides RunSettings)                               │
│ + RequiresAdmin: bool                                                    │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ executes to
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                             RUN_REPORT                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ + RunId: string (GUID)                                                   │
│ + ProfileId: string                                                      │
│ + ProfileName: string                                                    │
│ + StartTime: DateTimeOffset                                              │
│ + EndTime: DateTimeOffset                                                │
│ + Duration: TimeSpan                                                     │
│ + MachineInfo: MachineInfo                                               │
│ + Results: List<TestResult>                                              │
│ + Summary: RunSummary                                                    │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ 1:N
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                            TEST_RESULT                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ + TestId: string                                                         │
│ + TestType: string                                                       │
│ + DisplayName: string                                                    │
│ + Status: TestStatus (Pass | Fail | Skipped)                             │
│ + StartTime: DateTimeOffset                                              │
│ + EndTime: DateTimeOffset                                                │
│ + Duration: TimeSpan                                                     │
│ + AttemptCount: int                                                      │
│ + Evidence: TestEvidence                                                 │
│ + Error?: TestError                                                      │
│ + HumanSummary: string                                                   │
│ + TechnicalDetails: string                                               │
└─────────────────────────────────────────────────────────────────────────┘
```

## Entity Definitions

### Profile

The root configuration entity loaded from JSON files.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | string (GUID) | Yes | Unique identifier for the profile |
| Name | string | Yes | Human-readable profile name for display |
| SchemaVersion | int | Yes | Schema version (current: 1) for migration |
| Source | ProfileSource | Runtime | Bundled (company-managed) or UserProvided |
| RunSettings | RunSettings | Yes | Global defaults for test execution |
| Tests | List\<TestDefinition\> | Yes | Ordered list of tests to execute |
| Signature | string | Bundled only | HMAC-SHA256 signature for integrity |

**Validation Rules**:
- Name: 1-100 characters, non-empty
- SchemaVersion: Must be ≤ current supported version
- Tests: At least 1 test required

---

### RunSettings

Global defaults for test execution, can be overridden per-test.

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| DefaultTimeout | int | No | 30000 | Default timeout in milliseconds |
| DefaultRetryCount | int | No | 3 | Default retry attempts |
| RetryBackoff | BackoffStrategy | No | Exponential | Retry delay strategy |
| AdminBehavior | AdminBehavior | No | SkipWithReason | How to handle admin-required tests |

**BackoffStrategy Enum**: `None`, `Linear`, `Exponential`
**AdminBehavior Enum**: `SkipWithReason`, `PromptForElevation`, `FailImmediately`

---

### TestDefinition

Defines a single test to be executed.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Id | string | Yes | Unique identifier within profile |
| Type | string | Yes | Test type identifier (e.g., "HttpGet") |
| DisplayName | string | Yes | User-facing name |
| Description | string | No | Optional help text |
| Parameters | Dictionary\<string, JsonElement\> | Yes | Type-specific parameters |
| FieldPolicy | Dictionary\<string, FieldPolicyType\> | No | Per-field editability rules |
| Timeout | int | No | Override default timeout (ms) |
| RetryCount | int | No | Override default retry count |
| RequiresAdmin | bool | No | Whether test needs elevation |

**FieldPolicyType Enum**:
| Value | UI Behavior |
|-------|-------------|
| Locked | Read-only, lock icon, tooltip |
| Editable | Normal input field |
| Hidden | Not rendered |
| PromptAtRun | Prompt dialog before execution |

**Field Policy Path Examples**:
- `url` → Top-level parameter
- `headers.Authorization` → Nested parameter
- `credentials.username` → Nested credential field

---

### Test Parameters by Type

#### HttpGet / HttpPost
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| url | string | Yes | Target URL |
| expectedStatusCodes | int[] | No | Valid status codes (default: [200]) |
| headers | Dictionary\<string, string\> | No | Request headers |
| body | string | HttpPost only | Request body |
| validateSsl | bool | No | Validate SSL certificate (default: true) |

#### Ping
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| host | string | Yes | Hostname or IP |
| timeout | int | No | Ping timeout (ms) |
| ttl | int | No | Time to live |

#### FtpRead / FtpWrite
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| host | string | Yes | FTP server |
| port | int | No | Port (default: 21) |
| remotePath | string | Yes | Remote file path |
| localPath | string | FtpWrite | Local file to upload |
| credentialRef | string | No | Credential Manager reference |
| useTls | bool | No | Use FTPS (default: true) |
| passiveMode | bool | No | Use passive mode (default: true) |

#### FileExists / DirectoryExists
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| path | string | Yes | Local file/directory path |

#### FileRead / FileWrite
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| path | string | Yes | Local file path |
| content | string | FileWrite | Content to write |

#### ProcessList
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| processNames | string[] | Yes | Process names to check |
| matchAll | bool | No | All must be running (default: false) |

#### RegistryRead
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| keyPath | string | Yes | Registry key path |
| valueName | string | No | Value name (null = check key exists) |
| expectedValue | string | No | Expected value (null = just check exists) |

---

### RunReport

The output of a test execution session.

| Field | Type | Description |
|-------|------|-------------|
| RunId | string (GUID) | Unique run identifier |
| ProfileId | string | Source profile ID |
| ProfileName | string | Source profile name |
| StartTime | DateTimeOffset | Run start timestamp |
| EndTime | DateTimeOffset | Run end timestamp |
| Duration | TimeSpan | Total execution time |
| MachineInfo | MachineInfo | Environment details |
| Results | List\<TestResult\> | Per-test results |
| Summary | RunSummary | Aggregate statistics |

---

### MachineInfo

Captured environment information.

| Field | Type | Description |
|-------|------|-------------|
| Hostname | string | Machine name |
| OsVersion | string | Windows version string |
| OsBuild | string | Build number |
| ProcessorCount | int | CPU core count |
| TotalMemoryMB | long | Total RAM |
| UserName | string | Running user (without domain) |
| IsElevated | bool | Running as admin |
| NetworkInterfaces | List\<NetworkInterfaceInfo\> | Active NICs |

---

### TestResult

Outcome of a single test execution.

| Field | Type | Description |
|-------|------|-------------|
| TestId | string | Test definition ID |
| TestType | string | Test type identifier |
| DisplayName | string | User-facing name |
| Status | TestStatus | Pass, Fail, or Skipped |
| StartTime | DateTimeOffset | Test start timestamp |
| EndTime | DateTimeOffset | Test end timestamp |
| Duration | TimeSpan | Execution time |
| AttemptCount | int | Number of attempts (with retries) |
| Evidence | TestEvidence | Captured data |
| Error | TestError | Error details if failed |
| HumanSummary | string | User-friendly result message |
| TechnicalDetails | string | Detailed technical output |

**TestStatus Enum**: `Pass`, `Fail`, `Skipped`

---

### TestEvidence

Data captured during test execution.

| Field | Type | Description |
|-------|------|-------------|
| ResponseData | string | Response body/output (truncated to 4KB) |
| ResponseCode | int? | HTTP status code if applicable |
| ResponseHeaders | Dictionary\<string, string\> | Response headers if applicable |
| FileContent | string | File content sample if applicable |
| ProcessList | string[] | Running processes if applicable |
| RegistryValue | string | Registry value if applicable |
| Timing | TimingBreakdown | Detailed timing |

---

### TestError

Error details for failed tests.

| Field | Type | Description |
|-------|------|-------------|
| Category | ErrorCategory | Error classification |
| Message | string | User-friendly error message |
| ExceptionType | string | .NET exception type name |
| StackTrace | string | Stack trace (debug builds) |
| InnerError | TestError | Nested exception if any |

**ErrorCategory Enum**:
| Value | Description |
|-------|-------------|
| Network | Connection, DNS, socket errors |
| Timeout | Operation timed out |
| Permission | Access denied, elevation required |
| Validation | Unexpected response, assertion failed |
| Configuration | Invalid parameters |
| Unknown | Unclassified error |

---

### CredentialRef

Reference to stored credentials (not the credentials themselves).

| Field | Type | Description |
|-------|------|-------------|
| RefId | string | Identifier used in Credential Manager |
| Target | string | Full target name: `ReqChecker:{RefId}` |

**Storage**: Windows Credential Manager (CRED_TYPE_GENERIC)
**Retrieval**: Username + Password pair

---

## State Transitions

### Profile Lifecycle

```
[JSON File] → Load → Validate Schema → Migrate (if needed) → Verify Integrity → [Loaded Profile]
                         ↓                                          ↓
                   [Validation Error]                        [Integrity Error]
```

### Test Execution Lifecycle

```
[Pending] → [Running] → [Pass]
              ↓    ↘
          [Retry]   [Fail]
              ↓
          [Running] → ... (up to RetryCount)
              ↓
          [Fail]

[Pending] → [Skipped] (RequiresAdmin without elevation)
```

### Run Lifecycle

```
[Created] → [Running] → [Completed]
               ↓
          [Cancelled]
```

---

## Indexes and Queries

### Profile Operations
- Load all bundled profiles by directory scan
- Load user profiles from AppData
- Get profile by ID
- Validate profile before execution

### Result Operations
- Get results by RunId
- Get last run report
- Filter results by status
- Export results to JSON/CSV

---

## Data Validation Summary

| Entity | Field | Rule |
|--------|-------|------|
| Profile | Name | 1-100 chars, non-empty |
| Profile | SchemaVersion | ≤ supported version |
| Profile | Tests | Count ≥ 1 |
| TestDefinition | Id | Non-empty, unique in profile |
| TestDefinition | Type | Must match registered test type |
| TestDefinition | DisplayName | Non-empty |
| RunSettings | DefaultTimeout | 1000-300000 ms |
| RunSettings | DefaultRetryCount | 0-10 |
