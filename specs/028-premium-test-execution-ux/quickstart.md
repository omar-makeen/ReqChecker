# Quickstart: 028-premium-test-execution-ux

## Files to Modify

1. **`src/ReqChecker.App/Controls/ProgressRing.xaml.cs`** — Fix 0% arc rendering bug in `UpdateArc()` and guard in `UpdateVisualState()`
2. **`src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`** — Add "Preparing..." initial state for `CurrentTestName`
3. **`src/ReqChecker.App/Views/RunProgressView.xaml`** — Fix layout shift by replacing StackPanel with Grid; normalize card heights

## No New Files

This is a UI-only bug fix and polish feature. No new files, models, services, or contracts are needed.

## Verification

Run the app, load a profile with 4 tests, and click "Run Tests":
1. Progress ring should show clean empty track at 0% (no arc artifact)
2. Status card should immediately show "Preparing..." before first test runs
3. Progress ring should NOT shift position when transitioning to completion
4. All elements should feel visually consistent across states
