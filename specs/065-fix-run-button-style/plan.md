# Implementation Plan: Fix Run Button Style & UX

**Branch**: `065-fix-run-button-style` | **Date**: 2026-03-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/065-fix-run-button-style/spec.md`

## Summary

Fix two minor issues with the Run button on the Test Suite page: (1) add a contextual tooltip that explains why the button is disabled when no tests are selected, and (2) remove an inline `FontSize="15"` override that conflicts with the `PrimaryButtonLarge` style's `FontSize="16"`.

**Note**: Research revealed that the `PrimaryButtonLarge` style already exists and is correctly applied. FR-001 and FR-002 from the spec are already satisfied. Only FR-003, FR-004, and FR-005 require changes.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only fix)
**Testing**: dotnet test (ReqChecker.App.Tests)
**Target Platform**: Windows 10/11 desktop
**Project Type**: Desktop app (WPF)
**Performance Goals**: N/A (no runtime impact)
**Constraints**: Single XAML file change
**Scale/Scope**: 1 file, ~5 lines changed

## Constitution Check

*GATE: Constitution template is not configured for this project — no gates to enforce.*

All changes are XAML-only, no new dependencies, no new files, no architectural changes.

## Project Structure

### Documentation (this feature)

```text
specs/065-fix-run-button-style/
├── plan.md              # This file
├── research.md          # Phase 0 output — scope revision findings
├── quickstart.md        # Phase 1 output — verification guide
└── checklists/
    └── requirements.md  # Spec quality checklist
```

### Source Code (repository root)

```text
src/ReqChecker.App/
├── Views/
│   └── TestListView.xaml          # Run button tooltip + font size fix
└── Resources/Styles/
    └── Controls.xaml              # PrimaryButtonLarge (already exists, no changes needed)
```

**Structure Decision**: No new files. Single change to `TestListView.xaml`.

## Implementation

### Change 1: Contextual Disabled Tooltip (FR-003, FR-004)

**File**: `src/ReqChecker.App/Views/TestListView.xaml` (lines 124-134)

Replace the static `ToolTip` attribute with a `Style` trigger that swaps tooltip text based on `IsEnabled`:

```xml
<!-- Before -->
<Button x:Name="RunAllTestsButton"
        Style="{StaticResource PrimaryButtonLarge}"
        ...
        ToolTip="Execute all selected tests and view results"
        ToolTipService.InitialShowDelay="400">

<!-- After -->
<Button x:Name="RunAllTestsButton"
        Style="{StaticResource PrimaryButtonLarge}"
        ...
        ToolTipService.InitialShowDelay="400"
        ToolTipService.ShowOnDisabled="True">
    <Button.ToolTip>
        <ToolTip Style="{StaticResource ModernToolTip}">
            <ToolTip.Content>
                <TextBlock>
                    <TextBlock.Style>
                        <Style TargetType="TextBlock">
                            <Setter Property="Text" Value="Execute all selected tests and view results"/>
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding HasSelectedTests}" Value="False">
                                    <Setter Property="Text" Value="Select at least one test to run"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </TextBlock.Style>
                </TextBlock>
            </ToolTip.Content>
        </ToolTip>
    </Button.ToolTip>
```

### Change 2: Remove Inline FontSize Override (FR-005)

**File**: `src/ReqChecker.App/Views/TestListView.xaml` (line 132)

```xml
<!-- Before -->
<TextBlock Text="{Binding RunButtonLabel}" FontSize="15"/>

<!-- After -->
<TextBlock Text="{Binding RunButtonLabel}"/>
```

This lets the `PrimaryButtonLarge` style's `FontSize="16"` apply naturally.

## Complexity Tracking

No violations to justify — this is a minimal XAML-only change.
