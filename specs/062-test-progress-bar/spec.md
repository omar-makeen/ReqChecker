# Feature Specification: Test Progress Bar Enhancements

**Feature Branch**: `062-test-progress-bar`
**Created**: 2026-03-14
**Status**: Draft
**Input**: User description: "Replace the indeterminate test execution spinner with a real progress bar showing test completion count (e.g., 'Running test 3 of 12') and a determinate progress bar. Show the name of the currently executing test. When all tests complete, show the total pass/fail summary briefly before navigating to results. The progress should update in real-time as each test finishes."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Show Sequential Test Position During Execution (Priority: P1)

A user runs their test suite and wants to know exactly where they are in the execution sequence. While the existing progress ring shows a percentage and the current test name, there is no clear "Test 3 of 12" counter. The user should see a prominent sequential position indicator alongside the existing progress visualization so they can estimate remaining time at a glance.

**Why this priority**: The most impactful missing information — users can see percentage but not discrete test position, which is more intuitive for estimating time remaining.

**Independent Test**: Can be tested by running any test suite and verifying a "Test X of Y" counter is visible during execution, incrementing after each test completes.

**Acceptance Scenarios**:

1. **Given** the user has started a test run of 12 tests, **When** the third test begins executing, **Then** the progress area displays "Test 3 of 12" (or equivalent sequential position text).
2. **Given** the user is watching the progress view, **When** each test completes, **Then** the counter increments to reflect the next test position.
3. **Given** the user runs a subset of tests (e.g., 5 of 12 selected), **When** execution is in progress, **Then** the counter reflects the selected count (e.g., "Test 2 of 5"), not the total profile count.

---

### User Story 2 - Auto-Navigate to Results After Completion (Priority: P2)

After all tests finish, the user currently must manually click "View Results" to see the full report. Instead, the system should automatically navigate to the results page after a brief delay, giving the user a moment to see the final pass/fail summary before the transition.

**Why this priority**: Reduces friction in the most common workflow — run tests, review results. Eliminates an unnecessary click.

**Independent Test**: Can be tested by running any test suite, waiting for completion, and verifying automatic navigation to the results page after a brief visible summary.

**Acceptance Scenarios**:

1. **Given** all tests have completed, **When** the completion summary is shown, **Then** the system waits briefly (2–3 seconds) and then automatically navigates to the results page.
2. **Given** all tests have completed and the brief summary is showing, **When** the user clicks "View Results" before the auto-navigation timer expires, **Then** navigation happens immediately (no double-navigation).
3. **Given** all tests have completed and the brief summary is showing, **When** the user clicks "Back to Tests" before the auto-navigation timer expires, **Then** auto-navigation is cancelled and the user goes to the test list instead.
4. **Given** the test run was cancelled (not all tests completed), **When** the cancellation summary is shown, **Then** auto-navigation does NOT occur — the user must manually choose their next action.

---

### User Story 3 - Show Completion Summary with Pass/Fail Counts (Priority: P2)

When all tests complete, the existing completion card shows "All tests completed" and the total count. The summary should additionally show the pass/fail/skip breakdown prominently in the completion card, so the user can immediately assess overall health before navigating to detailed results.

**Why this priority**: Enhances the brief moment between completion and results navigation with actionable information.

**Independent Test**: Can be tested by running tests with a mix of pass/fail outcomes and verifying the completion card shows the breakdown.

**Acceptance Scenarios**:

1. **Given** all tests have completed with 10 passed, 1 failed, and 1 skipped, **When** the completion summary is displayed, **Then** the card shows the pass/fail/skip breakdown (e.g., "10 passed, 1 failed, 1 skipped").
2. **Given** all tests passed, **When** the completion summary is displayed, **Then** the card shows a success-oriented message (e.g., "All 12 tests passed").
3. **Given** some tests failed, **When** the completion summary is displayed, **Then** the failure count is visually emphasized (e.g., highlighted in red/warning color).

---

### Edge Cases

- What happens if only 1 test is in the suite? The counter should display "Test 1 of 1" and auto-navigation should still work after the brief delay.
- What happens if all tests are skipped (e.g., due to dependency failures)? The completion summary should reflect this and auto-navigate normally.
- What happens if the user navigates away during the auto-navigation countdown? The timer should be cancelled to prevent unexpected navigation later.
- What happens if auto-navigation fails (e.g., RunReport is null)? Fall back to existing behavior — stay on completion view, let user click manually.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The progress view MUST display a sequential test position indicator (e.g., "Test X of Y") that updates after each test completes during execution.
- **FR-002**: The sequential position MUST reflect the actual number of tests being run (selected subset or full suite), not the total profile count.
- **FR-003**: After all tests complete (not cancelled), the system MUST automatically navigate to the results page after a brief delay.
- **FR-004**: The auto-navigation delay MUST be long enough for the user to glance at the completion summary (2–3 seconds).
- **FR-005**: The user MUST be able to override auto-navigation by clicking "View Results" (navigate immediately) or "Back to Tests" (cancel auto-navigation).
- **FR-006**: Auto-navigation MUST NOT occur when the test run was cancelled by the user.
- **FR-007**: The completion summary card MUST display the pass/fail/skip breakdown alongside the total count.
- **FR-008**: When all tests pass, the completion summary MUST show a distinct success message (e.g., "All X tests passed").
- **FR-009**: When any tests fail, the failure count MUST be visually distinguished from passing counts (using the existing failure color/style).
- **FR-010**: If the user navigates away before auto-navigation fires, the timer MUST be cancelled.

### Key Entities

- **TestPosition**: A transient display value computed as `CurrentTestIndex` / `TotalTests`, shown as "Test X of Y" during execution. Reset when a new test run starts.
- **AutoNavigationTimer**: A countdown timer that starts when test execution completes. Fires navigation to results after the delay expires. Cancellable by user action or page departure.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of test runs display a sequential "Test X of Y" counter that accurately reflects the current position during execution.
- **SC-002**: After completion, auto-navigation to results occurs within 2–3 seconds without requiring user interaction.
- **SC-003**: Users can override auto-navigation in 100% of cases by clicking either navigation button before the timer expires.
- **SC-004**: The completion summary shows the correct pass/fail/skip breakdown for every completed test run.
- **SC-005**: Auto-navigation never occurs after a cancelled test run.
