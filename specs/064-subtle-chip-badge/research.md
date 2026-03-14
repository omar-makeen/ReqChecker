# Research: Subtle Chip Badge

**Feature**: 064-subtle-chip-badge
**Date**: 2026-03-14

## No NEEDS CLARIFICATION Items

All technical context is known from the existing codebase and CLAUDE.md. No unknowns to resolve.

## Existing Theme Resources

**Decision**: Use existing WPF-UI theme resources for the muted chip style.
**Rationale**: The app already defines surface/border/text theme resources used across all views. Reusing them ensures automatic dark/light theme support without custom brushes.
**Alternatives considered**: Custom brush resources — rejected because existing resources already provide the needed palette.

### Available Resources (confirmed in codebase)

| Resource | Purpose | Used for |
|----------|---------|----------|
| `BackgroundSurface` | Muted surface color | Badge background |
| `BorderSubtle` | Subtle border | Badge outline |
| `TextSecondary` | Secondary text color | Badge text |
| `TextTertiary` | Tertiary text color | Alternative if TextSecondary too prominent |

## Current Badge Implementations

### Page Header Badge (`TestListView.xaml` lines 112-120)

- Background: `AccentPrimary` (bright accent — problem)
- Text: White, SemiBold
- Padding: 16,8
- CornerRadius: 16
- Content: `TestCountDisplay` (e.g., "8 tests")

### Sidebar Badge (`MainWindow.xaml` lines 93-108)

- Background: `AccentPrimaryBrush` (bright accent — problem)
- Text: `TextOnAccentFillColorPrimaryBrush`, SemiBold, 10px
- Padding: 6,2
- CornerRadius: 8
- Content: `TestCount` (number only)

## Design Decision: Chip Style Properties

**Decision**: Apply these properties to both badges:
- Background: `BackgroundSurface`
- Border: 1px `BorderSubtle`
- Text color: `TextSecondary`
- Font weight: Regular (Normal)
- Padding: Reduced from current values

**Rationale**: This matches the "Option A: Subtle chip style" proposed in the UX review and aligns with how informational tags/chips are styled in modern Fluent Design.
