# Tasks: Remove Demo Mode UI Control

**Input**: Design documents from `/specs/007-remove-demo-delay/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: No new tests required - existing `SequentialTestRunnerTests` already verify delay behavior.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- Paths use existing ReqChecker project structure

---

## Phase 1: Setup (Core Infrastructure Change)

**Purpose**: Establish the single source of truth for delay value

- [x] T001 Change `InterTestDelayMs` default from 0 to 500 in `src/ReqChecker.Core/Models/RunSettings.cs`

**Checkpoint**: Delay default value is now 500ms - the single source of truth is established

---

## Phase 2: User Story 1 - Simplified Test Execution UI (Priority: P1)

**Goal**: Remove Demo Mode UI controls from Run Progress view

**Independent Test**: Run any test profile and verify Demo Mode controls (toggle, slider, ms display) are absent from UI

### Implementation for User Story 1

- [x] T002 [US1] Remove Demo Mode controls StackPanel (lines 144-167) from `src/ReqChecker.App/Views/RunProgressView.xaml`
- [x] T003 [US1] Remove `TestProgressDelayEnabled` property from `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`
- [x] T004 [US1] Remove `TestProgressDelayMs` property from `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`
- [x] T005 [US1] Simplify `StartTestsAsync` to use `new RunSettings()` in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`

**Checkpoint**: UI no longer shows Demo Mode controls

---

## Phase 3: User Story 2 - Automatic Inter-Test Delay (Priority: P1)

**Goal**: Clean up preferences service so delay is always automatic (no toggle needed)

**Independent Test**: Run a profile with multiple tests and verify ~500ms delay between each test result

### Implementation for User Story 2

- [x] T006 [P] [US2] Remove `TestProgressDelayEnabled` property from `src/ReqChecker.App/Services/IPreferencesService.cs`
- [x] T007 [P] [US2] Remove `TestProgressDelayMs` property from `src/ReqChecker.App/Services/IPreferencesService.cs`
- [x] T008 [US2] Remove `TestProgressDelayEnabled` field, property, and partial method from `src/ReqChecker.App/Services/PreferencesService.cs`
- [x] T009 [US2] Remove `TestProgressDelayMs` field, property, and partial method from `src/ReqChecker.App/Services/Services/PreferencesService.cs`
- [x] T010 [US2] Remove `TestProgressDelayEnabled` and `TestProgressDelayMs` from `UserPreferences` DTO in `src/ReqChecker.App/Services/PreferencesService.cs`
- [x] T011 [US2] Remove delay-related lines from `Load()` method in `src/ReqChecker.App/Services/PreferencesService.cs`
- [x] T012 [US2] Remove delay-related lines from `Save()` method in `src/ReqChecker.App/Services/PreferencesService.cs`

**Checkpoint**: Preferences service no longer has delay-related properties - delay is automatic

---

## Phase 4: Polish & Verification

**Purpose**: Final verification and cleanup

- [x] T013 Build solution with `dotnet build` and verify no compilation errors
- [x] T014 Run existing tests with `dotnet test tests/ReqChecker.Infrastructure.Tests` to verify delay behavior unchanged
- [x] T015 Manual verification: Run app and confirm Demo Mode controls removed, delay still works

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies - start immediately
- **Phase 2 (US1)**: Depends on Phase 1 completion
- **Phase 3 (US2)**: Can run in parallel with Phase 2 (different files)
- **Phase 4 (Polish)**: Depends on all previous phases

### Task Dependencies

```
T001 (RunSettings default)
  ↓
T002-T005 (ViewModel/XAML cleanup) ←→ T006-T012 (Preferences cleanup)
  ↓                                      ↓
  ↓←─────────────────────────────────────┘
  ↓
T013-T015 (Verification)
```

### Parallel Opportunities

- T006 and T007 can run in parallel (different sections of same interface file)
- Phase 2 and Phase 3 can run in parallel (US1 touches ViewModel/XAML, US2 touches Services)

---

## Parallel Example

```bash
# After T001 completes, launch US1 and US2 in parallel:

# US1 - ViewModel/XAML (one developer):
T002: Remove Demo Mode controls from RunProgressView.xaml
T003-T005: Clean up RunProgressViewModel.cs

# US2 - Services (another developer, or sequentially):
T006-T007: Clean up IPreferencesService.cs
T008-T012: Clean up PreferencesService.cs
```

---

## Implementation Strategy

### Recommended Order (Single Developer)

1. **T001**: Change RunSettings default (1 line change)
2. **T002**: Remove XAML controls (biggest visual change)
3. **T003-T005**: Clean up ViewModel
4. **T006-T012**: Clean up Preferences (can be done in one file-editing session)
5. **T013-T015**: Verify everything works

### MVP Scope

All tasks are required for a complete implementation. This is a small feature (~73 lines removed) that should be completed as a single unit.

---

## Notes

- No new tests needed - existing `SequentialTestRunnerTests` verify delay behavior
- All changes are removals/simplifications, not additions
- Single source of truth for delay value: `RunSettings.InterTestDelayMs = 500`
- Old preferences files with delay values will be silently ignored (existing error handling)
