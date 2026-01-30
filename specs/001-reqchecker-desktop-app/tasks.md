# Tasks: ReqChecker Desktop Application

**Input**: Design documents from `/specs/001-reqchecker-desktop-app/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests are NOT included unless explicitly requested.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md layered architecture:
- **Core**: `src/ReqChecker.Core/`
- **Infrastructure**: `src/ReqChecker.Infrastructure/`
- **App**: `src/ReqChecker.App/`
- **Tests**: `tests/ReqChecker.*.Tests/`

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Create solution structure and configure dependencies

- [X] T001 Create solution file and project structure per plan.md in src/ReqChecker.sln
- [X] T002 [P] Create ReqChecker.Core class library project in src/ReqChecker.Core/ReqChecker.Core.csproj
- [X] T003 [P] Create ReqChecker.Infrastructure class library project in src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj
- [X] T004 [P] Create ReqChecker.App WPF project in src/ReqChecker.App/ReqChecker.App.csproj
- [X] T005 Add project references (Infrastructure→Core, App→Core, App→Infrastructure)
- [X] T006 [P] Install NuGet packages for Infrastructure (Serilog, FluentValidation, FluentFTP, CsvHelper)
- [X] T007 [P] Install NuGet packages for App (WPF-UI, Microsoft.Extensions.Hosting, CommunityToolkit.Mvvm)
- [X] T008 [P] Configure Directory.Build.props with shared settings (nullable, LangVersion 12, TreatWarningsAsErrors)
- [X] T009 [P] Create test projects structure in tests/ (Core.Tests, Infrastructure.Tests, App.Tests)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**Note**: These are shared components used by multiple user stories

### Core Domain Models

- [X] T010 [P] Create TestStatus enum in src/ReqChecker.Core/Enums/TestStatus.cs
- [X] T011 [P] Create FieldPolicyType enum in src/ReqChecker.Core/Enums/FieldPolicyType.cs
- [X] T012 [P] Create ErrorCategory enum in src/ReqChecker.Core/Enums/ErrorCategory.cs
- [X] T013 [P] Create BackoffStrategy enum in src/ReqChecker.Core/Enums/BackoffStrategy.cs
- [X] T014 [P] Create AdminBehavior enum in src/ReqChecker.Core/Enums/AdminBehavior.cs
- [X] T015 [P] Create ProfileSource enum in src/ReqChecker.Core/Enums/ProfileSource.cs
- [X] T016 [P] Create RunSettings model in src/ReqChecker.Core/Models/RunSettings.cs
- [X] T017 [P] Create TestDefinition model in src/ReqChecker.Core/Models/TestDefinition.cs
- [X] T018 [P] Create Profile model in src/ReqChecker.Core/Models/Profile.cs
- [X] T019 [P] Create MachineInfo model in src/ReqChecker.Core/Models/MachineInfo.cs
- [X] T020 [P] Create NetworkInterfaceInfo model in src/ReqChecker.Core/Models/NetworkInterfaceInfo.cs
- [X] T021 [P] Create TestEvidence model in src/ReqChecker.Core/Models/TestEvidence.cs
- [X] T022 [P] Create TimingBreakdown model in src/ReqChecker.Core/Models/TimingBreakdown.cs
- [X] T023 [P] Create TestError model in src/ReqChecker.Core/Models/TestError.cs
- [X] T024 [P] Create TestResult model in src/ReqChecker.Core/Models/TestResult.cs
- [X] T025 [P] Create RunSummary model in src/ReqChecker.Core/Models/RunSummary.cs
- [X] T026 [P] Create RunReport model in src/ReqChecker.Core/Models/RunReport.cs

### Core Interfaces

- [X] T027 [P] Create ITest interface in src/ReqChecker.Core/Interfaces/ITest.cs
- [X] T028 [P] Create ITestRunner interface in src/ReqChecker.Core/Interfaces/ITestRunner.cs
- [X] T029 [P] Create IProfileLoader interface in src/ReqChecker.Core/Interfaces/IProfileLoader.cs
- [X] T030 [P] Create IProfileValidator interface in src/ReqChecker.Core/Interfaces/IProfileValidator.cs
- [X] T031 [P] Create IProfileMigrator interface in src/ReqChecker.Core/Interfaces/IProfileMigrator.cs
- [X] T032 [P] Create ICredentialProvider interface in src/ReqChecker.Core/Interfaces/ICredentialProvider.cs
- [X] T033 [P] Create IExporter interface in src/ReqChecker.Core/Interfaces/IExporter.cs
- [X] T034 [P] Create IIntegrityVerifier interface in src/ReqChecker.Core/Interfaces/IIntegrityVerifier.cs

### Core Services

- [X] T035 Create FieldPolicyEnforcer service in src/ReqChecker.Core/Services/FieldPolicyEnforcer.cs

### Infrastructure Foundation

- [X] T036 Create TestTypeAttribute for test discovery in src/ReqChecker.Infrastructure/Tests/TestTypeAttribute.cs
- [X] T037 Configure Serilog in src/ReqChecker.Infrastructure/Logging/SerilogConfiguration.cs
- [X] T038 Create MachineInfoCollector in src/ReqChecker.Infrastructure/Platform/MachineInfoCollector.cs
- [X] T039 Create AdminPrivilegeChecker in src/ReqChecker.Infrastructure/Platform/AdminPrivilegeChecker.cs

### App Foundation

- [X] T040 Configure App.xaml with WPF-UI theme dictionaries in src/ReqChecker.App/App.xaml
- [X] T041 Setup DI container and application host in src/ReqChecker.App/App.xaml.cs
- [X] T042 Create MainWindow shell with NavigationView in src/ReqChecker.App/MainWindow.xaml
- [X] T043 Create MainViewModel in src/ReqChecker.App/ViewModels/MainViewModel.cs
- [X] T044 Create NavigationService in src/ReqChecker.App/Services/NavigationService.cs
- [X] T045 Create DialogService in src/ReqChecker.App/Services/DialogService.cs
- [X] T046 [P] Create Theme.xaml with custom styles in src/ReqChecker.App/Resources/Styles/Theme.xaml
- [X] T047 [P] Create BoolToVisibilityConverter in src/ReqChecker.App/Converters/BoolToVisibilityConverter.cs

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Run Environment Readiness Tests (Priority: P1) MVP

**Goal**: Execute test suite with real-time progress and display pass/fail results with evidence

**Independent Test**: Launch app, load profile with HTTP GET test, run it, verify pass/fail result displayed with evidence

### Test Implementations (Required for US1)

- [X] T048 [P] [US1] Implement PingTest in src/ReqChecker.Infrastructure/Tests/PingTest.cs
- [X] T049 [P] [US1] Implement HttpGetTest in src/ReqChecker.Infrastructure/Tests/HttpGetTest.cs
- [X] T050 [P] [US1] Implement HttpPostTest in src/ReqChecker.Infrastructure/Tests/HttpPostTest.cs
- [X] T051 [P] [US1] Implement FileExistsTest in src/ReqChecker.Infrastructure/Tests/FileExistsTest.cs
- [X] T052 [P] [US1] Implement DirectoryExistsTest in src/ReqChecker.Infrastructure/Tests/DirectoryExistsTest.cs
- [X] T053 [P] [US1] Implement FileReadTest in src/ReqChecker.Infrastructure/Tests/FileReadTest.cs
- [X] T054 [P] [US1] Implement FileWriteTest in src/ReqChecker.Infrastructure/Tests/FileWriteTest.cs
- [X] T055 [P] [US1] Implement ProcessListTest in src/ReqChecker.Infrastructure/Tests/ProcessListTest.cs
- [X] T056 [P] [US1] Implement RegistryReadTest in src/ReqChecker.Infrastructure/Tests/RegistryReadTest.cs
- [X] T057 [P] [US1] Implement FtpReadTest in src/ReqChecker.Infrastructure/Tests/FtpReadTest.cs
- [X] T058 [P] [US1] Implement FtpWriteTest in src/ReqChecker.Infrastructure/Tests/FtpWriteTest.cs

### Execution Engine

- [X] T059 [US1] Create RetryPolicy with exponential backoff in src/ReqChecker.Infrastructure/Execution/RetryPolicy.cs
- [X] T060 [US1] Implement SequentialTestRunner with cancellation and progress in src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs
- [X] T061 [US1] Register all test types via DI attribute discovery in src/ReqChecker.App/App.xaml.cs (update)

### UI for Test Execution

- [X] T062 [P] [US1] Create TestStatusBadge control in src/ReqChecker.App/Controls/TestStatusBadge.xaml
- [X] T063 [P] [US1] Create TestStatusToColorConverter in src/ReqChecker.App/Converters/TestStatusToColorConverter.cs
- [X] T064 [P] [US1] Create ProgressRing control in src/ReqChecker.App/Controls/ProgressRing.xaml
- [X] T065 [US1] Create TestListViewModel in src/ReqChecker.App/ViewModels/TestListViewModel.cs
- [X] T066 [US1] Create TestListView with test cards in src/ReqChecker.App/Views/TestListView.xaml
- [X] T067 [US1] Create RunProgressViewModel with cancellation support in src/ReqChecker.App/ViewModels/RunProgressViewModel.cs
- [X] T068 [US1] Create RunProgressView with real-time progress in src/ReqChecker.App/Views/RunProgressView.xaml
- [X] T069 [US1] Create ResultsViewModel in src/ReqChecker.App/ViewModels/ResultsViewModel.cs
- [X] T070 [US1] Create ResultsView with pass/fail summary and evidence in src/ReqChecker.App/Views/ResultsView.xaml
- [X] T071 [US1] Wire navigation: TestList → RunProgress → Results in src/ReqChecker.App/Services/NavigationService.cs (update)

**Checkpoint**: User Story 1 complete - can run tests and see results

---

## Phase 4: User Story 2 - Load and Manage Configuration Profiles (Priority: P1)

**Goal**: Load bundled profiles atomically, import user profiles, validate and display in profile selector

**Independent Test**: Launch app, verify bundled profiles auto-load, import user profile, confirm both appear in list

### Profile Loading Infrastructure

- [ ] T072 [US2] Implement JsonProfileLoader in src/ReqChecker.Infrastructure/Profile/JsonProfileLoader.cs
- [ ] T073 [US2] Implement FluentProfileValidator in src/ReqChecker.Infrastructure/Profile/FluentProfileValidator.cs
- [ ] T074 [US2] Implement ProfileMigrationPipeline in src/ReqChecker.Infrastructure/Profile/ProfileMigrationPipeline.cs
- [ ] T075 [US2] Create V1ToV2Migration placeholder in src/ReqChecker.Infrastructure/Profile/Migrations/V1ToV2Migration.cs
- [ ] T076 [US2] Implement HmacIntegrityVerifier in src/ReqChecker.Infrastructure/Profile/HmacIntegrityVerifier.cs

### Bundled Profile

- [ ] T077 [US2] Create default-profile.json with sample tests in src/ReqChecker.App/Profiles/default-profile.json
- [ ] T078 [US2] Create signature file for bundled profile in src/ReqChecker.App/Profiles/default-profile.json.sig
- [ ] T079 [US2] Configure bundled profile as embedded resource in src/ReqChecker.App/ReqChecker.App.csproj (update)

### UI for Profile Management

- [ ] T080 [US2] Create ProfileSelectorViewModel with atomic loading in src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs
- [ ] T081 [US2] Create ProfileSelectorView with profile list and import button in src/ReqChecker.App/Views/ProfileSelectorView.xaml
- [ ] T082 [US2] Add file picker dialog for profile import in src/ReqChecker.App/Services/DialogService.cs (update)
- [ ] T083 [US2] Display validation errors in InfoBar on import failure in src/ReqChecker.App/Views/ProfileSelectorView.xaml (update)
- [ ] T084 [US2] Wire profile selection to TestListView navigation in src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs (update)

**Checkpoint**: User Story 2 complete - profiles load and can be imported

---

## Phase 5: User Story 3 - Review and Edit Test Parameters (Priority: P2)

**Goal**: View test configuration with field-level policy enforcement (Locked/Editable/Hidden/PromptAtRun)

**Independent Test**: Load profile with mixed policies, verify locked fields disabled with icons, editable fields work, hidden fields not shown

### Field Policy UI

- [ ] T085 [P] [US3] Create FieldPolicyToVisibilityConverter in src/ReqChecker.App/Converters/FieldPolicyToVisibilityConverter.cs
- [ ] T086 [P] [US3] Create LockedFieldControl with lock icon and tooltip in src/ReqChecker.App/Controls/LockedFieldControl.xaml
- [ ] T087 [US3] Create TestConfigViewModel with field policy application in src/ReqChecker.App/ViewModels/TestConfigViewModel.cs
- [ ] T088 [US3] Create TestConfigView with dynamic field rendering in src/ReqChecker.App/Views/TestConfigView.xaml
- [ ] T089 [US3] Create CredentialPromptViewModel in src/ReqChecker.App/ViewModels/CredentialPromptViewModel.cs
- [ ] T090 [US3] Create CredentialPromptDialog for PromptAtRun fields in src/ReqChecker.App/Views/CredentialPromptDialog.xaml
- [ ] T091 [US3] Integrate PromptAtRun handling into test execution flow in src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs (update)
- [ ] T092 [US3] Add navigation from TestList to TestConfig on test click in src/ReqChecker.App/ViewModels/TestListViewModel.cs (update)

**Checkpoint**: User Story 3 complete - field policies enforced in UI

---

## Phase 6: User Story 4 - Export Test Results (Priority: P2)

**Goal**: Export run reports to JSON and CSV formats with full evidence

**Independent Test**: Run tests, export to JSON and CSV, verify files contain run ID, timestamps, machine info, and results

### Export Infrastructure

- [ ] T093 [P] [US4] Implement JsonExporter with System.Text.Json in src/ReqChecker.Infrastructure/Export/JsonExporter.cs
- [ ] T094 [P] [US4] Implement CsvExporter with CsvHelper in src/ReqChecker.Infrastructure/Export/CsvExporter.cs
- [ ] T095 [US4] Create JSON serialization context for AOT in src/ReqChecker.Infrastructure/Export/AppJsonContext.cs

### Export UI

- [ ] T096 [US4] Add export commands to ResultsViewModel in src/ReqChecker.App/ViewModels/ResultsViewModel.cs (update)
- [ ] T097 [US4] Add Export buttons (JSON/CSV) to ResultsView in src/ReqChecker.App/Views/ResultsView.xaml (update)
- [ ] T098 [US4] Add save file dialog for export location in src/ReqChecker.App/Services/DialogService.cs (update)

**Checkpoint**: User Story 4 complete - results can be exported

---

## Phase 7: User Story 5 - View Diagnostics and Logs (Priority: P3)

**Goal**: View last run summary, open logs folder, copy diagnostic details to clipboard

**Independent Test**: Run tests, open diagnostics view, verify summary displays, test Open Logs and Copy Details buttons

### Diagnostics Infrastructure

- [ ] T099 [US5] Create ClipboardService in src/ReqChecker.App/Services/ClipboardService.cs

### Diagnostics UI

- [ ] T100 [US5] Create DiagnosticsViewModel with last run data in src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs
- [ ] T101 [US5] Create DiagnosticsView with summary, Open Logs, Copy Details in src/ReqChecker.App/Views/DiagnosticsView.xaml
- [ ] T102 [US5] Add Diagnostics navigation item to MainWindow in src/ReqChecker.App/MainWindow.xaml (update)

**Checkpoint**: User Story 5 complete - diagnostics accessible

---

## Phase 8: User Story 6 - Handle Secrets Securely (Priority: P2)

**Goal**: Resolve credentialRef from Windows Credential Manager, prompt if missing, mask in outputs

**Independent Test**: Configure test with credentialRef, run without stored credential, verify prompt appears, complete test

### Security Infrastructure

- [ ] T103 [US6] Implement WindowsCredentialProvider with P/Invoke in src/ReqChecker.Infrastructure/Security/WindowsCredentialProvider.cs
- [ ] T104 [US6] Implement DpapiProtector for local encryption in src/ReqChecker.Infrastructure/Security/DpapiProtector.cs
- [ ] T105 [US6] Integrate credential resolution into test execution in src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs (update)

### Secret Masking

- [ ] T106 [US6] Add credential masking to JsonExporter in src/ReqChecker.Infrastructure/Export/JsonExporter.cs (update)
- [ ] T107 [US6] Add credential masking to CsvExporter in src/ReqChecker.Infrastructure/Export/CsvExporter.cs (update)
- [ ] T108 [US6] Add credential masking to ResultsView display in src/ReqChecker.App/Views/ResultsView.xaml (update)

**Checkpoint**: User Story 6 complete - secrets handled securely

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

### Accessibility & UX

- [ ] T109 [P] Add keyboard navigation to all views in src/ReqChecker.App/Views/*.xaml (update all)
- [ ] T110 [P] Add focus states and tab order to all interactive elements
- [ ] T111 Implement dark/light theme toggle in settings
- [ ] T112 Add version information display to About section in src/ReqChecker.App/MainWindow.xaml (update)

### Error Handling

- [ ] T113 Add global exception handler in src/ReqChecker.App/App.xaml.cs (update)
- [ ] T114 Add user-friendly error messages for all failure scenarios

### Distribution

- [ ] T115 Configure self-contained publish in src/ReqChecker.App/ReqChecker.App.csproj (update)
- [ ] T116 Create WiX installer project in installer/ReqChecker.wxs
- [ ] T117 Add application icon and branding assets in src/ReqChecker.App/Resources/Icons/

### Validation

- [ ] T118 Run quickstart.md validation - verify all setup steps work
- [ ] T119 Verify all acceptance scenarios from spec.md pass manually

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - US1 and US2 are both P1 priority - complete US1 first (core functionality)
  - US3, US4, US6 are P2 - can proceed after US1+US2
  - US5 is P3 - lowest priority
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

| Story | Depends On | Can Start After |
|-------|------------|-----------------|
| US1 (Run Tests) | Foundational | Phase 2 |
| US2 (Profiles) | Foundational | Phase 2 |
| US3 (Field Policy) | US1, US2 | Phase 4 |
| US4 (Export) | US1 | Phase 3 |
| US5 (Diagnostics) | US1 | Phase 3 |
| US6 (Secrets) | US1 | Phase 3 |

### Within Each User Story

- Models/Enums before Services
- Infrastructure before UI
- ViewModels before Views
- Core implementation before integration

### Parallel Opportunities

**Phase 1 (Setup)**: T002, T003, T004 in parallel, then T006, T007, T008, T009 in parallel

**Phase 2 (Foundational)**:
- All enums (T010-T015) in parallel
- All models (T016-T026) in parallel
- All interfaces (T027-T034) in parallel
- App foundation (T040-T047) in parallel after DI setup

**Phase 3 (US1)**: All 11 test implementations (T048-T058) in parallel

**Phase 4 (US2)**: Profile loading tasks sequential, but can overlap with US1 completion

---

## Parallel Example: Phase 2 Foundation

```text
# Launch all enums together:
T010 Create TestStatus enum
T011 Create FieldPolicyType enum
T012 Create ErrorCategory enum
T013 Create BackoffStrategy enum
T014 Create AdminBehavior enum
T015 Create ProfileSource enum

# Launch all models together (after enums):
T016 Create RunSettings model
T017 Create TestDefinition model
T018 Create Profile model
... (all models in parallel)

# Launch all interfaces together:
T027 Create ITest interface
T028 Create ITestRunner interface
... (all interfaces in parallel)
```

## Parallel Example: Phase 3 Test Implementations

```text
# All test types can be implemented in parallel:
T048 PingTest
T049 HttpGetTest
T050 HttpPostTest
T051 FileExistsTest
T052 DirectoryExistsTest
T053 FileReadTest
T054 FileWriteTest
T055 ProcessListTest
T056 RegistryReadTest
T057 FtpReadTest
T058 FtpWriteTest
```

---

## Implementation Strategy

### MVP First (User Story 1 + 2)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1 (Run Tests)
4. Complete Phase 4: User Story 2 (Profiles)
5. **STOP and VALIDATE**: Test US1+US2 independently
6. Deploy/demo MVP with core test execution and profile loading

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Run Tests) → Core MVP functional
3. Add US2 (Profiles) → Full MVP with profile management
4. Add US3 (Field Policy) → Enterprise feature complete
5. Add US4 (Export) → Reporting capability
6. Add US5 (Diagnostics) → Support tooling
7. Add US6 (Secrets) → Security hardening
8. Polish → Production ready

### Single Developer Flow

Execute phases sequentially in order:
1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9

### Parallel Team Strategy

With 3 developers after Foundational:
- Developer A: US1 (Run Tests) → US4 (Export)
- Developer B: US2 (Profiles) → US3 (Field Policy)
- Developer C: US5 (Diagnostics) → US6 (Secrets)

---

## Notes

- [P] tasks = different files, no dependencies within that batch
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All file paths are relative to repository root
- Total tasks: 119
