# Feature Specification: Fix Run Progress View UI Bugs

**Feature Branch**: `005-fix-run-progress`
**Created**: 2026-01-30
**Status**: Draft
**Input**: User description: "Fix results window issues: 1- 100% ring progress not complete visually 2- 'Currently Running' shows test name but stats show all tests completed 3- Cancel button not responding 4- 'Waiting for results' shown despite completed tests count displayed 5- Check any other issues"

## Overview

The Run Progress view has multiple UI synchronization bugs where different parts of the interface show contradictory states. The progress ring, current test indicator, completion statistics, and results list are not properly synchronized, leading to a confusing user experience. Additionally, the Cancel button does not respond to user input.

## Clarifications

### Session 2026-01-30

- Q: Progress ring arc not updating - what is the exact issue? → A: The visual arc/border of the progress ring remains at its initial state (showing minimal fill) while the percentage text shows 100%. The arc is not updating to reflect actual progress - it should be a full circle when showing 100%.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Accurate Progress Display (Priority: P1)

As a user running diagnostic tests, I want the progress display to accurately reflect the current state of test execution, so I can trust what I see and understand how far along the tests are.

**Why this priority**: Users rely on visual feedback to understand test progress. Contradictory or inaccurate information destroys trust and causes confusion about whether tests are actually running or complete.

**Independent Test**: Run a profile with 4 tests and verify the progress ring percentage matches the actual completion state throughout execution.

**Acceptance Scenarios**:

1. **Given** tests are running, **When** 2 of 4 tests complete, **Then** the progress ring shows 50% and the visual arc reflects 50% completion.

2. **Given** all tests have completed, **When** the final test finishes, **Then** the progress ring shows 100% with a fully complete visual arc (no gap).

3. **Given** tests are running, **When** viewing the progress ring, **Then** the percentage matches the formula: (completed + failed + skipped) / total * 100.

---

### User Story 2 - Synchronized State Display (Priority: P1)

As a user, I want all UI elements to show consistent state information, so I don't see contradictory messages like "Currently Running" alongside "100% Complete".

**Why this priority**: Inconsistent state display is the core bug being reported. When the progress shows 100% but "Currently Running" still displays a test name, users cannot determine the actual state.

**Independent Test**: Run tests to completion and verify the "Currently Running" section changes to reflect completion state.

**Acceptance Scenarios**:

1. **Given** tests are in progress, **When** viewing the "Currently Running" section, **Then** it displays the name of the currently executing test with a spinning indicator.

2. **Given** all tests have completed, **When** viewing the "Currently Running" section, **Then** it either hides, shows "Complete", or transforms to show final status (no test name with spinner).

3. **Given** 3 tests passed and 1 failed (4 total), **When** all tests finish, **Then** the stats (Passed: 3, Failed: 1, Skipped: 0) match the completed count and no "Currently Running" test is shown.

---

### User Story 3 - Results List Display (Priority: P1)

As a user, I want to see completed test results appear in the results list in real-time, so I can track which tests have finished and their outcomes.

**Why this priority**: The "Waiting for results..." empty state persists even when the header shows "Completed Tests: 4", making it impossible to review individual test results.

**Independent Test**: Run tests and verify each completed test appears in the list immediately after it finishes.

**Acceptance Scenarios**:

1. **Given** tests are running, **When** a test completes, **Then** it immediately appears in the "Completed Tests" list with its status (Pass/Fail/Skipped).

2. **Given** the "Completed Tests" header shows count "4", **When** viewing the list area, **Then** exactly 4 test result items are visible (not "Waiting for results...").

3. **Given** no tests have completed yet, **When** viewing the results list, **Then** the "Waiting for results..." empty state is displayed and the header count shows "0".

---

### User Story 4 - Cancel Button Functionality (Priority: P2)

As a user, I want the Cancel button to stop test execution when clicked, so I can abort a test run if needed.

**Why this priority**: While important for user control, this is secondary to fixing the display bugs since users can still wait for tests to complete naturally.

**Independent Test**: Start a test run and click Cancel during execution; verify tests stop and UI reflects cancellation.

**Acceptance Scenarios**:

1. **Given** tests are running, **When** the user clicks the Cancel button, **Then** test execution stops within 2 seconds.

2. **Given** the Cancel button is clicked, **When** execution stops, **Then** the UI shows the run was cancelled (not stuck in "running" state).

3. **Given** all tests have completed, **When** viewing the Cancel button, **Then** it is either disabled or replaced with navigation options (e.g., "Back to Tests", "View Results").

---

### User Story 5 - Post-Completion Actions (Priority: P2)

As a user who has finished running tests, I want clear options for what to do next, so I can navigate to results or return to the test list.

**Why this priority**: After tests complete, users need to navigate somewhere. Without clear actions, they may be stuck on the progress screen.

**Independent Test**: Complete a test run and verify navigation buttons appear.

**Acceptance Scenarios**:

1. **Given** all tests have completed, **When** viewing the progress view, **Then** a "View Results" or "Back to Tests" button is visible and functional.

2. **Given** the run was cancelled, **When** viewing the progress view, **Then** navigation options are available to return to the test list.

---

### Edge Cases

- What happens when a test takes a very long time (timeout)?
- What happens if all tests are skipped (0 passed, 0 failed)?
- What happens if the profile has only 1 test?
- What happens if tests complete very quickly (faster than UI can update)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Progress ring MUST display a percentage that accurately reflects (completed + failed + skipped) / total * 100.
- **FR-002**: Progress ring visual arc (the colored border stroke) MUST update in real-time to match the percentage - at 50% the arc covers half the circle, at 100% the arc forms a complete circle with no gaps.
- **FR-002a**: Progress ring visual arc MUST NOT remain at initial state while percentage text updates - both must be synchronized.
- **FR-003**: "Currently Running" section MUST hide or show completion state when no test is actively executing.
- **FR-004**: "Currently Running" section MUST only display a test name when a test is actively running.
- **FR-005**: Completed Tests list MUST display all finished test results, not show empty state when tests have completed.
- **FR-006**: Completed Tests list items MUST appear immediately when each test finishes (real-time updates).
- **FR-007**: Completed Tests header count MUST match the actual number of items displayed in the list.
- **FR-008**: Cancel button MUST trigger cancellation of the running test execution when clicked.
- **FR-009**: Cancel button MUST be disabled or hidden when no tests are running.
- **FR-010**: UI MUST provide navigation options (View Results, Back to Tests) when test execution completes or is cancelled.
- **FR-011**: All UI elements (progress ring, current test, stats, results list) MUST reflect consistent state at any given moment.

### Key Entities

- **Test Execution State**: Represents the current phase of test execution (Running, Completed, Cancelled). All UI components must bind to a single source of truth for this state.
- **Test Result**: Individual test outcome including display name, status (Pass/Fail/Skipped), and duration. Results must be added to a collection that the UI observes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: When tests complete, 100% of users see consistent state across all UI elements (no contradictory displays).
- **SC-002**: Completed test results appear in the list within 500ms of each test finishing.
- **SC-003**: Cancel button stops execution within 2 seconds of being clicked.
- **SC-004**: Progress ring percentage matches actual completion ratio with 0% margin of error.
- **SC-005**: Zero instances of "Waiting for results..." displayed when Completed Tests count is greater than 0.
- **SC-006**: Users can navigate away from the progress view within 2 clicks after completion.

## Assumptions

- The underlying test runner infrastructure (`SequentialTestRunner`) is working correctly and reporting progress properly.
- The issue is in the UI binding/updating layer, not in the actual test execution logic.
- Standard WPF data binding patterns with `INotifyPropertyChanged` and `ObservableCollection` are being used.
- The progress callback from the test runner is being called on completion of each test.
