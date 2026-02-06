# Implementation Plan: Premium Results Dashboard

**Branch**: `027-premium-results` | **Date**: 2026-02-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/027-premium-results/spec.md`

## Summary

The Results Dashboard displays raw machine formats for dates (`2/6/2026 12:07 PM +00:00`), durations (`00:00:04.6432312`), and has no overflow handling for long profile names. This plan applies the existing FriendlyDateConverter and DurationFormatConverter (already used on the History page) to the Results page, adds text truncation with tooltip for the profile name, and fixes inconsistent duration formatting in individual test result cards.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (in-memory RunReport via IAppState)
**Testing**: Manual visual verification (WPF UI changes)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single desktop application
**Performance Goals**: N/A (UI-only formatting changes)
**Constraints**: No new dependencies; reuse existing converters
**Scale/Scope**: 2 files modified (ResultsView.xaml, no new files)

## Constitution Check

*GATE: Constitution is not configured for this project (blank template). No gates to enforce. Proceeding.*

## Project Structure

### Documentation (this feature)

```text
specs/027-premium-results/
├── plan.md              # This file
├── research.md          # Phase 0 output (completed)
├── spec.md              # Feature specification
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (files to modify)

```text
src/ReqChecker.App/
├── Views/
│   └── ResultsView.xaml          # Primary change: summary card + result list bindings
└── Controls/
    └── (no changes needed)       # ExpanderCard accepts string Subtitle — compatible
```

**Structure Decision**: No new files needed. All changes are XAML binding updates in `ResultsView.xaml`. The existing converters (`FriendlyDateConverter`, `DurationFormatConverter`) are already registered in `App.xaml` and require no modification.

## Implementation Details

### Change 1: Summary Card — Started Date (FR-001, FR-005)

**File**: `src/ReqChecker.App/Views/ResultsView.xaml` (lines ~232-238)

**Current**:
```xaml
<TextBlock Style="{DynamicResource TextBody}"
           Foreground="{DynamicResource TextSecondary}"
           Margin="0,0,0,8">
    <Run Text="Started: "/>
    <Run Text="{Binding Report.StartTime}"
         Foreground="{DynamicResource TextPrimary}"
         FontWeight="SemiBold"/>
</TextBlock>
```

**Target**: Replace with a horizontal `StackPanel` containing a label `<TextBlock>` and a value `<TextBlock>` that uses the FriendlyDateConverter:
```xaml
<StackPanel Orientation="Horizontal" Margin="0,0,0,8">
    <TextBlock Text="Started: "
               Style="{DynamicResource TextBody}"
               Foreground="{DynamicResource TextSecondary}"/>
    <TextBlock Text="{Binding Report.StartTime, Converter={StaticResource FriendlyDateConverter}}"
               Style="{DynamicResource TextBody}"
               Foreground="{DynamicResource TextPrimary}"
               FontWeight="SemiBold"/>
</StackPanel>
```

**Why StackPanel instead of Run**: `<Run>` bindings do not reliably support the `Converter` property. A `StackPanel` with two `TextBlock` elements achieves the same inline visual layout while supporting converter bindings.

### Change 2: Summary Card — Duration (FR-002, FR-005)

**File**: `src/ReqChecker.App/Views/ResultsView.xaml` (lines ~240-245)

**Current**:
```xaml
<TextBlock Style="{DynamicResource TextBody}"
           Foreground="{DynamicResource TextSecondary}">
    <Run Text="Duration: "/>
    <Run Text="{Binding Report.Duration}"
         Foreground="{DynamicResource TextPrimary}"
         FontWeight="SemiBold"/>
</TextBlock>
```

**Target**: Same StackPanel pattern with DurationFormatConverter:
```xaml
<StackPanel Orientation="Horizontal">
    <TextBlock Text="Duration: "
               Style="{DynamicResource TextBody}"
               Foreground="{DynamicResource TextSecondary}"/>
    <TextBlock Text="{Binding Report.Duration, Converter={StaticResource DurationFormatConverter}}"
               Style="{DynamicResource TextBody}"
               Foreground="{DynamicResource TextPrimary}"
               FontWeight="SemiBold"/>
</StackPanel>
```

### Change 3: Summary Card — Profile Name Truncation (FR-003, FR-006)

**File**: `src/ReqChecker.App/Views/ResultsView.xaml` (lines ~224-230)

**Current**:
```xaml
<TextBlock Style="{DynamicResource TextBody}"
           Foreground="{DynamicResource TextSecondary}"
           Margin="0,0,0,8">
    <Run Text="Profile: "/>
    <Run Text="{Binding Report.ProfileName}"
         Foreground="{DynamicResource TextPrimary}"
         FontWeight="SemiBold"/>
</TextBlock>
```

**Target**: StackPanel with truncation and tooltip on the value TextBlock:
```xaml
<StackPanel Orientation="Horizontal" Margin="0,0,0,8">
    <TextBlock Text="Profile: "
               Style="{DynamicResource TextBody}"
               Foreground="{DynamicResource TextSecondary}"/>
    <TextBlock Text="{Binding Report.ProfileName}"
               Style="{DynamicResource TextBody}"
               Foreground="{DynamicResource TextPrimary}"
               FontWeight="SemiBold"
               TextTrimming="CharacterEllipsis"
               MaxWidth="350"
               ToolTip="{Binding Report.ProfileName}"/>
</StackPanel>
```

**MaxWidth of 350**: This allows roughly 40-50 characters before truncation, fitting comfortably in the metadata column of the summary card grid while preventing overflow into adjacent columns.

### Change 4: ExpanderCard Subtitle — Consistent Duration (FR-004, FR-005)

**File**: `src/ReqChecker.App/Views/ResultsView.xaml` (lines ~389)

**Current**:
```xaml
Subtitle="{Binding Duration, StringFormat='{}{0:N0}ms'}"
```

**Target**:
```xaml
Subtitle="{Binding Duration, Converter={StaticResource DurationFormatConverter}}"
```

**Why this works**: ExpanderCard.Subtitle is a `string` DependencyProperty. DurationFormatConverter.Convert() returns a `string`. The binding resolves correctly without type conflicts.

## Complexity Tracking

No constitution violations. No new abstractions, patterns, or files introduced.

## Verification Plan

1. `dotnet build src/ReqChecker.App` — must compile with 0 errors
2. Run the app, execute a test suite, navigate to Results Dashboard:
   - Started field shows "Today at h:mm AM/PM" (not raw date)
   - Duration field shows compact format like "4.6s" (not "00:00:04.6432312")
   - Profile name with 60+ characters truncates with ellipsis; tooltip shows full name
3. Expand individual test result cards:
   - Duration subtitle shows compact format (not raw "X,XXXms")
4. Visual consistency check: compare Results Dashboard formatting with History page formatting — both should use identical date and duration styles
