# Data Model: 063-ux-quick-wins

No new data entities or persistence changes. All changes are UI-layer only.

## ViewModel Property Additions

### MainViewModel

| Property | Type | Source | Purpose |
|----------|------|--------|---------|
| `TestCount` | `int` | `IAppState.CurrentProfile?.Tests.Count ?? 0` | Badge display on sidebar nav item |
| `HasTests` | `bool` | `TestCount > 0` | Controls badge visibility |

### RunProgressViewModel

| Property | Type | Source | Purpose |
|----------|------|--------|---------|
| `ProfileName` | `string` | `CurrentProfile?.Name ?? string.Empty` | Display in RunProgress header |

### ResultsViewModel

No new properties. Existing `ToggleExportMenuCommand` and `ActiveFilter` are sufficient.

## State Flow

```
IAppState.CurrentProfileChanged event
  → MainViewModel.TestCount updated (badge)
  → RunProgressViewModel.ProfileName updated (header)
```

No new events, services, or interfaces required.
