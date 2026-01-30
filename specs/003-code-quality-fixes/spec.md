# Feature Specification: Code Quality Fixes and Architecture Improvements

**Feature**: 003-code-quality-fixes
**Date**: 2026-01-30
**Status**: Draft
**Priority**: High

## Overview

Address critical bugs, design issues, and architectural improvements identified through comprehensive code review. This specification covers validated issues ranging from crash-inducing bugs to security concerns and architectural violations.

## Problem Statement

The ReqChecker application has several issues affecting stability, correctness, and maintainability:

1. **Crash/Stability Issues**: Exception handlers leave app in invalid state; UI thread violations in error handling
2. **Functional Bugs**: Theme toggle fires twice; navigation ignores selected test; services not injected
3. **Data Integrity**: Run reports have no ID; migrations never execute
4. **Security Concerns**: Credentials persist with wrong scope; credentials injected into serializable parameters
5. **Architecture Violations**: File I/O in ViewModels; event subscriptions leak; layer violations

## Validated Issues

### High Priority (Must Fix)

#### H1: DispatcherUnhandledException leaves app in invalid state
- **File**: `src/ReqChecker.App/App.xaml.cs:60-73`
- **Problem**: Exception is marked as handled (`e.Handled = true`) but the message says the app "needs to close." App continues running in potentially corrupted state.
- **Evidence**: Code shows `e.Handled = true` after showing error dialog stating app "needs to close"
- **Fix**: Call `Shutdown()` after showing the dialog, or set `e.Handled = false`

#### H2: CurrentDomain_UnhandledException UI thread violation
- **File**: `src/ReqChecker.App/App.xaml.cs:76-91`
- **Problem**: Handler runs on non-UI thread but calls `ShowErrorDialog()` which uses `MessageBox.Show()`. Can throw or deadlock.
- **Evidence**: `AppDomain.CurrentDomain.UnhandledException` fires on the crashing thread, not UI thread
- **Fix**: Marshal UI work to `Dispatcher.Invoke`; log and terminate cleanly

#### H3: Theme toggle button fires command AND click handler
- **File**: `src/ReqChecker.App/MainWindow.xaml:189-198`
- **Problem**: Button has both `Command="{Binding ...ToggleThemeCommand}"` AND `Click="ThemeToggleButton_Click"`. Theme toggles twice, ending up at original theme.
- **Evidence**: XAML shows both bindings; code-behind at line 104-108 calls `_themeService.ToggleTheme()`
- **Fix**: Remove one handler (prefer Command for MVVM)

#### H4: NavigateToTestConfig ignores selected test
- **File**: `src/ReqChecker.App/ViewModels/TestListViewModel.cs:65-72`
- **Problem**: Command takes `TestDefinition? test` parameter but passes `CurrentProfile` to navigation. `NavigationService.NavigateToTestConfig()` then uses `profile.Tests.FirstOrDefault()`.
- **Evidence**: Line 71 shows `_navigationService.NavigateToTestConfig(CurrentProfile)` ignoring the `test` parameter
- **Fix**: Change `NavigationService.NavigateToTestConfig` to accept `TestDefinition`; pass selected test

#### H5: ResultsViewModel services never injected
- **File**: `src/ReqChecker.App/ViewModels/ResultsViewModel.cs:40-44`
- **Problem**: `NavigationService` and `DialogService` are `[ObservableProperty]` fields, not constructor-injected. They remain null, so `NavigateToTestList()` and export commands silently do nothing.
- **Evidence**: Constructor at line 59-64 only injects exporters and appState; NavigationService/DialogService are not set
- **Fix**: Inject via constructor or wire up through ViewModel factory/DI

### Medium Priority (Should Fix)

#### M1: RunReport.RunId never set
- **File**: `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs:128-138`
- **Problem**: `RunReport` is built without setting `RunId`. Exports default to `report-.json/.csv`; diagnostics show blank run IDs.
- **Evidence**: Report construction at lines 128-138 has no `RunId = ...` assignment
- **Fix**: Set `RunId = Guid.NewGuid().ToString("N")` when building report

#### M2: ProfileMigrationPipeline.CurrentSchemaVersion = 1 but V1ToV2Migration exists
- **File**: `src/ReqChecker.Infrastructure/Profile/ProfileMigrationPipeline.cs:13`
- **Problem**: `CurrentSchemaVersion` is 1, so `NeedsMigration()` returns false for v1 profiles. V1ToV2Migration never runs.
- **Evidence**: Line 13 shows `private const int CurrentSchemaVersion = 1`; App.xaml.cs registers `V1ToV2Migration`
- **Fix**: Update `CurrentSchemaVersion` to 2 (or latest schema version)

#### M3: CountToVisibilityConverter ignores "Invert" parameter
- **File**: `src/ReqChecker.App/Converters/CountToVisibilityConverter.cs:14-22`
- **Problem**: Converter ignores `ConverterParameter` but views use `ConverterParameter="Invert"`. Visibility logic is inverted.
- **Evidence**: ProfileSelectorView.xaml:136 uses `ConverterParameter=Invert`; converter ignores `parameter`
- **Fix**: Implement invert handling or remove parameter usage from XAML

#### M4: BoolToVisibilityConverter applied to strings in CredentialPromptDialog
- **File**: `src/ReqChecker.App/Views/CredentialPromptDialog.xaml:85,141`
- **Problem**: `BoolToVisibilityConverter` is applied to `CredentialRef` (string) and `ErrorMessage` (string). Strings are not booleans, so visibility is always collapsed.
- **Evidence**: Lines 85 and 141 bind string properties with BoolToVisibilityConverter
- **Fix**: Use `NullToVisibilityConverter` or `StringToBoolConverter`

#### M5: TextBox bindings for Timeout/RetryCount lack validation
- **File**: `src/ReqChecker.App/Views/TestConfigView.xaml:208-231`
- **Problem**: TextBox binds to nullable int without validation. Non-numeric input generates binding errors.
- **Evidence**: Lines 208-212 and 227-231 show direct Text binding to int properties
- **Fix**: Add `ValidatesOnExceptions=True`, use numeric input control, or add converter

#### M6: Profile load exceptions swallowed silently
- **File**: `src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs:140-143,185-188`
- **Problem**: Exceptions in profile load loops are caught and ignored. Corrupted profiles or parsing errors are hidden.
- **Evidence**: Empty catch blocks at lines 140-143 and 185-188
- **Fix**: Log exceptions (at least debug level); surface actionable errors for user profiles

#### M7: File I/O in ProfileSelectorViewModel
- **File**: `src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs:232-238`
- **Problem**: ViewModel performs `File.Copy()` and `Directory.CreateDirectory()`, mixing UI and infrastructure concerns.
- **Evidence**: Import method contains direct file system operations
- **Fix**: Move file operations to a profile storage service in Infrastructure

#### M8: ViewModels never unsubscribe from AppState events
- **File**: `src/ReqChecker.App/ViewModels/MainViewModel.cs:46`
- **Problem**: ViewModels subscribe to `IAppState.CurrentProfileChanged` but never unsubscribe. AppState is singleton, so ViewModels leak after navigation.
- **Evidence**: Line 46 shows subscription; no `IDisposable` implementation or unsubscription
- **Fix**: Implement `IDisposable` on ViewModels; detach handlers in `Dispose()`, or use weak events

#### M9: Credentials persist with CRED_PERSIST_LOCAL_MACHINE
- **File**: `src/ReqChecker.Infrastructure/Security/WindowsCredentialProvider.cs:84`
- **Problem**: Credentials are stored with `CRED_PERSIST_LOCAL_MACHINE`, exposing them to other users on the machine. Password bytes not zeroed after use.
- **Evidence**: Line 84 shows `Persist = NativeMethods.CRED_PERSIST_LOCAL_MACHINE`
- **Fix**: Use `CRED_PERSIST_SESSION` or enterprise per-user persistence; zero byte arrays after use

#### M10: Credentials injected into TestDefinition.Parameters
- **File**: `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs:196-197`
- **Problem**: Username/password are written to `TestDefinition.Parameters` dictionary. If parameters are serialized (logs, exports), credentials leak.
- **Evidence**: Lines 196-197 show `testDefinition.Parameters["username"] = ...`
- **Fix**: Pass credentials directly to test implementation without mutating profile parameters

### Low Priority (Consider Fixing)

#### L1: App.Services static service locator
- **File**: `src/ReqChecker.App/MainWindow.xaml.cs:27`
- **Problem**: `App.Services` static property injection reduces testability and hides dependencies.
- **Evidence**: Line 27 shows `App.Services.GetRequiredService<MainViewModel>()`
- **Fix**: Inject dependencies via constructor or use ViewModel factory

#### L2: CredentialMaskConverter depends on Infrastructure
- **File**: `src/ReqChecker.App/Converters/CredentialMaskConverter.cs:1`
- **Problem**: App layer imports `ReqChecker.Infrastructure.Export`, violating Core -> Infrastructure -> App layering.
- **Evidence**: Line 1 shows `using ReqChecker.Infrastructure.Export`
- **Fix**: Move `CredentialMasker` to Core or shared utility layer

#### L3: RunSummary.PassRate comment vs implementation mismatch
- **File**: `src/ReqChecker.Core/Models/RunSummary.cs:29`
- **Problem**: Comment says "passed / (passed + failed)" but implementation uses "passed / total" (includes skipped).
- **Evidence**: Comment at line 29; calculation in SequentialTestRunner.cs:124
- **Fix**: Align calculation or update comment

#### L4: RetryPolicy ignores RunSettings
- **File**: `src/ReqChecker.Infrastructure/Execution/RetryPolicy.cs:23-26`
- **Problem**: Backoff strategy and retry delay are hardcoded. Comment says "use defaults" but settings exist.
- **Evidence**: Lines 24-26 hardcode `retryDelayMs = 5000` and `backoffStrategy = None`
- **Fix**: Pull retry settings from RunSettings or test definition

## Architecture Recommendations

### AR1: Navigation Abstraction
- Create `INavigationService` interface
- Implement ViewModel factory pattern
- Eliminate `App.Services` static usage
- Register all ViewModels with explicit constructor dependencies

### AR2: Infrastructure Service Extraction
- Move file operations to `IProfileStorageService` in Infrastructure
- Move clipboard operations to existing `IClipboardService`
- Keep ViewModels as orchestration-only

### AR3: AppState Lifecycle Management
- Define `IAppState` as application-level service with clear lifecycle
- Add disposal hooks for event subscriptions
- Consider weak events for cross-component communication

### AR4: Single Composition Root
- Consolidate all DI registration in `App.xaml.cs`
- Register all ViewModels with explicit constructor dependencies
- Remove property injection patterns

### AR5: Test Coverage
- Add unit tests for ViewModels (profile import/validation, results filtering)
- Add integration tests for profile migration pipeline
- Add tests for credential handling edge cases

## Out of Scope

- UI/UX changes (covered by 002-ui-ux-redesign)
- New features
- Performance optimizations not related to identified issues
- Test framework changes

## Success Criteria

1. **Stability**: No crashes from exception handlers; theme toggle works correctly
2. **Correctness**: Test config opens correct test; exports have unique IDs; migrations run
3. **Security**: Credentials not exposed to other users; credentials not in serializable data
4. **Maintainability**: ViewModels don't perform I/O; no event subscription leaks
5. **Architecture**: Clear layering; testable components; single composition root

## Dependencies

- Existing ReqChecker.App, ReqChecker.Core, ReqChecker.Infrastructure projects
- No new external dependencies required

## Risks

| Risk | Mitigation |
|------|------------|
| Breaking changes to DI registration | Test all view navigation paths after changes |
| Credential storage changes affect existing credentials | Document migration path; handle missing credentials gracefully |
| Event unsubscription breaks functionality | Test profile switching and navigation thoroughly |

## References

- Codex Code Review Report (source of validated issues)
- MVVM Best Practices: https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm
- WPF Dispatcher: https://learn.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher
