# Quickstart: Settings Window

**Feature**: 038-settings-window
**Date**: 2026-02-13

## Prerequisites

- .NET 8.0 SDK (net8.0-windows TFM)
- Windows OS (WPF target)

## Build & Run

```bash
# Build the application
dotnet build src/ReqChecker.App/ReqChecker.App.csproj

# Run unit tests
dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj

# Launch the application
dotnet run --project src/ReqChecker.App/ReqChecker.App.csproj
```

## New Files

| File | Purpose |
| ---- | ------- |
| `src/ReqChecker.App/Views/SettingsView.xaml` | Settings page UI with Appearance, About, and Reset sections |
| `src/ReqChecker.App/ViewModels/SettingsViewModel.cs` | Settings page logic — theme commands, version, reset |
| `tests/ReqChecker.App.Tests/ViewModels/SettingsViewModelTests.cs` | Unit tests for SettingsViewModel |

## Modified Files

| File | Change |
| ---- | ------ |
| `src/ReqChecker.App/Services/NavigationService.cs` | Add `NavigateToSettings()` method |
| `src/ReqChecker.App/Services/ThemeService.cs` | Add `SetTheme(AppTheme)` method |
| `src/ReqChecker.App/Services/IPreferencesService.cs` | Add `ResetToDefaults()` method |
| `src/ReqChecker.App/Services/PreferencesService.cs` | Implement `ResetToDefaults()` |
| `src/ReqChecker.App/MainWindow.xaml` | Replace theme toggle with Settings nav item; remove status bar theme button |
| `src/ReqChecker.App/MainWindow.xaml.cs` | Add Settings navigation case; update selection sync |
| `src/ReqChecker.App/App.xaml.cs` | Register `SettingsViewModel` in DI container |

## Manual Verification

1. Launch the app → click the gear icon in the sidebar footer → Settings page opens with header
2. In Appearance section → click "Light" card → app switches to light theme immediately
3. Close and reopen app → theme persists as Light
4. Go to Settings → click "Dark" card → app switches back
5. Scroll to About → verify "ReqChecker" name and version number displayed
6. Click "Reset to Defaults" → confirmation dialog appears → confirm → theme reverts to Dark
7. Verify the old theme toggle is gone from sidebar footer and status bar
