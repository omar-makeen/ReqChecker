# Feature Specification: Premium History Cards

**Feature Branch**: `026-premium-history-cards`
**Created**: 2026-02-06
**Status**: Draft
**Input**: User description: "Improve test history result cards with better date format, clearer duration label, descriptive pass rate, and labeled test status indicators — with premium/authentic design"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Readable Date & Time at a Glance (Priority: P1)

A user navigates to Test History and immediately understands **when** each test run happened without mentally parsing the date. The current format `Feb 06, 2026 12:07` uses 24-hour time and an abbreviated month that can feel ambiguous. The improved card displays a friendly relative date for recent runs (e.g., "Today at 12:07 PM", "Yesterday at 9:30 AM") and falls back to a full readable date for older runs (e.g., "Feb 6, 2026 at 12:07 PM") using 12-hour clock with AM/PM.

**Why this priority**: Date is the first piece of metadata users scan when comparing runs. If it's hard to read, the entire card loses scannability.

**Independent Test**: Can be verified by running tests at different times and confirming the card shows the correct friendly date format.

**Acceptance Scenarios**:

1. **Given** a test run completed today, **When** the user views the history card, **Then** the date shows "Today at [h:mm AM/PM]"
2. **Given** a test run completed yesterday, **When** the user views the history card, **Then** the date shows "Yesterday at [h:mm AM/PM]"
3. **Given** a test run completed more than 2 days ago, **When** the user views the history card, **Then** the date shows "[MMM d, yyyy] at [h:mm tt]" (e.g., "Feb 3, 2026 at 12:01 AM")

---

### User Story 2 - Labeled Duration with Icon (Priority: P1)

A user sees "4.6s" on the current card with no context. It is unclear whether this represents response time, duration, or something else. The improved card prefixes the duration with a timer icon and the word "Duration:" so the meaning is self-evident.

**Why this priority**: Without a label, a bare number followed by "s" is ambiguous and forces users to guess its meaning.

**Independent Test**: Can be verified by inspecting the card and confirming the duration field has a visible icon and "Duration:" label prefix.

**Acceptance Scenarios**:

1. **Given** a completed test run with duration 4.6 seconds, **When** the user views the history card, **Then** the duration displays with a timer icon and text "Duration: 4.6s"
2. **Given** a completed test run with duration 2 minutes 15 seconds, **When** the user views the history card, **Then** the duration displays "Duration: 2m 15s"
3. **Given** a completed test run with duration under 1 second, **When** the user views the history card, **Then** the duration displays "Duration: 0.3s" (appropriate decimal)

---

### User Story 3 - Descriptive Pass Rate with Visual Indicator (Priority: P1)

A user sees "75%" floating on the right side of the card with no context — it's unclear what this percentage represents. The improved card replaces the bare percentage with a labeled badge that reads "Pass Rate: 75%" and uses a color-coded background to communicate quality at a glance:

- **Green** (≥ 80%): Healthy pass rate
- **Amber/Yellow** (50%–79%): Needs attention
- **Red** (< 50%): Critical failures

**Why this priority**: The pass rate is the single most important metric on the card and must be instantly understandable.

**Independent Test**: Can be verified by creating runs with different pass rates and confirming the badge shows the correct label, percentage, and color.

**Acceptance Scenarios**:

1. **Given** a test run with 100% pass rate, **When** the user views the history card, **Then** a green badge displays "Pass Rate: 100%"
2. **Given** a test run with 75% pass rate, **When** the user views the history card, **Then** an amber/yellow badge displays "Pass Rate: 75%"
3. **Given** a test run with 25% pass rate, **When** the user views the history card, **Then** a red badge displays "Pass Rate: 25%"
4. **Given** a test run with 0% pass rate, **When** the user views the history card, **Then** a red badge displays "Pass Rate: 0%"

---

### User Story 4 - Labeled Test Status Indicators (Priority: P1)

A user sees colored dots with numbers (e.g., `● 3  ● 1  ● 0`) but has no way to know what green/red/yellow mean without prior knowledge. The improved card adds explicit text labels next to each status: "3 Passed", "1 Failed", "0 Skipped". Each label retains its color for quick scanning but no longer relies solely on color to convey meaning.

**Why this priority**: Accessibility and clarity — relying on color alone violates usability best practices and excludes colorblind users.

**Independent Test**: Can be verified by checking that each status indicator shows both a colored icon and a text label with the status name.

**Acceptance Scenarios**:

1. **Given** a run with 3 passed, 1 failed, 0 skipped, **When** the user views the card, **Then** the status row shows "3 Passed" (green), "1 Failed" (red), "0 Skipped" (yellow/amber) with both color and text labels
2. **Given** a run with 0 passed tests, **When** the user views the card, **Then** the passed indicator still displays "0 Passed" (not hidden)
3. **Given** a colorblind user, **When** they view the card, **Then** the text labels "Passed", "Failed", "Skipped" convey meaning without relying on color

---

### Edge Cases

- What happens when the duration is exactly 0 seconds? Display "Duration: 0s"
- What happens when a run has 0 total tests (edge case)? Display "Pass Rate: —" (em-dash) with neutral styling
- What happens when the date is from a different year? Display full date with year included
- How are very long profile names handled? Truncate with ellipsis; pass rate badge remains visible

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The date/time field MUST display "Today at [time]" for runs from the current calendar day, "Yesterday at [time]" for runs from the previous calendar day, and "[MMM d, yyyy] at [h:mm tt]" for all other dates
- **FR-002**: All times MUST use 12-hour format with AM/PM indicator
- **FR-003**: The duration field MUST display with a timer icon and "Duration:" label prefix
- **FR-004**: Duration values MUST use human-readable formatting: seconds with one decimal for < 60s (e.g., "4.6s"), minutes and seconds for ≥ 60s (e.g., "2m 15s")
- **FR-005**: The pass rate MUST display as a labeled badge reading "Pass Rate: [N]%" with color-coded background: green (≥ 80%), amber (50%–79%), red (< 50%)
- **FR-006**: Each test status indicator MUST display with both a colored icon and a text label: "[count] Passed", "[count] Failed", "[count] Skipped"
- **FR-007**: The card layout MUST maintain the existing premium design system (card elevation, border radius, hover effects, entrance animations)
- **FR-008**: All text labels MUST be visible in both light and dark themes
- **FR-009**: Status indicators MUST not rely solely on color to convey meaning (accessibility compliance)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can identify the date, duration, pass rate, and test statuses on a history card without any prior training or tooltip interaction
- **SC-002**: Every data field on the card has a visible label or icon that communicates its meaning
- **SC-003**: Pass rate health status (good/warning/critical) is distinguishable at a glance through both color and label
- **SC-004**: Card readability is maintained in both light and dark themes without clipping or overflow
- **SC-005**: Colorblind users can determine all test status meanings through text labels alone

### Assumptions

- The existing card structure (profile name, date row, status row, delete button, chevron) remains; this is a refinement, not a redesign
- The premium design system tokens (AccentPrimary, StatusPass, StatusFail, StatusSkip, BackgroundSurface, etc.) are already defined and available
- Duration values come from the `Duration` (TimeSpan) property on the RunReport model
- The 12-hour clock format with AM/PM is preferred for this application's user base
- Relative date labels ("Today", "Yesterday") are based on the local system calendar day
