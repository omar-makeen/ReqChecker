# Quickstart: Premium History Cards

## Prerequisites

- .NET 8.0 SDK (Windows)
- Visual Studio 2022 or `dotnet` CLI

## Build & Run

```bash
cd src/ReqChecker.App
dotnet build
dotnet run
```

## Verify Changes

1. Launch the app
2. Load a test profile (e.g., Sample Diagnostics)
3. Run the test suite at least once
4. Navigate to **Test History** in the sidebar
5. Verify the history card displays:
   - **Date**: "Today at [time] PM/AM" (not "Feb 06, 2026 12:07")
   - **Duration**: Timer icon + "Duration: 4.6s" (not bare "4.6s")
   - **Pass Rate**: Labeled badge "Pass Rate: 75%" with color (green/amber/red)
   - **Status indicators**: "3 Passed", "1 Failed", "0 Skipped" with colored dots (not bare "3", "1", "0")
6. Toggle to **Light Mode** and verify all labels remain readable

## Files Changed

| File | Change |
|------|--------|
| `src/ReqChecker.App/Converters/FriendlyDateConverter.cs` | New converter for relative date display |
| `src/ReqChecker.App/Converters/PassRateToBrushConverter.cs` | New converter for pass rate color badge |
| `src/ReqChecker.App/Converters/DurationFormatConverter.cs` | Updated to handle ≥60s durations |
| `src/ReqChecker.App/Views/HistoryView.xaml` | Updated card item template |
| `src/ReqChecker.App/App.xaml` | Registered new converters |
