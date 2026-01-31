# Quickstart: Display Test Result Details

**Feature**: 010-result-details | **Date**: 2026-01-31

## Overview

This feature displays test result details in the expanded test card. Currently, clicking a card expands it but shows no content because `HumanSummary` and `TechnicalDetails` fields are not populated by test implementations.

## Solution Summary

1. Create converters to generate summary/details text from `TestResult.Evidence`
2. Update `ExpanderCard.xaml` with premium styling (accent left borders)
3. Add metadata section (duration, retry count)
4. Update `ResultsView.xaml` to use new converters

## Files to Modify

| File | Change Type | Description |
|------|-------------|-------------|
| `src/ReqChecker.App/Converters/TestResultSummaryConverter.cs` | NEW | Generate summary from TestResult |
| `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs` | NEW | Generate technical details from Evidence |
| `src/ReqChecker.App/Controls/ExpanderCard.xaml` | UPDATE | Premium styling with left borders |
| `src/ReqChecker.App/Controls/ExpanderCard.xaml.cs` | UPDATE | Add Duration, AttemptCount properties |
| `src/ReqChecker.App/Views/ResultsView.xaml` | UPDATE | Use converters, bind metadata |
| `tests/ReqChecker.App.Tests/Converters/TestResultSummaryConverterTests.cs` | NEW | Unit tests |
| `tests/ReqChecker.App.Tests/Converters/TestResultDetailsConverterTests.cs` | NEW | Unit tests |

## Build & Test

```bash
# Build
dotnet build src/ReqChecker.App

# Run tests
dotnet test tests/ReqChecker.App.Tests

# Run application
dotnet run --project src/ReqChecker.App
```

## Verification Steps

1. Run the application
2. Load a test profile with various test types
3. Run tests
4. Navigate to Results view
5. Click on a test card to expand it
6. Verify:
   - Summary section shows human-readable description
   - Technical details show formatted Evidence data (if available)
   - Error section shows error message for failed tests (styled red)
   - Duration is displayed
   - Retry count shows for tests with multiple attempts
   - Premium styling with accent left borders is visible

## Implementation Order

1. **Converters** (can be developed in parallel)
   - `TestResultSummaryConverter` with tests
   - `TestResultDetailsConverter` with tests

2. **ExpanderCard Updates**
   - Add new DependencyProperties
   - Update XAML styling

3. **ResultsView Integration**
   - Register converters in resources
   - Update bindings

## Key Code Patterns

### Converter Pattern
```csharp
public class TestResultSummaryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TestResult result)
            return null;

        return GenerateSummary(result);
    }

    private static string GenerateSummary(TestResult result)
    {
        // Generate based on Status and Evidence
    }
}
```

### XAML Binding Pattern
```xml
<controls:ExpanderCard
    Summary="{Binding Converter={StaticResource TestResultSummaryConverter}}"
    TechnicalDetails="{Binding Converter={StaticResource TestResultDetailsConverter}}"
    ... />
```

### Premium Section Styling
```xml
<Border Background="{DynamicResource BackgroundBase}"
        CornerRadius="4"
        Padding="12"
        Margin="0,12,0,0">
    <Grid>
        <Border Width="4"
                HorizontalAlignment="Left"
                Margin="-12,0,0,0"
                Background="{DynamicResource AccentGradient}"/>
        <TextBlock Text="{Binding TechnicalDetails}"
                   Margin="4,0,0,0" .../>
    </Grid>
</Border>
```
