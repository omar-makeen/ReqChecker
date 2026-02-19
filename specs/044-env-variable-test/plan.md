# Implementation Plan: EnvironmentVariable Test

**Branch**: `044-env-variable-test` | **Date**: 2026-02-20 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/044-env-variable-test/spec.md`

## Summary

Add an `EnvironmentVariable` test type that verifies environment variables exist and optionally validates their values. Supports four match modes: exact (case-insensitive), contains (substring), regex (with timeout), and pathContains (semicolon-delimited path entry matching with trailing-separator normalization). Evidence includes variable name, found status, actual value, match details. No admin elevation required.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (in-memory test results; parameters persisted in profile JSON files)
**Testing**: xUnit via `dotnet test`
**Target Platform**: Windows 10+ (x64/ARM64)
**Project Type**: WPF desktop application (multi-project solution)
**Performance Goals**: Test execution completes within 1 second (SC-001)
**Constraints**: No administrator elevation required (FR-018); reads process environment block only
**Scale/Scope**: 1 new test class, 4 converter updates, 3 default profile entries, 1 test file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution file contains template placeholders only (no project-specific principles defined). No gates to enforce. **PASS**.

**Post-Phase 1 re-check**: Design follows existing patterns (OsVersionTest, InstalledSoftwareTest). No new projects, packages, or architectural changes. **PASS**.

## Project Structure

### Documentation (this feature)

```text
specs/044-env-variable-test/
├── plan.md              # This file
├── spec.md              # Feature specification
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Task list (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   └── Tests/
│       └── EnvironmentVariableTest.cs           # NEW — ITest implementation
└── ReqChecker.App/
    ├── Converters/
    │   ├── TestTypeToIconConverter.cs            # MODIFY — add icon mapping
    │   ├── TestTypeToColorConverter.cs           # MODIFY — add color mapping
    │   └── TestResultDetailsConverter.cs         # MODIFY — add [Environment Variable] section
    └── Profiles/
        └── default-profile.json                 # MODIFY — add 3 sample entries

tests/
└── ReqChecker.Infrastructure.Tests/
    └── Tests/
        └── EnvironmentVariableTestTests.cs      # NEW — unit tests
```

**Structure Decision**: Follows established multi-project structure. New test class goes in `ReqChecker.Infrastructure/Tests/` alongside all other test types. Unit tests go in the matching `Tests/` folder in the test project. No new projects needed.
