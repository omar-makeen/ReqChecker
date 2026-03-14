# Feature Specification: Unsaved Changes Warning

**Feature Branch**: `061-unsaved-changes-warning`
**Created**: 2026-03-14
**Status**: Draft
**Input**: User description: "Add unsaved changes warning to the Test Configuration page. When a user edits timeout, retries, or test parameters and then clicks the Back button without saving, show a confirmation dialog asking whether to discard changes or go back and save. Track a dirty/modified state by comparing current values against original values loaded when the page opened. If no changes were made, navigate back immediately without prompting."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Warn on Unsaved Changes When Navigating Away (Priority: P1)

A user opens the Test Configuration page for a specific test, modifies the timeout value or a parameter, then clicks the Back button to return to the Test Suite. Instead of silently discarding their changes, the system detects that modifications were made and displays a confirmation dialog. The user can choose to discard changes and navigate away, or stay on the page to save first.

**Why this priority**: This is the core feature — preventing silent data loss. Without this, users lose work without realizing it, leading to frustration and mistrust of the application.

**Independent Test**: Can be fully tested by opening any test's configuration, changing the timeout value, clicking Back, and verifying the confirmation dialog appears with appropriate options.

**Acceptance Scenarios**:

1. **Given** the user is on the Test Configuration page and has modified the timeout value, **When** they click the Back button, **Then** a confirmation dialog appears asking whether to discard changes or stay on the page.
2. **Given** the confirmation dialog is showing, **When** the user chooses to discard changes, **Then** all modifications are discarded and the user is navigated back to the Test Suite.
3. **Given** the confirmation dialog is showing, **When** the user chooses to stay on the page, **Then** the dialog closes and the user remains on the Test Configuration page with their modifications intact.
4. **Given** the user is on the Test Configuration page and has modified a test parameter value, **When** they click the Back button, **Then** the same confirmation dialog appears.
5. **Given** the user is on the Test Configuration page and has modified the retry count, **When** they click the Back button, **Then** the same confirmation dialog appears.

---

### User Story 2 - No Warning When No Changes Made (Priority: P1)

A user opens the Test Configuration page to review a test's settings but does not modify anything. When they click the Back button, they are navigated directly back to the Test Suite without any interruption.

**Why this priority**: Equal priority to US1 — false positives (showing a dialog when nothing changed) are just as annoying as missing warnings. Both must work correctly for the feature to be useful.

**Independent Test**: Can be tested by opening any test's configuration, not changing anything, clicking Back, and verifying immediate navigation without a dialog.

**Acceptance Scenarios**:

1. **Given** the user is on the Test Configuration page and has not modified any values, **When** they click the Back button, **Then** they are navigated back to the Test Suite immediately without any dialog.
2. **Given** the user is on the Test Configuration page and changed a value but then reverted it to the original value, **When** they click the Back button, **Then** they are navigated back without a dialog (the system recognizes no net change).

---

### User Story 3 - No Warning After Saving (Priority: P2)

A user modifies test configuration values and clicks Save Changes. After saving, they click the Back button. Since the changes have been persisted, no warning dialog should appear.

**Why this priority**: Completes the save-then-navigate workflow. Users who save should not be re-prompted.

**Independent Test**: Can be tested by modifying a value, clicking Save, then clicking Back, and verifying no dialog appears.

**Acceptance Scenarios**:

1. **Given** the user has modified values and then clicked Save Changes, **When** they click the Back button, **Then** they are navigated back to the Test Suite without any dialog.
2. **Given** the user has saved changes and then makes additional modifications, **When** they click the Back button, **Then** the confirmation dialog appears (new unsaved changes exist since the last save).

---

### Edge Cases

- What happens if the user modifies a password field (which uses a different input control)? The dirty tracking should include password parameter changes.
- What happens if the user opens config for a test with no editable parameters (all locked/read-only)? Only timeout and retries are editable, so dirty tracking still applies to those fields. If timeout and retries are also locked, the Back button should navigate immediately (nothing can be dirty).
- What happens if the user modifies a value, saves, modifies again, then clicks Back? The dialog should appear because there are unsaved changes relative to the last save.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST track a "dirty" state for the Test Configuration page by comparing current field values against the values that were loaded when the page opened (or last saved).
- **FR-002**: The dirty state MUST track changes to: timeout value, retry count, and all editable/password test parameter values.
- **FR-003**: When the user clicks the Back button and there are unsaved changes (dirty state is true), the system MUST display a confirmation dialog before navigating away.
- **FR-004**: The confirmation dialog MUST offer two options: "Discard Changes" (navigates away, discarding modifications) and "Stay" (closes dialog, returns to the page).
- **FR-005**: When the user clicks the Back button and there are no unsaved changes (dirty state is false), the system MUST navigate back immediately without showing any dialog.
- **FR-006**: After the user clicks Save Changes, the dirty state MUST be reset (baseline updated to the newly saved values).
- **FR-007**: If the user modifies a value and then reverts it to the original value, the dirty state MUST be false (value-based comparison, not event-based).
- **FR-008**: The confirmation dialog MUST be consistent with the application's existing dialog style (matching the design system used for Settings reset confirmation).
- **FR-009**: The confirmation dialog MUST be keyboard-accessible (Escape to stay, Enter to confirm the focused action).

### Key Entities

- **DirtyState**: A session-only boolean flag on the Test Configuration page, computed by comparing current field values against stored baseline values. Reset when the page loads (baseline captured) or when Save is clicked (baseline updated).
- **FieldBaseline**: A snapshot of all editable field values (timeout, retries, parameters) captured when the Test Configuration page loads or after a successful save. Used for value-based comparison to determine dirty state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of unsaved modifications to timeout, retries, or parameters trigger a confirmation dialog when the user attempts to navigate away.
- **SC-002**: 0% of unmodified configurations trigger a false-positive confirmation dialog.
- **SC-003**: Users can dismiss the confirmation dialog and return to editing in under 1 second.
- **SC-004**: After clicking Save Changes, the dirty state resets and no dialog appears on subsequent Back navigation.
- **SC-005**: Reverting a field to its original value correctly clears the dirty state (no false positives from edit-then-revert workflows).
