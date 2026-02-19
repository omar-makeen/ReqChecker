# Research: InstalledSoftware Test

**Date**: 2026-02-20 | **Branch**: `043-installed-software-test`

## Decision 1: Detection Method — Windows Registry

**Decision**: Use the Windows registry Uninstall keys as the sole detection method.

**Rationale**: The registry approach is fast (<100ms), requires no admin elevation, and covers 100% of software registered in Add/Remove Programs. This matches the existing RegistryRead and OsVersion test patterns already in the codebase.

**Registry Hives to Search** (in order):
1. `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall` — 64-bit programs
2. `HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall` — 32-bit programs on 64-bit OS
3. `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall` — per-user installs

**Values to Extract Per Subkey**:
- `DisplayName` — software name (used for matching)
- `DisplayVersion` — version string (used for comparison)
- `InstallLocation` — install path
- `Publisher` — software publisher
- `InstallDate` — date installed (YYYYMMDD format)

**Alternatives Considered**:
- WMI (`Win32_Product`): Rejected — requires admin, slow (triggers MSI reconfiguration), incomplete coverage
- File system scan: Rejected — unreliable, no version metadata, requires known paths
- PowerShell `Get-Package`: Rejected — requires PowerShell host, additional process overhead

## Decision 2: Version Comparison Strategy

**Decision**: Use `System.Version` for numeric segment comparison with fallback for non-parseable versions.

**Rationale**: `System.Version` handles 2/3/4-part version strings natively and performs correct numeric comparison (3.10 > 3.9). This matches the approach used in `OsVersionTest.cs` line 123: `var expected = Version.Parse(expectedVersion)`.

**Edge Cases**:
- Non-parseable installed version (e.g., "latest", "2024a"): Treat as "unknown" — pass in informational mode, fail if `minimumVersion` specified
- Non-parseable `minimumVersion` parameter: Configuration error (FR-007)
- Version with pre-release suffix (e.g., "3.12.0-beta"): Strip suffix before parsing, compare numeric portion only

## Decision 3: Multi-Match Selection

**Decision**: When multiple registry entries match, select the entry with the highest parseable version as the primary match.

**Rationale**: Per clarification session — if any installed version satisfies the minimum, the machine is ready. All matches are included in evidence for transparency.

**Implementation**: Collect all matches, sort by parsed `Version` descending, pick first. Entries with unparseable versions sort last.

## Decision 4: Codebase Integration Pattern

**Decision**: Follow the `OsVersionTest` pattern exactly — it is the newest, cleanest test type and most similar in scope.

**Key Pattern Elements**:
- `[TestType("InstalledSoftware")]` attribute on class
- `ITest` interface implementation
- Parameter extraction via `JsonObject` indexer
- Evidence as `Dictionary<string, object>` serialized to JSON
- `HumanSummary` always populated (leverages the wildcard fallback in `TestResultSummaryConverter`)
- `TimingBreakdown` with `TotalMs`
- Auto-discovered by reflection in `App.xaml.cs` (no manual DI registration)

**Converter Updates Needed**:
- `TestTypeToIconConverter`: Add `"InstalledSoftware" => SymbolRegular.AppFolder24`
- `TestTypeToColorConverter`: Add to `"ProcessList" or "RegistryRead" or "WindowsService" or "OsVersion"` group (AccentSecondary)
- `TestResultSummaryConverter`: Already handled by wildcard `_` case (returns `HumanSummary`)
- `TestResultDetailsConverter`: Add `[Installed Software]` section keyed on `"displayName"` in evidenceData

## Decision 5: Default Profile Entries

**Decision**: Add 3 test entries to `default-profile.json`:
1. Informational — `softwareName: "Microsoft Edge"` (universally present, no version check)
2. Minimum version — `softwareName: "Microsoft Edge"` with `minimumVersion: "100.0"` (should pass on any modern Windows)
3. Not-found expected — `softwareName: "NonExistentSoftware12345"` (demonstrates failure path)
