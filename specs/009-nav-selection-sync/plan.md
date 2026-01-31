# Implementation Plan: Navigation Selection State Synchronization

**Branch**: `009-nav-selection-sync` | **Date**: 2026-01-31 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/009-nav-selection-sync/spec.md`

## Summary

Fix three navigation-related bugs in ReqChecker: (1) empty Results view shows blank screen instead of informative message, (2) multiple sidebar items appear selected simultaneously, and (3) programmatic navigation doesn't update sidebar selection. The solution involves centralizing sidebar selection management and ensuring the empty state visibility binding works correctly.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection
**Storage**: N/A (UI-only fix, state is in-memory via IAppState)
**Testing**: Manual UI testing (existing test infrastructure)
**Target Platform**: Windows 10/11 desktop
**Project Type**: Single WPF desktop application
**Performance Goals**: Sidebar selection update within 100ms of navigation
**Constraints**: Must maintain existing navigation animations; no breaking changes to NavigationService API
**Scale/Scope**: 4 navigation items (Profiles, Tests, Results, Diagnostics)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The constitution template has no specific gates defined. This is a bug fix feature with no architectural changes, no new projects, and no new dependencies. Proceeding with implementation planning.

- [x] No new libraries/projects being added
- [x] Fixes existing functionality only
- [x] No external integrations
- [x] Maintains existing patterns

## Project Structure

### Documentation (this feature)

```text
specs/009-nav-selection-sync/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (N/A - no data model changes)
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/ReqChecker.App/
├── MainWindow.xaml              # Navigation sidebar XAML (NavItems with IsActive)
├── MainWindow.xaml.cs           # Navigation click handlers, sidebar selection logic
├── Views/
│   └── ResultsView.xaml         # Empty state visibility bindings
├── ViewModels/
│   └── ResultsViewModel.cs      # Report property, empty state logic
└── Services/
    └── NavigationService.cs     # Navigation methods (no changes needed)
```

**Structure Decision**: Existing single-project WPF application structure. Changes limited to MainWindow (navigation management) and ResultsView (empty state display).

## Complexity Tracking

No violations to justify. This is a straightforward bug fix with no architectural complexity.

---

## Phase 0: Research Findings

### Issue 1: Empty Results State Not Displaying

**Analysis:**
- ResultsView.xaml has empty state UI at lines 267-296
- Visibility bound to `{Binding Report, Converter={StaticResource NullToVisibilityConverter}}`
- The `NullToVisibilityConverter` should show the empty state when `Report` is null

**Root Cause Investigation:**
- Need to verify if `NullToVisibilityConverter` is correctly registered and implemented
- Need to check if main content area is hiding when Report is null (competing visibility)

**Decision**: Investigate the converter implementation and ensure content/empty state visibility are mutually exclusive.

### Issue 2: Multiple Sidebar Items Appear Selected

**Analysis:**
- WPF-UI NavigationViewItem uses `IsActive` property for selection state
- Current code sets `IsActive = true` on clicked item but **never deselects other items**
- Initial state: `NavTests` has `IsActive="True"` in XAML
- When clicking "Results", both Tests and Results appear selected

**Root Cause**: Missing logic to deselect all items before setting new selection.

**Decision**: Add `ClearNavigationSelection()` helper method that sets `IsActive = false` on all nav items before setting the new selection.

### Issue 3: Programmatic Navigation Doesn't Update Sidebar

**Analysis:**
- `NavigateWithAnimation()` only sets `NavResults.IsActive = true` for Results
- Missing `IsActive` updates for: Tests, Profiles, Diagnostics
- ViewModels call `NavigationService.NavigateToX()` directly without sidebar update

**Root Cause**: Sidebar selection is scattered and incomplete. Only Results has partial implementation.

**Decision**: Centralize sidebar selection in `NavigateWithAnimation()` with complete switch case handling for all navigation targets.

### Research Summary

| Problem | Root Cause | Solution |
|---------|-----------|----------|
| Empty state not showing | Visibility binding conflict or converter issue | Verify converter; ensure mutually exclusive visibility |
| Multiple items selected | Never deselecting previous selection | Add `ClearNavigationSelection()` before setting new |
| Programmatic nav out of sync | Incomplete IsActive assignments | Complete switch statement with all nav items |

---

## Phase 1: Design

### Component Changes

#### 1. MainWindow.xaml.cs - Navigation Selection Management

**Add Helper Method:**
```csharp
private void ClearNavigationSelection()
{
    NavProfiles.IsActive = false;
    NavTests.IsActive = false;
    NavResults.IsActive = false;
    NavDiagnostics.IsActive = false;
}

private void SetNavigationSelection(string tag)
{
    ClearNavigationSelection();
    switch (tag)
    {
        case "Profiles": NavProfiles.IsActive = true; break;
        case "Tests": NavTests.IsActive = true; break;
        case "Results": NavResults.IsActive = true; break;
        case "Diagnostics": NavDiagnostics.IsActive = true; break;
    }
}
```

**Update NavigateWithAnimation:**
- Replace inline `NavResults.IsActive = true` with `SetNavigationSelection(tag)`
- Call before navigation switch statement (immediate visual feedback)

**Update OnWindowLoaded:**
- Replace direct `NavTests.IsActive = true` with `SetNavigationSelection("Tests")`
- Replace direct `NavProfiles.IsActive = true` with `SetNavigationSelection("Profiles")`

#### 2. ResultsView.xaml - Empty State Visibility

**Verify/Fix Visibility Logic:**
- Main results content should have `Visibility="{Binding HasReport, Converter={StaticResource BoolToVisibilityConverter}}"`
- Empty state should have `Visibility="{Binding HasNoReport, Converter={StaticResource BoolToVisibilityConverter}}"`
- Or use single property with inverted converters

**Current binding issue investigation points:**
- Check if `NullToVisibilityConverter` returns Visible for null (should) or Collapsed
- Check if results grid overlays the empty state

#### 3. ResultsViewModel.cs - Support Properties (if needed)

If the converter isn't working correctly, add explicit boolean properties:
```csharp
public bool HasReport => Report != null;
public bool HasNoReport => Report == null;
```

And notify on `Report` property change.

### File Change Summary

| File | Change Type | Description |
|------|-------------|-------------|
| MainWindow.xaml.cs | Modify | Add selection helpers, update navigation logic |
| MainWindow.xaml | Modify | Remove initial `IsActive="True"` (handled in code) |
| ResultsView.xaml | Modify | Fix empty state visibility bindings |
| ResultsViewModel.cs | Modify | Add HasReport/HasNoReport if needed |

### Contracts

N/A - This is a UI-only bug fix with no API contracts.

### Data Model

N/A - No data model changes. Uses existing `IAppState.LastRunReport` property.

---

## Implementation Sequence

1. **Fix sidebar multi-selection** (Issue 2)
   - Add `ClearNavigationSelection()` and `SetNavigationSelection()` methods
   - Update `NavigateWithAnimation()` to use helper
   - Update `OnWindowLoaded()` to use helper
   - Remove initial `IsActive="True"` from XAML

2. **Fix programmatic navigation sync** (Issue 3)
   - Ensure `SetNavigationSelection(tag)` is called for ALL navigation paths
   - Test "View Results" button updates sidebar
   - Test "Back to Tests" button updates sidebar

3. **Fix empty state display** (Issue 1)
   - Investigate current visibility bindings in ResultsView.xaml
   - Verify `NullToVisibilityConverter` implementation
   - Ensure content and empty state have mutually exclusive visibility
   - Add `HasReport`/`HasNoReport` properties if converter approach fails

4. **Testing**
   - Launch app without running tests, click Results → See empty state message
   - Click through all nav items → Only one selected at a time
   - Run tests, click "View Results" → Results selected in sidebar
   - Click "Back to Tests" → Tests selected in sidebar
