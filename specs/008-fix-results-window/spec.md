# Feature Specification: Fix Test Results Window

**Feature Branch**: `008-fix-results-window`
**Created**: 2026-01-31
**Status**: Draft
**Input**: User description: "create new spec to fix all test results window, 1- no data loaded in screen 2- json and csv buttons doesn't work 3- results in left side menu not focused when window open event to work at all when click on it 4- make sure filter works 5- discover any others issues to fix"

## Problem Summary

The Test Results window has multiple issues that prevent users from viewing and exporting their test results:

1. **No data displayed** - When navigating to Results via the sidebar menu, the screen shows "0 Tests" with no results
2. **Export buttons non-functional** - JSON and CSV export buttons don't produce any output
3. **Navigation menu not focused** - The "Results" menu item is not visually highlighted when the Results view is active
4. **Filter functionality unverified** - Need to confirm filters (All/Passed/Failed/Skipped) work correctly
5. **Additional discovered issues** - Empty state handling, Results menu availability when no results exist

## Root Cause Analysis

Based on code exploration:

1. **Data not loading**: `NavigationService.NavigateToResults()` creates a new `ResultsViewModel` but never sets the `Report` property from `AppState.LastRunReport`. The data exists in app state but is not passed to the view model.

2. **Export buttons failing silently**: The export commands check `if (DialogService == null || Report == null)` and return early. Since `Report` is null (not loaded), exports fail silently with no user feedback.

3. **Menu not focused**: When navigating via sidebar, the `NavResults.IsActive` property is never set to `true`, unlike other navigation items that properly update their active state.

4. **Additional issue - Results availability**: Users can click "Results" in the menu even when no test run has occurred, leading to an empty/confusing state.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Previous Test Results (Priority: P1)

As a user who has run tests, I want to navigate to the Results view from the sidebar menu and see my previous test results displayed with accurate counts and details.

**Why this priority**: This is the core functionality - without data loading, nothing else in the Results window works.

**Independent Test**: Can be fully tested by running tests, then clicking "Results" in sidebar, and verifying all test results appear with correct Pass/Fail/Skipped counts.

**Acceptance Scenarios**:

1. **Given** a user has completed a test run with 4 tests (2 passed, 1 failed, 1 skipped), **When** the user clicks "Results" in the sidebar menu, **Then** the Results view displays "4 Tests" with counts showing "2 Passed", "1 Failed", "1 Skipped"

2. **Given** a user is viewing test results, **When** the user navigates away and returns to Results, **Then** the same test results are still displayed (data persisted in session)

3. **Given** no tests have been run in the current session, **When** the user clicks "Results" in the sidebar menu, **Then** the Results view shows an appropriate empty state message indicating no results are available

---

### User Story 2 - Export Results to JSON (Priority: P2)

As a user viewing test results, I want to export my results to JSON format so I can archive, share, or process them externally.

**Why this priority**: Export is a key feature for sharing results and documentation, but viewing results must work first.

**Independent Test**: Can be tested by running tests, navigating to Results, clicking JSON button, selecting save location, and verifying valid JSON file is created.

**Acceptance Scenarios**:

1. **Given** a user is viewing test results with data, **When** the user clicks the "JSON" export button, **Then** a save file dialog appears with a default filename containing the run ID

2. **Given** the user selects a save location, **When** the export completes, **Then** a success message appears and the file contains valid JSON with all test results

3. **Given** no test results are loaded, **When** the user clicks the "JSON" button, **Then** the button should be disabled or show a message indicating no data to export

---

### User Story 3 - Export Results to CSV (Priority: P2)

As a user viewing test results, I want to export my results to CSV format so I can open them in spreadsheet applications.

**Why this priority**: Same as JSON export - important for data portability.

**Independent Test**: Can be tested by running tests, navigating to Results, clicking CSV button, and verifying valid CSV file is created that opens in a spreadsheet.

**Acceptance Scenarios**:

1. **Given** a user is viewing test results with data, **When** the user clicks the "CSV" export button, **Then** a save file dialog appears with a default filename

2. **Given** the user selects a save location, **When** the export completes, **Then** a success message appears and the file contains properly formatted CSV data

---

### User Story 4 - Filter Test Results (Priority: P3)

As a user viewing test results, I want to filter results by status (All/Passed/Failed/Skipped) so I can focus on specific outcomes.

**Why this priority**: Filtering is useful but secondary to viewing and exporting data.

**Independent Test**: Can be tested by running tests with mixed outcomes, navigating to Results, and clicking each filter tab to verify correct filtering.

**Acceptance Scenarios**:

1. **Given** test results contain 2 passed, 1 failed, 1 skipped, **When** the user clicks "Passed" filter, **Then** only the 2 passed tests are displayed in the list

2. **Given** the "Failed" filter is active, **When** the user clicks "All" filter, **Then** all 4 tests are displayed again

3. **Given** no tests match the current filter, **When** the user applies that filter, **Then** an empty state message is shown (e.g., "No failed tests")

---

### User Story 5 - Navigation Menu Highlighting (Priority: P3)

As a user navigating the application, I want the sidebar menu to clearly indicate which view is currently active so I always know where I am.

**Why this priority**: Visual feedback is important for usability but doesn't block core functionality.

**Independent Test**: Can be tested by navigating to each menu item and verifying it becomes visually highlighted/selected.

**Acceptance Scenarios**:

1. **Given** the user is on the Tests view, **When** the user clicks "Results" in the sidebar, **Then** the "Results" menu item becomes highlighted/active and "Tests" becomes inactive

2. **Given** the user navigates to Results after completing a test run, **Then** the "Results" menu item is highlighted

---

### Edge Cases

- What happens when the user exports to a read-only location? Error message should inform the user.
- What happens when the user cancels the export dialog? No action taken, no error shown.
- What happens when the app is restarted? Results from previous session are not available (session-only storage).
- What happens when the user runs new tests after viewing old results? Results update to show the newest run.
- What happens when clicking Results multiple times rapidly? Navigation should not duplicate or break.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST load the last run report from application state when navigating to the Results view
- **FR-002**: System MUST display accurate test counts (Total, Passed, Failed, Skipped) from the loaded report
- **FR-003**: System MUST display profile name, start time, and duration from the loaded report
- **FR-004**: System MUST enable JSON export button only when test results are available
- **FR-005**: System MUST enable CSV export button only when test results are available
- **FR-006**: System MUST show success message after successful export with the filename
- **FR-007**: System MUST show error message if export fails (permission denied, disk full, etc.)
- **FR-008**: System MUST highlight the "Results" navigation menu item when Results view is active
- **FR-009**: System MUST correctly filter displayed results when user selects a filter tab
- **FR-010**: System MUST show appropriate empty state when no results match the current filter
- **FR-011**: System MUST show appropriate empty state when navigating to Results with no prior test run
- **FR-012**: System MUST preserve filter selection when navigating away and returning to Results

### Key Entities

- **RunReport**: Represents a complete test execution session including profile info, timing, machine info, and all test results
- **TestResult**: Individual test outcome with status (Pass/Fail/Skipped), timing, and error details
- **AppState**: Application-wide state that persists the last run report across navigation

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view test results within 1 second of clicking the Results menu item
- **SC-002**: 100% of export operations complete successfully when user has write permissions to the selected location
- **SC-003**: Users can identify the active navigation item at a glance (correct visual highlighting)
- **SC-004**: Filter operations respond within 100ms of user click
- **SC-005**: All displayed counts (Total, Passed, Failed, Skipped) match the actual data in the run report
- **SC-006**: Zero silent failures - all error conditions display user-friendly messages

## Assumptions

- Results are stored in memory only (session scope); persistence across app restarts is not required
- The existing `IAppState.LastRunReport` mechanism is the correct data source for results
- Export functionality already works correctly when `Report` is properly set (only the data loading is broken)
- The WPF-UI NavigationViewItem has an `IsActive` property that can be set programmatically
