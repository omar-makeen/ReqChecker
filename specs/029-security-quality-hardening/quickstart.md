# Quickstart: 029-security-quality-hardening

## Files to Modify

1. **`src/ReqChecker.Core/Services/TestResultSanitizer.cs`** — **NEW** static utility to strip sensitive fields from TestResult before persistence/export
2. **`src/ReqChecker.Infrastructure/History/HistoryService.cs`** — Call sanitizer in SaveRunAsync; replace sync File.ReadAllText/WriteAllText with async equivalents
3. **`src/ReqChecker.Infrastructure/Export/JsonExporter.cs`** — Call sanitizer before exporting
4. **`src/ReqChecker.Infrastructure/Export/CsvExporter.cs`** — Call sanitizer before exporting
5. **`src/ReqChecker.Infrastructure/Export/PdfExporter.cs`** — Call sanitizer before exporting
6. **`src/ReqChecker.App/App.xaml.cs`** — Wire PromptForCredentials callback on SequentialTestRunner after DI build
7. **`src/ReqChecker.Infrastructure/Profile/ProfileStorageService.cs`** — Add ValidateFileName method; apply in DeleteProfile and ProfileExists
8. **`src/ReqChecker.App/ViewModels/MainViewModel.cs`** — Store theme PropertyChanged handler as field; unsubscribe in Dispose
9. **`src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs`** — Implement IDisposable; unsubscribe from LastRunReportChanged
10. **`src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`** — Dispose CancellationTokenSource after test completion
11. **`src/ReqChecker.App/Resources/Styles/Theme.xaml`** — Replace 6 hardcoded #0078D4 with DynamicResource accent color

## New Files

1. **`src/ReqChecker.Core/Services/TestResultSanitizer.cs`** — Static class with `SanitizeForPersistence(RunReport)` method

## Verification

### US1 — Sensitive Data Redaction
1. Run a test profile that includes HTTP tests with auth and file read tests
2. After completion, open `%LOCALAPPDATA%/ReqChecker/history.json`
3. Search for any passwords, raw response bodies, or file content — should find `[REDACTED]` placeholders instead
4. Export to JSON — same redaction should apply

### US2 — Credential Prompt Wiring
1. Load a profile with a `credentialRef` parameter
2. Run tests — a credential prompt dialog should appear
3. Enter credentials and submit — test should execute with those credentials
4. Cancel the dialog — test should be skipped with "credentials not provided" reason

### US3 — Path Traversal Prevention
1. (Code inspection) Verify DeleteProfile and ProfileExists reject filenames with `..`, `/`, or `\`
2. Build and confirm no compilation errors

### US4 — Resource Lifecycle
1. (Code inspection) Verify MainViewModel.Dispose unsubscribes from ThemeService.PropertyChanged
2. (Code inspection) Verify DiagnosticsViewModel.Dispose unsubscribes from LastRunReportChanged
3. (Code inspection) Verify CancellationTokenSource is disposed after test run

### US5 — Async File I/O
1. Run tests and observe UI responsiveness during history save
2. (Code inspection) Verify no File.ReadAllText or File.WriteAllText calls remain in HistoryService

### US6 — Theme Consistency
1. Open app, toggle between light and dark themes
2. Tab through buttons and text fields — focus indicators should use system accent color
3. (Code inspection) Verify no #0078D4 remains in Theme.xaml
