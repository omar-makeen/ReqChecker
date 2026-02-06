# Research: Premium Results Dashboard

**Feature**: 027-premium-results | **Date**: 2026-02-06

## Decision 1: How to apply FriendlyDateConverter to the Started field

**Decision**: Replace the `<Run Text="{Binding Report.StartTime}"/>` with a separate `<TextBlock>` that uses `Text="{Binding Report.StartTime, Converter={StaticResource FriendlyDateConverter}}"`. WPF `<Run>` elements do not support the `Converter` property on bindings in all contexts, but `TextBlock.Text` does reliably.

**Rationale**: The FriendlyDateConverter is already registered in App.xaml as a StaticResource and accepts `DateTimeOffset` (the type of `RunReport.StartTime`). Switching from inline `<Run>` to a `<TextBlock>` for the value portion allows converter binding while preserving the visual layout with the "Started:" label.

**Alternatives considered**:
- Using a ViewModel-level formatted property: Rejected because it duplicates converter logic and breaks the pattern used on the History page.
- Using `StringFormat` on the Run binding: Rejected because it cannot produce relative dates ("Today at...").

## Decision 2: How to apply DurationFormatConverter to the Duration field

**Decision**: Same approach as Decision 1 — replace `<Run Text="{Binding Report.Duration}"/>` with a `<TextBlock>` using the converter binding.

**Rationale**: Consistent approach with the Started field fix. The DurationFormatConverter accepts `TimeSpan` (the type of `RunReport.Duration`) and is already registered.

**Alternatives considered**:
- Using `StringFormat` with a custom format: Rejected because TimeSpan formatting cannot produce the "Xm Ys" / "Xms" compact style.

## Decision 3: How to handle profile name truncation

**Decision**: Replace the `<Run Text="{Binding Report.ProfileName}"/>` pattern with a `<TextBlock>` that has `TextTrimming="CharacterEllipsis"` and `MaxWidth` constraint, plus `ToolTip="{Binding Report.ProfileName}"` for the full name on hover.

**Rationale**: WPF `<Run>` elements do not support `TextTrimming`. A `<TextBlock>` with `TextTrimming` and a `MaxWidth` is the standard WPF pattern for ellipsis truncation. The summary card metadata column width is constrained by the grid layout, so a `MaxWidth` ensures the text doesn't push beyond available space.

**Alternatives considered**:
- Truncating in the ViewModel: Rejected because it requires hardcoding a character limit and doesn't adapt to window size.
- Using a `TextBox` with IsReadOnly: Rejected — unnecessary complexity.

## Decision 4: How to fix ExpanderCard subtitle duration format

**Decision**: Replace `Subtitle="{Binding Duration, StringFormat='{}{0:N0}ms'}"` with `Subtitle="{Binding Duration, Converter={StaticResource DurationFormatConverter}}"` on the ExpanderCard in ResultsView.xaml.

**Rationale**: The ExpanderCard.Subtitle is a `string` DependencyProperty. The DurationFormatConverter returns a `string`, so it's directly compatible. This makes individual test cards consistent with both the summary card and the History page.

**Alternatives considered**:
- Changing the StringFormat to handle multiple ranges: Rejected because StringFormat cannot conditionally switch between "ms", "s", and "m s" formats.

## Decision 5: Layout approach for refactored metadata section

**Decision**: Restructure the Profile/Started/Duration metadata from inline `<Run>` elements to a label-value grid or stacked `<TextBlock>` pairs. Each line will have a label `<TextBlock>` ("Profile:", "Started:", "Duration:") followed by a value `<TextBlock>` with the converter/truncation applied.

**Rationale**: The current `<Run>` approach is limiting because `<Run>` does not support converters or text trimming. Breaking into separate `<TextBlock>` elements per line preserves the same visual layout while enabling all required formatting features.

**Alternatives considered**:
- Keeping `<Run>` for labels and only changing value elements: This is essentially what we're doing — the label remains inline text while the value becomes a separate element.
