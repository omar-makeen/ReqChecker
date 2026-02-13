# Feature Specification: Move mTLS Credentials to Test Configuration

**Feature Branch**: `040-mtls-config-credentials`
**Created**: 2026-02-13
**Status**: Draft
**Input**: User description: "mTLS some in cases open dialog to ask for username and password, there is still issue in dialog as when click submit not appear and keep show up, why username and password not part of test configuration? to avoid dialog issue, please check and feedback and if this feasible and logic write spec"

## Problem Statement

The mTLS test type currently prompts users for credentials via a modal dialog at runtime. This dialog has persistent usability issues (Submit button not working reliably due to WPF command binding complexities). Additionally, the dialog asks for both username and password, but the mTLS test only uses the password field as the PFX certificate passphrase — the username field is never used.

Rather than continuing to fix the dialog, this feature eliminates the runtime prompt entirely by moving the PFX passphrase into the test configuration parameters. This is the standard approach: users configure the passphrase when they set up the test, and it is used automatically at execution time — no interruption, no dialog, no blocking UI issues.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Configure PFX Passphrase in Test Parameters (Priority: P1)

As a user configuring an mTLS test, I want to enter the PFX passphrase directly in the test configuration (alongside the certificate path, URL, etc.) so that the test runs without any runtime prompts.

**Why this priority**: This is the core change. It eliminates the broken dialog and provides a seamless test execution experience.

**Independent Test**: Can be fully tested by creating an mTLS test with a `pfxPassword` parameter, running it, and confirming it executes without prompting — delivering uninterrupted test execution.

**Acceptance Scenarios**:

1. **Given** an mTLS test definition with a `pfxPassword` parameter configured, **When** the user runs the test, **Then** the test executes using the configured passphrase without showing any credential dialog.
2. **Given** an mTLS test definition with a `pfxPassword` parameter configured, **When** the test executes, **Then** the passphrase is passed to the PFX certificate loader and the test completes (pass or fail based on connectivity).
3. **Given** an mTLS test definition with `pfxPassword` left empty or omitted, **When** the test executes, **Then** the system attempts to load the certificate without a passphrase (some PFX files have no password).

---

### User Story 2 - Edit PFX Passphrase via Test Configuration UI (Priority: P2)

As a user editing an mTLS test in the configuration panel, I want to see and edit the PFX passphrase field alongside other test parameters so that I can manage all test settings in one place.

**Why this priority**: Without a UI entry point, users would need to manually edit the JSON profile. Providing a UI field makes the feature accessible.

**Independent Test**: Can be tested by opening an mTLS test configuration, verifying a password field for PFX passphrase appears, entering a value, and confirming it persists.

**Acceptance Scenarios**:

1. **Given** the user opens the configuration for an mTLS test, **When** the configuration panel loads, **Then** a "PFX Password" field is displayed as a password-masked input.
2. **Given** the user enters a PFX passphrase in the configuration field, **When** the user saves or the field loses focus, **Then** the passphrase is stored in the test definition parameters.
3. **Given** the user re-opens the configuration for a previously configured mTLS test, **When** the configuration panel loads, **Then** the PFX password field shows a masked representation indicating a value is set (not the plaintext).

---

### User Story 3 - Remove Runtime Credential Dialog for mTLS (Priority: P1)

As a user running mTLS tests, I no longer want to be interrupted by a credential prompt dialog so that test execution flows smoothly from start to finish.

**Why this priority**: This directly resolves the reported bug — the dialog that won't close. Removing the dialog is the fix.

**Independent Test**: Can be tested by running an mTLS test suite and confirming no dialog appears during execution.

**Acceptance Scenarios**:

1. **Given** an mTLS test with `pfxPassword` configured, **When** the test run begins, **Then** no credential dialog is shown.
2. **Given** a profile with multiple mTLS tests, **When** all tests are executed, **Then** all tests run sequentially without any user interaction required.
3. **Given** an mTLS test that previously had a `credentialRef` parameter, **When** the profile is loaded, **Then** the test still works (backward compatibility: `credentialRef` is ignored if `pfxPassword` is present).

---

### Edge Cases

- What happens when `pfxPassword` is wrong? The test should fail with a clear error message indicating the passphrase is incorrect (not a cryptic certificate loading error).
- What happens when a PFX file has no passphrase and `pfxPassword` is provided? The system should attempt to load with the given passphrase; standard certificate loading behavior applies.
- What happens when `pfxPassword` is configured but the PFX file path is invalid? The test should fail with a "certificate file not found" error, same as today.
- What happens with profiles that still use `credentialRef`? Backward compatibility: if `pfxPassword` is present, use it directly and skip the credential prompt flow. If only `credentialRef` is present (no `pfxPassword`), the existing credential prompt flow remains as a fallback.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support a `pfxPassword` parameter in mTLS test definitions that provides the PFX certificate passphrase.
- **FR-002**: System MUST use the `pfxPassword` parameter value (when present) to load the PFX certificate, bypassing the runtime credential prompt entirely.
- **FR-003**: System MUST treat an empty or omitted `pfxPassword` as "no passphrase" and attempt to load the certificate without one.
- **FR-004**: System MUST display the `pfxPassword` field as a password-masked input in the test configuration UI.
- **FR-005**: System MUST remove the `credentialRef` parameter from default mTLS test profiles and replace it with `pfxPassword`.
- **FR-006**: System MUST maintain backward compatibility: if a profile contains `credentialRef` without `pfxPassword`, the existing credential prompt flow continues to work.
- **FR-007**: System MUST remove the username field requirement — mTLS tests only need the PFX passphrase, not a username.
- **FR-008**: System MUST provide a clear error message when the PFX passphrase is incorrect (e.g., "PFX passphrase is incorrect. Check the pfxPassword parameter.").

### Key Entities

- **TestDefinition.Parameters**: Extended with optional `pfxPassword` field for mTLS test type. This is a string value stored in the profile JSON alongside other test parameters like `clientCertPath` and `url`.
- **FieldPolicy**: The `pfxPassword` field should have an `Editable` policy to allow users to modify it in the configuration UI. The `credentialRef` field policy becomes `Hidden` or is removed from new profiles.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can configure and run an mTLS test end-to-end without encountering any modal dialog or runtime prompt.
- **SC-002**: 100% of mTLS test executions with a configured `pfxPassword` complete without user interaction.
- **SC-003**: Users can configure the PFX passphrase in under 30 seconds through the test configuration UI.
- **SC-004**: Existing profiles with `credentialRef` continue to function without modification (backward compatibility).

## Clarifications

### Session 2026-02-13

- Q: How should the PFX passphrase be stored in the profile JSON? → A: Plaintext in JSON — simple, consistent with existing parameters, acceptable for local-only files.

## Assumptions

- PFX passphrases are stored as plaintext in profile JSON files (decided). Profile files are local to the user's machine and not shared publicly. This is consistent with how other test parameters (URLs, hostnames, ports) are stored. No encryption or obfuscation is applied.
- The username field in the credential prompt was never used by MtlsConnectTest and can be safely dropped from the mTLS flow.
- Users prefer configuration-time setup over runtime prompts for test credentials.
- The existing `CredentialPromptDialog` and `CredentialPromptViewModel` should remain in the codebase for potential future use by other test types, but the mTLS flow will no longer trigger them.
