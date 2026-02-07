# Feature Specification: Re-run Failed Tests Only

**Feature Branch**: `034-rerun-failed-tests`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Feature: Re-run Failed Tests Only. Why It Matters: After fixing an issue, users must re-run the entire suite. A 'Re-run Failed' button on the Results page saves significant time, especially with slow network tests."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Re-run Failed Tests from Results Page (Priority: P1)

After running a test suite and seeing some failures, the user wants to quickly re-run only the failed tests without navigating back to the test list, re-selecting tests, and launching a new run manually.

The user clicks a "Re-run Failed" button on the Results page. The application automatically identifies all failed tests from the current run report, sets them as the selected tests, and navigates to the Run Progress page to execute only those tests.

**Why this priority**: This is the core feature — the single action that saves the most time. Without this, the user must navigate back, manually deselect passing tests, and re-run. For suites with slow network tests (DNS, TCP, HTTP), this can waste minutes per iteration.

**Independent Test**: Can be fully tested by running a profile with a mix of passing and failing tests, then clicking "Re-run Failed" on the Results page. Delivers immediate value by reducing re-test time to only the failed subset.

**Acceptance Scenarios**:

1. **Given** a completed test run with at least one failed test, **When** the user clicks "Re-run Failed", **Then** the application navigates to the Run Progress page and executes only the previously failed tests.
2. **Given** a completed test run with at least one failed test, **When** the user clicks "Re-run Failed", **Then** the previously passing and skipped tests are not re-executed.
3. **Given** a completed test run where all tests passed, **When** the user views the Results page, **Then** the "Re-run Failed" button is not visible or is disabled.
4. **Given** a completed test run with failed tests, **When** the user clicks "Re-run Failed" and the re-run completes, **Then** the new Results page shows only the results of the re-run (not merged with the previous run).

---

### User Story 2 - Re-run Includes Dependency-Skipped Tests (Priority: P2)

When a test fails and other tests depend on it (via the `dependsOn` feature), those dependent tests are automatically skipped. The user expects that re-running failed tests also includes the tests that were skipped due to dependency failures, since the root cause may now be resolved.

**Why this priority**: Without this, re-running only the failed test would pass, but the dependent tests that were skipped would remain untested. The user would need a second manual run to verify the full dependency chain.

**Independent Test**: Can be tested by running a profile where test A fails and test B (which depends on A) is skipped. Clicking "Re-run Failed" should re-run both A and B.

**Acceptance Scenarios**:

1. **Given** a completed run where test A failed and test B was skipped because it depends on A, **When** the user clicks "Re-run Failed", **Then** both test A and test B are included in the re-run.
2. **Given** a completed run where a test was skipped due to user cancellation (not a dependency failure), **When** the user clicks "Re-run Failed", **Then** that cancelled-skipped test is not included in the re-run.

---

### Edge Cases

- What happens when the underlying profile has been unloaded or changed since the original run?
  - The re-run uses the currently loaded profile. If a failed test ID no longer exists in the current profile, it is silently excluded. If no valid tests remain after filtering, the user is informed and no re-run starts.
- What happens if all failed tests pass on re-run — can the user re-run again?
  - The new Results page reflects the re-run results. If all tests now pass, the "Re-run Failed" button is hidden/disabled since there are no new failures.
- What happens if the user cancels the re-run midway?
  - Standard cancellation behavior applies. The Results page shows partial results from the re-run.
- What happens if there is exactly one failed test?
  - The button still appears and works — it re-runs that single test.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Results page MUST display a "Re-run Failed" action when the current run report contains at least one failed test.
- **FR-002**: The "Re-run Failed" action MUST be hidden or disabled when no tests in the current run report have a failed status.
- **FR-003**: When activated, the "Re-run Failed" action MUST identify all tests with a "Fail" status from the current run report.
- **FR-004**: The "Re-run Failed" action MUST also include tests that were skipped due to a dependency on a failed test.
- **FR-005**: The "Re-run Failed" action MUST set the identified test IDs as the selected tests and navigate to the test execution flow.
- **FR-006**: The re-run MUST use the existing test execution infrastructure (same runner, same progress tracking, same result reporting).
- **FR-007**: The re-run results MUST be displayed as a standalone run report (not merged with the previous run).
- **FR-008**: The "Re-run Failed" button MUST use the PrimaryButton style (gradient background, white text) to distinguish it as the primary corrective action, visually elevated above the secondary export buttons on the Results page.
- **FR-009**: The "Re-run Failed" button MUST display an ArrowRepeatAll24 Fluent icon with 8px right margin before the label text, consistent with the existing icon-text pattern used by all action buttons on the Results page.
- **FR-010**: The "Re-run Failed" button MUST be positioned after the "Back to Tests" navigation button and before the export buttons (JSON, CSV, PDF), creating a clear visual hierarchy: navigation → primary action → utilities.
- **FR-011**: The "Re-run Failed" button MUST animate in with the page header using the existing 300ms fade + translateY entrance animation, ensuring a cohesive page-load experience.

### Key Entities

- **RunReport**: Contains the list of test results from a completed run, including each test's status (Pass, Fail, Skipped) and its test ID.
- **TestResult**: Represents an individual test outcome; the `Status` and `TestId` fields are used to identify which tests to re-run.
- **SelectedTestIds**: The mechanism for passing a subset of test IDs to the test execution flow. Null means "run all"; a list means "run only these."

## Clarifications

### Session 2026-02-07

- Q: What button style should "Re-run Failed" use? → A: PrimaryButton (gradient background, white text) — the primary corrective action on the Results page.
- Q: What icon should the "Re-run Failed" button use? → A: ArrowRepeatAll24 Fluent icon — universally recognized retry/repeat symbol.
- Q: Where should the "Re-run Failed" button be placed? → A: After "Back to Tests", before export buttons — navigation → primary action → utilities hierarchy.
- Q: How should the button animate on page load? → A: Animate with the header using the existing 300ms fade + translateY entrance animation.

## Assumptions

- The existing selective test execution mechanism (storing selected test IDs in shared application state, then navigating to the execution page) is reused. No new execution path is needed.
- "Skipped due to dependency" can be determined by examining whether a skipped test's `dependsOn` list references a test that failed in the same run.
- The button label "Re-run Failed" is sufficiently clear; no tooltip or additional explanation is needed.
- The re-run uses the same run settings (timeout, retry count, admin behavior) as configured in the current profile.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can re-run only failed tests with a single click from the Results page, without navigating to other pages first.
- **SC-002**: Re-run execution time is proportional only to the number of failed (and dependency-skipped) tests, not the full suite.
- **SC-003**: The "Re-run Failed" button is not shown when all tests have passed, preventing user confusion.
- **SC-004**: 100% of failed tests and their dependency-skipped dependents are included in the re-run — no manual selection required.
