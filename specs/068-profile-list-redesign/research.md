# Phase 0 Research — Profile Manager List Redesign

**Branch**: `068-profile-list-redesign`
**Date**: 2026-04-30
**Purpose**: Resolve all open questions before design. Every decision below is grounded in code that already exists in the repository, so this feature can adopt established patterns rather than invent new ones.

---

## R1 — Canonical row pattern

**Decision**: Adopt `HistoryView`'s virtualized `ListBox` pattern verbatim for the Profile Manager list.

**Why**:
- It is the most premium and most accessible list pattern already in the codebase.
- It already satisfies the spec requirements: virtualization (FR-020), full-width rows (FR-001/003), single click target on row (FR-005), keyboard arrow navigation (FR-008 — comes free from `ListBox`), focus visuals (FR-019), and list semantics for AT (FR-019a — exposes `AutomationProperties.Name` on the list).
- Replicating it produces the consistency users will perceive (SC-001).

**Reference (verbatim from `src/ReqChecker.App/Views/HistoryView.xaml:209–245`)**:
```xml
<ListBox ItemsSource="{Binding FilteredHistory}"
         Background="Transparent" BorderThickness="0"
         ScrollViewer.HorizontalScrollBarVisibility="Disabled"
         ScrollViewer.VerticalScrollBarVisibility="Auto"
         VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling"
         VirtualizingPanel.ScrollUnit="Pixel"
         SelectionMode="Single"
         SelectedItem="{Binding SelectedRun, Mode=TwoWay}"
         AutomationProperties.Name="History runs list">
  <ListBox.ItemContainerStyle>
    <Style TargetType="ListBoxItem">
      <Setter Property="Padding" Value="0"/>
      <Setter Property="Margin"  Value="0,0,0,12"/>
      <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
      <Setter Property="Background"     Value="Transparent"/>
      <Setter Property="BorderThickness" Value="0"/>
      <Setter Property="FocusVisualStyle" Value="{StaticResource FocusVisualStyle}"/>
      <Setter Property="Template">…ContentPresenter only…</Setter>
    </Style>
  </ListBox.ItemContainerStyle>
  <ListBox.ItemsPanel><ItemsPanelTemplate><VirtualizingStackPanel/></ItemsPanelTemplate></ListBox.ItemsPanel>
  …
</ListBox>
```

**Alternatives considered**:
- *Keep `ItemsControl` + a vertical `StackPanel`*: rejected — no virtualization (fails FR-020 / SC-006 at 50 profiles) and no built-in selection/keyboard semantics (would require hand-rolled handlers).
- *Use `DataGrid`*: rejected — overkill for a non-tabular layout; also visually inconsistent with the rest of the app.

---

## R2 — Selected/active state visuals

**Decision**: Bind the `ListBox.SelectedItem` to the row VM whose underlying profile equals `IAppState.CurrentProfile`. The row's container Border swaps `Card` ↔ `CardSelected` via a `DataTrigger` on `IsSelected="{Binding IsSelected, RelativeSource=…ListBoxItem}"` (or simply on the `ListBoxItem.IsSelected` of the parent). The keyboard focus ring remains separate (`FocusVisualStyle`) so all three states (hover/focus/selected) are visually distinct (FR-009).

**Why**:
- `CardSelected` already exists in `Resources/Styles/Controls.xaml:485` with the exact treatment we want (accent border + accent glow). We do not need to invent a new "selected row" token.
- `IAppState.CurrentProfileChanged` (`src/ReqChecker.App/Services/AppState.cs:26`) is the existing source of truth for which profile is active across the app. Subscribing on the page VM and recomputing `SelectedItem` on event satisfies FR-009a without coupling.

**Alternatives considered**:
- *Independent `IsActive` flag on each row VM driving the visual via DataTrigger, ignoring `ListBox.SelectedItem`*: rejected — doing both fragments the source of truth and breaks keyboard arrow-key navigation (`ListBox` already maintains its own selection).
- *Persist selection across navigation in `IPreferencesService`*: rejected — out of scope and unnecessary; selection is derived from `IAppState.CurrentProfile`, which is itself in-memory and refreshed on every entry.

---

## R3 — Recommended-profile signal: keep one, remove two

**Decision**: Keep only the `RecommendedBadge` (text "Recommended" pill) inside the row. Remove the accent border (currently `BorderThickness="2"` + `BorderBrush="AccentPrimary"`) and the 6 px gradient header strip from the recommended state. Every row uses identical baseline chrome (`Card` style); only the badge differs.

**Why**:
- FR-015 is explicit: exactly one design element MUST communicate "Recommended."
- FR-016 is explicit: when no profile is recommended, every row must share identical baseline styling — meaning the gradient strip, today applied to *every* card, is itself a category violation if it remains.
- The badge alone is also stronger: a labeled chip is the most accessible signal (it has actual text, surfaceable to AT via FR-019b).

**Alternatives considered**:
- *Keep gradient strip but make it match across all rows for "calm decoration"*: rejected — adds visual noise without information; FR-016 favors quietness.
- *Replace badge with a star icon*: rejected — icon-only signals are less accessible and lose the "Recommended" word that FR-019b prefers screen readers announce.

---

## R4 — Source chip restyling

**Decision**: Render `Profile.Source` as a quiet outlined chip — `Background=Transparent`, `BorderBrush=BorderSubtle`, `BorderThickness=1`, `Foreground=TextSecondary`, `FontSize=11`, `FontWeight=Medium`, `CornerRadius=4`, `Padding=8,2`. No drop shadow, no accent fill.

**Why**:
- FR-014: "quiet metadata, not a saturated colored pill that competes with primary call-to-action styling."
- The current solid `AccentSecondary` pill on every row reads as a CTA. Reserving solid accent for the actual primary action ("Import Profile") restores the visual hierarchy.

**Alternatives considered**:
- *Drop the chip and write the source as plain text inline*: rejected — the chip groups the metadata and is a clearer affordance than an inline word that could be confused with the profile name.
- *Use `AccentSubtle` background (the soft 10% accent token)*: rejected — still a colored pill, just lighter; outlined is calmer.

---

## R5 — Recency indicator

**Decision**: For user-supplied profiles, derive the recency string from the underlying file's `File.GetLastWriteTime(filePath)` and format via the existing `FriendlyDateConverter`. Bundled (embedded-resource) profiles have no on-disk timestamp and MUST omit the field per FR-013.

**Why**:
- `IProfileStorageService.GetProfileFilePaths()` (`src/ReqChecker.Core/Interfaces/IProfileStorageService.cs:23`) already exposes the file paths the user-profile loader uses. We can capture the path on load and store it on the row VM.
- `FriendlyDateConverter` (`src/ReqChecker.App/Converters/FriendlyDateConverter.cs`) is already used by `HistoryView` for the same kind of relative-date display ("Apr 28" / "3 days ago"), so the wording matches the spec example verbatim.
- Bundled profiles are read from the assembly via `Assembly.GetManifestResourceStream` — no file path, no last-write time, no stable equivalent. The spec's "MUST omit gracefully" rule matches the data we have.

**Alternatives considered**:
- *Use the assembly's build timestamp for bundled profiles*: rejected — a single timestamp on every bundled profile is misleading and violates FR-013's spirit.
- *Add a `last-modified` field to the profile JSON*: rejected — out of scope (no profile-data changes).

---

## R6 — Welcome banner / page header duplication

**Decision**: Remove the gradient accent line from the welcome banner only (lines 165–167 of the current XAML). Keep the icon tile, headline, body text, and dismiss button. The page header continues to own the gradient line as the page's identity element.

**Why**:
- FR-004: header and banner MUST NOT duplicate the same combination of treatments.
- The gradient line is the page header's "identity"; duplicating it at the top of the banner makes the eye read two competing headers, which is exactly what FR-004 forbids.
- Stripping just the gradient line preserves the banner's content and dismiss flow (preserved per Assumptions).

**Alternatives considered**:
- *Remove the banner outright*: rejected — it is a first-run aid governed by `IPreferencesService.HasSeenOnboarding` and the spec's Assumptions explicitly preserve it.
- *Move the banner above the page header*: rejected — the page header should always be the dominant element.

---

## R7 — Reduced motion

**Decision**: Implement entrance and hover animations with conservative durations (≤ 200 ms hover, ≤ 300 ms entrance per row stagger) and short displacement (≤ 8 px translate). Tier the per-row entrance via `BeginTime` so the first ~8 visible rows stagger and any rows realized later by virtualization animate immediately. Honor `SystemParameters.ClientAreaAnimation` where available — when `false`, set zero-duration timelines for entrance and hover transitions.

**Why**:
- FR-018 requires animations be "reduced or omitted when the platform exposes a reduced-motion preference."
- WPF's `SystemParameters.ClientAreaAnimation` is the standard signal on Windows; checking it once at view-load and toggling a single resource (e.g., a `bool` exposed by the page VM as `EnableMotion`) is enough to turn animations off.
- Other animated styles in the app currently do NOT honor this signal; this feature does not retrofit them, but the new pattern can serve as a template for future cleanup.

**Alternatives considered**:
- *Always animate*: rejected — FR-018.
- *Strip all motion entirely for safety*: rejected — premium feel (US5, SC-005, SC-008) depends on subtle motion when the user has not disabled it.

---

## R8 — Where to compute "is recommended"

**Decision**: Move the recommended check from `ProfileRecommendedConverter` (which today is wired in XAML via `Visibility` triggers) into `ProfileListItemViewModel.IsRecommended` — a simple boolean computed once at construction (`profile.Id == ProfileSelectorViewModel.DefaultProfileId`). The XAML binds to `IsRecommended` directly via `BoolToVisibilityConverter`.

**Why**:
- Today's converter returns `Visibility` for `Visibility=` bindings AND is also abused as a flag inside a `DataTrigger` (`Value="Visible"`), which is hard to reason about.
- A boolean on the row VM is simpler, testable, and unifies "should the badge show?" with "is this row eligible for the accessible-name suffix?" (FR-019b).
- The existing converter file remains for back-compat but the new view does not reference it.

**Alternatives considered**:
- *Keep the converter and bind to it twice in XAML*: rejected — the new XAML benefits from the row VM owning all of its presentation flags.

---

## R9 — Keyboard activation (Enter/Space → Select)

**Decision**: Use a `ListBox.KeyDown` handler in the code-behind that, on `Key.Enter` or `Key.Space`, invokes the `SelectProfileCommand` with the currently selected item. Click-to-select is already routed through `MouseBinding` semantics provided by `ListBox`'s default `SelectedItem` binding plus a `MouseLeftButtonUp` handler on the row Border.

**Why**:
- `ListBox` natively handles arrow-key navigation and updates `SelectedItem`. We need only to add the activation step (Enter/Space → load the profile and navigate) — FR-007.
- Keeping the activation handler in code-behind (rather than a custom `InputBinding` per row) avoids per-item allocations during virtualization.

**Alternatives considered**:
- *`InputBindings` on each row Border*: rejected — reallocates per-row during virtualization scroll and complicates the row template.
- *A Behavior assembly*: rejected — adds a dependency for two lines of handler code.

---

## R10 — Tests

**Decision**: Add unit tests for the new `ProfileListItemViewModel` (formatting, `IsActive` sync) and extend `ProfileSelectorViewModelTests` with cases for active-profile reflection on `IAppState.CurrentProfileChanged`. WPF view tests are not part of this codebase's existing patterns; visual verification is covered by the manual review steps in `quickstart.md` against `visual-contract.md`.

**Why**:
- The codebase's app tests already use xUnit + Moq with constructor-injection mocks; new tests follow that template directly (`tests/ReqChecker.App.Tests/ViewModels/ProfileSelectorViewModelTests.cs`).
- Visual states (hover/focus/selected) are best validated by reviewer eyes against the visual contract, supplemented by the keyboard-only-navigation walkthrough in `quickstart.md`.

**Alternatives considered**:
- *Add a `dotnet xUnit` UI testing harness (e.g., FlaUI)*: rejected — out of scope and would set a new precedent the codebase does not currently follow.
