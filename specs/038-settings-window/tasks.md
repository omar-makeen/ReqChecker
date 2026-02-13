# Tasks: Settings Window

**Input**: Design documents from `/specs/038-settings-window/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Included — the plan specifies unit tests for SettingsViewModel.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/ReqChecker.App/`, `tests/ReqChecker.App.Tests/` at repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Extend existing services with methods needed by multiple user stories; register new ViewModel in DI

- [x] T001 Register SettingsViewModel as Transient in DI container in `src/ReqChecker.App/App.xaml.cs` — add `services.AddTransient<SettingsViewModel>();` alongside existing ViewModel registrations
- [x] T002 [P] Add `SetTheme(AppTheme theme)` method to ThemeService in `src/ReqChecker.App/Services/ThemeService.cs` — if theme differs from CurrentTheme, set CurrentTheme, update `_preferencesService.Theme`, call `ApplyTheme()`; no-op if same theme. Keep existing `ToggleTheme()` method intact.
- [x] T003 [P] Add `void ResetToDefaults()` to IPreferencesService interface in `src/ReqChecker.App/Services/IPreferencesService.cs` and implement in PreferencesService in `src/ReqChecker.App/Services/PreferencesService.cs` — set `Theme = AppTheme.Dark` and `SidebarExpanded = true` (auto-save triggers via existing partial methods `OnThemeChanged` and `OnSidebarExpandedChanged`)
- [x] T004 [P] Add `NavigateToSettings()` method to NavigationService in `src/ReqChecker.App/Services/NavigationService.cs` — follow the existing pattern: resolve `SettingsViewModel` from `_serviceProvider`, call `TrackViewModel()`, create `new Views.SettingsView(viewModel)`, navigate `_frame`, call `RaiseNavigated("Settings")`

**Checkpoint**: All shared infrastructure ready — user story implementation can begin

---

## Phase 2: User Story 1 — Access Settings from Navigation (Priority: P1) MVP

**Goal**: Add a Settings navigation item in the sidebar footer that navigates to a new Settings page with premium header

**Independent Test**: Click the gear icon in the sidebar footer and verify the Settings page loads with the premium header (48px icon container, "Settings" title, gradient accent bar). Navigate away and back. Verify keyboard accessibility.

### Implementation for User Story 1

- [x] T005 [US1] Create SettingsViewModel extending ObservableObject with constructor accepting `IPreferencesService` and `ThemeService`, store both as private readonly fields — in `src/ReqChecker.App/ViewModels/SettingsViewModel.cs`
- [x] T006 [US1] Create SettingsView.xaml as a Page with premium design shell in `src/ReqChecker.App/Views/SettingsView.xaml` — set `Background="{DynamicResource BackgroundBase}"`, add `Page.Resources` with `AnimatedSettingsCard` style (copy AnimatedDiagCard pattern from DiagnosticsView: 300ms fade-in Opacity 0→1 + TranslateTransform Y 20→0, CubicEase easing), outer Grid with `Margin="32"` and `KeyboardNavigation.TabNavigation="Once"`, premium header row with: 48px Border (12px CornerRadius, `{DynamicResource AccentPrimary}` background) containing Settings24 SymbolIcon, "Settings" title with TextH1 style, and 4px height Rectangle with `AccentGradientHorizontal` fill as gradient accent bar. Constructor accepts SettingsViewModel and sets DataContext.
- [x] T007 [US1] In MainWindow.xaml, replace the existing theme toggle NavigationViewItem in FooterMenuItems (NavTheme with Tag="Theme") with a Settings NavigationViewItem: `x:Name="NavSettings"`, `Tag="Settings"`, `Content="Settings"`, `Icon="{ui:SymbolIcon Settings24}"`, `Click="NavItem_Click"` — in `src/ReqChecker.App/MainWindow.xaml`
- [x] T008 [US1] In MainWindow.xaml.cs: (1) Add `case "Settings": _navigationService.NavigateToSettings(); break;` to the NavigateWithAnimation switch, (2) Add `NavSettings.IsActive = false;` to ClearNavigationSelection, (3) Add `case "Settings": NavSettings.IsActive = true; break;` to SetNavigationSelection, (4) Remove the entire `if (tag == "Theme") { ... }` block from NavItem_Click since the theme toggle nav item no longer exists — in `src/ReqChecker.App/MainWindow.xaml.cs`

**Checkpoint**: Settings page is navigable from sidebar with full premium header. No content sections yet.

---

## Phase 3: User Story 2 — Change Application Theme (Priority: P1)

**Goal**: Add an Appearance section with two premium elevated theme cards (Dark / Light) that immediately apply the selected theme, and remove old theme toggles

**Independent Test**: Navigate to Settings, verify the active theme card has accent border, click the other card, verify the app theme changes immediately and the card highlights swap. Close and reopen app — theme persists. Verify old theme toggles are gone from sidebar footer and status bar.

**Depends on**: US1 (Settings page must exist), Setup (T002 SetTheme method)

### Implementation for User Story 2

- [x] T009 [US2] Add `CurrentTheme` observable property (type `AppTheme`, initialized from `_themeService.CurrentTheme` in constructor), `SelectDarkThemeCommand` and `SelectLightThemeCommand` relay commands to SettingsViewModel — each command calls `_themeService.SetTheme(AppTheme.Dark/Light)` and sets `CurrentTheme` to the selected value — in `src/ReqChecker.App/ViewModels/SettingsViewModel.cs`
- [x] T010 [US2] Add "Appearance" section to SettingsView.xaml in `src/ReqChecker.App/Views/SettingsView.xaml` — section header StackPanel with Paintbrush24 icon and "Appearance" TextBlock (TextH2 style), then a two-column Grid (16px gap) containing two elevated theme cards: each is a Border with `Style="{StaticResource AnimatedSettingsCard}"`, 12px CornerRadius, `{DynamicResource BackgroundElevated}` background, `{DynamicResource ElevationGlowColor}` DropShadowEffect (10px BlurRadius, ShadowDepth=0, Opacity=0.3), 16px Padding, containing a StackPanel with theme icon (WeatherMoon24 for Dark / WeatherSunny24 for Light) and theme name TextBlock. Each card wrapped in a Button (Style="{StaticResource GhostButton}" or transparent background) bound to SelectDarkTheme/SelectLightTheme command. Add DataTrigger on `CurrentTheme` property: active card gets `{DynamicResource AccentPrimary}` 2px BorderBrush and stronger 20px BlurRadius shadow; inactive card gets `{DynamicResource BorderSubtle}` 1px border. Add hover EventTrigger: on MouseEnter translate Y -2px and increase shadow to 20px, on MouseLeave revert (150ms, CubicEase). Ensure cards are keyboard-focusable via TabIndex.
- [x] T011 [US2] Remove the theme toggle button from the status bar in `src/ReqChecker.App/MainWindow.xaml` — remove the ThemeToggleButton element from the Grid.Column="2" area in the status bar Border (Row 2). Keep the "Ready" status indicator if present. (FR-012)

**Checkpoint**: Theme can be changed from premium cards on Settings page. Old theme toggles fully removed.

---

## Phase 4: User Story 3 — View Application Information (Priority: P2)

**Goal**: Add an About section with premium card showing "ReqChecker" name and auto-detected version

**Independent Test**: Navigate to Settings, scroll to About section, verify "ReqChecker" label and version number (e.g., "1.0.0") are displayed in an elevated card. Version must match assembly version.

**Depends on**: US1 (Settings page must exist)

### Implementation for User Story 3

- [x] T012 [P] [US3] Add `AppVersion` string property to SettingsViewModel, initialized in constructor from `System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown"` — in `src/ReqChecker.App/ViewModels/SettingsViewModel.cs`
- [x] T013 [US3] Add "About" section to SettingsView.xaml in `src/ReqChecker.App/Views/SettingsView.xaml` — section header StackPanel with Info24 icon and "About" TextBlock (TextH2 style), then an elevated card Border with `Style="{StaticResource AnimatedSettingsCard}"`, 12px CornerRadius, `{DynamicResource BackgroundElevated}` background, `{DynamicResource ElevationGlowColor}` DropShadowEffect, 16px Padding, containing: "ReqChecker" TextBlock (TextH2 style, `{DynamicResource TextPrimary}`), "Version" label + AppVersion binding (TextCaption style, `{DynamicResource TextSecondary}`)

**Checkpoint**: About section displays app name and version in a premium card.

---

## Phase 5: User Story 4 — Reset Preferences to Defaults (Priority: P3)

**Goal**: Add a Reset to Defaults button in a premium card that shows a modal confirmation dialog, then reverts all preferences to defaults

**Independent Test**: Change theme to Light, click "Reset to Defaults", confirm in dialog — theme reverts to Dark and card highlights update. Cancel the dialog — no change.

**Depends on**: US1 (Settings page must exist), US2 (theme must be changeable to test reset), Setup (T003 ResetToDefaults method)

### Implementation for User Story 4

- [x] T014 [US4] Add `ResetToDefaultsCommand` relay command to SettingsViewModel in `src/ReqChecker.App/ViewModels/SettingsViewModel.cs` — show `System.Windows.MessageBox.Show("Reset all settings to defaults?", "Reset Settings", MessageBoxButton.YesNo, MessageBoxImage.Question)`; if result is Yes, call `_preferencesService.ResetToDefaults()`, then `_themeService.SetTheme(AppTheme.Dark)`, and set `CurrentTheme = AppTheme.Dark`
- [x] T015 [US4] Add "Reset" section to SettingsView.xaml in `src/ReqChecker.App/Views/SettingsView.xaml` — section header StackPanel with ArrowReset24 icon and "Reset" TextBlock (TextH2 style), then an elevated card Border with `Style="{StaticResource AnimatedSettingsCard}"`, 12px CornerRadius, `{DynamicResource BackgroundElevated}` background, `{DynamicResource ElevationGlowColor}` DropShadowEffect, 16px Padding, containing: descriptive TextBlock "Reset all settings to their default values" (TextBody style, `{DynamicResource TextSecondary}`), Margin="0,0,0,12", then a Button with `Style="{StaticResource SecondaryButton}"`, Content="Reset to Defaults", Command bound to ResetToDefaultsCommand

**Checkpoint**: Reset flow works end-to-end with modal confirmation and premium card design.

---

## Phase 6: Tests & Polish

**Purpose**: Unit tests for SettingsViewModel and final build/test validation

- [x] T016 Create SettingsViewModelTests test class with mock IPreferencesService (use Moq or manual mock matching existing test patterns) and a testable ThemeService stub in `tests/ReqChecker.App.Tests/ViewModels/SettingsViewModelTests.cs`
- [x] T017 [P] Add test: verify initial CurrentTheme property reflects value from ThemeService.CurrentTheme after construction in `tests/ReqChecker.App.Tests/ViewModels/SettingsViewModelTests.cs`
- [x] T018 [P] Add test: verify SelectDarkThemeCommand sets CurrentTheme to Dark and calls ThemeService.SetTheme(AppTheme.Dark) in `tests/ReqChecker.App.Tests/ViewModels/SettingsViewModelTests.cs`
- [x] T019 [P] Add test: verify SelectLightThemeCommand sets CurrentTheme to Light and calls ThemeService.SetTheme(AppTheme.Light) in `tests/ReqChecker.App.Tests/ViewModels/SettingsViewModelTests.cs`
- [x] T020 [P] Add test: verify AppVersion property is non-null and non-empty after construction in `tests/ReqChecker.App.Tests/ViewModels/SettingsViewModelTests.cs`
- [x] T021 Run `dotnet build src/ReqChecker.App/ReqChecker.App.csproj` to verify zero build errors
- [x] T022 Run `dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj` to verify all tests pass (existing + new)
- [ ] T023 Run quickstart.md manual verification steps to validate end-to-end premium design and behavior

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **US1 (Phase 2)**: Depends on Setup (T001 DI registration, T004 NavigateToSettings) — BLOCKS all other user stories
- **US2 (Phase 3)**: Depends on US1 (T005-T008 Settings page) and Setup (T002 SetTheme)
- **US3 (Phase 4)**: Depends on US1 only — can run in parallel with US2
- **US4 (Phase 5)**: Depends on US1 + US2 (needs theme cards to verify reset), and Setup (T003 ResetToDefaults)
- **Tests & Polish (Phase 6)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (P1)**: Can start after Setup — no dependencies on other stories
- **US2 (P1)**: Depends on US1 (Settings page must exist to add Appearance section)
- **US3 (P2)**: Depends on US1 only — can run in parallel with US2
- **US4 (P3)**: Depends on US1 + US2 (needs theme cards to verify reset works)

### Parallel Opportunities

- **Setup phase**: T002, T003, T004 can all run in parallel (different files)
- **US3 and US2**: T012 (ViewModel property) can run in parallel with US2 work since it touches a different section of SettingsViewModel
- **Tests**: T017, T018, T019, T020 can all run in parallel (independent test methods)

---

## Parallel Example: Setup Phase

```text
# These can all run simultaneously (different files):
Task T002: "Add SetTheme method to ThemeService.cs"
Task T003: "Add ResetToDefaults to IPreferencesService.cs + PreferencesService.cs"
Task T004: "Add NavigateToSettings to NavigationService.cs"
```

## Parallel Example: Tests Phase

```text
# These can all run simultaneously (independent test methods):
Task T017: "Test initial CurrentTheme reflects ThemeService value"
Task T018: "Test SelectDarkThemeCommand calls SetTheme(Dark)"
Task T019: "Test SelectLightThemeCommand calls SetTheme(Light)"
Task T020: "Test AppVersion is non-null and non-empty"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: US1 — Settings Navigation + Premium Header (T005-T008)
3. **STOP and VALIDATE**: Click gear icon → Settings page loads with premium header (icon container, title, gradient bar)
4. Deploy/demo if ready — Settings page exists and is navigable

### Incremental Delivery

1. Setup → Foundation ready
2. Add US1 → Settings page with premium header navigable → Deploy/Demo (MVP!)
3. Add US2 → Premium theme cards working, old toggles removed → Deploy/Demo
4. Add US3 → About section in premium card → Deploy/Demo
5. Add US4 → Reset to Defaults in premium card with confirmation → Deploy/Demo
6. Tests & Polish → Full test coverage, quickstart validated

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- No new NuGet packages — all tasks use existing dependencies
- Premium design: every card section uses AnimatedSettingsCard style, elevated backgrounds, drop shadows, hover effects
- Auto-save pattern: changing any preference property triggers Save() automatically
- Total: 23 tasks across 6 phases
