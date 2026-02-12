# Feature Specification: Improve Test Configuration Page UI/UX

**Feature Branch**: `036-test-config-ux`
**Created**: 2026-02-12
**Status**: Draft
**Input**: User description: "Check screenshot - test configuration window needs improvements, there are a lot of spaces. Improve UI/UX and anything else as UI/UX expert."

## Clarifications

### Session 2026-02-12

- Q: Should sections remain as separate cards (with reduced styling) or be merged into a single continuous form with dividers? → A: Keep separate cards, reduce styling (lighter borders, no glow, tighter padding).

## UI/UX Analysis (from screenshot)

The current Test Configuration page exhibits several visual and usability issues:

1. **Excessive vertical spacing**: Large gaps between form fields within each card create a sparse, disconnected feel. The 16px bottom margin on each field row combined with 20px section header margins and 20px card padding produces too much whitespace for a form with few fields.
2. **Oversized label column**: The fixed 140px label column is wider than necessary for labels like "Name", "Type", and "Retries", wasting horizontal space and pushing the input fields away from their labels.
3. **Visually heavy section cards**: The cyan/teal border glow effect on every card draws excessive attention to the container rather than the content. Combined with the dark surface background, this creates a "floating island" effect where cards feel disconnected from each other.
4. **Redundant lock icon column**: The lock icon occupies a separate column with its own bordered container, adding visual noise. A simpler inline indicator would suffice.
5. **Low information density**: Only ~7 fields are spread across 3 separate cards, requiring scrolling on smaller screens when the content could comfortably fit in a more compact layout.
6. **Inconsistent field alignment**: The "Requires Admin" row uses a badge-style display while other locked fields use the LockedFieldControl, breaking visual consistency.
7. **Section header icons are oversized**: 32x32 colored icon boxes are disproportionately prominent for section headers in a settings form.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Scan and Edit Configuration Quickly (Priority: P1)

As a user viewing a test's configuration, I want to see all fields in a compact, well-organized layout so I can quickly scan the test setup and edit what I need without excessive scrolling.

**Why this priority**: The primary purpose of the page is to view and edit test configuration. A dense, scannable layout directly improves task completion speed and reduces cognitive load.

**Independent Test**: Can be fully tested by navigating to any test's configuration page and verifying that all sections are visible without scrolling on a standard 1080p display, with clear visual hierarchy between read-only and editable fields.

**Acceptance Scenarios**:

1. **Given** a user opens the Test Configuration page for a test with 3-5 parameters, **When** the page loads, **Then** all three sections (Basic Information, Execution Settings, Test Parameters) are visible without scrolling on a 1080p display.
2. **Given** a user opens the Test Configuration page, **When** they scan the form, **Then** labels are visually close to their corresponding values with no excessive gap between label text and value.
3. **Given** a user opens the Test Configuration page, **When** they look at the form fields, **Then** locked (read-only) fields are visually distinct from editable fields but do not use excessive visual weight.

---

### User Story 2 - Distinguish Editable from Read-Only Fields (Priority: P2)

As a user, I want a clear but subtle visual distinction between fields I can edit and fields that are locked, so I immediately know where I can make changes without the read-only indicators being visually distracting.

**Why this priority**: Users need to quickly identify actionable areas. The current lock icon + bordered container approach is overly prominent and wastes horizontal space.

**Independent Test**: Can be tested by opening any test configuration and verifying that read-only fields have a muted/disabled appearance while editable fields have a standard input appearance, with no confusion about which fields accept input.

**Acceptance Scenarios**:

1. **Given** a user views a locked field (Name, Type), **When** they see the field, **Then** it appears as a muted, non-interactive text value without a separate lock icon container consuming horizontal space.
2. **Given** a user views an editable field (Timeout, Retries, editable parameters), **When** they see the field, **Then** it has a standard input appearance with visible border and focus states.
3. **Given** a user views the "Requires Admin" field, **When** they see the field, **Then** it displays consistently with other read-only fields rather than using a different visual treatment.

---

### User Story 3 - Visual Hierarchy Without Excessive Decoration (Priority: P2)

As a user, I want the page sections to have clear visual grouping without heavy borders, glows, or oversized icons that compete for attention with the actual content.

**Why this priority**: The current glowing card borders and large colored icon badges create visual clutter. A subtler approach improves readability and focus.

**Independent Test**: Can be tested by opening the test configuration page and verifying that section groupings are clear through spacing and subtle separators rather than heavy card styling, and that section header icons are proportional to the heading text.

**Acceptance Scenarios**:

1. **Given** a user views the Test Configuration page, **When** they scan the sections, **Then** section headers use smaller, inline icons that complement rather than dominate the heading text.
2. **Given** a user views the Test Configuration page, **When** they look at the overall layout, **Then** sections are visually grouped without heavy border glows or drop shadows.
3. **Given** a user views the page, **When** they focus on content, **Then** the visual weight is on the field labels and values, not on container decoration.

---

### Edge Cases

- What happens when a test has many parameters (10+)? The compact layout should still allow comfortable scrolling with clear field separation.
- What happens when a parameter label is very long? Labels should truncate with an ellipsis and show the full text in a tooltip.
- What happens on high-DPI displays? Spacing and sizing should scale appropriately.
- What happens when the window is resized to a narrow width? The form should remain usable with fields adjusting gracefully.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The page MUST reduce vertical spacing between form field rows to increase information density.
- **FR-002**: The page MUST reduce the fixed label column width to a narrower value that still accommodates the longest label ("Requires Admin") while keeping labels close to their values.
- **FR-003**: Locked fields MUST display as inline muted text without a separate bordered lock icon container. A small inline lock icon may appear next to the value as a subtle indicator.
- **FR-004**: The section header icon badges MUST be reduced in size to maintain proportionality with the heading text.
- **FR-005**: The section header bottom margin MUST be reduced to tighten the gap between headers and content.
- **FR-006**: The card inner padding MUST be reduced to decrease wasted space within sections.
- **FR-007**: The gap between section cards MUST be reduced to bring sections closer together.
- **FR-008**: The "Requires Admin" field MUST use the same visual treatment as other locked/read-only fields for consistency.
- **FR-009**: The page outer margin MUST be reduced to maximize the content area.
- **FR-010**: The three-section card structure MUST be retained, but card border glow/drop shadow effects MUST be removed and borders made lighter to decrease visual weight on containers.
- **FR-011**: All existing functionality (saving, canceling, back navigation, field policies, entrance animations) MUST be preserved.
- **FR-012**: Long parameter labels MUST truncate with an ellipsis and show the full text in a tooltip.

### Key Entities

- **Test Configuration Form**: The central form containing three sections (Basic Info, Execution Settings, Test Parameters) that allows viewing and editing test settings.
- **Form Field**: A label-value pair that can be in one of four states: Editable, Locked, PromptAtRun, or Hidden.

## Assumptions

- The 1080p (1920x1080) display is the baseline target for fitting all content without scrolling.
- The maximum number of visible parameters on a single test is typically under 10.
- The existing entrance animation stagger effect is desired and should be preserved.
- Color theme resources (AccentPrimary, AccentSecondary, etc.) remain unchanged; only sizing, spacing, and visual weight are adjusted.
- The LockedFieldControl can be simplified in-place to remove the separate lock icon container.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All three configuration sections (Basic Information, Execution Settings, Test Parameters) are fully visible without scrolling on a 1080p display for tests with up to 5 parameters.
- **SC-002**: The vertical space consumed by the form content (excluding header and footer) is reduced by at least 25% compared to the current layout.
- **SC-003**: Users can identify editable vs. read-only fields within 2 seconds of viewing the page.
- **SC-004**: The visual weight ratio shifts from container decoration to content — section borders and icons occupy less visual prominence than field labels and values.
- **SC-005**: No existing functionality is broken: save, cancel, back navigation, field policies, and animations all continue to work as before.
