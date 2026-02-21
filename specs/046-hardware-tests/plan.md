# Implementation Plan: SystemRam & CpuCores Hardware Tests

**Branch**: `046-hardware-tests` | **Date**: 2026-02-21 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/046-hardware-tests/spec.md`

## Summary

Add two new test types — `SystemRam` and `CpuCores` — that detect hardware specs and optionally enforce minimum thresholds from deployment guides. Both follow the established test implementation pattern (ITest, TestType attribute, evidence dictionary, informational mode). Changes span the Infrastructure test layer, the build manifest, and the bundled profiles.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (in-memory test results; parameters persisted in profile JSON files)
**Testing**: Manual verification via profile execution (consistent with existing test types)
**Target Platform**: Windows 10/11 desktop (WPF)
**Project Type**: Single solution, multi-project (App, Core, Infrastructure)
**Performance Goals**: < 2 seconds per test execution (local hardware queries, no I/O)
**Constraints**: Windows-only APIs acceptable; no admin elevation required
**Scale/Scope**: 2 new test classes, 2 manifest entries, 4 profile entries (2 per profile)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is unconfigured (template placeholders only). No project-specific gates to enforce. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/046-hardware-tests/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Core/
│   ├── Interfaces/ITest.cs                          # Existing — no changes
│   └── Models/TestResult.cs, TestEvidence.cs        # Existing — no changes
├── ReqChecker.Infrastructure/
│   ├── Tests/SystemRamTest.cs                       # NEW — SystemRam test implementation
│   ├── Tests/CpuCoresTest.cs                        # NEW — CpuCores test implementation
│   └── TestManifest.props                           # EDIT — register both new test types
└── ReqChecker.App/
    ├── Profiles/default-profile.json                # EDIT — add 2 test entries
    └── Profiles/sample-diagnostics.json             # EDIT — add 2 test entries
```

**Structure Decision**: Follows the established single-project pattern. New test files go in `Infrastructure/Tests/` alongside all other test implementations. No new projects or architectural layers needed.

## Implementation Approach

### Phase A: Core Test Implementations (FR-001 through FR-005, FR-009, FR-010)

**SystemRamTest.cs** — `[TestType("SystemRam")]`, implements `ITest`
- Detect total physical RAM via `GCMemoryInfo` or WMI (`Win32_ComputerSystem.TotalPhysicalMemory`). Preferred: `new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory` (available in .NET 8 on Windows, no extra package) which returns bytes. Convert to GB with 1 decimal place.
- Parameter: `minimumGB` (optional `double?`). Null/absent → informational mode (always pass).
- Negative `minimumGB` → `ErrorCategory.Configuration` error. Zero → always pass.
- Evidence dict: `detectedGB`, `minimumGB`, `thresholdMet`, `detectedBytes`.
- HumanSummary examples:
  - Informational: `"16.0 GB RAM detected"`
  - Pass: `"16.0 GB RAM detected (minimum: 8.0 GB) — Pass"`
  - Fail: `"16.0 GB RAM detected (minimum: 32.0 GB) — Fail"`

**CpuCoresTest.cs** — `[TestType("CpuCores")]`, implements `ITest`
- Detect logical processor count via `Environment.ProcessorCount`.
- Parameter: `minimumCores` (optional `int?`). Null/absent → informational mode.
- Negative `minimumCores` → `ErrorCategory.Configuration` error. Zero → always pass.
- Evidence dict: `detectedCores`, `minimumCores`, `thresholdMet`.
- HumanSummary examples:
  - Informational: `"8 logical processors detected"`
  - Pass: `"8 logical processors detected (minimum: 4) — Pass"`
  - Fail: `"8 logical processors detected (minimum: 16) — Fail"`

### Phase B: Build Manifest Registration (FR-006)

**TestManifest.props** — Add to KnownTestType registry and conditional Compile ItemGroups:
- `<KnownTestType Include="SystemRam" SourceFile="Tests\SystemRamTest.cs" />`
- `<KnownTestType Include="CpuCores" SourceFile="Tests\CpuCoresTest.cs" />`
- Two new `<ItemGroup Condition="...">` blocks for conditional compilation.

DI registration is automatic (reflection-based discovery in App.xaml.cs).

### Phase C: Bundled Profile Entries (FR-007, FR-008)

**default-profile.json** — Append 6 entries after the last existing test (3 per type, matching OsVersion/InstalledSoftware pattern):
- `test-028` SystemRam informational (`minimumGB: null`)
- `test-029` SystemRam minimum 4 GB (expected pass on most machines)
- `test-030` SystemRam minimum 1024 GB (expected fail — unreachable threshold)
- `test-031` CpuCores informational (`minimumCores: null`)
- `test-032` CpuCores minimum 2 (expected pass on most machines)
- `test-033` CpuCores minimum 256 (expected fail — unreachable threshold)
- All with `dependsOn: []`, `requiresAdmin: false`, fieldPolicy `"Editable"`

**sample-diagnostics.json** — Append 6 entries (3 per type):
- `10000000-…-000000000005` SystemRam informational
- `10000000-…-000000000006` SystemRam minimum 4 GB (expected pass)
- `10000000-…-000000000007` SystemRam minimum 1024 GB (expected fail)
- `10000000-…-000000000008` CpuCores informational
- `10000000-…-000000000009` CpuCores minimum 2 (expected pass)
- `10000000-…-00000000000a` CpuCores minimum 256 (expected fail)

### Verification

1. `dotnet build` — full build succeeds, no warnings
2. `dotnet build src/ReqChecker.App -p:IncludeTests=SystemRam` — filtered build includes only SystemRam
3. Launch app → default profile loads → both tests visible → run → both pass informational

## Complexity Tracking

No constitution violations. No complexity justifications needed.
