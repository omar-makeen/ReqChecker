# Data Model: First-Run Onboarding

**Feature**: 060-first-run-onboarding
**Date**: 2026-03-11

## Entities

### UserPreferences (modified)

Existing entity stored at `%APPDATA%/ReqChecker/preferences.json`.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Theme | string | "Dark" | App theme (existing) |
| SidebarExpanded | bool | true | Sidebar state (existing) |
| HasSeenOnboarding | bool | false | **NEW** — Whether user has dismissed the welcome banner |
| LastUpdated | DateTime | now | Timestamp (existing) |

**JSON representation** (after change):
```json
{
  "theme": "Dark",
  "sidebarExpanded": true,
  "hasSeenOnboarding": false,
  "lastUpdated": "2026-03-11T10:00:00Z"
}
```

**Backward compatibility**: When loading a preferences file from a previous version that lacks `hasSeenOnboarding`, `System.Text.Json` will default it to `false` — which is the correct behavior (show banner for upgrading users on their next visit to Profile Manager).

### Profile (unmodified)

No changes to the Profile model. The recommended profile is identified at runtime by comparing `Profile.Id` against the constant `00000001-0000-0000-0000-000000000001`.

## State Transitions

### Welcome Banner Visibility

```
[App Launch]
    │
    ├─ startup-profile.json exists → TestList (banner never shown)
    │
    └─ No startup profile → Profile Manager
         │
         ├─ HasSeenOnboarding = true → Banner hidden
         │
         └─ HasSeenOnboarding = false → Banner visible
              │
              ├─ User clicks Dismiss → Animate out, set HasSeenOnboarding=true, save
              │
              └─ User selects a profile → Set HasSeenOnboarding=true, save, navigate to TestList
```

### Preference Reset Flow

```
Settings → Reset to Defaults
    │
    └─ HasSeenOnboarding = false (along with Theme=Dark, SidebarExpanded=true)
         │
         └─ Next visit to Profile Manager → Banner visible again
```

## Constants

| Constant | Value | Location |
|----------|-------|----------|
| DefaultProfileId | `"00000001-0000-0000-0000-000000000001"` | ProfileSelectorViewModel |
