# Implementation Plan: Code Quality Fixes and Architecture Improvements

**Branch**: `003-code-quality-fixes` | **Date**: 2026-01-30 | **Spec**: [spec.md](./spec.md)
**Input**: Validated issues from comprehensive code review

## Summary

Fix critical bugs, design issues, and architectural problems across the ReqChecker codebase:
- 5 high-priority crash/correctness bugs
- 10 medium-priority functional/security issues
- 4 low-priority improvements
- 5 architectural recommendations

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 LTS
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Testing**: xUnit with coverlet
**Project Type**: WPF desktop application
**Affected Layers**: App, Infrastructure, Core

## Constitution Check

- [x] No new dependencies required
- [x] Fixes improve testability and separation of concerns
- [x] Changes are minimal and focused on identified issues
- [x] No over-engineering or speculative changes

## Project Structure Impact

### Files to Modify

```text
src/ReqChecker.App/
├── App.xaml.cs                              # [H1, H2] Fix exception handlers
├── MainWindow.xaml                          # [H3] Remove duplicate theme handler
├── MainWindow.xaml.cs                       # [L1] Consider DI improvements
├── Services/
│   ├── NavigationService.cs                 # [H4] Accept TestDefinition for config
│   └── IProfileStorageService.cs            # [M7] NEW - file operations interface
├── ViewModels/
│   ├── MainViewModel.cs                     # [M8] Add IDisposable
│   ├── TestListViewModel.cs                 # [H4, M8] Fix navigation, add dispose
│   ├── ResultsViewModel.cs                  # [H5, M8] Inject services, add dispose
│   └── ProfileSelectorViewModel.cs          # [M6, M7, M8] Logging, extract I/O, dispose
├── Converters/
│   ├── CountToVisibilityConverter.cs        # [M3] Implement Invert parameter
│   ├── NullToVisibilityConverter.cs         # [M4] NEW - for string visibility
│   └── CredentialMaskConverter.cs           # [L2] Move masker to Core
└── Views/
    ├── CredentialPromptDialog.xaml          # [M4] Use correct converter
    └── TestConfigView.xaml                  # [M5] Add input validation

src/ReqChecker.Infrastructure/
├── Execution/
│   ├── SequentialTestRunner.cs              # [M1, M10] Set RunId, don't mutate params
│   └── RetryPolicy.cs                       # [L4] Use configurable settings
├── Profile/
│   └── ProfileMigrationPipeline.cs          # [M2] Update CurrentSchemaVersion
├── Security/
│   └── WindowsCredentialProvider.cs         # [M9] Use per-user persistence
└── Export/
    └── CredentialMasker.cs                  # [L2] Move to Core (or keep)

src/ReqChecker.Core/
├── Models/
│   └── RunSummary.cs                        # [L3] Fix comment/calculation
└── Utilities/
    └── CredentialMasker.cs                  # [L2] NEW - moved from Infrastructure

tests/ReqChecker.App.Tests/
└── ViewModels/
    ├── MainViewModelTests.cs                # [AR5] Test disposal
    ├── ResultsViewModelTests.cs             # [AR5] Test filtering, export
    └── ProfileSelectorViewModelTests.cs     # [AR5] Test import, validation
```

## Implementation Phases

### Phase 1: Critical Bug Fixes (High Priority)

**Goal**: Fix crash-inducing and functionally broken code

**Tasks**:
1. **[H1]** Fix `App_DispatcherUnhandledException` - call `Shutdown()` after error dialog
2. **[H2]** Fix `CurrentDomain_UnhandledException` - marshal to UI thread, then terminate
3. **[H3]** Remove `Click="ThemeToggleButton_Click"` from MainWindow.xaml (keep Command)
4. **[H4]** Update `NavigationService.NavigateToTestConfig` to accept `TestDefinition`
5. **[H4]** Update `TestListViewModel.NavigateToTestConfig` to pass selected test
6. **[H5]** Inject `NavigationService` and `DialogService` into `ResultsViewModel` constructor

**Verification**:
- Exception handling terminates app correctly
- Theme toggle changes theme exactly once
- Test configuration opens correct test
- Export buttons work in ResultsView

### Phase 2: Data Integrity Fixes (Medium Priority - Part 1)

**Goal**: Fix data integrity and migration issues

**Tasks**:
1. **[M1]** Set `RunId = Guid.NewGuid().ToString("N")` in SequentialTestRunner
2. **[M2]** Update `CurrentSchemaVersion` to 2 in ProfileMigrationPipeline
3. **[M3]** Implement "Invert" parameter handling in CountToVisibilityConverter
4. **[M4]** Create NullToVisibilityConverter for string-to-visibility
5. **[M4]** Update CredentialPromptDialog.xaml to use NullToVisibilityConverter
6. **[M5]** Add validation to Timeout/RetryCount TextBoxes in TestConfigView.xaml

**Verification**:
- Exported reports have unique RunId
- V1 profiles are migrated to V2
- Profile list visibility logic works correctly
- Credential dialog shows/hides reference and error correctly
- Non-numeric input in config view shows validation error

### Phase 3: Security Fixes (Medium Priority - Part 2)

**Goal**: Fix credential handling and security issues

**Tasks**:
1. **[M9]** Change `CRED_PERSIST_LOCAL_MACHINE` to `CRED_PERSIST_ENTERPRISE` or `SESSION`
2. **[M9]** Zero password byte arrays after use in WindowsCredentialProvider
3. **[M10]** Refactor SequentialTestRunner to pass credentials without mutating Parameters
4. **[M10]** Create `TestExecutionContext` class to hold transient credentials

**Verification**:
- Credentials stored per-user, not machine-wide
- Password bytes cleared from memory after use
- Credentials not present in TestDefinition.Parameters after execution

### Phase 4: Architecture Improvements (Medium Priority - Part 3)

**Goal**: Fix architectural violations and memory leaks

**Tasks**:
1. **[M6]** Add logging for swallowed exceptions in ProfileSelectorViewModel
2. **[M7]** Create `IProfileStorageService` interface
3. **[M7]** Implement `ProfileStorageService` with file operations
4. **[M7]** Update ProfileSelectorViewModel to use IProfileStorageService
5. **[M8]** Add `IDisposable` to MainViewModel with event unsubscription
6. **[M8]** Add `IDisposable` to TestListViewModel with event unsubscription
7. **[M8]** Add `IDisposable` to ResultsViewModel (if it subscribes to events)
8. **[M8]** Call Dispose on ViewModels when navigating away

**Verification**:
- Profile load errors appear in logs
- File operations isolated in Infrastructure
- ViewModels properly disposed on navigation
- No memory leaks from event subscriptions

### Phase 5: Low Priority Improvements

**Goal**: Clean up remaining issues

**Tasks**:
1. **[L2]** Move CredentialMasker to Core.Utilities (or document layering decision)
2. **[L3]** Update RunSummary.PassRate comment to match implementation
3. **[L4]** Make RetryPolicy use configurable delay/strategy from TestDefinition

**Verification**:
- Layer dependencies are clean
- Documentation matches code
- Retry behavior respects configuration

### Phase 6: Testing (AR5)

**Goal**: Add test coverage for fixed functionality

**Tasks**:
1. Add disposal tests for ViewModels
2. Add ResultsViewModel filtering tests
3. Add ProfileSelectorViewModel import validation tests
4. Add integration tests for profile migration

**Verification**:
- All new tests pass
- Regression tests for fixed bugs

## Key Technical Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Credential persistence | `CRED_PERSIST_ENTERPRISE` | Per-user but survives logoff; enterprise-friendly |
| Credential passing | New `TestExecutionContext` | Clean separation; credentials never in serializable objects |
| ViewModel disposal | `IDisposable` pattern | Standard .NET pattern; explicit lifecycle |
| Profile storage | New Infrastructure service | Maintains layer separation; testable |
| Converter fix | New NullToVisibilityConverter | Reusable; follows existing pattern |

## Dependencies Between Phases

```
Phase 1 (Critical Bugs)
    ↓
Phase 2 (Data Integrity) ← can run parallel with Phase 3
    ↓
Phase 3 (Security) ← can run parallel with Phase 2
    ↓
Phase 4 (Architecture) ← depends on Phase 1 completion
    ↓
Phase 5 (Low Priority) ← can run any time after Phase 1
    ↓
Phase 6 (Testing) ← depends on all implementation phases
```

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Exception handler changes crash app | Test error scenarios thoroughly |
| Credential migration | Check for existing credentials; handle missing gracefully |
| ViewModel disposal breaks navigation | Test all navigation paths |
| Service injection breaks DI | Test app startup and all views |

## Success Criteria Mapping

| Issue | Success Criteria |
|-------|------------------|
| H1, H2 | App terminates cleanly on unhandled exception |
| H3 | Theme toggles exactly once per click |
| H4 | Test config opens the selected test |
| H5 | Export buttons function in Results view |
| M1 | Reports have unique RunId |
| M2 | V1 profiles migrate to V2 |
| M8 | No memory growth during navigation |
| M9, M10 | Credentials not exposed |

## Parallel Opportunities

- Phase 2 and Phase 3 can run in parallel (different files)
- Within Phase 4, M6 and M7 can run in parallel
- Phase 5 tasks are independent

## Estimated Complexity

| Phase | Complexity | Files Changed |
|-------|------------|---------------|
| Phase 1 | Medium | 5 files |
| Phase 2 | Low | 5 files |
| Phase 3 | Medium | 3 files |
| Phase 4 | High | 6+ files |
| Phase 5 | Low | 3 files |
| Phase 6 | Medium | 4+ test files |

**Total**: ~20-25 files modified or created
