# Research: Premium Page Headers

**Feature**: 018-premium-page-headers
**Date**: 2026-02-01

## Research Questions

### 1. What design pattern creates a "premium" header appearance?

**Decision**: Layered visual hierarchy with gradient accent, contained icon, and typography depth

**Rationale**:
- Modern premium apps (Figma, Linear, Notion) use gradient accent lines to create visual distinction
- Icons in colored containers create focal points and establish page identity
- Typography hierarchy (large title + smaller subtitle) improves scannability
- Subtle shadows/elevation add depth without being distracting

**Design Elements**:
1. **Gradient accent line** (4-6px) at top of header area
2. **Icon container** (40-48px) with accent background and rounded corners
3. **Title** in larger, semi-bold typography
4. **Subtitle** in smaller, secondary color for context
5. **Enhanced count badge** with accent background instead of plain text

### 2. What existing design tokens can be reused?

**Decision**: Leverage existing AccentPrimary, AccentGradientHorizontal, BackgroundElevated, and TextH1/TextBody styles

**Existing tokens identified in Controls.xaml and Theme.xaml**:
- `AccentPrimary` - Primary brand color for icon backgrounds
- `AccentGradientHorizontal` - Gradient for accent lines (already defined)
- `BackgroundElevated` - Elevated surface color for containers
- `TextPrimary`, `TextSecondary` - Typography colors
- `TextH1`, `TextH3`, `TextBody`, `TextCaption` - Typography styles

**Rationale**: Reusing existing tokens ensures consistency and theme support (light/dark).

### 3. How to implement entrance animation?

**Decision**: Reuse existing `AnimatedCard` pattern with fade + translateY

**Rationale**:
- The codebase already has `AnimatedTestCard`, `AnimatedProfileCard`, `AnimatedDiagCard` styles
- Pattern uses `Loaded` event trigger with Storyboard
- Fade from 0 to 1 opacity + TranslateY from 20px to 0
- Duration: 300ms with CubicEase EaseOut

**Implementation**: Create `AnimatedPageHeader` style following same pattern.

### 4. What contextual subtitles/metadata should each page display?

**Decision**: Page-specific contextual information

| Page | Subtitle/Metadata |
|------|-------------------|
| Profile Manager | "Manage and import test profiles" |
| Test Suite | "{N} tests" badge + "Run diagnostics and verify requirements" |
| Results Dashboard | "View test execution results" + optional last run info |
| System Diagnostics | "System information and logs" |

**Rationale**: Each page gets a brief description that helps users understand the page purpose at a glance.

### 5. Should we create a reusable control or inline XAML?

**Decision**: Create a reusable XAML style pattern, not a custom control

**Rationale**:
- Each page header has slightly different content (different icons, subtitles, metadata)
- A full custom control adds complexity for little benefit
- A consistent XAML pattern/style is easier to maintain
- Follow existing pattern where each page defines its own header structure

**Alternatives Considered**:
- Custom UserControl: Rejected - over-engineering for 4 pages with different content
- ContentControl with DataTemplate: Rejected - adds complexity

## Technical Findings

### Current Header Structure (TestListView.xaml example)

```xml
<StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
    <ui:SymbolIcon Symbol="Beaker24"
                   FontSize="24"
                   Foreground="{DynamicResource AccentPrimary}"
                   Margin="0,0,12,0"/>
    <TextBlock Text="Test Suite"
               Style="{DynamicResource TextH1}"
               Foreground="{DynamicResource TextPrimary}"/>
    <Border Background="{DynamicResource BackgroundElevated}"
            CornerRadius="10"
            Padding="10,4"
            Margin="12,0,0,0">
        <TextBlock Text="{Binding CurrentProfile.Tests.Count}"
                   Style="{DynamicResource TextCaption}"/>
    </Border>
</StackPanel>
```

### Proposed Premium Header Structure

```xml
<!-- Premium Page Header -->
<Border Style="{StaticResource AnimatedPageHeader}">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <!-- Gradient accent line -->
            <RowDefinition Height="*"/>    <!-- Header content -->
        </Grid.RowDefinitions>

        <!-- Gradient Accent Line -->
        <Border Grid.Row="0" Height="4" Background="{DynamicResource AccentGradientHorizontal}"/>

        <!-- Header Content -->
        <Grid Grid.Row="1" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/> <!-- Icon -->
                <ColumnDefinition Width="*"/>    <!-- Title/Subtitle -->
                <ColumnDefinition Width="Auto"/> <!-- Metadata badge -->
            </Grid.ColumnDefinitions>

            <!-- Icon Container -->
            <Border Grid.Column="0" Width="48" Height="48"
                    Background="{DynamicResource AccentPrimary}"
                    CornerRadius="12" Margin="0,0,16,0">
                <ui:SymbolIcon Symbol="Beaker24" FontSize="24" Foreground="White"/>
            </Border>

            <!-- Title + Subtitle -->
            <StackPanel Grid.Column="1" VerticalAlignment="Center">
                <TextBlock Text="Test Suite"
                           FontSize="24" FontWeight="SemiBold"
                           Foreground="{DynamicResource TextPrimary}"/>
                <TextBlock Text="Run diagnostics and verify requirements"
                           Style="{DynamicResource TextBody}"
                           Foreground="{DynamicResource TextSecondary}"
                           Margin="0,4,0,0"/>
            </StackPanel>

            <!-- Enhanced Count Badge -->
            <Border Grid.Column="2" Background="{DynamicResource AccentPrimary}"
                    CornerRadius="16" Padding="16,8" VerticalAlignment="Center">
                <TextBlock Foreground="White" FontWeight="SemiBold">
                    <Run Text="{Binding CurrentProfile.Tests.Count}"/>
                    <Run Text=" tests"/>
                </TextBlock>
            </Border>
        </Grid>
    </Grid>
</Border>
```

## Conclusion

This is a straightforward UI enhancement:
- Reuses existing design tokens and animation patterns
- Modifies 4 XAML files with consistent header structure
- Adds ~50 lines of XAML per page
- No new dependencies or architectural changes
