# Feature Specification: Profile Manager List Redesign (Premium UI/UX)

**Feature Branch**: `068-profile-list-redesign`
**Created**: 2026-04-30
**Status**: Draft
**Input**: User description: "proceed with option A and make sure that app is premuim UI/UX"

## Overview

The Profile Manager screen currently presents profiles as a wrapping grid of fixed-width cards. Every other primary list screen in the app (Test List, History, Schedules) presents items as a single vertical column of full-width rows. This visual inconsistency makes the Profile Manager feel disconnected from the rest of the application.

This feature redesigns the Profile Manager so that profiles are displayed as a vertical list of full-width rows that match the visual rhythm and interaction model of the other list screens, and elevates the entire profile-selection experience so it feels coherent, calm, and premium throughout.

## Clarifications

### Session 2026-04-30

- Q: What level of accessibility commitment should this feature carry? → A: Match the rest of the app's baseline — keyboard parity, visible focus, accessible names on rows, and list/listbox semantics — without committing this feature to a formal WCAG audit.
- Q: When the user returns to Profile Manager later, should the currently-active profile be marked as selected? → A: Yes — show the currently-active profile as visually selected (distinct from hover and focus).
- Q: For the per-row recency indicator, which timestamp is canonical when both are available? → A: The underlying file's last-modified date (e.g., "modified Apr 28" / "modified 3 days ago"); import date is not displayed.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Profiles match the rest of the app visually (Priority: P1)

A returning user opens the Profile Manager. They expect it to look and behave like Test List, History, and Schedules — a single vertical column of full-width rows, scannable from top to bottom, with the same spacing, hover treatment, and rhythm.

**Why this priority**: This is the explicit user request and the root cause of the "doesn't match" complaint. Without it, the rest of the polish has nowhere to land.

**Independent Test**: Open Profile Manager and a second list screen (e.g., Test List) side-by-side. The list-item layout pattern (row width, row height range, vertical spacing, divider/elevation treatment, hover effect) must read as the same component family.

**Acceptance Scenarios**:

1. **Given** the user opens the Profile Manager, **When** the screen renders, **Then** profiles appear in a single vertical column of full-width rows (no grid, no fixed-width tiles, no horizontal wrapping).
2. **Given** the user resizes the window from narrow to wide, **When** the layout reflows, **Then** rows always span the available content width and the list never produces dead horizontal space or two-column wrapping.
3. **Given** the user navigates between Profile Manager, Test List, History, and Schedules, **When** they compare the lists, **Then** the row pattern, padding, hover/focus treatment, and item spacing read as belonging to the same design system.

---

### User Story 2 — Selecting a profile takes one obvious action (Priority: P1)

A user wants to load a specific profile. Today, clicking the card and clicking a "Select Profile" button on every card both work, which produces visual noise and ambiguity. The redesigned row makes selection one clear, full-row interaction.

**Why this priority**: Profile selection is the primary action of this screen. Removing redundant affordances and clarifying the click target is the single biggest usability win.

**Independent Test**: Open Profile Manager, click anywhere on a profile row. The profile loads. There is no per-row "Select Profile" button competing for attention.

**Acceptance Scenarios**:

1. **Given** the user hovers a profile row, **When** the row is hoverable, **Then** the cursor indicates a clickable element and the row shows a subtle hover treatment that is consistent with other list rows in the app.
2. **Given** the user clicks anywhere on a profile row (excluding any explicit secondary controls), **When** the click is received, **Then** the profile is selected and loaded.
3. **Given** the user activates a profile row using the keyboard (Enter or Space), **When** the row has focus, **Then** the profile is selected and loaded.
4. **Given** the user inspects the row, **When** they look for a "Select Profile" button on every row, **Then** no such redundant per-row button is present.

---

### User Story 3 — The recommended profile is identifiable at a glance with one signal (Priority: P2)

A first-time or returning user needs to spot the recommended profile immediately, without three competing visual signals (today: pill + accent border + gradient stripe).

**Why this priority**: First-run users rely on the recommended hint to choose confidently. Premium UI/UX uses one strong, unambiguous signal per piece of information.

**Independent Test**: Open Profile Manager with at least one profile marked recommended. The recommended profile is visually distinguishable at a glance, but only one design signal carries that meaning.

**Acceptance Scenarios**:

1. **Given** at least one profile is recommended, **When** the list renders, **Then** the recommended profile is identifiable within 2 seconds by an unprimed user.
2. **Given** a recommended profile, **When** inspecting its visual treatment, **Then** exactly one design element (a labeled badge) communicates "Recommended" — no additional border, gradient stripe, or background tint duplicates that meaning on the same row.
3. **Given** no profile is recommended, **When** the list renders, **Then** all rows share identical baseline styling with no orphaned decoration.

---

### User Story 4 — Each row carries enough information to choose confidently (Priority: P2)

The user is comparing several profiles. Each row must surface the metadata needed to decide without opening the profile: profile name, source, number of tests, schema version, and a sense of recency.

**Why this priority**: Today's cards under-utilize their space. The redesigned rows have more horizontal real estate and should pay it back in scannable metadata.

**Independent Test**: With at least three profiles loaded, the user can answer "which profile has the most tests?" and "which is the newest?" without clicking anything.

**Acceptance Scenarios**:

1. **Given** a profile row, **When** it renders, **Then** it displays the profile name as the dominant text element.
2. **Given** a profile row, **When** it renders, **Then** it displays — in a calm, secondary text style — the source, test count, schema version, and a recency indicator showing the underlying file's last-modified date (e.g., "modified Apr 28" or "modified 3 days ago").
3. **Given** a row whose name is too long to fit, **When** the row renders, **Then** the name truncates with an ellipsis and the full name is available via tooltip.
4. **Given** the source label, **When** it renders, **Then** it appears as a quiet metadata chip (outlined or muted), not as a colored solid pill that competes with primary actions.
5. **Given** a profile is currently loaded as the active profile, **When** the user returns to the Profile Manager, **Then** that profile's row is shown in the selected/active visual state, distinct from hover and focus, and is announced as the selected item to assistive technologies.

---

### User Story 5 — The list feels premium under interaction (Priority: P3)

Hover, focus, selection, and entry transitions feel intentional and calm, not noisy or busy. Motion respects reduced-motion preferences. Keyboard parity is complete.

**Why this priority**: "Premium" is a felt quality. Once layout and information design are right, interaction polish carries the perception across the line.

**Independent Test**: Navigate the full screen using only the keyboard; trigger every state (hover, focus, active, selected, disabled if applicable). Each transition is smooth, consistent with other list screens, and never longer than 250 ms.

**Acceptance Scenarios**:

1. **Given** the user tabs into the list, **When** focus lands on a row, **Then** the focus ring is clearly visible and matches the focus pattern used elsewhere in the app.
2. **Given** the user presses Up/Down arrow keys with focus in the list, **When** keys are received, **Then** focus moves between rows predictably and the focused row scrolls into view if needed.
3. **Given** the user has any system-level "reduce motion" preference enabled (where the platform exposes it), **When** the list renders, **Then** entrance and hover animations are reduced or omitted.
4. **Given** the user hovers a row, **When** the hover state activates, **Then** the transition is subtle (no large color flash, no layout shift) and reaches its end state within 200 ms.

---

### User Story 6 — Empty, loading, and error states feel coherent with the redesign (Priority: P3)

When the list is empty, loading, or shows an error, the surrounding chrome feels like one calm system, not a different screen.

**Why this priority**: Edge states are where premium products typically lose their polish. Keeping them coherent finishes the feature.

**Independent Test**: Trigger each of: zero profiles, profiles loading, and an error state. Each looks like a deliberate part of the same screen.

**Acceptance Scenarios**:

1. **Given** no profiles exist, **When** the screen loads, **Then** the empty state is centered, uses the established empty-state composition, and clearly invites the user to import a profile.
2. **Given** profiles are still loading, **When** the screen renders, **Then** a single, centered progress indicator shows with a short label, without the list shell flashing into view first.
3. **Given** an error occurs while loading profiles, **When** the error banner shows, **Then** it appears in a fixed position above or below the header (consistent with other screens) and does not stack visually with the welcome banner or the page header decoration.
4. **Given** the welcome banner is visible, **When** the user sees it next to the page header, **Then** the two elements do not duplicate the same visual treatment (gradient accent line + colored icon tile + title/subtitle) — visual hierarchy reads as page header first, supporting message second.

---

### Edge Cases

- A profile name is extremely long (> 80 characters) — the row must truncate cleanly with the full name available on hover, without breaking row alignment.
- Two profiles share the same name — the row must surface enough secondary metadata (source, file path or directory) to disambiguate.
- A profile is missing optional metadata (e.g., no schema version, no recency timestamp) — the row gracefully omits that field rather than showing a placeholder like "v" or "—".
- A profile contains zero tests — the row still renders and displays "0 tests" calmly without warning styling.
- The list contains a large number of profiles (50+) — scrolling remains smooth and entrance animations do not cause perceptible jank.
- The window is at its minimum supported width — rows remain readable, secondary metadata may collapse or wrap but the primary name and primary action target remain intact.
- The user has dismissed the welcome banner — its absence does not leave a visible gap or change vertical rhythm.

## Requirements *(mandatory)*

### Functional Requirements

**Layout & visual consistency**

- **FR-001**: The Profile Manager MUST present profiles as a single vertical column of full-width rows.
- **FR-002**: Profile rows MUST share their visual rhythm (row height range, internal padding, vertical spacing between rows, hover/focus treatment) with the row patterns used in Test List, History, and Schedules.
- **FR-003**: The list MUST occupy the full available content width at all supported window widths and MUST NOT wrap into multiple columns or leave horizontal dead space.
- **FR-004**: The page header (title, subtitle, primary actions) and the optional welcome banner MUST NOT duplicate the same combination of visual treatments (gradient accent line, colored icon tile, title/subtitle pair) such that they read as two competing headers.

**Selection interaction**

- **FR-005**: Clicking anywhere on a profile row (outside of any explicit secondary controls) MUST select and load that profile.
- **FR-006**: A redundant per-row "Select Profile" button MUST NOT be present on each row.
- **FR-007**: Pressing Enter or Space with keyboard focus on a profile row MUST select and load that profile.
- **FR-008**: The user MUST be able to move keyboard focus between rows using Up/Down arrow keys, and the focused row MUST scroll into view if it is off-screen.
- **FR-009**: A profile row MUST display a hover state, a focus state, and a selected/active state, each visually distinct from one another and consistent with the rest of the app.
- **FR-009a**: When the user navigates to the Profile Manager and a profile is currently loaded as the active profile, that profile's row MUST be rendered in the selected/active visual state.
- **FR-009b**: The selected/active state MUST be exposed to assistive technologies (the active row is announced as the selected/current item) so screen-reader users have parity with the visual cue.

**Information display per row**

- **FR-010**: Each profile row MUST display the profile name as the dominant text element, truncated with an ellipsis if it does not fit, with the full name available via tooltip.
- **FR-011**: Each profile row MUST display the test count (e.g., "8 tests") as secondary metadata.
- **FR-012**: Each profile row MUST display the schema version as secondary metadata when available.
- **FR-013**: Each profile row MUST display a recency indicator showing the profile file's last-modified date (e.g., "modified Apr 28" or "modified 3 days ago") when the underlying metadata is available, and MUST omit the field gracefully when it is not. The import date MUST NOT be displayed in place of, or in addition to, the last-modified date.
- **FR-014**: The profile source MUST be displayed as quiet metadata (e.g., outlined chip or muted text), not as a saturated colored pill that competes with primary call-to-action styling elsewhere on the page.

**Recommended-profile signal**

- **FR-015**: When a profile is recommended, the row MUST communicate that status using exactly one labeled visual signal (a "Recommended" badge); additional border, gradient, or background-tint treatments specific to the recommended state MUST NOT be applied on the same row.
- **FR-016**: When no profile is recommended, every row MUST share identical baseline styling with no orphaned decoration.

**Premium polish**

- **FR-017**: Hover, focus, and selection state transitions MUST complete within 200 ms and MUST NOT cause layout shift.
- **FR-018**: Entrance animations for the list MUST be subtle (short duration, single direction, low displacement) and MUST be reduced or omitted when the platform exposes a reduced-motion preference.
- **FR-019**: A focus ring on a row MUST be clearly visible against both light and dark themes and MUST match the focus pattern used elsewhere in the app.
- **FR-019a**: The list MUST expose list/listbox semantics to assistive technologies (the container is announced as a list; each row is announced as a list item) consistent with how other primary lists in the app are exposed.
- **FR-019b**: Each profile row MUST expose an accessible name to assistive technologies that includes at minimum the profile name, and SHOULD include the recommended status when applicable so a screen-reader user can identify the recommended profile without sighted cues.
- **FR-019c**: This feature is NOT required to undergo a formal WCAG conformance audit; the accessibility bar for this feature is parity with the rest of the app's established practices (keyboard navigation, visible focus, accessible names, list semantics).
- **FR-020**: With 50 profiles loaded, the list MUST scroll smoothly without perceptible frame drops on a typical user machine.

**Edge states**

- **FR-021**: When no profiles exist, the empty state MUST remain centered with its current composition (icon + add badge + headline + supporting text + clear path to import) and MUST NOT show a partially-rendered list shell first.
- **FR-022**: While profiles are loading, a single centered progress indicator with a short label MUST be displayed, and the list MUST NOT flicker into a different layout before settling.
- **FR-023**: Error messages related to profile loading MUST appear in a single, predictable location consistent with other screens and MUST NOT visually stack with the welcome banner or the page header decoration.

### Key Entities

- **Profile (presentation)**: The visual representation of a test profile in the list. Carries name, source, test count, schema version, recency indicator, and recommended flag. No new persisted fields are introduced by this feature; recency is derived from existing file/system metadata where available.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a side-by-side comparison test, at least 9 of 10 reviewers (internal stakeholders or unprimed test users) describe the Profile Manager row pattern as "matching" or "consistent with" the Test List / History / Schedules pages — a measurable shift from today's baseline where the same reviewers identify the Profile Manager as visually distinct.
- **SC-002**: Selecting a profile from the list takes a single click action; in usability testing, 100% of participants successfully select a profile on their first attempt without ambiguity about where to click.
- **SC-003**: In an unprimed identification task, at least 90% of participants correctly identify the recommended profile within 2 seconds of seeing the list.
- **SC-004**: 100% of profile selection and navigation actions in this screen are achievable without using a mouse (keyboard parity).
- **SC-005**: Hover, focus, and selection state transitions complete within 200 ms; entrance animations complete within 300 ms total per row stagger.
- **SC-006**: With 50 profiles loaded, the list scrolls at ≥ 55 frames per second on a representative user machine, with no perceptible jank reported by reviewers.
- **SC-007**: After the redesign, no individual row contains more than one decoration that conveys the recommended status; verified by visual inspection against FR-015.
- **SC-008**: User-reported "polish" rating (asked as "How premium does this screen feel on a scale of 1–5?") averages ≥ 4.0 across at least 5 reviewers, up from the screen's pre-redesign baseline.

## Assumptions

- **Scope is presentational, not functional.** This feature redesigns how existing profile data is *displayed and selected*. It does NOT introduce new actions on profiles (no delete, duplicate, rename, set-as-default, or file-location command), and it does NOT change persistence, file format, or what a profile is.
- **Existing profile data is sufficient** for the row's information design. Name, source, test collection, and schema version already exist on the profile object; recency uses the underlying file's last-modified or import timestamp where available, and gracefully omits the field where not.
- **Premium feel is anchored to the existing design system.** Colors, typography, spacing, and easing tokens already used by Test List, History, and Schedules are the source of truth; this feature does NOT introduce a new color palette or typographic scale.
- **The welcome banner remains** as a dismissible first-run aid; this feature only adjusts its visual relationship to the page header, not its content or dismissal logic.
- **Refresh and Import** controls remain in the page header in their current positions; this feature does not change them beyond ensuring they continue to read as the page's primary actions.
- **Search and filtering of profiles are out of scope** for this feature and may be addressed in a follow-up; the current expectation of profile counts (typically under 20) does not yet warrant in-list filtering.
- **Quick actions (kebab/overflow menu per row)** are out of scope for this feature; the row's only interactive surface is the row itself.
- **Reduced-motion handling** uses the platform's exposed user preference where available; if no such preference is exposed, animations remain at their tuned premium defaults.

## Out of Scope

- Adding new profile actions (delete, duplicate, rename, set as default, open file location).
- Adding search, filter, or sort controls to the profile list.
- Changing profile data, persistence, file format, or what is considered a "profile."
- Redesigning Test List, History, or Schedules; this feature only conforms Profile Manager *to* their established pattern.
- Introducing new design tokens (colors, fonts, spacing scale) beyond those already used by the app.

## Dependencies

- The shared row/list visual pattern used by Test List, History, and Schedules is the visual reference and is assumed to be stable for the duration of this feature's implementation.
- The existing design system tokens (colors, typography, spacing, focus rings, easings) are assumed to already cover all states this feature requires; if a gap is found, it will be raised separately rather than introducing one-off styling here.
