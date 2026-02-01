# Implementation Plan: Improve Test Configuration View

**Branch**: `013-improve-test-config` | **Date**: 2026-02-01 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/013-improve-test-config/spec.md`

## Summary

Fix the Test Configuration view to properly separate concerns between Execution Settings (Timeout, Retries) and Test Parameters (test-specific custom parameters from profile). Also fix the Back button to navigate back to the Tests list instead of just resetting form values.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection
**Storage**: N/A (in-memory TestDefinition objects from profile JSON)
**Testing**: Manual verification (WPF UI bug fix)
**Target Platform**: Windows (win-x64)
**Project Type**: WPF Desktop Application
**Performance Goals**: N/A (bug fix)
**Constraints**: Must maintain existing field policy support
**Scale/Scope**: Two file changes (ViewModel and View)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Constitution not configured | PASS | Project uses placeholder constitution; no gates enforced |

## Project Structure

### Documentation (this feature)

```text
specs/013-improve-test-config/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Root cause analysis and solution design
└── checklists/
    └── requirements.md  # Spec validation checklist
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── ViewModels/
│   │   └── TestConfigViewModel.cs    # FILE TO MODIFY (remove hardcoded params, add back command)
│   ├── Views/
│   │   └── TestConfigView.xaml       # FILE TO MODIFY (wire back button to navigation)
│   └── Services/
│       └── NavigationService.cs      # REFERENCE (has GoBack() method)
```

**Structure Decision**: Existing WPF structure. Modifications to two files in `src/ReqChecker.App/`.

## Complexity Tracking

No violations. This is a bug fix removing incorrect code and wiring existing navigation.

---

## Phase 0: Research

### Root Cause Analysis

**Issue 1: Back Button Not Working**
- **Current behavior**: Back button calls `CancelCommand` which invokes `OnCancel()` method
- **OnCancel() implementation** (lines 126-132): Only resets form values to original TestDefinition values
- **Root cause**: Missing navigation call to return to Tests list
- **Solution**: Create a `BackCommand` that calls `NavigationService.GoBack()`

**Issue 2: Duplicate Timeout/RetryCount/RequiresAdmin**
- **Current behavior**: `InitializeParameters()` method (lines 58-80) adds these values to Parameters collection
- **Root cause**: Lines 68-79 unconditionally add Timeout, RetryCount, RequiresAdmin as parameters
- **Conflict**: These are already shown in Execution Settings (Timeout/Retries) and Basic Information (RequiresAdmin) sections
- **Solution**: Remove lines 68-79 from `InitializeParameters()`

**Issue 3: Empty State Never Shown**
- **Current behavior**: Parameters collection always has at least 3 items
- **Root cause**: Same as Issue 2 - hardcoded parameter additions
- **Solution**: After removing hardcoded additions, empty state will show correctly when `_testDefinition.Parameters` is empty

### Navigation Service Analysis

The `NavigationService` already has a `GoBack()` method (lines 163-169):
```csharp
public void GoBack()
{
    if (_frame != null && _frame.CanGoBack)
    {
        _frame.GoBack();
    }
}
```

The ViewModel needs to:
1. Receive `NavigationService` via constructor injection
2. Create a `BackCommand` that calls `NavigationService.GoBack()`

---

## Phase 1: Implementation Design

### Changes to TestConfigViewModel.cs

**1. Add NavigationService Dependency**
```csharp
private readonly NavigationService _navigationService;

public TestConfigViewModel(TestDefinition testDefinition, NavigationService navigationService)
{
    _testDefinition = testDefinition ?? throw new ArgumentNullException(nameof(testDefinition));
    _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    // ... rest of constructor
}
```

**2. Add BackCommand**
```csharp
public ICommand BackCommand { get; }

// In constructor:
BackCommand = new RelayCommand(OnBack);

// New method:
private void OnBack()
{
    _navigationService.GoBack();
}
```

**3. Remove Hardcoded Parameter Additions**

Delete lines 68-79 in `InitializeParameters()`:
```csharp
// REMOVE THESE LINES:
if (!Parameters.Any(p => p.Name == "Timeout"))
{
    Parameters.Add(new TestParameterViewModel("Timeout", Timeout?.ToString() ?? "30000", FieldPolicyType.Editable));
}
if (!Parameters.Any(p => p.Name == "RetryCount"))
{
    Parameters.Add(new TestParameterViewModel("RetryCount", RetryCount?.ToString() ?? "3", FieldPolicyType.Editable));
}
if (!Parameters.Any(p => p.Name == "RequiresAdmin"))
{
    Parameters.Add(new TestParameterViewModel("RequiresAdmin", RequiresAdmin.ToString().ToLower(), FieldPolicyType.Locked));
}
```

**4. Update SaveAsync to NOT process Timeout/RetryCount from Parameters**

Remove special handling for Timeout/RetryCount/RequiresAdmin in SaveAsync (lines 101-112):
```csharp
// REMOVE THESE CONDITIONALS - they are no longer in Parameters:
// if (param.Name == "Timeout" && int.TryParse(param.Value, out int timeout))
// if (param.Name == "RetryCount" && int.TryParse(param.Value, out int retryCount))
// if (param.Name == "RequiresAdmin" && bool.TryParse(param.Value, out bool requiresAdmin))
```

### Changes to TestConfigView.xaml

**1. Wire Back Button to BackCommand**

Change line 67 from:
```xml
Command="{Binding CancelCommand}"
```
To:
```xml
Command="{Binding BackCommand}"
```

### Changes to NavigationService.cs

**Update NavigateToTestConfig to pass NavigationService to ViewModel**

```csharp
public void NavigateToTestConfig(TestDefinition test)
{
    // ... existing null checks
    var viewModel = new TestConfigViewModel(test, this);  // Pass 'this' as NavigationService
    // ... rest of method
}
```

### Verification Steps

1. Build: `dotnet build src/ReqChecker.App`
2. Run application and navigate to Tests view
3. Click on any test item to open Test Configuration view
4. Verify:
   - Back button returns to Tests list
   - Timeout/Retries appear ONLY in Execution Settings section
   - Test Parameters section shows only custom parameters from profile
   - Tests with no parameters show "No parameters defined" empty state
   - Cancel button still resets form without navigating
   - Save button still persists changes

### Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Navigation stack empty | Low | Low | GoBack() has null check, falls through gracefully |
| Breaking parameter persistence | Low | Medium | SaveAsync still handles Parameters dictionary |
| Breaking field policies | Very Low | Low | Not touching GetFieldPolicy() method |

---

## Artifacts Generated

- [x] `plan.md` - This implementation plan
- [x] `research.md` - Consolidated in this document (root cause analysis complete)
- [ ] `data-model.md` - Not applicable (no data model changes)
- [ ] `contracts/` - Not applicable (no API changes)
- [ ] `quickstart.md` - Not applicable (bug fix with manual verification)

---

## Next Steps

1. Run `/speckit.tasks` to generate implementation tasks
2. Modify `TestConfigViewModel.cs` to add BackCommand and remove hardcoded params
3. Modify `TestConfigView.xaml` to wire Back button to BackCommand
4. Modify `NavigationService.cs` to pass itself to ViewModel
5. Build and verify fix
6. Commit and push changes
