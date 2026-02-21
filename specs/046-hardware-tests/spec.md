# Feature Specification: SystemRam & CpuCores Hardware Tests

**Feature Branch**: `046-hardware-tests`
**Created**: 2026-02-21
**Status**: Draft
**Input**: User description: "I need to add new test SystemRam / CpuCores — hardware minimums are in every deployment guide"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Verify System RAM Meets Deployment Minimum (Priority: P1)

An IT administrator loading a deployment readiness profile wants to verify that the target machine has enough physical RAM to run the software being deployed. The profile author specifies a minimum RAM threshold (e.g., 8 GB). When the test runs, it detects the machine's total installed RAM and compares it against the threshold. The result shows the detected RAM, the required minimum, and whether the machine passes or fails.

When no minimum is specified, the test runs in informational mode — it reports the installed RAM and always passes, letting the administrator see hardware specs without enforcing a constraint.

**Why this priority**: RAM is the most common hardware minimum in deployment guides and the most frequent cause of performance problems when undersized. This is the core value of the feature.

**Independent Test**: Can be fully tested by creating a profile with a SystemRam test entry and running it. Delivers immediate value by reporting installed RAM and enforcing minimums.

**Acceptance Scenarios**:

1. **Given** a profile with a SystemRam test specifying `minimumGB: 8`, **When** the machine has 16 GB of RAM, **Then** the test passes and the result displays the detected RAM (16 GB), the minimum required (8 GB), and a pass summary.
2. **Given** a profile with a SystemRam test specifying `minimumGB: 32`, **When** the machine has 16 GB of RAM, **Then** the test fails with a clear message indicating the shortfall (16 GB detected, 32 GB required).
3. **Given** a profile with a SystemRam test with no `minimumGB` parameter (or null), **When** the test runs, **Then** it passes in informational mode and displays the detected RAM amount.

---

### User Story 2 - Verify CPU Core Count Meets Deployment Minimum (Priority: P1)

An IT administrator wants to verify that the target machine has enough logical processor cores to run the software. The profile author specifies a minimum core count (e.g., 4 cores). The test detects the machine's logical processor count and compares it against the threshold.

When no minimum is specified, the test runs in informational mode — it reports the core count and always passes.

**Why this priority**: CPU core count is the second most common hardware requirement in deployment guides, alongside RAM. Both are needed for a complete hardware readiness check.

**Independent Test**: Can be fully tested by creating a profile with a CpuCores test entry and running it. Delivers value independently of the SystemRam test.

**Acceptance Scenarios**:

1. **Given** a profile with a CpuCores test specifying `minimumCores: 4`, **When** the machine has 8 logical processors, **Then** the test passes and displays the detected core count (8), the minimum required (4), and a pass summary.
2. **Given** a profile with a CpuCores test specifying `minimumCores: 16`, **When** the machine has 8 logical processors, **Then** the test fails with a clear message indicating the shortfall (8 detected, 16 required).
3. **Given** a profile with a CpuCores test with no `minimumCores` parameter (or null), **When** the test runs, **Then** it passes in informational mode and displays the detected core count.

---

### User Story 3 - Default Profile Includes Hardware Tests (Priority: P2)

When a user opens ReqChecker with the bundled default profile, they see SystemRam and CpuCores test entries among the existing diagnostics. This lets users immediately see their hardware specs without needing to create a custom profile.

**Why this priority**: Bundled profile entries provide out-of-the-box discoverability and let users see the new test types without any configuration work. Lower priority than the core test logic itself.

**Independent Test**: Can be verified by launching the application and checking that the default profile lists both hardware tests with sensible defaults.

**Acceptance Scenarios**:

1. **Given** the user launches ReqChecker with no custom profile, **When** they view the default profile's test list, **Then** they see three SystemRam entries (informational, low threshold, unreachable threshold) and three CpuCores entries (informational, low threshold, unreachable threshold).
2. **Given** the user runs the default profile, **When** all hardware tests execute, **Then** the informational entries pass with detected values displayed, the low-threshold entries pass with threshold comparison shown, and the unreachable-threshold entries fail with a clear shortfall message.

---

### Edge Cases

- What happens when the machine reports 0 logical processors? The CpuCores test should still report the value and pass in informational mode (or fail if a minimum is set and not met). Zero is unlikely but should not crash.
- What happens when `minimumGB` is set to 0 or a negative number? The SystemRam test should treat this as a configuration error (always-pass if 0, error if negative).
- What happens when `minimumCores` is set to 0 or a negative number? Same handling as `minimumGB`.
- What happens when the RAM detection returns a fractional amount (e.g., 15.9 GB due to reserved memory)? The comparison should use the actual detected value without rounding. The display should show a reasonable precision (e.g., 1 decimal place).
- How does conditional build filtering interact with these test types? Both `SystemRam` and `CpuCores` must be registered in the TestManifest.props so that `IncludeTests=SystemRam` works correctly, and `filter-profile.ps1` strips them when excluded.

## Clarifications

### Session 2026-02-21

- Q: Should CpuCores detect logical processors (includes hyperthreading) or physical cores only? → A: Logical processors (includes hyperthreading — a 4C/8T CPU reports 8).
- Q: How many profile entries per test type in bundled profiles? → A: 3 each — informational + low threshold (expected pass) + unreachable threshold (expected fail), matching the OsVersion/InstalledSoftware pattern.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a `SystemRam` test type that detects the machine's total installed physical RAM and reports it in the test result evidence.
- **FR-002**: The `SystemRam` test MUST accept an optional `minimumGB` parameter. When set, the test passes only if detected RAM meets or exceeds the minimum; when absent or null, the test passes in informational mode.
- **FR-003**: System MUST provide a `CpuCores` test type that detects the machine's logical processor count (including hyperthreading — e.g., a 4-core/8-thread CPU reports 8) and reports it in the test result evidence.
- **FR-004**: The `CpuCores` test MUST accept an optional `minimumCores` parameter. When set, the test passes only if the detected core count meets or exceeds the minimum; when absent or null, the test passes in informational mode.
- **FR-005**: Both test types MUST follow the existing test execution pattern: stopwatch timing, evidence dictionary, human-readable summary, and proper error categorization.
- **FR-006**: Both test types MUST be registered in the conditional build manifest so they can be selectively included or excluded in customer builds.
- **FR-007**: The bundled default profile MUST include three entries for each new test type: (a) informational mode (no threshold), (b) a low threshold expected to pass on most machines (e.g., 4 GB RAM, 2 cores), and (c) an unreachable threshold expected to fail (e.g., 1024 GB RAM, 256 cores) — matching the OsVersion/InstalledSoftware pattern.
- **FR-008**: The bundled sample-diagnostics profile MUST include three entries for each new test type, following the same informational/pass/fail pattern as the default profile.
- **FR-009**: Both test results MUST include a human-friendly summary (e.g., "16.0 GB RAM detected (minimum: 8 GB)") and technical evidence data (detected value, threshold, pass/fail reason).
- **FR-010**: The SystemRam test MUST report RAM in gigabytes (GB) with one decimal place of precision in the display.

### Key Entities

- **SystemRam Test**: Detects total physical RAM. Parameters: `minimumGB` (optional, decimal). Evidence: detected RAM in GB, threshold, pass/fail.
- **CpuCores Test**: Detects logical processor count. Parameters: `minimumCores` (optional, integer). Evidence: detected core count, threshold, pass/fail.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Both SystemRam and CpuCores tests execute and return results within 2 seconds on any supported machine (these are local hardware queries with no network dependency).
- **SC-002**: When a minimum threshold is specified and the machine meets it, the test reports Pass with a summary showing both the detected value and the threshold.
- **SC-003**: When a minimum threshold is specified and the machine does not meet it, the test reports Fail with a clear message showing the shortfall.
- **SC-004**: When no threshold is specified, the test reports Pass with the detected hardware value displayed in informational mode.
- **SC-005**: Both test types appear in the default and sample-diagnostics bundled profiles, and these profiles load and pass validation without errors.
- **SC-006**: Both test types are selectable in filtered/customer builds via the existing `IncludeTests` mechanism.
