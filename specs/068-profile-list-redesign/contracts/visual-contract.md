# Visual Contract — Profile Manager List Redesign

**Branch**: `068-profile-list-redesign`
**Date**: 2026-04-30
**Purpose**: The visual state machine of a profile row, the tokens used per state, and the explicit visual rules a code reviewer or designer can verify against. This document is the source of truth for what the redesigned list looks like; spec FRs map to it directly.

---

## Page structure (top to bottom)

1. **Page header** (existing `AnimatedPageHeader` style) — gradient accent line at top, 48 × 48 colored icon tile, title "Profile Manager", subtitle "Manage and import test profiles", and the action group `[Refresh] [Import Profile]` at right.
2. **Welcome banner** — same horizontal composition as today **minus the 4-px gradient accent line**. Pure rounded card with icon tile, headline "Welcome to ReqChecker", body text, and a top-right dismiss button.
3. **Inline error banner** (when `HasError`) — keeps current treatment (red glow border, `ErrorCircle24` + message + Dismiss button).
4. **Profile list** — virtualized full-width rows, separated by 12 px vertical gap. Anchored to the remaining vertical space (`Grid.Row="*"`).
5. **Empty state** (when `Items.Count == 0`) — kept verbatim (centered icon-with-add-badge composition, headline "No profiles found", subtext "Import a profile to get started").
6. **Loading overlay** (when `IsLoading`) — kept verbatim (centered `ProgressRing` + label).

The page outer margin remains `32`. The list-area margin matches the rest of the app.

---

## Row anatomy

```text
┌──────────────────────────────────────────────────────────────────────────────┐
│ ▌  Profile Name (SemiBold, ellipsis)        [Recommended]                     │
│     Source · 8 tests · v3 · modified 3 days ago                               │
└──────────────────────────────────────────────────────────────────────────────┘
   ↑                                       ↑
   16-px content padding               Right-aligned RecommendedBadge
```

- Outer Border — `Style="{StaticResource Card}"`, `Padding=16`, `Cursor=Hand`.
- Top row (left → right):
  - Optional 24 × 24 icon container (folder symbol) — kept subtle; uses `BackgroundElevated` background and `TextTertiary` foreground. Not a colored accent tile.
  - **Name** — FontSize 15, FontWeight SemiBold, `Foreground=TextPrimary`, `TextTrimming=CharacterEllipsis`, with `ToolTipService.ToolTip="{Binding Name}"`.
  - **RecommendedBadge** (right-aligned) — applied via `Style="{StaticResource RecommendedBadge}"`. Visible only when `IsRecommended` is true.
- Bottom row (single horizontal `StackPanel`, 8 px above the dot separators):
  - **Source chip** — outlined: `BorderBrush=BorderSubtle`, `BorderThickness=1`, `Background=Transparent`, `Foreground=TextSecondary`, `FontSize=11`, `FontWeight=Medium`, `CornerRadius=4`, `Padding=8,2`. Text = `SourceLabel`.
  - **Dot separator** ` · ` (`Foreground=TextTertiary`).
  - **Test count** — `FontSize=13`, `Foreground=TextSecondary`. Text = `TestCountLabel`.
  - **Dot separator**.
  - **Schema version** — same style as test count; entire segment hidden when `SchemaVersionLabel` is `null`.
  - **Dot separator**.
  - **Modified** — same style as test count; entire segment hidden when `ModifiedLabel` is `null`.

Spacing inside the row: 8 px between top-row and bottom-row, 8 px between segments in the bottom row (margin handles the dots).

---

## Visual states

A row has four mutually exclusive states. The page header focus is independent.

| State | Trigger | Tokens / treatment |
|---|---|---|
| **Default** | none | `Card` style: `Background=BackgroundSurface`, `BorderBrush=BorderDefault`, 1-px border, 8-px corner radius, ambient drop shadow (existing `Card` settings). |
| **Hover** | mouse over the row | `Background=BackgroundElevated`, `BorderBrush=BorderStrong`, slight `TranslateTransform Y=-1` (≤ 200 ms ease-out — FR-017). No size change. |
| **Focus (keyboard)** | `ListBoxItem.IsKeyboardFocusWithin` | The default `FocusVisualStyle` from `Resources/Styles/Controls.xaml` (`AccentSecondary` 2-px ring at `Margin=-2`). Renders **on top of** Default or Hover states — additive, not replacing. |
| **Selected / Active** | `IsActive` is true (i.e., `Profile` matches `IAppState.CurrentProfile`) | Inner Border swaps to `CardSelected`: `BorderBrush=AccentPrimary`, 2-px border, accent glow. The accent border replaces the Default 1-px border, not the focus ring. |

State transitions:

- All transitions complete in ≤ 200 ms (FR-017, SC-005).
- No state change causes layout shift (no width/height delta — only color, border, glow, and Y-translate).
- Hover + Focus + Selected can all be true at once; the visual stack is `Selected (border + glow) → Hover (background + lift) → Focus (ring)` from inside out.

---

## Decorations explicitly forbidden (regression guards)

These are removals from the current view; they MUST NOT reappear:

1. **Per-row "Select Profile" button** — the row itself is the action target (FR-006).
2. **2-px accent border on the recommended profile** — only the badge marks it (FR-015).
3. **6-px gradient header strip on every card** — every row uses the plain `Card` chrome (FR-016).
4. **Solid-color source pill** — the source is rendered as a quiet outlined chip (FR-014).
5. **Gradient accent line at the top of the welcome banner** — removed so the banner stops competing with the page header (FR-004). The page header keeps its line.
6. **Fixed-width 320-px tiles** / **`WrapPanel` layout** — replaced by full-width virtualized rows (FR-001, FR-003).

---

## Motion contract

- **Entrance** — list rows fade-in + translate-Y (8 px) over 250 ms with a 30 ms stagger across the first 8 visible rows. Rows realized later by virtualization animate independently with no stagger.
- **Hover transitions** — color/elevation transitions ≤ 200 ms with `CubicEase` `EaseOut`.
- **Selected state transition** — when `IsActive` flips, the border-swap is instantaneous (no animation) to keep the cue authoritative; the surrounding glow fades in over ≤ 200 ms.
- **Reduced motion** — when `SystemParameters.ClientAreaAnimation == false`, all entrance staggers and hover translate-Y are zero-duration; only color changes remain.

---

## Accessibility contract

- The `ListBox` carries `AutomationProperties.Name="Profiles list"` (list semantics — FR-019a).
- Each row Border carries `AutomationProperties.Name="{Binding AccessibleName}"`, which equals `Name` (and ` (recommended)` when applicable — FR-019b).
- The selected/active row is announced to assistive technologies as the selected item via the standard `ListBox.SelectedItem` mechanism (no extra plumbing required) — FR-009b.
- The focus ring (`FocusVisualStyle`) is mandatory; it is the only mechanism by which keyboard users perceive focus position — FR-019.
- No formal WCAG audit is required (FR-019c); this contract is the bar.

---

## Cross-reference (FR ↔ visual rule)

| FR | Visual rule above |
|---|---|
| FR-001 / FR-003 | Page structure §4 (full-width virtualized rows) |
| FR-002 | Row anatomy + Visual states (mirrors `HistoryView` Card chrome) |
| FR-004 | Page structure §2 (gradient line removed from banner) |
| FR-005 / FR-006 | View-Model contract (whole-row click) + Decorations forbidden §1 |
| FR-007 / FR-008 | View-Model contract `SelectedItem` rules |
| FR-009 / FR-009a / FR-009b | Visual states: Default / Hover / Focus / Selected, plus AT contract |
| FR-010 | Row anatomy: Name w/ ellipsis + tooltip |
| FR-011 / FR-012 / FR-013 | Row anatomy: bottom-row segments, hidden when null |
| FR-014 | Row anatomy: outlined source chip; Decorations forbidden §4 |
| FR-015 / FR-016 | Decorations forbidden §2 / §3 |
| FR-017 / FR-018 | Motion contract |
| FR-019 / FR-019a / FR-019b / FR-019c | Accessibility contract |
| FR-020 | Page structure §4 (virtualization) |
| FR-021 / FR-022 / FR-023 | Page structure §3, §5, §6 |
