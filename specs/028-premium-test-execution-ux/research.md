# Research: 028-premium-test-execution-ux

## Decision 1: Progress Ring 0% Arc Rendering Bug

**Decision**: Hide the arc Path element using `Visibility.Collapsed` (not `Visibility.Hidden`) when progress is 0, ensuring no arc geometry is rendered or laid out.

**Rationale**: The current code at `ProgressRing.xaml.cs:177-181` uses `Visibility.Hidden`, which hides the arc visually but still reserves layout space. More importantly, the `UpdateVisualState()` method (line 229-232) unconditionally sets `_progressArc.Visibility = Visibility.Visible` when not in indeterminate mode — this can override the `Hidden` set by `UpdateArc()`, causing a race condition where the arc appears briefly at 0%. The fix is to use `Collapsed` in `UpdateArc` and also guard against `UpdateVisualState` overriding it by checking the progress value.

**Alternatives considered**:
- Setting the arc segment to a degenerate point (0-length arc) — rejected because it still creates a visible dot at the start point due to round line caps.
- Hiding via Opacity=0 — rejected because it still renders and consumes GPU resources.

## Decision 2: "Preparing..." State Implementation

**Decision**: Set `CurrentTestName` to `"Preparing..."` at the start of `StartTestsAsync()` (in the ViewModel) before calling `RunTestsAsync()`. This uses the existing "Currently Running" card and its spinner animation without adding new UI elements.

**Rationale**: The current flow is:
1. `OnLoaded` → `StartTestsAsync()` begins
2. `IsRunning = true`, `CurrentTestName = null`
3. Test runner begins — for the first test, there's no delay (delay only applies between tests, `i > 0` check at `SequentialTestRunner.cs:114`)
4. However, `PromptForCredentialsIfNeededAsync` and `RetryPolicy.ExecuteWithRetryAsync` take measurable time
5. During this gap, the card shows `FallbackValue='Waiting...'` — which is confusing

By setting `CurrentTestName = "Preparing..."` at the start of `StartTestsAsync()`, the user sees a clear status immediately. The first `OnTestCompleted` callback will then overwrite it with the next test's name.

Additionally, `CurrentTestName` should be set to the FIRST test's name (index 0) when execution starts, so users see both "Preparing..." and then the actual test name. The `OnTestCompleted` already updates to the next test (index `CurrentTestIndex` after increment), so we just need the initial value.

Revised approach: Set `CurrentTestName = "Preparing..."` initially, then in the test runner loop, the ViewModel can set it to the first test name when the first progress callback fires (existing behavior handles subsequent tests).

**Alternatives considered**:
- Adding a new `IsPreparing` property with a separate UI card — rejected as over-engineering for a simple label change.
- Adding a pre-execution delay to simulate preparation — rejected because it artificially slows execution.

## Decision 3: Layout Shift Fix

**Decision**: Replace the `StackPanel` with `VerticalAlignment="Center"` (at `RunProgressView.xaml:156`) with a `Grid` using fixed row heights for the card area, ensuring the progress ring position is independent of which card is currently visible.

**Rationale**: The root cause is that the left column uses a `StackPanel` with `VerticalAlignment="Center"`. When the "Currently Running" card (smaller: ~80px tall with 20px spinner icon) is swapped for the "Completion Summary" card (taller: ~140px tall with 48px checkmark icon), the StackPanel's total content height increases by ~60px. Since it's centered, the entire stack shifts upward by ~30px.

The fix uses a Grid with explicit rows:
- Row 0 (Auto): Progress ring (fixed 160px)
- Row 1 (Fixed height): Card area — both cards occupy this same row, so the row height never changes
- Row 2 (Auto): Stats summary

Both the "Currently Running" card and the "Completion Summary" card will be sized to match the taller card's height (using MinHeight), so the Grid row height remains constant regardless of which card is visible.

**Alternatives considered**:
- Making both cards the same height by adding padding to the smaller card — workable but fragile if card content changes.
- Using a fixed-height container around both cards — this IS the chosen approach, implemented via Grid row.
- Animating the transition to hide the shift — rejected because it masks the problem rather than fixing it.

## Decision 4: Premium Visual Consistency

**Decision**: Normalize the completion summary card to use the same visual weight as the "Currently Running" card: smaller checkmark icon (24px instead of 48px), consistent padding, and matching card height. This ensures state transitions feel stable.

**Rationale**: The current completion card uses a 48px checkmark icon and TextH3 heading, making it significantly taller than the running card. Reducing the icon to 24px and using TextBody for the heading brings both cards to similar heights and feels more professional — premium design is about restraint, not size.

**Alternatives considered**:
- Keep the 48px icon and enlarge the running card to match — rejected because the running card would feel unnecessarily bloated.
- Use animation to morph between cards — over-engineering for this context.
