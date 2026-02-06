# Implementation Plan: Premium Test Execution UX Polish

**Branch**: `028-premium-test-execution-ux` | **Date**: 2026-02-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/028-premium-test-execution-ux/spec.md`

## Summary

Fix three visual bugs on the test execution page and polish for premium quality: (1) progress ring renders an arc artifact at 0%, (2) no "Preparing" state shown before first test runs, (3) progress ring shifts upward on completion due to card height differences. All fixes are contained to 3 files — ProgressRing control, RunProgressViewModel, and RunProgressView XAML.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (in-memory only)
**Testing**: Manual visual testing (UI-only changes)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single desktop application
**Performance Goals**: 60fps smooth transitions, no layout shift
**Constraints**: UI-only changes, no new dependencies, no new files
**Scale/Scope**: 3 files modified

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is not customized (placeholder template). No gates to enforce. Proceeding.

**Post-design re-check**: No violations. This feature modifies 3 existing files, adds no new projects, no new dependencies, no new abstractions.

## Project Structure

### Documentation (this feature)

```text
specs/028-premium-test-execution-ux/
├── plan.md              # This file
├── research.md          # Phase 0 output — research decisions
├── quickstart.md        # Phase 1 output — verification steps
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (files to modify)

```text
src/ReqChecker.App/
├── Controls/
│   └── ProgressRing.xaml.cs       # Fix 0% arc rendering (FR-001)
├── ViewModels/
│   └── RunProgressViewModel.cs    # Add "Preparing..." initial state (FR-002, FR-003)
└── Views/
    └── RunProgressView.xaml        # Fix layout shift, normalize cards (FR-004, FR-005, FR-006)
```

**Structure Decision**: Existing single-project WPF application. All changes are in-place modifications to 3 existing files. No new files or structural changes needed.

## Implementation Details

### Change 1: Fix Progress Ring 0% Arc (FR-001)

**File**: `src/ReqChecker.App/Controls/ProgressRing.xaml.cs`

**Root cause**: `UpdateArc()` at line 179 sets `_progressArc.Visibility = Visibility.Hidden` when Progress ≤ 0. Two problems:
1. `Hidden` still occupies layout space (should be `Collapsed`)
2. `UpdateVisualState()` at line 231 can override this by unconditionally setting `_progressArc.Visibility = Visibility.Visible` when not in indeterminate mode

**Fix**:
- In `UpdateArc()`: Change `Visibility.Hidden` to `Visibility.Collapsed` when Progress ≤ 0
- In `UpdateVisualState()`: Only set `_progressArc.Visibility = Visible` when Progress > 0; otherwise keep it `Collapsed`

### Change 2: Add "Preparing..." Initial State (FR-002, FR-003, FR-007)

**File**: `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`

**Root cause**: `CurrentTestName` is `null` when `StartTestsAsync()` begins. The XAML shows `FallbackValue='Waiting...'` which is ambiguous. The first `OnTestCompleted` callback sets it to the NEXT test's name (index 1), so users never see the first test's name in the "Currently Running" card.

**Fix**:
- In `StartTestsAsync()`, after resetting counters and before calling `RunTestsAsync()`, set `CurrentTestName = "Preparing..."`
- This uses the existing "Currently Running" card (visible because `IsTestRunning = true`)
- When the first `OnTestCompleted` fires, `CurrentTestName` is updated to the next test's name (existing behavior)

### Change 3: Fix Layout Shift on Completion (FR-004, FR-005, FR-006)

**File**: `src/ReqChecker.App/Views/RunProgressView.xaml`

**Root cause**: The left column (line 156) uses a `StackPanel` with `VerticalAlignment="Center"`. The "Currently Running" card (~80px tall) and "Completion Summary" card (~140px tall) have different heights. When one replaces the other, the StackPanel's total height changes and the centered position recalculates, causing the ring to shift ~30px upward.

**Fix**:
- Replace the StackPanel container with a Grid using fixed row definitions:
  - Row 0 (Auto): Progress ring (fixed 160px, unaffected)
  - Row 1 (Fixed height): Card area — both cards occupy the same Grid cell. Use a fixed `MinHeight` matching the taller card, so the row height never changes.
  - Row 2 (Auto): Stats summary
- Keep `VerticalAlignment="Center"` on the outer Grid so the whole group is still centered, but now the group's total height is constant.
- Normalize the completion card to be similar height to the running card: reduce checkmark icon from 48px to 24px, use TextBody instead of TextH3 for the heading.
