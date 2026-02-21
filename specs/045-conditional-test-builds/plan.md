# Implementation Plan: Conditional Test Builds

**Branch**: `045-conditional-test-builds` | **Date**: 2026-02-21 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/045-conditional-test-builds/spec.md`

## Summary

Enable customer-specific builds by conditionally compiling only the needed test type source files via an MSBuild `IncludeTests` property. A central `TestManifest.props` file maps each test type identifier to its source file. Build-time validation catches typos and unregistered files. A GitHub Actions workflow automates customer builds with artifact naming. The default profile is filtered at build time to exclude entries for omitted test types.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 LTS (net8.0 / net8.0-windows)
**Primary Dependencies**: MSBuild (build system), GitHub Actions (CI/CD) — no new runtime packages
**Storage**: N/A (build-time only; no runtime data changes)
**Testing**: xunit 2.5.3 via `dotnet test` — build validation tested via `dotnet build` invocations
**Target Platform**: Windows (win-x64), self-contained single-file publish
**Project Type**: WPF desktop application (existing multi-project solution)
**Performance Goals**: Build time increase < 5 seconds over current build
**Constraints**: Zero runtime code changes to test discovery (reflection-based DI already works); backwards-compatible default (no parameter = full build)
**Scale/Scope**: 22 test types today, scaling to 150+; one manifest file, one workflow file

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is not yet configured (template with placeholders). No gates to enforce. **PASS** — no violations possible.

Post-Phase 1 re-check: Still no configured gates. **PASS**.

## Project Structure

### Documentation (this feature)

```text
specs/045-conditional-test-builds/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0: technology research & decisions
├── data-model.md        # Phase 1: test manifest registry & relationships
├── quickstart.md        # Phase 1: usage guide for developers
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── ReqChecker.Infrastructure.csproj  # Modified: imports TestManifest.props
│   ├── TestManifest.props                # NEW: test type registry + build logic
│   └── Tests/
│       ├── TestTypeAttribute.cs          # Existing (always compiled)
│       ├── PingTest.cs                   # Existing (conditionally compiled)
│       ├── HttpGetTest.cs                # Existing (conditionally compiled)
│       └── ... (22 test files total)
│
├── ReqChecker.App/
│   ├── ReqChecker.App.csproj            # Modified: profile filtering target
│   ├── App.xaml.cs                      # Unchanged (reflection DI works as-is)
│   └── Profiles/
│       └── default-profile.json         # Filtered at build time
│
.github/
└── workflows/
    └── customer-build.yml               # NEW: GitHub Actions workflow
```

**Structure Decision**: No new projects. Changes are confined to two `.csproj` modifications, one new `.props` file, and one new workflow file. The existing project structure is preserved.

## Implementation Details

### File 1: `src/ReqChecker.Infrastructure/TestManifest.props` (NEW)

Central manifest file imported by the `.csproj`. Contains:

1. **Test registry**: One `<ItemGroup>` per test type with a condition checking `$(IncludeTests)`. When `IncludeTests` is empty, the condition defaults to true (include all).

2. **Compile removal**: A top-level `<ItemGroup>` that removes all `Tests/*Test.cs` files from compilation. The per-test-type `<ItemGroup>` blocks then selectively re-add them.

3. **Validation target `ValidateIncludeTests`**: Runs `BeforeTargets="CoreCompile"`. For each name in `IncludeTests`, checks it exists in a known list. Emits `<Error>` for unknowns.

4. **Validation target `ValidateManifestSync`**: Globs `Tests/*Test.cs`, checks each has a manifest entry. Emits `<Error>` for unregistered files.

5. **Known test type list**: An `<ItemGroup>` of `<KnownTestType>` items used by both validation targets.

### File 2: `src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj` (MODIFIED)

Add one line to import the manifest:
```xml
<Import Project="TestManifest.props" />
```

### File 3: `src/ReqChecker.App/ReqChecker.App.csproj` (MODIFIED)

Add an MSBuild target that filters `default-profile.json` before it's embedded as a resource:

- Target: `FilterDefaultProfile`
- Runs: `BeforeTargets="PrepareForBuild"` (before resource embedding)
- Logic: If `IncludeTests` is set, run an inline task or PowerShell script that:
  1. Reads `Profiles/default-profile.json`
  2. Parses the JSON `tests` array
  3. Removes entries whose `type` is not in `IncludeTests`
  4. Writes the filtered JSON to a temp copy
  5. Updates the `EmbeddedResource` item to point to the filtered copy
- When `IncludeTests` is empty: no filtering (original file embedded as-is)

### File 4: `.github/workflows/customer-build.yml` (NEW)

GitHub Actions `workflow_dispatch` workflow with inputs:

- `tests` (string, required): Semicolon-separated test types or "all"
- `customer-name` (string, required): Customer identifier for artifact naming
- `version` (string, required): Version string (e.g., "1.0.0")

Steps:
1. Checkout code
2. Setup .NET 8.0
3. Restore dependencies
4. Publish with `/p:IncludeTests="${{ inputs.tests }}"` (or no parameter if "all")
5. Rename output to `ReqChecker-{customer-name}-v{version}.zip`
6. Upload artifact

### File 5: Runtime graceful handling (FR-006)

The profile loader already creates `TestDefinition` objects from JSON. The `SequentialTestRunner` matches definitions to `ITest` implementations by the `[TestType]` attribute. If no matching `ITest` is found for a definition, it should skip that test gracefully.

Need to verify current behavior — if it already skips unknown types, no change needed. If it throws, add a null check in the test runner loop.

## Dependency Chain

```
TestManifest.props (FR-001, FR-002, FR-003, FR-011)
    → Infrastructure.csproj import (wiring)
        → Validation targets (FR-004, FR-007, FR-014)
            → Profile filtering target (FR-005)
                → Runtime graceful handling (FR-006)
                    → DependsOn warning (FR-008)
                        → GitHub Actions workflow (FR-009, FR-010)
```

## Complexity Tracking

No constitution violations to justify. No new projects, packages, or architectural patterns introduced.
