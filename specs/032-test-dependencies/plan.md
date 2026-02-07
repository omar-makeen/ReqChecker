# Implementation Plan: Test Dependencies / Skip-on-Fail

**Branch**: `032-test-dependencies` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/032-test-dependencies/spec.md`

## Summary

Allow test definitions to declare prerequisite tests via an optional `dependsOn` field. During sequential execution, if a prerequisite test fails or is skipped, all dependent tests are automatically skipped with a clear reason message. Profile validation detects missing references and circular chains at load time. The test list UI shows dependency indicators, and the results view distinguishes dependency-skips from other skip types.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, FluentValidation, Microsoft.Extensions.DependencyInjection
**Storage**: N/A (in-memory session-only; `dependsOn` persisted in profile JSON files)
**Testing**: xUnit 2.5.3+, Moq 4.20.72
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop WPF application (3-project solution: Core, Infrastructure, App)
**Performance Goals**: Dependency checks add negligible overhead (in-memory dictionary lookups)
**Constraints**: Must be backward-compatible with existing profiles (no `dependsOn` = no dependencies)
**Scale/Scope**: Profiles typically contain 5–30 tests; dependency chains are short (2–5 levels)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is not yet configured for this project (template placeholders only). No gates to evaluate. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/032-test-dependencies/
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
│   ├── Models/
│   │   └── TestDefinition.cs          # ADD: DependsOn property
│   ├── Enums/
│   │   └── ErrorCategory.cs           # ADD: Dependency enum value
│   └── Interfaces/
│       └── IProfileValidator.cs       # No changes (existing interface)
├── ReqChecker.Infrastructure/
│   ├── Execution/
│   │   └── SequentialTestRunner.cs    # MODIFY: dependency check before each test
│   ├── Profile/
│   │   ├── FluentProfileValidator.cs  # ADD: dependency validation rules
│   │   └── Migrations/
│   │       └── V2ToV3Migration.cs     # NEW: schema migration adding dependsOn
│   └── ProfileManagement/
│       └── ProfileMigrationPipeline.cs # UPDATE: bump CurrentSchemaVersion to 3
└── ReqChecker.App/
    ├── Profiles/
    │   └── default-profile.json       # UPDATE: add dependsOn to HTTP test
    ├── Views/
    │   └── TestListView.xaml          # ADD: dependency indicator in test card
    └── ViewModels/
        └── TestListViewModel.cs       # ADD: validation error banner property

tests/
├── ReqChecker.Infrastructure.Tests/
│   ├── Execution/
│   │   └── SequentialTestRunnerTests.cs  # ADD: dependency skip tests
│   └── Profile/
│       └── DependencyValidationTests.cs  # NEW: validation unit tests
└── ReqChecker.App.Tests/
    └── ViewModels/
        └── TestListViewModelTests.cs     # ADD: validation banner tests (if needed)
```

**Structure Decision**: Follows existing 3-project architecture. No new projects needed. Changes span all three layers: Core (model), Infrastructure (execution + validation), App (UI display).

## Complexity Tracking

No constitution violations to justify.
