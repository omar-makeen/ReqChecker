# Implementation Plan: Test Progress Bar Enhancements

**Branch**: `062-test-progress-bar` | **Date**: 2026-03-14 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/062-test-progress-bar/spec.md`

## Summary

Enhance the existing RunProgress page with three incremental improvements: (1) add a "Test X of Y" sequential position counter near the progress ring, (2) auto-navigate to results after a 3-second delay on completion, and (3) enhance the completion summary card with pass/fail/skip breakdown and a distinct all-passed message.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2
**Storage**: N/A (in-memory session-only; no persistence changes)
**Testing**: xUnit + Moq (existing test infrastructure)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Desktop application
**Performance Goals**: Progress updates render in real-time (<100ms per update)
**Constraints**: Auto-navigation timer must be cancellable; no changes to test execution logic
**Scale/Scope**: Single page enhancement (RunProgressView + RunProgressViewModel)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is template-only (no project-specific gates defined). No violations to evaluate. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/062-test-progress-bar/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── ViewModels/
│   │   └── RunProgressViewModel.cs    # Add TestPositionText, auto-nav timer, CompletionSummaryText
│   └── Views/
│       └── RunProgressView.xaml       # Add position counter, update completion card
tests/
└── ReqChecker.App.Tests/
    └── ViewModels/
        └── RunProgressViewModelTests.cs  # New: position counter, auto-nav, completion summary tests
```

**Structure Decision**: All changes fit within existing files. No new projects, services, or architectural changes needed.

## Key Design Decisions

### 1. Sequential Position Counter: Computed Property

Add a `TestPositionText` computed property to `RunProgressViewModel` that returns `"Test {CurrentTestIndex + 1} of {TotalTests}"` during execution. The `HeaderSubtitle` already shows similar info in the header — this new property is for display near the progress ring.

**Why**: Reuses existing `CurrentTestIndex` and `TotalTests` properties. No new state tracking needed — just a formatted string. Trigger `OnPropertyChanged` in existing `OnCurrentTestIndexChanged` partial method.

### 2. Auto-Navigation Timer: DispatcherTimer

After `OnCompletion()` sets `IsComplete = true`, start a `DispatcherTimer` with a 3-second interval. On tick, call `ViewResults()` and stop the timer. Cancel the timer if user clicks "Back to Tests" or "View Results" manually.

**Why**: `DispatcherTimer` runs on the UI thread (no cross-thread issues), integrates naturally with WPF, and is easily cancellable via `Stop()`. A 3-second delay provides enough time to glance at the summary.

### 3. Timer Cancellation on Manual Navigation

Both `NavigateToTestList()` and `ViewResults()` commands stop the timer before navigating. The timer is also stopped/disposed when the ViewModel is cleaned up (via `NavigationService.TrackViewModel` which disposes IDisposable ViewModels).

**Why**: Prevents double-navigation or unexpected navigation after the user has already acted. Implementing `IDisposable` ensures cleanup if the user navigates away via other means.

### 4. No Auto-Nav on Cancellation

The timer only starts in `OnCompletion()` when `IsCancelling` is false. If the run was cancelled, the user stays on the completion view to decide their next action.

**Why**: Cancelled runs may have partial results — the user should consciously choose whether to view results or go back.

### 5. Enhanced Completion Card: Computed Summary Text

Add `CompletionSummaryText` computed property that returns either "All X tests passed" (when `FailedTests == 0 && SkippedTests == 0`) or "X passed, Y failed, Z skipped" with appropriate formatting. The XAML completion card binds to this property.

**Why**: Simple string formatting in the ViewModel, displayed in the existing completion card. Color-coding is handled via existing `StatusPass`/`StatusFail`/`StatusSkip` dynamic resources in XAML.

### 6. IDisposable for Timer Cleanup

Implement `IDisposable` on `RunProgressViewModel` to stop and dispose the auto-navigation timer. The existing `NavigationService.TrackViewModel()` already disposes IDisposable ViewModels on navigation.

**Why**: Follows the pattern established by `ProfileSelectorViewModel` (feature 060). Prevents timer from firing after the ViewModel is no longer active.

## Complexity Tracking

No constitution violations to justify. Feature is a straightforward ViewModel/View enhancement with no new projects, patterns, or dependencies.
