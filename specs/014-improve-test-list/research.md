# Research: Improve Test List UI/UX

**Feature**: 014-improve-test-list
**Date**: 2026-02-01

## Icon Mapping Analysis

### WPF-UI Fluent Icons Available

Based on analysis of existing XAML files in the codebase, the following icons are already used:
- `Beaker24` - Current test icon (generic)
- `Globe24` - Web-related
- `Document24` - File/document
- `Folder24` / `FolderOpen24` - Directories
- `Settings24` - Configuration
- `Signal24` - Network connectivity
- `Search24` - Lookup operations
- `Apps24` - Applications/processes
- `Shield24` - Admin/security

### Icon Mapping Decision

| Test Type | Icon | Category | Rationale |
|-----------|------|----------|-----------|
| Ping | `Signal24` | Network | Represents network signal/connectivity |
| HttpGet | `Globe24` | Network | Represents web/HTTP requests |
| DnsLookup | `Search24` | Network | Represents DNS lookup/resolution |
| FileExists | `Document24` | FileSystem | Represents file checking |
| DirectoryExists | `Folder24` | FileSystem | Represents folder/directory |
| ProcessList | `Apps24` | System | Represents running processes |
| RegistryRead | `Settings24` | System | Represents registry/system settings |
| (unknown) | `Beaker24` | Default | Current fallback |

### Alternatives Considered

| Alternative | Rejected Because |
|-------------|-----------------|
| Custom SVG icons | Adds maintenance burden, WPF-UI icons are sufficient |
| No icon mapping | Doesn't solve visual differentiation problem |
| Icon + badge combo | Over-engineered for this use case |

## Color Categorization Analysis

### Existing Design System Colors

From `Colors.Dark.xaml`:
- `AccentPrimary`: #00d9ff (cyan)
- `AccentSecondary`: #6366f1 (purple)
- `StatusPass`: #10b981 (green)
- `StatusFail`: #ef4444 (red)
- `StatusSkip`: #f59e0b (orange/amber)
- `StatusInfo`: #3b82f6 (blue)
- `TextTertiary`: #8585a0 (gray)

### Color Mapping Decision

| Category | Tests | Color Token | Rationale |
|----------|-------|-------------|-----------|
| Network | Ping, HttpGet, DnsLookup | `StatusInfo` (#3b82f6) | Blue = network/connectivity convention |
| FileSystem | FileExists, DirectoryExists | `StatusSkip` (#f59e0b) | Orange = file/storage convention |
| System | ProcessList, RegistryRead | `AccentSecondary` (#6366f1) | Purple = system/advanced operations |
| Default | Unknown types | `TextTertiary` (#8585a0) | Neutral fallback |

### Alternatives Considered

| Alternative | Rejected Because |
|-------------|-----------------|
| All same color | Doesn't add visual grouping value |
| Custom colors | Would need new tokens in both theme files |
| AccentPrimary for all | Too strong, distracting |

## Description Display Analysis

### Current Layout Structure

From `TestListView.xaml` item template (lines 243-267):
```
[Icon 44x44] | [Test Info Stack] | [Status Badge] | [Chevron]
             |   - DisplayName (TextBody, Primary)
             |   - Type (TextCaption, Tertiary) + Admin badge
```

### Proposed Layout

```
[Icon 44x44] | [Test Info Stack] | [Status Badge] | [Chevron]
             |   - DisplayName (TextBody, Primary)
             |   - Description (TextCaption, Secondary) - NEW
             |   - Type (TextCaption, Tertiary) + Admin badge
```

### Description Truncation

- **Strategy**: `TextTrimming="CharacterEllipsis"` with `MaxHeight="32"` (approx 2 lines at 14px)
- **Visibility**: Use existing `NullToVisibilityConverter` to hide when empty
- **Alternative rejected**: Line clamp via custom control - over-engineered for this case

## Implementation Approach

### Converter Pattern

Following existing converter patterns in `src/ReqChecker.App/Converters/`:
- `TestStatusToColorConverter.cs` - Similar switch expression pattern
- `NullToVisibilityConverter.cs` - Already available for description visibility

### XAML Changes

Minimal changes to `TestListView.xaml`:
1. Add converter instances to Page.Resources
2. Update SymbolIcon binding for icon and color
3. Add description TextBlock to test info stack

### Theme Support

Using `DynamicResource` for all color references ensures theme switching works correctly without code changes.

## References

- `TestListView.xaml` - Current implementation (lines 46-50, 231-267)
- `Colors.Dark.xaml` - Color token definitions
- `TestStatusToColorConverter.cs` - Converter pattern reference
- `NullToVisibilityConverter.cs` - Visibility converter for empty descriptions
