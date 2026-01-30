# Research: UI/UX Premium Redesign

## WPF-UI FluentWindow Implementation

**Decision**: Use WPF-UI's `FluentWindow` base class for the main window

**Rationale**:
- Native Windows 11 Mica backdrop support built-in
- Automatic fallback to solid colors on Windows 10
- Preserves native window behaviors (snap layouts, drag-to-dock, accessibility)
- Eliminates need for manual WindowChrome configuration
- Already a dependency in the project (WPF-UI 4.2.0)

**Alternatives Considered**:
- `WindowStyle=None` with custom chrome: More control but loses native behaviors, requires significant custom code
- Standard window with content customization only: Limits design flexibility, can't achieve seamless title bar

**Implementation Notes**:
```csharp
// Change MainWindow base class
public partial class MainWindow : FluentWindow

// XAML namespace
xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
<ui:FluentWindow>
```

## Sidebar Navigation Pattern

**Decision**: Use WPF-UI's `NavigationView` control with custom styling

**Rationale**:
- Built-in collapse/expand with `IsPaneOpen` binding
- Supports icon + text items natively
- Keyboard navigation and accessibility built-in
- Smooth animation for pane transitions
- Frame-based content switching

**Alternatives Considered**:
- Custom sidebar control: Full control but reinvents the wheel, accessibility issues
- Standard WPF TreeView: Not designed for navigation, poor UX

**Implementation Notes**:
```xml
<ui:NavigationView
    IsPaneOpen="{Binding IsSidebarExpanded}"
    PaneDisplayMode="LeftCompact"
    OpenPaneLength="240"
    CompactPaneLength="72">
```

## Animation System Approach

**Decision**: Use WPF Storyboards with custom easing functions via resource dictionaries

**Rationale**:
- Native WPF approach, no additional dependencies
- Easing functions can be defined as reusable resources
- Works with WPF-UI's existing animation infrastructure
- Can be conditionally disabled for reduced motion

**Alternatives Considered**:
- Third-party animation libraries (e.g., FluentTransitions): Additional dependency, potential conflicts
- Code-behind animations: Harder to maintain, not declarative

**Implementation Notes**:
```xml
<!-- Define in App.xaml or Theme.xaml -->
<QuadraticEase x:Key="EaseOut" EasingMode="EaseOut"/>
<Duration x:Key="FastDuration">0:0:0.15</Duration>
<Duration x:Key="NormalDuration">0:0:0.2</Duration>
<Duration x:Key="SlowDuration">0:0:0.3</Duration>
```

## Reduced Motion Detection

**Decision**: Check Windows system setting via `SystemParameters.ClientAreaAnimation`

**Rationale**:
- Built-in WPF API, no P/Invoke needed
- Automatically reflects system preference
- Can be bound to control animation behavior

**Alternatives Considered**:
- Registry key check: Requires P/Invoke, more complex
- Always enable animations: Accessibility violation

**Implementation Notes**:
```csharp
public bool IsReducedMotionEnabled => !SystemParameters.ClientAreaAnimation;
// Bind to animation triggers to conditionally disable
```

## Theme Persistence

**Decision**: Use `IsolatedStorageSettings` or simple JSON file in `%APPDATA%`

**Rationale**:
- Lightweight, no database needed
- Standard Windows app data location
- Already have JSON infrastructure in project

**Alternatives Considered**:
- Windows Registry: Heavier, requires more permissions
- User.config (Settings.settings): Can cause issues with single-file publish

**Implementation Notes**:
```csharp
// Location: %APPDATA%/ReqChecker/preferences.json
{
  "theme": "dark",
  "sidebarExpanded": true
}
```

## Progress Ring Implementation

**Decision**: Create custom `ProgressRing` control using WPF Path and Arc geometry

**Rationale**:
- Full control over gradient stroke and animation
- WPF-UI's built-in ProgressRing is simpler (no gradient support)
- Can animate stroke-dashoffset for smooth progress

**Alternatives Considered**:
- WPF-UI ProgressRing: Limited customization for gradient stroke
- Third-party control: Additional dependency

**Implementation Notes**:
- Use `ArcSegment` within `PathGeometry`
- Animate `StrokeDashOffset` for progress effect
- Apply `LinearGradientBrush` to stroke

## Status Badge Glow Effect

**Decision**: Use `DropShadowEffect` with color matching status

**Rationale**:
- Native WPF effect, GPU-accelerated
- Can be animated for pulse effect
- Low performance impact for small elements

**Alternatives Considered**:
- Custom shader: Overkill for simple glow
- Multiple layered elements: Performance overhead

**Implementation Notes**:
```xml
<Border>
    <Border.Effect>
        <DropShadowEffect Color="#10b981" BlurRadius="8" ShadowDepth="0" Opacity="0.3"/>
    </Border.Effect>
</Border>
```

## Card Elevation System

**Decision**: Use dynamic `DropShadowEffect` with theme-aware colors

**Rationale**:
- Different shadow colors for dark (subtle glow) vs light (traditional shadow)
- Can animate blur radius on hover
- Consistent with Fluent Design elevation

**Alternatives Considered**:
- Multiple stacked borders: Complex XAML, hard to animate
- Static shadows only: Less engaging

**Implementation Notes**:
- Dark theme: Subtle cyan glow (`rgba(0, 217, 255, 0.05)`)
- Light theme: Traditional gray shadow (`rgba(0, 0, 0, 0.1)`)
- Hover: Increase blur radius from 10 to 20

## Donut Chart for Results

**Decision**: Use custom WPF drawing with `DrawingVisual` or Path-based approach

**Rationale**:
- Lightweight, no charting library dependency
- Full control over animation (arc draw-in effect)
- Simple requirement (just 3 segments)

**Alternatives Considered**:
- LiveCharts2: Heavy dependency for simple chart
- OxyPlot: Overkill, no easy animation

**Implementation Notes**:
- Use three `ArcSegment` paths with percentage-based angles
- Animate `StrokeDashOffset` for draw-in effect
- Include center text for total/percentage

## Virtualization for Large Lists

**Decision**: Use WPF `VirtualizingStackPanel` with `VirtualizingPanel.ScrollUnit="Pixel"`

**Rationale**:
- Built-in WPF feature, no additional code
- Pixel-based scrolling for smooth experience
- Works with ItemsControl and ListBox

**Alternatives Considered**:
- Custom virtualization: Complex, error-prone
- No virtualization: Performance issues with 100+ items

**Implementation Notes**:
```xml
<ItemsControl VirtualizingPanel.IsVirtualizing="True"
              VirtualizingPanel.ScrollUnit="Pixel"
              VirtualizingPanel.VirtualizationMode="Recycling">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

## Staggered Entrance Animations

**Decision**: Use `ItemsControl` with `Loaded` event and incremental delay per item

**Rationale**:
- Can be implemented via attached behavior
- Works with data binding and templates
- Delay calculated from item index

**Alternatives Considered**:
- Manual animation per item: Not scalable
- Third-party animation library: Unnecessary dependency

**Implementation Notes**:
- Create `StaggeredEntranceAnimation` attached behavior
- Trigger on `Loaded` event with delay = `index * 50ms`
- Animate `Opacity` 0→1 and `TranslateY` 10→0

## Focus Ring Styling

**Decision**: Override default focus visual with accent-colored border

**Rationale**:
- WPF allows global focus visual template override
- Consistent with design system accent color
- Meets accessibility requirements for visibility

**Implementation Notes**:
```xml
<Style x:Key="FocusVisualStyle">
    <Setter Property="Control.Template">
        <Setter.Value>
            <ControlTemplate>
                <Border BorderBrush="{DynamicResource AccentColor}"
                        BorderThickness="2"
                        CornerRadius="4"/>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```
