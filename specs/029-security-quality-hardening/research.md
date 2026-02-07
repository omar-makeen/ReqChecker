# Research: Security & Quality Hardening

**Feature**: 029-security-quality-hardening
**Date**: 2026-02-07

## Decision Log

### D-001: Redaction Strategy — Sanitize at Persistence Boundary

**Options Considered**:
1. Modify each test implementation to stop storing sensitive data in TestEvidence
2. Create a sanitization step that strips sensitive fields from TestResult before persistence/export
3. Add a custom JSON serializer that excludes sensitive properties

**Decision**: Option 2 — Create a `TestResultSanitizer` utility class that clones/strips sensitive fields from TestResult objects. Called in `HistoryService.SaveRunAsync()` before serialization, and in each exporter before generating output.

**Rationale**: Option 1 would require modifying 6+ test classes and risks breaking test diagnostics (the data is useful at runtime). Option 3 is brittle (adding a new sensitive field requires serializer changes). Option 2 is a single choke point applied at the two persistence boundaries (history save + export).

**Sensitive fields to redact** (from `TestEvidence`):
- `ResponseData` → replace with `"[REDACTED — {length} chars]"`
- `ResponseHeaders` → remove `Authorization`, `Set-Cookie`, `X-Api-Key` headers; keep safe headers like `Content-Type`
- `FileContent` → replace with `"[REDACTED — {length} chars]"`
- `RegistryValue` → keep as-is (not sensitive)
- `ProcessList` → keep as-is (not sensitive)

Also redact from `TestResult`:
- `TechnicalDetails` → truncate to 500 chars, strip anything matching credential patterns

### D-002: Credential Prompt Wiring — Callback Assignment in DI

**Current State**: `SequentialTestRunner` has a `PromptForCredentials` property (line 24) that is `Func<string, string, string?, Task<(string?, string?)>>?`. It's never assigned — see `App.xaml.cs` lines 191-195 where the runner is created with only `tests` and `credentialProvider`.

**Decision**: After building the `ServiceProvider`, resolve `ITestRunner`, cast to `SequentialTestRunner`, and assign the `PromptForCredentials` callback. The callback will show the existing `CredentialPromptViewModel` in a `ContentDialog`.

**Implementation**: In `App.xaml.cs` after `Services = services.BuildServiceProvider()`:
```
var runner = Services.GetRequiredService<ITestRunner>() as SequentialTestRunner;
runner.PromptForCredentials = async (label, credRef, _) => { /* show dialog */ };
```

The `CredentialPromptViewModel` already has `Submit`/`Cancel` commands and `OnCredentialsSubmitted`/`OnCancelled` callbacks. We wire a `TaskCompletionSource` to bridge the async gap.

### D-003: Path Traversal — Filename-Only Validation

**Current State**: `ProfileStorageService.DeleteProfile()` (line 98) does `Path.Combine(_profilesPath, fileName)` with no validation. `ProfileExists()` (line 119) has the same pattern.

**Decision**: Add a private `ValidateFileName(string fileName)` method that:
1. Checks `Path.GetFileName(fileName) == fileName` (rejects any path separators)
2. Checks `!fileName.Contains("..")` (rejects traversal sequences)
3. Throws `ArgumentException` on failure

Apply to `DeleteProfile` and `ProfileExists`. `CopyProfileToUserDirectory` already uses `Path.GetFileName(sourcePath)` at line 78, which extracts just the filename — safe.

### D-004: Resource Lifecycle — IDisposable Pattern

**MainViewModel** (line 88-103): The `OnThemeServiceChanged` partial method subscribes a lambda to `ThemeService.PropertyChanged` — this lambda captures `this` and can never be unsubscribed because it's anonymous. `MainViewModel` already implements `IDisposable` (line 13) and `Dispose()` (line 144).

**Decision**: Store the lambda as a field `PropertyChangedEventHandler _themePropertyChangedHandler` so it can be unsubscribed in `Dispose()`.

**DiagnosticsViewModel** (line 50): Subscribes `_appState.LastRunReportChanged += OnLastRunReportChanged` but never unsubscribes. Does not implement `IDisposable`.

**Decision**: Implement `IDisposable` on `DiagnosticsViewModel` and unsubscribe in `Dispose()`.

**RunProgressViewModel** (line 121): `CancellationTokenSource` property `Cts` is never disposed.

**Decision**: Add `Cts?.Dispose()` in the `finally` block of `StartTestsAsync()` after `OnCompletion()`, and ensure `OnCompletion` runs before disposal.

### D-005: Async File I/O — Replace Sync Methods

**Current State**: `HistoryService` uses `File.ReadAllText` (line 61), `File.ReadAllText` (line 85 for backup), and `File.WriteAllText` (line 304) wrapped in `Task.Run()`.

**Decision**: Replace the `Task.Run(() => { ... File.ReadAllText ... })` pattern in `LoadHistoryAsync()` with `File.ReadAllTextAsync()` directly (remove the `Task.Run` wrapper). Replace `SaveToFile()` synchronous method with `SaveToFileAsync()` using `File.WriteAllTextAsync()`.

This eliminates unnecessary thread pool usage and simplifies the code.

### D-006: RetryPolicy — Already Correct

**Finding**: The Codex review flagged retry exception handling, but `RetryPolicy.ExecuteWithRetryAsync()` (lines 35-60) already properly handles failures per-attempt. The `test.ExecuteAsync()` call is not wrapped in try/catch because individual test implementations catch their own exceptions and return `TestResult` with `Status = Fail`. The retry loop checks `result.Status == TestStatus.Pass` and only retries on failure.

**Decision**: No changes needed to RetryPolicy. The Codex review finding was inaccurate.

### D-007: Theme Hardcoded Colors — Replace with DynamicResource

**Current State**: `Theme.xaml` has 6 instances of hardcoded `#0078D4` (Windows accent blue) used in focus visual styles for Button, TextBox, PasswordBox, CheckBox, and Label, plus a keyboard focus trigger.

**Decision**: Replace all 6 instances of `#0078D4` with `{DynamicResource SystemAccentColorPrimary}` or the WPF-UI equivalent accent resource. This ensures focus indicators follow the system accent color and adapt to theme changes.

### D-008: Encoding Artifacts — False Positive

**Finding**: The Codex review flagged encoding artifacts in `HistoryView.xaml:281` and `TestListView.xaml:301`. Investigation reveals these are `" • "` (Unicode bullet U+2022) used intentionally as visual separators between metadata fields. No encoding artifacts exist.

**Decision**: No changes needed. The bullet characters render correctly.

### D-009: Virtualization — Already Implemented

**Finding**: All list views already have proper virtualization:
- `HistoryView.xaml`: `VirtualizingPanel.IsVirtualizing="True"`, `VirtualizationMode="Recycling"`, `ScrollUnit="Pixel"`
- `TestListView.xaml`: Same pattern
- `ResultsView.xaml`: Same pattern
- `RunProgressView.xaml`: Same pattern

**Decision**: No changes needed. Virtualization is correctly implemented.

## Files to Modify

| File | User Story | Change |
|------|-----------|--------|
| `src/ReqChecker.Core/Services/TestResultSanitizer.cs` | US1 | **NEW** — Sanitize sensitive fields from TestResult/TestEvidence |
| `src/ReqChecker.Infrastructure/History/HistoryService.cs` | US1, US5 | Call sanitizer before save; replace sync I/O with async |
| `src/ReqChecker.Infrastructure/Export/JsonExporter.cs` | US1 | Call sanitizer before export |
| `src/ReqChecker.Infrastructure/Export/CsvExporter.cs` | US1 | Call sanitizer before export |
| `src/ReqChecker.Infrastructure/Export/PdfExporter.cs` | US1 | Call sanitizer before export |
| `src/ReqChecker.App/App.xaml.cs` | US2 | Wire PromptForCredentials callback after DI build |
| `src/ReqChecker.Infrastructure/Profile/ProfileStorageService.cs` | US3 | Add filename validation to DeleteProfile and ProfileExists |
| `src/ReqChecker.App/ViewModels/MainViewModel.cs` | US4 | Store theme PropertyChanged handler; unsubscribe in Dispose |
| `src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs` | US4 | Implement IDisposable; unsubscribe LastRunReportChanged |
| `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` | US4 | Dispose CancellationTokenSource |
| `src/ReqChecker.App/Resources/Styles/Theme.xaml` | US6 | Replace #0078D4 with DynamicResource accent |

## Files NOT Modified (False Positives)

| File | Codex Finding | Why No Change |
|------|--------------|---------------|
| `RetryPolicy.cs` | Retry exception handling | Already correctly handles per-attempt failures |
| `HistoryView.xaml` | Encoding artifacts | Bullet character is intentional separator |
| `TestListView.xaml` | Encoding artifacts | Bullet character is intentional separator |
| All list views | Virtualization | Already properly implemented |
