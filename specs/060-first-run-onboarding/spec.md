# Feature Specification: First-Run Onboarding

**Feature Branch**: `060-first-run-onboarding`
**Created**: 2026-03-11
**Status**: Draft
**Input**: User description: "Add first-run onboarding experience with welcome banner and guided profile selection for new users"

## Clarifications

### Session 2026-03-11

- Q: Should users who bypass Profile Manager via startup-profile.json see any onboarding? → A: No — the welcome banner is tied to the Profile Manager page only. Startup-profile users skip onboarding entirely, as they are typically set up by an admin or deployment script and don't need first-run guidance.
- Q: How should the "Recommended" profile be identified? → A: Match by the well-known profile ID of the "Default Environment Readiness Profile" (hardcoded known UUID). No schema changes needed; this profile is specifically designed as the introductory experience.
- Q: What visual style should the welcome banner use? → A: Gradient-accent card style, matching existing page header patterns (accent gradient border, icon container, heading + body text, dismiss button). Reuses the established visual language so the banner feels native.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Welcome Banner on First Launch (Priority: P1)

A new user launches ReqChecker for the first time and lands on the Profile Manager page. Instead of seeing an unexplained list of profiles, they see a prominent welcome banner at the top of the page that explains what ReqChecker does and guides them to select a profile to get started. The banner includes a brief tagline ("Verify your environment meets requirements"), a short explanation of profiles ("Profiles define which checks to run"), and a dismiss button. Once dismissed, the banner never appears again.

**Why this priority**: This is the single highest-impact improvement for new user retention. Without context, users don't understand what profiles are, what tests do, or why they should care. A welcome banner bridges this gap with minimal development effort.

**Independent Test**: Can be fully tested by clearing preferences (or first install), launching the app, verifying the welcome banner appears on the Profile Manager page, dismissing it, restarting the app, and confirming it does not reappear.

**Acceptance Scenarios**:

1. **Given** the user has never launched ReqChecker before (no preferences file exists), **When** they open the app and land on the Profile Manager, **Then** a welcome banner is displayed above the profile list with a heading, description, and dismiss button.
2. **Given** the welcome banner is visible, **When** the user clicks the dismiss button, **Then** the banner disappears with a smooth animation and a preference flag is persisted so it won't show again.
3. **Given** the user has previously dismissed the welcome banner, **When** they relaunch the app and navigate to the Profile Manager, **Then** the welcome banner is not displayed.
4. **Given** the welcome banner is visible, **When** the user selects a profile instead of dismissing the banner, **Then** the banner is automatically dismissed (preference saved) and the user proceeds to the Test Suite as normal.

---

### User Story 2 - Guided First Profile Selection (Priority: P2)

After seeing the welcome banner, the user notices that the bundled "Default Environment Readiness Profile" has a visual highlight or recommendation badge indicating it's a good starting point. This guides new users toward a known-good profile rather than leaving them to guess which one to pick.

**Why this priority**: Reduces decision paralysis for new users. The default profile is designed as an introduction, and highlighting it makes the happy path obvious without removing choice.

**Independent Test**: Can be tested by launching with the welcome banner visible (first run) and verifying the default bundled profile card has a "Recommended" badge or highlight. After dismissing the banner or on subsequent launches, the highlight should still be visible on the default profile but the "Recommended for first-time users" context disappears.

**Acceptance Scenarios**:

1. **Given** the user is on the Profile Manager with the welcome banner visible (first run), **When** they view the profile cards, **Then** the default bundled profile displays a "Recommended" badge and a subtle visual highlight (e.g., accent border).
2. **Given** the user is on the Profile Manager after dismissing the welcome banner (not first run), **When** they view the profile cards, **Then** the default bundled profile still shows the "Recommended" badge (this is a permanent quality of the default profile, not tied to first-run state).
3. **Given** only one bundled profile exists, **When** the Profile Manager loads, **Then** that profile receives the "Recommended" badge.

---

### User Story 3 - Contextual Help Tooltips on Key Actions (Priority: P3)

Key action buttons across the app (Run All Tests, Export, Re-run Failed) display informative tooltips when hovered, explaining what the action does and any prerequisites. This helps users who are exploring the app understand the workflow without external documentation.

**Why this priority**: Low effort, incremental improvement. Tooltips don't block any workflow but provide just-in-time guidance for users who need it.

**Independent Test**: Can be tested by hovering over each key action button and verifying a descriptive tooltip appears within 400ms, containing a brief explanation of the action.

**Acceptance Scenarios**:

1. **Given** the user is on any page with action buttons, **When** they hover over an action button for 400ms, **Then** a tooltip appears with a brief description of what the button does.
2. **Given** the user is on the Test Suite page, **When** they hover over the "Run All Tests" button, **Then** a tooltip explains "Execute all selected tests and view results."
3. **Given** the user is on the Results Dashboard, **When** they hover over the "Export" dropdown button, **Then** a tooltip explains "Save test results in PDF, CSV, or JSON format."
4. **Given** the user is on the Results Dashboard with failed tests, **When** they hover over the "Re-run Failed" button, **Then** a tooltip explains "Re-execute only the tests that failed or were skipped due to dependencies."

---

### Edge Cases

- What happens when the preferences file is corrupted or unreadable? The welcome banner should default to showing (fail-open), treating corruption as equivalent to first-run.
- What happens when all bundled profiles are removed or invalid? The "Recommended" badge logic should gracefully handle zero valid profiles — the welcome banner still shows but no profile is highlighted.
- What happens if the user resets preferences via Settings? The welcome banner should reappear on next navigation to Profile Manager, since "has seen onboarding" flag is part of preferences.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a welcome banner on the Profile Manager page when the user has not previously dismissed it.
- **FR-002**: The welcome banner MUST use a gradient-accent card style (matching existing page header patterns) and include a heading (e.g., "Welcome to ReqChecker"), a brief description of the app's purpose and what profiles are, and a visible dismiss button.
- **FR-003**: System MUST persist a "hasSeenOnboarding" flag in user preferences when the welcome banner is dismissed (either explicitly via dismiss button or implicitly by selecting a profile).
- **FR-004**: System MUST NOT display the welcome banner when the "hasSeenOnboarding" preference is true.
- **FR-005**: The default bundled profile MUST display a "Recommended" badge on its profile card in the Profile Manager.
- **FR-006**: Key action buttons (Run All Tests, Export, Re-run Failed, Back to Tests, Import Profile, Refresh) MUST have descriptive tooltips.
- **FR-007**: The welcome banner MUST be dismissible with a smooth fade-out animation consistent with existing app animations.
- **FR-008**: Resetting preferences via Settings MUST reset the "hasSeenOnboarding" flag, causing the welcome banner to reappear.
- **FR-009**: The welcome banner MUST be keyboard-accessible (dismiss button focusable and activatable via Enter/Space).

### Key Entities

- **OnboardingPreference**: A boolean flag ("hasSeenOnboarding") stored within the existing user preferences structure, indicating whether the user has completed first-run onboarding.
- **ProfileRecommendation**: A visual indicator on profile cards denoting the recommended starting profile. Determined by matching against the well-known UUID of the "Default Environment Readiness Profile" — no profile schema changes required.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of first-time users see the welcome banner on their initial launch, providing immediate context about the app's purpose.
- **SC-002**: Users can dismiss the welcome banner in a single click, and it never reappears unless preferences are reset.
- **SC-003**: The recommended profile is visually distinguishable from other profiles within 2 seconds of viewing the Profile Manager.
- **SC-004**: All key action buttons (minimum 6) have descriptive tooltips that appear on hover.
- **SC-005**: The onboarding experience adds no more than 1 second to app startup time.
