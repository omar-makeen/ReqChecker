# Implementation Plan: Fix Run Progress View UI Bugs

**Branch**: `005-fix-run-progress` | **Date**: 2026-01-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-fix-run-progress/spec.md`

## Summary

Fix multiple UI synchronization bugs in the Run Progress view where the progress ring, "Currently Running" indicator, completion statistics, and results list show contradictory states. The Cancel button is also unresponsive. This is a bug fix focused on proper WPF data binding and UI thread synchronization.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection
**Storage**: N/A (UI-only fix)
**Testing**: Manual testing, xUnit for ViewModel unit tests
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop application (WPF MVVM)
**Performance Goals**: UI updates within 500ms of test completion, Cancel responds within 2 seconds
**Constraints**: Must maintain existing test runner infrastructure, fix UI layer only
**Scale/Scope**: Single view (RunProgressView), single ViewModel (RunProgressViewModel)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| MVVM Pattern | ✅ Pass | Using CommunityToolkit.Mvvm with ObservableObject |
| UI Thread Safety | ⚠️ Fix Needed | Dispatcher.Invoke usage needs verification |
| Data Binding | ⚠️ Fix Needed | ObservableCollection not updating UI properly |
| Single Responsibility | ✅ Pass | ViewModel handles state, View handles display |

**Gate Status**: PASS with fixes required - this is a bug fix plan, violations are the bugs being fixed.

## Project Structure

### Documentation (this feature)

```text
specs/005-fix-run-progress/
├── plan.md              # This file
├── research.md          # Bug root cause analysis
├── data-model.md        # N/A for UI bug fix
├── quickstart.md        # Testing scenarios
└── tasks.md             # Phase 2 output
```

### Source Code (affected files)

```text
src/ReqChecker.App/
├── ViewModels/
│   └── RunProgressViewModel.cs    # PRIMARY - Fix state management
├── Views/
│   ├── RunProgressView.xaml       # Fix bindings and visibility triggers
│   └── RunProgressView.xaml.cs    # Verify Loaded event handler
└── Controls/
    └── ProgressRing.xaml          # Check 100% visual rendering

tests/ReqChecker.App.Tests/
└── ViewModels/
    └── RunProgressViewModelTests.cs  # Add/fix unit tests
```

**Structure Decision**: Existing WPF MVVM structure. Bug fix targets App layer only - no Core or Infrastructure changes needed.

## Complexity Tracking

No constitution violations requiring justification - this is a targeted bug fix.

## Root Cause Analysis

Based on the screenshot and code review:

### Bug 1: Progress Ring Visual Gap at 100%
- **Symptom**: Small dot/gap visible at top of ring when showing 100%
- **Likely Cause**: ProgressRing control rendering edge case at boundary value
- **Fix Area**: `Controls/ProgressRing.xaml` or ProgressRing code-behind

### Bug 2: "Currently Running" Shows Stale Test Name
- **Symptom**: Shows "System File Check" when all tests completed
- **Likely Cause**: `CurrentTestName` property not cleared when execution completes
- **Fix Area**: `RunProgressViewModel.cs` - clear state in completion path

### Bug 3: Results List Shows "Waiting for results..."
- **Symptom**: Empty state visible despite count showing 4
- **Likely Cause**: `TestResults.Count` binding works but ItemsControl not refreshing
- **Fix Area**: `RunProgressView.xaml` visibility binding logic or `ObservableCollection` thread issue

### Bug 4: Cancel Button Not Responding
- **Symptom**: Click has no effect
- **Likely Cause**: Command binding issue or `CancelCommand` not properly implemented
- **Fix Area**: `RunProgressViewModel.cs` - verify CancelCommand implementation

### Bug 5: State Synchronization
- **Symptom**: Multiple UI elements show contradictory states
- **Likely Cause**: Properties updated from background thread without proper marshalling
- **Fix Area**: All Dispatcher.Invoke calls, ensure atomic state updates

## Design Decisions

### D1: State Management Approach
**Decision**: Use single `IsRunning` property as source of truth for UI state
**Rationale**: Prevents state divergence between progress ring, current test, and action buttons
**Alternative Rejected**: Multiple independent state flags - harder to synchronize

### D2: UI Update Strategy
**Decision**: All property updates via Dispatcher.Invoke with batch updates
**Rationale**: Ensures thread safety and atomic UI updates
**Alternative Rejected**: Individual BeginInvoke calls - can cause visual inconsistency

### D3: Completion State Display
**Decision**: Hide "Currently Running" section when `IsComplete=true`, show completion summary instead
**Rationale**: Clear visual distinction between running and completed states
**Alternative Rejected**: Show "Complete" as test name - confusing semantically

### D4: Cancel Button Behavior
**Decision**: Disable (not hide) Cancel button when not running, show navigation options
**Rationale**: Consistent button location, clear affordance for next actions
**Alternative Rejected**: Replace Cancel with different button - jarring layout shift
