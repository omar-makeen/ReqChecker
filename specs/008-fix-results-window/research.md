# Research: Fix Test Results Window

**Feature**: 008-fix-results-window
**Date**: 2026-01-31

## Summary

This is a bug fix feature with well-understood root causes. No external research required - all issues are internal implementation bugs with clear solutions following existing codebase patterns.

## Research Tasks

### 1. Data Loading Pattern

**Decision**: Use existing pattern from `NavigateToTestListWithProfile()` - get AppState from DI and set ViewModel properties before navigation.

**Rationale**: Consistent with established codebase conventions. The pattern already exists and works correctly for other navigation scenarios.

**Alternatives considered**:
- Constructor injection: Rejected - ViewModel already has constructor parameters, adding more would complicate DI registration
- Event-based loading: Rejected - More complex, unnecessary for this use case

### 2. Export Button Enable/Disable Pattern

**Decision**: Add `CanExport` property to ViewModel that returns `Report != null`, bind button `IsEnabled` to it.

**Rationale**: CommunityToolkit.Mvvm pattern. Simple, testable, follows MVVM principles.

**Alternatives considered**:
- Command CanExecute: Already implemented in export commands, but doesn't visually disable buttons
- View code-behind: Rejected - violates MVVM pattern used throughout codebase

### 3. Navigation Menu Active State Pattern

**Decision**: Set `NavResults.IsActive = true` in switch case, matching pattern used for other menu items.

**Rationale**: Existing code in `OnWindowLoaded()` uses this exact pattern for NavTests and NavProfiles.

**Alternatives considered**:
- NavigationView SelectedItem binding: More complex, would require refactoring MainWindow
- Custom attached behavior: Over-engineered for this simple fix

### 4. Empty State Handling

**Decision**: Verify existing empty state in ResultsView.xaml works when Report is null. Add if missing.

**Rationale**: View already has empty state for filtered results. Same pattern should apply for no-data state.

**Alternatives considered**: None - standard pattern already in use

## Codebase Verification

### Verified Patterns

1. **AppState injection pattern** (`NavigationService.cs:80`):
   ```csharp
   var appState = _serviceProvider.GetRequiredService<IAppState>();
   appState.SetCurrentProfile(profile);
   ```

2. **ViewModel property binding** (`ResultsViewModel.cs:31-32`):
   ```csharp
   [ObservableProperty]
   private RunReport? _report;
   ```

3. **Menu active state** (`MainWindow.xaml.cs:57`):
   ```csharp
   NavTests.IsActive = true;
   ```

## Conclusion

No NEEDS CLARIFICATION items remain. All implementation decisions follow existing codebase patterns with clear precedents.
