# Quickstart: 036-test-config-ux

**Date**: 2026-02-12

## What This Feature Does

Improves the Test Configuration page UI/UX by reducing excessive spacing, simplifying locked field indicators, lightening card decoration, and increasing information density — all without changing any functionality.

## Files to Modify

### Primary Changes
1. **`src/ReqChecker.App/Views/TestConfigView.xaml`** — Main view: reduce margins, padding, label widths, icon sizes, and update "Requires Admin" field
2. **`src/ReqChecker.App/Controls/LockedFieldControl.xaml`** — Simplify from two-column (icon box + value box) to single-row (inline icon + value)
3. **`src/ReqChecker.App/Resources/Styles/Controls.xaml`** — Update ParameterGroupCard style: remove DropShadowEffect, reduce padding, use lighter border

### No Changes Required
- `TestConfigViewModel.cs` — No ViewModel logic changes
- `Spacing.xaml` — Use existing tokens, don't modify system-wide values
- `InputField` style — Shared with TestListView, don't modify

## Key Design Decisions

| Decision | Choice | Why |
|----------|--------|-----|
| Card structure | Keep 3 separate cards | User-confirmed; reduce styling only |
| Card glow | Remove entirely | User flagged as excessive |
| Lock icon | Inline 14px icon + muted text | Removes bulky separate container |
| Label width | 120px (from 140px) | Closes label-to-value gap |
| Field row spacing | 12px (from 16px) | Tighter form density |
| Card padding | 16px (from 20px) | Use existing CardPadding token |
| Card gap | 12px (from 16px) | Use existing CardMargin token |
| Page margin | 24px (from 32px) | More content area |
| Section header icons | 24x24 (from 32x32) | Proportional to heading text |

## Build & Verify

```bash
# Build the project
dotnet build src/ReqChecker.App/ReqChecker.App.csproj

# Run existing tests (ensure no regressions)
dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj

# Launch app and navigate to any test's configuration page to verify visually
dotnet run --project src/ReqChecker.App/ReqChecker.App.csproj
```

## Visual Verification Checklist

- [ ] All 3 sections visible without scrolling on 1080p
- [ ] Locked fields show inline lock icon (no separate bordered container)
- [ ] "Requires Admin" matches other locked field styling
- [ ] No card glow/shadow effects
- [ ] Section header icons are smaller and proportional
- [ ] Labels are closer to their values
- [ ] Save, Cancel, Back buttons still work
- [ ] Entrance animations still play
- [ ] PromptAtRun indicators still display correctly
- [ ] Long parameter labels truncate with tooltip
