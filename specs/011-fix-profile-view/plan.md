# Implementation Plan: Fix Profile Selector View

**Branch**: `011-fix-profile-view` | **Date**: 2026-01-31 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/011-fix-profile-view/spec.md`

## Summary

Fix a critical WPF binding error that causes the Profile Selector view to crash with cascading error dialogs. The root cause is `{Binding Tests.Count}` using default TwoWay binding mode on a read-only `Count` property. The fix requires adding `Mode=OneWay` to the binding expression.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2
**Storage**: N/A (UI-only fix)
**Testing**: Manual verification (WPF UI testing)
**Target Platform**: Windows (win-x64)
**Project Type**: WPF Desktop Application
**Performance Goals**: N/A (bug fix)
**Constraints**: Must not alter existing visual design or functionality
**Scale/Scope**: Single XAML file change

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Constitution not configured | PASS | Project uses placeholder constitution; no gates enforced |

## Project Structure

### Documentation (this feature)

```text
specs/011-fix-profile-view/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output (minimal - bug fix)
└── checklists/
    └── requirements.md  # Spec validation checklist
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/           # WPF Application (target of fix)
│   ├── Views/
│   │   └── ProfileSelectorView.xaml  # FILE TO MODIFY
│   ├── ViewModels/
│   │   └── ProfileSelectorViewModel.cs
│   └── Controls/
├── ReqChecker.Core/          # Domain models
│   └── Models/
│       └── Profile.cs
└── ReqChecker.Infrastructure/ # Services
```

**Structure Decision**: Existing WPF MVVM structure. Single file modification in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`.

## Complexity Tracking

No violations. This is a minimal single-line fix.

---

## Phase 0: Research

### Bug Analysis

**Decision**: Add `Mode=OneWay` to all `Run.Text` bindings that reference read-only properties.

**Rationale**:
- WPF `Run.Text` uses TwoWay binding by default
- `List<T>.Count` and `ICollection.Count` are read-only properties
- TwoWay binding on read-only properties throws `InvalidOperationException`
- OneWay binding reads from source without attempting to write back

**Alternatives Considered**:
1. ~~Use TextBlock instead of Run~~ - Would break inline text formatting
2. ~~Create ViewModel property~~ - Over-engineering for a simple fix
3. **Use Mode=OneWay** - Correct minimal fix

### Binding Locations to Fix

From `ProfileSelectorView.xaml`:

| Line | Current Binding | Issue |
|------|-----------------|-------|
| 136 | `{Binding Profiles.Count, ...}` | ItemsControl visibility |
| 214 | `{Binding Tests.Count}` | Profile card test count display |
| 229 | `{Binding SchemaVersion}` | May need OneWay if read-only |
| 250 | `{Binding Profiles.Count, ...}` | Empty state visibility |

**Primary Fix**: Line 214 - `{Binding Tests.Count}` causes the cascade error
**Secondary Fixes**: Lines 136, 250 use converters but should also be OneWay for safety

---

## Phase 1: Implementation Design

### Change Specification

**File**: `src/ReqChecker.App/Views/ProfileSelectorView.xaml`

**Changes**:

1. **Line 214** - Add `Mode=OneWay` to test count binding:
   ```xml
   <!-- Before -->
   <Run Text="{Binding Tests.Count}"/>

   <!-- After -->
   <Run Text="{Binding Tests.Count, Mode=OneWay}"/>
   ```

2. **Line 229** - Add `Mode=OneWay` to schema version binding (preventive):
   ```xml
   <!-- Before -->
   <Run Text="{Binding SchemaVersion}"/>

   <!-- After -->
   <Run Text="{Binding SchemaVersion, Mode=OneWay}"/>
   ```

### Verification Steps

1. Build: `dotnet build src/ReqChecker.App`
2. Run application
3. Click "Profiles" in navigation
4. Verify:
   - No error dialogs appear
   - Profile cards display correctly with test counts
   - Empty state shows when no profiles exist
   - Import and refresh functionality works
   - Visual styling matches app aesthetic

### Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Fix incomplete | Low | High | Check all Run bindings in file |
| Breaks existing behavior | Very Low | Medium | OneWay is read-only, cannot break writes |
| Visual regression | Very Low | Low | No visual changes, just binding mode |

---

## Artifacts Generated

- [x] `plan.md` - This implementation plan
- [x] `research.md` - Consolidated in this document (minimal research needed)
- [ ] `data-model.md` - Not applicable (no data model changes)
- [ ] `contracts/` - Not applicable (no API changes)
- [ ] `quickstart.md` - Not applicable (simple bug fix)

---

## Next Steps

1. Run `/speckit.tasks` to generate implementation tasks
2. Execute fix in `ProfileSelectorView.xaml`
3. Verify fix with manual testing
4. Commit and push changes
