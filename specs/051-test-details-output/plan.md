# Implementation Plan: Test Details Output for All Test Types

**Branch**: `051-test-details-output` | **Date**: 2026-02-23 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/051-test-details-output/spec.md`

## Summary

Add dedicated detail sections to the `TestResultDetailsConverter` for the 10 test types that currently only display `[General]` + `[Timing]` in the results view. Each new section extracts evidence keys unique to that test type and renders them as aligned key-value pairs, matching the established converter patterns.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, System.Text.Json (existing — no new packages)
**Storage**: N/A (in-memory TestResult evidence data)
**Testing**: Manual verification via app UI (no unit test framework for converters)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop application (WPF + MVVM)
**Performance Goals**: N/A — converter is synchronous string formatting, negligible cost
**Constraints**: Single file change only; must not alter existing section rendering
**Scale/Scope**: 10 new converter sections in one file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is a blank template — no project-level gates defined. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/051-test-details-output/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output — evidence key mappings
├── data-model.md        # Phase 1 output — evidence key → section mapping
├── quickstart.md        # Phase 1 output — manual test scenarios
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/ReqChecker.App/Converters/
└── TestResultDetailsConverter.cs   # ONLY file modified
```

**Structure Decision**: This feature modifies a single existing file. No new files, classes, or projects are introduced. All 10 new sections are added as consecutive `if` blocks within the existing `GenerateTechnicalDetails` method, inserted before the generic `[Response]` section (line 299).

## Design Decisions

### Section Detection Strategy

Each section is detected by a unique combination of evidence keys that only that test type produces. This prevents false matches:

| Section | Detection Keys | Rationale |
|---------|---------------|-----------|
| `[Ping]` | `successRate` + `pingResults` | Only Ping has both |
| `[DNS]` | `hostname` + `addresses` | Only DnsResolve has both |
| `[TCP]` | `host` + `port` + `connected` | Only TcpPortOpen has all three |
| `[UDP]` | `responded` + `payloadSentBytes` | Only UdpPortOpen has both |
| `[Disk Space]` | `totalSpaceGB` + `freeSpaceGB` | Only DiskSpace has both |
| `[Service]` | `serviceName` + `expectedStatus` | Only WindowsService has both |
| `[mTLS]` | `certificateSubject` + `certificateThumbprint` | Only MtlsConnect has both |
| `[Certificate]` | `daysUntilExpiry` + `isExpired` | Only CertificateExpiry has both |
| `[File]` | `path` + `exists` + `size` | Only FileExists has all three |
| `[Directory]` | `path` + `exists` + `directoryCount` | Only DirectoryExists has both |

### Insertion Point

New sections are inserted **after** `[Traceroute]` (line 297) and **before** `[Response]` (line 299). This keeps all dedicated test-type sections grouped together, with generic sections (`[Response]`, `[Headers]`, `[Body]`, etc.) remaining at the end.

### Formatting Convention

All new sections follow the established pattern:
- Section header: `[SectionName]`
- Key-value pairs with aligned labels (using padding to column 14)
- Null/missing fields silently omitted
- Empty line at end of section
- Boolean values displayed as `yes` / `no`
- Lists (addresses, ping results) rendered as indented sub-items

## Complexity Tracking

No constitution violations — no complexity tracking needed.
