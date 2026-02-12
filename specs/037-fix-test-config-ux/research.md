# Research: Fix Test Configuration Page UX

**Feature**: 037-fix-test-config-ux
**Date**: 2026-02-13

## Research Summary

All technical decisions are straightforward — no unknowns or NEEDS CLARIFICATION items exist. This is a UI-only feature modifying 4 existing files with well-established patterns already in the codebase.

## Decision 1: Header Button Pattern

**Decision**: Use the same header button pattern as ResultsView, HistoryView, ProfileSelectorView, TestListView, and RunProgressView.

**Rationale**: All 5 other pages in the app place action buttons in a StackPanel (Orientation="Horizontal") right-aligned in the header grid. TestConfigView is the only page using a footer bar. Consistency demands following the established pattern.

**Alternatives considered**:
- Inline button after scrollable content — rejected because no other page does this
- Sticky bottom bar (lighter styling) — rejected because it still creates a visual band inconsistent with other pages

## Decision 2: Error Bar Styling Approach

**Decision**: Use existing theme tokens (`StatusFailGlowColor`, `StatusFail`, `BackgroundSurface`) and add a DropShadowEffect for the premium glow, matching the elevation pattern used by cards throughout the app.

**Rationale**: The app already has a `StatusFailGlowColor` (#4Def4444) token designed for error glow effects. Using it avoids adding any new colors while matching the premium aesthetic. The `CornerRadius` should increase from 8 to 12 to match ParameterGroupCard and other premium cards.

**Alternatives considered**:
- Creating a new dedicated ErrorBar style in Controls.xaml — rejected as over-engineering for a single usage
- Using a Snackbar/InfoBar from WPF-UI — rejected because the current Border-based approach is simpler and already works

## Decision 3: Margin Normalization

**Decision**: Change TestConfigView root Grid margin from 24 to 32 to match all other pages.

**Rationale**: TestListView, ResultsView, ProfileSelectorView, and HistoryView all use `Margin="32"`. TestConfigView's `Margin="24"` is an inconsistency, not a deliberate design choice.

**Alternatives considered**:
- Keeping 24 margin but removing MaxWidth only — rejected because it leaves the margin inconsistency

## Decision 4: CancelCommand Removal

**Decision**: Remove CancelCommand property, constructor initialization, and OnCancel() method entirely from TestConfigViewModel.

**Rationale**: No tests reference CancelCommand. No other code references it. The only binding is in TestConfigView.xaml's footer bar which is being removed. Dead code should be deleted, not commented out.

**Alternatives considered**:
- Keeping OnCancel() as a private utility — rejected because it has no callers after footer removal
