# Quickstart: Move mTLS Credentials to Test Configuration

**Feature**: 040-mtls-config-credentials

## What this feature does

Moves the PFX certificate passphrase from a runtime dialog prompt into the test configuration parameters. Users configure `pfxPassword` alongside other test settings — no more dialog interruption during test execution.

## Files to modify

### 1. SequentialTestRunner (credential resolution)
**File**: `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs`
**Method**: `PromptForCredentialsIfNeededAsync`
**Change**: Add `pfxPassword` check before `credentialRef` check. If `pfxPassword` exists in parameters, create `TestExecutionContext` directly with the password and return — no prompt needed.

### 2. MtlsConnectTest (improved error message)
**File**: `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs`
**Method**: `LoadClientCertificate`
**Change**: Update the `CryptographicException` error message to reference `pfxPassword` parameter.

### 3. Default profile (replace credentialRef with pfxPassword)
**File**: `src/ReqChecker.App/Profiles/default-profile.json`
**Change**: In mTLS test entries, replace `credentialRef` with `pfxPassword`. Update corresponding `fieldPolicy`.

### 4. TestConfigView (password masking in UI)
**File**: `src/ReqChecker.App/Views/TestConfigView.xaml`
**Change**: Add conditional `PasswordBox` in the parameter DataTemplate for fields whose name ends with "Password".

### 5. TestParameterViewModel (password detection)
**File**: `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`
**Change**: Add `IsPassword` property to `TestParameterViewModel` based on parameter name convention.

### 6. Tests
**File**: `tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs`
**Change**: Add tests for pfxPassword-based credential resolution (no prompt triggered).

## Build & verify

```bash
dotnet build src/ReqChecker.App/ReqChecker.App.csproj
dotnet test
```

## Manual verification

1. Run mTLS test with `pfxPassword` configured → no dialog appears, test executes
2. Run mTLS test with `credentialRef` (no `pfxPassword`) → dialog still appears (backward compat)
3. Edit mTLS test config → PFX Password field shows masked input
4. Enter wrong `pfxPassword` → clear error message about incorrect passphrase
