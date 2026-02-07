# Feature Specification: Security & Quality Hardening

**Feature Branch**: `029-security-quality-hardening`
**Created**: 2026-02-07
**Status**: Draft
**Input**: Comprehensive code review (health score 6.1/10) identifying P0 security vulnerabilities, P1 code quality issues, and P2 polish items across architecture, security, persistence, resource management, and UI quality.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sensitive Data Redaction in Test Evidence (Priority: P1)

When tests execute and produce results, sensitive data (credentials, response bodies containing tokens, file contents) must be stripped from test evidence before it is persisted to history or exported to reports. Currently, test implementations store raw response payloads, file contents, and credential data directly into TestResult objects, which are then saved verbatim to history files and PDF exports. A user running tests against authenticated endpoints should be confident that their credentials and sensitive response data are not written to disk in cleartext.

**Why this priority**: This is the highest-impact security issue. Persisted credentials and sensitive payloads on disk represent a data exposure risk. Any user running tests with credential-protected endpoints is unknowingly leaving secrets in plaintext JSON files.

**Independent Test**: Can be tested by running a test suite that includes credential-protected tests (e.g., HTTP with auth, FTP), then inspecting the history JSON file to confirm no passwords, tokens, or raw response bodies are present.

**Acceptance Scenarios**:

1. **Given** a test produces results containing sensitive data (credentials, response bodies, tokens), **When** the results are saved to history, **Then** all sensitive fields are redacted or excluded from the persisted file.
2. **Given** a test result contains credential information, **When** the result is exported to a report, **Then** credential values are replaced with redacted placeholders (e.g., "****").
3. **Given** a test produces a large response payload, **When** the result is stored, **Then** only a summary or truncated, non-sensitive excerpt is retained — not the full raw response.

---

### User Story 2 - Credential Prompt Pipeline Wiring (Priority: P1)

When a test profile contains tests that require user credentials (via `credentialRef` parameters), the application must prompt the user for those credentials before test execution. Currently, the credential prompt UI components exist in the codebase but are not wired into the application composition — the test runner's `PromptForCredentials` callback is never assigned. This means credential-dependent tests silently receive empty credentials, leading to false failures or misleading results.

**Why this priority**: Without this wiring, credential-dependent tests are fundamentally broken. Users see unexplained test failures with no prompt for required input, making the feature unusable for authenticated test scenarios.

**Independent Test**: Can be tested by loading a profile with a `credentialRef` parameter, running the tests, and verifying that a credential prompt dialog appears before the test executes.

**Acceptance Scenarios**:

1. **Given** a test profile contains a test with `credentialRef` configuration, **When** the user runs the test suite, **Then** a credential prompt dialog appears requesting username and password before that test executes.
2. **Given** the user provides credentials via the prompt, **When** the test executes, **Then** the credentials are passed to the test runner and the test can authenticate successfully.
3. **Given** the user cancels the credential prompt, **When** the prompt is dismissed, **Then** the test is skipped with a clear reason ("credentials not provided") rather than failing with a confusing error.
4. **Given** credentials were previously provided and stored, **When** the same test runs again, **Then** stored credentials are used without re-prompting.

---

### User Story 3 - Path Traversal Prevention in Profile Operations (Priority: P1)

When a user deletes a profile, the system must validate that the provided filename cannot escape the designated profiles directory. Currently, the `DeleteProfile` method directly combines user-supplied filenames with the base path, allowing potential path traversal attacks (e.g., `../../sensitive-file`). The system must sanitize all file path inputs to prevent directory escape.

**Why this priority**: Path traversal is a well-known security vulnerability (OWASP Top 10). While the attack surface is limited to local users, the fix is straightforward and the risk is real.

**Independent Test**: Can be tested by attempting to delete a profile with a path-traversal filename (e.g., `../../../important.json`) and verifying the operation is rejected.

**Acceptance Scenarios**:

1. **Given** a user attempts to delete a profile with a filename containing path traversal characters (`..`, `/`, `\`), **When** the delete operation is invoked, **Then** the operation is rejected and the file outside the profiles directory is not affected.
2. **Given** a user deletes a profile with a valid filename, **When** the delete operation completes, **Then** only the specified file within the profiles directory is removed.

---

### User Story 4 - Resource Lifecycle Management (Priority: P2)

Event subscriptions and disposable resources must be properly managed to prevent memory leaks. Currently, several ViewModels subscribe to events (PropertyChanged, state change callbacks) via lambdas and never unsubscribe. The CancellationTokenSource in the run ViewModel is never disposed. These leaks accumulate as users navigate between pages, potentially degrading performance over extended sessions.

**Why this priority**: Memory leaks from subscription and resource mismanagement degrade app stability over time. While not immediately visible, they cause gradual performance decline during extended use sessions.

**Independent Test**: Can be tested by navigating between pages repeatedly (20+ times) and monitoring memory usage — memory should remain stable rather than growing linearly.

**Acceptance Scenarios**:

1. **Given** a ViewModel subscribes to external events, **When** the ViewModel's associated view is unloaded or navigated away from, **Then** all event subscriptions are properly unsubscribed.
2. **Given** a test execution creates a cancellation token source, **When** the execution completes or is cancelled, **Then** the token source is disposed.
3. **Given** the user navigates between pages repeatedly, **When** monitoring memory usage, **Then** memory remains stable with no unbounded growth.

---

### User Story 5 - Async File Operations and Retry Resilience (Priority: P2)

File operations in the history service must use asynchronous APIs to avoid blocking the UI thread. Additionally, the retry policy must wrap individual test attempt exceptions so that a single test failure does not abort the entire test run.

**Why this priority**: Synchronous file I/O on async code paths can cause UI freezing during history saves. Unhandled retry exceptions cause entire test runs to abort unexpectedly, losing partial results.

**Independent Test**: Can be tested by running a large test suite while observing UI responsiveness during history save operations, and by running a suite with a test configured to fail — the remaining tests should still execute.

**Acceptance Scenarios**:

1. **Given** a test run completes and history is being saved, **When** the save operation runs, **Then** the UI remains responsive with no freezing or stuttering.
2. **Given** a test is configured with a retry policy and one attempt throws an exception, **When** the retry logic processes the failure, **Then** the exception is caught per-attempt and does not abort the remaining tests in the suite.
3. **Given** a retry policy is configured with multiple attempts, **When** an attempt fails with a transient error, **Then** subsequent retry attempts execute normally.

---

### User Story 6 - UI Quality and Theme Consistency (Priority: P3)

Hardcoded visual values (colors, font sizes) must be replaced with dynamic theme resources, text encoding artifacts must be fixed, and list virtualization must work correctly. These issues reduce visual consistency and performance.

**Why this priority**: While not functionally broken, hardcoded visual tokens prevent proper theme switching and encoding artifacts look unprofessional.

**Independent Test**: Can be tested by switching between dark and light themes and verifying all elements use the correct theme colors, and by checking list views for encoding artifacts in text.

**Acceptance Scenarios**:

1. **Given** the user switches between dark and light themes, **When** viewing any page, **Then** all visual elements use the active theme's colors with no hardcoded values leaking through.
2. **Given** a page displays list content, **When** the list has many items, **Then** the list uses proper virtualization and scrolls smoothly.
3. **Given** the user views any page with text content, **When** reading the text, **Then** no encoding artifacts (garbled characters, misrendered symbols) are visible.

---

### Edge Cases

- What happens when the history directory does not exist during async write? The system should create it on demand, just as it does today.
- What happens when a credential prompt is shown but the user closes the application? The cancellation token should propagate and the test run should terminate cleanly.
- What happens when the path traversal check encounters URL-encoded characters (e.g., `%2e%2e`)? The sanitization should normalize the path before validation.
- What happens when a test result contains binary data in its evidence? Binary data should be excluded entirely from persistence, not just truncated.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST redact sensitive data (credentials, tokens, raw response bodies) from test results before persisting to history files.
- **FR-002**: System MUST redact sensitive data from test results before exporting to reports.
- **FR-003**: System MUST wire the credential prompt UI to the test runner so that tests requiring credentials trigger a user prompt.
- **FR-004**: System MUST skip tests with a clear reason message when the user cancels or dismisses a credential prompt.
- **FR-005**: System MUST validate and sanitize profile filenames to prevent path traversal — rejecting any filename containing `..`, `/`, or `\` path separators.
- **FR-006**: System MUST unsubscribe from all external event subscriptions when ViewModels are no longer in use.
- **FR-007**: System MUST dispose CancellationTokenSource instances when test execution completes or is cancelled.
- **FR-008**: System MUST use asynchronous file operations for all history read/write operations — no synchronous file I/O on async code paths.
- **FR-009**: System MUST wrap individual test execution attempts in the retry policy with per-attempt exception handling so that failures do not abort the entire test run.
- **FR-010**: System MUST replace all hardcoded color/foreground values with dynamic theme resources.
- **FR-011**: System MUST fix text encoding artifacts in list views.
- **FR-012**: System MUST ensure list virtualization works correctly with ScrollViewer and ItemsControl combinations.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero sensitive data (passwords, tokens, raw response bodies) found in any history JSON file after a test run — verified by automated content scan.
- **SC-002**: 100% of credential-dependent tests trigger a user prompt when credentials are not already stored — verified by running a profile with `credentialRef` parameters.
- **SC-003**: Path traversal attempts (filenames containing `..`) are rejected 100% of the time — verified by attempting 5 different traversal patterns.
- **SC-004**: Memory usage remains within 10% of baseline after 20 page navigation cycles — verified by monitoring process memory.
- **SC-005**: UI remains responsive (no freeze >100ms) during history save operations — verified by interaction testing during save.
- **SC-006**: All visual elements render correctly in both dark and light themes with zero hardcoded color leakage — verified by visual inspection.

## Assumptions

- "Sensitive data" includes: passwords, authentication tokens, raw HTTP response bodies, file contents read during tests, and FTP connection details. Test metadata (test name, status, duration, error category/message) is not considered sensitive.
- The redaction approach will strip sensitive fields from TestResult objects before passing them to the history service and export service, rather than modifying the test implementations themselves.
- The credential prompt dialog already exists as `CredentialPromptViewModel` — only the wiring (connecting it to the test runner callback) is missing.
- Path sanitization uses filename-only validation (rejecting any input that is not a simple filename without directory separators) rather than complex path normalization.
- "Async file operations" means replacing `File.ReadAllText`/`File.WriteAllText` with their `Async` counterparts — no architectural changes to the history service interface.
