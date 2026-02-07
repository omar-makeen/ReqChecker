# Feature Specification: Fix Checkbox Visibility and Select All Placement

**Feature Branch**: `031-fix-checkbox-ux`
**Created**: 2026-02-07
**Status**: Draft
**Input**: User description: "Check screenshot — checkboxes show no checkmark (✓) even when tests are selected, and the Select All checkbox is not in the correct place from a UI/UX perspective."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Checkbox Checkmark Visibility (Priority: P1)

A user navigates to the Test Suite page. All tests are selected by default, but the checkboxes appear as empty squares with no visible checkmark. The user cannot tell whether tests are selected or deselected, making the selection feature confusing and unusable.

After this fix, every checkbox displays a clearly visible checkmark (✓) when checked, and an empty box when unchecked, in both dark and light themes.

**Why this priority**: The invisible checkmark renders the entire selective test run feature non-functional from a usability standpoint. Users cannot see or understand the selection state of any test.

**Independent Test**: Can be verified by opening the Test Suite page with a loaded profile and confirming that all checkboxes display a visible ✓ glyph by default (since all tests start selected).

**Acceptance Scenarios**:

1. **Given** a profile with tests is loaded, **When** the user navigates to the Test Suite page, **Then** every test card checkbox displays a visible checkmark indicating the selected state.
2. **Given** a selected test (checked), **When** the user unchecks the checkbox, **Then** the checkmark disappears and the box appears empty.
3. **Given** an unselected test (unchecked), **When** the user checks the checkbox, **Then** a checkmark becomes visible inside the box.
4. **Given** the dark theme is active, **When** checkboxes are in the checked state, **Then** the checkmark is clearly visible against the checkbox background.
5. **Given** the light theme is active, **When** checkboxes are in the checked state, **Then** the checkmark is clearly visible against the checkbox background.

---

### User Story 2 - Relocate Select All Checkbox (Priority: P2)

The "Select All" checkbox is currently placed in the page header row, squeezed between the "4 tests" badge and the "Run All Tests" button. This placement is spatially disconnected from the test list it controls and is easy to overlook.

After this fix, the "Select All" control is moved to a toolbar row positioned between the header and the test list, giving it a clear visual association with the test cards below.

**Why this priority**: While the Select All control works correctly, its current placement hurts discoverability and feels cramped. Relocating it improves the overall user experience and visual hierarchy of the page.

**Independent Test**: Can be verified by navigating to the Test Suite page and confirming the Select All checkbox appears in a distinct toolbar row directly above the test list, separate from the page title and Run button.

**Acceptance Scenarios**:

1. **Given** a profile with tests is loaded, **When** the user views the Test Suite page, **Then** the "Select All" checkbox appears in a toolbar row between the page header and the test list.
2. **Given** the toolbar row is visible, **When** the user clicks "Select All", **Then** all test checkboxes become checked.
3. **Given** all tests are selected, **When** the user clicks "Select All" again, **Then** all test checkboxes become unchecked.
4. **Given** no profile is loaded, **When** the user views the Test Suite page, **Then** the Select All toolbar row is hidden (only the empty-state message is shown).

---

### Edge Cases

- What happens when only one test exists in the profile? The Select All row should still appear and function correctly.
- What happens when the user toggles between light and dark themes? Checkbox checkmarks must remain visible in both themes.
- What happens during the entrance animation? Checkmarks should be visible as soon as the card fades in; they should not appear delayed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Checkboxes MUST display a clearly visible checkmark glyph when in the checked state.
- **FR-002**: Checkboxes MUST display an empty box when in the unchecked state.
- **FR-003**: The checkmark MUST be visible in both dark and light application themes.
- **FR-004**: The "Select All" control MUST be placed in a dedicated toolbar row between the page header and the test list.
- **FR-005**: The "Select All" toolbar row MUST be hidden when no profile is loaded (empty state).
- **FR-006**: The "Select All" checkbox MUST toggle all test checkboxes when clicked, matching existing behavior.
- **FR-007**: The checkbox checked state MUST be visually correct on initial page load (not just after user interaction).

## Assumptions

- The checkmark visibility issue is caused by the checkmark glyph geometry being sized or positioned outside the visible area of the 18x18 checkbox container. The fix involves correcting the glyph coordinates to fit within the container.
- The Select All toolbar will remain a simple row with the checkbox and "Select All" label; no additional controls (e.g., search, filter) are added in this scope.
- The "4 tests" badge and "Run All Tests" button remain in the page header — only the Select All checkbox+label move to the new toolbar row.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of checkboxes display a visible checkmark when checked, verified visually on both dark and light themes.
- **SC-002**: The Select All control is immediately discoverable in the toolbar row — users can locate it without scrolling or searching the header area.
- **SC-003**: All existing selective test run functionality continues to work (select, deselect, run subset, Select All toggle) with no regressions.
- **SC-004**: The fix introduces zero build warnings and all existing tests continue to pass.
