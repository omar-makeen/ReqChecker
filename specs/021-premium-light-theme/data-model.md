# Data Model: Premium Light Theme

**Feature**: 021-premium-light-theme
**Date**: 2026-02-02

## Overview

This feature is a UI-only fix with no data persistence changes. The "data model" for this feature consists of theme color tokens defined in XAML resource dictionaries.

## Entities

### ColorToken (Resource Dictionary Entry)

Color tokens are named XAML resources that provide semantic color values based on the current theme.

| Token Category | Light Theme Value | Purpose |
|----------------|-------------------|---------|
| **Backgrounds** | | |
| BackgroundBase | `#f8f9fa` | Main app background |
| BackgroundSurface | `#ffffff` | Card/surface background |
| BackgroundElevated | `#ffffff` | Elevated element background |
| **Text** | | |
| TextPrimary | `#1a1a2e` | Primary text (15.1:1 contrast) |
| TextSecondary | `#4b5563` | Secondary text (7.5:1 contrast) |
| TextTertiary | `#6b7280` | Tertiary text (5.0:1 contrast) |
| **Navigation (NEW)** | | |
| NavigationViewItemForeground | TextSecondary | Default nav item text |
| NavigationViewItemForegroundPointerOver | TextPrimary | Hover state text |
| NavigationViewItemForegroundSelected | AccentPrimary | Selected item text |
| **WPF-UI Text (NEW)** | | |
| TextFillColorPrimaryBrush | TextPrimary | WPF-UI primary text |
| TextFillColorSecondaryBrush | TextSecondary | WPF-UI secondary text |
| **Elevation** | | |
| ElevationGlowColor | `#1A000000` | Card shadow (black 10%) |
| ElevationGlowHoverColor | `#1A000000` | Card hover shadow |

### ThemeState

The application theme state managed by `ThemeService`.

| Property | Type | Values |
|----------|------|--------|
| CurrentTheme | `AppTheme` enum | `Light`, `Dark` |
| IsTransitioning | `bool` | Theme change in progress |
| IsReducedMotionEnabled | `bool` | Accessibility preference |

**Storage**: Persisted in `%APPDATA%/ReqChecker/preferences.json` via `IPreferencesService`

## Resource Dictionary Structure

```
Colors.Light.xaml
├── Background Colors (existing)
├── Text Colors (existing)
├── Border Colors (existing)
├── Accent Colors (existing)
├── Status Colors (existing)
├── Elevation Shadow Colors (existing)
├── WPF-UI Text Fill Colors (NEW)
├── NavigationView Item Foregrounds (NEW)
└── WPF-UI Control/Window Backgrounds (existing)
```

## No Database/API Changes

This feature modifies only:
- XAML resource dictionary files
- No C# code changes expected
- No API contract changes
- No data persistence changes
