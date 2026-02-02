# Data Model: Export Test Results

## Overview

This feature extends the existing export infrastructure. No new entities are required - the feature operates on the existing `RunReport` aggregate.

## Existing Entities (No Changes)

### RunReport (Aggregate Root)
The primary data structure for export, already fully defined.

```
RunReport
├── RunId: string
├── ProfileId: string
├── ProfileName: string
├── StartTime: DateTimeOffset
├── EndTime: DateTimeOffset
├── Duration: TimeSpan
├── MachineInfo: MachineInfo
├── Results: List<TestResult>
└── Summary: RunSummary
```

### TestResult
Individual test execution outcome.

```
TestResult
├── TestId: string
├── TestType: string
├── DisplayName: string
├── Status: TestStatus (Pass|Fail|Skipped)
├── StartTime: DateTimeOffset
├── EndTime: DateTimeOffset
├── Duration: TimeSpan
├── AttemptCount: int
├── Evidence: TestEvidence
├── Error: TestError? (nullable)
├── HumanSummary: string
└── TechnicalDetails: string
```

### Supporting Types
- `MachineInfo`: Hostname, OS, memory, network interfaces
- `RunSummary`: TotalTests, Passed, Failed, Skipped, PassRate
- `TestEvidence`: ResponseCode, ResponseData, captured artifacts
- `TestError`: Category, Message, StackTrace

## Export Format Mapping

### JSON Export
Direct serialization of `RunReport` using `System.Text.Json` source generation.
- All fields preserved
- Dates as ISO 8601 strings
- Durations as ISO 8601 duration format

### CSV Export
Flattened representation with sections:
1. **Run Summary**: Key-value pairs (RunId, Profile, Times, Stats)
2. **Machine Info**: Key-value pairs (Hostname, OS, etc.)
3. **Test Results**: Tabular data (one row per test)

### PDF Export
Formatted document with:
1. **Header**: Logo, profile name, date
2. **Summary Dashboard**: Visual stats (total, pass, fail, skip)
3. **Machine Info**: Formatted section
4. **Test Results Table**: Formatted table with status colors
5. **Error Details**: Inline under failed tests

## Credential Masking

All exporters support `maskCredentials` parameter (default: true):
- Sensitive fields in evidence/details are masked
- Uses existing `CredentialMasker.MaskCredentials(report)` utility
- Original report is not modified (returns masked copy)
