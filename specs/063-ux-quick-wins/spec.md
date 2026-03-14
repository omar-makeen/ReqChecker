# Feature Specification: UX Quick Wins

**Feature Branch**: `063-ux-quick-wins`
**Created**: 2026-03-14
**Status**: Draft
**Input**: User description: "Remaining Quick Wins: tooltips, test count badge, export shortcut, profile name in progress header, filter tab transitions"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Profile Name in Test Execution Header (Priority: P1)

When a user starts running tests, they want to see which profile is being executed directly in the RunProgress header, especially when switching between multiple profiles in a session.

**Why this priority**: Highest impact for orientation — during long-running test suites, users glance at the progress page and need instant context about what profile is running. Currently only "Test Execution" and "Running X of Y tests" are shown.

**Independent Test**: Load a profile, run tests, and verify the profile name appears in the RunProgress header alongside the existing title/subtitle.

**Acceptance Scenarios**:

1. **Given** a user has loaded a profile named "Production-East", **When** they start a test run, **Then** the RunProgress header displays "Production-East" as a secondary label beneath or alongside the "Test Execution" title.
2. **Given** a user is viewing the RunProgress page after tests complete, **When** they look at the header, **Then** the profile name is still visible.
3. **Given** a profile has a long name (50+ characters), **When** displayed in the header, **Then** it is truncated with an ellipsis and the full name is available via tooltip.

---

### User Story 2 - Test Count Badge on Sidebar Navigation (Priority: P2)

Users want at-a-glance awareness of how many tests are loaded in the current profile without navigating to the test list page. A small badge on the "Test Suite" sidebar nav item shows the loaded test count.

**Why this priority**: Low-effort, high-visibility improvement. The sidebar is always visible and a badge provides persistent context that a profile is loaded and how large it is.

**Independent Test**: Load a profile and verify the sidebar "Test Suite" nav item shows a badge with the test count.

**Acceptance Scenarios**:

1. **Given** no profile is loaded, **When** the user views the sidebar, **Then** the "Test Suite" nav item has no badge.
2. **Given** a profile with 12 tests is loaded, **When** the user views the sidebar, **Then** the "Test Suite" nav item displays a badge showing "12".
3. **Given** a profile is loaded and then unloaded (navigating back to profile selector), **When** the user views the sidebar, **Then** the badge is removed.
4. **Given** the sidebar is in compact mode (collapsed), **When** a profile is loaded, **Then** the badge is still visible as a small indicator on or near the icon.

---

### User Story 3 - Export Keyboard Shortcut (Priority: P3)

Power users who frequently export test results want a keyboard shortcut to quickly trigger the export dropdown, reducing clicks and improving workflow speed.

**Why this priority**: Benefits repeat users who export after every test run. Small effort, meaningful efficiency gain for the target audience.

**Independent Test**: After a test run completes, press the keyboard shortcut on the Results page and verify the export dropdown opens.

**Acceptance Scenarios**:

1. **Given** the user is on the Results page with a completed test run, **When** they press Ctrl+E, **Then** the export dropdown menu opens.
2. **Given** the export dropdown is open, **When** the user presses Ctrl+E again, **Then** the dropdown closes (toggle behavior).
3. **Given** the user is on a page other than Results (e.g., Test List, History), **When** they press Ctrl+E, **Then** nothing happens (shortcut is scoped to the Results page).
4. **Given** an export is already in progress, **When** the user presses Ctrl+E, **Then** the shortcut is ignored (same behavior as clicking the disabled button).

---

### User Story 4 - Filter Tab Transition Animation (Priority: P4)

When switching between result filters (All, Passed, Failed, Skipped), the result list currently swaps instantly. A subtle fade or slide transition makes the switch feel smoother and more polished.

**Why this priority**: Pure visual polish. The app already has entrance animations on result items; this extends that consistency to tab switching. Low impact on functionality but improves perceived quality.

**Independent Test**: Run tests with mixed results, switch between filter tabs, and verify a brief visual transition occurs on the result list.

**Acceptance Scenarios**:

1. **Given** the user is viewing "All" results, **When** they click the "Passed" filter tab, **Then** the result list fades out briefly and the filtered list fades in (total transition under 200ms).
2. **Given** the user rapidly clicks between multiple filter tabs, **When** transitions overlap, **Then** the animation completes cleanly without visual glitches (interrupting animations snap to final state).
3. **Given** a filter returns zero results, **When** the transition completes, **Then** the empty state message appears with the same fade-in animation.

---

### User Story 5 - Complete Tooltip Coverage (Priority: P5)

A few interactive elements still lack tooltips. All clickable buttons and interactive controls should provide tooltip hints describing their function, consistent with the existing 400ms delay and styling convention.

**Why this priority**: Lowest priority because most buttons already have tooltips. This is a completeness pass to catch any remaining gaps.

**Independent Test**: Hover over every interactive button/control in the application and verify a tooltip appears within 400ms.

**Acceptance Scenarios**:

1. **Given** any interactive button or control in the application, **When** the user hovers for 400ms, **Then** a tooltip describing the control's function appears.
2. **Given** a disabled button, **When** the user hovers, **Then** the tooltip still appears (consistent with existing `ShowOnDisabled` behavior).

### Edge Cases

- What happens when the profile name is empty or null? The header falls back to showing only "Test Execution" with no secondary label.
- What happens when test count changes while on a different page (e.g., selective test run modifies count)? The badge updates reactively when the underlying test collection changes.
- What happens if the user presses Ctrl+E while a dialog (e.g., save file dialog) is open? The shortcut is consumed by the dialog and does not trigger the export dropdown.
- What happens with filter animation when the Results page first loads? The initial load uses the existing entrance animation; the filter transition animation only applies when switching between tabs.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The RunProgress header MUST display the current profile name as secondary text below or alongside the "Test Execution" title.
- **FR-002**: Profile names exceeding the available header width MUST be truncated with an ellipsis, with the full name available via tooltip.
- **FR-003**: The sidebar "Test Suite" navigation item MUST display a badge showing the number of tests when a profile is loaded.
- **FR-004**: The sidebar badge MUST be removed when no profile is loaded.
- **FR-005**: The sidebar badge MUST be visible in both expanded and compact sidebar modes.
- **FR-006**: Pressing Ctrl+E on the Results page MUST toggle the export dropdown menu open/closed.
- **FR-007**: The Ctrl+E shortcut MUST only function on the Results page.
- **FR-008**: The Ctrl+E shortcut MUST be ignored when an export operation is in progress.
- **FR-009**: Switching between filter tabs on the Results page MUST play a brief fade transition (under 200ms) on the result list.
- **FR-010**: Rapid filter tab switching MUST not cause visual glitches; interrupted animations MUST snap to their final state.
- **FR-011**: All interactive buttons and controls MUST have descriptive tooltips with a 400ms initial show delay.
- **FR-012**: Tooltips MUST appear on disabled controls (ShowOnDisabled behavior).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can identify which profile is running within 1 second of viewing the RunProgress page (profile name visible in header).
- **SC-002**: Users can determine the loaded test count without navigating away from any page (badge visible on sidebar).
- **SC-003**: Users can open the export menu with a single keyboard shortcut, reducing export workflow from 2 clicks to 1 keystroke.
- **SC-004**: Filter tab transitions complete in under 200ms with no visual artifacts during rapid switching.
- **SC-005**: 100% of interactive buttons and controls display a tooltip on hover.
- **SC-006**: All existing unit tests continue to pass after these changes.
