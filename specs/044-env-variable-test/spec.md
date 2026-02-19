# Feature Specification: EnvironmentVariable Test

**Feature Branch**: `044-env-variable-test`
**Created**: 2026-02-20
**Status**: Draft
**Input**: User description: "Add EnvironmentVariable test type for checking PATH, JAVA_HOME, etc."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Verify That a Named Environment Variable Exists (Priority: P1)

An IT administrator configures a profile test entry with an environment variable name (e.g., "JAVA_HOME"). When the test runs, the system reads the environment variable from the current machine. The test passes if the variable exists (is defined and non-empty) and fails if it is missing or empty. The results panel shows the variable name and its current value.

**Why this priority**: The most fundamental readiness question for environment variables is "does this variable exist?" Many deployments fail because a required variable (e.g., JAVA_HOME, DOTNET_ROOT) was never set. This story delivers the core value with a single required parameter.

**Independent Test**: Can be fully tested by adding a profile entry with `variableName: "PATH"` (universally present on Windows) and running the test. A pass result with the variable's value confirms the feature works end-to-end.

**Acceptance Scenarios**:

1. **Given** a test entry with `variableName` set to an environment variable that IS defined on the machine, **When** the test executes, **Then** the test passes and the summary shows the variable name and its value.
2. **Given** a test entry with `variableName` set to an environment variable that is NOT defined on the machine, **When** the test executes, **Then** the test fails with a clear message stating the variable was not found.
3. **Given** a test entry with `variableName` left empty or null, **When** the test executes, **Then** the test fails with a configuration error explaining that `variableName` is required.

---

### User Story 2 - Validate Environment Variable Value Against Expected Value (Priority: P2)

An IT administrator needs to verify that an environment variable not only exists but contains a specific value. They configure the test with `variableName` and `expectedValue`, and optionally set `matchType` to control how the comparison works. The test passes only if the variable exists and its value satisfies the match criteria.

**Why this priority**: After confirming presence, value validation is the next most valuable check. It prevents "variable exists but is misconfigured" surprises during deployment. This covers three match modes: exact, contains (substring), and regex pattern matching.

**Independent Test**: Can be tested by configuring a test with `variableName: "OS"` and `expectedValue: "Windows_NT"` with `matchType: "exact"` (should pass on Windows). Can also test `matchType: "contains"` with `expectedValue: "Windows"` and `matchType: "regex"` with `expectedValue: "^Windows.*"`.

**Acceptance Scenarios**:

1. **Given** a test entry with `variableName`, `expectedValue`, and `matchType: "exact"`, **When** the variable's value matches the expected value exactly, **Then** the test passes.
2. **Given** a test entry with `variableName`, `expectedValue`, and `matchType: "exact"`, **When** the variable's value does not match, **Then** the test fails with a message showing expected vs. actual value.
3. **Given** a test entry with `variableName`, `expectedValue`, and `matchType: "contains"`, **When** the variable's value contains the expected substring, **Then** the test passes.
4. **Given** a test entry with `variableName`, `expectedValue`, and `matchType: "contains"`, **When** the variable's value does not contain the expected substring, **Then** the test fails.
5. **Given** a test entry with `variableName`, `expectedValue`, and `matchType: "regex"`, **When** the variable's value matches the regex pattern, **Then** the test passes.
6. **Given** a test entry with `variableName`, `expectedValue`, and `matchType: "regex"`, **When** the regex pattern is invalid, **Then** the test fails with a configuration error describing the invalid pattern.
7. **Given** a test entry with `variableName` and `expectedValue` but no explicit `matchType`, **When** the test executes, **Then** it defaults to `matchType: "exact"`.

---

### User Story 3 - Check PATH-Style Variable for a Specific Directory Entry (Priority: P3)

An IT administrator needs to verify that a specific directory exists within a semicolon-delimited environment variable such as PATH. They configure the test with `variableName: "PATH"`, `expectedValue: "C:\Program Files\Java\jdk-17\bin"`, and `matchType: "pathContains"`. The system splits the variable's value by semicolons and checks if any entry matches the expected path (case-insensitive, with trailing-separator normalization).

**Why this priority**: PATH is the most commonly audited environment variable, and checking for a specific entry within it is a distinct use case from substring matching. A plain "contains" check could produce false positives (e.g., "C:\Java" matching inside "C:\JavaTools"), so path-aware splitting is needed.

**Independent Test**: Can be tested by configuring a test with `variableName: "PATH"`, `expectedValue: "C:\Windows\System32"`, and `matchType: "pathContains"` (should pass on any Windows machine).

**Acceptance Scenarios**:

1. **Given** a test entry with `matchType: "pathContains"` and the expected path IS present as a semicolon-delimited entry in the variable, **When** the test executes, **Then** the test passes.
2. **Given** a test entry with `matchType: "pathContains"` and the expected path is NOT present as a semicolon-delimited entry, **When** the test executes, **Then** the test fails with a message listing the expected path and indicating it was not found in the variable.
3. **Given** a test entry with `matchType: "pathContains"` and the expected path differs only by a trailing backslash (e.g., `C:\Tools\` vs `C:\Tools`), **When** the test executes, **Then** the comparison normalizes trailing separators and the test passes.

---

### Edge Cases

- What happens when the environment variable exists but has an empty string value? The existence-only check (no `expectedValue`) should treat an empty value as "not found" and fail the test, since an empty variable is functionally equivalent to missing.
- What happens when the `matchType` is set to an unrecognized value (e.g., "fuzzy")? The test should fail with a configuration error listing the valid match types: "exact", "contains", "regex", "pathContains".
- What happens when `matchType` is specified without `expectedValue`? The test should fail with a configuration error explaining that `expectedValue` is required when `matchType` is provided.
- What happens when the variable's value is very long (e.g., PATH with hundreds of entries)? The evidence should include the full value for diagnostic purposes, but the human-readable summary should truncate the displayed value to a reasonable length (e.g., 200 characters) with an ellipsis.
- What happens when the regex pattern in `expectedValue` takes excessively long to evaluate? The test should apply a reasonable timeout (e.g., 5 seconds) on regex evaluation and fail with an error if the pattern does not complete in time.
- What happens when `variableName` contains invalid characters or is just whitespace? The test should fail with a configuration error.
- What happens when the user cancels the test run while the EnvironmentVariable test is executing? The test should respect cancellation and report a skipped status, consistent with all other test types.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an `EnvironmentVariable` test type that reads environment variables from the current machine using `Environment.GetEnvironmentVariable`.
- **FR-002**: System MUST require a `variableName` parameter (string, non-empty) specifying the name of the environment variable to check.
- **FR-003**: System MUST fail with a configuration error if `variableName` is empty, null, or whitespace.
- **FR-004**: System MUST support an optional `expectedValue` parameter; when provided, the test validates the variable's value against it.
- **FR-005**: System MUST support an optional `matchType` parameter with valid values: "exact", "contains", "regex", "pathContains". When `expectedValue` is provided and `matchType` is omitted, the system MUST default to "exact".
- **FR-006**: System MUST fail with a configuration error if `matchType` is provided without `expectedValue`.
- **FR-007**: System MUST fail with a configuration error if `matchType` is set to an unrecognized value.
- **FR-008**: When `matchType` is "exact", the system MUST perform a case-insensitive string comparison between the variable's value and `expectedValue`.
- **FR-009**: When `matchType` is "contains", the system MUST perform a case-insensitive substring search for `expectedValue` within the variable's value.
- **FR-010**: When `matchType` is "regex", the system MUST evaluate `expectedValue` as a regular expression pattern against the variable's value, failing with a configuration error if the pattern is invalid.
- **FR-011**: When `matchType` is "pathContains", the system MUST split the variable's value by semicolons and check if any resulting entry matches `expectedValue` using case-insensitive comparison with trailing path-separator normalization.
- **FR-012**: When no `expectedValue` is provided, the test MUST pass if the variable exists and has a non-empty value, and fail otherwise.
- **FR-013**: System MUST populate test evidence with structured data including: variable name, whether the variable was found, the actual value, match type used, expected value (if any), and match result.
- **FR-014**: System MUST truncate the displayed value in the human-readable summary to 200 characters (with ellipsis) when the value exceeds that length, while preserving the full value in the evidence data.
- **FR-015**: System MUST produce a human-readable summary describing the outcome (e.g., "JAVA_HOME is set to 'C:\Java\jdk-17'", "PATH contains 'C:\Tools\bin'", "JAVA_HOME is not defined").
- **FR-016**: System MUST assign an appropriate icon and color category for the `EnvironmentVariable` type in the test list UI, consistent with existing system-level tests.
- **FR-017**: System MUST include sample `EnvironmentVariable` test entries in the default profile (e.g., a PATH existence check and a JAVA_HOME check).
- **FR-018**: System MUST NOT require administrator privileges to read environment variables.

### Key Entities

- **EnvironmentVariable Test Entry**: A profile test configuration with type "EnvironmentVariable", containing parameters `variableName` (required string), `expectedValue` (optional string), and `matchType` (optional string: "exact", "contains", "regex", "pathContains"). Configured via the standard profile JSON format with field policies.
- **EnvironmentVariable Evidence**: Captured data including the variable name, whether it was found, actual value, match type, expected value, and match result. Serialized as JSON evidence in the test result.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can verify any environment variable exists on the system and receive a clear pass/fail result within 1 second of execution (local operation, no network involved).
- **SC-002**: Users can validate environment variable values using exact, contains, regex, or path-contains matching, with each mode producing accurate results and no false positives.
- **SC-003**: PATH-style semicolon-delimited variables can be checked for specific directory entries with correct path normalization (trailing separators, case insensitivity).
- **SC-004**: Test results clearly show the variable name, whether it was found, and match details in both the summary and evidence panels.
- **SC-005**: All existing tests continue to pass without modification after the EnvironmentVariable test type is added (zero regressions).

## Assumptions

- The primary detection method is `Environment.GetEnvironmentVariable`, which reads from the current process's environment block. This reflects the effective merged view of system and user variables as seen by the running process.
- Path-style variable splitting uses the semicolon delimiter, which is the standard on Windows. Cross-platform path separator differences are out of scope since the application targets Windows only.
- The "exact" and "contains" match modes are case-insensitive because Windows environment variable values are conventionally treated as case-insensitive (especially for paths).
- Regex matching uses the .NET `Regex` class with a default timeout to prevent catastrophic backtracking.
- Environment variable names are case-insensitive on Windows; the system relies on `Environment.GetEnvironmentVariable` which handles this natively.
