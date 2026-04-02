# Research: Fix Run Button Style & UX

**Date**: 2026-03-14
**Feature**: 065-fix-run-button-style

## Finding 1: PrimaryButtonLarge Style Already Exists

**Decision**: No new style needed — `PrimaryButtonLarge` is already defined and applied.

**Rationale**: Investigation revealed that `PrimaryButtonLarge` exists at `Controls.xaml:314` as a variant of `PrimaryButton` with `Height=48`, `MinWidth=120`, `FontSize=16`, `Padding=24,0`. It is already referenced by the Run button in `TestListView.xaml:124`. The initial assumption that the style was missing was caused by a tool encoding issue during the original grep search.

**Alternatives considered**: Creating a new style or switching to `PrimaryButton` — both unnecessary since the correct style already exists and is applied.

## Finding 2: Inline FontSize Override

**Decision**: Remove the inline `FontSize="15"` from the TextBlock inside the Run button.

**Rationale**: The `PrimaryButtonLarge` style sets `FontSize="16"`. The inline `FontSize="15"` on the TextBlock (line 132) overrides this at the content level, creating a 1px inconsistency. Removing it lets the style's font size propagate naturally.

**Alternatives considered**: Changing the style's FontSize to 15 — rejected because 16 is the intended large button size and is consistent with the design system.

## Finding 3: Disabled Tooltip Approach

**Decision**: Use conditional tooltip that changes based on `IsEnabled` state.

**Rationale**: WPF supports `ToolTipService.ShowOnDisabled="True"` (already used elsewhere in the app) combined with a style trigger or converter to swap the tooltip text. The current static tooltip "Execute all selected tests and view results" provides no guidance when the button is disabled.

**Alternatives considered**: Using a separate overlay/hint text below the button — rejected as over-engineered for this use case.

## Revised Scope

The original spec assumed `PrimaryButtonLarge` was missing (the primary CTA looked broken). In reality, the style exists and works. The remaining scope is:

1. **P2**: Contextual disabled tooltip ("Select at least one test to run")
2. **P3**: Remove inline `FontSize="15"` override

FR-001 (gradient style) and FR-002 (reusable resource) are already satisfied. Only FR-003, FR-004, and FR-005 require implementation.
