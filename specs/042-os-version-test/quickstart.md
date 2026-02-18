# Quickstart: OS Version Validation Test

**Branch**: `042-os-version-test` | **Date**: 2026-02-19

## Prerequisites

- .NET 8.0 SDK installed
- Windows 10 or later
- Repository cloned and on the `042-os-version-test` branch

## Build

```bash
dotnet build
```

## Run Tests

```bash
# All infrastructure tests (includes OsVersion tests)
dotnet test tests/ReqChecker.Infrastructure.Tests/

# Just OsVersion tests
dotnet test tests/ReqChecker.Infrastructure.Tests/ --filter "FullyQualifiedName~OsVersionTestTests"
```

## Files Changed

| File | Action | Purpose |
|------|--------|---------|
| `src/ReqChecker.Infrastructure/Tests/OsVersionTest.cs` | Create | Test implementation |
| `tests/ReqChecker.Infrastructure.Tests/Tests/OsVersionTestTests.cs` | Create | Unit tests |
| `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs` | Modify | Add Desktop24 icon |
| `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs` | Modify | Add AccentSecondary colour |
| `src/ReqChecker.App/Profiles/default-profile.json` | Modify | Add test-019 entry |

## Profile Configuration

Add to any profile JSON to use the OsVersion test:

```json
{
  "id": "test-019",
  "type": "OsVersion",
  "displayName": "Check Windows Version",
  "description": "Verifies the local machine meets minimum Windows build requirements.",
  "parameters": {
    "minimumBuild": 19045,
    "expectedVersion": null
  },
  "fieldPolicy": {
    "minimumBuild": "Editable",
    "expectedVersion": "Editable"
  },
  "timeout": null,
  "retryCount": null,
  "requiresAdmin": false,
  "dependsOn": []
}
```

### Parameter Modes

- **Informational** (both null): Reports OS version, always passes.
- **Minimum build**: Set `minimumBuild` to an integer (e.g., `19045`). Passes if machine build >= value.
- **Exact match**: Set `expectedVersion` to a string (e.g., `"10.0.22631"`). Passes only on exact match.
- Setting both is a configuration error.

## Verification

1. Run the app: the test list should show "Check Windows Version" with a desktop icon in purple.
2. Run the test suite: the OsVersion test should pass in informational mode.
3. Change `minimumBuild` to a high value (e.g., 99999): the test should fail with a validation error.
