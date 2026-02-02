# Research: Improve Test History Empty State UI/UX

**Branch**: `024-history-empty-state` | **Date**: 2026-02-03

## Research Summary

This is a UI-only fix with no technical unknowns. The research focused on understanding the existing implementation and identifying the specific changes needed.

## Findings

### 1. Current Implementation Analysis

**HistoryView.xaml Structure:**
- Row 0: Header (always visible)
- Row 1: Status Message (bound to `StatusMessage` via `NullToVisibilityConverter`)
- Row 2: Trend Chart (bound to `TrendDataPoints.Count` via `CountToVisibilityConverter`)
- Row 3: Filter Tabs (bound to `IsHistoryEmpty` inverse)
- Row 4: History List / Empty State (both in same row, mutually exclusive via `IsHistoryEmpty`)

**Key Bindings:**
- `StatusMessage` - shown via `NullToVisibilityConverter` (null = hidden)
- `TrendDataPoints.Count` - shown via `CountToVisibilityConverter` with `ConverterParameter=Inverse`
- `IsHistoryEmpty` - boolean property that returns `HistoryRuns.Count == 0`

### 2. Root Cause: Status Message Always Set

In `HistoryViewModel.LoadHistoryAsync()` (line 124):
```csharp
StatusMessage = $"Loaded {HistoryRuns.Count} historical runs";
```

This always sets a status message, even when count is 0. The `NullToVisibilityConverter` shows the banner because StatusMessage is not null.

**Fix**: Only set StatusMessage when count > 0.

### 3. Trend Chart Visibility Issue

The trend chart uses:
```xml
Visibility="{Binding TrendDataPoints.Count, Converter={StaticResource CountToVisibilityConverter}, ConverterParameter=Inverse, FallbackValue=Collapsed}"
```

Checking `CountToVisibilityConverter`:
- With `ConverterParameter=Inverse`, it should show when count > 0, hide when count = 0
- The FallbackValue=Collapsed is correct

**Issue**: The converter seems inverted. Need to verify the logic. If count=0, `Inverse` would make it Visible (wrong). Should be without Inverse parameter.

### 4. Empty State Design Guidelines

Based on existing app patterns (from other views):
- Use full available space
- Large, centered icon (64px+)
- Clear heading text (20px, SemiBold)
- Supporting description (14px, secondary color)
- Prominent action button (PrimaryButton style)

### 5. Decision: Fix Approach

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| Fix ViewModel StatusMessage logic | Simple, targeted fix | Could add new binding property - more complex |
| Redesign empty state in XAML | Follows app patterns | Could create reusable EmptyState control - overkill for single use |
| Verify/fix trend chart visibility | May need converter parameter adjustment | N/A |

## No Clarifications Needed

All requirements are clear from the specification and screenshot analysis. The implementation is straightforward UI modifications.
