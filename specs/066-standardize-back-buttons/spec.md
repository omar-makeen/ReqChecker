# Feature Specification: Standardize Back Buttons

**Feature Branch**: `066-standardize-back-buttons`
**Created**: 2026-03-14
**Status**: Draft
**Input**: User description: "Standardize back button style, position, and label across all views"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent Back Button Position (Priority: P1)

A user navigates between pages (Test Config, Results, History, Run Progress) and always finds the back button in the same place — top-left of the page header, before the title. This matches the universal UX convention (browsers, mobile apps, Windows Settings) where users instinctively look top-left to navigate backward.

**Why this priority**: Position is the strongest usability factor. Users develop muscle memory for navigation. When the back button moves between pages (left on TestConfigView, right on ResultsView), users waste time scanning the header to find it. Consistent placement eliminates this friction.

**Independent Test**: Navigate to each page that has a back button and confirm it appears in the same position — top-left of the header area (Column 0), before the page title and icon.

**Acceptance Scenarios**:

1. **Given** a user is on the Results Dashboard, **When** they look at the page header, **Then** the back button is positioned on the left side before the page title.
2. **Given** a user is on the Test History page, **When** they look at the page header, **Then** the back button is in the same left-side position as on Results.
3. **Given** a user is on the Run Progress page (after tests complete), **When** they look at the page header, **Then** the back button is in the same left-side position as on Results and History.
4. **Given** a user is on the Test Config page, **When** they look at the page header, **Then** the back button remains in its current left-side position (already correct).

---

### User Story 2 - Consistent Back Button Style (Priority: P2)

All back buttons use the same visual style so they are recognizable as the same type of action. The style should be secondary (not primary) — prominent enough to find but not competing with forward-action CTAs like "Run Tests" or "Export".

**Why this priority**: Using three different styles (GhostButton, SecondaryButton, PrimaryButton) for the same action breaks visual consistency. The RunProgressView back button currently uses PrimaryButton (gradient CTA), which is the wrong semantic — back is not a primary action. Standardizing to SecondaryButton balances visibility with proper hierarchy.

**Independent Test**: Navigate to each page with a back button and confirm they all share the same visual appearance — outlined border style with accent color, no gradient fill.

**Acceptance Scenarios**:

1. **Given** a user is on the Run Progress page (completed), **When** they see the back button, **Then** it displays as a secondary-style button (outlined, not gradient-filled), visually distinct from the primary CTA.
2. **Given** a user is on the Test Config page, **When** they see the back button, **Then** it displays with the same secondary style as back buttons on other pages (not ghost/invisible style).
3. **Given** a user navigates between Results, History, and Run Progress, **When** they see the back button on each page, **Then** all back buttons look identical in color, border, and hover behavior.

---

### User Story 3 - Consistent Back Button Label (Priority: P3)

All back buttons use the same label text ("Back to Tests") and the same left-arrow icon, making the action immediately recognizable across all pages.

**Why this priority**: Minor polish, but "Back" vs "Back to Tests" vs "Go to Tests" creates unnecessary inconsistency. A descriptive label ("Back to Tests") tells users exactly where they'll go. The empty-state "Go to Tests" button in ResultsView is excluded — it's a navigation CTA in an empty state, not a back button.

**Independent Test**: Navigate to each page with a back button and confirm they all display "Back to Tests" with an ArrowLeft icon.

**Acceptance Scenarios**:

1. **Given** a user is on the Test Config page, **When** they see the back button, **Then** it reads "Back to Tests" with a left-arrow icon (currently just "Back").
2. **Given** a user is on any page with a back button, **When** they see it, **Then** the text is "Back to Tests" with a left-arrow icon — identical across all pages.

---

### Edge Cases

- What happens when the back button is on a page with many header action buttons (e.g., ResultsView has "Re-run Failed" and "Export")? The left-positioned back button should not overlap or crowd the title. Adequate spacing between back button and title is needed.
- What happens on the TestConfigView which has unsaved changes detection on the back button? The behavior (unsaved changes prompt) must be preserved even after restyling.
- What happens when the sidebar is collapsed (compact mode)? The back button position should remain consistent regardless of sidebar state.
- What happens with the empty-state "Go to Tests" button on ResultsView? It should remain unchanged — it's a different navigation context, not a back button.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Back buttons on ResultsView, HistoryView, and RunProgressView MUST be positioned on the left side of the page header (before the page title), matching TestConfigView's current layout.
- **FR-002**: All back buttons MUST use the same button style — a secondary-level style that is visible but does not compete with primary action buttons.
- **FR-003**: All back buttons MUST display the text "Back to Tests" with an ArrowLeft icon.
- **FR-004**: The tooltip for all back buttons MUST read "Return to the test suite".
- **FR-005**: The TestConfigView back button MUST preserve its existing unsaved-changes prompt behavior after restyling.
- **FR-006**: The RunProgressView back button MUST remain visible only after test execution completes (existing conditional visibility preserved).
- **FR-007**: The empty-state "Go to Tests" button on ResultsView MUST NOT be changed — it is a navigation CTA, not a back button.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All four back buttons (TestConfig, Results, History, RunProgress) are in the same position — users can find the back button without scanning the header on any page.
- **SC-002**: All four back buttons are visually identical in style, icon, label, and tooltip.
- **SC-003**: No existing functionality is broken — unsaved changes prompt on TestConfig and conditional visibility on RunProgress both continue to work correctly.
- **SC-004**: The empty-state "Go to Tests" button on ResultsView is unchanged.
