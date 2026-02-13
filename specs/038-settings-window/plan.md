# Implementation Plan: Settings Window

**Branch**: `038-settings-window` | **Date**: 2026-02-13 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/038-settings-window/spec.md`

## Summary

Add a dedicated Settings page to the ReqChecker WPF application with the full premium design language (staggered animated cards, drop shadows, gradient header, icon containers, hover effects). The page is accessible from a new sidebar footer item and provides centralized theme selection (two elevated side-by-side cards), an About section with auto-detected version, and a Reset to Defaults action with modal confirmation. The existing theme toggles in the sidebar footer and status bar are removed.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: `%APPDATA%/ReqChecker/preferences.json` (existing `IPreferencesService` / `PreferencesService`)
**Testing**: MSTest via `dotnet test` (existing test project: `ReqChecker.App.Tests`)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single desktop application
**Performance Goals**: Settings page loads instantly (no async data fetching required)
**Constraints**: No new NuGet packages; reuse existing patterns, styles, and premium design elements
**Scale/Scope**: 1 new View, 1 new ViewModel, modifications to 4-5 existing files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is a blank template — no project-specific gates defined. Proceeding.

**Post-design re-check**: No violations. Single new View/ViewModel pair, no new packages, no new projects.

## Project Structure

### Documentation (this feature)

```text
specs/038-settings-window/
├── plan.md              # This file
├── research.md          # Phase 0 output (7 research decisions)
├── data-model.md        # Phase 1 output (UserPreferences entity)
├── quickstart.md        # Phase 1 output (build/run/verify steps)
├── contracts/           # Phase 1 output (README only — no API contracts for UI-only feature)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/ReqChecker.App/
├── Views/
│   └── SettingsView.xaml          # NEW: Settings page XAML (premium design)
├── ViewModels/
│   └── SettingsViewModel.cs       # NEW: Settings page ViewModel
├── Services/
│   ├── NavigationService.cs       # MODIFY: Add NavigateToSettings()
│   ├── IPreferencesService.cs     # MODIFY: Add ResetToDefaults()
│   ├── PreferencesService.cs      # MODIFY: Implement ResetToDefaults()
│   └── ThemeService.cs            # MODIFY: Add SetTheme(AppTheme) method
├── MainWindow.xaml                # MODIFY: Replace theme toggle with Settings nav item; remove status bar theme button
├── MainWindow.xaml.cs             # MODIFY: Add "Settings" case to navigation switch; remove theme toggle handler; add NavSettings to selection sync
└── App.xaml.cs                    # MODIFY: Register SettingsViewModel in DI

tests/ReqChecker.App.Tests/
└── ViewModels/
    └── SettingsViewModelTests.cs  # NEW: Unit tests for SettingsViewModel
```

**Structure Decision**: Single WPF desktop application. New files follow the existing View/ViewModel pattern. No new projects or assemblies.

## Implementation Approach

### Phase 1: Foundation (Settings navigation + premium page shell)

**Files**: `SettingsView.xaml`, `SettingsViewModel.cs`, `NavigationService.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `App.xaml.cs`

1. Create `SettingsViewModel` extending `ObservableObject` with DI constructor accepting `IPreferencesService` and `ThemeService`
2. Create `SettingsView.xaml` as a `Page` with the full premium design:
   - `Background="{DynamicResource BackgroundBase}"`, `FocusManager.FocusedElement` for accessibility
   - Page.Resources: define `AnimatedSettingsCard` style (same pattern as `AnimatedDiagCard` — 300ms fade-in from Opacity 0→1 + TranslateTransform Y 20→0 with CubicEase)
   - Outer Grid with `Margin="32"`, `KeyboardNavigation.TabNavigation="Once"`
   - Premium header row: 48px Border with 12px CornerRadius and `{DynamicResource AccentPrimary}` background containing Settings24 icon, title "Settings" with TextH1 style, 4px gradient accent bar using `AccentGradientHorizontal`
3. Add `NavigateToSettings()` to `NavigationService` following the existing pattern (DI-resolve ViewModel, create View, navigate Frame, raise Navigated with "Settings")
4. Replace the theme toggle `NavigationViewItem` in `MainWindow.xaml` FooterMenuItems with a Settings item (`x:Name="NavSettings"`, `Tag="Settings"`, `Content="Settings"`, `Icon="{ui:SymbolIcon Settings24}"`, `Click="NavItem_Click"`)
5. Add `"Settings"` case to `NavigateWithAnimation` switch in `MainWindow.xaml.cs` calling `_navigationService.NavigateToSettings()`
6. Add `NavSettings` to `ClearNavigationSelection` (set IsActive=false) and `SetNavigationSelection` (set IsActive=true for "Settings" case)
7. Remove the `tag == "Theme"` handler block from `NavItem_Click` since theme toggle nav item no longer exists
8. Register `SettingsViewModel` as Transient in `App.xaml.cs` DI container

### Phase 2: Appearance section (premium theme cards)

**Files**: `SettingsView.xaml`, `SettingsViewModel.cs`, `ThemeService.cs`

1. Add `SetTheme(AppTheme theme)` method to `ThemeService` — if theme differs from CurrentTheme, set CurrentTheme, update IPreferencesService.Theme, and call ApplyTheme; no-op if same theme
2. Add `CurrentTheme` observable property (initialized from `ThemeService.CurrentTheme`) and `SelectDarkThemeCommand` / `SelectLightThemeCommand` relay commands (each calls `ThemeService.SetTheme()` and updates `CurrentTheme`) to `SettingsViewModel`
3. Build the Appearance section in XAML using premium design:
   - Section label with Paintbrush24 icon and "Appearance" heading
   - Two-column Grid with elevated cards (CardElevated-style: 12px CornerRadius, `{DynamicResource ElevationGlowColor}` DropShadowEffect at 10px BlurRadius, `{DynamicResource BackgroundElevated}` background)
   - Each card: theme icon (WeatherMoon24 / WeatherSunny24), theme name label, click handler bound to command
   - Active card: 2px accent border (`{DynamicResource AccentPrimary}`), stronger glow shadow
   - Hover effect: card elevates -2px and shadow increases to 20px BlurRadius via EventTrigger
   - DataTrigger on `CurrentTheme` property to toggle active/inactive visual states
   - Both cards wrapped in `AnimatedSettingsCard` style for staggered entrance

### Phase 3: Remove old theme toggles

**Files**: `MainWindow.xaml`, `MainWindow.xaml.cs`

1. Remove the theme toggle button from the status bar (the `ThemeToggleButton` in Grid.Column="2" area)
2. Clean up any unused theme toggle properties/bindings (ThemeLabel, ThemeIcon) if they are no longer referenced

### Phase 4: About section (premium card)

**Files**: `SettingsView.xaml`, `SettingsViewModel.cs`

1. Add `AppVersion` string property to `SettingsViewModel` populated from `Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown"`
2. Add About section to XAML using premium card layout:
   - Section label with Info24 icon and "About" heading
   - Elevated card with app name "ReqChecker" (TextH2), version label bound to `AppVersion` (TextCaption), and app description text
   - Card uses `AnimatedSettingsCard` style for entrance animation

### Phase 5: Reset to Defaults (premium card)

**Files**: `SettingsView.xaml`, `SettingsViewModel.cs`, `PreferencesService.cs`, `IPreferencesService.cs`

1. Add `void ResetToDefaults()` to `IPreferencesService` interface
2. Implement `ResetToDefaults()` in `PreferencesService` — sets `Theme = AppTheme.Dark` and `SidebarExpanded = true` (triggers auto-save via existing partial methods)
3. Add `ResetToDefaultsCommand` relay command to `SettingsViewModel` — shows `MessageBox` with "Reset all settings to defaults?" and YesNo buttons; if Yes, calls `IPreferencesService.ResetToDefaults()`, then `ThemeService.SetTheme(AppTheme.Dark)`, and updates `CurrentTheme`
4. Add Reset section to XAML using premium card layout:
   - Section label with ArrowReset24 icon and "Reset" heading
   - Elevated card with descriptive text ("Reset all settings to their default values") and a "Reset to Defaults" button (SecondaryButton style)
   - Card uses `AnimatedSettingsCard` style for entrance animation

### Phase 6: Tests

**Files**: `SettingsViewModelTests.cs`

1. Create test class with mock `IPreferencesService` and mock/stub `ThemeService`
2. Test: initial `CurrentTheme` reflects value from `ThemeService.CurrentTheme`
3. Test: `SelectDarkThemeCommand` calls `ThemeService.SetTheme(Dark)` and updates `CurrentTheme`
4. Test: `SelectLightThemeCommand` calls `ThemeService.SetTheme(Light)` and updates `CurrentTheme`
5. Test: `AppVersion` is non-null and non-empty
6. Test: `ResetToDefaultsCommand` — mock MessageBox to return Yes, verify `IPreferencesService.ResetToDefaults()` called

## Premium Design Reference

The following existing styles and resources are reused on the Settings page:

| Element | Resource/Style | Source |
| ------- | -------------- | ------ |
| Page background | `{DynamicResource BackgroundBase}` | Colors.Dark/Light.xaml |
| Card background | `{DynamicResource BackgroundElevated}` | Colors.Dark/Light.xaml |
| Card border | `{DynamicResource BorderSubtle}` | Colors.Dark/Light.xaml |
| Card shadow | `{DynamicResource ElevationGlowColor}` | Colors.Dark/Light.xaml |
| Active card border | `{DynamicResource AccentPrimary}` | Colors.Dark/Light.xaml |
| Gradient accent bar | `AccentGradientHorizontal` | Controls.xaml |
| Header title | `TextH1` style | Controls.xaml |
| Section heading | `TextH2` style | Controls.xaml |
| Caption text | `TextCaption` style | Controls.xaml |
| Button style | `SecondaryButton` style | Controls.xaml |
| Entrance animation | AnimatedDiagCard pattern (300ms, CubicEase) | DiagnosticsView.xaml (pattern copied) |
| Icons | Settings24, Paintbrush24, WeatherMoon24, WeatherSunny24, Info24, ArrowReset24 | WPF-UI SymbolRegular |

## Complexity Tracking

No constitution violations to justify.
