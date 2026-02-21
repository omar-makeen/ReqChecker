# Research: SystemRam & CpuCores Hardware Tests

**Feature**: 046-hardware-tests | **Date**: 2026-02-21

## RAM Detection API

**Decision**: Use `Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory`

**Rationale**: Returns total installed physical RAM in bytes as a `ulong`. Available in .NET 8 on Windows without any additional NuGet package (the `Microsoft.VisualBasic` namespace is part of the Windows desktop runtime). Simple, reliable, and consistent with how other system info is gathered in the codebase. Converting bytes to GB is trivial: `bytes / (1024.0 * 1024 * 1024)`.

**Alternatives considered**:
- `GCMemoryInfo` via `GC.GetGCMemoryInfo()` — reports GC-managed memory limits, not total physical RAM. Unsuitable.
- WMI `Win32_ComputerSystem.TotalPhysicalMemory` — works but requires `System.Management` NuGet package and is slower (COM interop). Overkill for this use case.
- P/Invoke `GlobalMemoryStatusEx` — works and is fast, but adds unnecessary native interop complexity when a managed API exists.
- `PerformanceCounter` — unreliable and deprecated in .NET Core.

## CPU Core Detection API

**Decision**: Use `Environment.ProcessorCount`

**Rationale**: Returns the number of logical processors available to the process. This is the standard .NET API, cross-platform, requires no packages, and matches the clarified requirement (logical processors, including hyperthreading). A 4-core/8-thread CPU returns 8.

**Alternatives considered**:
- WMI `Win32_Processor.NumberOfLogicalProcessors` — same result but requires `System.Management` and is slower.
- `RuntimeInformation` — provides architecture info but not core count.
- P/Invoke `GetSystemInfo` — unnecessary when `Environment.ProcessorCount` exists.

## Informational Mode Pattern

**Decision**: Follow the established pattern from `OsVersionTest` and `InstalledSoftwareTest`

**Rationale**: When optional threshold parameters are null/absent, the test auto-passes and reports the detected value in evidence and HumanSummary. This pattern is already well-established in the codebase and understood by users.

## Error Handling for Invalid Parameters

**Decision**: Negative threshold values → `ErrorCategory.Configuration` error; zero → always pass (trivially met)

**Rationale**: Consistent with how `DiskSpaceTest` handles `minimumFreeGB`. A negative minimum makes no physical sense and indicates a profile authoring error. Zero is technically valid (any machine meets it) and harmless.

## RAM Display Precision

**Decision**: 1 decimal place (e.g., "15.9 GB", "32.0 GB")

**Rationale**: Matches deployment guide conventions. Two decimal places would show noise from OS memory reservation. Zero decimal places would hide meaningful differences (15 vs 16 GB).
