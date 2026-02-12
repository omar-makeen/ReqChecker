# Feature Specification: Fix Test Configuration Page UX

**Feature Branch**: `037-fix-test-config-ux`
**Created**: 2026-02-13
**Status**: Draft
**Input**: User description: "still there is a lot of space in test configuration window 2- cancel button do nothing 3- this is not premium authentic"

## Clarifications

### Session 2026-02-13

- Q: Should we keep the Cancel button (redundant with existing Back button), remove it, or keep both? → A: Remove Cancel button entirely. Keep only the Back button at the top. This follows modern single-page navigation patterns and eliminates redundancy.
- Q: Should the footer bar be removed entirely, with the "Save Changes" button moved to the header? → A: Yes. Remove the footer bar entirely and move "Save Changes" to the header (right-aligned). This matches all 5 other pages in the app, eliminates the visually heavy footer band, and reclaims vertical space.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Remove Cancel Button and Footer Bar (Priority: P1)

The test configuration page currently has a footer bar (with background, top border, and negative margins) containing a Cancel button and a Save button. The Cancel button only resets field values without navigating away, making it appear broken. The footer bar itself is unique to this page — all other pages in the app place action buttons in the header. Both the Cancel button and the footer bar should be removed. The "Save Changes" button should move to the header area (right-aligned), consistent with every other page in the application.

**Why this priority**: A non-functional button combined with an inconsistent page layout is the most critical UX defect — it breaks user trust and makes the page feel out of place within the app.

**Independent Test**: Can be fully tested by verifying the Cancel button and footer bar are gone, the "Save Changes" button appears in the header, and the Back button still navigates back correctly.

**Acceptance Scenarios**:

1. **Given** the user is on the test configuration page, **When** they view the page, **Then** there is no footer bar at the bottom of the page.
2. **Given** the user is on the test configuration page, **When** they view the header area, **Then** a "Save Changes" button is visible right-aligned in the header, alongside the Back button on the left.
3. **Given** the user wants to leave without saving, **When** they click the Back button at the top, **Then** they are navigated back to the test list page with unsaved changes discarded.
4. **Given** the user has modified editable fields, **When** they click "Save Changes" in the header, **Then** the changes are saved successfully.

---

### User Story 2 - Reduce Excessive Whitespace in Test Configuration Layout (Priority: P2)

A user opens the test configuration page and sees large empty areas of unused space on both sides, particularly on wider screens. The whitespace is caused by two factors: (1) the content area is capped at a narrow fixed width while the rest of the window is empty, and (2) the page's left/right margins are inconsistent with all other pages in the app. With the footer bar removed (User Story 1), vertical space is also reclaimed. The layout should use full available width (like every other page) and have consistent page margins.

**Why this priority**: Excessive whitespace makes the page feel incomplete and unprofessional, though it doesn't block functionality.

**Independent Test**: Can be tested by opening the test configuration page at various window sizes and verifying the content fills the available width consistently with other pages, and left/right margins match the rest of the app.

**Acceptance Scenarios**:

1. **Given** the user opens the test configuration page on a standard-width window, **When** viewing the page, **Then** the content fills the available width with no narrow centered column.
2. **Given** the user compares the test configuration page with other pages, **When** viewing the left and right margins, **Then** the margins are consistent with all other pages in the application.
3. **Given** the user resizes the application window to a narrow width, **When** viewing the test configuration page, **Then** the layout remains readable and does not overflow or break.
4. **Given** the user opens the test configuration page, **When** viewing the spacing between sections, **Then** vertical spacing between card sections is compact and visually balanced.

---

### User Story 3 - Premium-Styled Error Notification Bar (Priority: P3)

When a profile validation error occurs (e.g., "Profile ID must be a valid GUID"), the red error notification bar appears with a flat, basic style that looks out of place compared to the premium dark theme used throughout the rest of the application. The error bar should match the polished, elevated aesthetic of other UI components.

**Why this priority**: Visual consistency reinforces brand quality but does not block functionality. The error bar works correctly — it just looks inconsistent with the premium theme.

**Independent Test**: Can be tested by triggering a profile validation error and verifying the error notification bar uses premium styling consistent with the application theme.

**Acceptance Scenarios**:

1. **Given** a profile validation error occurs, **When** the error notification bar appears, **Then** it uses the application's premium theme styling including proper elevation, glow effects, and theme-aware colors.
2. **Given** the error notification bar is displayed, **When** the user views the "Dismiss" button, **Then** the button style is consistent with other buttons in the application and clearly interactive.
3. **Given** the error notification bar is displayed, **When** the user clicks "Dismiss", **Then** the error bar is dismissed smoothly.

---

### Edge Cases

- What happens when the error notification bar contains a very long validation message? The text should wrap properly within the bar.
- What happens on very narrow window widths? The layout must remain functional and readable.
- What happens when the user clicks Back after modifying fields? Unsaved changes are silently discarded (no confirmation dialog needed — the configuration page is low-stakes since changes can be easily re-applied).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Cancel button, its associated command, and the entire footer bar MUST be removed from the test configuration page.
- **FR-002**: The "Save Changes" button MUST be placed in the page header area, right-aligned, consistent with how all other pages in the app display action buttons.
- **FR-003**: The Back button (top-left) MUST remain the sole way to navigate back to the test list, discarding unsaved changes.
- **FR-004**: The test configuration page content MUST use the full available width (removing the narrow fixed-width cap) so it behaves like all other pages in the app.
- **FR-004a**: The test configuration page left and right margins MUST be consistent with all other pages in the application.
- **FR-005**: The vertical spacing between configuration sections MUST be compact and visually balanced, reducing unnecessary gaps.
- **FR-006**: The error notification bar MUST use theme-aware color tokens (not hard-coded color values) for background, border, and text.
- **FR-007**: The error notification bar MUST include premium styling consistent with the application theme (elevation glow, proper corner radius, theme-appropriate button style).
- **FR-008**: The error notification bar's Dismiss button MUST be clearly visible and styled consistently with other application buttons.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The test configuration page has no footer bar and no Cancel button. The "Save Changes" button is in the header area.
- **SC-002**: The test configuration content area fills the available page width more effectively, with no more than 10% unused horizontal space on standard window sizes.
- **SC-003**: The error notification bar visually matches the premium styling of other elevated components in the application (uses consistent corner radius, color tokens, and elevation effects).
- **SC-004**: All three issues (cancel/footer removal, whitespace, error styling) are resolved without regressions to existing test configuration functionality.

## Assumptions

- The Back button at the top already handles navigation correctly; no new navigation logic is needed.
- Unsaved changes are silently discarded on Back navigation (no confirmation dialog) since configuration changes are low-stakes and easily re-applied.
- "Premium authentic" styling means using the existing theme tokens (elevation glows, theme-aware backgrounds, consistent corner radii) already established in the application's design system.
- The whitespace is caused by two factors: a narrow fixed-width cap on the content area (unique to this page) and inconsistent left/right page margins compared to other pages. Both should be fixed.
- The header layout pattern for action buttons follows the same approach used by ResultsView, ProfileSelectorView, HistoryView, TestListView, and RunProgressView.
