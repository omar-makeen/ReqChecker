# Data Model: UI/UX Premium Redesign

## Overview

This feature is a visual/UX redesign with no changes to business domain entities. The data model focuses on UI state management and theme configuration.

## UI State Entities

### UserPreferences

Persisted user preferences for UI state restoration.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Theme | `ThemeMode` | Current theme setting | Required, enum |
| SidebarExpanded | `bool` | Sidebar expansion state | Default: true |
| LastUpdated | `DateTime` | Preference save timestamp | Auto-generated |

**Persistence**: `%APPDATA%/ReqChecker/preferences.json`

**State Transitions**:
- On app launch: Load preferences → Apply theme → Restore sidebar state
- On theme toggle: Update Theme → Persist → Apply to UI
- On sidebar toggle: Update SidebarExpanded → Persist

### ThemeMode (Enum)

```csharp
public enum ThemeMode
{
    Dark,   // Default - deep charcoal (#1a1a2e)
    Light   // Off-white (#f8f9fa)
}
```

### AnimationState

Runtime state for animation control (not persisted).

| Field | Type | Description |
|-------|------|-------------|
| IsReducedMotionEnabled | `bool` | System accessibility setting |
| AnimationDuration | `Duration` | Current animation duration (0 if reduced motion) |

**Source**: `SystemParameters.ClientAreaAnimation`

## View Models (UI State)

### NavigationState

| Field | Type | Description |
|-------|------|-------------|
| CurrentView | `ViewType` | Active navigation item |
| IsSidebarExpanded | `bool` | Bound to sidebar width |
| IsTransitioning | `bool` | True during view transitions |

### ViewType (Enum)

```csharp
public enum ViewType
{
    Profiles,
    Tests,
    TestConfig,
    RunProgress,
    Results,
    Diagnostics
}
```

### ProgressState (Run Progress View)

| Field | Type | Description |
|-------|------|-------------|
| PercentComplete | `double` | 0.0 to 1.0 for progress ring |
| CurrentTestName | `string` | Display name of running test |
| CurrentTestStatus | `string` | "Running...", "Checking...", etc. |
| CompletedTests | `ObservableCollection<TestResultSummary>` | Mini-list of completed |

### ResultsFilterState

| Field | Type | Description |
|-------|------|-------------|
| ActiveFilter | `ResultFilter` | Current filter selection |
| FilteredResults | `ICollectionView` | Filtered test results |

### ResultFilter (Enum)

```csharp
public enum ResultFilter
{
    All,
    Passed,
    Failed,
    Skipped
}
```

## Design Token Resources

### ColorTokens (Resource Dictionary)

| Token | Dark Theme | Light Theme |
|-------|------------|-------------|
| BackgroundBase | #0f0f1a | #f8f9fa |
| BackgroundSurface | #1a1a2e | #ffffff |
| BackgroundElevated | #252542 | #ffffff (shadow) |
| TextPrimary | #ffffff | #1a1a2e |
| TextSecondary | #a0a0b8 | #6b7280 |
| AccentPrimary | #00d9ff | #00d9ff |
| AccentSecondary | #6366f1 | #6366f1 |
| StatusPass | #10b981 | #10b981 |
| StatusFail | #ef4444 | #ef4444 |
| StatusSkip | #f59e0b | #f59e0b |

### SpacingTokens

| Token | Value |
|-------|-------|
| SpacingXs | 4px |
| SpacingSm | 8px |
| SpacingMd | 12px |
| SpacingBase | 16px |
| SpacingLg | 24px |
| SpacingXl | 32px |
| Spacing2xl | 48px |

### AnimationTokens

| Token | Value | Reduced Motion |
|-------|-------|----------------|
| DurationFast | 100ms | 0ms |
| DurationNormal | 200ms | 0ms |
| DurationSlow | 300ms | 0ms |
| DurationSlower | 400ms | 0ms |
| EaseOut | QuadraticEase(EaseOut) | - |
| EaseInOut | QuadraticEase(EaseInOut) | - |

## Component Models

### CardElevation

| Level | Dark Theme Effect | Light Theme Effect |
|-------|-------------------|-------------------|
| 0 (Base) | None | None |
| 1 (Surface) | Glow: rgba(0,217,255,0.05) | Shadow: 0 1px 3px rgba(0,0,0,0.1) |
| 2 (Hover) | Glow: rgba(0,217,255,0.1) | Shadow: 0 4px 6px rgba(0,0,0,0.1) |
| 3 (Modal) | Glow: rgba(0,217,255,0.15) | Shadow: 0 20px 25px rgba(0,0,0,0.15) |

### SidebarItem

| Field | Type | Description |
|-------|------|-------------|
| Icon | `SymbolRegular` | Fluent icon enum |
| Label | `string` | Navigation text |
| ViewType | `ViewType` | Target view |
| IsActive | `bool` | Current selection state |

## Relationships

```
UserPreferences (persisted)
    ├── Theme → ThemeMode
    └── SidebarExpanded → NavigationState.IsSidebarExpanded

NavigationState (runtime)
    ├── CurrentView → ViewType → Loads corresponding View
    └── IsSidebarExpanded ←→ UserPreferences.SidebarExpanded

AnimationState (runtime)
    └── IsReducedMotionEnabled → Controls all animation durations

ThemeService (singleton)
    ├── Reads UserPreferences.Theme on startup
    ├── Applies ColorTokens based on theme
    └── Persists on theme change
```

## File Structure for Theme Resources

```
src/ReqChecker.App/
├── Resources/
│   ├── Styles/
│   │   ├── Theme.xaml           # Base theme (to be replaced)
│   │   ├── Colors.Dark.xaml     # Dark theme color tokens
│   │   ├── Colors.Light.xaml    # Light theme color tokens
│   │   ├── Typography.xaml      # Font styles
│   │   ├── Spacing.xaml         # Spacing values
│   │   ├── Animations.xaml      # Animation storyboards
│   │   └── Controls.xaml        # Control style overrides
│   └── Icons/
│       └── (Fluent icons via WPF-UI SymbolIcon)
└── Services/
    ├── ThemeService.cs          # Theme switching logic
    └── PreferencesService.cs    # User preferences persistence
```
