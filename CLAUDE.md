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
- N/A (UI-only enhancement, uses existing in-memory data from IAppState) (016-premium-diagnostics-page)
- N/A (in-memory only, refreshed on navigation) (017-diagnostics-auto-load)
- N/A (UI-only feature, no data persistence) (019-premium-buttons)
- N/A (in-memory via IAppState) (020-premium-test-execution)
- N/A (UI-only fix, theme preference already persisted via IPreferencesService) (021-premium-light-theme)
- C# 12 / .NET 8.0-windows + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, QuestPDF (for PDF generation) (022-export-results)
- N/A (file export to user-selected location) (022-export-results)
- C# 12 / .NET 8.0-windows + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, System.Text.Json (023-test-history)
- JSON files in `%APPDATA%/ReqChecker/history/` (023-test-history)
- N/A (UI-only changes) (024-history-empty-state)
- N/A (code removal) (025-remove-trend-chart)
- N/A (in-memory RunReport via IAppState) (027-premium-results)
- N/A (in-memory only) (028-premium-test-execution-ux)
- JSON files in `%LOCALAPPDATA%/ReqChecker/` (history.json), Windows Credential Manager (credentials) (029-security-quality-hardening)
- C# 12 / .NET 8.0-windows (net8.0-windows TFM) + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (030-selective-test-run)
- N/A (in-memory session-only selection state; no persistence) (030-selective-test-run)
- N/A (UI-only fix, no data persistence changes) (031-fix-checkbox-ux)
- C# 12 / .NET 8.0-windows (net8.0-windows TFM) + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, FluentValidation, Microsoft.Extensions.DependencyInjection (032-test-dependencies)
- N/A (in-memory session-only; `dependsOn` persisted in profile JSON files) (032-test-dependencies)
- C# 12 / .NET 8.0-windows (net8.0-windows TFM) + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages) (033-new-test-types)
- N/A (test results are in-memory; parameters persisted in profile JSON files) (033-new-test-types)
- N/A (in-memory session-only; reuses IAppState.SelectedTestIds) (034-rerun-failed-tests)
- N/A (in-memory filtering on the already-loaded test collection) (035-test-list-search)
- C# 12 / .NET 8.0-windows (net8.0-windows TFM) + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0 (all existing — no new packages) (036-test-config-ux)
- N/A (UI-only enhancement, no data persistence changes) (036-test-config-ux)
- `%APPDATA%/ReqChecker/preferences.json` (existing `IPreferencesService` / `PreferencesService`) (038-settings-window)
- Profile JSON files (System.Text.Json / JsonObject) (040-mtls-config-credentials)
- N/A (in-memory test results; parameters persisted in profile JSON files) (041-cert-expiry-test)
- C# 12 / .NET 8.0 LTS (net8.0 / net8.0-windows) + MSBuild (build system), GitHub Actions (CI/CD) — no new runtime packages (045-conditional-test-builds)
- N/A (build-time only; no runtime data changes) (045-conditional-test-builds)

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
- 046-hardware-tests: Added C# 12 / .NET 8.0-windows (net8.0-windows TFM) + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
- 045-conditional-test-builds: Added C# 12 / .NET 8.0 LTS (net8.0 / net8.0-windows) + MSBuild (build system), GitHub Actions (CI/CD) — no new runtime packages
- 043-installed-software-test: Added C# 12 / .NET 8.0-windows (net8.0-windows TFM) + WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)



<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
