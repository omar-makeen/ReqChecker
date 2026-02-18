# Research: OS Version Validation Test

**Branch**: `042-os-version-test` | **Date**: 2026-02-19

## Decision 1: OS Version Detection Method

**Decision**: Use `Environment.OSVersion.Version` for the version string (major.minor.build) and `System.Runtime.InteropServices.RuntimeInformation.OSArchitecture` for processor architecture.

**Rationale**: `Environment.OSVersion` is the standard .NET API for retrieving the Windows version. It returns `Version` with Major, Minor, Build, and Revision components. This is reliable on .NET 8.0 (unlike older .NET Framework where it could return incorrect values due to manifest requirements). `RuntimeInformation.OSArchitecture` provides the processor architecture (X64, Arm64, etc.).

**Alternatives considered**:
- WMI (`Win32_OperatingSystem`) — heavier, requires COM interop, slower startup. Overkill for version/build.
- Registry (`HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion`) — useful for product name (e.g., "Windows 11 Pro") but not the canonical source for version numbers. Will use for product name only.
- `RtlGetVersion` P/Invoke — lower level, not needed when .NET 8 already returns correct values.

## Decision 2: Product Name Source

**Decision**: Read `ProductName` from `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion` via `Microsoft.Win32.Registry`.

**Rationale**: The `Environment.OSVersion` API does not expose a human-friendly product name (e.g., "Windows 11 Pro"). The registry key is the standard location for this information and is readable without elevated privileges. The existing `RegistryReadTest` already demonstrates registry access patterns in this codebase.

**Alternatives considered**:
- WMI `Caption` field — same data but slower and heavier.
- Hardcoded mapping from build ranges — fragile, requires updates with each Windows release.

## Decision 3: Version Comparison Strategy

**Decision**: For `minimumBuild`, compare only the Build component (third segment). For `expectedVersion`, parse as `Version` and compare Major, Minor, and Build components.

**Rationale**: Windows 10 and 11 both use major version 10 and minor version 0. The build number is the meaningful differentiator across feature updates (e.g., 19045 = Windows 10 22H2, 22631 = Windows 11 23H2). Comparing only the build number for the minimum check is simpler and matches how IT administrators think about Windows versioning. The exact match mode uses all three segments for strict compliance scenarios.

**Alternatives considered**:
- Full four-segment comparison (including Revision) — Revision is rarely meaningful for compliance checks.
- Semantic versioning — Windows doesn't follow semver.

## Decision 4: Icon and Colour

**Decision**: Icon = `SymbolRegular.Desktop24`, Colour = `AccentSecondary` (purple).

**Rationale**: OsVersion is a local system test (like ProcessList, RegistryRead, WindowsService) and should share their visual identity. `Desktop24` represents the machine/system itself. `AccentSecondary` is the established colour for system-level tests in the converter.

**Alternatives considered**:
- `SymbolRegular.Settings24` — already used by RegistryRead.
- `SymbolRegular.WindowApps24` — already used by WindowsService.
- `StatusInfo` (blue) — reserved for network/protocol tests.

## Decision 5: No New Packages Required

**Decision**: All functionality implementable with existing dependencies.

**Rationale**: `Environment.OSVersion`, `RuntimeInformation`, and `Microsoft.Win32.Registry` are all available in .NET 8.0 without additional NuGet packages. The test follows the exact same patterns as existing tests (RegistryRead, DirectoryExists).
