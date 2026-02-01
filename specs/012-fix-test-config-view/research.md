# Research: Fix Test Configuration View

**Feature**: 012-fix-test-config-view
**Date**: 2026-02-01

## Problem Analysis

### Root Cause

The Test Configuration view crashes due to missing WPF style definitions:

```
Error: 'Provide value on 'System.Windows.StaticResourceExtension' threw an exception.'
Line number '95' and line position '22'.
```

### Technical Explanation

1. `TestConfigView.xaml` references two styles via `StaticResource`:
   - `ParameterGroupCard` at lines 95, 176, 239
   - `PromptAtRunIndicator` at line 289

2. These styles are NOT defined in the application's resource dictionaries

3. WPF `StaticResource` throws at XAML load time when the resource key is not found

4. The view cannot be instantiated, causing the error dialog

## Solution

### Decision

Add the two missing style definitions to `Controls.xaml` following existing design patterns.

### Rationale

- Styles should be added to the existing `Controls.xaml` where all other control styles are defined
- New styles should follow the established design system (colors, spacing, effects)
- Using `BasedOn` inheritance where appropriate for consistency

### Alternatives Rejected

| Alternative | Reason Rejected |
|-------------|-----------------|
| Define styles inline in TestConfigView.xaml | Violates separation of concerns; styles should be centralized |
| Remove style references from view | Would break the intended premium design |
| Create new resource dictionary | Over-engineering; Controls.xaml already handles all control styles |

## Style Design Decisions

### ParameterGroupCard

**Purpose**: Container for grouped form sections (Basic Information, Execution Settings, Test Parameters)

**Design Choices**:
- Based on existing `Card` pattern for consistency
- `CornerRadius="12"` - Slightly larger than standard cards (8px) to distinguish section grouping
- `Padding="20"` - More internal spacing for form content readability
- `Margin="0,0,0,16"` - Bottom margin for visual separation between sections
- `BackgroundSurface` + `BorderDefault` - Standard card colors
- `ElevationGlowColor` effect - Consistent with other cards

### PromptAtRunIndicator

**Purpose**: Visual indicator badge for "prompt at run" parameters (secure/sensitive values)

**Design Choices**:
- `AccentSecondary` background - Distinguishes from status badges (pass/fail/skip)
- `CornerRadius="6"` - Rounded for badge appearance
- `Padding="12,8"` - Comfortable touch target for indicator
- Subtle glow effect with `AccentSecondaryColor` at 30% opacity

## References

- Existing styles in `Controls.xaml`: Card, CardElevated, CardInteractive, CardSelected
- Status badge patterns: StatusBadgeBase, StatusBadgePass, StatusBadgeFail, StatusBadgeSkip
- Design tokens: BackgroundSurface, BorderDefault, AccentSecondary, ElevationGlowColor
