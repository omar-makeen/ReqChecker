# Tasks: New Test Types (DnsResolve, TcpPortOpen, WindowsService, DiskSpace)

**Input**: Design documents from `/specs/033-new-test-types/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in the feature specification. Test tasks are omitted.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No project scaffolding needed — all infrastructure already exists. This phase handles the cross-cutting changes that all four test types depend on.

- [x] T001 Add `DnsLookup` → `DnsResolve` type alias in `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs` so the runner resolves `DnsLookup` profiles to the `DnsResolve` test implementation
- [x] T002 [P] Add icon mappings for `DnsResolve`, `TcpPortOpen`, `WindowsService`, `DiskSpace` in `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs` — use `Link24` (DnsResolve, already exists for DnsLookup), `PlugConnected24` (TcpPortOpen), `WindowApps24` (WindowsService), `HardDrive20` (DiskSpace)
- [x] T003 [P] Add color mappings for `DnsResolve`, `TcpPortOpen`, `WindowsService`, `DiskSpace` in `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs` — DnsResolve/TcpPortOpen → `StatusInfo` (blue/network), WindowsService → `AccentSecondary` (purple/system), DiskSpace → `StatusSkip` (gray/storage)

---

## Phase 2: User Story 1 — DNS Resolution Check (Priority: P1) MVP

**Goal**: IT admins can add a DnsResolve test to a profile that resolves a hostname and optionally validates an expected IP address. Existing `DnsLookup` profiles work via the alias from T001.

**Independent Test**: Load a profile with a DnsResolve test for `www.google.com`, run it, and verify the test passes with resolved IP addresses and timing in evidence.

### Implementation for User Story 1

- [x] T004 [US1] Implement `DnsResolveTest` class in `src/ReqChecker.Infrastructure/Tests/DnsResolveTest.cs` — use `[TestType("DnsResolve")]`, `System.Net.Dns.GetHostAddressesAsync`, parameters: `hostname` (required), `expectedAddress` (optional). Evidence: resolved addresses, address family (IPv4/IPv6/Mixed), resolution time. Error categories: `Network` for resolution failure, `Validation` for expected-address mismatch, `Configuration` for missing hostname parameter. Handle cancellation → Skipped.
- [x] T005 [US1] Update `src/ReqChecker.App/Profiles/sample-diagnostics.json` — change test 003 type from `DnsLookup` to `DnsResolve` (the alias in T001 ensures backward compatibility for any external profiles still using `DnsLookup`)

**Checkpoint**: DnsResolve test is functional. Launch app, load sample profile, run DNS Resolution Check — passes with evidence showing resolved IPs and timing. `dotnet build` succeeds.

---

## Phase 3: User Story 2 — TCP Port Connectivity Check (Priority: P1)

**Goal**: IT admins can add a TcpPortOpen test to verify that a specific host:port is reachable via TCP handshake.

**Independent Test**: Add a TcpPortOpen test for `www.google.com:443` to a profile, run it, and verify it passes with connection time in evidence.

### Implementation for User Story 2

- [x] T006 [US2] Implement `TcpPortOpenTest` class in `src/ReqChecker.Infrastructure/Tests/TcpPortOpenTest.cs` — use `[TestType("TcpPortOpen")]`, `System.Net.Sockets.TcpClient.ConnectAsync` with linked CTS for timeout. Parameters: `host` (required), `port` (required, int), `connectTimeout` (optional, default 5000ms). Evidence: host, port, connected (bool), remote endpoint, connect time. Error categories: `Network` for connection refused, `Timeout` for connection timeout, `Configuration` for missing/invalid parameters. Handle cancellation → Skipped.
- [x] T007 [US2] Add a TcpPortOpen test entry to `src/ReqChecker.App/Profiles/default-profile.json` — target `www.example.com` port `443`, add `dependsOn`, `fieldPolicy`, `timeout`, `retryCount`, `requiresAdmin` fields per schema v3 format

**Checkpoint**: TcpPortOpen test is functional. Launch app, load default profile, run TCP port test — passes with connection time in evidence. `dotnet build` succeeds.

---

## Phase 4: User Story 3 — Windows Service Status Check (Priority: P2)

**Goal**: IT admins can add a WindowsService test to check if a named Windows service is installed and in the expected state (default: Running).

**Independent Test**: Add a WindowsService test for `Winmgmt` (WMI service, always running on Windows) to a profile, run it, and verify it passes with display name, status, and start type in evidence.

### Implementation for User Story 3

- [x] T008 [US3] Implement `WindowsServiceTest` class in `src/ReqChecker.Infrastructure/Tests/WindowsServiceTest.cs` — use `[TestType("WindowsService")]`, `System.ServiceProcess.ServiceController` with `#if WINDOWS` guard. Parameters: `serviceName` (required, internal name only), `expectedStatus` (optional, default `Running`). Evidence: service name, display name, status, expected status, start type, status match (bool). Error categories: `Validation` for status mismatch, `Configuration` for service not found or missing parameter, `Permission` for non-Windows skip. Non-Windows path: return Skipped with "Windows Service tests are only supported on Windows" message. Handle cancellation → Skipped.

**Checkpoint**: WindowsService test is functional. Create a test profile with a WindowsService test for `Winmgmt`, run it — passes with service details in evidence. `dotnet build` succeeds.

---

## Phase 5: User Story 4 — Disk Space Check (Priority: P2)

**Goal**: IT admins can add a DiskSpace test to check if a drive or mount point has at least a specified amount of free space.

**Independent Test**: Add a DiskSpace test for `C:\` with `minimumFreeGB: 1` to a profile, run it, and verify it passes with total/free/percent in evidence.

### Implementation for User Story 4

- [x] T009 [US4] Implement `DiskSpaceTest` class in `src/ReqChecker.Infrastructure/Tests/DiskSpaceTest.cs` — use `[TestType("DiskSpace")]`, `System.IO.DriveInfo`. Parameters: `path` (required, e.g. `C:\` or `/`), `minimumFreeGB` (required, decimal). Evidence: path, total space GB, free space GB, percent free, minimum threshold, threshold met (bool). Pass when free >= minimum (inclusive). Error categories: `Validation` for insufficient space, `Configuration` for invalid/missing path or drive not found. Handle cancellation → Skipped.
- [x] T010 [US4] Add a DiskSpace test entry to `src/ReqChecker.App/Profiles/default-profile.json` — target `C:\` with `minimumFreeGB: 10`, add `dependsOn`, `fieldPolicy`, `timeout`, `retryCount`, `requiresAdmin` fields per schema v3 format

**Checkpoint**: DiskSpace test is functional. Launch app, load default profile, run disk space test — passes with storage details in evidence. `dotnet build` succeeds.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final build verification and validation across all stories

- [x] T011 Run `dotnet build` to verify zero errors across the full solution
- [x] T012 Run `dotnet test` to verify all existing tests still pass (no regressions)
- [x] T013 Launch the app and verify all four new test types display with correct icons and colors in the test list

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — T001, T002, T003 can start immediately. T002 and T003 are parallel (different files).
- **US1 (Phase 2)**: Depends on T001 (alias) and T002/T003 (converters). T004 depends on T001. T005 is independent of T004.
- **US2 (Phase 3)**: Depends on T002/T003 (converters). T006 can start as soon as converters are done. Independent of US1.
- **US3 (Phase 4)**: Depends on T002/T003 (converters). T008 can start as soon as converters are done. Independent of US1/US2.
- **US4 (Phase 5)**: Depends on T002/T003 (converters). T009 can start as soon as converters are done. T010 should follow T007 (same file). Independent of US1/US2/US3.
- **Polish (Phase 6)**: Depends on all user stories being complete.

### User Story Dependencies

- **US1 (DnsResolve)**: Depends on T001 (alias in runner) + T002/T003 (converters). No dependency on other stories.
- **US2 (TcpPortOpen)**: Depends on T002/T003 (converters). No dependency on other stories.
- **US3 (WindowsService)**: Depends on T002/T003 (converters). No dependency on other stories.
- **US4 (DiskSpace)**: Depends on T002/T003 (converters). T010 should be sequenced after T007 (both modify `default-profile.json`).

### Parallel Opportunities

- T002 and T003 can run in parallel (different converter files)
- T004, T006, T008, T009 can run in parallel once converters are complete (each creates a new file)
- T005 is independent of all other implementation tasks (different file)

---

## Parallel Example: After Setup Complete

```text
# Once T001, T002, T003 are done, launch all test implementations in parallel:
Task T004: "Implement DnsResolveTest in src/ReqChecker.Infrastructure/Tests/DnsResolveTest.cs"
Task T006: "Implement TcpPortOpenTest in src/ReqChecker.Infrastructure/Tests/TcpPortOpenTest.cs"
Task T008: "Implement WindowsServiceTest in src/ReqChecker.Infrastructure/Tests/WindowsServiceTest.cs"
Task T009: "Implement DiskSpaceTest in src/ReqChecker.Infrastructure/Tests/DiskSpaceTest.cs"

# Then profile updates (T007 before T010 — same file):
Task T005: "Update sample-diagnostics.json DnsLookup → DnsResolve"
Task T007: "Add TcpPortOpen to default-profile.json"
Task T010: "Add DiskSpace to default-profile.json" (after T007)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001, T002, T003)
2. Complete Phase 2: US1 DnsResolve (T004, T005)
3. **STOP and VALIDATE**: Build, launch app, run DnsResolve test against sample profile
4. This alone delivers value — the previously broken DnsLookup test now works

### Incremental Delivery

1. Setup (T001–T003) → Infrastructure ready
2. US1 DnsResolve (T004–T005) → MVP: DNS tests work
3. US2 TcpPortOpen (T006–T007) → Port checks available
4. US3 WindowsService (T008) → Service checks available
5. US4 DiskSpace (T009–T010) → Storage checks available
6. Polish (T011–T013) → Full validation

---

## Notes

- All four test implementations create NEW files — no merge conflicts between stories
- Profile updates (T005, T007, T010) touch existing files — sequence T007 before T010 (same file)
- No new NuGet packages required — all APIs are built into .NET 8
- WindowsService test uses `#if WINDOWS` guard per established RegistryReadTest pattern
- DnsLookup backward compatibility is handled by alias in runner (T001), not by renaming
