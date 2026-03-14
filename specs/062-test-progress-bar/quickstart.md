# Quickstart: Test Progress Bar Enhancements

**Feature**: 062-test-progress-bar | **Date**: 2026-03-14

## Manual Test Scenarios

### Scenario 1: Sequential position counter shows during execution
1. Launch app, select a profile with multiple tests (e.g., 5+ tests)
2. Click "Run All Tests"
3. Watch the progress ring area
4. **Expected**: "Test 1 of N" counter is visible, incrementing after each test completes

### Scenario 2: Position counter reflects selected subset
1. Select 3 of 10 tests via checkboxes
2. Click "Run 3 of 10 Tests"
3. **Expected**: Counter shows "Test 1 of 3", "Test 2 of 3", "Test 3 of 3" — not "of 10"

### Scenario 3: Auto-navigation to results after completion
1. Run all tests
2. Wait for all tests to complete
3. **Expected**: Completion summary shows for ~3 seconds, then automatically navigates to the results page

### Scenario 4: Manual "View Results" cancels auto-navigation
1. Run all tests, wait for completion
2. Immediately click "View Results" before the 3-second timer expires
3. **Expected**: Navigates to results immediately — no double-navigation or flicker

### Scenario 5: "Back to Tests" cancels auto-navigation
1. Run all tests, wait for completion
2. Click "Back to Tests" before the 3-second timer expires
3. **Expected**: Navigates to test list — auto-navigation does NOT fire afterward

### Scenario 6: No auto-navigation on cancelled run
1. Run all tests
2. Click "Cancel" partway through
3. Wait 5+ seconds after cancellation
4. **Expected**: Stays on the progress page — no auto-navigation occurs

### Scenario 7: Completion summary shows all-passed message
1. Run a suite where all tests pass
2. **Expected**: Completion card shows "All X tests passed" with success styling

### Scenario 8: Completion summary shows pass/fail/skip breakdown
1. Run a suite with a mix of pass, fail, and skip outcomes
2. **Expected**: Completion card shows "X passed, Y failed, Z skipped" with color-coded counts

### Scenario 9: Single test run
1. Select and run only 1 test
2. **Expected**: Counter shows "Test 1 of 1", completion summary shows result, auto-navigation works

### Scenario 10: All tests skipped
1. Run a suite where all tests are skipped (e.g., dependency failures)
2. **Expected**: Completion card shows "0 passed, 0 failed, X skipped", auto-navigation still fires

## Key Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` | Add `TestPositionText`, `CompletionSummaryText`, auto-nav timer, `IDisposable` |
| `src/ReqChecker.App/Views/RunProgressView.xaml` | Add position counter TextBlock, update completion card binding |
| `tests/ReqChecker.App.Tests/ViewModels/RunProgressViewModelTests.cs` | New test file |
