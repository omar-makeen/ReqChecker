# Implementation Plan: InstalledSoftware Test

**Branch**: `043-installed-software-test` | **Date**: 2026-02-20 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/043-installed-software-test/spec.md`

## Summary

Add an `InstalledSoftware` test type that answers "Is X installed?" — the #1 readiness question. The test searches Windows registry uninstall keys (64-bit, 32-bit, and per-user hives) for software by display name using case-insensitive substring matching. Supports optional `minimumVersion` enforcement. When multiple matches exist, the highest-version entry is used as the primary result. No admin elevation required.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (in-memory test results; parameters persisted in profile JSON files)
**Testing**: xUnit via `dotnet test`
**Target Platform**: Windows 10+ (x64/ARM64)
**Project Type**: WPF desktop application (multi-project solution)
**Performance Goals**: Test execution completes within 2 seconds (SC-001)
**Constraints**: No administrator elevation required (FR-013); registry-only detection
**Scale/Scope**: 1 new test class, 4 converter updates, 3 default profile entries, 1 test file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution file contains template placeholders only (no project-specific principles defined). No gates to enforce. **PASS**.

**Post-Phase 1 re-check**: Design follows existing patterns (OsVersionTest, ProcessListTest). No new projects, packages, or architectural changes. **PASS**.

## Project Structure

### Documentation (this feature)

```text
specs/043-installed-software-test/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: technology decisions
├── data-model.md        # Phase 1: entity definitions and evidence schema
├── quickstart.md        # Phase 1: build & run instructions
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   └── Tests/
│       └── InstalledSoftwareTest.cs          # NEW — ITest implementation
└── ReqChecker.App/
    ├── Converters/
    │   ├── TestTypeToIconConverter.cs         # MODIFY — add icon mapping
    │   ├── TestTypeToColorConverter.cs        # MODIFY — add color mapping
    │   └── TestResultDetailsConverter.cs      # MODIFY — add [Installed Software] section
    └── Profiles/
        └── default-profile.json              # MODIFY — add 3 sample entries

tests/
└── ReqChecker.Infrastructure.Tests/
    └── Tests/
        └── InstalledSoftwareTestTests.cs     # NEW — unit tests
```

**Structure Decision**: Follows established multi-project structure. New test class goes in `ReqChecker.Infrastructure/Tests/` alongside all other test types. Unit tests go in the matching `Tests/` folder in the test project. No new projects needed.
