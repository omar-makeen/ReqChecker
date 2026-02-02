# Implementation Plan: Improve Test History Empty State UI/UX

**Branch**: `024-history-empty-state` | **Date**: 2026-02-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/024-history-empty-state/spec.md`

## Summary

Fix the Test History page's empty state UI/UX by hiding redundant elements (status banner, empty trend chart, filter tabs) when no history exists, and displaying a prominent, centered empty state with a clear call-to-action button.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only changes)
**Testing**: Manual visual testing
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop application
**Performance Goals**: N/A (UI-only)
**Constraints**: Must follow existing design system
**Scale/Scope**: Single page modification

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- ✅ No new libraries needed
- ✅ No new projects needed
- ✅ Follows existing patterns (MVVM, WPF-UI)
- ✅ Changes are scoped to single feature (History page)

## Project Structure

### Documentation (this feature)

```text
specs/024-history-empty-state/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── quickstart.md        # Phase 1 output
└── checklists/          # Validation checklists
```

### Source Code (files to modify)

```text
src/ReqChecker.App/
├── Views/
│   └── HistoryView.xaml           # MODIFY: Update visibility bindings & empty state
└── ViewModels/
    └── HistoryViewModel.cs        # MODIFY: Don't show status message when empty
```

**Structure Decision**: Existing WPF MVVM structure, modifying 2 files only.

## Implementation Details

### Changes Required

#### 1. HistoryViewModel.cs

- **Line 124**: Change `StatusMessage = $"Loaded {HistoryRuns.Count} historical runs";` to only set message when count > 0
- Logic: `if (HistoryRuns.Count > 0) StatusMessage = $"Loaded {HistoryRuns.Count} historical runs"; else StatusMessage = null;`

#### 2. HistoryView.xaml

**Current Issues to Fix:**

| Element | Current | Problem | Fix |
|---------|---------|---------|-----|
| Status Banner (Row 1) | Shows "Loaded 0 historical runs" | Misleading success message | ViewModel already controls; ensure empty = hidden |
| Trend Chart (Row 2) | Uses `TrendDataPoints.Count` converter | Shows empty card | Already has CountToVisibilityConverter - verify it hides on 0 |
| Filter Tabs (Row 3) | Uses `IsHistoryEmpty` inverse binding | Should hide when empty | Already correct |
| Empty State (Row 4) | Small, at bottom, HorizontalAlignment="Center" | Not prominent enough | Make full-height, larger icon, better spacing |

**Empty State Redesign (lines 367-397):**

Current:
- `Padding="48"` - too cramped
- `FontSize="48"` for icon - too small
- `GhostButton` style - not prominent enough

Target:
- Remove `HorizontalAlignment="Center"` and `VerticalAlignment="Center"` from Border
- Use full row height with centered content
- Increase icon to 64px
- Add larger heading text (20px)
- Use PrimaryButton or AccentButton style for CTA
- Better vertical spacing between elements

### Implementation Steps

1. **Update HistoryViewModel.cs**
   - Modify `LoadHistoryAsync()` to only set StatusMessage when HistoryRuns.Count > 0

2. **Update HistoryView.xaml Empty State**
   - Change Border to fill available space
   - Increase icon size from 48 to 64
   - Make heading larger (FontSize="20")
   - Improve description text
   - Change button to PrimaryButton style
   - Better vertical spacing

3. **Verify Trend Chart Visibility**
   - Confirm CountToVisibilityConverter hides card when TrendDataPoints.Count == 0

## Complexity Tracking

No violations - simple UI-only changes following existing patterns.
