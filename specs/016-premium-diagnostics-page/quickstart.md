# Quickstart: Premium System Diagnostics Page

**Feature**: 016-premium-diagnostics-page

## Overview

This feature fixes the System Diagnostics page by defining three missing XAML card styles that are referenced but not defined, causing potential rendering issues.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension

## Quick Verification

```bash
# Build to verify no XAML errors
dotnet build src/ReqChecker.App

# Run the application
dotnet run --project src/ReqChecker.App
```

## Files to Modify

| File | Action |
|------|--------|
| `src/ReqChecker.App/Resources/Styles/Controls.xaml` | Add 3 new style definitions |

## Style Definitions to Add

Add the following styles to Controls.xaml after the existing CARD STYLES section:

### 1. DiagnosticCard

Standard card for diagnostic information sections (Machine Info, Network Interfaces).

```xml
<Style x:Key="DiagnosticCard" TargetType="Border" BasedOn="{StaticResource Card}">
    <Setter Property="Padding" Value="24"/>
    <Setter Property="Margin" Value="0,0,0,16"/>
    <Setter Property="CornerRadius" Value="12"/>
</Style>
```

### 2. DiagnosticCardHighlight

Highlighted card for the Last Run Summary section - draws attention with accent styling.

```xml
<Style x:Key="DiagnosticCardHighlight" TargetType="Border" BasedOn="{StaticResource DiagnosticCard}">
    <Setter Property="Background" Value="{DynamicResource BackgroundElevated}"/>
    <Setter Property="BorderBrush" Value="{DynamicResource AccentPrimary}"/>
    <Setter Property="BorderThickness" Value="2"/>
    <Setter Property="Effect">
        <Setter.Value>
            <DropShadowEffect Color="{DynamicResource AccentPrimaryColor}"
                              BlurRadius="15"
                              ShadowDepth="0"
                              Opacity="0.3"
                              Direction="0"/>
        </Setter.Value>
    </Setter>
</Style>
```

### 3. NetworkInterfaceCard

Compact card for individual network interface items in the list.

```xml
<Style x:Key="NetworkInterfaceCard" TargetType="Border">
    <Setter Property="Background" Value="{DynamicResource BackgroundElevated}"/>
    <Setter Property="BorderBrush" Value="{DynamicResource BorderSubtle}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="CornerRadius" Value="8"/>
    <Setter Property="Padding" Value="16"/>
    <Setter Property="Margin" Value="0,0,0,8"/>
</Style>
```

## Verification Steps

1. **Build Check**: `dotnet build src/ReqChecker.App` - no errors
2. **Run Application**: Navigate to System Diagnostics page
3. **Visual Check**:
   - Last Run Summary card has accent border and glow
   - Machine Information card has standard styling
   - Network Interfaces display with compact cards
4. **Theme Check**: Toggle theme and verify all cards adapt correctly

## Common Issues

| Issue | Solution |
|-------|----------|
| Style not found error | Ensure styles are inside `<ResourceDictionary>` tags |
| Hover not working | DiagnosticCard/DiagnosticCardHighlight should inherit from Card which has hover |
| Wrong colors in theme | Use `DynamicResource` not `StaticResource` for color bindings |
