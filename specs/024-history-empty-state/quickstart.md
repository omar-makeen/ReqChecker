# Quickstart: Improve Test History Empty State UI/UX

**Branch**: `024-history-empty-state` | **Date**: 2026-02-03

## Overview

Fix the Test History page to show a clean, prominent empty state when no history exists, by hiding redundant UI elements and improving the empty state design.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension

## Quick Implementation Guide

### Step 1: Fix ViewModel Status Message (HistoryViewModel.cs)

**File**: `src/ReqChecker.App/ViewModels/HistoryViewModel.cs`

**Change** (around line 124):

```csharp
// BEFORE:
StatusMessage = $"Loaded {HistoryRuns.Count} historical runs";
IsStatusError = false;

// AFTER:
if (HistoryRuns.Count > 0)
{
    StatusMessage = $"Loaded {HistoryRuns.Count} historical runs";
    IsStatusError = false;
}
// StatusMessage stays null when empty (already cleared at line 92)
```

### Step 2: Fix Trend Chart Visibility (HistoryView.xaml)

**File**: `src/ReqChecker.App/Views/HistoryView.xaml`

**Change** (line 179):

```xml
<!-- BEFORE: ConverterParameter=Inverse shows when count > 0 BUT may be buggy -->
<Border Grid.Row="2"
        Style="{StaticResource Card}"
        Margin="0,0,0,16"
        Visibility="{Binding TrendDataPoints.Count, Converter={StaticResource CountToVisibilityConverter}, ConverterParameter=Inverse, FallbackValue=Collapsed}">

<!-- AFTER: Bind to IsHistoryEmpty directly for consistency -->
<Border Grid.Row="2"
        Style="{StaticResource Card}"
        Margin="0,0,0,16"
        Visibility="{Binding IsHistoryEmpty, Converter={StaticResource BoolToVisibilityConverter}, ConverterParameter=Inverse, FallbackValue=Collapsed}">
```

### Step 3: Redesign Empty State (HistoryView.xaml)

**File**: `src/ReqChecker.App/Views/HistoryView.xaml`

**Replace** (lines 367-397):

```xml
<!-- BEFORE -->
<Border Grid.Row="4"
        Background="{DynamicResource BackgroundSurface}"
        BorderBrush="{DynamicResource BorderSubtle}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="48"
        HorizontalAlignment="Center"
        VerticalAlignment="Center"
        Visibility="{Binding IsHistoryEmpty, Converter={StaticResource BoolToVisibilityConverter}, FallbackValue=Collapsed}">
    <StackPanel HorizontalAlignment="Center">
        <ui:SymbolIcon Symbol="History24"
                       FontSize="48"
                       Foreground="{DynamicResource TextTertiary}"
                       HorizontalAlignment="Center"/>
        <TextBlock Text="No test history yet"
                   Style="{DynamicResource TextBody}"
                   Foreground="{DynamicResource TextSecondary}"
                   HorizontalAlignment="Center"
                   Margin="0,16,0,8"/>
        <TextBlock Text="Run tests to build your history"
                   Style="{DynamicResource TextBody}"
                   Foreground="{DynamicResource TextTertiary}"
                   HorizontalAlignment="Center"
                   Margin="0,0,0,12"/>
        <Button Style="{StaticResource GhostButton}"
                Content="Run Tests"
                Command="{Binding NavigateToTestListCommand}"
                HorizontalAlignment="Center"/>
    </StackPanel>
</Border>

<!-- AFTER -->
<Grid Grid.Row="4"
      Visibility="{Binding IsHistoryEmpty, Converter={StaticResource BoolToVisibilityConverter}, FallbackValue=Collapsed}">
    <StackPanel HorizontalAlignment="Center"
                VerticalAlignment="Center">
        <!-- Large History Icon -->
        <Border Width="80" Height="80"
                Background="{DynamicResource BackgroundSurface}"
                CornerRadius="40"
                HorizontalAlignment="Center"
                Margin="0,0,0,24">
            <ui:SymbolIcon Symbol="History24"
                           FontSize="40"
                           Foreground="{DynamicResource TextTertiary}"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center"/>
        </Border>

        <!-- Heading -->
        <TextBlock Text="No Test History Yet"
                   FontSize="20"
                   FontWeight="SemiBold"
                   Foreground="{DynamicResource TextPrimary}"
                   HorizontalAlignment="Center"
                   Margin="0,0,0,8"/>

        <!-- Description -->
        <TextBlock Text="Run your first test to start tracking results over time."
                   Style="{DynamicResource TextBody}"
                   Foreground="{DynamicResource TextSecondary}"
                   HorizontalAlignment="Center"
                   TextAlignment="Center"
                   MaxWidth="300"
                   Margin="0,0,0,24"/>

        <!-- Call-to-Action Button -->
        <Button Style="{StaticResource PrimaryButton}"
                Command="{Binding NavigateToTestListCommand}"
                HorizontalAlignment="Center"
                Padding="24,12">
            <StackPanel Orientation="Horizontal">
                <ui:SymbolIcon Symbol="Play24" FontSize="16" Margin="0,0,8,0"/>
                <TextBlock Text="Run Tests"/>
            </StackPanel>
        </Button>
    </StackPanel>
</Grid>
```

## Verification

1. **Build**: `dotnet build src/ReqChecker.App`
2. **Run the app**: `dotnet run --project src/ReqChecker.App`
3. **Navigate to Test History** (with no history data)
4. **Verify**:
   - No green status banner visible
   - No empty trend chart visible
   - No filter tabs visible
   - Prominent empty state centered on page
   - "Run Tests" button navigates to Test Suite

## Files Changed

| File | Changes |
|------|---------|
| `src/ReqChecker.App/ViewModels/HistoryViewModel.cs` | Only show status message when count > 0 |
| `src/ReqChecker.App/Views/HistoryView.xaml` | Fix trend chart binding, redesign empty state |
