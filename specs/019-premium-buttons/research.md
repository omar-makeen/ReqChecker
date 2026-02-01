# Research: Premium Button Styles & Consistency

**Feature**: 019-premium-buttons
**Date**: 2026-02-01

## Overview

This document captures research findings for implementing premium button styles in WPF with smooth transitions, accessibility features, and consistent patterns.

## Research Topics

### 1. WPF Smooth Hover Transitions

**Decision**: Use VisualStateManager with Storyboard animations

**Rationale**: WPF's VisualStateManager provides the cleanest way to define state transitions with animation durations. Using Storyboard with DoubleAnimation allows smooth property transitions without code-behind.

**Implementation Pattern**:
```xml
<VisualStateManager.VisualStateGroups>
    <VisualStateGroup x:Name="CommonStates">
        <VisualStateGroup.Transitions>
            <VisualTransition GeneratedDuration="0:0:0.15" />
        </VisualStateGroup.Transitions>
        <VisualState x:Name="Normal" />
        <VisualState x:Name="MouseOver">
            <Storyboard>
                <DoubleAnimation Storyboard.TargetName="BackgroundBorder"
                                 Storyboard.TargetProperty="Opacity"
                                 To="0.9" Duration="0:0:0.15" />
            </Storyboard>
        </VisualState>
    </VisualStateGroup>
</VisualStateManager.VisualStateGroups>
```

**Alternatives Considered**:
- EventTrigger with BeginStoryboard: More verbose, requires Enter/Leave pairs
- Code-behind animations: Breaks MVVM pattern, harder to maintain
- Third-party animation libraries: Overkill for simple hover transitions

### 2. Keyboard Focus Indicators in WPF

**Decision**: Use IsKeyboardFocused trigger with border property changes

**Rationale**: WPF distinguishes between `IsFocused` (logical focus) and `IsKeyboardFocused` (actual keyboard navigation focus). Using `IsKeyboardFocused` ensures focus rings only appear during keyboard navigation, not mouse clicks.

**Implementation Pattern**:
```xml
<Trigger Property="IsKeyboardFocused" Value="True">
    <Setter TargetName="FocusRing" Property="Visibility" Value="Visible" />
</Trigger>
```

**Focus Ring Design**:
- 2px border offset from button edge
- Use AccentPrimary color for visibility
- Semi-transparent (0.6 opacity) to not overwhelm the button

**Alternatives Considered**:
- FocusVisualStyle: System default is ugly dotted rectangle
- IsFocused trigger: Fires on mouse click too, not keyboard-specific
- Custom adorner: Overkill for simple border effect

### 3. Disabled State Best Practices

**Decision**: Combine opacity reduction with foreground color change and cursor

**Rationale**: Opacity alone (0.5) may not provide sufficient visual distinction, especially on gradient backgrounds. Adding a muted foreground color and not-allowed cursor provides multiple accessibility cues.

**Implementation Pattern**:
```xml
<Trigger Property="IsEnabled" Value="False">
    <Setter Property="Opacity" Value="0.5" />
    <Setter Property="Foreground" Value="{DynamicResource TextDisabled}" />
    <Setter Property="Cursor" Value="No" />
</Trigger>
```

**Alternatives Considered**:
- Opacity only: Insufficient accessibility for some users
- Grayscale filter: Not available in WPF without custom effects
- Complete visual redesign: Overkill, users expect consistent patterns

### 4. FilterTab vs FilterTabButton Style Consolidation

**Decision**: Enhance global FilterTab style and remove local FilterTabButton

**Rationale**: The local FilterTabButton style in ResultsView duplicates functionality. The global FilterTab style is more feature-complete with an animated bottom border indicator.

**Migration Steps**:
1. Review global FilterTab style properties
2. Ensure it has all properties needed by ResultsView
3. Update ResultsView RadioButtons to use global style
4. Remove local FilterTabButton style definition

**Alternatives Considered**:
- Keep both styles: Creates maintenance burden and inconsistency
- Merge into FilterTabButton: Would require renaming throughout codebase

### 5. Icon-Text Spacing Standardization

**Decision**: Use 8px right margin on icons consistently

**Rationale**: 8px is a common spacing unit that provides visual separation without being excessive. This matches most existing buttons and creates a rhythm.

**Implementation**:
```xml
<ui:SymbolIcon Symbol="..." Margin="0,0,8,0" />
```

**Alternatives Considered**:
- 10px: Too much separation, breaks visual connection
- 6px: Too tight, especially at smaller sizes
- Variable spacing: Creates inconsistency, harder to maintain

### 6. Button MinWidth Standards

**Decision**: Use MinWidth instead of fixed Width for responsive behavior

**Rationale**: Fixed widths prevent buttons from adapting to content or container. MinWidth ensures touch targets while allowing growth.

**Standards**:
- Icon buttons: 40px (square)
- Small buttons: MinWidth="80"
- Default buttons: MinWidth="100"
- Large buttons: MinWidth="120"

**Alternatives Considered**:
- Fixed widths: Prevents responsiveness, truncates long text
- No minimum: Could create tiny, unusable touch targets

## Conclusion

All research topics have been resolved. The implementation will use:
- VisualStateManager with 150ms transitions for smooth hover
- IsKeyboardFocused triggers for accessibility focus rings
- Combined opacity + foreground + cursor for disabled states
- Global FilterTab style (removing local duplicate)
- Consistent 8px icon margins
- MinWidth instead of fixed Width constraints
