# Implementation Plan: Traceroute Test Type

**Branch**: `050-traceroute-test` | **Date**: 2026-02-22 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/050-traceroute-test/spec.md`

## Summary

Add a `Traceroute` test type that traces the network path to a target host by sending ICMP echo requests with incrementing TTL values. Each hop's responding IP address and round-trip time are recorded. Uses .NET 8's built-in `System.Net.NetworkInformation.Ping` with `PingOptions.Ttl` — the same API already used by the existing PingTest. No new packages required. Evidence is displayed in `tracert`-style compact lines.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: System.Net.NetworkInformation (Ping, PingOptions, PingReply — all built-in, no new packages)
**Storage**: N/A (in-memory test results; parameters persisted in profile JSON files)
**Testing**: Manual testing via app launch (consistent with all other test types in this project)
**Target Platform**: Windows 10/11 (x64)
**Project Type**: Single — WPF desktop application with layered architecture
**Performance Goals**: Complete trace within `maxHops * timeout` (default 30 * 5000ms = 150s worst case)
**Constraints**: Must not introduce new NuGet packages; must follow existing test implementation patterns
**Scale/Scope**: Single new test type; 7 files created/modified

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is unconfigured (template placeholders only). No gates to evaluate. Proceeding.

**Post-Phase 1 re-check**: No constitution violations — constitution is unconfigured.

## Project Structure

### Documentation (this feature)

```text
specs/050-traceroute-test/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output — technology decisions
├── data-model.md        # Phase 1 output — parameters & evidence schema
├── quickstart.md        # Phase 1 output — usage examples
└── checklists/
    └── requirements.md  # Specification quality checklist
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── Tests/
│   │   └── TracerouteTest.cs           # NEW — test implementation
│   └── TestManifest.props              # MODIFY — add conditional build entry
├── ReqChecker.App/
│   ├── Converters/
│   │   └── TestResultDetailsConverter.cs  # MODIFY — add [Traceroute] evidence section
│   └── Profiles/
│       ├── default-profile.json           # MODIFY — add sample Traceroute test
│       └── sample-diagnostics.json        # MODIFY — add sample Traceroute test
README.md                                  # MODIFY — update count 25→26, add docs
CLAUDE.md                                  # MODIFY — add 050-traceroute-test entry
```

**Structure Decision**: Follows the existing single-project structure. New test class goes in `Infrastructure/Tests/` alongside all other test implementations. No new projects or directories needed.
