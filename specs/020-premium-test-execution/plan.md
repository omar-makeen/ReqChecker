# Implementation Plan: Premium Test Execution Page

**Branch**: `020-premium-test-execution` | **Date**: 2026-02-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/020-premium-test-execution/spec.md`

## Summary

Upgrade the Test Execution page (RunProgressView) to match the premium styling established in other application pages. The implementation involves three main changes: (1) replacing the basic header with the AnimatedPageHeader pattern, (2) ensuring TestStatusBadge displays correctly with colored left borders on result cards, and (3) verifying entrance animations work smoothly for test result items.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (in-memory via IAppState)
**Testing**: Manual visual verification
**Target Platform**: Windows 10/11 desktop
**Project Type**: WPF desktop application
**Performance Goals**: Animations complete within 300ms, smooth 60fps rendering
**Constraints**: Must reuse existing styles from Controls.xaml, maintain existing ViewModel binding structure
**Scale/Scope**: Single view file modification + potential converter additions

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution is a template without specific rules defined. No violations to check.

**Status**: PASS (no constitution rules defined)

## Project Structure

### Documentation (this feature)

```text
specs/020-premium-test-execution/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (minimal - UI-only feature)
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Views/
│   │   └── RunProgressView.xaml          # PRIMARY: Update header + result card styling
│   ├── ViewModels/
│   │   └── RunProgressViewModel.cs       # Add subtitle binding property
│   ├── Converters/
│   │   └── TestStatusToBorderBrushConverter.cs  # NEW: Status to colored border
│   ├── Controls/
│   │   └── TestStatusBadge.xaml          # VERIFY: Already exists, check bindings
│   └── Resources/
│       └── Styles/
│           └── Controls.xaml             # VERIFY: AnimatedPageHeader style exists
├── ReqChecker.Core/
│   ├── Models/
│   │   └── TestResult.cs                 # EXISTING: Has Status property (TestStatus enum)
│   └── Enums/
│       └── TestStatus.cs                 # EXISTING: Pass, Fail, Skipped values
└── ReqChecker.Infrastructure/

tests/
└── (no tests for UI-only changes)
```

**Structure Decision**: Single WPF desktop application. Primary changes in ReqChecker.App/Views/RunProgressView.xaml with supporting converter addition.

## Implementation Design

### Component 1: Premium Header

**Current State** (RunProgressView.xaml lines 52-67):
- Simple StackPanel with icon and text
- No gradient accent line
- No icon container styling
- No entrance animation

**Target State**:
- Use AnimatedPageHeader style (same as TestListView, DiagnosticsView)
- Gradient accent line (4px height)
- Icon in rounded container (48x48px) with accent background
- Contextual subtitle showing run status
- Entrance animation on page load

**Key Reference**: TestListView.xaml lines 64-120 shows exact pattern to follow

### Component 2: Test Status Badges

**Current State** (RunProgressView.xaml lines 344-347):
```xml
<controls:TestStatusBadge Grid.Column="1"
                          Status="{Binding Status}"
                          ShowIcon="True"
                          VerticalAlignment="Center"/>
```

**Analysis**: The TestStatusBadge control exists and is correctly bound. The issue is the badge isn't visible in the screenshot. Possible causes:
1. Converter missing for TestStatusToColorConverter
2. Badge visibility/alignment issue
3. Status property not propagating

**Action**: Verify TestStatusToColorConverter is registered and badge displays correctly

### Component 3: Status-Colored Card Borders

**New Requirement**: Add colored left border (3-4px) to test result cards based on status

**Implementation**: Create TestStatusToBorderBrushConverter to return appropriate border color:
- Pass → StatusPass (green)
- Fail → StatusFail (red)
- Skipped → StatusSkip (amber)

Apply as BorderBrush with BorderThickness="4,0,0,0" (left border only)

### Component 4: Entrance Animations

**Current State**: AnimatedTestResultItem style already exists (lines 14-43) with:
- Fade in (0 → 1 opacity)
- Slide from right (X: 30 → 0)
- 300ms duration with CubicEase

**Analysis**: Animation style exists and is applied. Should work correctly when items are added to TestResults collection.

**Verification needed**: Confirm animations trigger properly when tests complete

## ViewModel Changes

**RunProgressViewModel.cs** additions:

```csharp
// New computed property for header subtitle
public string HeaderSubtitle => IsComplete
    ? $"{TotalTests} tests completed"
    : IsRunning
        ? $"Running {CurrentTestIndex + 1} of {TotalTests} tests"
        : "Ready to run";
```

Update property change notifications to include HeaderSubtitle when relevant properties change.

## File Change Summary

| File | Change Type | Description |
|------|-------------|-------------|
| RunProgressView.xaml | MODIFY | Replace header section with AnimatedPageHeader pattern; add colored left border to result cards |
| RunProgressViewModel.cs | MODIFY | Add HeaderSubtitle computed property |
| TestStatusToBorderBrushConverter.cs | NEW | Convert TestStatus to border brush color |
| App.xaml or Controls.xaml | MODIFY | Register new converter if needed |

## Complexity Tracking

No constitution violations to justify. This is a straightforward UI enhancement following established patterns.

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| TestStatusBadge not displaying | Medium | High | Verify converter registration and binding path |
| Animation performance issues | Low | Medium | Already using virtualization; 300ms duration is reasonable |
| Header layout breaking with long subtitles | Low | Low | Use TextTrimming="CharacterEllipsis" |

## Dependencies

- Existing AnimatedPageHeader style in Controls.xaml
- Existing TestStatusBadge control
- Existing color tokens (StatusPass, StatusFail, StatusSkip)
- Existing TestStatusToColorConverter (needs verification)
