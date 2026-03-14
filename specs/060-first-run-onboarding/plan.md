# Implementation Plan: First-Run Onboarding

**Branch**: `060-first-run-onboarding` | **Date**: 2026-03-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/060-first-run-onboarding/spec.md`

## Summary

Add a first-run onboarding experience to ReqChecker: a welcome banner on the Profile Manager page for new users, a "Recommended" badge on the default bundled profile, and descriptive tooltips on key action buttons across the app. The welcome banner uses the existing gradient-accent card style and persists a `HasSeenOnboarding` flag in the existing `UserPreferences` model via `IPreferencesService`. The recommended profile is identified by matching against the well-known UUID `00000001-0000-0000-0000-000000000001`.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2
**Storage**: `%APPDATA%/ReqChecker/preferences.json` (existing `PreferencesService` / `UserPreferences`)
**Testing**: xUnit + Moq (existing test infrastructure in `tests/ReqChecker.App.Tests/`)
**Target Platform**: Windows 10/11 desktop (WPF)
**Project Type**: Desktop app (WPF)
**Performance Goals**: Onboarding adds no more than 1 second to startup; banner animation completes within 300ms
**Constraints**: No new NuGet packages; UI-only changes plus one new preference field
**Scale/Scope**: 3 views modified (ProfileSelector, TestList, Results), 1 service modified (PreferencesService), 1 new style added

## Constitution Check

*No constitution file found. Gate passes by default.*

## Project Structure

### Documentation (this feature)

```text
specs/060-first-run-onboarding/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/ReqChecker.App/
├── Services/
│   ├── IPreferencesService.cs        # Add HasSeenOnboarding property
│   └── PreferencesService.cs         # Add HasSeenOnboarding field + persistence
├── ViewModels/
│   └── ProfileSelectorViewModel.cs   # Add ShowWelcomeBanner property, DismissWelcomeBanner command
├── Views/
│   ├── ProfileSelectorView.xaml      # Add welcome banner XAML, recommended badge on profile cards
│   ├── ProfileSelectorView.xaml.cs   # Add banner dismiss animation handler
│   ├── TestListView.xaml             # Add tooltips to action buttons
│   └── ResultsView.xaml              # Add tooltips to action buttons
└── Resources/Styles/
    └── Controls.xaml                 # Add WelcomeBanner style

tests/ReqChecker.App.Tests/
├── ViewModels/
│   └── ProfileSelectorViewModelTests.cs  # Tests for banner visibility + dismiss logic
└── Services/
    └── PreferencesServiceTests.cs        # Tests for HasSeenOnboarding persistence
```

**Structure Decision**: Existing WPF MVVM structure. All changes are modifications to existing files except potentially new test files. No new projects or directories needed.

## Complexity Tracking

No constitution violations. Feature is straightforward: one new preference field, one new UI element (banner), one badge addition, and tooltip attributes on existing buttons.
