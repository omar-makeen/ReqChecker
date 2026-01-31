# Implementation Plan: Display Test Result Details in Expanded Card

**Branch**: `010-result-details` | **Date**: 2026-01-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/010-result-details/spec.md`

## Summary

Display test result details (summary, technical details, error info) within the expanded test card in the Results view. The ExpanderCard control already supports these properties, but test implementations don't populate the `HumanSummary` and `TechnicalDetails` fields. The solution requires: (1) updating ExpanderCard XAML styling for premium look with accent left borders, and (2) generating readable summary/details from TestResult.Evidence data.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (in-memory TestResult objects)
**Testing**: xUnit (existing test infrastructure)
**Target Platform**: Windows 10/11 desktop
**Project Type**: Single WPF application with layered architecture (App/Core/Infrastructure)
**Performance Goals**: Expand animation < 200ms, no visible lag
**Constraints**: Must support dark/light themes, credential masking required
**Scale/Scope**: Display up to 4KB of response data per test

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution is a template without specific principles defined. No gates apply.

**Status**: PASS (no constraints defined)

## Project Structure

### Documentation (this feature)

```text
specs/010-result-details/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Controls/
│   │   ├── ExpanderCard.xaml       # UPDATE: Premium styling with accent borders
│   │   └── ExpanderCard.xaml.cs    # UPDATE: Add metadata properties (Duration, AttemptCount)
│   ├── Views/
│   │   └── ResultsView.xaml        # UPDATE: Bind new metadata properties
│   ├── Converters/
│   │   └── TestResultFormatter.cs  # NEW: Generate HumanSummary/TechnicalDetails from Evidence
│   └── Resources/Styles/
│       └── Controls.xaml           # Reference: Existing premium styles
├── ReqChecker.Core/
│   └── Models/
│       └── TestResult.cs           # Reference only (no changes needed)
└── ReqChecker.Infrastructure/
    └── Tests/
        └── *.cs                    # Reference: Evidence population patterns

tests/
└── ReqChecker.App.Tests/
    └── Converters/
        └── TestResultFormatterTests.cs  # NEW: Unit tests for formatter
```

**Structure Decision**: Extend existing App layer with a new converter for formatting. No new projects needed.

## Root Cause Analysis

The expanded card shows no content because:

1. **ExpanderCard bindings are correct** - `Summary="{Binding HumanSummary}"`, etc.
2. **TestResult fields are empty** - Test implementations set `Evidence` but leave `HumanSummary` and `TechnicalDetails` as empty strings
3. **No post-processing** - The runner doesn't generate human-readable text from Evidence

## Solution Approach

### Option A: Generate at Display Time (Chosen)
Create a multi-binding converter that generates readable text from TestResult properties when rendering. This keeps the model clean and allows formatting flexibility.

### Option B: Generate at Test Execution Time (Rejected)
Modify each test implementation to set HumanSummary/TechnicalDetails. This would require changes to 10+ test files and couples display logic to tests.

### Option C: Generate in Test Runner (Rejected)
Post-process results in SequentialTestRunner. This adds responsibility to the runner and still couples display logic to execution.

## Implementation Components

### 1. TestResultFormatter Converter
A `IMultiValueConverter` that takes TestResult and generates formatted text:
- `HumanSummary`: Friendly description from Status, Error, Evidence
- `TechnicalDetails`: Formatted Evidence data (JSON prettified, headers formatted)

### 2. ExpanderCard Premium Styling
Update XAML for premium look:
- Technical details: Elevated background + accent gradient left border
- Error section: Elevated background + red left border
- Add Duration and AttemptCount display

### 3. ResultsView Binding Updates
- Use MultiBinding with TestResultFormatter for dynamic Summary/TechnicalDetails
- Bind new metadata (Duration formatted, AttemptCount with "retry" label)

## Complexity Tracking

> No Constitution violations - no complexity justification needed.

| Component | Complexity | Justification |
|-----------|------------|---------------|
| TestResultFormatter | Medium | Single class, well-defined input/output |
| ExpanderCard styling | Low | XAML-only changes |
| ResultsView bindings | Low | Swap simple bindings for multi-bindings |
