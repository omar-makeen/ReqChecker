# Implementation Plan: Remove Demo Mode UI Control

**Branch**: `007-remove-demo-delay` | **Date**: 2026-01-31 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-remove-demo-delay/spec.md`

## Summary

Remove the "Demo Mode" UI controls (toggle, slider, ms display) from the Run Progress view while keeping the inter-test delay functionality with a fixed 500ms default. The delay value will be defined in a single location (`RunSettings.InterTestDelayMs` default) for easy future modification.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (removing preferences, using hardcoded default)
**Testing**: xUnit 2.5.3
**Target Platform**: Windows (WPF)
**Project Type**: Desktop application (WPF)
**Performance Goals**: N/A (UI simplification only)
**Constraints**: Minimal code changes, single source of truth for delay value
**Scale/Scope**: 4 files modified, ~50 lines removed

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is template (not configured for this project). No gates apply.

## Project Structure

### Documentation (this feature)

```text
specs/007-remove-demo-delay/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output (minimal - straightforward feature)
├── checklists/
│   └── requirements.md  # Spec validation checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (files to modify)

```text
src/
├── ReqChecker.Core/
│   └── Models/
│       └── RunSettings.cs          # Change InterTestDelayMs default: 0 → 500
├── ReqChecker.App/
│   ├── Views/
│   │   └── RunProgressView.xaml    # Remove Demo Mode controls (lines 144-167)
│   ├── ViewModels/
│   │   └── RunProgressViewModel.cs # Simplify: always use 500ms, remove preferences
│   └── Services/
│       ├── IPreferencesService.cs  # Remove TestProgressDelay* properties
│       └── PreferencesService.cs   # Remove TestProgressDelay* properties
```

**Structure Decision**: Existing WPF desktop app structure. No new files needed - this is a removal/simplification task.

## Complexity Tracking

No violations - this is a simplification that removes code rather than adding complexity.

---

## Phase 0: Research

### Decision Log

| Topic | Decision | Rationale |
|-------|----------|-----------|
| Single source of truth | `RunSettings.InterTestDelayMs` default value | Already exists, used by SequentialTestRunner, minimal change |
| Preferences cleanup | Remove `TestProgressDelayEnabled` and `TestProgressDelayMs` entirely | No longer needed - delay is always on at fixed value |
| ViewModel simplification | Always pass 500ms to RunSettings | Remove conditional logic, just use the default |

### Alternatives Considered

1. **Keep delay in preferences but hide UI**: Rejected - adds unnecessary complexity, user can't change it anyway
2. **Add constant class for magic numbers**: Rejected - overkill for single value, RunSettings default is clear enough
3. **Make delay configurable via app.config**: Rejected - over-engineering for current needs

**Output**: No external research needed - straightforward UI removal with existing infrastructure.

---

## Phase 1: Design

### Data Model Changes

**File**: `src/ReqChecker.Core/Models/RunSettings.cs`

```csharp
// BEFORE
public int InterTestDelayMs { get; set; } = 0;

// AFTER
public int InterTestDelayMs { get; set; } = 500;
```

This is the **single source of truth** for the delay value.

### Interface Changes

**File**: `src/ReqChecker.App/Services/IPreferencesService.cs`

Remove:
- `bool TestProgressDelayEnabled { get; set; }`
- `int TestProgressDelayMs { get; set; }`

### Implementation Changes

**File**: `src/ReqChecker.App/Services/PreferencesService.cs`

Remove:
- `TestProgressDelayEnabled` field and property
- `TestProgressDelayMs` field and property
- `OnTestProgressDelayEnabledChanged` partial method
- `OnTestProgressDelayMsChanged` partial method
- Related lines in `UserPreferences` DTO
- Related lines in `Load()` and `Save()` methods

**File**: `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`

Remove:
- `TestProgressDelayEnabled` property
- `TestProgressDelayMs` property

Simplify `StartTestsAsync`:
```csharp
// BEFORE
var runSettings = new RunSettings
{
    InterTestDelayMs = TestProgressDelayEnabled ? TestProgressDelayMs : 0
};

// AFTER
var runSettings = new RunSettings(); // Uses default 500ms
```

### UI Changes

**File**: `src/ReqChecker.App/Views/RunProgressView.xaml`

Remove lines 144-167 (Demo Mode Controls StackPanel):
```xml
<!-- REMOVE THIS ENTIRE BLOCK -->
<!-- Demo Mode Controls -->
<StackPanel Orientation="Horizontal" ...>
    <ui:ToggleSwitch IsChecked="{Binding TestProgressDelayEnabled, Mode=TwoWay}" ... />
    <Slider Value="{Binding TestProgressDelayMs, Mode=TwoWay}" ... />
    <TextBlock Text="{Binding TestProgressDelayMs, StringFormat={}{0} ms}" ... />
</StackPanel>
```

### No Contracts Needed

This feature has no API contracts - it's a UI simplification only.

---

## Implementation Summary

| File | Change Type | Lines Changed |
|------|-------------|---------------|
| `RunSettings.cs` | Modify default | 1 line |
| `IPreferencesService.cs` | Remove properties | ~8 lines |
| `PreferencesService.cs` | Remove properties & logic | ~25 lines |
| `RunProgressViewModel.cs` | Remove properties, simplify | ~15 lines |
| `RunProgressView.xaml` | Remove UI controls | ~24 lines |

**Total**: ~5 files, ~73 lines removed/simplified
