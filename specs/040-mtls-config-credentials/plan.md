# Implementation Plan: Move mTLS Credentials to Test Configuration

**Branch**: `040-mtls-config-credentials` | **Date**: 2026-02-13 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/040-mtls-config-credentials/spec.md`

## Summary

Move the PFX certificate passphrase from a runtime credential dialog into the test configuration parameters (`pfxPassword`). The test runner reads the passphrase from configuration and creates a `TestExecutionContext` directly — no dialog, no prompt, no UI blocking. The test config UI renders the field as a masked `PasswordBox` using a naming convention. Backward compatibility with `credentialRef` is preserved.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: Profile JSON files (System.Text.Json / JsonObject)
**Testing**: xUnit + Moq via `dotnet test`
**Target Platform**: Windows (WPF desktop application)
**Project Type**: Desktop application (multi-project solution)
**Performance Goals**: N/A (configuration change, no performance-sensitive paths)
**Constraints**: No new dependencies; backward compatible with existing profiles
**Scale/Scope**: 6 files modified, ~100 lines changed

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is a blank template — no project-specific gates defined. All gates pass by default.

**Post-Phase 1 re-check**: No violations. Feature adds no new projects, no new dependencies, no new patterns. Uses existing `TestExecutionContext` and `FieldPolicyType` patterns.

## Project Structure

### Documentation (this feature)

```text
specs/040-mtls-config-credentials/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: research findings
├── data-model.md        # Phase 1: data model changes
├── quickstart.md        # Phase 1: implementation quickstart
├── contracts/           # Phase 1: no external contracts (README only)
│   └── README.md
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── Execution/
│   │   └── SequentialTestRunner.cs        # Add pfxPassword check before credentialRef
│   └── Tests/
│       └── MtlsConnectTest.cs             # Improve error message for wrong passphrase
├── ReqChecker.App/
│   ├── Views/
│   │   └── TestConfigView.xaml            # Add PasswordBox for password-type parameters
│   ├── ViewModels/
│   │   └── TestConfigViewModel.cs         # Add IsPassword property to TestParameterViewModel
│   └── Profiles/
│       └── default-profile.json           # Replace credentialRef with pfxPassword in mTLS entries

tests/
└── ReqChecker.Infrastructure.Tests/
    └── Execution/
        └── SequentialTestRunnerTests.cs    # Add tests for pfxPassword credential resolution
```

**Structure Decision**: Uses existing multi-project structure. No new projects or directories needed. All changes are modifications to existing files within the existing `src/` and `tests/` layout.

## Complexity Tracking

No constitution violations. No complexity justification needed.
