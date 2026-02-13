# Implementation Plan: UDP Port Reachability Test

**Branch**: `039-udp-port-test` | **Date**: 2026-02-13 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/039-udp-port-test/spec.md`

## Summary

Add a new `UdpPortOpen` test type to ReqChecker for validating UDP port reachability with optional custom payloads and response pattern matching. Implementation uses .NET 8's `UdpClient` class with async socket operations. The test follows the established `ITest` + `[TestType]` pattern, integrating with existing execution pipeline, field-level policies, icon/color converters, and profile system. Supports IPv4/IPv6, DNS resolution, configurable timeouts, hex/base64-encoded payloads, and response validation.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (test results are in-memory; parameters persisted in profile JSON files)
**Testing**: xUnit (existing test project `ReqChecker.Infrastructure.Tests`)
**Target Platform**: Windows (primary), with cross-platform support via .NET 8 `UdpClient` (works on Linux/macOS)
**Project Type**: Single WPF desktop application
**Performance Goals**: Each UDP test completes within configured timeout (default 5 seconds for UDP, overridable)
**Constraints**: No new NuGet dependencies; follow existing test implementation patterns exactly; use System.Net.Sockets.UdpClient
**Scale/Scope**: 1 new test class, 2 converter updates, 1 profile update, unit tests with mocking for UDP socket operations

## Constitution Check

*GATE: Constitution is a blank template — no project-specific gates defined. Proceeding.*

## Project Structure

### Documentation (this feature)

```text
specs/039-udp-port-test/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Technology decisions (Phase 0)
├── data-model.md        # Parameter schemas and evidence output (Phase 1)
├── quickstart.md        # Implementation guide (Phase 1)
├── checklists/
│   └── requirements.md  # Quality checklist (completed)
└── tasks.md             # Task breakdown (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── Tests/
│   │   ├── UdpPortOpenTest.cs          # NEW — UDP port reachability test
│   │   └── (existing tests unchanged)
│   └── Execution/                       # NO CHANGES
├── ReqChecker.App/
│   ├── Converters/
│   │   ├── TestTypeToIconConverter.cs  # MODIFY — add UdpPortOpen icon mapping
│   │   └── TestTypeToColorConverter.cs # MODIFY — add UdpPortOpen color mapping
│   └── Profiles/
│       └── default-profile.json        # MODIFY — add UdpPortOpen sample test
└── ReqChecker.Core/                    # NO CHANGES

tests/
└── ReqChecker.Infrastructure.Tests/
    └── Tests/
        └── UdpPortOpenTestTests.cs     # NEW — unit tests
```

**Structure Decision**: New UDP test implementation goes in `src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs`, following the established single-class-per-file convention. Unit tests go in `tests/ReqChecker.Infrastructure.Tests/Tests/UdpPortOpenTestTests.cs`.

## Complexity Tracking

> No constitutional violations — standard test type addition following established patterns.
