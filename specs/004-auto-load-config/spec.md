# Feature Specification: Auto-Load Bundled Configuration

**Feature Branch**: `004-auto-load-config`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "My company when client complain about any system issue, it will send ReqChecker app with tests configuration file so when user opens app load tests automatically"

## Overview

When support teams assist clients experiencing system issues, they need to send ReqChecker as a diagnostic tool with pre-configured tests specific to the client's environment or issue. Currently, clients must manually load configuration files after opening the app. This feature enables automatic loading of bundled configuration files when the application starts, reducing client friction and ensuring the correct diagnostics run immediately.

## Clarifications

### Session 2026-01-30

- Q: How do support teams bundle client-specific configurations with the app? → A: Config file placed alongside executable (support adds file before sending)
- Q: What should the standard filename be for the bundled configuration? → A: `startup-profile.json`
- Q: What happens on subsequent app launches when startup file still exists? → A: Always auto-load when `startup-profile.json` exists (consistent behavior)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Auto-Load Bundled Configuration on Startup (Priority: P1)

As a client receiving ReqChecker from support, I want the application to automatically load the bundled test configuration when I open it, so I can immediately run diagnostics without any manual setup steps.

**Why this priority**: This is the core feature - without auto-loading, clients must navigate file dialogs and find configuration files, which creates friction and potential for errors. Immediate test availability is the primary value proposition.

**Independent Test**: Can be fully tested by launching an app with a bundled config file and verifying tests appear ready to run without user intervention.

**Acceptance Scenarios**:

1. **Given** the application is distributed with a configuration file in a designated location (e.g., alongside the executable), **When** the user launches the application, **Then** the tests from that configuration are automatically loaded and visible in the test list.

2. **Given** the application has a bundled configuration file, **When** the user launches the application, **Then** no file selection dialog appears and no manual steps are required to view the configured tests.

3. **Given** the bundled configuration file is valid, **When** the application loads it on startup, **Then** the user sees the profile name and test count displayed in the UI confirming successful load.

---

### User Story 2 - Graceful Handling When No Bundled Config Exists (Priority: P2)

As a user opening ReqChecker without a bundled configuration, I want the application to start normally and show the profile selector, so I can manually choose or import a profile as before.

**Why this priority**: Important fallback behavior - the application must work correctly even when not bundled with a config, supporting the standard use case of manually selecting profiles.

**Independent Test**: Can be tested by launching the app without any bundled config file and verifying normal profile selection workflow.

**Acceptance Scenarios**:

1. **Given** no bundled configuration file exists, **When** the user launches the application, **Then** the standard profile selector view is displayed.

2. **Given** no bundled configuration file exists, **When** the user launches the application, **Then** no error messages are shown regarding missing configuration.

---

### User Story 3 - Handle Invalid or Corrupted Bundled Config (Priority: P3)

As a user receiving ReqChecker with a corrupted or invalid configuration file, I want to see a clear error message and be able to use the application manually, so I can either contact support or import a valid configuration.

**Why this priority**: Error recovery is important but secondary - most distributed configs should be valid. Users need a path forward when things go wrong.

**Independent Test**: Can be tested by placing an invalid JSON file in the bundled config location and verifying the app shows an appropriate error and falls back to manual mode.

**Acceptance Scenarios**:

1. **Given** the bundled configuration file is corrupted or invalid JSON, **When** the user launches the application, **Then** a user-friendly error message explains the configuration could not be loaded.

2. **Given** the bundled configuration file fails to load, **When** the error is displayed, **Then** the user is given the option to select a different profile or import a new configuration.

3. **Given** the bundled configuration file has an unsupported schema version, **When** the application attempts to load it, **Then** the error message indicates the configuration format is not compatible with this version of the application.

---

### User Story 4 - Sample Diagnostic Profile Included with Application (Priority: P2)

As a support team member preparing ReqChecker for a client, I want a pre-configured sample diagnostic profile included with the application, so I can quickly customize it for the client's specific environment instead of creating profiles from scratch.

**Why this priority**: This accelerates support workflows - having a ready-made template with common diagnostic tests means support teams spend less time configuring and more time helping clients.

**Independent Test**: Can be tested by verifying the sample profile exists, contains useful diagnostic tests, and can be exported/renamed to `startup-profile.json`.

**Acceptance Scenarios**:

1. **Given** a fresh installation of ReqChecker, **When** the support team opens the application, **Then** a sample diagnostic profile named "Sample Diagnostics" is available in the bundled profiles list.

2. **Given** the sample diagnostic profile exists, **When** the support team views its contents, **Then** it contains a set of common diagnostic tests (connectivity checks, system info gathering, etc.).

3. **Given** the sample diagnostic profile, **When** the support team exports it, **Then** they can rename it to `startup-profile.json` and place it alongside the executable for client distribution.

4. **Given** the sample diagnostic profile, **When** the support team needs client-specific tests, **Then** they can duplicate and modify the sample profile to add custom endpoints or configurations.

---

### Edge Cases

- What happens when the bundled configuration file is empty (0 bytes)?
- What happens when the bundled configuration file exists but has no tests defined?
- What happens when the user has both a bundled configuration and existing user profiles - which takes precedence on startup?
- What happens if file permissions prevent reading the bundled configuration?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST check for a file named `startup-profile.json` in the application's directory when the application starts.
- **FR-002**: System MUST automatically load and display the bundled configuration if one is found and valid.
- **FR-003**: System MUST skip the profile selector screen when a bundled configuration is successfully loaded.
- **FR-004**: System MUST display the standard profile selector screen when no bundled configuration is found.
- **FR-005**: System MUST display a clear error message when a bundled configuration exists but cannot be loaded (invalid format, corrupted, unsupported version).
- **FR-006**: System MUST allow users to proceed to manual profile selection after a bundled configuration load failure.
- **FR-007**: System MUST validate the bundled configuration against the current schema before displaying tests.
- **FR-008**: System MUST treat empty configuration files (0 bytes or no tests) as "no bundled configuration" and show the profile selector.
- **FR-009**: System MUST log configuration loading events (success, failure, not found) for troubleshooting purposes.
- **FR-010**: System MUST auto-load `startup-profile.json` on every application launch when the file is present, regardless of previous sessions or user profile selections.
- **FR-011**: Application MUST include a pre-configured sample diagnostic profile named "Sample Diagnostics" as a bundled profile.
- **FR-012**: The sample diagnostic profile MUST contain a representative set of common diagnostic tests that demonstrate the application's capabilities.
- **FR-013**: The sample diagnostic profile MUST be exportable using the existing profile export functionality.
- **FR-014**: The sample diagnostic profile MUST use placeholder/example values that are clearly identifiable and safe to run (e.g., public test endpoints).

### Key Entities

- **Bundled Configuration**: A test profile file placed alongside the application executable that contains pre-configured tests for client diagnostics. Uses the same profile schema as user-created profiles.
- **Configuration Location**: The same directory as the application executable. The application looks for a file named `startup-profile.json` in its directory on startup.
- **Auto-Load State**: Application state tracking whether a bundled config was loaded, allowing the UI to skip or show the profile selector accordingly.
- **Sample Diagnostic Profile**: A pre-configured profile bundled with the application containing common diagnostic tests. Serves as a template for support teams to customize for specific clients.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users receiving ReqChecker with a bundled configuration can see tests ready to run within 3 seconds of launching the application, without any manual interaction.
- **SC-002**: 100% of valid bundled configurations load successfully on first app launch.
- **SC-003**: When bundled configuration fails to load, users can proceed to manual profile selection within 2 clicks.
- **SC-004**: Zero increase in application startup time for scenarios without bundled configuration (no noticeable delay checking for config).
- **SC-005**: Support teams can prepare and distribute ReqChecker bundles to clients in under 5 minutes.
- **SC-006**: The sample diagnostic profile is visible to 100% of users on first application launch.
- **SC-007**: Support teams can export and customize the sample profile without technical documentation.

## Assumptions

- The bundled configuration file will use the same JSON profile format already supported by the application.
- The designated configuration location is the same directory as the application executable.
- Support teams prepare bundles by: (1) exporting a profile using existing functionality, (2) renaming it to `startup-profile.json`, (3) placing it alongside the executable, and (4) packaging/sending to the client (e.g., as a zip file).
- Bundled configuration takes precedence over user profiles on startup (user can switch profiles after launch if needed).
- The application already supports profile loading, validation, and migration - this feature reuses those capabilities.

## Out of Scope

- Automatic test execution on startup (tests auto-load but user still initiates the run).
- Remote configuration fetching or cloud-based configuration distribution.
- Multiple bundled configurations (only one auto-load config supported).
- Configuration file encryption or digital signing for this initial implementation.
- Modifications to the profile creation or export workflow.
- Client-specific customization UI (support teams customize profiles manually or via text editor).
- Profile version management or update mechanisms for the sample profile.
