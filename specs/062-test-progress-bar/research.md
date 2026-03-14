# Research: Test Progress Bar Enhancements

**Feature**: 062-test-progress-bar | **Date**: 2026-03-14

## Decision 1: Auto-Navigation Timer Mechanism

**Decision**: Use `System.Windows.Threading.DispatcherTimer` with a 3-second interval

**Rationale**: DispatcherTimer executes its Tick handler on the UI thread, eliminating cross-thread marshaling for navigation calls. It integrates naturally with WPF's threading model and is trivially cancellable via `Stop()`. The existing `OnCompletion()` method already runs on the Dispatcher, making timer creation straightforward.

**Alternatives considered**:
- `Task.Delay` with `CancellationToken`: Would work but requires explicit `Dispatcher.Invoke` for the navigation call. Harder to cancel cleanly if the token source is already disposed from the test run.
- `System.Timers.Timer`: Fires on a thread pool thread, requiring `Dispatcher.Invoke` for all UI operations. More complex for no benefit.
- No timer (immediate navigation): Rejected — users need a moment to see the completion summary.

## Decision 2: Auto-Navigation Delay Duration

**Decision**: 3 seconds

**Rationale**: The spec allows 2–3 seconds. 3 seconds gives users comfortable time to glance at the pass/fail summary without feeling like the app is sluggish. This matches common patterns in CI/CD dashboards and test runners that show brief summaries before transitioning.

**Alternatives considered**:
- 2 seconds: May feel rushed for users scanning the pass/fail/skip breakdown
- 5 seconds: Too long — users would click manually before the timer fires, defeating the purpose
- User-configurable delay: Over-engineered for a minor UX enhancement

## Decision 3: Timer Cancellation Strategy

**Decision**: Stop timer in both `NavigateToTestList()` and `ViewResults()` methods, plus implement `IDisposable` for cleanup

**Rationale**: Covers all cancellation paths: (1) user clicks "Back to Tests" — timer stops, navigates to test list; (2) user clicks "View Results" — timer stops, navigates to results (no double-nav); (3) user navigates away via other means — `IDisposable.Dispose()` stops the timer. The existing `NavigationService.TrackViewModel()` already calls `Dispose()` on IDisposable ViewModels.

**Alternatives considered**:
- Cancel only on button clicks: Misses edge case where user navigates away via sidebar or keyboard shortcut
- Use a boolean guard instead of stopping timer: More fragile — timer still fires and checks the flag

## Decision 4: Position Counter Display Location

**Decision**: Add `TestPositionText` below the progress ring, above the "Currently Running" card

**Rationale**: The progress ring already shows percentage. Adding "Test 3 of 12" directly below it creates a natural visual hierarchy: ring (graphical) → position text (discrete count) → current test card (detail). This doesn't require a new control — just a `TextBlock` with data binding.

**Alternatives considered**:
- Inside the progress ring (replacing percentage): Would lose the percentage display, which is also useful
- In the header subtitle: Already present there as "Running 3 of 12 tests" but it's far from the visual focus area
- Overlay on the progress ring: Cluttered, especially with the percentage already there

## Decision 5: Completion Summary Text Format

**Decision**: Computed property returning "All X tests passed" or "X passed, Y failed, Z skipped"

**Rationale**: A single string property keeps the ViewModel simple. The "all passed" special case provides a satisfying success signal. Individual pass/fail/skip counts with color-coding in XAML reuse existing `StatusPass`/`StatusFail`/`StatusSkip` resources.

**Alternatives considered**:
- Separate properties for each count: Already exist (`CompletedTests`, `FailedTests`, `SkippedTests`). The completion card just needs to display them more prominently — could bind directly instead of a computed summary text.
- Both approaches combined: The completion card can show the computed text AND individual colored counts. The computed text serves as the headline, colored counts as detail.
