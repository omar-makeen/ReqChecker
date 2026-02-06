# Data Model: Premium History Cards

## Overview

No data model changes required. This feature is UI-only and uses existing entities.

## Existing Entities (read-only usage)

### RunReport
- `StartTime: DateTimeOffset` — Used by FriendlyDateConverter for relative date display
- `Duration: TimeSpan` — Used by DurationFormatConverter for labeled duration display

### RunSummary
- `PassRate: double` — Used by PassRateToBrushConverter for color-coded badge (0–100 scale)
- `Passed: int` — Displayed as "[count] Passed" with green indicator
- `Failed: int` — Displayed as "[count] Failed" with red indicator
- `Skipped: int` — Displayed as "[count] Skipped" with amber indicator
- `TotalTests: int` — Used to detect 0-test edge case for pass rate display

## New Converters (presentation layer only)

### FriendlyDateConverter
- **Input**: `DateTimeOffset`
- **Output**: `string`
- **Logic**: Today → "Today at h:mm tt" | Yesterday → "Yesterday at h:mm tt" | Older → "MMM d, yyyy at h:mm tt"

### PassRateToBrushConverter
- **Input**: `double` (pass rate 0–100)
- **Parameter**: `string` ("Background" or "Foreground")
- **Output**: `SolidColorBrush`
- **Logic**: ≥80% → green | 50–79% → amber | <50% → red | Background mode returns 20% opacity variant

### DurationFormatConverter (modified)
- **Input**: `TimeSpan`
- **Output**: `string`
- **Logic**: <1s → "Xms" | 1–59s → "X.Xs" | ≥60s → "Xm Ys"
