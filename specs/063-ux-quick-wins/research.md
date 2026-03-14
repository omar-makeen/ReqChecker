# Research: 063-ux-quick-wins

## R1: Profile Name Display in RunProgress Header

**Decision**: Add a `ProfileName` computed property to `RunProgressViewModel` and bind a new `TextBlock` in the header StackPanel between the title and subtitle.

**Rationale**: `RunProgressViewModel` already stores `CurrentProfile` (line 28) loaded from `_appState.CurrentProfile` (line 175). The header StackPanel already has title + subtitle pattern — adding a third line for profile name is trivial.

**Alternatives considered**:
- Replacing the subtitle with profile name: Rejected — loses "Running X of Y" context.
- Showing profile name in a separate badge: Rejected — over-engineered for a simple label.

## R2: Sidebar Test Count Badge

**Decision**: Use a custom overlay approach — wrap the NavigationViewItem content in a Grid with a small badge Border positioned in the top-right corner. Expose a `TestCount` property on `MainViewModel` synced from `IAppState.CurrentProfile.Tests.Count`.

**Rationale**: WPF-UI 4.2.0's `NavigationViewItem` has no native `InfoBadge` property. A custom overlay badge (small rounded Border with TextBlock) inside the nav item's content area is the simplest approach.

**Alternatives considered**:
- Upgrading WPF-UI for InfoBadge: Rejected — version change risk for one feature.
- Custom NavigationViewItem subclass: Rejected — over-engineered.
- Appending count to Content text ("Test Suite (12)"): Rejected — looks cluttered and doesn't work in compact mode.

## R3: Export Keyboard Shortcut (Ctrl+E)

**Decision**: Add an `InputBinding` with `KeyGesture Ctrl+E` to `ResultsView.xaml`, bound to the existing `ToggleExportMenuCommand` on `ResultsViewModel`.

**Rationale**: WPF InputBindings are scoped to the view they're defined in, so placing it on ResultsView naturally scopes it to the Results page. The `ToggleExportMenuCommand` already handles open/close toggle and respects `CanExportNow`.

**Alternatives considered**:
- Global shortcut on MainWindow: Rejected — requires routing logic and violates page-scoped behavior spec.
- Command in code-behind: Rejected — ViewModel command already exists.

## R4: Filter Tab Transition Animation

**Decision**: Wrap the results ListBox in a named Border, and trigger a fade-out/fade-in Storyboard in `ResultsViewModel` or code-behind when `ActiveFilter` changes. Use existing animation pattern: opacity 1→0 (150ms QuadraticEase EaseIn), swap filter, opacity 0→1 (150ms QuadraticEase EaseOut).

**Rationale**: Matches existing app animation patterns (ViewFadeIn/ViewFadeOut in Animations.xaml). Total 200ms transition keeps it snappy. Using code-behind for Storyboard control is consistent with MainWindow's `ApplyViewFadeOut`/`ApplyViewFadeIn` pattern.

**Alternatives considered**:
- XAML-only DataTrigger animation: Rejected — filter changes via ICollectionView.Refresh() don't trigger XAML property changes on the ListBox.
- VisualStateManager: Rejected — adds unnecessary complexity for a simple fade.

## R5: Tooltip Completeness Audit

**Decision**: Audit all views for buttons/controls missing tooltips. Apply consistent pattern: `ToolTipService.InitialShowDelay="400"`, `ToolTipService.ShowOnDisabled="True"`, content via `ToolTip` property with `ModernToolTip` style.

**Rationale**: Existing tooltip pattern is well-established. This is a sweep to catch gaps, not a new pattern.

**Alternatives considered**: None — straightforward completeness pass.
