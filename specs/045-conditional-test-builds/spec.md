# Feature Specification: Conditional Test Builds

**Feature Branch**: `045-conditional-test-builds`
**Created**: 2026-02-21
**Status**: Draft
**Input**: User description: "MSBuild conditional compilation to build app with only customer-specific test types, controlled via CI/CD GitHub Actions"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build App With Selected Tests Only (Priority: P1)

As a build engineer, I want to produce a customer-specific build that includes only the test types that customer needs, so the delivered application is as small as possible and contains no unnecessary functionality.

**Why this priority**: This is the core value proposition. Without the ability to select which tests are compiled into the binary, the entire feature has no purpose.

**Independent Test**: Can be fully tested by running a publish command with a test selection parameter and verifying the resulting binary only contains the specified test types.

**Acceptance Scenarios**:

1. **Given** the solution is configured for conditional builds, **When** a developer runs a publish command specifying only Http and Tcp test types, **Then** only HttpTest and TcpPortOpenTest are compiled into the output binary, and no other test type classes exist in the assembly.
2. **Given** the solution is configured for conditional builds, **When** a developer runs a publish command with no test selection parameter, **Then** all test types are included (backwards-compatible default).
3. **Given** a build with only Http and Tcp tests, **When** the application starts, **Then** only Http and Tcp tests appear in the UI and profile, and no errors occur from missing test types.

---

### User Story 2 - CI/CD Workflow for Customer Builds (Priority: P2)

As a build engineer, I want a CI/CD workflow that accepts a list of test types as input and produces a customer-specific build artifact, so I can automate delivery for each customer without manual intervention.

**Why this priority**: Automation is essential for managing multiple customers at scale — manually editing build files per customer is error-prone and unsustainable with 150+ test types.

**Independent Test**: Can be fully tested by triggering the CI/CD workflow with a test type list and verifying the produced artifact contains only the specified tests.

**Acceptance Scenarios**:

1. **Given** the CI/CD workflow exists, **When** a user triggers it with a list of test types (e.g., Http, Tcp, DnsResolution), **Then** the workflow produces a published build artifact containing only those three test types.
2. **Given** the CI/CD workflow exists, **When** a user triggers it with a special "all" value or leaves the input empty, **Then** the workflow produces a full build with all test types included.
3. **Given** the CI/CD workflow exists, **When** a user provides an invalid test type name, **Then** the build fails with a clear error message identifying the unrecognized test type.

---

### User Story 3 - Default Profile Matches Included Tests (Priority: P2)

As a user of a customer-specific build, I want the default profile to only show tests that are actually available in my build, so I don't see test entries that cannot run.

**Why this priority**: If the default profile references test types that weren't compiled in, the user experience is confusing — tests would appear in the list but fail or be invisible.

**Independent Test**: Can be fully tested by building with a subset of tests and verifying the bundled default profile only contains entries for those test types.

**Acceptance Scenarios**:

1. **Given** a build includes only Http and Tcp test types, **When** the default profile is loaded, **Then** it contains only test entries of type Http and Tcp.
2. **Given** a build includes all test types, **When** the default profile is loaded, **Then** it contains entries for every available test type (same as current behavior).

---

### User Story 4 - Build Validation of Test Type Names (Priority: P3)

As a build engineer, I want the build system to validate that every test type name I specify actually exists, so typos and outdated names are caught at build time rather than silently producing incomplete builds.

**Why this priority**: Without validation, a misspelled test type would be silently ignored, leading to a build that's missing a required test — a subtle and hard-to-diagnose issue.

**Independent Test**: Can be fully tested by running a build with a deliberately misspelled test type and verifying the build fails with a descriptive error.

**Acceptance Scenarios**:

1. **Given** the build system has a registry of valid test type names, **When** a build is run with an invalid test type name, **Then** the build fails with an error listing the invalid name and all valid options.
2. **Given** the build system has a registry, **When** a new test type is added to the codebase, **Then** it must also be added to the registry or the build fails — ensuring the registry stays in sync.

---

### Edge Cases

- What happens when a profile JSON file references a test type that was not included in the build? The application must gracefully ignore unknown test types and optionally log a warning.
- What happens when zero test types are specified? The build must fail with a clear error — an app with no tests is not useful.
- What happens when a test type has a dependency (via `dependsOn`) on a test type that was excluded from the build? The build must warn, since the dependency cannot be satisfied at runtime.
- What happens when a test type has shared dependencies (e.g., a base class or utility used by multiple test types)? Shared infrastructure code must always be included regardless of test selection.

## Clarifications

### Session 2026-02-21

- Q: Should UI converter entries (icon, color, result details) for excluded test types also be conditionally compiled out? → A: No. Converters keep all entries since they have safe fallback defaults. Only test class source files in the Infrastructure project are conditionally excluded.
- Q: How should missing manifest entries for new test files be caught? → A: Build must fail if a test file exists in the Tests folder but has no corresponding manifest entry (automatic detection).
- Q: How should customer-specific build artifacts be named? → A: Named by customer (e.g., ReqChecker-Acme-v1.0.zip). CI/CD workflow accepts an optional customer name input.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The build system MUST support a parameter that accepts a semicolon-separated list of test type identifiers (e.g., Http;Tcp;DnsResolution).
- **FR-002**: When the test selection parameter is specified, only the test class source files (in the Infrastructure project) for the listed test types MUST be compiled into the output assembly. All other test class source files MUST be excluded. UI code (converters, icons, colors) MUST remain intact since it uses safe fallback defaults.
- **FR-003**: When the test selection parameter is not specified or is empty, all test types MUST be included (full build, backwards compatible).
- **FR-004**: The build system MUST validate every name in the selection parameter against a known registry of test type identifiers and MUST fail the build if an unrecognized name is found.
- **FR-005**: The default profile bundled with the application MUST be filtered at build time to include only test entries whose type matches the included test types.
- **FR-006**: The application MUST gracefully handle profile JSON files that reference test types not present in the build, by skipping those entries without crashing.
- **FR-007**: The build system MUST fail if the test selection parameter is explicitly set to an empty value (zero tests selected).
- **FR-008**: The build system MUST emit a warning if a selected test type has a dependsOn reference to a test type that is not included in the build.
- **FR-009**: A CI/CD workflow MUST exist that accepts a test type list and an optional customer name as workflow inputs and produces a published build artifact containing only the specified tests, named by customer (e.g., ReqChecker-Acme-v1.0.zip).
- **FR-010**: The CI/CD workflow MUST support a special value or empty input to produce a full build with all test types.
- **FR-011**: Each test type in the codebase MUST be registered in a central manifest that maps the test type identifier to its source file path(s).
- **FR-012**: The application's runtime test discovery MUST work without code changes — it discovers only the test types that were compiled in.
- **FR-013**: Shared infrastructure code (base classes, utilities, interfaces) used by test types MUST always be included regardless of test selection.
- **FR-014**: The build system MUST fail if a test source file exists in the Tests folder but has no corresponding entry in the manifest, ensuring the manifest stays in sync with the codebase.

### Key Entities

- **Test Type Identifier**: A short, unique name for each test type (e.g., Http, Tcp, DnsResolution, EnvironmentVariable). Used in the selection parameter and the manifest.
- **Test Manifest**: A central file that maps each test type identifier to the source file(s) that implement it. Used by the build system for inclusion/exclusion and validation.
- **Customer Build Configuration**: A combination of the test selection parameter and optionally a customer name, used by CI/CD to produce a specific build variant.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A build specifying 5 test types produces a published output measurably smaller than the full build with all 150+ test types.
- **SC-002**: Building with an invalid test type name fails within seconds with a clear, actionable error message.
- **SC-003**: The full build (no test selection parameter) produces identical output to the current build — zero regressions.
- **SC-004**: A CI/CD workflow run completes end-to-end (trigger to downloadable artifact) within a reasonable time for a customer build with 10 or fewer test types.
- **SC-005**: Adding a new test type to the codebase requires updating exactly one manifest entry — no other build configuration changes needed.
- **SC-006**: The default profile in a customer build contains only entries matching the included test types — zero orphaned entries.
