# Quickstart: Diagnostics Auto-Load

**Feature**: 017-diagnostics-auto-load
**Branch**: `017-diagnostics-auto-load`

## Overview

Enable the System Diagnostics page to display Machine Information and Network Interfaces immediately on page load, without requiring test execution.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# Dev Kit
- Windows 10/11 (target platform)

## Quick Setup

```bash
# Clone and checkout feature branch
git checkout 017-diagnostics-auto-load

# Restore and build
cd src/ReqChecker.App
dotnet build

# Run the application
dotnet run
```

## Implementation Tasks

### Task 1: Add CurrentMachineInfo Property

**File**: `src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs`

Add observable property:
```csharp
[ObservableProperty]
private MachineInfo? _currentMachineInfo;
```

Add load method:
```csharp
public void LoadMachineInfo()
{
    try
    {
        CurrentMachineInfo = MachineInfoCollector.Collect();
        UpdateCurrentMachineSummary();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to collect machine info");
        CurrentMachineInfo = null;
    }
}
```

### Task 2: Update MachineInfoSummary Logic

Modify `UpdateSummaries()` to compute `MachineInfoSummary` from `CurrentMachineInfo` instead of `LastRunReport.MachineInfo`.

### Task 3: Trigger Load on Page Navigation

**File**: `src/ReqChecker.App/Views/DiagnosticsView.xaml.cs`

```csharp
public DiagnosticsView(DiagnosticsViewModel viewModel)
{
    InitializeComponent();
    DataContext = viewModel;
    Loaded += (s, e) => viewModel.LoadMachineInfo();
}
```

### Task 4: Update XAML Bindings

**File**: `src/ReqChecker.App/Views/DiagnosticsView.xaml`

Change network interfaces binding from:
```xml
ItemsSource="{Binding LastRunReport.MachineInfo.NetworkInterfaces}"
```

To:
```xml
ItemsSource="{Binding CurrentMachineInfo.NetworkInterfaces}"
```

Update count badge similarly.

## Verification

1. Launch application fresh (no prior test runs)
2. Navigate to System Diagnostics page
3. **Expected**: Machine Information section shows hostname, OS, CPU, RAM
4. **Expected**: Network Interfaces section shows all network adapters
5. **Expected**: Last Run Summary shows "No test runs have been performed yet."

## Test Commands

```bash
# Run unit tests
cd tests/ReqChecker.App.Tests
dotnet test

# Run specific test class
dotnet test --filter "DiagnosticsViewModelTests"
```

## Key Files

| File | Purpose |
|------|---------|
| `DiagnosticsViewModel.cs` | Add CurrentMachineInfo property and LoadMachineInfo() method |
| `DiagnosticsView.xaml` | Update bindings to use CurrentMachineInfo |
| `DiagnosticsView.xaml.cs` | Add Loaded event handler |
| `MachineInfoCollector.cs` | No changes (reuse existing) |

## Troubleshooting

**Issue**: Machine info not appearing
- Check that `Loaded` event is wired up in code-behind
- Verify `LoadMachineInfo()` is being called (add breakpoint)
- Check for exceptions in log file

**Issue**: Network interfaces showing empty
- Verify `CurrentMachineInfo.NetworkInterfaces` binding path
- Check that `MachineInfoCollector.Collect()` returns interfaces
