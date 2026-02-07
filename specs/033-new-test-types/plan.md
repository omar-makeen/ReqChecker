# Implementation Plan: New Test Types (DnsResolve, TcpPortOpen, WindowsService, DiskSpace)

**Branch**: `033-new-test-types` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/033-new-test-types/spec.md`

## Summary

Add four new test types (`DnsResolve`, `TcpPortOpen`, `WindowsService`, `DiskSpace`) to the ReqChecker test infrastructure using built-in .NET 8 APIs. Each test follows the established `ITest` + `[TestType]` pattern, plugging into the existing execution pipeline, icon/color converters, and profile system. A `DnsLookup` alias preserves backward compatibility with existing profiles.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (test results are in-memory; parameters persisted in profile JSON files)
**Testing**: xUnit (existing test project `ReqChecker.Infrastructure.Tests`)
**Target Platform**: Windows (primary), with cross-platform fallbacks for WindowsService (skip) and DiskSpace (mount paths)
**Project Type**: Single WPF desktop application
**Performance Goals**: Each test completes within configured timeout (default 30s)
**Constraints**: No new NuGet dependencies; follow existing test implementation patterns exactly
**Scale/Scope**: 4 new test classes, 2 converter updates, 1 runner alias, 2 profile updates, unit tests

## Constitution Check

*GATE: Constitution is a blank template — no project-specific gates defined. Proceeding.*

## Project Structure

### Documentation (this feature)

```text
specs/033-new-test-types/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Technology decisions
├── data-model.md        # Parameter schemas and evidence output
├── quickstart.md        # Implementation guide
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Task breakdown (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── Tests/
│   │   ├── DnsResolveTest.cs          # NEW — DNS resolution test
│   │   ├── TcpPortOpenTest.cs         # NEW — TCP port connectivity test
│   │   ├── WindowsServiceTest.cs      # NEW — Windows service status check
│   │   ├── DiskSpaceTest.cs           # NEW — Disk space threshold check
│   │   └── (existing tests unchanged)
│   └── Execution/
│       └── SequentialTestRunner.cs     # MODIFY — add DnsLookup→DnsResolve alias
├── ReqChecker.App/
│   ├── Converters/
│   │   ├── TestTypeToIconConverter.cs  # MODIFY — add 4 new type mappings
│   │   └── TestTypeToColorConverter.cs # MODIFY — add 4 new type mappings
│   └── Profiles/
│       ├── sample-diagnostics.json     # MODIFY — update DnsLookup→DnsResolve
│       └── default-profile.json        # MODIFY — add TcpPortOpen + DiskSpace tests
└── ReqChecker.Core/                    # NO CHANGES

tests/
└── ReqChecker.Infrastructure.Tests/
    └── Tests/
        ├── DnsResolveTestTests.cs      # NEW — unit tests
        ├── TcpPortOpenTestTests.cs     # NEW — unit tests
        ├── WindowsServiceTestTests.cs  # NEW — unit tests
        └── DiskSpaceTestTests.cs       # NEW — unit tests
```

**Structure Decision**: All new test implementations go in the existing `src/ReqChecker.Infrastructure/Tests/` directory, following the established single-class-per-file convention. Unit tests go in a new `Tests/` subdirectory under the existing test project.
