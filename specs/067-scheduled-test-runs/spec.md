# Feature Specification: Scheduled Test Runs

**Feature Branch**: `067-scheduled-test-runs`
**Created**: 2026-03-16
**Status**: Draft
**Input**: User description: "I need to add feature to schedule run all test at specific time. I need to support one-time and periodical and more flexibility to schedule tests run"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Schedule a One-Time Test Run (Priority: P1)

As a user, I want to schedule a test run to execute at a specific future date and time so that I can run tests during off-hours or maintenance windows without needing to be present.

The user opens the test suite, chooses "Schedule Run," picks a date and time, and confirms. The application executes the tests at the scheduled time and stores the results for later review.

**Why this priority**: One-time scheduling is the fundamental building block of the feature. Without it, no other scheduling capability is possible. It delivers immediate value by letting users set-and-forget a test run.

**Independent Test**: Can be fully tested by scheduling a run 2 minutes in the future, waiting for it to execute, and verifying results appear in history.

**Acceptance Scenarios**:

1. **Given** the user has a profile loaded with tests, **When** they select "Schedule Run" and pick a future date/time and confirm, **Then** a scheduled run entry is created and visible in the schedule list.
2. **Given** a scheduled one-time run exists, **When** the scheduled time arrives, **Then** the application automatically executes all tests in the profile and saves the results to history.
3. **Given** a one-time run has completed, **Then** the schedule entry is marked as "Completed" and does not run again.
4. **Given** the user schedules a run, **When** they select a date/time in the past, **Then** the system rejects the schedule with a clear validation message.

---

### User Story 2 - Schedule Recurring Test Runs (Priority: P2)

As a user, I want to schedule recurring test runs (e.g., every day at 9 AM, every Monday, every hour) so that I can continuously monitor system health without manual intervention.

The user creates a recurring schedule by choosing a recurrence pattern (interval-based or calendar-based), setting start time, and optionally an end date. The application runs tests at each scheduled occurrence.

**Why this priority**: Recurring schedules are the most valuable scheduling capability for ongoing monitoring. It builds on the one-time scheduling infrastructure and is the primary use case for most users who want automated, hands-off testing.

**Independent Test**: Can be tested by creating a recurring schedule with a 5-minute interval, verifying at least two consecutive executions occur, and checking both results appear in history.

**Acceptance Scenarios**:

1. **Given** the user has a profile loaded, **When** they create a recurring schedule with a daily recurrence at a specific time, **Then** the system executes tests at that time every day.
2. **Given** a recurring schedule is active, **When** the next occurrence time arrives, **Then** the tests run automatically and results are saved to history with the schedule name.
3. **Given** a recurring schedule has an end date, **When** the end date passes, **Then** the schedule is marked as "Expired" and no further runs occur.
4. **Given** a recurring schedule is active, **When** the user pauses it, **Then** no further runs occur until the user resumes it.

---

### User Story 3 - View and Manage Scheduled Runs (Priority: P2)

As a user, I want to view all my scheduled runs in one place, see their status, and be able to edit, pause, resume, or delete them so that I have full control over my testing schedule.

The user navigates to a "Schedules" page that lists all scheduled runs with their status, next run time, and recurrence pattern. They can take actions on each schedule.

**Why this priority**: Without management capabilities, users cannot correct mistakes, adapt to changing needs, or maintain control over scheduled runs. This is essential for the feature to be usable in practice.

**Independent Test**: Can be tested by creating multiple schedules (one-time and recurring), verifying they all appear in the schedules list with correct details, and performing edit/delete/pause operations.

**Acceptance Scenarios**:

1. **Given** the user has created multiple schedules, **When** they navigate to the Schedules page, **Then** they see a list of all schedules showing name, type (one-time/recurring), next run time, and status.
2. **Given** a schedule exists, **When** the user edits it and changes the time, **Then** the schedule updates and the next run uses the new time.
3. **Given** a schedule exists, **When** the user deletes it, **Then** the schedule is removed and no future runs occur.
4. **Given** a recurring schedule is running, **When** the user pauses it, **Then** the status changes to "Paused" and the schedule skips all occurrences until resumed.

---

### User Story 4 - Schedule Notifications and Missed Run Handling (Priority: P3)

As a user, I want to know when a scheduled run completes (or fails to run because the application was closed) so that I don't miss important test results or failures.

When a scheduled run completes, the application shows a notification. If the application was not running when a schedule was due, it detects the missed run on next launch and notifies the user.

**Why this priority**: Notifications and missed-run handling improve reliability and trust in the scheduling system, but the core scheduling functionality works without them.

**Independent Test**: Can be tested by scheduling a run, closing the application before the scheduled time, reopening it after the scheduled time, and verifying a "missed run" notification appears.

**Acceptance Scenarios**:

1. **Given** a scheduled run begins executing (whether the app window is open or minimized to tray), **Then** a Windows toast notification appears showing "Schedule X is now running..."
2. **Given** a scheduled run completes (whether the app window is open or minimized to tray), **Then** a Windows toast notification appears showing the schedule name and summary count (e.g., "12/15 tests passed") with an action to view results. The notification persists in Windows Action Center.
3. **Given** the application was closed during a scheduled run time, **When** the application is next launched, **Then** it detects the missed run and shows an in-app dialog listing the missed schedules with "Run Now" and "Dismiss" actions.
4. **Given** a recurring schedule had multiple missed occurrences while the app was closed, **When** the application launches, **Then** it reports the missed runs but only offers to run once (not replay all missed occurrences).

---

### Edge Cases

- What happens when two schedules overlap (same time)? They run sequentially in creation order.
- What happens if the system clock changes (daylight saving, manual change)? Schedules use local time and adjust for DST transitions; a clear indication of timezone is shown.
- What happens if a scheduled run is in progress when the next occurrence is due? The next occurrence is skipped with a "Skipped (previous run still in progress)" status logged.
- What happens if the loaded profile is modified after a schedule is created? The schedule runs with the current profile state at execution time, not the state when the schedule was created.
- What happens if the profile file is deleted or becomes invalid? The scheduled run fails gracefully with an error status and the user is notified on next app launch.
- What happens when the application is closed? Schedules persist but cannot execute; missed runs are detected on next launch.

## Clarifications

### Session 2026-03-16

- Q: Should the app stay open as a normal window, minimize to system tray, or offer a setting? → A: App minimizes to system tray when the user closes the window; schedules continue executing while the app is in the tray.
- Q: Are schedules tied to the currently loaded profile, or do they auto-load their associated profile? → A: Schedules store the profile file path and auto-load it at execution time, regardless of what profile is currently loaded.
- Q: What is the minimum allowed recurrence interval for recurring schedules? → A: 5 minutes minimum.
- Q: Should schedules support selective test runs or always run the full profile? → A: Always run all tests in the profile. Users can create smaller profiles for partial runs.
- Q: How should notifications be displayed for scheduled run completions? → A: Windows toast notifications (system-level); they appear even when the app is minimized to tray and persist in Windows Action Center.
- Q: What level of detail should the toast notification show? → A: Schedule name plus summary count (e.g., "12/15 tests passed").
- Q: How should missed run notifications be displayed on app launch? → A: In-app dialog showing missed runs with "Run Now" and "Dismiss" actions.
- Q: Should a notification appear when a scheduled run starts executing? → A: Yes, a Windows toast notification: "Schedule X is now running..." (brief, informational).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to create a one-time scheduled test run with a specific future date and time.
- **FR-002**: System MUST allow users to create recurring scheduled test runs with the following recurrence patterns: hourly, daily, weekly (with day selection), and custom interval (every N minutes/hours/days, with a minimum interval of 5 minutes).
- **FR-003**: System MUST auto-load the profile associated with a schedule and execute all its tests when the scheduled time arrives, regardless of which profile is currently loaded in the UI.
- **FR-004**: System MUST save scheduled run results to test history, tagged with the schedule name for identification.
- **FR-005**: System MUST persist schedule definitions so they survive application restarts.
- **FR-006**: System MUST provide a dedicated Schedules page listing all schedules with their name, type, status, and next run time.
- **FR-007**: System MUST allow users to edit, delete, pause, and resume existing schedules.
- **FR-008**: System MUST validate that one-time schedules are set to a future date/time and that recurring schedules have an interval of at least 5 minutes before accepting them.
- **FR-009**: System MUST allow recurring schedules to optionally have an end date, after which they automatically expire.
- **FR-010**: System MUST detect missed scheduled runs (due to application being closed) and show an in-app dialog on next launch listing the missed runs with "Run Now" and "Dismiss" actions.
- **FR-011**: System MUST prevent overlapping executions — if a scheduled run is still in progress when the next occurrence is due, the next occurrence is skipped.
- **FR-012**: System MUST show a Windows toast notification when a scheduled run **starts**, displaying the schedule name (e.g., "Schedule X is now running..."). The toast MUST appear even when the app is minimized to system tray.
- **FR-017**: System MUST show a Windows toast notification when a scheduled run **completes**, displaying the schedule name and a summary count (e.g., "12/15 tests passed") with an action to view results. The toast MUST persist in Windows Action Center.
- **FR-013**: System MUST allow users to give each schedule a descriptive name for identification.
- **FR-014**: System MUST display the recurrence pattern in human-readable format (e.g., "Every Monday at 9:00 AM", "Every 2 hours").
- **FR-015**: System MUST minimize to the system tray when the user closes the main window, keeping scheduled runs active.
- **FR-016**: System MUST provide a tray icon with a context menu allowing the user to restore the window, view next scheduled run, or fully exit the application.

### Key Entities

- **Schedule**: Represents a scheduled test run. Contains a name, the associated profile file path (auto-loaded at execution time), schedule type (one-time or recurring), recurrence pattern, next run time, status (Active, Paused, Completed, Expired, Missed), and creation timestamp.
- **Recurrence Pattern**: Defines the repeat behavior of a recurring schedule. Includes frequency type (hourly, daily, weekly, custom interval), interval value, selected days (for weekly), start time, and optional end date.
- **Schedule Execution Record**: Links a schedule to its run history. Contains the schedule reference, execution timestamp, outcome (completed, skipped, missed, failed), and reference to the corresponding history entry.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can create a one-time or recurring schedule in under 60 seconds from the test list page.
- **SC-002**: Scheduled test runs execute within 30 seconds of their scheduled time when the application is running.
- **SC-003**: 100% of scheduled run results are saved to history and retrievable after completion.
- **SC-004**: Missed runs are detected and reported to the user within 10 seconds of application launch.
- **SC-005**: Users can view, edit, pause, resume, or delete any schedule in 3 or fewer interactions from the Schedules page.
- **SC-006**: Schedule definitions persist across application restarts with zero data loss.
- **SC-007**: The scheduling feature supports at least 50 concurrent active schedules without performance degradation.

## Assumptions

- The application must be running (including minimized to system tray) for scheduled tests to execute. Closing the window minimizes to tray rather than exiting. Users can fully exit via the tray icon context menu. Missed runs (due to full exit or system shutdown) are detected on next launch.
- Schedules are stored locally on the user's machine (consistent with existing data persistence patterns using `%APPDATA%/ReqChecker/`).
- The scheduling feature uses the system's local time for all schedule definitions and displays.
- When a scheduled run executes, it runs all tests in the profile (selective test scheduling is out of scope for this feature).
- Schedule history entries follow the same format as manual run history entries, with an additional tag identifying the source schedule.
- The user must have a profile loaded to create a schedule. The schedule stores the profile file path and auto-loads it at execution time, independent of the currently active profile in the UI.
