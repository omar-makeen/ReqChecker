# Tasks: First-Run Onboarding

**Feature**: 060-first-run-onboarding
**Branch**: `060-first-run-onboarding`
**Generated**: 2026-03-11
**Total Tasks**: 16

## Phase 1: Setup

No setup tasks required — all changes are modifications to existing files. No new projects or packages needed.

## Phase 2: Foundational (blocking prerequisites)

These tasks add the `HasSeenOnboarding` preference field that all user stories depend on.

- [X] T001 Add `HasSeenOnboarding` property to `IPreferencesService` interface in `src/ReqChecker.App/Services/IPreferencesService.cs`. Add `bool HasSeenOnboarding { get; set; }` to the interface.
- [X] T002 Add `HasSeenOnboarding` field to `PreferencesService` in `src/ReqChecker.App/Services/PreferencesService.cs`. Add `[ObservableProperty] private bool _hasSeenOnboarding;` field, add `HasSeenOnboarding = false` to `UserPreferences` class defaults, include in `ResetToDefaults()` method, and ensure auto-save on change (matching existing `OnThemeChanged`/`OnSidebarExpandedChanged` pattern).
- [X] T003 Add unit tests for `HasSeenOnboarding` persistence in `tests/ReqChecker.App.Tests/Services/PreferencesServiceTests.cs`. Test: default value is `false`, setting to `true` persists to JSON, `ResetToDefaults()` resets to `false`, loading preferences without field defaults to `false` (backward compatibility).

## Phase 3: User Story 1 — Welcome Banner on First Launch (P1)

**Goal**: Display a dismissible welcome banner above the profile list for first-time users.
**Independent Test**: Clear preferences → launch app → verify banner appears → dismiss → restart → verify banner gone.

- [X] T004 [US1] Add `ShowWelcomeBanner` computed property and `DismissWelcomeBannerCommand` to `ProfileSelectorViewModel` in `src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs`. `ShowWelcomeBanner` returns `!_preferencesService.HasSeenOnboarding`. `DismissWelcomeBannerCommand` sets `HasSeenOnboarding = true`, saves preferences, and raises `PropertyChanged` for `ShowWelcomeBanner`. Inject `IPreferencesService` (add to constructor). Modify `SelectProfile` command to also call `DismissWelcomeBanner` logic (auto-dismiss on profile select per FR-003).
- [X] T005 [US1] Add `WelcomeBanner` style to `src/ReqChecker.App/Resources/Styles/Controls.xaml`. Create a named style targeting `Border` with: gradient accent top border (4px, AccentGradient), `BackgroundElevated` background, `CornerRadius=12`, `Padding=24`, `Margin=0,0,0,16`. Include a dismiss button template matching `GhostButton` style.
- [X] T006 [US1] Add welcome banner XAML to `src/ReqChecker.App/Views/ProfileSelectorView.xaml`. Insert a new Grid row after the header (Row 0) for the banner. Banner contains: icon container (48x48, `Lightbulb24` icon with AccentPrimary background), heading TextBlock ("Welcome to ReqChecker", `TextH2` style), body TextBlock ("Verify your environment meets requirements. Select a profile below to define which checks to run.", `BodyTextStyle`), and dismiss button (`DismissCircle24` icon, `GhostButton` style, bound to `DismissWelcomeBannerCommand`). Bind `Visibility` to `ShowWelcomeBanner` using `BooleanToVisibilityConverter`. Add storyboard resource `BannerDismissStoryboard` (opacity 1→0, Y translate 0→-10, 200ms, QuadraticEase EaseIn). Shift existing row definitions down by one to accommodate the new row. Give banner element `x:Name="WelcomeBanner"`.
- [X] T007 [US1] Add banner dismiss animation handler in `src/ReqChecker.App/Views/ProfileSelectorView.xaml.cs`. Add method that plays `BannerDismissStoryboard` on the `WelcomeBanner` element, then on `Completed` event sets `Visibility=Collapsed`. Wire the dismiss button click to trigger animation before the command executes (or use `Storyboard.Completed` to call `DismissWelcomeBannerCommand`). Ensure keyboard accessibility — dismiss button has `Focusable=True`, `TabIndex` after Import Profile button.
- [X] T008 [US1] Add unit tests for welcome banner logic in `tests/ReqChecker.App.Tests/ViewModels/ProfileSelectorViewModelTests.cs`. Tests: `ShowWelcomeBanner` returns `true` when `HasSeenOnboarding=false`, returns `false` when `HasSeenOnboarding=true`, `DismissWelcomeBannerCommand` sets `HasSeenOnboarding=true` and raises `PropertyChanged`, `SelectProfile` command auto-dismisses banner (sets `HasSeenOnboarding=true`).

## Phase 4: User Story 2 — Guided First Profile Selection (P2)

**Goal**: Display a "Recommended" badge on the default bundled profile card.
**Independent Test**: Load Profile Manager → verify default profile has "Recommended" badge → other profiles do not.

- [X] T009 [P] [US2] Add `DefaultProfileId` constant and `IsRecommendedProfile` helper method to `ProfileSelectorViewModel` in `src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs`. Add `private const string DefaultProfileId = "00000001-0000-0000-0000-000000000001";` and `public bool IsRecommendedProfile(Profile profile) => profile.Id == DefaultProfileId;`. Also add an `IValueConverter` class `ProfileRecommendedConverter` (in `src/ReqChecker.App/Converters/` or inline) that takes a Profile Id and returns `Visibility.Visible` if it matches `DefaultProfileId`, else `Visibility.Collapsed`.
- [X] T010 [P] [US2] Add `RecommendedBadge` style to `src/ReqChecker.App/Resources/Styles/Controls.xaml`. Style a `Border` with `AccentPrimary` background, `CornerRadius=10`, `Padding=8,3`, containing a TextBlock "Recommended" in white, `CaptionTextStyle`, `FontWeight=Medium`. Include a subtle glow effect (`DropShadowEffect` with AccentPrimary color, 6px blur, 0 offset, 0.3 opacity).
- [X] T011 [US2] Add "Recommended" badge to profile card DataTemplate in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`. Inside the profile card template (near the profile name area), add a `Border` using `RecommendedBadge` style. Bind its `Visibility` to the profile's `Id` via `ProfileRecommendedConverter` (compare against `DefaultProfileId`). Also add a subtle accent border (`AccentPrimary`, 1px) to the recommended profile's card using the same converter on the card's outer `Border.BorderBrush`. Register the converter in `Page.Resources`.
- [X] T012 [US2] Add unit tests for recommended profile identification in `tests/ReqChecker.App.Tests/ViewModels/ProfileSelectorViewModelTests.cs`. Tests: `IsRecommendedProfile` returns `true` for profile with ID `00000001-0000-0000-0000-000000000001`, returns `false` for other IDs, `ProfileRecommendedConverter` returns `Visible` for matching ID and `Collapsed` for non-matching.

## Phase 5: User Story 3 — Contextual Help Tooltips (P3)

**Goal**: Add descriptive tooltips to key action buttons across the app.
**Independent Test**: Hover over each listed button → verify tooltip appears with correct text.

- [X] T013 [P] [US3] Add tooltips to Profile Manager buttons in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`. Add `ToolTip="Reload profiles from disk"` with `ToolTipService.InitialShowDelay="400"` to the Refresh button. Add `ToolTip="Import a test profile from a JSON file"` to the Import Profile button. Add `ToolTip="Load this profile and view its tests"` to each "Select Profile" button in the card template.
- [X] T014 [P] [US3] Add tooltips to Test Suite buttons in `src/ReqChecker.App/Views/TestListView.xaml`. Add `ToolTip="Execute all selected tests and view results"` with `ToolTipService.InitialShowDelay="400"` to the "Run All Tests" button. Add `ToolTip="Select or deselect all visible tests"` to the "Select All" checkbox.
- [X] T015 [P] [US3] Add tooltips to Results Dashboard buttons in `src/ReqChecker.App/Views/ResultsView.xaml`. Add `ToolTip="Return to the test suite"` with `ToolTipService.InitialShowDelay="400"` to "Back to Tests" button. Add `ToolTip="Re-execute only the tests that failed or were skipped due to dependencies"` to "Re-run Failed" button. Add `ToolTip="Save test results in PDF, CSV, or JSON format"` to the Export dropdown button.

## Phase 6: Polish & Cross-Cutting Concerns

- [X] T016 Verify reset-to-defaults integration end-to-end. Confirm that `SettingsViewModel.ResetToDefaultsCommand` in `src/ReqChecker.App/ViewModels/SettingsViewModel.cs` calls `_preferencesService.ResetToDefaults()` which now resets `HasSeenOnboarding=false`. No code changes expected — just verify the existing call chain handles the new field correctly. If the `ResetToDefaults()` method uses `_suppressSave` pattern, ensure `HasSeenOnboarding` is set before the final `Save()` call (same as T002).

## Dependencies

```text
T001 → T002 → T003 (foundational chain)
T002 → T004 (US1 needs preferences field)
T004 → T005 → T006 → T007 (US1 chain: ViewModel → Style → XAML → Animation)
T004 → T008 (US1 tests need ViewModel)
T009, T010 → T011 (US2: converter + style before XAML)
T009 → T012 (US2 tests need helper)
T013, T014, T015 (US3: all parallel, no dependencies on each other)
T016 depends on T002 (needs HasSeenOnboarding in ResetToDefaults)
```

## Parallel Execution Opportunities

**Within Phase 2**: T001 → T002 sequential; T003 after T002
**Within Phase 3 (US1)**: T005 can run parallel with T004 (different files); T006 depends on both; T008 after T004
**Within Phase 4 (US2)**: T009 ∥ T010 (different files); T011 after both; T012 after T009
**Within Phase 5 (US3)**: T013 ∥ T014 ∥ T015 (all different files, fully parallel)
**Cross-phase**: US2 (Phase 4) can start after T002 completes (doesn't depend on US1). US3 (Phase 5) has no dependencies on US1 or US2 — can run fully in parallel with both.

## Implementation Strategy

**MVP**: Phase 2 + Phase 3 (US1) — the welcome banner alone delivers the core onboarding value.
**Increment 2**: Phase 4 (US2) — recommended badge enhances the profile selection experience.
**Increment 3**: Phase 5 (US3) — tooltips provide finishing polish across the app.
**Final**: Phase 6 — verification pass.
