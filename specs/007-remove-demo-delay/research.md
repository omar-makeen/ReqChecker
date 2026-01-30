# Research: Remove Demo Mode UI Control

**Feature**: 007-remove-demo-delay
**Date**: 2026-01-31

## Overview

This is a straightforward UI simplification task. No external research was required.

## Decisions

### 1. Single Source of Truth for Delay Value

**Decision**: Use `RunSettings.InterTestDelayMs` default value (change from 0 to 500)

**Rationale**:
- `RunSettings` already exists and is used by `SequentialTestRunner`
- The property already has a default value mechanism
- Changing the default requires modification in exactly one place
- No new classes or patterns needed

**Alternatives Considered**:
- **Dedicated constants class**: Overkill for a single value
- **App configuration file**: Over-engineering for a fixed value
- **Keep in PreferencesService**: Unnecessary since UI is removed

### 2. Preferences Cleanup Strategy

**Decision**: Remove `TestProgressDelayEnabled` and `TestProgressDelayMs` entirely from preferences

**Rationale**:
- These properties exist only to support the Demo Mode UI controls
- With UI removed, there's no way for users to change these values
- Keeping them adds dead code and maintenance burden
- Simpler is better - if you can't configure it, don't store it

**Migration**: None needed - old preferences files with these values will be silently ignored during load (existing error handling covers unknown properties)

### 3. ViewModel Simplification

**Decision**: Remove properties and use `new RunSettings()` directly

**Rationale**:
- `RunProgressViewModel` no longer needs to delegate to preferences
- Creating a default `RunSettings` object gets the 500ms value automatically
- Removes conditional logic (`TestProgressDelayEnabled ? TestProgressDelayMs : 0`)

## Files Analysis

| File | Current State | Target State |
|------|---------------|--------------|
| `RunSettings.cs` | `InterTestDelayMs = 0` | `InterTestDelayMs = 500` |
| `IPreferencesService.cs` | Has delay properties | Remove delay properties |
| `PreferencesService.cs` | Has delay properties + persistence | Remove delay properties |
| `RunProgressViewModel.cs` | Has delay properties + conditional logic | Remove properties, use default |
| `RunProgressView.xaml` | Has Demo Mode UI controls | Remove UI controls |

## Risk Assessment

**Risk Level**: Low

- No new code added, only code removed
- Existing unit tests for delay behavior still valid (SequentialTestRunnerTests)
- No database migrations or external dependencies
- Backwards compatible (old preferences files ignored gracefully)
