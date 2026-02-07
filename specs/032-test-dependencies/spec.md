# Feature Specification: Test Dependencies / Skip-on-Fail

**Feature Branch**: `032-test-dependencies`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "If a Ping test fails, the 5 HTTP tests targeting that host will also fail — wasting time and cluttering results. Allow defining dependsOn: 'test-001' so dependent tests auto-skip with a clear reason."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Auto-Skip Dependent Tests on Prerequisite Failure (Priority: P1)

A profile author defines that certain tests depend on a prerequisite test (e.g., five HTTP tests depend on a Ping connectivity check). When the user runs the test suite and the prerequisite Ping test fails, all five dependent HTTP tests are automatically skipped instead of being executed. Each skipped test displays a clear reason indicating which prerequisite failed, saving the user time and producing a cleaner, more actionable results report.

**Why this priority**: This is the core value proposition. Without auto-skip, users waste time watching doomed tests run and must mentally filter noise from predictable failures. This single capability eliminates the problem.

**Independent Test**: Can be fully tested by creating a profile with one Ping test and several HTTP tests that declare a dependency on it. Simulate a Ping failure and verify the dependent tests are skipped with the correct reason message.

**Acceptance Scenarios**:

1. **Given** a profile where tests B, C, D declare `dependsOn: "A"` and test A fails, **When** the user runs the suite, **Then** tests B, C, D are each skipped with status "Skipped" and a reason message stating "Skipped: prerequisite test 'A' (Network Connectivity Check) failed."
2. **Given** a profile where test B depends on test A and test A passes, **When** the user runs the suite, **Then** test B executes normally.
3. **Given** a profile where test C depends on test B which depends on test A, and test A fails, **When** the user runs the suite, **Then** both B and C are skipped — B because A failed directly, and C because its prerequisite B was skipped.

---

### User Story 2 - View Dependency Relationships in Test List (Priority: P2)

Before running tests, the user can see which tests have dependencies. Each dependent test shows a visual indicator (e.g., a label or badge) naming its prerequisite. This helps users understand test relationships at a glance without reading the raw profile configuration.

**Why this priority**: Visibility of dependencies helps users understand why certain tests were skipped and builds confidence in the system's behavior. However, the skip logic (P1) delivers value even without this UI enhancement.

**Independent Test**: Can be tested by loading a profile with dependency declarations and verifying the test list displays dependency indicators on the correct tests.

**Acceptance Scenarios**:

1. **Given** a profile where test B declares `dependsOn: "A"`, **When** the user views the test list, **Then** test B shows a visual indicator that it depends on test A (referencing test A's display name).
2. **Given** a profile where a test has no dependencies, **When** the user views the test list, **Then** no dependency indicator is shown for that test.

---

### User Story 3 - Clear Skip Reason in Results (Priority: P3)

After a test run where some tests were skipped due to dependency failures, the user can view the results and immediately understand why each test was skipped. The skip reason includes the name of the failed prerequisite test, distinguishing dependency-skips from other skip reasons (e.g., "requires admin" skips).

**Why this priority**: Enhances the post-run experience. The skip itself (P1) prevents wasted time, but a clear explanation in results reduces confusion and support questions. This builds on P1 with minimal additional effort.

**Independent Test**: Can be tested by running a suite with a failing prerequisite, then inspecting the results page to verify each dependency-skipped test shows a distinct, informative skip reason.

**Acceptance Scenarios**:

1. **Given** a completed run where test B was skipped because prerequisite A failed, **When** the user views the results, **Then** test B's result shows status "Skipped" with a human-readable reason: "Prerequisite test 'Network Connectivity Check' failed."
2. **Given** a completed run where test C was skipped because it requires admin privileges, **When** the user views the results, **Then** test C's skip reason reflects the admin requirement, not a dependency reason.

---

### Edge Cases

- What happens when a test declares a dependency on a test ID that does not exist in the profile? The system should treat the profile as invalid and report a clear validation error at load time, preventing the run.
- What happens when tests form a circular dependency chain (A depends on B, B depends on A)? The system should detect the cycle at profile load time and report a validation error.
- What happens when a dependent test is deselected (via selective run) but its prerequisite is selected? The dependent test is simply not run — no issue.
- What happens when a prerequisite test is deselected but a dependent test is selected? The dependent test should be skipped with a reason indicating its prerequisite was not included in the run.
- What happens when a test depends on a test that was itself skipped (not failed) due to a different reason (e.g., requires admin)? The dependent test should also be skipped, since a skipped prerequisite means the precondition was not verified.
- What happens when a test has multiple dependencies and only some fail? The test should be skipped if any one of its prerequisites failed or was skipped.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST support an optional `dependsOn` field on each test definition, accepting one or more test IDs that represent prerequisite tests.
- **FR-002**: When a prerequisite test has a status of "Failed" or "Skipped", the system MUST skip all tests that depend on it instead of executing them.
- **FR-003**: Each dependency-skipped test MUST record a human-readable skip reason that names the failed or skipped prerequisite test (using the prerequisite's display name).
- **FR-004**: The system MUST support transitive dependencies — if A fails, B (depends on A) is skipped, and C (depends on B) is also skipped.
- **FR-005**: The system MUST validate dependency references at profile load time and report an error if a `dependsOn` value references a test ID that does not exist in the profile. Validation errors MUST be displayed as an inline banner on the test list page.
- **FR-006**: The system MUST detect circular dependency chains at profile load time and report a validation error displayed as an inline banner on the test list page. There is no limit on dependency chain depth; only cycles are invalid.
- **FR-007**: When a prerequisite test is not included in a selective test run but a dependent test is selected, the system MUST skip the dependent test with a reason indicating the prerequisite was not part of the run.
- **FR-008**: The test list view MUST display a visual dependency indicator on tests that have prerequisites, showing the display name of the prerequisite test(s).
- **FR-009**: Tests with no `dependsOn` field (or an empty list) MUST behave exactly as they do today — no behavior change for existing profiles.
- **FR-012**: The bundled sample profile MUST include `dependsOn` declarations on existing HTTP tests referencing the Ping connectivity test, so the dependency feature is demonstrable out-of-the-box.
- **FR-010**: Dependency-skipped tests MUST be distinguishable from tests skipped for other reasons (e.g., requires admin) in the results view.
- **FR-011**: If a dependent test appears before its prerequisite in the profile execution order, the system MUST skip the dependent with a reason indicating the prerequisite has not yet been executed. The system MUST NOT reorder tests.

### Key Entities

- **Test Dependency**: A relationship where one test (the dependent) requires another test (the prerequisite) to pass before it can meaningfully execute. Defined via the prerequisite's test ID.
- **Skip Reason**: A human-readable explanation attached to a skipped test result, indicating why it was not executed. For dependency skips, this includes the prerequisite test's display name and its failure/skip status.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: When a prerequisite test fails, all dependent tests are skipped within the same run — zero dependent tests execute against a known-failed precondition.
- **SC-002**: Users can identify why a test was skipped in under 5 seconds by reading the skip reason in the results view.
- **SC-003**: Existing profiles without dependency declarations continue to work identically — zero regressions in behavior.
- **SC-004**: Profile validation catches 100% of invalid dependency references (missing IDs, circular chains) before any tests execute.
- **SC-005**: A test run with 1 failed prerequisite and 5 dependent tests completes noticeably faster than running all 6 tests to completion.
- **SC-006**: Automated unit tests cover the core dependency skip scenarios: prerequisite failure triggers skip, transitive dependency chains propagate skips, out-of-order dependencies are skipped, and all-pass runs execute every test normally.
- **SC-007**: Automated unit tests cover profile dependency validation: missing dependency ID is rejected, circular dependency chain is rejected, and valid dependency declarations are accepted.

## Clarifications

### Session 2026-02-07

- Q: Should the system enforce a maximum dependency chain depth? → A: No limit — allow any depth; validate cycles only.
- Q: If a dependent test is listed before its prerequisite in the profile, what should happen? → A: Skip with reason — skip the dependent and explain the prerequisite hasn't executed yet.
- Q: How should profile validation errors (missing dependency IDs, circular chains) be presented? → A: Inline banner on the test list page with error details.
- Q: What automated test coverage is expected for the dependency skip logic? → A: Unit tests for the runner covering skip-on-fail, transitive skip, out-of-order skip, and all-pass (no skip) scenarios.
- Q: Should profile dependency validation (circular deps, missing IDs) have dedicated unit tests? → A: Yes — unit tests for missing ID detection, circular chain detection, and valid profile acceptance.
- Q: Should the sample startup profile include dependency declarations for out-of-the-box testing? → A: Yes — add dependsOn to existing HTTP tests in the sample profile pointing to the Ping test.

## Assumptions

- Dependencies are declared in the profile configuration by the profile author, not by end users at runtime.
- A test can depend on multiple prerequisites (all must pass for the dependent to run).
- Dependencies are evaluated based on the execution results within the current run, not historical results from previous runs.
- The execution order defined in the profile is expected to list prerequisites before their dependents. If a dependent test appears before its prerequisite in the list, the dependent is skipped with a reason indicating the prerequisite has not yet executed. The system does not reorder tests.
