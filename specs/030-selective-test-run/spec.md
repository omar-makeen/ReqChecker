# Feature Specification: Selective Test Run

**Feature Branch**: `030-selective-test-run`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Users can only 'Run All.' Large profiles (20+ tests) force running everything even when debugging one failing check. Let users checkbox-select which tests to run."

## Clarifications

### Session 2026-02-07

- Q: How should the per-test selection checkbox be visually integrated into the premium card layout? → A: Accent-styled checkbox embedded inside each test card (left side, before the test type icon), consistent with the app's custom-control design language.
- Q: Where should the "Select All" checkbox be placed? → A: In the list header row, between the test count label and the run button, for a natural and cohesive action bar.
- Q: How should deselected test cards be visually differentiated? → A: Reduced opacity (50-60%) on deselected cards, making selected cards visually prominent. Combined with the unchecked checkbox for a strong dual signal.
- Q: Does clicking the card body toggle selection or navigate to test details? → A: Checkbox click toggles selection; card body click still navigates to test details (existing behavior preserved). Standard list UI pattern avoids breaking muscle memory.
- Q: Should the opacity change on selection toggle animate or snap instantly? → A: Smooth opacity transition (150-200ms) matching the app's existing hover transition speed, consistent with the micro-interaction design language.
- Q: Should the profile JSON control which tests are enabled/disabled (per-test `enabled` field)? → A: No. Keep selection UI-only with no profile JSON changes. All tests default to selected. Profile-level control can be a future enhancement if needed.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Select and Run Specific Tests (Priority: P1)

A user has a profile with 20+ tests. One test recently started failing and they want to re-run only that test to debug it. They open the test list, see checkboxes next to each test, uncheck "Select All," check the single failing test, and click "Run Selected." Only that one test executes, saving them from waiting for the entire suite.

**Why this priority**: This is the core value proposition. Without the ability to select individual tests and run them, the feature has no purpose. It directly addresses the pain point of running all tests when only one needs attention.

**Independent Test**: Can be fully tested by loading any profile with 2+ tests, selecting a subset via checkboxes, running them, and verifying only the selected tests execute.

**Acceptance Scenarios**:

1. **Given** a loaded profile with 10 tests and all tests checked by default, **When** the user unchecks 8 tests and clicks "Run Selected," **Then** only the 2 checked tests execute and appear in the results.
2. **Given** a loaded profile with tests, **When** the user checks a single test and runs it, **Then** the run progress page shows "1 of 1" tests, not the full profile count.
3. **Given** a loaded profile, **When** the user clicks "Run Selected" with no tests checked, **Then** the run button is disabled (cannot start an empty run).

---

### User Story 2 - Select All / Deselect All (Priority: P2)

A user wants to quickly toggle between running the full suite and running a subset. They use a "Select All" checkbox in the test list header to check or uncheck all tests at once, then fine-tune individual selections.

**Why this priority**: Bulk selection is essential for usability. Without it, users with 20+ tests would need to individually check each box to return to a full run, which is tedious and error-prone.

**Independent Test**: Can be tested by toggling the "Select All" checkbox and verifying all individual test checkboxes reflect the change.

**Acceptance Scenarios**:

1. **Given** a loaded profile with all tests checked, **When** the user unchecks the "Select All" checkbox, **Then** all individual test checkboxes become unchecked.
2. **Given** a loaded profile with no tests checked, **When** the user checks the "Select All" checkbox, **Then** all individual test checkboxes become checked.
3. **Given** a loaded profile with all tests checked, **When** the user unchecks one individual test, **Then** the "Select All" checkbox shows an indeterminate (partial) state.
4. **Given** a loaded profile with some tests unchecked (indeterminate state), **When** the user checks the "Select All" checkbox, **Then** all tests become checked.

---

### User Story 3 - Selection Count Feedback (Priority: P3)

A user wants to know at a glance how many tests are selected before starting a run. The run button and a summary label reflect the current selection count (e.g., "Run 3 of 10 Tests"), giving the user confidence about what will happen when they click it.

**Why this priority**: This is a usability enhancement that builds on top of the core selection feature. Users can technically count themselves, but a clear count reduces errors and builds trust.

**Independent Test**: Can be tested by toggling selections and verifying the button label and summary text update correctly.

**Acceptance Scenarios**:

1. **Given** a loaded profile with 10 tests and 3 selected, **When** the user views the run button, **Then** it displays "Run 3 of 10 Tests."
2. **Given** a loaded profile with all 10 tests selected, **When** the user views the run button, **Then** it displays "Run All Tests" (the familiar default label).
3. **Given** a loaded profile with 0 tests selected, **When** the user views the run button, **Then** it is disabled and displays "Run Tests" or similar neutral text.

---

### Edge Cases

- What happens when a new profile is loaded? All tests in the new profile should be checked by default, matching the current "run all" behavior.
- What happens when the user navigates away from the test list and comes back? The selection state should reset to all-checked (since selections are session-only and not persisted).
- What happens with a profile containing exactly 1 test? The checkbox and "Select All" should still appear, and the run button should say "Run All Tests" when that one test is checked.
- How does selection interact with tests that require admin privileges? Selecting a test that requires admin should still work; the test will be skipped at runtime with the existing "requires admin" message if the user lacks privileges.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display an accent-styled checkbox embedded inside each test card (left side, before the test type icon), allowing individual test selection. The checkbox must match the app's premium design language (accent colors, consistent spacing).
- **FR-001a**: Deselected (unchecked) test cards MUST be displayed at reduced opacity (50-60%) so that selected cards are visually prominent at a glance.
- **FR-001b**: Clicking the checkbox MUST toggle selection without navigating. Clicking the card body MUST navigate to test details (existing behavior preserved). The checkbox and card body are independent click targets.
- **FR-001c**: Selection state changes (opacity transition on check/uncheck) MUST animate smoothly (150-200ms) consistent with the app's existing micro-interaction transitions.
- **FR-002**: System MUST check all tests by default when a profile is loaded, preserving the current "run all" behavior as the default experience.
- **FR-003**: System MUST provide a "Select All" checkbox positioned in the list header row (between the test count label and the run button) that toggles all individual test checkboxes on or off.
- **FR-004**: The "Select All" checkbox MUST display an indeterminate (partial) state when some but not all tests are selected.
- **FR-005**: System MUST only execute the selected (checked) tests when the user initiates a run.
- **FR-006**: The run button MUST be disabled when no tests are selected.
- **FR-007**: The run button label MUST reflect the selection count: "Run All Tests" when all are selected, "Run N of M Tests" when a subset is selected.
- **FR-008**: The run results and history MUST accurately reflect only the tests that were actually executed (not the full profile).
- **FR-009**: Test selection state MUST NOT be persisted; it resets to all-selected when the profile is reloaded or the user navigates away and returns.

### Key Entities

- **Test Selection State**: A per-test boolean (selected/not selected) that exists only in the test list view's session. Not persisted. Defaults to selected for all tests in the loaded profile.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can select a subset of tests and complete a partial run in time proportional only to the selected tests (e.g., selecting 1 of 20 tests runs ~20x faster than running all).
- **SC-002**: 100% of runs execute exactly the tests the user selected — no more, no fewer.
- **SC-003**: The default experience (all tests selected on load) is indistinguishable from the current "Run All" behavior, ensuring zero regression for users who do not use selection.
- **SC-004**: Users can go from "all selected" to "one test selected" in at most 2 interactions (uncheck all + check one).

## Assumptions

- Test selection is a lightweight, session-only concept. There is no need to persist selections across sessions or associate them with profiles.
- The profile JSON schema is NOT modified by this feature. No `enabled` field is added to TestDefinition. Profile-level test enablement is out of scope and can be a future enhancement.
- The existing run progress page, results page, and history recording work correctly with a subset of tests because they already operate on whatever tests the runner returns — no changes needed to those downstream pages.
- The existing "Run All Tests" button will be renamed/repurposed to "Run Selected Tests" (or dynamic label) rather than adding a second button.
