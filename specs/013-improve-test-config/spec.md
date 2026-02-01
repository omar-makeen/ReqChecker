# Feature Specification: Improve Test Configuration View

**Feature Branch**: `013-improve-test-config`
**Created**: 2026-02-01
**Status**: Draft
**Input**: User description: "1- execution settings timeout and retries contradict test parameters section, 2- back button not work, 3- test parameters can be editable or not based on test configuration file, 4- discover any other issues"

## Problem Statement

The Test Configuration view has several usability and data consistency issues that need to be addressed:

1. **Duplicate/Contradicting Fields**: The "Execution Settings" section displays Timeout and Retries fields, while the "Test Parameters" section also displays Timeout and RetryCount as parameters. This creates confusion and potential data conflicts.

2. **Non-functional Back Button**: The back button calls `CancelCommand` which resets form values but does not navigate back to the previous view (Tests list).

3. **Parameter Editability**: The Test Parameters section already supports field policies (Editable, Locked, Hidden, PromptAtRun), but the ViewModel incorrectly adds Timeout/RetryCount/RequiresAdmin as parameters regardless of the test definition's intended structure.

4. **Additional Issues Discovered**:
   - RequiresAdmin is shown in both Basic Information (as locked display) AND added to Parameters collection (as locked parameter)
   - The `CountToVisibilityConverter` is referenced in XAML but may not show the empty state correctly due to always having parameters (Timeout, RetryCount, RequiresAdmin are always added)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Navigate Back from Test Configuration (Priority: P1)

As a user viewing test configuration, I want to return to the test list by clicking the back button, so that I can continue browsing or selecting other tests.

**Why this priority**: Core navigation is broken - users cannot return from the detail view without closing the app.

**Independent Test**: Open test list, click a test to open configuration, click Back button - user returns to test list.

**Acceptance Scenarios**:

1. **Given** I am viewing Test Configuration for any test, **When** I click the "Back" button in the header, **Then** I am navigated back to the Tests list view.
2. **Given** I am viewing Test Configuration with unsaved changes, **When** I click "Back", **Then** I am navigated back (changes are discarded without confirmation for now).
3. **Given** I am viewing Test Configuration, **When** I click "Cancel" button at the bottom, **Then** the form resets to original values (does NOT navigate).

---

### User Story 2 - View Test Configuration Without Duplicate Fields (Priority: P1)

As a user viewing test configuration, I want to see Timeout and Retries only in the Execution Settings section, so that I don't see contradicting or duplicate information.

**Why this priority**: Duplicate fields create confusion about which value is authoritative and where to make changes.

**Independent Test**: Open test configuration, verify Timeout/Retries appear ONLY in Execution Settings section, NOT in Test Parameters section.

**Acceptance Scenarios**:

1. **Given** I open Test Configuration for any test, **When** the view loads, **Then** I see Timeout field in "Execution Settings" section only.
2. **Given** I open Test Configuration for any test, **When** the view loads, **Then** I see Retries field in "Execution Settings" section only.
3. **Given** I open Test Configuration for a test with custom parameters, **When** the view loads, **Then** Test Parameters section shows ONLY the test-specific parameters from the profile (not Timeout, RetryCount, or RequiresAdmin).
4. **Given** I open Test Configuration for a test with NO custom parameters, **When** the view loads, **Then** Test Parameters section shows the "No parameters defined" empty state.

---

### User Story 3 - Parameter Editability Based on Field Policy (Priority: P2)

As a test administrator, I want parameters to respect their defined field policies, so that locked parameters cannot be edited and prompt-at-run parameters show the appropriate indicator.

**Why this priority**: Field policy support already exists in code but is being bypassed by hardcoded parameter additions.

**Independent Test**: Load a test with various field policies defined and verify each renders correctly.

**Acceptance Scenarios**:

1. **Given** a test has a parameter with policy "Editable", **When** I view Test Configuration, **Then** the parameter shows as an editable text field.
2. **Given** a test has a parameter with policy "Locked", **When** I view Test Configuration, **Then** the parameter shows with the locked field control (not editable).
3. **Given** a test has a parameter with policy "Hidden", **When** I view Test Configuration, **Then** the parameter is not visible.
4. **Given** a test has a parameter with policy "PromptAtRun", **When** I view Test Configuration, **Then** the parameter shows the "Will be prompted during test execution" indicator.

---

### Edge Cases

- What happens when a test has no parameters at all? → Empty state message should display
- What happens when all parameters are Hidden? → Empty state message should display
- How does Back navigation work when opened from different entry points? → Always navigates back in frame history

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST navigate to the previous view (Tests list) when Back button is clicked
- **FR-002**: System MUST display Timeout and Retries fields ONLY in the Execution Settings section
- **FR-003**: System MUST NOT add Timeout, RetryCount, or RequiresAdmin to the Parameters collection
- **FR-004**: System MUST display only test-specific parameters (from profile definition) in the Test Parameters section
- **FR-005**: System MUST respect FieldPolicyType for each parameter (Editable, Locked, Hidden, PromptAtRun)
- **FR-006**: Cancel button MUST reset form values without navigating
- **FR-007**: System MUST show "No parameters defined" empty state when Parameters collection is empty or all parameters are Hidden

### Key Entities

- **TestDefinition**: Defines a test including DisplayName, Type, RequiresAdmin, Timeout, RetryCount, Parameters dictionary, and FieldPolicy dictionary
- **TestParameterViewModel**: Represents a single parameter with Name, Value, and Policy (determines editability/visibility)
- **TestConfigViewModel**: Manages the test configuration form state and commands

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Back button successfully returns user to Tests list in 100% of navigation scenarios
- **SC-002**: Zero duplicate fields shown between Execution Settings and Test Parameters sections
- **SC-003**: Field policies correctly render for all four policy types (Editable, Locked, Hidden, PromptAtRun)
- **SC-004**: Empty state displays when no visible parameters exist
