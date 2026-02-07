# Tasks: Security & Quality Hardening

**Input**: Design documents from `/specs/029-security-quality-hardening/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, quickstart.md

**Tests**: Not requested — manual verification per quickstart.md.

**Organization**: Tasks are grouped by user story. US1 requires a new foundational file (TestResultSanitizer) before the other US1 tasks. US2, US3, US4, US5, and US6 are fully independent of each other and can all run in parallel.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: User Story 1 — Sensitive Data Redaction (Priority: P1) 🎯 MVP

**Goal**: Strip sensitive fields (credentials, response bodies, file contents) from test results before persisting to history or exporting to reports. In-memory data remains unmodified for the Results page.

**Independent Test**: Run a test profile with HTTP auth and file read tests, then inspect `%LOCALAPPDATA%/ReqChecker/history.json` — should find `[REDACTED]` placeholders instead of raw data. Export to JSON and verify same redaction.

### Implementation for User Story 1

- [x] T001 [US1] Create `TestResultSanitizer` static class with `SanitizeForPersistence(RunReport)` method that deep-clones the report and redacts `Evidence.ResponseData`, `Evidence.FileContent`, sensitive `Evidence.ResponseHeaders` keys (`Authorization`, `Set-Cookie`, `X-Api-Key`, `Cookie`), and truncates `TechnicalDetails` to 500 chars in `src/ReqChecker.Core/Services/TestResultSanitizer.cs`
- [x] T002 [US1] Call `TestResultSanitizer.SanitizeForPersistence(report)` in `SaveRunAsync()` before adding to `_history` and serializing in `src/ReqChecker.Infrastructure/History/HistoryService.cs`
- [x] T003 [P] [US1] Call `TestResultSanitizer.SanitizeForPersistence()` on the RunReport before generating output in `src/ReqChecker.Infrastructure/Export/JsonExporter.cs`
- [x] T004 [P] [US1] Call `TestResultSanitizer.SanitizeForPersistence()` on the RunReport before generating output in `src/ReqChecker.Infrastructure/Export/CsvExporter.cs`
- [x] T005 [P] [US1] Call `TestResultSanitizer.SanitizeForPersistence()` on the RunReport before generating output in `src/ReqChecker.Infrastructure/Export/PdfExporter.cs`

**Checkpoint**: History JSON and all exports contain `[REDACTED]` placeholders instead of sensitive data. In-memory Results page still shows full details (SC-001).

---

## Phase 2: User Story 2 — Credential Prompt Wiring (Priority: P1)

**Goal**: Wire the existing `CredentialPromptViewModel` to the `SequentialTestRunner.PromptForCredentials` callback so credential-dependent tests trigger a user prompt. Handle cancellation by skipping the test with a clear reason.

**Independent Test**: Load a profile with a `credentialRef` parameter, run tests — a credential prompt dialog should appear. Submit credentials to authenticate, or cancel to see the test skipped with "Credentials not provided".

### Implementation for User Story 2

- [x] T006 [P] [US2] Add skip-on-cancel logic in `PromptForCredentialsIfNeededAsync()`: when the prompt callback returns `(null, null)`, return a sentinel or null that causes the test loop to produce a `Skipped` result with message "Credentials not provided" in `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs`
- [x] T007 [US2] Wire `PromptForCredentials` callback on `SequentialTestRunner` after `Services = services.BuildServiceProvider()` in `ConfigureServices()` — the callback dispatches to UI thread, creates `CredentialPromptViewModel`, shows a `ContentDialog`, uses `TaskCompletionSource` to bridge async, returns `(username, password)` on submit or `(null, null)` on cancel in `src/ReqChecker.App/App.xaml.cs`

**Checkpoint**: Credential-dependent tests trigger a prompt dialog. Submitting credentials passes them to the test runner. Cancelling skips the test with "Credentials not provided" (SC-002).

---

## Phase 3: User Story 3 — Path Traversal Prevention (Priority: P1)

**Goal**: Reject filenames containing path traversal characters (`..`, `/`, `\`) in profile delete and exists operations.

**Independent Test**: Code inspection — verify `DeleteProfile` and `ProfileExists` reject traversal filenames. `CopyProfileToUserDirectory` already safe (uses `Path.GetFileName`).

### Implementation for User Story 3

- [x] T008 [P] [US3] Add private `ValidateFileName(string fileName)` method that checks `Path.GetFileName(fileName) == fileName` and `!fileName.Contains("..")`, throwing `ArgumentException` on failure. Call it at the start of `DeleteProfile()` and `ProfileExists()` in `src/ReqChecker.Infrastructure/Profile/ProfileStorageService.cs`

**Checkpoint**: Path traversal filenames (`../../../important.json`, `..\..\file`, `/etc/passwd`) are rejected with `ArgumentException`. Valid filenames pass through (SC-003).

---

## Phase 4: User Story 4 — Resource Lifecycle Management (Priority: P2)

**Goal**: Fix event subscription leaks in ViewModels and dispose CancellationTokenSource after test execution.

**Independent Test**: Code inspection — verify all event subscriptions are unsubscribed in `Dispose()` and CTS is disposed after completion.

### Implementation for User Story 4

- [x] T009 [P] [US4] Replace anonymous lambda in `OnThemeServiceChanged()` with a stored `PropertyChangedEventHandler` field; unsubscribe from `ThemeService.PropertyChanged` in `Dispose()` in `src/ReqChecker.App/ViewModels/MainViewModel.cs`
- [x] T010 [P] [US4] Implement `IDisposable` on the class; add `Dispose()` method that unsubscribes `_appState.LastRunReportChanged -= OnLastRunReportChanged` in `src/ReqChecker.App/ViewModels/DiagnosticsViewModel.cs`
- [x] T011 [P] [US4] Add `Cts?.Dispose(); Cts = null;` in the `finally` block of `StartTestsAsync()` after `OnCompletion()` in `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs`

**Checkpoint**: No event subscription leaks. CancellationTokenSource disposed after every test run. Memory stable after 20+ page navigation cycles (SC-004).

---

## Phase 5: User Story 5 — Async File Operations (Priority: P2)

**Goal**: Replace synchronous `File.ReadAllText`/`File.WriteAllText` with async equivalents in HistoryService, removing unnecessary `Task.Run` wrappers.

**Independent Test**: Run tests and observe UI responsiveness during history save. Code inspection — verify no sync file I/O in HistoryService.

### Implementation for User Story 5

- [x] T012 [P] [US5] Replace `LoadHistoryAsync()` `Task.Run(() => { File.ReadAllText(...) })` pattern with direct `await File.ReadAllTextAsync()` calls for both main and backup files; remove `Task.Run` wrapper in `src/ReqChecker.Infrastructure/History/HistoryService.cs`
- [x] T013 [US5] Rename `SaveToFile()` to `async Task SaveToFileAsync()`; replace `File.WriteAllText()` with `await File.WriteAllTextAsync()`; update all callers (`SaveRunAsync`, `DeleteRunAsync`, `ClearHistoryAsync`) to `await SaveToFileAsync()` instead of `await Task.Run(() => SaveToFile())` in `src/ReqChecker.Infrastructure/History/HistoryService.cs`

**Checkpoint**: No `File.ReadAllText` or `File.WriteAllText` calls remain in HistoryService. UI remains responsive during save (SC-005).

---

## Phase 6: User Story 6 — Theme Consistency (Priority: P3)

**Goal**: Replace all 6 hardcoded `#0078D4` color values with dynamic theme resource references.

**Independent Test**: Toggle between dark and light themes — focus indicators on buttons, text fields, password boxes, checkboxes, and labels should use system accent color rather than fixed blue.

### Implementation for User Story 6

- [x] T014 [P] [US6] Replace all 6 instances of `#0078D4` (in `Stroke=` and `Value=` attributes) with `{DynamicResource SystemAccentColorPrimaryBrush}` or WPF-UI accent resource across Button, TextBox, PasswordBox, CheckBox, and Label focus visual styles in `src/ReqChecker.App/Resources/Styles/Theme.xaml`

**Checkpoint**: Zero hardcoded `#0078D4` in Theme.xaml. Focus indicators adapt to theme and system accent color (SC-006).

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all stories combined

- [x] T015 Build the project and verify no compilation errors
- [x] T016 Run quickstart.md end-to-end validation across all 6 user stories (Manual testing required - see quickstart.md for verification steps)

---

## Dependencies & Execution Order

### Phase Dependencies

- **US1 (Phase 1)**: No dependencies — but T001 must complete before T002-T005 (they consume the sanitizer)
- **US2 (Phase 2)**: No dependencies on other stories — T006 and T007 are sequential (T006 modifies runner, T007 wires callback)
- **US3 (Phase 3)**: No dependencies — single task, standalone file
- **US4 (Phase 4)**: No dependencies — all 3 tasks modify different files, fully parallel
- **US5 (Phase 5)**: **Depends on US1 (Phase 1)** — both modify `HistoryService.cs`, so US5 should run after US1's T002
- **US6 (Phase 6)**: No dependencies — standalone XAML file
- **Polish (Phase 7)**: Depends on all user stories being complete

### Parallel Opportunities

All user stories modify **different files** (except US1 and US5 share `HistoryService.cs`). Parallel groups:

```text
Parallel Group 1 (all independent, different files):
  T001           →  TestResultSanitizer.cs (NEW)     (US1 — foundational)

After T001 completes:
  T002           →  HistoryService.cs                 (US1 — sanitizer call)
  T003 + T004 + T005  →  Exporters (3 files)          (US1 — parallel)
  T006           →  SequentialTestRunner.cs            (US2)
  T008           →  ProfileStorageService.cs           (US3)
  T009           →  MainViewModel.cs                   (US4)
  T010           →  DiagnosticsViewModel.cs            (US4)
  T011           →  RunProgressViewModel.cs            (US4)
  T014           →  Theme.xaml                         (US6)

After T002 completes (HistoryService.cs freed):
  T012 + T013    →  HistoryService.cs                  (US5)

After T006 completes:
  T007           →  App.xaml.cs                        (US2)

Final:
  T015 + T016    →  Build + validate                   (after all)
```

---

## Implementation Strategy

### MVP First (User Stories 1-3: Security Fixes)

1. Implement US1 (sensitive data redaction) — highest security impact
2. Implement US2 + US3 in parallel — credential wiring + path traversal
3. **STOP and VALIDATE**: Test all 3 P1 stories independently
4. These 3 stories address all security findings from the code review

### Full Delivery

1. Complete US1 → US2 + US3 in parallel → Security fixes done
2. Complete US4 + US5 + US6 in parallel → Quality + polish done
3. Build + validate end-to-end → Ship

---

## Notes

- 16 tasks across 11 files (1 new, 10 modified)
- US1 and US5 share `HistoryService.cs` — must be sequenced (US1 first, US5 second)
- US2 has internal dependency: T006 (runner skip logic) before T007 (callback wiring)
- 3 Codex findings were false positives (RetryPolicy, encoding artifacts, virtualization) — no tasks generated
- FR-009 (retry resilience), FR-011 (encoding artifacts), FR-012 (virtualization) require no code changes per research.md
- No tests requested — validation is manual per quickstart.md
