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
- 007-remove-demo-delay: Added C# 12 / .NET 8.0-windows + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
- 006-test-progress-delay: Added C# 12 / .NET 8.0-windows (net8.0-windows) + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection 10.0.2
- 005-fix-run-progress: Added C# 12 / .NET 8.0-windows + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection



<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
