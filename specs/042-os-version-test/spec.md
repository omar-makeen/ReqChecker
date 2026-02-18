# Feature Specification: OS Version Validation Test

**Feature Branch**: `042-os-version-test`
**Created**: 2026-02-19
**Status**: Draft
**Input**: User description: "I need to add new testOsVersion to Validate Windows version/build number"

## Clarifications

### Session 2026-02-19

- Q: What should happen when both `minimumBuild` and `expectedVersion` are configured simultaneously? → A: Configuration error — reject the test with a clear message asking the user to pick one mode.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Validate Minimum OS Version (Priority: P1)

An IT administrator configures a test profile that includes an OS version check. They specify a minimum required Windows version and build number (e.g., "Windows 10 build 19045 or higher"). When the test suite runs, the OsVersion test reads the current machine's OS version and compares it against the configured requirement. The test passes if the machine meets or exceeds the minimum, and fails with a clear message if it does not.

**Why this priority**: This is the core use case. Most compliance and readiness checks need to verify that a machine is running at least a specific OS version before deploying software or applying configurations.

**Independent Test**: Can be fully tested by adding an OsVersion test entry to a profile and running the suite on any Windows machine. The result clearly indicates whether the machine's OS version meets the configured minimum.

**Acceptance Scenarios**:

1. **Given** a profile with an OsVersion test requiring minimum build 19045, **When** the test runs on a machine with build 22631, **Then** the test passes and displays the detected version in the results.
2. **Given** a profile with an OsVersion test requiring minimum build 22631, **When** the test runs on a machine with build 19045, **Then** the test fails with a validation error explaining the expected vs. actual version.
3. **Given** a profile with an OsVersion test specifying no minimum version, **When** the test runs, **Then** the test passes and reports the detected OS version information for informational purposes.

---

### User Story 2 - Exact Version Match (Priority: P2)

An IT administrator needs to verify that a fleet of machines is running a specific OS version (not just a minimum). They configure the OsVersion test with an exact version requirement. The test fails if the machine's version does not match exactly, enabling strict environment standardisation.

**Why this priority**: Some regulated environments require exact version matching for compliance audits. This extends the core comparison with an additional mode.

**Independent Test**: Can be tested by configuring an OsVersion test with an exact version value and running it on machines with matching and non-matching versions.

**Acceptance Scenarios**:

1. **Given** a profile with an OsVersion test requiring exact version "10.0.22631", **When** the test runs on a machine with version "10.0.22631", **Then** the test passes.
2. **Given** a profile with an OsVersion test requiring exact version "10.0.22631", **When** the test runs on a machine with version "10.0.19045", **Then** the test fails with a validation error showing expected vs. actual.

---

### User Story 3 - Report OS Details in Evidence (Priority: P3)

After the test runs, the user views the results page and sees detailed OS information captured as evidence: the full version string, product name (e.g., "Windows 11 Pro"), build number, and architecture (x64/ARM64). This information is useful for diagnostics and audit trail purposes, regardless of whether the test passed or failed.

**Why this priority**: Evidence collection enhances the diagnostic value of the tool, but the test can function without rich evidence display.

**Independent Test**: Can be tested by running any OsVersion test and inspecting the result details page for the captured OS information.

**Acceptance Scenarios**:

1. **Given** a completed OsVersion test, **When** the user views the result details, **Then** they see the OS product name, full version string, build number, and processor architecture.

---

### Edge Cases

- What happens when the user provides a malformed version string in the configuration (e.g., "abc" or "10.0")?
  The test should fail with a configuration error explaining the expected format.
- What happens when the test runs on a non-Windows platform?
  The application targets Windows only, so this scenario is out of scope. The test may report the detected OS information and fail if a Windows-specific version was expected.
- What happens when no version constraint parameters are specified at all?
  The test should pass and report the detected OS version information (informational mode).
- What happens when the user configures both a minimum build number and an expected version at the same time?
  The test should fail with a configuration error explaining that only one comparison mode may be used at a time.
- What happens when the user cancels the test run while the OsVersion test is executing?
  The test should respect cancellation and report a skipped status, consistent with all other test types.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST recognise "OsVersion" as a valid test type that can be included in test profiles.
- **FR-002**: System MUST read the current machine's Windows version (major.minor.build) when the OsVersion test executes.
- **FR-003**: System MUST support a "minimum build number" parameter; the test passes when the detected build number is greater than or equal to the configured value, and fails otherwise.
- **FR-004**: System MUST support an "expected version" parameter for exact match comparison (format: "major.minor.build"); the test passes only when the detected version matches exactly.
- **FR-005**: System MUST treat the test as informational (always pass) when neither minimum build nor expected version is configured, reporting the detected OS version.
- **FR-006**: System MUST fail with a configuration error when both minimum build number and expected version are configured simultaneously, instructing the user to choose one comparison mode.
- **FR-007**: System MUST fail with a configuration error when the user provides an invalid version string (e.g., non-numeric segments, missing components for exact match).
- **FR-008**: System MUST capture OS evidence including: product name, full version string, build number, and processor architecture.
- **FR-009**: System MUST produce a human-readable summary describing the outcome (e.g., "OS version 10.0.22631 meets minimum build 19045" or "OS version 10.0.19045 does not meet minimum build 22631").
- **FR-010**: System MUST display an appropriate icon and colour for the OsVersion test type in the test list, consistent with existing system-level tests.
- **FR-011**: System MUST include at least one OsVersion test entry in the default profile so new users can see the test type in action.

### Key Entities

- **OsVersion Test Parameters**: Configuration for the test, including optional minimum build number, optional expected version string, and timeout.
- **OS Evidence**: Captured machine information including product name, full version string, build number, and processor architecture.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can add an OsVersion test to a profile and receive a pass/fail result within 1 second of execution (local operation, no network involved).
- **SC-002**: 100% of OsVersion test results display accurate OS version and build information matching the machine's actual configuration.
- **SC-003**: Users can distinguish OsVersion tests from other test types at a glance in the test list via a distinct icon and colour.
- **SC-004**: All existing tests continue to pass without modification after the OsVersion test type is added (zero regressions).

## Assumptions

- The application runs exclusively on Windows, so cross-platform OS detection is out of scope.
- The version string format follows the Windows convention of "major.minor.build" (e.g., "10.0.22631").
- The OsVersion test is a local, non-network operation that completes near-instantly and does not require elevated privileges.
- The "minimum build number" comparison uses only the build component (the third segment of the version string), as this is the most commonly varying and meaningful segment across Windows 10/11 updates.
