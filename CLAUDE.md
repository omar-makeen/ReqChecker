# ReqChecker Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-01-30

## Active Technologies
- C# 12 / .NET 8.0 LTS + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0 (002-ui-ux-redesign)
- %APPDATA%/ReqChecker/preferences.json (user preferences) (002-ui-ux-redesign)
- C# 12 / .NET 8.0-windows + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (004-auto-load-config)
- File system (startup-profile.json alongside executable), Embedded resources (sample profile) (004-auto-load-config)
- N/A (UI-only fix) (005-fix-run-progress)
- C# 12 / .NET 8.0-windows (net8.0-windows) + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2 (006-test-progress-delay)
- `%APPDATA%/ReqChecker/preferences.json` (JSON via System.Text.Json) (006-test-progress-delay)
- N/A (removing preferences, using hardcoded default) (007-remove-demo-delay)
- In-memory (session-only via IAppState) (008-fix-results-window)
- N/A (UI-only fix, state is in-memory via IAppState) (009-nav-selection-sync)
- N/A (in-memory TestResult objects) (010-result-details)
- N/A (in-memory TestDefinition objects from profile JSON) (013-improve-test-config)
- C# 12 / .NET 8.0-windows + WPF-UI 4.2.0 (Fluent icons), CommunityToolkit.Mvvm 8.4.0 (014-improve-test-list)
- N/A (UI-only enhancement, uses existing TestDefinition data) (014-improve-test-list)
- C# 12 / .NET 8.0-windows + WPF-UI 4.2.0 (SymbolRegular Fluent icons), CommunityToolkit.Mvvm 8.4.0 (015-improve-page-titles)
- N/A (UI-only enhancement) (015-improve-page-titles)

- C# 12 / .NET 8.0 LTS (001-reqchecker-desktop-app)

## Project Structure

```text
src/
tests/
```

## Commands

# Add commands for C# 12 / .NET 8.0 LTS

## Code Style

C# 12 / .NET 8.0 LTS: Follow standard conventions

## Recent Changes
- 015-improve-page-titles: Added C# 12 / .NET 8.0-windows + WPF-UI 4.2.0 (SymbolRegular Fluent icons), CommunityToolkit.Mvvm 8.4.0
- 014-improve-test-list: Added C# 12 / .NET 8.0-windows + WPF-UI 4.2.0 (Fluent icons), CommunityToolkit.Mvvm 8.4.0
- 013-improve-test-config: Added C# 12 / .NET 8.0-windows + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection



<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
