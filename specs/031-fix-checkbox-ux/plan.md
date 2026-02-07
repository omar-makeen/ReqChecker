# Implementation Plan: Fix Checkbox Visibility and Select All Placement

**Branch**: `031-fix-checkbox-ux` | **Date**: 2026-02-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/031-fix-checkbox-ux/spec.md`

## Summary

Fix two UI issues on the Test Suite page: (1) checkbox checkmarks are invisible because the Path geometry coordinates exceed the 18x18 container bounds, and (2) the "Select All" checkbox is poorly placed in the header — relocate it to a dedicated toolbar row between the header and test list.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only fix, no data persistence changes)
**Testing**: xUnit 2.9.2, Moq 4.20.72 (existing test suite)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single solution, WPF desktop app
**Performance Goals**: N/A (visual fix only)
**Constraints**: Must work in both dark and light themes
**Scale/Scope**: 2 files changed (Controls.xaml checkbox style, TestListView.xaml layout)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is a placeholder template (not customized for this project). No gates to enforce. Proceeding.

## Project Structure

### Documentation (this feature)

```text
specs/031-fix-checkbox-ux/
├── spec.md
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (minimal — no data changes)
├── quickstart.md        # Phase 1 output
└── checklists/
    └── requirements.md  # Spec quality checklist
```

### Source Code (repository root)

```text
src/ReqChecker.App/
├── Resources/Styles/Controls.xaml    # AccentCheckBox style (Fix 1: checkmark Path geometry)
└── Views/TestListView.xaml           # Test Suite page layout (Fix 2: toolbar row for Select All)

tests/ReqChecker.App.Tests/
└── ViewModels/TestListViewModelTests.cs  # Existing tests (no changes needed — logic unchanged)
```

**Structure Decision**: Existing project structure. Only two XAML files are modified; no new files or projects.

## Design

### Fix 1: Checkbox Checkmark Visibility

**Root Cause**: The `AccentCheckBox` style in `Controls.xaml` (line 996) defines a checkmark `Path` with `Data="M3.5,7.5 L6.5,10.5 L14.5,2.5"`. This geometry spans 11x8 logical pixels (x: 3.5→14.5, y: 2.5→10.5), but it's placed inside an 18x18 `Border` with 1.5px `BorderThickness`. The inner content area is ~15x15, but the Path has no explicit `Width`/`Height`/`Stretch` — WPF defaults `Stretch` to `None` for Path, meaning the path renders at its natural size from origin (0,0). The checkmark coordinates place the drawing well outside the visible center of the 15x15 area, causing it to be clipped or invisible.

**Fix**: Rescale the Path `Data` coordinates to fit centered within the 18x18 container. Target geometry: a checkmark approximately 10x8 pixels, centered. Add `Stretch="None"` explicitly and use `HorizontalAlignment="Center"` / `VerticalAlignment="Center"` on the Path. Adjust coordinates to:

```
M4,9 L7,12 L13,4
```

This keeps the checkmark centered within the border and visible at 18x18 size. The stroke thickness of 2 remains appropriate.

### Fix 2: Relocate Select All to Toolbar Row

**Current Layout** (TestListView.xaml grid rows):
- Row 0: Header (title + Select All + badge + Run button)
- Row 1: Test list / Empty state

**New Layout**:
- Row 0: Header (title + badge + Run button) — Select All removed from here
- Row 1: Toolbar row (Select All checkbox + label, left-aligned) — NEW
- Row 2: Test list / Empty state

The toolbar row:
- Uses `Auto` height in the grid
- Has the same left margin as the test list for visual alignment
- Visibility bound to `CurrentProfile` (hidden when no profile loaded)
- Contains the Select All checkbox + "Select All" label, left-aligned
- Has subtle bottom margin to separate from the test list

### No ViewModel Changes

Both fixes are purely XAML. The `TestListViewModel` properties (`IsAllSelected`, `HasSelectedTests`, `SelectableTests`, etc.) remain unchanged. No new tests are needed since the logic is identical — only the visual presentation changes.

## Complexity Tracking

No constitution violations. No complexity justification needed.
