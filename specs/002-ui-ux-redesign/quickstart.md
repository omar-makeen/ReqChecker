# Quickstart: UI/UX Premium Redesign

## Overview

This guide covers implementing the premium UI/UX redesign for ReqChecker, transforming it from a basic utility into a world-class enterprise application with Fluent Design aesthetics.

## Prerequisites

- Visual Studio 2022 with .NET 8.0 SDK
- WPF-UI 4.2.0 (already installed)
- Windows 10/11 for testing

## Implementation Order

### Phase 1: Foundation (Do First)

1. **Theme Infrastructure**
   - Create color resource dictionaries (Dark/Light)
   - Implement `ThemeService` for theme switching
   - Add `PreferencesService` for persistence
   - Set up animation duration resources

2. **Main Window Shell**
   - Convert `MainWindow` to `FluentWindow`
   - Replace header navigation with `NavigationView` sidebar
   - Add status bar with version and theme toggle
   - Implement sidebar collapse/expand with persistence

### Phase 2: Core Components

3. **Reusable Controls**
   - Enhance `StatusBadge` with glow effects
   - Create `ProgressRing` custom control with gradient
   - Build `ExpanderCard` for expandable results
   - Style buttons (Primary/Secondary/Ghost variants)

4. **Card System**
   - Create base card style with elevation
   - Add hover animations
   - Implement theme-aware shadows/glows

### Phase 3: View Redesigns

5. **Profile Selector View**
   - Large profile cards with gradient headers
   - Empty state with Fluent icon composition
   - Drag-drop styling for import

6. **Test List View**
   - Interactive test cards with type icons
   - Run button with gradient accent
   - Staggered entrance animations

7. **Run Progress View**
   - Centered progress ring (120px)
   - Current test card with pulse animation
   - Completed tests mini-list

8. **Results View**
   - Summary dashboard with stats cards
   - Donut chart visualization
   - Filter tabs (All/Pass/Fail/Skip)
   - Expandable result cards

9. **Other Views**
   - Test Config View with locked field indicators
   - Diagnostics View with key-value cards
   - Credential Prompt Dialog restyling

### Phase 4: Polish

10. **Animations**
    - Page transition animations
    - Hover micro-interactions
    - Staggered list entrances
    - Reduced motion support

11. **Accessibility**
    - Focus ring styling
    - Keyboard navigation testing
    - Screen reader announcements

## Key Code Patterns

### Converting to FluentWindow

```xml
<!-- MainWindow.xaml -->
<ui:FluentWindow x:Class="ReqChecker.App.MainWindow"
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
    WindowBackdropType="Mica"
    ExtendsContentIntoTitleBar="True">
```

```csharp
// MainWindow.xaml.cs
public partial class MainWindow : FluentWindow
```

### Theme-Aware Colors

```xml
<!-- Colors.Dark.xaml -->
<ResourceDictionary>
    <SolidColorBrush x:Key="BackgroundBase" Color="#0f0f1a"/>
    <SolidColorBrush x:Key="BackgroundSurface" Color="#1a1a2e"/>
    <SolidColorBrush x:Key="TextPrimary" Color="#ffffff"/>
</ResourceDictionary>
```

### Animation with Reduced Motion Support

```xml
<Style.Triggers>
    <DataTrigger Binding="{Binding IsReducedMotion}" Value="False">
        <DataTrigger.EnterActions>
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation
                        Storyboard.TargetProperty="Opacity"
                        From="0" To="1" Duration="0:0:0.2"/>
                </Storyboard>
            </BeginStoryboard>
        </DataTrigger.EnterActions>
    </DataTrigger>
</Style.Triggers>
```

### Card with Elevation

```xml
<Border Style="{StaticResource CardStyle}">
    <Border.Effect>
        <DropShadowEffect
            Color="{DynamicResource ElevationColor}"
            BlurRadius="10" ShadowDepth="0" Opacity="0.1"/>
    </Border.Effect>
</Border>
```

### Status Badge with Glow

```xml
<Border Background="{Binding Status, Converter={StaticResource StatusColorConverter}}"
        CornerRadius="12" Padding="8,4">
    <Border.Effect>
        <DropShadowEffect
            Color="{Binding Status, Converter={StaticResource StatusGlowConverter}}"
            BlurRadius="8" ShadowDepth="0" Opacity="0.3"/>
    </Border.Effect>
    <TextBlock Text="{Binding Status}" Foreground="White"/>
</Border>
```

## Testing Checklist

- [ ] Theme switches smoothly without layout shifts
- [ ] Theme persists across app restarts
- [ ] Sidebar state persists across app restarts
- [ ] All animations run at 60fps (use VS profiler)
- [ ] Reduced motion setting disables decorative animations
- [ ] All views render correctly on Windows 10 (no Mica)
- [ ] All views render correctly on Windows 11 (with Mica)
- [ ] Keyboard navigation reaches all interactive elements
- [ ] Focus rings are clearly visible
- [ ] High DPI displays render crisp (100%, 125%, 150%, 200%)

## Common Issues

### Mica not showing on Windows 11

Ensure `WindowBackdropType="Mica"` is set and the window has transparency:
```xml
AllowsTransparency="False"
```
Note: `AllowsTransparency` must be `False` for Mica to work.

### Animations janky

- Use `RenderOptions.BitmapScalingMode="HighQuality"` for images
- Avoid animating `Margin` - use `RenderTransform` with `TranslateTransform`
- Use `SnapsToDevicePixels="True"` on containers

### Theme colors not updating

Ensure colors are referenced as `{DynamicResource}` not `{StaticResource}`:
```xml
<Border Background="{DynamicResource BackgroundSurface}"/>
```

## Files to Modify

### New Files
- `Resources/Styles/Colors.Dark.xaml`
- `Resources/Styles/Colors.Light.xaml`
- `Resources/Styles/Typography.xaml`
- `Resources/Styles/Spacing.xaml`
- `Resources/Styles/Animations.xaml`
- `Resources/Styles/Controls.xaml`
- `Services/PreferencesService.cs`
- `Controls/ExpanderCard.xaml` + `.cs`
- `Controls/SummaryCard.xaml` + `.cs`

### Modified Files
- `MainWindow.xaml` - Convert to FluentWindow with NavigationView
- `MainWindow.xaml.cs` - Update base class and navigation logic
- `App.xaml` - Update resource dictionary merging
- `Services/ThemeService.cs` - Enhance with persistence
- `Controls/TestStatusBadge.xaml` - Add glow effects
- `Controls/ProgressRing.xaml` - Gradient stroke implementation
- `Views/*.xaml` - All 7 views need visual updates

## Running the App

```bash
cd src/ReqChecker.App
dotnet run
```

Or press F5 in Visual Studio 2022 with ReqChecker.App as startup project.
