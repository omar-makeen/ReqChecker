# Research: Move mTLS Credentials to Test Configuration

**Feature**: 040-mtls-config-credentials
**Date**: 2026-02-13

## R1: How MtlsConnectTest uses credentials today

**Decision**: The test already reads `context?.Password` and passes it directly to `new X509Certificate2(certPath, password, ...)`. No username is used. The password field maps 1:1 to the PFX passphrase.

**Rationale**: The `Password` property on `TestExecutionContext` is the sole bridge between the credential prompt and the PFX loader. Moving the passphrase to `pfxPassword` in parameters means the test can read it directly from parameters, eliminating the need for `TestExecutionContext` entirely for mTLS.

**Alternatives considered**:
- Add a `PfxPassword` property to `TestExecutionContext` — rejected because the goal is to eliminate the runtime context/prompt entirely and read from config.
- Keep using `credentialRef` + `TestExecutionContext` — rejected because it routes through the broken credential dialog.

## R2: How SequentialTestRunner decides to prompt

**Decision**: The runner checks `testDefinition.Parameters.ContainsKey("credentialRef")`. If found, it prompts. The fix is to add a check for `pfxPassword` *before* the `credentialRef` check: if `pfxPassword` exists in parameters, create a `TestExecutionContext` directly from it (no prompt, no dialog). Fall through to `credentialRef` only if `pfxPassword` is absent.

**Rationale**: This preserves backward compatibility. Old profiles with `credentialRef` still trigger the prompt. New profiles with `pfxPassword` bypass it entirely. Both paths produce the same `TestExecutionContext` output.

**Alternatives considered**:
- Remove `credentialRef` support entirely — rejected because existing user profiles would break.
- Have `MtlsConnectTest` read `pfxPassword` from parameters directly (bypassing `TestExecutionContext`) — rejected because the test runner already has the credential-checking responsibility; keeping it there maintains separation of concerns.

## R3: Test configuration UI field rendering

**Decision**: The test config UI renders editable fields as `TextBox` controls via an `ItemsControl` with `DataTemplate`. There is no existing `PasswordBox` support. To display `pfxPassword` as a masked field, the `DataTemplate` needs a conditional `PasswordBox` for password-type parameters.

**Rationale**: The `FieldPolicyType` enum controls visibility (Hidden, Editable, Locked, PromptAtRun), but does not distinguish text vs password input types. A new mechanism is needed to indicate that a parameter should render as a `PasswordBox`.

**Decision on approach**: Use a naming convention — if the parameter name ends with `Password` (case-insensitive), render as `PasswordBox` instead of `TextBox`. This avoids adding a new enum value or field metadata.

**Alternatives considered**:
- Add `FieldPolicyType.Password` enum value — rejected because field policy controls visibility/editability, not input type. Mixing concerns.
- Add a separate `fieldType` dictionary in `TestDefinition` — rejected as over-engineering for a single use case.
- Naming convention (`*Password`) — chosen because it's simple, zero-config, and self-documenting.

## R4: Default profile changes

**Decision**: Replace `credentialRef` with `pfxPassword` in default mTLS test entries. Set `pfxPassword` field policy to `Editable`. Remove `credentialRef` from default profiles entirely.

**Rationale**: New installations should use the simpler, dialog-free approach. The `credentialRef` fallback exists only for backward compatibility with user-modified profiles.

## R5: Passphrase storage security

**Decision**: Store `pfxPassword` as plaintext in the JSON profile file (per clarification session).

**Rationale**: Profile files are local-only, never shared. PFX passphrases protect a local file — they are not credentials to a remote service. This is consistent with all other test parameters (URLs, hostnames, paths).
