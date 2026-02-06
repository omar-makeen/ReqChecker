# Research: Premium History Cards

## R1: Friendly Relative Date Formatting in WPF

**Decision**: Create a new `FriendlyDateConverter` (IValueConverter) that accepts `DateTimeOffset` and returns a formatted string.

**Rationale**: WPF StringFormat in XAML binding cannot express conditional logic ("Today" vs "Yesterday" vs full date). A custom converter is the standard WPF pattern for conditional formatting. The converter compares the input date's `.LocalDateTime.Date` against `DateTime.Today` and `DateTime.Today.AddDays(-1)` to determine which format to use.

**Format rules**:
- Same calendar day → `"Today at h:mm tt"` (e.g., "Today at 2:07 PM")
- Previous calendar day → `"Yesterday at h:mm tt"` (e.g., "Yesterday at 9:30 AM")
- Older → `"MMM d, yyyy 'at' h:mm tt"` (e.g., "Feb 3, 2026 at 12:01 AM")

**Alternatives considered**:
- Humanizer library: Overkill for 3 format branches; adds unnecessary dependency
- ViewModel computed property: Violates separation of concerns; converter is reusable

## R2: Duration Format Enhancement

**Decision**: Modify the existing `DurationFormatConverter` to handle durations ≥ 60 seconds with "Xm Ys" format.

**Rationale**: Current converter only handles `<1s` (ms) and `≥1s` (seconds with decimal). FR-004 requires "2m 15s" for durations at or above 60 seconds. Adding a third branch to the existing converter is the simplest approach.

**Updated format rules**:
- `< 1s` → `"Xms"` (e.g., "350ms") — unchanged
- `≥ 1s and < 60s` → `"X.Xs"` (e.g., "4.6s") — unchanged
- `≥ 60s` → `"Xm Ys"` (e.g., "2m 15s")

**Alternatives considered**:
- New converter class: Unnecessary; the existing class already handles TimeSpan formatting
- TimeSpan.ToString custom format: Doesn't easily express "Xm Ys" shorthand

## R3: Pass Rate Color-Coded Badge

**Decision**: Create a new `PassRateToBrushConverter` that returns a SolidColorBrush based on pass rate thresholds.

**Rationale**: The pass rate badge needs a background color that varies by value range. WPF DataTriggers on numeric ranges are verbose and hard to maintain. A converter is cleaner and keeps the thresholds in one place.

**Threshold rules**:
- `≥ 80%` → Green (use `StatusPass` resource: `#10b981`)
- `50% – 79%` → Amber (use `StatusSkip` resource: `#f59e0b`)
- `< 50%` → Red (use `StatusFail` resource: `#ef4444`)
- Special: `0 total tests` → Neutral (use `TextTertiary` resource)

**Implementation note**: The converter will return semi-transparent versions of the status colors for the badge background (e.g., 20% opacity) to maintain the premium muted aesthetic, with the full-opacity color for the text. This can be achieved using a `MultiValueConverter` or by the XAML binding the text foreground to one converter and the background to the same converter with a parameter.

**Simpler approach**: Use a single converter with a `parameter` string to distinguish between "Background" and "Foreground" return modes. Background returns 20% opacity brush; Foreground returns full opacity brush.

**Alternatives considered**:
- DataTriggers in XAML: Requires 3+ triggers with threshold comparisons — verbose and duplicated for background vs foreground
- ViewModel property: PassRate already exists on RunSummary; adding brush logic to the model mixes concerns

## R4: Status Indicator Text Labels

**Decision**: Add text labels ("Passed", "Failed", "Skipped") directly in the XAML template next to the existing colored dot and count.

**Rationale**: This is a pure XAML change — no converter needed. The current template already binds `Summary.Passed`, `Summary.Failed`, `Summary.Skipped`. We simply change the text format from `"{0}"` to `"{0} Passed"` etc. using StringFormat or by adding a static TextBlock with the label word.

**Alternatives considered**:
- Converter for status label: Unnecessary; the label is static text per indicator ("Passed", "Failed", "Skipped")
- Tooltip-only labels: Doesn't meet FR-009 accessibility requirement (must be visible, not hidden)

## R5: Theme Compatibility

**Decision**: Use existing DynamicResource tokens for all new colors and brushes.

**Rationale**: The app already has `StatusPass`, `StatusFail`, `StatusSkip`, `TextPrimary`, `TextSecondary`, `TextTertiary`, `BackgroundSurface` defined in both `Colors.Dark.xaml` and `Colors.Light.xaml`. New converters should reference these resources rather than hardcoding hex values. For the pass rate badge, use semi-transparent overlays of the status colors.

**No new theme tokens needed**: All required colors already exist in the design system.
