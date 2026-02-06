# Feature Specification: Premium Results Dashboard

**Feature Branch**: `027-premium-results`
**Created**: 2026-02-06
**Status**: Draft
**Input**: User description: "Improve Results Dashboard formatting: 1) started date format, 2) duration, 3) profile name overflow, 4) any other issues found"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Readable Date and Duration in Run Summary (Priority: P1)

An IT administrator completes a test run and lands on the Results Dashboard. In the summary card at the top, they see the run's start time and duration. Currently, the date shows a raw machine format (e.g., `2/6/2026 12:07 PM +00:00`) and the duration shows a raw timespan (e.g., `00:00:04.6432312`). The user cannot quickly parse either value. After this improvement, the date displays in a human-friendly relative format (e.g., "Today at 12:07 PM") and the duration displays in a compact readable format (e.g., "4.6s").

**Why this priority**: These raw formats are visible on every single Results page visit. They are the most immediately noticeable quality issue and affect every user on every run.

**Independent Test**: Can be fully tested by running any test suite and checking that the summary card shows friendly date and compact duration values.

**Acceptance Scenarios**:

1. **Given** a test run completed today, **When** the user views the Results Dashboard summary card, **Then** the Started field shows "Today at h:mm AM/PM" format.
2. **Given** a test run completed yesterday, **When** the user views the Results Dashboard summary card, **Then** the Started field shows "Yesterday at h:mm AM/PM" format.
3. **Given** a test run completed more than two days ago, **When** the user views the Results Dashboard summary card, **Then** the Started field shows "MMM d, yyyy at h:mm AM/PM" format (e.g., "Feb 3, 2026 at 12:01 AM").
4. **Given** a test run with a duration under 1 second, **When** the user views the Results Dashboard summary card, **Then** the Duration field shows a compact millisecond format (e.g., "250ms").
5. **Given** a test run with a duration between 1 and 59 seconds, **When** the user views the Results Dashboard summary card, **Then** the Duration field shows a compact seconds format (e.g., "4.6s").
6. **Given** a test run with a duration of 60 seconds or more, **When** the user views the Results Dashboard summary card, **Then** the Duration field shows minutes and seconds (e.g., "2m 15s").

---

### User Story 2 - Profile Name Handles Long Text Gracefully (Priority: P2)

An IT administrator has a profile with a long descriptive name (e.g., "Enterprise Production Server Validation Suite - US East Region"). On the Results Dashboard summary card, the profile name currently overflows or pushes other elements out of position. After this improvement, long profile names are truncated with an ellipsis and the full name is accessible via a tooltip on hover.

**Why this priority**: While less common than the formatting issues, long profile names can break the layout entirely, making the summary card unusable.

**Independent Test**: Can be tested by loading a profile with a name longer than 40 characters and verifying the summary card layout remains intact.

**Acceptance Scenarios**:

1. **Given** a profile with a short name (under 40 characters), **When** the user views the Results Dashboard, **Then** the profile name displays in full without truncation.
2. **Given** a profile with a long name (over 40 characters), **When** the user views the Results Dashboard, **Then** the profile name is truncated with an ellipsis ("...") and the layout does not break.
3. **Given** a truncated profile name, **When** the user hovers over it, **Then** a tooltip shows the complete profile name.

---

### User Story 3 - Consistent Duration Format in Test Result Cards (Priority: P3)

When reviewing individual test results in the expandable cards below the summary, each card shows a duration in its subtitle. Currently, this displays raw milliseconds only (e.g., "1,234ms") regardless of the actual duration, which is inconsistent with the summary card format and unhelpful for longer tests. After this improvement, individual test result cards use the same compact duration format as the summary (e.g., "4.6s" or "1m 23s").

**Why this priority**: While functional, the inconsistency between summary and detail card duration formats creates a disjointed user experience. Lower priority because users can still read the values.

**Independent Test**: Can be tested by running a test suite with tests of varying durations and confirming all result cards display durations in the same compact format.

**Acceptance Scenarios**:

1. **Given** a test that completed in under 1 second, **When** viewing the result card, **Then** the subtitle shows duration in millisecond format (e.g., "250ms").
2. **Given** a test that completed in 5.3 seconds, **When** viewing the result card, **Then** the subtitle shows "5.3s" (not "5,300ms").
3. **Given** a test that completed in 90 seconds, **When** viewing the result card, **Then** the subtitle shows "1m 30s" (not "90,000ms").

---

### User Story 4 - Timezone Clarity on Start Time (Priority: P3)

The current raw date format includes a timezone offset (`+00:00`) which, while confusing in raw form, does communicate timezone. After switching to a friendly format, the timezone context could be lost. The friendly date should display in the user's local time (which the existing converter already does) so there is no ambiguity.

**Why this priority**: This is a detail that the friendly date converter already handles correctly by using local time, but it should be verified as part of this feature.

**Independent Test**: Can be tested by running a test and confirming the friendly date reflects the machine's local time, not UTC.

**Acceptance Scenarios**:

1. **Given** a test run completed at 2:00 PM local time, **When** the user views the Results Dashboard, **Then** the Started field shows "Today at 2:00 PM" (local time, not UTC).

---

### Edge Cases

- What happens when the duration is exactly 0 seconds (e.g., a skipped test)? It should display "0s".
- What happens when the profile name is empty or null? The label should show "Unknown Profile" or a graceful fallback.
- What happens when the start time is null or invalid? The date field should show a dash ("-") or "Unknown" rather than crashing.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Results Dashboard summary card MUST display the run start time in a human-friendly relative format ("Today at...", "Yesterday at...", or "MMM d, yyyy at h:mm tt") using the user's local timezone.
- **FR-002**: The Results Dashboard summary card MUST display the run duration in compact human-readable format ("0s", "Xms", "X.Xs", or "Xm Ys") instead of raw timespan.
- **FR-003**: The Results Dashboard summary card MUST truncate profile names that exceed the available display width, showing an ellipsis, and provide a tooltip with the full name.
- **FR-004**: Individual test result cards MUST display their duration in the same compact format as the summary card, ensuring consistency across the page.
- **FR-005**: The date and duration formatting MUST reuse the existing converters (FriendlyDateConverter, DurationFormatConverter) already used on the History page to maintain consistency across the application.
- **FR-006**: The summary card layout MUST remain visually intact with profile names up to 100 characters long.

## Assumptions

- The existing FriendlyDateConverter and DurationFormatConverter are production-ready and do not need modification (they were recently fixed in commit 847d198).
- Both converters are already registered as application-wide resources and can be referenced from the Results page without additional setup.
- The user's local timezone is the desired display timezone (consistent with the History page behavior).
- The ExpanderCard control's subtitle property accepts formatted strings and can be bound with a converter.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All date/time values on the Results Dashboard display in a user-friendly format — no raw ISO dates or timezone offsets visible anywhere on the page.
- **SC-002**: All duration values on the Results Dashboard (both summary and individual test cards) display in compact format — no raw timespan strings (containing colons or excessive decimal places) visible anywhere on the page.
- **SC-003**: Profile names up to 100 characters long do not break the summary card layout or cause horizontal overflow.
- **SC-004**: The formatting on the Results Dashboard is visually consistent with the Test History page (same date and duration styles).
