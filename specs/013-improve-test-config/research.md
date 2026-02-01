# Research: Improve Test Configuration View

**Feature**: 013-improve-test-config
**Date**: 2026-02-01

## Problem Analysis

### Issue 1: Back Button Not Working

**Root Cause**:
- Back button in `TestConfigView.xaml` line 67 binds to `CancelCommand`
- `CancelCommand` invokes `OnCancel()` method (lines 126-132 in ViewModel)
- `OnCancel()` only resets form values to original `TestDefinition` values
- Missing navigation call to return to Tests list

**Solution**:
- Create new `BackCommand` that calls `NavigationService.GoBack()`
- Inject `NavigationService` into `TestConfigViewModel` constructor
- Update `NavigationService.NavigateToTestConfig()` to pass itself to ViewModel

### Issue 2: Duplicate Fields (Timeout/RetryCount/RequiresAdmin)

**Root Cause**:
- `InitializeParameters()` method (lines 58-80) has hardcoded additions
- Lines 68-79 unconditionally add Timeout, RetryCount, RequiresAdmin as parameters
- These values are already displayed in dedicated sections:
  - Timeout/Retries → Execution Settings section (lines 196-232 in XAML)
  - RequiresAdmin → Basic Information section (lines 144-169 in XAML)

**Solution**:
- Remove lines 68-79 from `InitializeParameters()`
- Parameters collection will only contain actual test-specific parameters from profile

### Issue 3: Empty State Never Shown

**Root Cause**:
- Parameters collection always has at least 3 items (Timeout, RetryCount, RequiresAdmin)
- `CountToVisibilityConverter` in XAML never triggers empty state

**Solution**:
- After removing hardcoded additions, empty state will show correctly
- When `_testDefinition.Parameters` is empty or all are Hidden, the "No parameters defined" message displays

### Issue 4: SaveAsync Has Redundant Logic

**Root Cause**:
- `SaveAsync()` has special handling for Timeout/RetryCount/RequiresAdmin in the foreach loop (lines 101-112)
- This was needed because these values were in Parameters collection

**Solution**:
- Simplify `SaveAsync()` by removing the special case handling
- Timeout/RetryCount are already saved at lines 97-98 from the ViewModel properties
- The foreach loop only needs to handle actual custom parameters

## Code Analysis

### TestConfigViewModel.cs Current Structure

```csharp
// Lines 44-56: Constructor
public TestConfigViewModel(TestDefinition testDefinition)
{
    _testDefinition = testDefinition;
    // Initialize properties from TestDefinition
    InitializeParameters();
    // Create commands
}

// Lines 58-80: InitializeParameters
private void InitializeParameters()
{
    // Lines 60-66: Correct - adds actual parameters from TestDefinition
    // Lines 68-79: BUG - adds hardcoded Timeout/RetryCount/RequiresAdmin
}

// Lines 91-124: SaveAsync
private async Task SaveAsync()
{
    // Lines 96-98: Saves Timeout/RetryCount from ViewModel properties (CORRECT)
    // Lines 99-117: Loops Parameters with special handling (NEEDS CLEANUP)
}

// Lines 126-132: OnCancel
private void OnCancel()
{
    // Only resets values, does not navigate (INCOMPLETE for Back button)
}
```

### NavigationService.cs GoBack Method

```csharp
// Lines 163-169: Already implemented
public void GoBack()
{
    if (_frame != null && _frame.CanGoBack)
    {
        _frame.GoBack();
    }
}
```

## Decision Summary

| Decision | Rationale | Alternatives Rejected |
|----------|-----------|----------------------|
| Create separate BackCommand | Keeps Cancel behavior intact (reset form) | Modifying CancelCommand would lose reset functionality |
| Inject NavigationService | Standard DI pattern, ViewModel stays testable | Using static service locator - less testable |
| Remove hardcoded params | They duplicate existing UI sections | Hiding them with visibility converter - adds complexity |
| Simplify SaveAsync | Less code, clearer intent | Keep special handling - unnecessary complexity |

## References

- `TestConfigViewModel.cs` - lines 44-144
- `TestConfigView.xaml` - lines 65-75 (Back button), 174-235 (Execution Settings), 237-327 (Parameters)
- `NavigationService.cs` - lines 59-76 (NavigateToTestConfig), lines 163-169 (GoBack)
