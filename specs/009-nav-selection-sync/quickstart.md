# Quickstart: Navigation Selection State Synchronization

**Branch**: `009-nav-selection-sync`

## Overview

Fix three navigation bugs: empty state not showing, multiple items selected, and programmatic navigation not updating sidebar.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- WPF-UI 4.2.0 (already in project)

## Quick Setup

```bash
# Clone and checkout
git checkout 009-nav-selection-sync

# Build
dotnet build src/ReqChecker.App

# Run
dotnet run --project src/ReqChecker.App
```

## Files to Modify

| File | Purpose |
|------|---------|
| `src/ReqChecker.App/MainWindow.xaml.cs` | Add selection helper methods, update navigation logic |
| `src/ReqChecker.App/MainWindow.xaml` | Remove hardcoded `IsActive="True"` |
| `src/ReqChecker.App/Views/ResultsView.xaml` | Fix empty state visibility bindings |
| `src/ReqChecker.App/ViewModels/ResultsViewModel.cs` | Add `HasReport` property if needed |

## Implementation Steps

### Step 1: Add Selection Helpers (MainWindow.xaml.cs)

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

### Step 2: Update NavigateWithAnimation

Call `SetNavigationSelection(tag)` at the start of the method, before the navigation switch statement.

### Step 3: Update OnWindowLoaded

Replace:
```csharp
NavTests.IsActive = true;
// and
NavProfiles.IsActive = true;
```

With:
```csharp
SetNavigationSelection("Tests");
// and
SetNavigationSelection("Profiles");
```

### Step 4: Remove XAML Default

In MainWindow.xaml, remove `IsActive="True"` from NavTests.

### Step 5: Fix ResultsView Empty State

Ensure the empty state visibility binding works correctly. If needed, add `HasReport` property to ViewModel.

## Testing Checklist

- [ ] Launch app, click Results without running tests → See "No test results available"
- [ ] Click Profiles → Only Profiles selected
- [ ] Click Tests → Only Tests selected
- [ ] Click Results → Only Results selected
- [ ] Click Diagnostics → Only Diagnostics selected
- [ ] Run tests, click "View Results" button → Results view opens, Results selected in sidebar
- [ ] Click "Back to Tests" button → Tests view opens, Tests selected in sidebar

## Common Issues

**Build fails**: Close running app instance first (file lock on exe)

**Selection not updating**: Check that `SetNavigationSelection` is called before navigation, not after

**Empty state still blank**: Verify ResultsView has mutually exclusive visibility between content and empty state
