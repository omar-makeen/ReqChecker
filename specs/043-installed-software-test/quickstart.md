# Quickstart: InstalledSoftware Test

**Branch**: `043-installed-software-test`

## Overview

Add an `InstalledSoftware` test type that answers "Is X installed?" by searching the Windows registry uninstall keys. Supports informational mode (just report what's installed) and minimum-version enforcement.

## Files to Create

| File | Purpose |
|------|---------|
| `src/ReqChecker.Infrastructure/Tests/InstalledSoftwareTest.cs` | Test implementation (ITest) |
| `tests/ReqChecker.Infrastructure.Tests/Tests/InstalledSoftwareTestTests.cs` | Unit tests |

## Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs` | Add `"InstalledSoftware" => SymbolRegular.AppFolder24` |
| `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs` | Add `"InstalledSoftware"` to AccentSecondary group |
| `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs` | Add `[Installed Software]` section |
| `src/ReqChecker.App/Profiles/default-profile.json` | Add 3 sample test entries |

## No Changes Required

- **DI registration**: Auto-discovered via reflection in `App.xaml.cs`
- **TestResultSummaryConverter**: Already handled by wildcard `_` case (returns `HumanSummary`)
- **XAML files**: No template changes needed
- **New packages**: None — uses `Microsoft.Win32.Registry` (built into .NET on Windows)

## Build & Test

```bash
dotnet build
dotnet test tests/ReqChecker.Infrastructure.Tests/
```

## Profile JSON Example

```json
{
  "id": "test-022",
  "type": "InstalledSoftware",
  "displayName": "Check Microsoft Edge",
  "description": "Verifies that Microsoft Edge is installed.",
  "parameters": {
    "softwareName": "Microsoft Edge",
    "minimumVersion": null
  },
  "fieldPolicy": {
    "softwareName": "Editable",
    "minimumVersion": "Editable"
  },
  "timeout": null,
  "retryCount": null,
  "requiresAdmin": false,
  "dependsOn": []
}
```
