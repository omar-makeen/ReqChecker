# Contracts: Move mTLS Credentials to Test Configuration

**Feature**: 040-mtls-config-credentials

## Overview

This feature has no external API contracts. All changes are internal to the application:

- **Profile JSON schema**: Extended with optional `pfxPassword` parameter (see data-model.md)
- **Internal method signatures**: `PromptForCredentialsIfNeededAsync` gains a new code path but signature is unchanged
- **UI rendering**: `TestConfigView` DataTemplate gains a conditional `PasswordBox` for password-type parameters

No breaking changes to any public interface.
