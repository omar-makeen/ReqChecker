# Data Model: InstalledSoftware Test

**Date**: 2026-02-20 | **Branch**: `043-installed-software-test`

## Entities

### InstalledSoftware Test Parameters (Profile JSON)

| Field           | Type     | Required | Default | Description                                      |
|-----------------|----------|----------|---------|--------------------------------------------------|
| `softwareName`  | string   | Yes      | —       | Substring to match against registry DisplayName   |
| `minimumVersion`| string?  | No       | null    | Minimum version (major.minor[.patch[.revision]]) |

**Validation Rules**:
- `softwareName`: Must not be null, empty, or whitespace → `ErrorCategory.Configuration`
- `minimumVersion`: If provided, must parse as `System.Version` → `ErrorCategory.Configuration`

### Installed Program Record (Evidence)

Discovered from registry. Serialized as JSON in `Evidence.ResponseData`.

| Field          | Type     | Source Registry Value | Notes                          |
|----------------|----------|----------------------|--------------------------------|
| `displayName`  | string   | `DisplayName`        | Matched entry's full name      |
| `version`      | string?  | `DisplayVersion`     | "unknown" if absent/empty      |
| `installLocation` | string? | `InstallLocation`  | null if absent                 |
| `publisher`    | string?  | `Publisher`          | null if absent                 |
| `installDate`  | string?  | `InstallDate`        | YYYYMMDD format, null if absent|
| `allMatches`   | array    | (derived)            | All matching entries (name + version) |

**Evidence JSON Example (informational mode)**:
```json
{
  "displayName": "Python 3.12.0 (64-bit)",
  "version": "3.12.0",
  "installLocation": "C:\\Python312\\",
  "publisher": "Python Software Foundation",
  "installDate": "20240115",
  "allMatches": [
    { "displayName": "Python 3.12.0 (64-bit)", "version": "3.12.0" },
    { "displayName": "Python 3.10.11 (64-bit)", "version": "3.10.11" }
  ]
}
```

### Test Result Mapping

| Mode            | Condition                           | Status | HumanSummary                                                      |
|-----------------|-------------------------------------|--------|-------------------------------------------------------------------|
| Informational   | Found, no minimumVersion            | Pass   | `"{name} {version} installed"`                                    |
| Informational   | Found, version unknown              | Pass   | `"{name} installed (version unknown)"`                            |
| Minimum version | Found, version >= minimum           | Pass   | `"{name} {version} meets minimum {min}"`                          |
| Minimum version | Found, version < minimum            | Fail   | `"{name} {version} does not meet minimum {min}"`                  |
| Minimum version | Found, version unknown              | Fail   | `"Cannot verify version for {name} — version unknown"`            |
| Not found       | No matches in any hive              | Fail   | `"'{softwareName}' not found in installed programs"`              |
| Config error    | softwareName empty                  | Fail   | `"Invalid configuration: softwareName is required"`               |
| Config error    | minimumVersion invalid format       | Fail   | `"Invalid minimumVersion format '{val}': expected major.minor..."` |

## Registry Search Order

1. `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall` (64-bit)
2. `HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall` (32-bit)
3. `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall` (per-user)

Enumerate all subkeys across all three hives. For each subkey, read `DisplayName`; if it contains `softwareName` (case-insensitive), collect the record. After all hives searched, select primary match by highest parseable version.
