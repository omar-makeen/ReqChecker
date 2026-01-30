# Quickstart: Test Progress Delay Implementation

**Feature**: 006-test-progress-delay
**Date**: 2026-01-31

## Overview

This guide walks through implementing the test progress delay feature step by step.

## Prerequisites

- .NET 8.0 SDK installed
- ReqChecker solution builds successfully (`dotnet build`)
- Understanding of MVVM pattern with CommunityToolkit.Mvvm

## Implementation Steps

### Step 1: Extend RunSettings Model

**File**: `src/ReqChecker.Core/Models/RunSettings.cs`

Add new property for inter-test delay:

```csharp
/// <summary>
/// Delay in milliseconds between test completions. 0 = no delay.
/// </summary>
public int InterTestDelayMs { get; set; } = 0;
```

### Step 2: Modify SequentialTestRunner

**File**: `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs`

After `progress?.Report(testResult)`, add delay logic:

```csharp
// Report result to UI
progress?.Report(testResult);

// Apply inter-test delay (skip after last test)
if (runSettings.InterTestDelayMs > 0 && i < tests.Count - 1)
{
    await Task.Delay(runSettings.InterTestDelayMs, cancellationToken);
}
```

**Key Points**:
- Check `i < tests.Count - 1` to skip delay after final test
- Pass `cancellationToken` for immediate cancellation support

### Step 3: Extend UserPreferences

**File**: `src/ReqChecker.App/Services/PreferencesService.cs`

Add new fields to `UserPreferences` class:

```csharp
public bool TestProgressDelayEnabled { get; set; } = true;
public int TestProgressDelayMs { get; set; } = 500;
```

### Step 4: Extend PreferencesService

**File**: `src/ReqChecker.App/Services/PreferencesService.cs`

Add observable properties:

```csharp
[ObservableProperty]
private bool _testProgressDelayEnabled = true;

[ObservableProperty]
private int _testProgressDelayMs = 500;

partial void OnTestProgressDelayEnabledChanged(bool value) => Save();
partial void OnTestProgressDelayMsChanged(int value) => Save();
```

Update `Load()` method:

```csharp
TestProgressDelayEnabled = prefs.TestProgressDelayEnabled;
TestProgressDelayMs = Math.Clamp(prefs.TestProgressDelayMs, 0, 3000);
```

Update `Save()` method:

```csharp
TestProgressDelayEnabled = TestProgressDelayEnabled,
TestProgressDelayMs = TestProgressDelayMs,
```

### Step 5: Extend RunProgressViewModel

**File**: `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`

Add properties that bind to PreferencesService:

```csharp
public bool TestProgressDelayEnabled
{
    get => _preferencesService.TestProgressDelayEnabled;
    set => _preferencesService.TestProgressDelayEnabled = value;
}

public int TestProgressDelayMs
{
    get => _preferencesService.TestProgressDelayMs;
    set => _preferencesService.TestProgressDelayMs = value;
}
```

Modify `StartTestsAsync()` to pass delay to RunSettings:

```csharp
var runSettings = new RunSettings
{
    // ... existing settings ...
    InterTestDelayMs = TestProgressDelayEnabled ? TestProgressDelayMs : 0
};
```

### Step 6: Update RunProgressView

**File**: `src/ReqChecker.App/Views/RunProgressView.xaml`

Add delay controls (below progress ring, above stats):

```xaml
<!-- Demo Mode Controls -->
<StackPanel Orientation="Horizontal"
            HorizontalAlignment="Center"
            Margin="0,16,0,0">
    <ui:ToggleSwitch IsChecked="{Binding TestProgressDelayEnabled, Mode=TwoWay}"
                     OnContent="Demo Mode"
                     OffContent="Demo Mode" />
    <Slider Value="{Binding TestProgressDelayMs, Mode=TwoWay}"
            Minimum="0"
            Maximum="3000"
            Width="120"
            Margin="16,0,8,0"
            IsEnabled="{Binding TestProgressDelayEnabled}"
            TickFrequency="500"
            IsSnapToTickEnabled="False" />
    <TextBlock Text="{Binding TestProgressDelayMs, StringFormat={}{0} ms}"
               VerticalAlignment="Center"
               Width="50" />
</StackPanel>
```

## Verification

### Manual Test Cases

1. **Default State**
   - Launch app, navigate to Run Progress
   - Verify Demo Mode toggle is ON, slider shows 500ms

2. **Observable Execution**
   - Run a profile with 4+ tests
   - Verify each test pauses ~500ms before next

3. **Disable Delay**
   - Turn Demo Mode OFF
   - Run tests - verify no artificial delay

4. **Adjust Duration**
   - Set slider to 1500ms
   - Run tests - verify longer pause

5. **Cancellation**
   - Start tests with delay enabled
   - Click Cancel during a delay pause
   - Verify immediate cancellation (<200ms)

6. **Persistence**
   - Adjust delay settings
   - Close and reopen app
   - Verify settings preserved

### Build Verification

```bash
dotnet build src/ReqChecker.App
dotnet test tests/ReqChecker.Infrastructure.Tests
```

## Troubleshooting

### Delay Not Working

- Verify `InterTestDelayMs > 0` is passed to runner
- Check `TestProgressDelayEnabled` is true
- Confirm delay logic is after `progress?.Report()`

### Settings Not Persisting

- Check `%APPDATA%\ReqChecker\preferences.json` exists
- Verify JSON includes `testProgressDelayEnabled` and `testProgressDelayMs`
- Ensure `Save()` is called in partial methods

### Cancellation Slow

- Ensure `cancellationToken` is passed to `Task.Delay()`
- Verify not using `Thread.Sleep()` anywhere

## Files Modified

| File | Lines Changed (est.) |
|------|---------------------|
| `RunSettings.cs` | +3 |
| `SequentialTestRunner.cs` | +5 |
| `PreferencesService.cs` | +15 |
| `RunProgressViewModel.cs` | +15 |
| `RunProgressView.xaml` | +15 |
| **Total** | ~53 lines |
