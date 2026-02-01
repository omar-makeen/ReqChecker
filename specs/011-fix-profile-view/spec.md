# Feature Specification: Fix Profile Selector View

**Feature Branch**: `011-fix-profile-view`
**Created**: 2026-01-31
**Status**: Draft
**Input**: User description: "create spec to fix profile window when click on it make sure it looks premium aesthetic like the whole app"

## Problem Statement

The Profile Selector view crashes with a cascade of error dialogs when clicked/navigated to. The root cause is a WPF binding error: `A TwoWay or OneWayToSource binding cannot work on the read-only property 'Count'`. This occurs because `{Binding Tests.Count}` uses default TwoWay binding mode on `Run.Text`, but `Count` is a read-only property on `List<T>`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Navigate to Profiles Without Crash (Priority: P1)

As a user, I want to click on the Profiles navigation item and see the profile selector view load successfully without any errors.

**Why this priority**: This is a critical bug that completely blocks access to the Profiles feature. Without fixing this, users cannot select or manage profiles at all.

**Independent Test**: Can be fully tested by clicking "Profiles" in the navigation and verifying the view loads without error dialogs.

**Acceptance Scenarios**:

1. **Given** the application is running, **When** I click on "Profiles" in the navigation, **Then** the Profile Selector view loads without any error dialogs
2. **Given** I am on any other view, **When** I navigate to Profiles, **Then** the transition is smooth with no crashes or exceptions
3. **Given** I navigate to Profiles multiple times, **When** I switch back and forth between views, **Then** no errors accumulate or appear

---

### User Story 2 - View Profile Cards with Test Count (Priority: P1)

As a user, I want to see profile cards displaying the correct test count for each profile so I can understand what each profile contains.

**Why this priority**: Displaying profile metadata (including test count) is core functionality of the profile selector. This must work correctly after fixing the binding error.

**Independent Test**: Can be tested by loading a profile with known test count and verifying the displayed count matches.

**Acceptance Scenarios**:

1. **Given** a profile with 5 tests exists, **When** I view the Profile Selector, **Then** the profile card shows "5 tests"
2. **Given** a profile with 0 tests exists, **When** I view the Profile Selector, **Then** the profile card shows "0 tests"
3. **Given** multiple profiles with different test counts, **When** I view the Profile Selector, **Then** each card displays its correct test count

---

### User Story 3 - Premium Visual Consistency (Priority: P2)

As a user, I want the Profile Selector view to maintain the same premium, polished aesthetic as the rest of the application.

**Why this priority**: Visual consistency is important for user experience but secondary to fixing the crash. The current design already follows the app's aesthetic; this ensures it remains intact after the fix.

**Independent Test**: Can be tested by visual inspection comparing the Profile Selector view styling with other views in the app.

**Acceptance Scenarios**:

1. **Given** the Profile Selector view is displayed, **When** I compare it to other views, **Then** it uses the same color scheme, typography, and spacing
2. **Given** profile cards are displayed, **When** I hover over them, **Then** they show the same hover effects (accent border highlight) as other cards
3. **Given** the app is in dark mode, **When** I view the Profile Selector, **Then** all elements respect the dark theme

---

### Edge Cases

- What happens when the profile list is empty? (Should show empty state with import prompt)
- What happens when a profile has null/missing Tests collection? (Should handle gracefully, show 0 tests)
- What happens when loading profiles fails? (Should show error message with dismiss option)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST load the Profile Selector view without throwing binding exceptions
- **FR-002**: System MUST display the test count for each profile using read-only data binding
- **FR-003**: System MUST handle profiles with null or empty Tests collections gracefully
- **FR-004**: System MUST maintain visual consistency with the application's existing design system
- **FR-005**: System MUST preserve all existing functionality (select profile, import profile, refresh)

### Key Entities

- **Profile**: Represents a test configuration containing name, schema version, source (Bundled/UserProvided), and a collection of test definitions
- **TestDefinition**: Individual test within a profile (only the count is displayed in this view)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can navigate to Profile Selector 100% of the time without error dialogs appearing
- **SC-002**: Profile cards correctly display test counts for all loaded profiles
- **SC-003**: No binding errors appear in debug output when navigating to or interacting with the Profile Selector view
- **SC-004**: Visual appearance matches the premium aesthetic established in other views (dark surfaces, accent colors, smooth animations)

## Assumptions

- The fix involves changing binding mode from default (TwoWay) to OneWay for the `Tests.Count` binding
- No structural changes to the view model or data models are required
- The existing visual design is already premium and just needs the bug fix to be visible
