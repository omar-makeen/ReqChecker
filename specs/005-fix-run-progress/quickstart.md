# Quickstart: Testing Run Progress View Fixes

**Branch**: 005-fix-run-progress

## Prerequisites

1. Build the application: `dotnet build`
2. Have a valid `startup-profile.json` in the app directory with 4 tests (use sample-diagnostics.json)
3. Or launch normally and select the "Sample Diagnostics" profile

## Test Scenarios

### Scenario 1: Progress Ring at 100%

**Steps**:
1. Launch app with a profile containing 4 tests
2. Click "Run All Tests"
3. Wait for all tests to complete
4. Observe the progress ring

**Expected**:
- Progress ring shows "100%" text
- Progress ring arc is fully complete (no gap, no dot at top)
- Ring forms a complete circle

**Before Fix**: Small gap/dot visible at top of ring
**After Fix**: Ring is visually complete

---

### Scenario 2: "Currently Running" State on Completion

**Steps**:
1. Run all tests
2. Wait for completion
3. Observe the "Currently Running" card

**Expected**:
- When complete, "Currently Running" section either:
  - Hides entirely, OR
  - Shows "Complete" or summary text
- Spinning animation stops
- No test name displayed with spinner

**Before Fix**: Shows last test name "System File Check" with spinning icon
**After Fix**: Section shows completion state or hides

---

### Scenario 3: Completed Tests List

**Steps**:
1. Run all tests
2. Observe the "Completed Tests" section on the right

**Expected**:
- Header shows "Completed Tests" with count badge showing "4"
- List displays 4 test result items
- Each item shows test name, duration, and Pass/Fail badge
- "Waiting for results..." message is NOT visible

**Before Fix**: Empty state "Waiting for results..." shown despite count being 4
**After Fix**: All 4 test results visible in list

---

### Scenario 4: Cancel Button Responsiveness

**Steps**:
1. Start a test run
2. Immediately click Cancel
3. Observe UI behavior

**Expected** (during run):
- Cancel button is enabled
- Clicking Cancel stops test execution
- UI shows "Cancelled" or similar state

**Expected** (after completion):
- Cancel button is disabled or hidden
- Navigation options visible (e.g., "Back to Tests", "View Results")

**Before Fix**: Cancel button appears to do nothing
**After Fix**: Cancel stops execution, button disabled when not running

---

### Scenario 5: State Synchronization

**Steps**:
1. Run all tests
2. At completion, verify ALL of these simultaneously:
   - Progress ring: 100%
   - Stats: Total = Passed + Failed + Skipped
   - Results list: Shows all tests
   - Current test: No longer shows a running test

**Expected**:
- All UI elements reflect consistent "complete" state
- No contradictory information displayed

**Before Fix**: Mixed states - 100% but "Currently Running", count 4 but empty list
**After Fix**: All elements synchronized

---

### Scenario 6: Single Test Profile

**Steps**:
1. Create/use a profile with only 1 test
2. Run the test
3. Verify all UI states

**Expected**:
- Progress shows 0% → 100% (no intermediate)
- Results list shows 1 item
- State synchronization works correctly

---

### Scenario 7: All Tests Skipped

**Steps**:
1. Create a profile with tests that will be skipped (e.g., require admin on non-admin session)
2. Run tests
3. Verify UI states

**Expected**:
- Progress reaches 100%
- Skipped count increments correctly
- Results list shows skipped tests with appropriate status

---

## Validation Checklist

After implementing fixes, verify:

- [ ] Progress ring shows complete circle at 100%
- [ ] "Currently Running" hides or shows completion state
- [ ] Results list displays all completed tests
- [ ] Cancel button disabled when not running
- [ ] All UI elements show consistent state
- [ ] No "Waiting for results..." when tests have completed
- [ ] Works with 1 test, 4 tests, and all-skipped scenarios
