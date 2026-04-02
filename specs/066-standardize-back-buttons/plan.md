# Implementation Plan: Standardize Back Buttons

**Branch**: `066-standardize-back-buttons` | **Date**: 2026-03-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/066-standardize-back-buttons/spec.md`

## Summary

Standardize all back buttons across 4 views (TestConfigView, HistoryView, ResultsView, RunProgressView) to use the same position (top-left, Column 0), style (SecondaryButton), label ("Back to Tests" + ArrowLeft icon), and tooltip ("Return to the test suite"). Preserves existing behaviors (unsaved changes prompt on TestConfig, conditional visibility on RunProgress).

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only)
**Testing**: dotnet test (ReqChecker.App.Tests)
**Target Platform**: Windows 10/11 desktop
**Project Type**: Desktop app (WPF)
**Performance Goals**: N/A (no runtime impact)
**Constraints**: XAML-only changes across 4 files
**Scale/Scope**: 4 XAML files, ~20-30 lines changed per file

## Constitution Check

*GATE: Constitution template is not configured for this project — no gates to enforce.*

All changes are XAML-only, no new dependencies, no new files, no architectural changes.

## Project Structure

### Documentation (this feature)

```text
specs/066-standardize-back-buttons/
├── plan.md              # This file
├── research.md          # Phase 0 output — layout analysis findings
├── quickstart.md        # Phase 1 output — verification guide
└── checklists/
    └── requirements.md  # Spec quality checklist
```

### Source Code (repository root)

```text
src/ReqChecker.App/
└── Views/
    ├── TestConfigView.xaml     # Restyle GhostButton → SecondaryButton, update label/tooltip
    ├── HistoryView.xaml        # Move back button to Column 0, restyle
    ├── ResultsView.xaml        # Move back button to Column 0, add ShowOnDisabled
    └── RunProgressView.xaml    # Move back button to Column 0, restyle PrimaryButton → SecondaryButton
```

**Structure Decision**: No new files. Four XAML files modified.

## Implementation

### Standard Back Button Pattern

All four views must use this exact back button markup (Column 0, before the page icon/title):

```xml
<Button x:Name="BackToTestsButton"
        Grid.Column="0"
        Style="{StaticResource SecondaryButton}"
        Command="{Binding [BackCommand or NavigateToTestListCommand]}"
        TabIndex="0"
        Margin="0,0,16,0"
        VerticalAlignment="Center"
        ToolTip="Return to the test suite"
        ToolTipService.InitialShowDelay="400"
        ToolTipService.ShowOnDisabled="True">
    <StackPanel Orientation="Horizontal">
        <ui:SymbolIcon Symbol="ArrowLeft24" FontSize="16" Margin="0,0,8,0"/>
        <TextBlock Text="Back to Tests"/>
    </StackPanel>
</Button>
```

### Header Grid Restructure Pattern

Current layout on HistoryView, ResultsView, RunProgressView:
```
Column 0 (Auto): Icon container (48x48)
Column 1 (*):    Title + Subtitle
Column 2 (Auto): Action buttons (includes back button)
```

Target layout (matching TestConfigView's pattern):
```
Column 0 (Auto): Back button
Column 1 (Auto): Icon container (48x48)
Column 2 (*):    Title + Subtitle
Column 3 (Auto): Action buttons (back button removed)
```

This requires adding a 4th ColumnDefinition to the header Grid and shifting column assignments.

### Change 1: TestConfigView.xaml (FR-002, FR-003, FR-004)

**Current state**: Position already correct (Column 0). Uses `GhostButton`, label "Back", tooltip "Return to test list".

**Changes** (minimal — style, label, tooltip only):
- `Style="{StaticResource GhostButton}"` → `Style="{StaticResource SecondaryButton}"`
- `<TextBlock Text="Back"/>` → `<TextBlock Text="Back to Tests"/>`
- `ToolTip="Return to test list"` → `ToolTip="Return to the test suite"`
- Keep `Command="{Binding BackCommand}"` unchanged (preserves unsaved changes prompt — FR-005)
- Keep `x:Name="BackButton"` unchanged (preserves FocusManager binding)

### Change 2: HistoryView.xaml (FR-001, FR-003, FR-004)

**Current state**: Back button in Column 2 right-side StackPanel. Style is SecondaryButton (correct).

**Changes**:
1. Add 4th ColumnDefinition to header Grid: `Auto | Auto | * | Auto`
2. Add back button as new element in Column 0 with standard pattern
3. Shift icon container from `Grid.Column="0"` to `Grid.Column="1"`
4. Shift title StackPanel from `Grid.Column="1"` to `Grid.Column="2"`
5. Shift action buttons StackPanel from `Grid.Column="2"` to `Grid.Column="3"`
6. Remove back button from action buttons StackPanel ("Clear All" stays)
7. Keep `x:Name="BackToTestsButton"` for FocusManager

### Change 3: ResultsView.xaml (FR-001, FR-003, FR-004)

**Current state**: Back button in Column 2 right-side StackPanel. Style is SecondaryButton (correct). Missing `ToolTipService.ShowOnDisabled`.

**Changes**:
1. Add 4th ColumnDefinition to header Grid: `Auto | Auto | * | Auto`
2. Add back button as new element in Column 0 with standard pattern
3. Shift icon container to Column 1, title to Column 2, actions to Column 3
4. Remove back button from action buttons StackPanel ("Re-run Failed" + "Export" stay)
5. Add `ToolTipService.ShowOnDisabled="True"` on new back button
6. Keep `x:Name="BackToTestsButton"` for FocusManager

### Change 4: RunProgressView.xaml (FR-001, FR-002, FR-003, FR-004, FR-006)

**Current state**: Back button in Column 2 completion StackPanel. Style is `PrimaryButton` (wrong). Visibility bound to `IsComplete`.

**Changes**:
1. Add 4th ColumnDefinition to header Grid: `Auto | Auto | * | Auto`
2. Add back button as new element in Column 0 with standard pattern
3. Add `Visibility="{Binding IsComplete, Converter={StaticResource BoolToVisibilityConverter}}"` on the Column 0 back button (preserve FR-006)
4. Change style from `PrimaryButton` to `SecondaryButton`
5. Shift icon container to Column 1, title to Column 2, actions to Column 3
6. Remove back button from completion StackPanel ("View Results" stays)
7. Cancel button (Column 3, conditional on IsRunning) is unaffected
8. FocusManager stays on CancelButton (correct — Cancel is primary during execution)

### Preserved Behaviors

- **TestConfigView**: `BackCommand` with unsaved changes prompt — ViewModel unchanged (FR-005)
- **RunProgressView**: `Visibility="{Binding IsComplete}"` on back button — same binding on new Column 0 element (FR-006)
- **ResultsView empty state**: "Go to Tests" GhostButton — untouched (FR-007)
- **FocusManager**: All `FocusedElement` bindings unchanged
- **All tooltips**: Standardized to `ModernToolTip`-compatible text with `ShowOnDisabled="True"`

## Complexity Tracking

No violations to justify — these are straightforward XAML layout changes with no new dependencies or architectural changes.
