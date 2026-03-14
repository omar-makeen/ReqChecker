# Quickstart: Unsaved Changes Warning

**Feature**: 061-unsaved-changes-warning | **Date**: 2026-03-14

## Manual Test Scenarios

### Scenario 1: Dialog appears on unsaved changes
1. Launch app, select a profile, open any test's configuration
2. Change the Timeout value (e.g., from 5000 to 10000)
3. Click the Back button
4. **Expected**: Confirmation dialog appears with "Discard Changes" and "Stay" options

### Scenario 2: No dialog when no changes
1. Open any test's configuration
2. Do NOT change any values
3. Click the Back button
4. **Expected**: Navigates back immediately, no dialog

### Scenario 3: Edit-then-revert (no dialog)
1. Open any test's configuration
2. Change the Timeout value (e.g., 5000 → 10000)
3. Change it back to the original value (10000 → 5000)
4. Click the Back button
5. **Expected**: Navigates back immediately, no dialog (value-based comparison)

### Scenario 4: Discard changes
1. Open any test's configuration, change Timeout
2. Click Back → dialog appears
3. Click "Discard Changes"
4. **Expected**: Navigates back to test list, changes are lost

### Scenario 5: Stay on page
1. Open any test's configuration, change Timeout
2. Click Back → dialog appears
3. Click "Stay" (or press Escape)
4. **Expected**: Dialog closes, remains on config page with changes intact

### Scenario 6: Save then navigate (no dialog)
1. Open any test's configuration, change Timeout
2. Click "Save Changes"
3. Click the Back button
4. **Expected**: Navigates back immediately, no dialog

### Scenario 7: Save then edit more (dialog appears)
1. Open any test's configuration, change Timeout
2. Click "Save Changes"
3. Change the Retry Count
4. Click Back
5. **Expected**: Dialog appears (new unsaved changes since last save)

### Scenario 8: Parameter changes trigger dialog
1. Open a test with editable parameters (e.g., an HttpGet test with URL parameter)
2. Change a parameter value
3. Click Back
4. **Expected**: Dialog appears

### Scenario 9: Password parameter changes trigger dialog
1. Open a test with a password parameter (field name ending in "Password")
2. Type a new password value
3. Click Back
4. **Expected**: Dialog appears

### Scenario 10: Keyboard accessibility
1. Open a test, make a change, click Back → dialog appears
2. Press Escape
3. **Expected**: Dialog closes (Stay behavior)
4. Click Back again → dialog appears
5. Press Enter (with Discard focused) or Tab to select option + Enter
6. **Expected**: Navigates back

## Key Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs` | Add baseline snapshot, `HasUnsavedChanges`, modify `BackCommand` |
| `src/ReqChecker.App/Services/DialogService.cs` | Add `ShowConfirmationDialog()` method |
| `tests/ReqChecker.App.Tests/ViewModels/TestConfigViewModelTests.cs` | New test file for dirty tracking |
