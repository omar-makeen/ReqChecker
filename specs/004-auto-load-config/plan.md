# Implementation Plan: Auto-Load Bundled Configuration

**Branch**: `004-auto-load-config` | **Date**: 2026-01-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-auto-load-config/spec.md`

## Summary

Enable automatic loading of a `startup-profile.json` configuration file placed alongside the application executable. When present, the app bypasses the profile selector and directly loads the bundled tests. This reduces friction for clients receiving ReqChecker from support teams with pre-configured diagnostics. Additionally, include a sample diagnostic profile as a bundled resource for support teams to use as a template.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection
**Storage**: File system (startup-profile.json alongside executable), Embedded resources (sample profile)
**Testing**: xUnit with Moq
**Target Platform**: Windows x64 (WPF desktop application)
**Project Type**: Desktop WPF application with layered architecture (Core, Infrastructure, App)
**Performance Goals**: Profile detection and load within 100ms; no noticeable startup delay when file absent
**Constraints**: Must not break existing profile loading workflow; sample profile must use safe public endpoints
**Scale/Scope**: Single configuration file per installation; affects startup flow only

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is a template (not yet customized for this project). No violations possible. Proceeding with standard best practices:

- [x] Reuse existing profile loading infrastructure (IProfileLoader, IProfileValidator, ProfileMigrationPipeline)
- [x] Maintain layered architecture (Core interfaces, Infrastructure implementations, App startup logic)
- [x] Follow existing MVVM patterns with CommunityToolkit.Mvvm
- [x] Add logging via existing Serilog configuration
- [x] Test coverage for new startup path

## Project Structure

### Documentation (this feature)

```text
specs/004-auto-load-config/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # N/A - no new APIs
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Core/
│   ├── Interfaces/
│   │   └── IStartupProfileService.cs    # NEW: Interface for startup profile detection
│   └── Models/
│       └── (existing Profile.cs)
├── ReqChecker.Infrastructure/
│   └── Profile/
│       └── StartupProfileService.cs     # NEW: Implementation of startup profile detection
└── ReqChecker.App/
    ├── App.xaml.cs                      # MODIFY: Check for startup profile on launch
    ├── Profiles/
    │   └── sample-diagnostics.json      # NEW: Sample diagnostic profile (embedded resource)
    └── Services/
        └── NavigationService.cs         # MODIFY: Add method to navigate directly with profile

tests/
├── ReqChecker.Infrastructure.Tests/
│   └── Profile/
│       └── StartupProfileServiceTests.cs  # NEW: Unit tests for startup profile service
└── ReqChecker.App.Tests/
    └── Integration/
        └── StartupProfileIntegrationTests.cs  # NEW: Integration tests for startup flow
```

**Structure Decision**: Follows existing layered architecture. New interface in Core, implementation in Infrastructure, startup logic in App. Sample profile as embedded resource matching existing pattern.

## Complexity Tracking

No violations requiring justification. Design reuses existing infrastructure:
- Profile loading: Existing `IProfileLoader`
- Validation: Existing `IProfileValidator`
- Migration: Existing `ProfileMigrationPipeline`
- Navigation: Existing `NavigationService`
