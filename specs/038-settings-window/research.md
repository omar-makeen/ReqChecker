# Research: Settings Window

**Feature**: 038-settings-window
**Date**: 2026-02-13

## Technical Context Resolution

No NEEDS CLARIFICATION items existed in the technical context. All technologies and patterns are already established in the codebase.

## Research Findings

### 1. Navigation Pattern for New Pages

**Decision**: Follow the existing NavigationService pattern — DI-resolve ViewModel, create View with ViewModel parameter, navigate Frame, raise Navigated event.

**Rationale**: All 7 existing navigable pages use this exact pattern. Consistency reduces cognitive load and ensures sidebar sync works automatically.

**Alternatives considered**:
- MVVM navigation with DataTemplates: Would require significant refactoring of the existing Frame-based approach. Rejected.
- Dialog/Window instead of Page: Settings would be a modal window. Rejected — the spec requires sidebar navigation integration.

### 2. Theme Selection UI (Side-by-Side Cards)

**Decision**: Use two `Border` elements styled as selectable cards within a horizontal `StackPanel` or `Grid`. The active card gets an accent border/background via a DataTrigger bound to the ViewModel's `CurrentTheme` property.

**Rationale**: The app already uses styled `Border` elements extensively (DiagnosticsView cards, ProfileSelectorView). WPF-UI doesn't provide a built-in "card selector" control, so manual styling is the standard approach.

**Alternatives considered**:
- RadioButton with custom template: Would work but adds unnecessary template complexity for a two-option selector.
- ListBox with ItemTemplate: Overkill for exactly two items.
- ToggleSwitch (WPF-UI): Only supports on/off, doesn't visually represent two named options side-by-side.

### 3. ThemeService Extension — SetTheme vs ToggleTheme

**Decision**: Add a `SetTheme(AppTheme theme)` method to `ThemeService` alongside the existing `ToggleTheme()`. The Settings page calls `SetTheme` directly rather than toggling.

**Rationale**: `ToggleTheme()` is a blind toggle — it doesn't know which theme the user wants. The Settings page needs explicit selection. `SetTheme` is idempotent (setting the same theme twice is a no-op).

**Alternatives considered**:
- Expose `CurrentTheme` setter publicly: Would bypass the `ApplyTheme()` call. Rejected.
- Remove `ToggleTheme` entirely: Not needed since all toggle entry points are being removed, but keeping it doesn't hurt and maintains backward compatibility if future features want a quick toggle.

### 4. Reset to Defaults Confirmation

**Decision**: Use `System.Windows.MessageBox.Show()` with `MessageBoxButton.YesNo` for the reset confirmation dialog.

**Rationale**: The app doesn't have a custom dialog infrastructure for simple confirmations (the existing `DialogService` only handles file dialogs). A native MessageBox is the simplest approach for a yes/no confirmation and matches platform conventions.

**Alternatives considered**:
- WPF-UI ContentDialog: Would require setting up a ContentPresenter host in MainWindow. More work than the feature warrants for a single confirmation.
- Custom modal Window: Over-engineered for a simple yes/no prompt.

### 5. Assembly Version Retrieval

**Decision**: Use `Assembly.GetEntryAssembly()?.GetName().Version?.ToString()` with a fallback to "Unknown".

**Rationale**: This is the standard .NET approach. The entry assembly is the running application, so its version matches what was deployed. No external dependencies needed.

**Alternatives considered**:
- `FileVersionInfo.GetVersionInfo()`: Reads PE file metadata — more complex, same result.
- Hard-coded version: Explicitly rejected by FR-008.

### 7. Premium Design Language for Settings Page

**Decision**: Apply the full premium design language to the Settings page: premium header with 48px rounded icon container and gradient accent bar, elevated card sections with drop shadows (Card/CardElevated styles), staggered entrance animations (AnimatedDiagCard pattern: 300ms fade-in + Y:20→0 slide with CubicEase), hover elevation effects on interactive cards, and consistent 8px spacing grid.

**Rationale**: All other pages (Diagnostics, Results, History) use this design language. A visually inconsistent Settings page would feel jarring and unprofessional. The existing styles (Card, CardElevated, AnimatedDiagCard, AccentGradientHorizontal) can be reused directly — no new style authoring needed.

**Alternatives considered**:
- Minimal card layout without animations: Would be faster to implement but visually inconsistent with the rest of the app. Rejected.
- Functional-only flat layout: Would save the most implementation time but creates a noticeably lower-quality page. Rejected.

### 6. IPreferencesService.ResetToDefaults()

**Decision**: Add `void ResetToDefaults()` to `IPreferencesService` interface and implement in `PreferencesService` by setting `Theme = AppTheme.Dark` and `SidebarExpanded = true` (which triggers auto-save via the existing partial methods).

**Rationale**: Default values are already defined in `UserPreferences` class (Theme="Dark", SidebarExpanded=true). The reset method simply restores these. Auto-save on property change means no explicit `Save()` call is needed.

**Alternatives considered**:
- Delete the preferences file and reload: Would work but is unnecessarily destructive and doesn't update the in-memory state without a reload step.
- Expose default values as constants: Adds indirection for no benefit when defaults are simple and unlikely to change.
