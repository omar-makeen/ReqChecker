# Quickstart: Premium Page Headers

**Feature**: 018-premium-page-headers
**Branch**: `018-premium-page-headers`

## Overview

Enhance all 4 main page headers with premium visual treatment: gradient accent lines, icon containers, refined typography, subtitles, and entrance animations.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# Dev Kit
- Windows 10/11 (target platform)

## Quick Setup

```bash
# Checkout feature branch
git checkout 018-premium-page-headers

# Build and run
cd src/ReqChecker.App
dotnet build
dotnet run
```

## Implementation Tasks

### Task 1: Add AnimatedPageHeader Style

**File**: `src/ReqChecker.App/Resources/Styles/Controls.xaml`

Add entrance animation style for page headers:
```xml
<!-- Premium Page Header Animation -->
<Style x:Key="AnimatedPageHeader" TargetType="Border">
    <Setter Property="Opacity" Value="0"/>
    <Setter Property="RenderTransform">
        <Setter.Value>
            <TranslateTransform Y="20"/>
        </Setter.Value>
    </Setter>
    <Style.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                                     From="0" To="1" Duration="0:0:0.3">
                        <DoubleAnimation.EasingFunction>
                            <CubicEase EasingMode="EaseOut"/>
                        </DoubleAnimation.EasingFunction>
                    </DoubleAnimation>
                    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
                                     From="20" To="0" Duration="0:0:0.3">
                        <DoubleAnimation.EasingFunction>
                            <CubicEase EasingMode="EaseOut"/>
                        </DoubleAnimation.EasingFunction>
                    </DoubleAnimation>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Style.Triggers>
</Style>
```

### Task 2: Update TestListView Header

**File**: `src/ReqChecker.App/Views/TestListView.xaml`

Replace the existing header with premium design including:
- Gradient accent line (4px)
- Icon in 48px accent-colored container
- Larger title (24px, SemiBold)
- Subtitle: "Run diagnostics and verify requirements"
- Enhanced count badge with accent background

### Task 3: Update ProfileSelectorView Header

**File**: `src/ReqChecker.App/Views/ProfileSelectorView.xaml`

- Gradient accent line
- Folder icon in accent container
- Title: "Profile Manager"
- Subtitle: "Manage and import test profiles"

### Task 4: Update ResultsView Header

**File**: `src/ReqChecker.App/Views/ResultsView.xaml`

- Gradient accent line
- Chart icon in accent container
- Title: "Results Dashboard"
- Subtitle: "View test execution results"

### Task 5: Update DiagnosticsView Header

**File**: `src/ReqChecker.App/Views/DiagnosticsView.xaml`

- Gradient accent line
- Stethoscope icon in accent container
- Title: "System Diagnostics"
- Subtitle: "System information and logs"

## Header Structure Template

Use this structure for all pages:

```xml
<!-- Premium Page Header -->
<Border Style="{StaticResource AnimatedPageHeader}" Margin="0,0,0,24">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Gradient Accent Line -->
        <Border Grid.Row="0" Height="4"
                Background="{DynamicResource AccentGradientHorizontal}"
                CornerRadius="2"/>

        <!-- Header Content -->
        <Grid Grid.Row="1" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Icon Container -->
            <Border Grid.Column="0" Width="48" Height="48"
                    Background="{DynamicResource AccentPrimary}"
                    CornerRadius="12" Margin="0,0,16,0">
                <ui:SymbolIcon Symbol="[PAGE_ICON]"
                               FontSize="24"
                               Foreground="White"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"/>
            </Border>

            <!-- Title + Subtitle -->
            <StackPanel Grid.Column="1" VerticalAlignment="Center">
                <TextBlock Text="[PAGE_TITLE]"
                           FontSize="24" FontWeight="SemiBold"
                           Foreground="{DynamicResource TextPrimary}"/>
                <TextBlock Text="[PAGE_SUBTITLE]"
                           Style="{DynamicResource TextBody}"
                           Foreground="{DynamicResource TextSecondary}"
                           Margin="0,4,0,0"/>
            </StackPanel>

            <!-- Action Buttons (existing) -->
            <StackPanel Grid.Column="2" Orientation="Horizontal">
                <!-- Keep existing buttons -->
            </StackPanel>
        </Grid>
    </Grid>
</Border>
```

## Page-Specific Content

| Page | Icon | Title | Subtitle | Metadata |
|------|------|-------|----------|----------|
| Profile Manager | Folder24 | Profile Manager | Manage and import test profiles | - |
| Test Suite | Beaker24 | Test Suite | Run diagnostics and verify requirements | N tests (badge) |
| Results Dashboard | Poll24 | Results Dashboard | View test execution results | - |
| System Diagnostics | Stethoscope24 | System Diagnostics | System information and logs | - |

## Verification

1. Launch application
2. Navigate to each page (Profile Manager, Test Suite, Results Dashboard, System Diagnostics)
3. Verify each page has:
   - ✅ Gradient accent line at top of header
   - ✅ Icon in colored container
   - ✅ Larger title text
   - ✅ Subtitle below title
   - ✅ Smooth entrance animation
   - ✅ Action buttons still functional
4. Switch between light and dark themes - verify headers look good in both

## Troubleshooting

**Issue**: Header not animating
- Verify `AnimatedPageHeader` style is defined in Controls.xaml
- Check that style is applied to outer Border

**Issue**: Colors look wrong in dark mode
- Use DynamicResource for all colors, not StaticResource
- Verify AccentPrimary and AccentGradientHorizontal support dark mode
