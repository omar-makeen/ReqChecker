# Research: 036-test-config-ux

**Date**: 2026-02-12

## R1: Optimal Spacing Values for Compact Form Layout

**Decision**: Use the project's existing spacing tokens with tighter values — SpacingMd (12px) for field rows, SpacingBase (16px) for card padding, SpacingMd (12px) for card gaps, SpacingLg (24px) for page margin.

**Rationale**: The existing Spacing.xaml already defines a comprehensive 8px-base system. The current TestConfigView uses hardcoded values that are looser than what the token system provides. By switching to the tighter standard tokens (which already exist as CardPadding=16 and CardMargin=0,0,0,12), we achieve the density goal without inventing new values.

**Alternatives considered**:
- Custom spacing tokens specific to this page — rejected because it would fragment the design system
- Using SpacingSm (8px) for field rows — rejected as too tight for comfortable touch/click targets

## R2: Locked Field Redesign Approach

**Decision**: Simplify LockedFieldControl to a single-row layout: inline lock icon (14px) + muted text value in one continuous container, removing the separate bordered lock icon column.

**Rationale**: The current two-column layout (bordered lock icon box + bordered value box) consumes ~50px of horizontal space for the icon alone and creates visual noise. A single container with a small inline lock icon preserves the "read-only" signal while being much lighter. The lock icon tooltip is retained for accessibility.

**Alternatives considered**:
- Remove lock icon entirely and rely only on muted text color — rejected because the visual distinction between "muted editable" and "locked" would be ambiguous
- Use a lock overlay on the field — rejected as overly complex for a simple indicator

## R3: Section Header Icon Size

**Decision**: Reduce icon badge from 32x32 to 24x24, using existing IconSizeLarge (24) and IconSizeMedium (20) tokens from Spacing.xaml.

**Rationale**: The 32x32 badge is disproportionate to the TextH3 heading (16px font). A 24x24 badge (with 14px inner icon) maintains the colored accent while being proportional. This aligns with the IconSizeLarge token already defined.

**Alternatives considered**:
- Remove colored badges entirely, use plain icons — rejected because the colored badges provide useful visual categorization of sections
- Use 20x20 badges — rejected as slightly too small for the section header context

## R4: Card Styling Without Glow

**Decision**: Remove the DropShadowEffect from ParameterGroupCard entirely. Use only BorderSubtle (instead of BorderDefault) for a lighter border. Keep the surface background and corner radius.

**Rationale**: The current glow effect (BlurRadius=10, Opacity=1, ElevationGlowColor) creates the heavy "floating island" appearance visible in the screenshot. Removing it and using a subtler border preserves card grouping without visual heaviness. This also slightly improves rendering performance (DropShadowEffect is GPU-intensive in WPF).

**Alternatives considered**:
- Reduce glow opacity to 0.2 — rejected because even subtle glows are visible on dark backgrounds and the user specifically flagged this
- Use a very thin top-border-only separator — rejected per clarification decision to keep separate cards

## R5: "Requires Admin" Field Consistency

**Decision**: Replace the custom badge-style display with the same LockedFieldControl used for Name and Type, passing the boolean value as "Yes"/"No" text.

**Rationale**: The current implementation uses a different visual treatment (Badge with Shield icon) that breaks alignment with other locked fields. Using LockedFieldControl ensures visual consistency across all read-only fields in the Basic Information section.

**Alternatives considered**:
- Keep the badge but align it to match LockedFieldControl height — rejected because it still creates visual inconsistency
- Use a toggle switch in disabled state — rejected because it implies interactivity

## R6: Label Column Width

**Decision**: Reduce from 140px to 120px.

**Rationale**: "Requires Admin" (longest label) measures ~110px at 14px font. 120px provides adequate space with 10px breathing room while being 20px narrower than current, closing the label-to-value gap meaningfully.

**Alternatives considered**:
- Auto-sizing based on content — rejected because it would cause misalignment between rows
- 100px — rejected as too tight for "Requires Admin" with potential padding

## R7: Blast Radius Assessment

**Decision**: Changes are isolated and safe to proceed.

**Rationale**:
- ParameterGroupCard: Used ONLY in TestConfigView.xaml (3 instances) — safe to modify the shared style
- InputField: Used in TestConfigView AND TestListView — do NOT modify this shared style; changes to field height/padding should be done inline or via a local override
- LockedFieldControl: Self-contained custom control, only used in TestConfigView — safe to modify
- PromptAtRunIndicator: Used ONLY in TestConfigView — safe to modify
- Spacing.xaml: Do NOT modify system-wide tokens; use existing tokens via references in TestConfigView
- No existing unit tests for TestConfigViewModel — ViewModel changes are minimal (none for pure UI work)
