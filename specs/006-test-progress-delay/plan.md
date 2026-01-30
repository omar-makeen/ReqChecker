# Implementation Plan: Test Progress Delay for User Visibility

**Branch**: `006-test-progress-delay` | **Date**: 2026-01-31 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/006-test-progress-delay/spec.md`

## Summary

Add a configurable delay between test executions so users can observe which test is currently running before the next test begins. The feature includes inline controls on the Run Progress view (toggle + slider), persisted preferences, and immediate cancellation support during delay pauses.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2
**Storage**: `%APPDATA%/ReqChecker/preferences.json` (JSON via System.Text.Json)
**Testing**: xUnit (existing test projects in tests/)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single solution with layered architecture (App → Infrastructure → Core)
**Performance Goals**: Cancellation during delay completes within 200ms (per SC-004)
**Constraints**: Delay range 0-3000ms, default 500ms, no delay before first or after last test
**Scale/Scope**: Single-user desktop application

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The project constitution is not yet configured with specific principles. Proceeding with standard best practices:

- [x] **Minimal Scope**: Feature touches only necessary files (preferences, test runner, view/viewmodel)
- [x] **Existing Patterns**: Uses established MVVM, DI, and async patterns already in codebase
- [x] **No New Dependencies**: Uses existing WPF-UI controls and System.Text.Json
- [x] **Testable Design**: Delay logic isolated in SequentialTestRunner, cancellation token support

## Project Structure

### Documentation (this feature)

```text
specs/006-test-progress-delay/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Services/
│   │   └── PreferencesService.cs        # Extend with delay settings
│   ├── ViewModels/
│   │   └── RunProgressViewModel.cs      # Add delay toggle/slider bindings
│   └── Views/
│       └── RunProgressView.xaml         # Add inline delay controls
├── ReqChecker.Core/
│   └── Models/
│       └── RunSettings.cs               # Add InterTestDelayMs property
└── ReqChecker.Infrastructure/
    └── Execution/
        └── SequentialTestRunner.cs      # Insert Task.Delay between tests

tests/
├── ReqChecker.App.Tests/
└── ReqChecker.Infrastructure.Tests/
    └── Execution/
        └── SequentialTestRunnerTests.cs # Test delay behavior
```

**Structure Decision**: Layered architecture with Core (models) → Infrastructure (execution) → App (UI). Delay configuration flows from App (preferences) through ViewModel to Infrastructure (runner).

## Complexity Tracking

No constitution violations. Feature follows existing patterns:
- Preferences extension follows `PreferencesService` pattern
- RunSettings extension follows existing model pattern
- Task.Delay follows existing retry delay pattern in `RetryPolicy.cs`
- UI controls follow existing WPF-UI component patterns

---

## Phase 0: Research

See [research.md](./research.md) for detailed findings.

### Key Decisions

1. **Delay Location**: Insert in `SequentialTestRunner.cs` after `progress?.Report()` - same pattern as retry delays
2. **Cancellation**: Use existing `CancellationToken` passed to `Task.Delay()` for immediate response
3. **UI Controls**: Use WPF-UI `ToggleSwitch` for enable/disable + `Slider` for duration
4. **Persistence**: Extend existing `UserPreferences` class and `PreferencesService`

---

## Phase 1: Design

### Data Model

See [data-model.md](./data-model.md) for entity definitions.

**Extended Entities:**

1. **UserPreferences** (existing, extend)
   - `TestProgressDelayEnabled: bool` (default: true)
   - `TestProgressDelayMs: int` (default: 500, range: 0-3000)

2. **RunSettings** (existing, extend)
   - `InterTestDelayMs: int` (default: 0, set from preferences when running)

### Component Design

```
┌─────────────────────────────────────────────────────────────────┐
│                     RunProgressView.xaml                        │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  [Toggle] Enable delay  [Slider 0-3000ms]  [Label: 500ms] │ │
│  └───────────────────────────────────────────────────────────┘ │
│            ↕ Data Binding                                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                  RunProgressViewModel.cs                         │
│  - TestProgressDelayEnabled (bool)                              │
│  - TestProgressDelayMs (int)                                    │
│  - Binds to IPreferencesService for persistence                 │
│  - Passes delay to RunSettings when starting tests              │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                   SequentialTestRunner.cs                        │
│  RunTestsAsync(profile, progress, token, runSettings):          │
│    foreach test:                                                │
│      result = ExecuteTest()                                     │
│      progress?.Report(result)                                   │
│      if (runSettings.InterTestDelayMs > 0 && hasMoreTests)      │
│        await Task.Delay(delay, token)  ← CANCELLABLE            │
└─────────────────────────────────────────────────────────────────┘
```

### Execution Flow

1. User opens Run Progress view
2. Delay controls show current preference values (bound to PreferencesService)
3. User adjusts toggle/slider → auto-saved to preferences.json
4. User clicks "Run Tests"
5. ViewModel reads current delay setting, passes to RunSettings
6. SequentialTestRunner executes tests with delay between each
7. If user clicks Cancel during delay → CancellationToken triggers → immediate exit
8. Tests complete, delay not applied after final test

### UI Control Placement

Add controls below the progress ring, above the stats summary:

```xaml
<!-- Delay Controls (inline, below progress ring) -->
<StackPanel Orientation="Horizontal" Margin="0,16,0,0">
    <ui:ToggleSwitch IsChecked="{Binding TestProgressDelayEnabled}"
                     Content="Demo Mode" />
    <Slider Value="{Binding TestProgressDelayMs}"
            Minimum="0" Maximum="3000"
            Width="150" Margin="16,0,0,0"
            IsEnabled="{Binding TestProgressDelayEnabled}" />
    <TextBlock Text="{Binding TestProgressDelayMs, StringFormat={}{0}ms}"
               Margin="8,0,0,0" />
</StackPanel>
```

---

## Implementation Phases

### Phase 1: Core Infrastructure (P1 - Observable Execution)

1. **Extend RunSettings model** - Add `InterTestDelayMs` property
2. **Modify SequentialTestRunner** - Insert delay after progress report
3. **Unit tests** - Verify delay behavior and cancellation

### Phase 2: Persistence Layer (P1 continuation)

4. **Extend UserPreferences** - Add delay settings properties
5. **Extend PreferencesService** - Add observable properties with auto-save

### Phase 3: UI Integration (P2 - Configurable Duration + Toggle)

6. **Extend RunProgressViewModel** - Bind to preferences, pass to runner
7. **Update RunProgressView.xaml** - Add inline delay controls
8. **Manual testing** - Verify end-to-end flow

### Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.Core/Models/RunSettings.cs` | Add `InterTestDelayMs` property |
| `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs` | Add delay after `progress?.Report()` |
| `src/ReqChecker.App/Services/PreferencesService.cs` | Add delay properties, extend UserPreferences |
| `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` | Bind delay settings, pass to runner |
| `src/ReqChecker.App/Views/RunProgressView.xaml` | Add toggle switch and slider controls |
| `tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs` | Add delay and cancellation tests |

---

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Delay blocks UI thread | Use `Task.Delay` (async) - already proven in RetryPolicy |
| Cancellation not immediate | Pass CancellationToken to `Task.Delay` - built-in support |
| Settings not persisting | Follow existing PreferencesService pattern exactly |
| UI controls clutter progress view | Minimal inline design, collapsible if needed |

---

## Success Verification

After implementation, verify:

1. [ ] Run 4+ tests with default 500ms delay - each test visible for readable duration
2. [ ] Toggle delay off - tests run immediately without pauses
3. [ ] Adjust slider to 1000ms - delay duration changes accordingly
4. [ ] Cancel during delay - cancellation completes within 200ms
5. [ ] Close and reopen app - delay settings preserved
6. [ ] Single test profile - delay applies (shows completion briefly)
