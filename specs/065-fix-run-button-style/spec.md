# Feature Specification: Fix Run Button Style & UX

**Feature Branch**: `065-fix-run-button-style`
**Created**: 2026-03-14
**Status**: Draft
**Input**: User description: "Fix missing PrimaryButtonLarge style and improve Run button UX"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Premium Run Button Appearance (Priority: P1)

A user loads a test profile and sees the "Run Tests" button in the Test Suite page header. The button must look like a premium, gradient-styled call-to-action — visually dominant over surrounding elements like the test count badge — so it's immediately obvious where to click to execute tests.

**Why this priority**: The Run button is the single most important action in the entire app. A broken/missing style makes the primary CTA look like a generic system button, undermining the premium design language used everywhere else.

**Independent Test**: Can be fully tested by loading any test profile and visually confirming the Run button displays with the accent gradient background, white text, rounded corners, hover/press animations, and proper sizing — matching the `PrimaryButton` style already used on other pages.

**Acceptance Scenarios**:

1. **Given** a user is on the Test Suite page with a profile loaded, **When** they look at the header, **Then** the Run button displays with a gradient accent background, white text, and is visually the most prominent element in the header.
2. **Given** a user hovers over the Run button, **When** the cursor enters the button area, **Then** the button shows a subtle hover effect (opacity change) indicating interactivity.
3. **Given** a user clicks/presses the Run button, **When** the mouse is held down, **Then** the button shows a press animation (scale down) providing tactile feedback.

---

### User Story 2 - Helpful Disabled State (Priority: P2)

When no tests are selected, the Run button is disabled. The user should clearly understand why the button is disabled and what they need to do to enable it, without guessing.

**Why this priority**: A disabled button without explanation creates confusion and a dead-end UX. Guiding the user reduces frustration and increases task completion.

**Independent Test**: Can be tested by deselecting all tests and verifying the button appears disabled with a tooltip explaining the required action.

**Acceptance Scenarios**:

1. **Given** no tests are selected, **When** the user hovers over the disabled Run button, **Then** a tooltip appears explaining "Select at least one test to run".
2. **Given** no tests are selected, **When** the user looks at the button, **Then** it appears visually dimmed (reduced opacity) with a "not allowed" cursor.
3. **Given** the user selects one or more tests, **When** the selection changes, **Then** the button immediately becomes enabled with full opacity and a hand cursor.

---

### User Story 3 - Consistent Font Size (Priority: P3)

The button content (icon + text) should use consistent sizing that matches the button style definition, avoiding visual mismatch between the style's font size and the inline content's font size.

**Why this priority**: Minor visual inconsistency, but contributes to overall polish. The current inline `FontSize="15"` overrides the style's `FontSize="14"`, which is a maintainability and consistency issue.

**Independent Test**: Can be tested by verifying the Run button text renders at the same font size defined by the button style, without inline overrides.

**Acceptance Scenarios**:

1. **Given** the Run button is visible, **When** the user views the button text, **Then** the font size matches the button style's defined size (no inline override).

---

### Edge Cases

- What happens when the button style resource is not found at runtime? The button should still render usably (WPF falls back to default chrome).
- What happens when the button label text is very long (e.g., "Run 99 of 100 Tests")? The button should accommodate the text without clipping or breaking layout.
- What happens when the window is resized to minimum width? The header layout should not overlap the badge and button.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Run button on the Test Suite page MUST display with the app's premium gradient accent style (matching the existing `PrimaryButton` style) including gradient background, white text, rounded corners, and hover/press animations.
- **FR-002**: The button style MUST be defined as a reusable resource (either by referencing the existing `PrimaryButton` style or creating a `PrimaryButtonLarge` variant) so it can be maintained in one place.
- **FR-003**: When no tests are selected (button disabled), the tooltip MUST read "Select at least one test to run" instead of the generic action description.
- **FR-004**: When tests are selected (button enabled), the tooltip MUST describe the action (e.g., "Execute all selected tests and view results").
- **FR-005**: The button content font size MUST not override the style's defined font size via inline attributes, ensuring consistency.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The Run button is visually identifiable as the primary action within 1 second of viewing the Test Suite page — it stands out from the test count badge and other header elements.
- **SC-002**: 100% of users can determine why the Run button is disabled by hovering over it and reading the tooltip.
- **SC-003**: The Run button's visual style (gradient, hover, press states) matches the premium button style used elsewhere in the app (e.g., "Load Profile" button on the empty state).
- **SC-004**: No inline style overrides exist on the Run button that conflict with the referenced button style resource.
