# Feature Specification: InstalledSoftware Test

**Feature Branch**: `043-installed-software-test`
**Created**: 2026-02-20
**Status**: Draft
**Input**: User description: "I need to add new test InstalledSoftware to answer 'is X installed?' — the #1 readiness question"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Check That a Named Application Is Installed (Priority: P1)

A readiness engineer configures a profile test entry with a software name (e.g., "Python" or "Microsoft .NET Runtime"). When the test runs, the system searches the local machine's installed-program registry for a matching entry by display name. The test passes if the software is found and fails if it is not. The results panel shows the installed version and install location when found, or a clear "not found" message when absent.

**Why this priority**: "Is X installed?" is the single most common readiness question. This story delivers the core value with no optional parameters — just a name to search for.

**Independent Test**: Can be fully tested by adding a profile entry with `softwareName: "Microsoft Edge"` (universally present on Windows) and running the test. A pass result with version and location confirms the feature works end-to-end.

**Acceptance Scenarios**:

1. **Given** a test entry with `softwareName` set to an application that IS installed, **When** the test executes, **Then** the test passes and the summary shows the software name and installed version.
2. **Given** a test entry with `softwareName` set to an application that is NOT installed, **When** the test executes, **Then** the test fails with a clear message stating the software was not found.
3. **Given** a test entry with `softwareName` left empty or null, **When** the test executes, **Then** the test fails with a configuration error explaining that `softwareName` is required.

---

### User Story 2 - Enforce a Minimum Version Requirement (Priority: P2)

A readiness engineer additionally specifies a `minimumVersion` (e.g., "3.10.0"). The test still searches for the software by name, but after finding it, it compares the installed version against the minimum. The test passes only if the installed version is equal to or greater than the required minimum. If the software is found but the version is too old, the test fails with a message showing both the installed and required versions.

**Why this priority**: After confirming presence, version gating is the next most valuable check. It prevents "installed but outdated" surprises during deployment.

**Independent Test**: Can be tested by configuring a test with `softwareName: "Microsoft Edge"` and `minimumVersion: "1.0.0"` (should pass) and then `minimumVersion: "999.0.0"` (should fail with version comparison details).

**Acceptance Scenarios**:

1. **Given** a test entry with `softwareName` and `minimumVersion` where the installed version meets or exceeds the minimum, **When** the test executes, **Then** the test passes and the summary confirms the version meets the requirement.
2. **Given** a test entry with `softwareName` and `minimumVersion` where the installed version is below the minimum, **When** the test executes, **Then** the test fails with a message showing the installed version and the required minimum.
3. **Given** a test entry with `minimumVersion` in an invalid format (e.g., "abc"), **When** the test executes, **Then** the test fails with a configuration error describing the expected version format.
4. **Given** a test entry with `minimumVersion` but the software is not found at all, **When** the test executes, **Then** the test fails with a "not found" message (not a version mismatch message).

---

### User Story 3 - Informational Mode (Priority: P3)

A readiness engineer wants to simply report what version of a software is installed without enforcing any requirement. When `softwareName` is provided but `minimumVersion` is null, the test runs in informational mode: it passes if the software is found and reports the details. This is useful for audit reports and environment snapshots.

**Why this priority**: This is a convenience mode. The P1 story already covers the "found/not found" case; this story emphasizes that the absence of `minimumVersion` is an intentional informational mode, not an oversight.

**Independent Test**: Configure a test with `softwareName: "Microsoft Edge"` and no `minimumVersion`. Verify the test passes and the summary shows the installed version without mentioning any requirement.

**Acceptance Scenarios**:

1. **Given** a test entry with `softwareName` and no `minimumVersion`, **When** the software is found, **Then** the test passes and the summary shows "{softwareName} {version} installed" with no mention of a minimum requirement.

---

### Edge Cases

- What happens when multiple installed programs match the search name (e.g., "Python" matching "Python 3.10" and "Python 3.12")? The test should select the match with the highest version number as the primary result (used for pass/fail and summary). The evidence should include all matches found.
- What happens when the installed software has no version information in the registry? The test should still pass (software is present) but report "unknown" for the version. If `minimumVersion` was specified, the test should fail because the version cannot be verified.
- What happens when the software name search is case-sensitive? The search MUST be case-insensitive (e.g., "python" matches "Python 3.12.0").
- What happens when the registry contains entries with empty or null display names? These entries should be silently skipped during the search.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an `InstalledSoftware` test type that searches the local machine for installed software by display name
- **FR-002**: System MUST search the Windows registry uninstall keys (both 64-bit and 32-bit hive paths, plus the current-user hive) for matching entries
- **FR-003**: System MUST perform case-insensitive substring matching on the `DisplayName` registry value against the configured `softwareName` parameter
- **FR-004**: When multiple registry entries match the `softwareName`, system MUST select the entry with the highest parseable version as the primary match (used for pass/fail decision and summary). All matches MUST be included in the evidence data.
- **FR-005**: System MUST support an optional `minimumVersion` parameter; when provided, the test passes only if the primary match's version is equal to or greater than the specified minimum
- **FR-006**: System MUST fail with a configuration error if `softwareName` is empty, null, or whitespace
- **FR-007**: System MUST fail with a configuration error if `minimumVersion` is provided but is not a valid version string (expected format: major.minor or major.minor.patch or major.minor.patch.revision)
- **FR-008**: System MUST populate test evidence with structured data including: matched display name, installed version, install location, publisher, and install date (when available)
- **FR-009**: System MUST display the software details in the results panel technical details section (an "[Installed Software]" section showing name, version, location, and publisher)
- **FR-010**: System MUST display a human-readable summary line: "{name} {version} installed" (informational), "{name} {version} meets minimum {min}" (version check pass), or "{name} not found" / "{name} {version} does not meet minimum {min}" (failure cases)
- **FR-011**: System MUST assign an appropriate icon and color category for the `InstalledSoftware` type in the test list UI
- **FR-012**: System MUST include sample `InstalledSoftware` test entries in the default profile for common software (e.g., "Microsoft Edge" as a universally-present baseline)
- **FR-013**: System MUST NOT require administrator privileges for the registry-based search

### Key Entities

- **InstalledSoftware Test Entry**: A profile test configuration with type "InstalledSoftware", containing parameters `softwareName` (required string) and `minimumVersion` (optional version string). Configured via the standard profile JSON format with field policies.
- **Installed Program Record**: A discovered software entry from the registry containing display name, version, install location, publisher, and install date. Serialized as JSON evidence in the test result.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The "Is X installed?" question is answerable in a single test execution — users see a clear pass/fail result within 2 seconds
- **SC-002**: 100% of software registered in Windows Add/Remove Programs is discoverable by the test (both 64-bit and 32-bit entries)
- **SC-003**: Version comparison correctly handles all standard version formats (2-part, 3-part, and 4-part version strings) with no false positives or negatives
- **SC-004**: The test executes without requiring administrator elevation on a standard Windows workstation

## Clarifications

### Session 2026-02-20

- Q: When multiple registry entries match, which one is used for the pass/fail decision and version comparison? → A: Highest-version match — pick the match with the greatest version number so that if any installed version satisfies the minimum, the machine is considered ready.

## Assumptions

- The primary detection method is the Windows registry uninstall keys. Software installed via non-standard methods (e.g., portable apps, xcopy deployments) will not be detected unless they register in the standard uninstall registry hive.
- Version comparison uses standard numeric segment comparison (e.g., "3.10.0" > "3.9.0"). Pre-release suffixes (e.g., "-beta") are not supported in `minimumVersion` comparisons.
- The `softwareName` parameter uses substring matching (not exact match) to be forgiving with long display names like "Python 3.12.0 (64-bit)".
