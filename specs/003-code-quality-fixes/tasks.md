# Tasks: Code Quality Fixes and Architecture Improvements

**Input**: Validated issues from spec.md, implementation approach from plan.md
**Prerequisites**: plan.md, spec.md

**Organization**: Tasks grouped by phase and priority. Each task references the issue ID from spec.md.

## Format: `[ID] [P?] [IssueRef] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[IssueRef]**: Which issue this task addresses (e.g., H1, M3)
- Include exact file paths in descriptions

## Path Conventions

- **App source**: `src/ReqChecker.App/`
- **Infrastructure source**: `src/ReqChecker.Infrastructure/`
- **Core source**: `src/ReqChecker.Core/`
- **Tests**: `tests/ReqChecker.App.Tests/`

---

## Phase 1: Critical Bug Fixes (High Priority)

**Purpose**: Fix crash-inducing and functionally broken code - MUST complete first

- [ ] T001 [H1] Fix DispatcherUnhandledException to call Shutdown() after error dialog in src/ReqChecker.App/App.xaml.cs
- [ ] T002 [H2] Fix CurrentDomain_UnhandledException to marshal UI work to Dispatcher and terminate cleanly in src/ReqChecker.App/App.xaml.cs
- [ ] T003 [H3] Remove Click="ThemeToggleButton_Click" from theme toggle button in src/ReqChecker.App/MainWindow.xaml (keep Command binding)
- [ ] T004 [H3] Remove ThemeToggleButton_Click method from src/ReqChecker.App/MainWindow.xaml.cs
- [ ] T005 [H4] Update NavigationService.NavigateToTestConfig to accept TestDefinition parameter in src/ReqChecker.App/Services/NavigationService.cs
- [ ] T006 [H4] Update TestListViewModel.NavigateToTestConfig to pass selected test to navigation in src/ReqChecker.App/ViewModels/TestListViewModel.cs
- [ ] T007 [H5] Inject NavigationService and DialogService into ResultsViewModel constructor in src/ReqChecker.App/ViewModels/ResultsViewModel.cs
- [ ] T008 [H5] Update ResultsViewModel DI registration to include NavigationService and DialogService in src/ReqChecker.App/App.xaml.cs

**Checkpoint**: App terminates cleanly on errors; theme toggles once; test config opens correct test; export buttons work

---

## Phase 2: Data Integrity Fixes

**Purpose**: Fix data integrity and migration issues

- [ ] T009 [M1] Set RunId = Guid.NewGuid().ToString("N") when building RunReport in src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs
- [ ] T010 [M2] Update CurrentSchemaVersion from 1 to 2 in src/ReqChecker.Infrastructure/Profile/ProfileMigrationPipeline.cs
- [ ] T011 [M3] Implement ConverterParameter="Invert" handling in src/ReqChecker.App/Converters/CountToVisibilityConverter.cs
- [ ] T012 [P] [M4] Create NullToVisibilityConverter in src/ReqChecker.App/Converters/NullToVisibilityConverter.cs
- [ ] T013 [M4] Update CredentialPromptDialog.xaml to use NullToVisibilityConverter for CredentialRef and ErrorMessage in src/ReqChecker.App/Views/CredentialPromptDialog.xaml
- [ ] T014 [M4] Register NullToVisibilityConverter as resource in src/ReqChecker.App/App.xaml or CredentialPromptDialog.xaml
- [ ] T015 [M5] Add ValidatesOnExceptions=True and UpdateSourceTrigger=LostFocus to Timeout TextBox in src/ReqChecker.App/Views/TestConfigView.xaml
- [ ] T016 [M5] Add ValidatesOnExceptions=True and UpdateSourceTrigger=LostFocus to RetryCount TextBox in src/ReqChecker.App/Views/TestConfigView.xaml

**Checkpoint**: Reports have unique RunId; V1 profiles migrate; visibility converters work correctly; input validation present

---

## Phase 3: Security Fixes

**Purpose**: Fix credential handling and security issues

- [ ] T017 [M9] Change CRED_PERSIST_LOCAL_MACHINE to CRED_PERSIST_ENTERPRISE in src/ReqChecker.Infrastructure/Security/WindowsCredentialProvider.cs
- [ ] T018 [M9] Add constant for CRED_PERSIST_ENTERPRISE (0x00000003) in WindowsCredentialProvider.NativeMethods
- [ ] T019 [M9] Zero password byte arrays after use in StoreCredentialsAsync in src/ReqChecker.Infrastructure/Security/WindowsCredentialProvider.cs
- [ ] T020 [M9] Zero password byte arrays after use in GetCredentialsAsync in src/ReqChecker.Infrastructure/Security/WindowsCredentialProvider.cs
- [ ] T021 [M10] Create TestExecutionContext class to hold transient credentials in src/ReqChecker.Infrastructure/Execution/TestExecutionContext.cs
- [ ] T022 [M10] Update ITest.ExecuteAsync to accept TestExecutionContext in src/ReqChecker.Core/Interfaces/ITest.cs
- [ ] T023 [M10] Update all test implementations to use TestExecutionContext instead of Parameters in src/ReqChecker.Infrastructure/Tests/
- [ ] T024 [M10] Update SequentialTestRunner to pass credentials via TestExecutionContext, not Parameters in src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs

**Checkpoint**: Credentials stored per-user; password bytes zeroed; credentials not in Parameters dictionary

---

## Phase 4: Architecture Improvements

**Purpose**: Fix architectural violations and memory leaks

### Logging Improvements

- [ ] T025 [M6] Add Serilog.Log.Warning for swallowed exceptions in LoadBundledProfilesAsync in src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs
- [ ] T026 [M6] Add Serilog.Log.Warning for swallowed exceptions in LoadUserProfilesAsync in src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs

### Profile Storage Service Extraction

- [ ] T027 [P] [M7] Create IProfileStorageService interface in src/ReqChecker.App/Services/IProfileStorageService.cs
- [ ] T028 [P] [M7] Create ProfileStorageService implementation in src/ReqChecker.Infrastructure/Profile/ProfileStorageService.cs
- [ ] T029 [M7] Register ProfileStorageService in DI container in src/ReqChecker.App/App.xaml.cs
- [ ] T030 [M7] Update ProfileSelectorViewModel to use IProfileStorageService for file operations in src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs

### ViewModel Disposal

- [ ] T031 [P] [M8] Add IDisposable to MainViewModel with event unsubscription in src/ReqChecker.App/ViewModels/MainViewModel.cs
- [ ] T032 [P] [M8] Add IDisposable to TestListViewModel with event unsubscription in src/ReqChecker.App/ViewModels/TestListViewModel.cs
- [ ] T033 [P] [M8] Add IDisposable to ProfileSelectorViewModel if it subscribes to events in src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs
- [ ] T034 [M8] Update NavigationService to call Dispose on ViewModels when navigating away in src/ReqChecker.App/Services/NavigationService.cs

**Checkpoint**: Profile errors logged; file ops in Infrastructure; no ViewModel memory leaks

---

## Phase 5: Low Priority Improvements

**Purpose**: Clean up remaining issues

- [ ] T035 [P] [L2] Move CredentialMasker to src/ReqChecker.Core/Utilities/CredentialMasker.cs (or document decision to keep in Infrastructure)
- [ ] T036 [P] [L2] Update CredentialMaskConverter to import from Core in src/ReqChecker.App/Converters/CredentialMaskConverter.cs
- [ ] T037 [P] [L3] Update PassRate comment in RunSummary to match implementation (passed/total) in src/ReqChecker.Core/Models/RunSummary.cs
- [ ] T038 [L4] Update RetryPolicy to read delay/strategy from TestDefinition or RunSettings in src/ReqChecker.Infrastructure/Execution/RetryPolicy.cs

**Checkpoint**: Clean layer dependencies; accurate documentation; configurable retry

---

## Phase 6: Testing

**Purpose**: Add test coverage for fixed functionality

- [ ] T039 [P] Add unit tests for MainViewModel disposal in tests/ReqChecker.App.Tests/ViewModels/MainViewModelTests.cs
- [ ] T040 [P] Add unit tests for TestListViewModel disposal in tests/ReqChecker.App.Tests/ViewModels/TestListViewModelTests.cs
- [ ] T041 [P] Add unit tests for ResultsViewModel filtering in tests/ReqChecker.App.Tests/ViewModels/ResultsViewModelTests.cs
- [ ] T042 [P] Add unit tests for ProfileSelectorViewModel import validation in tests/ReqChecker.App.Tests/ViewModels/ProfileSelectorViewModelTests.cs
- [ ] T043 Add integration tests for profile migration (V1 to V2) in tests/ReqChecker.Infrastructure.Tests/Profile/ProfileMigrationTests.cs
- [ ] T044 Add tests for CountToVisibilityConverter with Invert parameter in tests/ReqChecker.App.Tests/Converters/CountToVisibilityConverterTests.cs
- [ ] T045 Add tests for NullToVisibilityConverter in tests/ReqChecker.App.Tests/Converters/NullToVisibilityConverterTests.cs

**Checkpoint**: All tests pass; regression coverage for fixed bugs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Critical)**: No dependencies - MUST complete first
- **Phase 2 (Data Integrity)**: Can start after Phase 1; can run parallel with Phase 3
- **Phase 3 (Security)**: Can start after Phase 1; can run parallel with Phase 2
- **Phase 4 (Architecture)**: Depends on Phase 1 completion
- **Phase 5 (Low Priority)**: Can start any time after Phase 1
- **Phase 6 (Testing)**: Depends on all implementation phases

### Task Dependencies Within Phases

**Phase 1**:
- T001, T002 can run in parallel (same file, different methods)
- T003 depends on T004 (or vice versa - remove both together)
- T005 must complete before T006
- T007 must complete before T008

**Phase 2**:
- T012 can run in parallel with T009-T011
- T013, T014 depend on T012
- T015, T016 can run in parallel

**Phase 3**:
- T017, T018 must run together
- T019, T020 can run in parallel
- T021 must complete before T022, T023, T024
- T022, T023 can run in parallel
- T024 depends on T22, T023

**Phase 4**:
- T025, T026 can run in parallel
- T027, T028 can run in parallel
- T029 depends on T028
- T030 depends on T029
- T031, T032, T033 can run in parallel
- T034 depends on T031, T032, T033

**Phase 5**:
- All tasks can run in parallel

**Phase 6**:
- T039-T042 can run in parallel
- T043 depends on T010
- T044 depends on T011
- T045 depends on T012

### Execution Flow Diagram

```
Phase 1 ─────────────────────────────────────────────────────────────────────┐
    │                                                                         │
    ├──── T001 [H1] ───┐                                                      │
    │                  │ (App.xaml.cs - exception handlers)                   │
    ├──── T002 [H2] ───┘                                                      │
    │                                                                         │
    ├──── T003 [H3] ───┐                                                      │
    │                  │ (MainWindow - theme toggle)                          │
    ├──── T004 [H3] ───┘                                                      │
    │                                                                         │
    ├──── T005 [H4] ──── T006 [H4]  (Navigation - test config)                │
    │                                                                         │
    └──── T007 [H5] ──── T008 [H5]  (ResultsViewModel DI)                     │
                                                                              │
    │ CHECKPOINT: Critical bugs fixed                                         │
    ▼                                                                         │
┌─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   Phase 2 (Data)              Phase 3 (Security)                            │
│       │                           │                                         │
│   ├── T009 [M1]               ├── T017 [M9] ─┐                              │
│   ├── T010 [M2]               ├── T018 [M9] ─┘                              │
│   ├── T011 [M3]               ├── T019 [M9]                                 │
│   │                           ├── T020 [M9]                                 │
│   ├── T012 [M4] ──┐           │                                             │
│   │               │           ├── T021 [M10] ───┐                           │
│   ├── T013 [M4] ──┤           │                 │                           │
│   ├── T014 [M4] ──┘           ├── T022 [M10] ───┤                           │
│   │                           ├── T023 [M10] ───┤                           │
│   ├── T015 [M5]               └── T024 [M10] ───┘                           │
│   └── T016 [M5]                                                             │
│                                                                             │
└──────────────────────────┬──────────────────────────────────────────────────┘
                           │
                           ▼
                    Phase 4 (Architecture)
                           │
    ├──── T025 [M6] ───┐
    │                  │ (Logging)
    ├──── T026 [M6] ───┘
    │
    ├──── T027 [M7] ───┐
    │                  │ (Profile Storage Service)
    ├──── T028 [M7] ───┤
    │                  │
    ├──── T029 [M7] ───┤
    │                  │
    └──── T030 [M7] ───┘
    │
    ├──── T031 [M8] ───┐
    │                  │ (ViewModel Disposal)
    ├──── T032 [M8] ───┤
    │                  │
    ├──── T033 [M8] ───┤
    │                  │
    └──── T034 [M8] ───┘
                           │
                           ▼
                    Phase 5 (Low Priority)
                           │
    ├──── T035 [L2] ─┐
    ├──── T036 [L2] ─┤ (All parallel)
    ├──── T037 [L3] ─┤
    └──── T038 [L4] ─┘
                           │
                           ▼
                    Phase 6 (Testing)
                           │
    ├──── T039-T042 (Parallel unit tests)
    ├──── T043 (Migration integration test)
    ├──── T044 (CountToVisibility test)
    └──── T045 (NullToVisibility test)
```

---

## Notes

- **[P]** tasks can run in parallel (different files)
- **[IssueRef]** maps task to spec.md issue for traceability
- Commit after each phase or logical group
- Test app startup and all navigation paths after each phase
- Run existing tests after each phase to catch regressions
- Phase 3 changes may require re-storing existing credentials (document in release notes)
