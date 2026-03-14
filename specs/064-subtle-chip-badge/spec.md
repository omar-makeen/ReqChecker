# Feature Specification: Subtle Chip Badge

**Feature Branch**: `064-subtle-chip-badge`
**Created**: 2026-03-14
**Status**: Draft
**Input**: User description: "Restyle test count badge to subtle chip style — muted background, secondary text, smaller padding — so it reads as informational rather than actionable"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Test Count Badge Reads as Informational on Page Header (Priority: P1)

When a user loads a profile and views the Test Suite page, the test count badge ("8 tests") should appear visually distinct from the "Run All Tests" action button. The badge should use a muted surface background, secondary-color text, and smaller padding so it reads as a passive information chip — not a competing call-to-action.

**Why this priority**: This is the core problem reported by the user. The current badge uses the same accent color and pill shape as the primary CTA, creating visual confusion about what is clickable versus informational.

**Independent Test**: Load a profile with tests, navigate to the Test Suite page, and visually confirm the test count badge is clearly distinguishable from the "Run All Tests" button. The badge should feel passive/informational; the button should feel like the primary action.

**Acceptance Scenarios**:

1. **Given** a profile with 8 tests is loaded, **When** the user views the Test Suite page header, **Then** the test count badge appears with a muted surface background (not accent-colored), secondary text color, regular font weight, and smaller padding than the CTA button.
2. **Given** a profile with 8 tests is loaded, **When** the user views the Test Suite page header, **Then** the "Run All Tests" button is the only accent-colored, visually prominent element in the action area — the badge does not compete for attention.
3. **Given** the app is in light theme, **When** the user views the Test Suite page header, **Then** the badge remains visually muted and readable against the light background.
4. **Given** a profile with 150 tests is loaded, **When** the user views the Test Suite page, **Then** the badge displays "150 tests" without clipping or overflow, maintaining its pill shape.

---

### User Story 2 - Sidebar Badge Consistency (Priority: P2)

The sidebar navigation badge on the "Test Suite" nav item should also be restyled to use a muted/subtle appearance consistent with the page header badge, so both badges share the same "informational chip" visual language.

**Why this priority**: Visual consistency between sidebar and page header reinforces that both badges serve the same informational role. Lower priority because the sidebar badge is smaller and the visual confusion is less pronounced there.

**Independent Test**: Load a profile, observe the sidebar "Test Suite" nav item badge, confirm it uses the same muted chip style as the page header badge. Check both expanded and compact sidebar modes.

**Acceptance Scenarios**:

1. **Given** a profile with tests is loaded, **When** the user views the sidebar in expanded mode, **Then** the test count badge next to "Test Suite" uses a muted surface background and secondary text color — not the accent color.
2. **Given** a profile with tests is loaded, **When** the sidebar is in compact mode (icons only), **Then** the badge is hidden (since there is no room for it next to an icon-only nav item).
3. **Given** the app switches between dark and light themes, **When** the user views the sidebar badge, **Then** the badge colors adapt to remain readable in both themes.

---

### Edge Cases

- What happens when the test count is 0? The badge should be hidden entirely (existing behavior, no change needed).
- What happens with very large numbers (e.g., 999 tests)? The badge should expand horizontally to fit without clipping.
- What happens when no profile is loaded? The badge should be hidden entirely (existing behavior, no change needed).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The page header test count badge MUST use a muted surface background color (e.g., `BackgroundSurface` or `BackgroundElevated`) instead of the accent color.
- **FR-002**: The page header test count badge MUST use a secondary or tertiary text color instead of white.
- **FR-003**: The page header test count badge MUST use regular (not bold/semibold) font weight.
- **FR-004**: The page header test count badge MUST have smaller padding than the primary CTA button so it appears compact.
- **FR-005**: The page header test count badge MUST have a subtle border (1px, `BorderSubtle` or similar) to define its shape without drawing attention.
- **FR-006**: The sidebar nav badge MUST be restyled to use the same muted chip appearance as the page header badge.
- **FR-007**: Both badges MUST remain readable and visually appropriate in both dark and light themes.
- **FR-008**: The badge styling MUST NOT introduce any new interactive behavior — the badge remains non-clickable and purely informational.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can immediately distinguish the test count badge from the "Run All Tests" button at a glance — the badge reads as informational, the button reads as actionable.
- **SC-002**: Both badges (page header and sidebar) use a consistent muted chip style across all views where they appear.
- **SC-003**: Both badges remain legible in dark and light themes without manual intervention.
- **SC-004**: No existing tests are broken by the restyling.

## Assumptions

- The existing badge visibility logic (hidden when no profile or 0 tests) is correct and does not need changes.
- The badge text content ("X tests" on page, count-only on sidebar) remains unchanged.
- Only the visual styling (colors, padding, font weight, border) changes — no structural XAML layout changes needed.
- The `BackgroundSurface` and `BorderSubtle` theme resources already exist and are appropriate for this use case.
