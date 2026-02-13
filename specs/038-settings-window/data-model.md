# Data Model: Settings Window

**Feature**: 038-settings-window
**Date**: 2026-02-13

## Entities

### UserPreferences (existing — no schema changes)

The Settings page reads and writes the existing `UserPreferences` entity. No new fields are added.

| Field           | Type       | Default            | Description                        |
| --------------- | ---------- | ------------------ | ---------------------------------- |
| theme           | string     | "Dark"             | Active theme: "Dark" or "Light"    |
| sidebarExpanded | boolean    | true               | Sidebar expanded/collapsed state   |
| lastUpdated     | datetime   | current UTC time   | Timestamp of last preference save  |

**Storage**: `%APPDATA%/ReqChecker/preferences.json`
**Format**: JSON with camelCase naming, indented

### Default Values (for Reset to Defaults)

| Field           | Default Value |
| --------------- | ------------- |
| theme           | "Dark"        |
| sidebarExpanded | true          |

## State Transitions

### Theme State

```
Dark ──(user selects Light card)──> Light
Light ──(user selects Dark card)──> Dark
Any ──(Reset to Defaults)──> Dark
```

### Persistence Flow

```
User selects theme card
  → SettingsViewModel.SelectDarkTheme/SelectLightTheme command
  → ThemeService.SetTheme(AppTheme)
  → IPreferencesService.Theme = newTheme (triggers auto-save)
  → preferences.json updated on disk

User clicks Reset to Defaults
  → SettingsViewModel.ResetToDefaultsCommand
  → Modal confirmation dialog shown
  → If confirmed: IPreferencesService.ResetToDefaults()
    → Sets Theme=Dark, SidebarExpanded=true (triggers auto-save for each)
    → ThemeService.SetTheme(AppTheme.Dark) to apply visually
```

## Relationships

- **SettingsViewModel** → reads/writes → **IPreferencesService**
- **SettingsViewModel** → calls → **ThemeService** (for applying theme changes)
- **ThemeService** → reads/writes → **IPreferencesService** (for persisting theme)
- **NavigationService** → creates → **SettingsViewModel** (via DI)
