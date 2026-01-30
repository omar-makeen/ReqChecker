# Implementation Plan: ReqChecker Desktop Application

**Branch**: `001-reqchecker-desktop-app` | **Date**: 2026-01-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-reqchecker-desktop-app/spec.md`

## Summary

ReqChecker is an enterprise-grade Windows desktop application that validates environment readiness by running configurable system and network tests. The application uses WPF with WPF-UI for a premium Fluent Design interface, implements field-level policy enforcement for company-controlled configurations, and provides comprehensive test results with JSON/CSV export. Key technical differentiators include HMAC-SHA256 profile integrity verification, Windows Credential Manager integration for secrets, and a plugin-like test architecture using dependency injection.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 LTS
**Primary Dependencies**:
- WPF-UI 3.x (Fluent Design UI framework)
- Microsoft.Extensions.DependencyInjection (DI container)
- Microsoft.Extensions.Hosting (application host)
- Serilog (structured logging)
- System.Text.Json (JSON serialization)
- CsvHelper (CSV export)
- FluentValidation (schema validation)

**Storage**: File-based (JSON profiles in AppData, logs in AppData\Logs)
**Testing**: xUnit + FluentAssertions + Moq + WpfTestFramework
**Target Platform**: Windows 10/11 x64 desktop
**Project Type**: Single WPF desktop application with layered architecture
**Performance Goals**:
- UI responsiveness: <100ms for all interactions
- Profile load: <500ms for profiles with 100 tests
- Test execution: No UI freeze during async operations

**Constraints**:
- Self-contained deployment (no .NET runtime required on target)
- Single-file publish preferred
- <100MB installer size
- Offline operation (no network required except for tests that explicitly test network)

**Scale/Scope**:
- Profiles: Up to 200 tests per profile
- Concurrent profiles loaded: Up to 10
- Export size: Up to 10MB JSON, 1MB CSV

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Clean Architecture | PASS | Layered design: Core → Infrastructure → UI |
| Testability | PASS | All services interface-based, DI throughout |
| Security | PASS | No plaintext secrets, DPAPI/Credential Manager |
| Observability | PASS | Serilog structured logging with file sink |
| Simplicity | PASS | Single project, no microservices overhead |

No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/001-reqchecker-desktop-app/
├── plan.md              # This file
├── research.md          # Phase 0 output - technology decisions
├── data-model.md        # Phase 1 output - entity definitions
├── quickstart.md        # Phase 1 output - developer setup guide
├── contracts/           # Phase 1 output - JSON schemas
│   ├── profile-schema.json
│   └── run-report-schema.json
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Core/                    # Domain layer (no external dependencies)
│   ├── Models/                         # Entity classes
│   │   ├── Profile.cs
│   │   ├── TestDefinition.cs
│   │   ├── FieldPolicy.cs
│   │   ├── RunReport.cs
│   │   ├── TestResult.cs
│   │   └── MachineInfo.cs
│   ├── Enums/
│   │   ├── TestStatus.cs
│   │   ├── FieldPolicyType.cs
│   │   └── ErrorCategory.cs
│   ├── Interfaces/                     # Abstractions
│   │   ├── ITest.cs
│   │   ├── ITestRunner.cs
│   │   ├── IProfileLoader.cs
│   │   ├── IProfileValidator.cs
│   │   ├── IProfileMigrator.cs
│   │   ├── ICredentialProvider.cs
│   │   ├── IExporter.cs
│   │   └── IIntegrityVerifier.cs
│   └── Services/                       # Domain services
│       └── FieldPolicyEnforcer.cs
│
├── ReqChecker.Infrastructure/          # Implementation layer
│   ├── Tests/                          # Test type implementations
│   │   ├── PingTest.cs
│   │   ├── HttpGetTest.cs
│   │   ├── HttpPostTest.cs
│   │   ├── FtpReadTest.cs
│   │   ├── FtpWriteTest.cs
│   │   ├── FileExistsTest.cs
│   │   ├── DirectoryExistsTest.cs
│   │   ├── FileReadTest.cs
│   │   ├── FileWriteTest.cs
│   │   ├── ProcessListTest.cs
│   │   └── RegistryReadTest.cs
│   ├── Profile/                        # Profile management
│   │   ├── JsonProfileLoader.cs
│   │   ├── FluentProfileValidator.cs
│   │   ├── ProfileMigrationPipeline.cs
│   │   ├── HmacIntegrityVerifier.cs
│   │   └── Migrations/
│   │       └── V1ToV2Migration.cs
│   ├── Execution/
│   │   ├── SequentialTestRunner.cs
│   │   └── RetryPolicy.cs
│   ├── Security/
│   │   ├── WindowsCredentialProvider.cs
│   │   └── DpapiProtector.cs
│   ├── Export/
│   │   ├── JsonExporter.cs
│   │   └── CsvExporter.cs
│   ├── Logging/
│   │   └── SerilogConfiguration.cs
│   └── Platform/
│       ├── MachineInfoCollector.cs
│       └── AdminPrivilegeChecker.cs
│
├── ReqChecker.App/                     # WPF Application
│   ├── App.xaml(.cs)                   # Application entry, DI setup
│   ├── MainWindow.xaml(.cs)            # Shell window
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   ├── ProfileSelectorViewModel.cs
│   │   ├── TestListViewModel.cs
│   │   ├── TestConfigViewModel.cs
│   │   ├── RunProgressViewModel.cs
│   │   ├── ResultsViewModel.cs
│   │   ├── DiagnosticsViewModel.cs
│   │   └── CredentialPromptViewModel.cs
│   ├── Views/
│   │   ├── ProfileSelectorView.xaml
│   │   ├── TestListView.xaml
│   │   ├── TestConfigView.xaml
│   │   ├── RunProgressView.xaml
│   │   ├── ResultsView.xaml
│   │   ├── DiagnosticsView.xaml
│   │   └── CredentialPromptDialog.xaml
│   ├── Controls/                       # Custom controls
│   │   ├── LockedFieldControl.xaml
│   │   ├── TestStatusBadge.xaml
│   │   └── ProgressRing.xaml
│   ├── Converters/
│   │   ├── FieldPolicyToVisibilityConverter.cs
│   │   ├── TestStatusToColorConverter.cs
│   │   └── BoolToVisibilityConverter.cs
│   ├── Services/
│   │   ├── NavigationService.cs
│   │   ├── DialogService.cs
│   │   └── ClipboardService.cs
│   ├── Resources/
│   │   ├── Styles/
│   │   │   └── Theme.xaml
│   │   └── Icons/
│   └── Profiles/                       # Bundled company profiles
│       └── default-profile.json
│
└── ReqChecker.sln

tests/
├── ReqChecker.Core.Tests/
│   ├── Models/
│   └── Services/
├── ReqChecker.Infrastructure.Tests/
│   ├── Tests/
│   ├── Profile/
│   ├── Execution/
│   └── Export/
└── ReqChecker.App.Tests/
    └── ViewModels/
```

**Structure Decision**: Layered architecture with three projects (Core, Infrastructure, App) following Clean Architecture principles. Core contains pure domain logic with no external dependencies. Infrastructure implements all external concerns (file I/O, network, Windows APIs). App contains WPF-specific code with MVVM pattern.

## Complexity Tracking

No violations requiring justification. The three-project structure is the minimum needed to maintain separation of concerns:
- Core: Must be dependency-free for testability
- Infrastructure: Groups all external integrations
- App: WPF-specific code that can't be in other layers

## Architecture Decisions

### AD-001: WPF-UI for Fluent Design

**Decision**: Use WPF-UI library (https://wpfui.lepo.co/) for the UI framework.

**Rationale**:
- Provides modern Windows 11 Fluent Design out of the box
- Active maintenance and community support
- Native WPF integration (not a wrapper)
- Includes all required controls: NavigationView, InfoBar, ProgressRing, Dialog
- MIT licensed, suitable for commercial use

**Alternatives Rejected**:
- MaterialDesignInXaml: Material Design aesthetic doesn't match Windows enterprise UX
- MahApps.Metro: Dated Metro style, less modern than Fluent
- HandyControl: Less polished Fluent implementation
- Custom styling: Too time-consuming for enterprise-quality result

### AD-002: Test Registration via DI

**Decision**: Register test types using keyed services in DI container with `[TestType("identifier")]` attribute discovery.

**Rationale**:
- Automatic discovery of test implementations at startup
- Clean separation - new tests require no registration code changes
- Testable - mock tests can be injected
- Type-safe resolution by test type identifier

### AD-003: Profile Integrity with HMAC-SHA256

**Decision**: Company-managed profiles include an HMAC-SHA256 signature computed over normalized JSON. Key embedded in obfuscated form in application binary.

**Rationale**:
- Offline verification (no network required)
- Detects any tampering with locked field values
- HMAC prevents length extension attacks
- Obfuscation raises bar for casual tampering (not meant to stop determined attackers)

**Trade-offs**:
- Key extraction is possible with reverse engineering
- Acceptable for preventing accidental/casual modification, not military-grade security

### AD-004: Navigation Model

**Decision**: Single-window application with NavigationView sidebar. Views: Profiles, Tests, Results, Diagnostics.

**Rationale**:
- Familiar Windows 11 settings-style navigation
- Keyboard accessible (arrow keys + Enter)
- Breadcrumb not needed - flat hierarchy
- ContentFrame hosts view pages

### AD-005: Async Test Execution

**Decision**: All test implementations are async. Runner uses `Task.Run` with `ConfigureAwait(false)` and reports progress via `IProgress<T>`. UI binds to observable properties updated on dispatcher.

**Rationale**:
- No UI blocking during network/file operations
- Cancellation via CancellationToken
- Progress reporting decoupled from UI framework
