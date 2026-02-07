# Research: 033-new-test-types

**Date**: 2026-02-07
**Branch**: `033-new-test-types`

## Decision 1: DnsResolve / DnsLookup naming and backward compatibility

**Decision**: Implement a single class `DnsResolveTest` with `[TestType("DnsResolve")]`. Handle the `DnsLookup` alias in the test runner's type-matching logic (or register the same instance under both names). The icon/color converters already map `DnsLookup` — add `DnsResolve` alongside it.

**Rationale**: The sample profile (`sample-diagnostics.json`) already has `"type": "DnsLookup"` for test 003. The icon converter and color converter both reference `"DnsLookup"`. A single implementation class avoids code duplication; the alias can be handled at the DI/runner level. The `TestTypeAttribute` only supports one identifier, so the runner/DI registration must handle the alias mapping.

**Alternatives considered**:
- Two separate classes (rejected — identical code, maintenance burden)
- Rename existing references from `DnsLookup` to `DnsResolve` (rejected — breaks existing profiles)
- Use `[TestType("DnsLookup")]` only (rejected — spec explicitly names the type `DnsResolve`, and `DnsResolve` is more descriptive of what the test does)

## Decision 2: DNS resolution API

**Decision**: Use `System.Net.Dns.GetHostAddressesAsync(hostname, cancellationToken)` — the built-in .NET API. No external dependencies needed.

**Rationale**: .NET 8 provides async DNS resolution with cancellation support out of the box. The API returns `IPAddress[]` which gives us all resolved addresses and address families (IPv4/IPv6) for the evidence.

**Alternatives considered**:
- DnsClient NuGet package (rejected — unnecessary external dependency for a simple lookup)
- `Dns.GetHostEntryAsync` (viable but `GetHostAddressesAsync` is simpler when we only need IPs)

## Decision 3: TCP port connectivity approach

**Decision**: Use `System.Net.Sockets.TcpClient.ConnectAsync(host, port, cancellationToken)` with a `CancellationTokenSource.CreateLinkedTokenSource` for timeout control.

**Rationale**: Built-in .NET 8 API with native async and cancellation support. The TcpClient is the standard approach for TCP connection checks. Using a linked CTS allows honoring both the user's cancel button and the per-test timeout.

**Alternatives considered**:
- Raw `Socket` with `ConnectAsync` (rejected — TcpClient is simpler and sufficient for a connect-only check)
- `HttpClient` (rejected — we want TCP-level, not HTTP-level)

## Decision 4: Windows Service API

**Decision**: Use `System.ServiceProcess.ServiceController` class with `#if WINDOWS` guard. The `net8.0-windows` TFM already provides access to this API.

**Rationale**: `ServiceController` is the standard .NET API for querying Windows services. It provides `Status`, `DisplayName`, `StartType`, and `ServiceName`. The existing `RegistryReadTest` establishes the `#if WINDOWS` pattern for platform-specific tests.

**Alternatives considered**:
- WMI queries via `ManagementObjectSearcher` (rejected — heavier, slower, requires additional NuGet package)
- PowerShell invocation (rejected — process overhead, complex output parsing)

## Decision 5: Disk space API

**Decision**: Use `System.IO.DriveInfo` for Windows drive letters and `DriveInfo.GetDrives()` to validate the path. This works cross-platform in .NET 8 — on Linux/macOS, mount points like `/` are represented as drives.

**Rationale**: `DriveInfo` provides `AvailableFreeSpace`, `TotalSize`, and `DriveFormat` — everything needed for the evidence. No external dependencies. The path parameter maps directly to `DriveInfo` construction.

**Alternatives considered**:
- `FileSystemInfo` / `DirectoryInfo` (rejected — doesn't provide free space information)
- P/Invoke `GetDiskFreeSpaceEx` (rejected — Windows-only, `DriveInfo` is cross-platform)

## Decision 6: DnsLookup alias registration in DI

**Decision**: Modify the DI registration in `App.xaml.cs` to detect the `DnsResolveTest` class and register it so the `SequentialTestRunner` can find it for both `DnsResolve` and `DnsLookup` type strings. The simplest approach: add a second `[TestType]` is not possible (AllowMultiple = false). Instead, the test runner already looks up tests by matching `testDefinition.Type` against the `TestTypeAttribute.TypeIdentifier`. We can add a type-alias dictionary in the runner, or register the class twice with a wrapper, or handle it in the converter. The cleanest approach is to add a small alias map in the runner that maps `DnsLookup` → `DnsResolve` before lookup.

**Rationale**: Minimal code change, no attribute modifications needed, clear and explicit alias declaration.

**Alternatives considered**:
- Change `AllowMultiple = true` on the attribute (rejected — wider change affecting all test types, plus attribute-based reflection scan would need updating)
- Create a `DnsLookupTest` subclass that delegates (rejected — unnecessary indirection)

## Decision 7: Icon and color assignments for new types

**Decision**:
- `DnsResolve` / `DnsLookup`: Already mapped — `SymbolRegular.Link24`, `StatusInfo` (blue). Keep as-is.
- `TcpPortOpen`: `SymbolRegular.PlugConnected24` (represents connectivity), `StatusInfo` (blue — network category)
- `WindowsService`: `SymbolRegular.WindowApps24` (represents Windows services), `AccentSecondary` (purple — system category, same as ProcessList/RegistryRead)
- `DiskSpace`: `SymbolRegular.HardDrive20` (represents storage), `StatusSkip` (gray — infrastructure category, same as FileExists/DirectoryExists)

**Rationale**: Follows existing grouping convention: network tests = blue, system/process tests = purple, file/storage tests = gray.

**Alternatives considered**:
- Custom colors for each new type (rejected — breaks category convention established by existing types)

## Decision 8: Profile updates

**Decision**: Update `sample-diagnostics.json` to change test 003 from `"type": "DnsLookup"` to `"type": "DnsResolve"` and keep the alias for backward compatibility. Add two new tests to `default-profile.json` (TcpPortOpen and DiskSpace) to showcase the new types. Do not add WindowsService to bundled profiles since not all users may have a specific service to check.

**Rationale**: The sample profile already has a DnsLookup entry that never worked — upgrading it to DnsResolve makes it functional. Default profile gains coverage of the most universally useful new types.

**Alternatives considered**:
- Leave sample profile unchanged (rejected — DnsLookup test would continue showing as broken)
- Add all 4 new types to both profiles (rejected — WindowsService requires a specific service name that varies by environment)
