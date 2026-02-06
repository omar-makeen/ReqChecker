# Quickstart: 027-premium-results

## What This Feature Does

Fixes raw machine-format display of dates, durations, and long profile names on the Results Dashboard page by applying the same converters already used on the History page.

## Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/Views/ResultsView.xaml` | Apply FriendlyDateConverter to Started, DurationFormatConverter to Duration, add TextTrimming+tooltip to ProfileName, fix ExpanderCard subtitle |

## No New Files

All changes are XAML-only in a single file. No new converters, models, services, or view models needed.

## Build & Verify

```bash
dotnet build src/ReqChecker.App
```

Then run the app, execute tests, and check the Results Dashboard for friendly formatting.
