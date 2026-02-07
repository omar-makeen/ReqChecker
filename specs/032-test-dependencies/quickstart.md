# Quickstart: Test Dependencies / Skip-on-Fail

**Feature**: 032-test-dependencies
**Date**: 2026-02-07

## Overview

This feature adds an optional `dependsOn` field to test definitions, enabling the test runner to automatically skip tests whose prerequisites have failed or been skipped. It includes profile validation (missing IDs, circular chains), a UI indicator showing dependency relationships, and clear skip reasons in results.

## Key Files to Modify

### Core Layer (Models & Enums)

| File | Action | What |
|------|--------|------|
| `src/ReqChecker.Core/Models/TestDefinition.cs` | Modify | Add `DependsOn` property (`List<string>`) |
| `src/ReqChecker.Core/Enums/ErrorCategory.cs` | Modify | Add `Dependency` enum value |

### Infrastructure Layer (Execution & Validation)

| File | Action | What |
|------|--------|------|
| `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs` | Modify | Add dependency check before each test in the execution loop |
| `src/ReqChecker.Infrastructure/Profile/FluentProfileValidator.cs` | Modify | Add validation rules for `dependsOn` references and circular chains |
| `src/ReqChecker.Infrastructure/Profile/Migrations/V2ToV3Migration.cs` | Create | Schema migration: add empty `DependsOn` to existing tests |
| `src/ReqChecker.Infrastructure/Profile/ProfileMigrationPipeline.cs` | Modify | Bump `CurrentSchemaVersion` to 3, register V2ToV3 migrator |

### App Layer (UI)

| File | Action | What |
|------|--------|------|
| `src/ReqChecker.App/Profiles/default-profile.json` | Modify | Add `dependsOn` to HTTP test referencing Ping test |
| `src/ReqChecker.App/Views/TestListView.xaml` | Modify | Add dependency indicator label in test card |
| `src/ReqChecker.App/ViewModels/TestListViewModel.cs` | Modify | Add validation error banner property and validation on profile load |

### Tests

| File | Action | What |
|------|--------|------|
| `tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs` | Modify | Add tests for skip-on-fail, transitive skip, out-of-order skip, all-pass |
| `tests/ReqChecker.Infrastructure.Tests/Profile/DependencyValidationTests.cs` | Create | Tests for missing ID, circular chain, valid profile |

## Implementation Order

1. **Core model** — Add `DependsOn` to `TestDefinition`, add `Dependency` to `ErrorCategory`
2. **Validation** — Add dependency validation rules to `FluentProfileValidator`
3. **Schema migration** — Create `V2ToV3Migration`, bump pipeline version
4. **Runner logic** — Add dependency check in `SequentialTestRunner.RunTestsAsync`
5. **Unit tests** — Add runner tests and validation tests
6. **Sample profile** — Update `default-profile.json` with `dependsOn`
7. **UI indicator** — Add dependency label in `TestListView.xaml`
8. **Validation banner** — Add error display in `TestListViewModel`

## Build & Test

```bash
# Build
dotnet build src/ReqChecker.App/ReqChecker.App.csproj

# Run all tests
dotnet test

# Run specific test files
dotnet test tests/ReqChecker.Infrastructure.Tests --filter "FullyQualifiedName~SequentialTestRunnerTests"
dotnet test tests/ReqChecker.Infrastructure.Tests --filter "FullyQualifiedName~DependencyValidationTests"
```

## Key Design Decisions

- **No test reordering**: If a dependent test appears before its prerequisite, it is skipped (not reordered). This preserves the profile author's intended execution order.
- **No depth limit**: Dependency chains can be any length; only cycles are invalid.
- **Transitive propagation**: If A fails, B (depends on A) is skipped, and C (depends on B) is also skipped — via the completed results dictionary.
- **Backward compatible**: `DependsOn` defaults to empty list. Existing profiles work without changes.
- **Validation at load time**: Missing IDs and circular chains are caught before any tests execute, displayed as an inline banner.
