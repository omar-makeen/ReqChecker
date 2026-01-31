# Feature Specification: Display Test Result Details in Expanded Card

**Feature Branch**: `010-result-details`
**Created**: 2026-01-31
**Status**: Draft
**Input**: User description: "Implement the functionality to display test result details within the expanded test card in the results window. Currently, the test card expands when clicked, but no information is displayed in the expanded view. Based on the specifications defined in speckit.specify.md, update the necessary XAML views and ViewModels to bind the specific test result data properties, ensuring the details render correctly when a user interacts with the test card."

## Clarifications

### Session 2026-01-31

- Q: How should the expanded content sections be visually styled to match the app's premium aesthetic? → A: Elevated sections with accent left borders (status-colored bar for errors, accent gradient for technical details)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Test Result Summary (Priority: P1)

A user runs a test suite and wants to understand what happened with each individual test. After the tests complete, the user clicks on a test card to expand it and see a human-readable summary explaining the test outcome in plain language.

**Why this priority**: This is the core value proposition - users need to understand test results without reading technical details. A clear summary enables quick triage of failures and confirmation of successes.

**Independent Test**: Can be fully tested by running any test, clicking on the result card, and verifying the summary text displays and explains the outcome clearly.

**Acceptance Scenarios**:

1. **Given** test results are displayed in the Results view, **When** the user clicks on a test card, **Then** the card expands to show the human summary text explaining what the test verified and the outcome
2. **Given** a test has passed, **When** the user expands the card, **Then** the summary explains what was successfully verified (e.g., "Successfully connected to server at example.com:443")
3. **Given** a test has failed, **When** the user expands the card, **Then** the summary explains what went wrong in user-friendly language

---

### User Story 2 - View Technical Details (Priority: P2)

A technical user needs to see the raw technical output from a test execution for debugging purposes. When they expand a test card, they can view formatted technical details such as response data, headers, timing information, and other execution artifacts.

**Why this priority**: Technical details are essential for debugging but only needed by technical users. The human summary (P1) serves most users, while this provides depth for troubleshooting.

**Independent Test**: Can be tested by running a test that produces technical output (e.g., HTTP GET test with response body), expanding the card, and verifying the technical details section shows the captured data.

**Acceptance Scenarios**:

1. **Given** a test produces technical output (response data, file content, etc.), **When** the user expands the card, **Then** the technical details section displays the captured output in a monospace font
2. **Given** a test involves HTTP/FTP requests, **When** the user expands the card, **Then** response codes and relevant headers are visible in the technical details
3. **Given** technical output contains sensitive credentials, **When** displayed in the expanded card, **Then** credentials are masked for security

---

### User Story 3 - View Error Information (Priority: P1)

When a test fails, the user needs to quickly understand what went wrong. The expanded test card displays error information prominently with visual styling that distinguishes it from successful test data.

**Why this priority**: Error visibility is critical for the primary use case of identifying and diagnosing test failures. Users must be able to quickly spot and understand errors.

**Independent Test**: Can be tested by running a test that fails (e.g., ping to unreachable host), expanding the card, and verifying the error message displays with appropriate styling.

**Acceptance Scenarios**:

1. **Given** a test has failed with an error, **When** the user expands the card, **Then** the error message is displayed in a visually distinct error section (red styling)
2. **Given** the error has additional context (exception type), **When** the user expands the card, **Then** the error section shows the relevant error classification
3. **Given** a test passed without errors, **When** the user expands the card, **Then** no error section is visible

---

### User Story 4 - View Test Metadata (Priority: P3)

A user wants to see additional context about a test execution, including timing information, retry attempts, and test type identification.

**Why this priority**: Metadata provides useful context but is not essential for understanding test outcomes. It supports advanced use cases like performance analysis and debugging retry behavior.

**Independent Test**: Can be tested by expanding any test card and verifying that timing/attempt information is visible.

**Acceptance Scenarios**:

1. **Given** a test has completed, **When** the user expands the card, **Then** the execution duration is visible
2. **Given** a test required multiple retry attempts, **When** the user expands the card, **Then** the number of attempts is indicated
3. **Given** a test has timing breakdown data, **When** the user expands the card, **Then** the timing phases are displayed (connection time, data transfer time, etc.)

---

### Edge Cases

- What happens when a test result has no summary text? Display a fallback message indicating no summary is available
- What happens when technical details are extremely long? Truncate with indication that data is truncated, maintaining readability
- What happens when all three sections (summary, technical, error) are empty? Show a minimal message indicating the test completed but produced no output
- How does the expanded card handle very long error messages? Wrap text appropriately while maintaining visual hierarchy

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display the human summary text when a test card is expanded
- **FR-002**: System MUST display technical details in a formatted, monospace section with elevated background and accent gradient left border when available
- **FR-003**: System MUST display error information in an elevated section with status-colored (red) left border for failed tests
- **FR-004**: System MUST mask sensitive credentials (passwords, API keys, tokens) in displayed technical details
- **FR-005**: System MUST show test execution duration in the expanded card
- **FR-006**: System MUST indicate retry attempt count when a test required multiple attempts
- **FR-007**: System MUST hide sections (summary, technical, error) when the corresponding data is null or empty
- **FR-008**: System MUST preserve the expand/collapse animation behavior when displaying content
- **FR-009**: System MUST support keyboard navigation for expanding/collapsing test cards (Enter/Space when focused)
- **FR-010**: System MUST maintain proper visual hierarchy between summary, technical details, and error sections
- **FR-011**: System MUST style expanded content sections with elevated backgrounds and accent left borders to match the app's premium visual design system

### Key Entities

- **TestResult**: Core entity containing all test outcome data
  - DisplayName: Test identifier shown in card header
  - Status: Pass/Fail/Skipped status
  - Duration: Execution time
  - AttemptCount: Number of retry attempts
  - HumanSummary: Plain language explanation of outcome
  - TechnicalDetails: Raw technical output
  - Error: Error information for failed tests
  - Evidence: Captured data (response, files, processes, etc.)

- **TestError**: Error details for failed tests
  - Category: Error classification
  - Message: User-friendly error description
  - ExceptionType: Technical exception type
  - StackTrace: Debug information (conditional display)

- **TestEvidence**: Data captured during execution
  - ResponseData: HTTP/FTP response body
  - ResponseCode: HTTP status code
  - ResponseHeaders: HTTP headers
  - FileContent: File content sample
  - Timing: Detailed timing breakdown

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view test result details within 1 click of seeing the results list (single click to expand)
- **SC-002**: All non-empty data fields from TestResult are visible in the expanded view
- **SC-003**: 100% of failed tests display their error message in the expanded card
- **SC-004**: 100% of tests with technical output display that output when expanded
- **SC-005**: Credential masking applies to all instances of sensitive data patterns in displayed content
- **SC-006**: Expanded cards remain readable with content up to 4KB (the truncation limit for response data)
- **SC-007**: Users can identify test status (pass/fail/skip) from expanded content styling within 2 seconds

## Assumptions

- The existing ExpanderCard control's expand/collapse animation and structure will be preserved
- The TestResult model already contains all necessary data populated by the test runner
- Credential masking logic already exists in the codebase (CredentialMaskConverter)
- The current binding setup in ResultsView.xaml is correct but the data may not be populating properly
- Dark and light theme support should be maintained for all new UI elements
