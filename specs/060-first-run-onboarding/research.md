# Research: First-Run Onboarding

**Feature**: 060-first-run-onboarding
**Date**: 2026-03-11

## Research Findings

### 1. Preferences Storage Pattern

**Decision**: Add `HasSeenOnboarding` boolean to the existing `UserPreferences` class and `IPreferencesService` interface.

**Rationale**: The existing `PreferencesService` already handles JSON serialization/deserialization of `UserPreferences` to `%APPDATA%/ReqChecker/preferences.json`. Adding a new field is backward-compatible — `System.Text.Json` will default missing fields to `false` when deserializing an older preferences file, which is exactly the desired behavior (show banner for users who haven't seen it).

**Alternatives considered**:
- Separate onboarding state file: Rejected — unnecessary complexity for a single boolean
- Registry key: Rejected — inconsistent with existing file-based preferences pattern
- Check if preferences file exists (first-run proxy): Rejected — unreliable; file could exist from a previous version without the onboarding flag

### 2. Welcome Banner Placement

**Decision**: Insert the welcome banner as a new Grid row between the existing header (Row 0) and the error message / profile list area. The banner occupies its own row and collapses when hidden.

**Rationale**: ProfileSelectorView.xaml uses a Grid with defined rows. Adding a new row for the banner keeps it independent of the header styling and profile list. Using `Visibility=Collapsed` when not shown ensures no layout impact.

**Alternatives considered**:
- Overlay/adorner layer: Rejected — overcomplicated for a dismissible element
- Replace header subtitle: Rejected — header should remain consistent across visits
- Popup/dialog: Rejected — too intrusive; spec calls for an inline banner

### 3. Recommended Profile Identification

**Decision**: Use the well-known UUID `00000001-0000-0000-0000-000000000001` to identify the default profile in ProfileSelectorViewModel. Expose an `IsRecommended` property (computed, not persisted) on the profile card's data template binding.

**Rationale**: The default profile has a stable, hardcoded ID in the embedded `default-profile.json`. Matching by ID is deterministic and doesn't require schema changes. The ViewModel can expose a helper method `IsRecommendedProfile(Profile p)` that compares against the known constant.

**Alternatives considered**:
- Match by profile name string: Rejected — fragile if name changes
- Match by `ProfileSource.Bundled` + first in list: Rejected — multiple bundled profiles exist; order is not guaranteed
- Add `isRecommended` field to profile JSON schema: Rejected — unnecessary schema change per clarification decision

### 4. Banner Dismiss Animation

**Decision**: Use the existing animation pattern — opacity fade-out (200ms) + Y translate (0 → -10) with QuadraticEase. After animation completes, set `Visibility=Collapsed` and persist the preference.

**Rationale**: Consistent with `ViewFadeOut` storyboard duration (150ms) but slightly longer for a more deliberate feel. The slide-up direction communicates dismissal. Existing animation infrastructure (Storyboard resources in page resources) supports this pattern.

**Alternatives considered**:
- Instant hide: Rejected — inconsistent with app's animated design language
- Slide to side: Rejected — vertical collapse is more natural for a horizontal banner

### 5. Tooltip Implementation

**Decision**: Add `ToolTip` attributes directly on existing Button elements in XAML. Use `ToolTipService.InitialShowDelay="400"` to match the existing 400ms tooltip convention.

**Rationale**: WPF's built-in `ToolTip` property is the simplest approach. No custom tooltip controls needed — the default tooltip styling from WPF-UI 4.2.0 is already themed to match the app's design system.

**Alternatives considered**:
- Custom tooltip control with animations: Rejected — over-engineering for static text hints
- Help icon buttons that show flyouts: Rejected — spec specifically calls for hover tooltips

### 6. ResetToDefaults Integration

**Decision**: The existing `PreferencesService.ResetToDefaults()` method sets fields to defaults and calls `Save()`. Adding `HasSeenOnboarding = false` to the reset logic ensures the welcome banner reappears after reset — matching spec requirement FR-008.

**Rationale**: Direct integration into the existing reset flow. No additional methods or events needed.

**Alternatives considered**:
- Separate "reset onboarding" button in settings: Rejected — unnecessary granularity; reset-all is sufficient
