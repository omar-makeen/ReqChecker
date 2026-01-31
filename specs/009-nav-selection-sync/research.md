# Research: Navigation Selection State Synchronization

**Branch**: `009-nav-selection-sync`
**Date**: 2026-01-31

## Overview

Research findings for fixing three navigation-related bugs in ReqChecker WPF application.

---

## Issue 1: Empty Results State Not Displaying

### Current Implementation

**ResultsView.xaml (lines 267-296):**
- Empty state UI exists with message "No test results available"
- Visibility bound to `{Binding Report, Converter={StaticResource NullToVisibilityConverter}}`

**ResultsViewModel.cs:**
- `Report` property is set by NavigationService when navigating to Results view
- `Report` is null when no tests have been run (IAppState.LastRunReport is null)

### Investigation Points

1. **NullToVisibilityConverter** - Verify it returns `Visible` when value is `null` (for empty state)
2. **Content area visibility** - Check if main results grid has competing visibility that overlays empty state
3. **Grid.Row placement** - Both empty state and content may be in same row, causing overlap

### Decision

- **Solution**: Add explicit `HasReport` / `HasNoReport` boolean properties to ViewModel
- **Rationale**: More explicit than relying on null-to-visibility converter behavior
- **Alternatives Considered**:
  - Fix converter (but adds coupling to converter implementation details)
  - Use DataTriggers (adds XAML complexity)

---

## Issue 2: Multiple Sidebar Items Appear Selected

### Current Implementation

**MainWindow.xaml (line 84):**
```xaml
<ui:NavigationViewItem x:Name="NavTests" ... IsActive="True" ...>
```
- NavTests has hardcoded `IsActive="True"` in XAML

**MainWindow.xaml.cs (NavigateWithAnimation, line 149):**
```csharp
case "Results":
    _navigationService.NavigateToResults();
    NavResults.IsActive = true;  // Only Results sets IsActive!
    break;
```
- Only Results case sets `IsActive = true`
- **Never clears previous selection**

### Root Cause

When user clicks "Results":
1. `NavItem_Click` fires
2. `NavigateWithAnimation("Results")` called
3. `NavResults.IsActive = true` is set
4. `NavTests.IsActive` remains `true` (from XAML default)
5. **Both items appear selected**

### Decision

- **Solution**: Add `ClearNavigationSelection()` helper that sets all nav items to `IsActive = false`
- **Rationale**: Centralized, explicit clearing prevents any item from being "forgotten"
- **Alternatives Considered**:
  - Track "previous selection" and clear only that (error-prone if state gets out of sync)
  - Use WPF-UI's built-in selection (requires TargetPageType which conflicts with custom NavigationService)

---

## Issue 3: Programmatic Navigation Doesn't Update Sidebar

### Current Implementation

**NavigateWithAnimation switch statement:**
```csharp
case "Profiles":
    _navigationService.NavigateToProfileSelector();
    break;  // ❌ No IsActive update
case "Tests":
    _navigationService.NavigateToTestList();
    break;  // ❌ No IsActive update
case "Results":
    _navigationService.NavigateToResults();
    NavResults.IsActive = true;  // ✅ Only one with update
    break;
case "Diagnostics":
    _navigationService.NavigateToDiagnostics();
    break;  // ❌ No IsActive update
```

**ViewModel navigation calls:**
- `ResultsViewModel.NavigateToTestList()` - Calls NavigationService directly
- `RunProgressViewModel.ViewResults()` - Calls NavigationService directly
- These bypass sidebar selection updates entirely

### Root Cause

1. Sidebar selection updates are incomplete (only Results implemented)
2. ViewModel-initiated navigation bypasses MainWindow entirely
3. No callback mechanism from NavigationService to update UI

### Decision

- **Solution**: Call `SetNavigationSelection(tag)` at the start of `NavigateWithAnimation()` for all cases
- **Rationale**: Single location handles all sidebar updates; tag-based approach is consistent
- **Alternatives Considered**:
  - Event system from NavigationService (over-engineering for this bug fix)
  - Pass callback to ViewModels (tight coupling, complex)

---

## Technology Patterns

### WPF-UI NavigationViewItem

- `IsActive` property controls visual selection state
- Multiple items can have `IsActive = true` simultaneously (no built-in mutual exclusion)
- For custom navigation (not using TargetPageType), must manually manage IsActive

### Best Practice: Centralized Selection Management

```csharp
private void SetNavigationSelection(string tag)
{
    // Clear all first (mutual exclusion)
    NavProfiles.IsActive = false;
    NavTests.IsActive = false;
    NavResults.IsActive = false;
    NavDiagnostics.IsActive = false;

    // Set the active one
    switch (tag)
    {
        case "Profiles": NavProfiles.IsActive = true; break;
        case "Tests": NavTests.IsActive = true; break;
        case "Results": NavResults.IsActive = true; break;
        case "Diagnostics": NavDiagnostics.IsActive = true; break;
    }
}
```

---

## Summary

| Issue | Root Cause | Solution | Effort |
|-------|-----------|----------|--------|
| Empty state not showing | Visibility binding/overlap issue | Add explicit HasReport properties | Low |
| Multiple items selected | No clearing of previous selection | Add ClearNavigationSelection() | Low |
| Programmatic nav out of sync | Incomplete IsActive assignments | Complete SetNavigationSelection() | Low |

All issues are low effort and can be fixed in a single implementation session.
