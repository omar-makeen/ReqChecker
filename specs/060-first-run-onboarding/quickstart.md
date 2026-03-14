# Quickstart: First-Run Onboarding

**Feature**: 060-first-run-onboarding
**Date**: 2026-03-11

## Overview

This feature adds three components:
1. **Welcome banner** on the Profile Manager page for first-time users
2. **"Recommended" badge** on the default bundled profile card
3. **Descriptive tooltips** on key action buttons across the app

## Key Files to Modify

### Service Layer
- `src/ReqChecker.App/Services/IPreferencesService.cs` — Add `HasSeenOnboarding` property
- `src/ReqChecker.App/Services/PreferencesService.cs` — Add field, persistence, reset logic

### ViewModel Layer
- `src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs` — Add `ShowWelcomeBanner` computed property, `DismissWelcomeBannerCommand`, `IsRecommendedProfile()` helper, constant for default profile ID

### View Layer
- `src/ReqChecker.App/Views/ProfileSelectorView.xaml` — Welcome banner XAML, recommended badge on profile card template
- `src/ReqChecker.App/Views/ProfileSelectorView.xaml.cs` — Banner dismiss animation handler
- `src/ReqChecker.App/Views/TestListView.xaml` — Tooltips on Run All Tests, Select All buttons
- `src/ReqChecker.App/Views/ResultsView.xaml` — Tooltips on Export, Re-run Failed, Back to Tests buttons
- `src/ReqChecker.App/Views/ProfileSelectorView.xaml` — Tooltips on Refresh, Import Profile buttons

### Styles
- `src/ReqChecker.App/Resources/Styles/Controls.xaml` — WelcomeBanner style, RecommendedBadge style

### Tests
- `tests/ReqChecker.App.Tests/ViewModels/ProfileSelectorViewModelTests.cs` — Banner visibility, dismiss, auto-dismiss on profile select
- `tests/ReqChecker.App.Tests/Services/PreferencesServiceTests.cs` — HasSeenOnboarding persistence and reset

## Implementation Order

1. **PreferencesService** — Add `HasSeenOnboarding` to model, interface, and reset logic
2. **ProfileSelectorViewModel** — Add banner state management and recommended profile logic
3. **Controls.xaml** — Add WelcomeBanner and RecommendedBadge styles
4. **ProfileSelectorView.xaml** — Add banner UI and recommended badge to profile card template
5. **ProfileSelectorView.xaml.cs** — Add banner dismiss animation handler
6. **Tooltip pass** — Add ToolTip attributes to buttons across TestListView, ResultsView, ProfileSelectorView
7. **Tests** — Unit tests for ViewModel and Service changes

## Design Decisions

- **No new packages**: All functionality uses existing WPF + CommunityToolkit.Mvvm + System.Text.Json
- **No schema changes**: Profile JSON format unchanged; recommended detection by known UUID
- **Backward compatible**: Missing `hasSeenOnboarding` in old preferences defaults to `false` (show banner)
- **Consistent styling**: Banner reuses gradient-accent card pattern from existing page headers
- **Default profile UUID**: `00000001-0000-0000-0000-000000000001`
