# Implementation Plan: OS Version Validation Test

**Branch**: `042-os-version-test` | **Date**: 2026-02-19 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/042-os-version-test/spec.md`

## Summary

Add a new `OsVersion` test type that reads the local machine's Windows version/build number and compares it against configurable requirements (minimum build or exact version match). Follows the established test type pattern: implement `ITest`, add `[TestType("OsVersion")]` attribute for auto-discovery, add icon/colour mappings, add a default profile entry, and write unit tests.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (in-memory test results; parameters persisted in profile JSON files)
**Testing**: xUnit + Moq (existing test infrastructure)
**Target Platform**: Windows 10/11 (x64, ARM64)
**Project Type**: WPF desktop application (single solution, multi-project)
**Performance Goals**: Test executes in <100ms (local OS query, no I/O)
**Constraints**: No elevated privileges required; Windows-only
**Scale/Scope**: 1 new test class, 1 test file, 3 converter/profile touchpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution file contains placeholder template only (not configured for this project). No gates to evaluate. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/042-os-version-test/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Core/
│   ├── Enums/              # TestStatus, ErrorCategory (existing, unchanged)
│   ├── Interfaces/ITest.cs # (existing, unchanged)
│   └── Models/             # TestResult, TestEvidence, TestError (existing, unchanged)
├── ReqChecker.Infrastructure/
│   └── Tests/
│       └── OsVersionTest.cs          # NEW — test implementation
└── ReqChecker.App/
    ├── Converters/
    │   ├── TestTypeToIconConverter.cs  # MODIFY — add "OsVersion" case
    │   └── TestTypeToColorConverter.cs # MODIFY — add "OsVersion" case
    └── Profiles/
        └── default-profile.json       # MODIFY — add test-019 entry

tests/
└── ReqChecker.Infrastructure.Tests/
    └── Tests/
        └── OsVersionTestTests.cs      # NEW — unit tests
```

**Structure Decision**: Follows the existing multi-project layout. No new projects or directories needed. The OsVersion test is added alongside existing tests in the Infrastructure project.

## Files to Create

### 1. `src/ReqChecker.Infrastructure/Tests/OsVersionTest.cs`

New file implementing `ITest` with `[TestType("OsVersion")]` attribute.

**Class structure** (follows RegistryReadTest pattern):
- `OsVersionTest : ITest`
- `ExecuteAsync(TestDefinition, TestExecutionContext?, CancellationToken)` — main entry
- Parameter extraction: `minimumBuild` (int?), `expectedVersion` (string?), `timeout` (int)
- OS detection via `Environment.OSVersion` and `RuntimeInformation.OSArchitecture`
- Product name via registry (`HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProductName`)
- Validation logic:
  - Both `minimumBuild` and `expectedVersion` set → `Configuration` error (FR-006)
  - Invalid `expectedVersion` format → `Configuration` error (FR-007)
  - Neither set → informational pass (FR-005)
  - `minimumBuild` set → compare build component (FR-003)
  - `expectedVersion` set → exact match on "major.minor.build" (FR-004)
- Evidence: serialize `{ productName, version, buildNumber, architecture }` as JSON to `ResponseData`
- Human summary: e.g. "OS version 10.0.22631 meets minimum build 19045"
- Error handling: `OperationCanceledException` → Skipped; `Exception` → Fail/Configuration

### 2. `tests/ReqChecker.Infrastructure.Tests/Tests/OsVersionTestTests.cs`

New file with unit tests covering:
- **Parameter Validation**: missing params (informational), invalid `expectedVersion`, both params set
- **Cancellation**: pre-cancelled token → Skipped
- **Result Properties**: TestId, TestType, DisplayName, StartTime, EndTime, Duration populated
- **Evidence**: ResponseData contains OS info JSON
- **Human Summary**: non-empty on all paths
- **Informational Mode**: no constraints → Pass with OS info
- **Minimum Build**: current build >= minimum → Pass; mock scenario for fail
- **Exact Match**: current version matches → Pass; mismatched → Fail
- **Integration Tests** (skipped): real OS version check

## Files to Modify

### 3. `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs`

Add `"OsVersion"` case to the switch expression. Use `SymbolRegular.Desktop24` (represents the system/machine). Place after `"RegistryRead"` in the system tests group.

### 4. `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs`

Add `"OsVersion"` to the `AccentSecondary` group (system-level tests alongside ProcessList, RegistryRead, WindowsService).

Change:
```
"ProcessList" or "RegistryRead" or "WindowsService" =>
```
To:
```
"ProcessList" or "RegistryRead" or "WindowsService" or "OsVersion" =>
```

### 5. `src/ReqChecker.App/Profiles/default-profile.json`

Add `test-019` entry after `test-018`:
```json
{
  "id": "test-019",
  "type": "OsVersion",
  "displayName": "Check Windows Version",
  "description": "Verifies the local machine's Windows version and build number.",
  "parameters": {
    "minimumBuild": null,
    "expectedVersion": null
  },
  "fieldPolicy": {
    "minimumBuild": "Editable",
    "expectedVersion": "Editable"
  },
  "timeout": null,
  "retryCount": null,
  "requiresAdmin": false,
  "dependsOn": []
}
```

This uses informational mode (no constraints) so the test always passes and reports OS info out of the box.

## Complexity Tracking

No constitution violations to justify. Single test class following established patterns.
