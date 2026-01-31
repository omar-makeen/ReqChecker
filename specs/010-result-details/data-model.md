# Data Model: Display Test Result Details

**Feature**: 010-result-details | **Date**: 2026-01-31

## Entities

### TestResult (Existing - No Changes)

The core entity containing test outcome data. Already has all needed fields.

| Field | Type | Description |
|-------|------|-------------|
| TestId | string | Test definition ID |
| TestType | string | Test type identifier (HttpGet, Ping, etc.) |
| DisplayName | string | User-facing test name |
| Status | TestStatus | Pass, Fail, or Skipped |
| StartTime | DateTimeOffset | Test start timestamp |
| EndTime | DateTimeOffset | Test end timestamp |
| Duration | TimeSpan | Execution time |
| AttemptCount | int | Number of retry attempts |
| Evidence | TestEvidence | Captured data |
| Error | TestError? | Error details if failed |
| HumanSummary | string | User-friendly result message (empty - generated at display) |
| TechnicalDetails | string | Detailed technical output (empty - generated at display) |

### TestEvidence (Existing - No Changes)

Data captured during test execution.

| Field | Type | Description |
|-------|------|-------------|
| ResponseData | string? | JSON-serialized evidence dictionary |
| ResponseCode | int? | HTTP status code |
| ResponseHeaders | Dictionary<string,string>? | HTTP headers |
| FileContent | string? | File content sample |
| ProcessList | string[]? | Running processes |
| RegistryValue | string? | Registry value |
| Timing | TimingBreakdown? | Detailed timing breakdown |

### TestError (Existing - No Changes)

Error details for failed tests.

| Field | Type | Description |
|-------|------|-------------|
| Category | ErrorCategory | Error classification |
| Message | string | User-friendly error description |
| ExceptionType | string? | .NET exception type name |
| StackTrace | string? | Stack trace (debug builds) |
| InnerError | TestError? | Nested exception |

## New Components

### TestResultSummaryConverter

A value converter that generates human-readable summary text from TestResult.

**Input**: TestResult object
**Output**: string (formatted summary)

**Logic**:
```
IF Status == Pass:
  Generate success message from Evidence (test-type specific)
ELSE IF Status == Fail:
  Return Error.Message or "Test failed"
ELSE IF Status == Skipped:
  Return Error.Message or "Test was skipped"
```

### TestResultDetailsConverter

A value converter that generates formatted technical details from TestResult.Evidence.

**Input**: TestResult object
**Output**: string (formatted details)

**Logic**:
```
Parse Evidence.ResponseData as JSON
Format as:
  - Status line (if HTTP)
  - Timing info
  - Headers (if present)
  - Response body (if present)
  - File content (if present)
  - Process list (if present)
  - Registry value (if present)
```

## UI Component Updates

### ExpanderCard (Updated Properties)

New dependency properties to support metadata display:

| Property | Type | Description |
|----------|------|-------------|
| TestDuration | TimeSpan | Test execution duration |
| AttemptCount | int | Number of retry attempts |
| ShowMetadata | bool | Whether to show metadata section |

### ExpanderCard Section Styling

| Section | Background | Left Border |
|---------|------------|-------------|
| Summary | Transparent | None |
| Metadata | BackgroundBase | AccentPrimary (4px) |
| Technical Details | BackgroundBase | AccentGradient (4px) |
| Error | #1AEF4444 | StatusFail (4px) |

## State Diagram

```
TestResult Display States:

┌─────────────────┐
│  Card Collapsed │
│  (Header only)  │
└────────┬────────┘
         │ Click
         ▼
┌─────────────────┐
│  Card Expanded  │
│                 │
│  ┌───────────┐  │
│  │  Summary  │  │ ← Always visible if non-empty
│  └───────────┘  │
│  ┌───────────┐  │
│  │ Metadata  │  │ ← Duration + AttemptCount (if > 1)
│  └───────────┘  │
│  ┌───────────┐  │
│  │ Technical │  │ ← Visible if Evidence populated
│  └───────────┘  │
│  ┌───────────┐  │
│  │  Error    │  │ ← Visible only for failures
│  └───────────┘  │
└─────────────────┘
```

## Validation Rules

| Rule | Description |
|------|-------------|
| Summary Fallback | If all content empty, show "Test completed with no additional details" |
| Duration Format | Show as "Xms" for < 1s, "X.Xs" for >= 1s |
| AttemptCount Display | Only show if > 1 ("Retry: X attempts") |
| Technical Details Truncation | Visual indicator if content truncated (already handled in tests) |
