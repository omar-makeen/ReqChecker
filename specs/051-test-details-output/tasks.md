# Tasks: Test Details Output for All Test Types

**Input**: Design documents from `/specs/051-test-details-output/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md

**Tests**: Not requested — no test tasks generated.

**Organization**: Tasks are grouped by user story. All tasks modify the same file (`src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`), so they are sequential within each phase.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: No setup needed — existing project, existing file, no new dependencies.

*(No tasks — this feature modifies a single existing file with no new infrastructure.)*

---

## Phase 2: Foundational

**Purpose**: No foundational tasks — all work is self-contained in the converter.

*(No tasks — the converter already parses `evidenceData` and the insertion point exists.)*

---

## Phase 3: User Story 1 - Network Test Details (Priority: P1) 🎯 MVP

**Goal**: Add dedicated detail sections for Ping, DnsResolve, TcpPortOpen, and UdpPortOpen tests so network engineers can see diagnostic data without inspecting raw JSON.

**Independent Test**: Run each of the four network tests, view the results details, and verify each displays a dedicated section with its key evidence fields.

### Implementation for User Story 1

All sections are inserted after the `[Traceroute]` section (after `sections.Add(string.Empty);` at line 296) and before the `[Response]` section (line 299) in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`.

- [x] T001 [US1] Add `[Ping]` section to converter — detect via `successRate` + `pingResults` keys, display host, success count/total, success rate, avg RTT, and per-attempt results (JSON array parsed to indented lines) in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`
- [x] T002 [US1] Add `[DNS]` section to converter — detect via `hostname` + `addresses` keys, display hostname, resolved addresses (JSON array → indented IP lines), address count, and resolution time in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`
- [x] T003 [US1] Add `[TCP]` section to converter — detect via `host` + `port` + `connected` keys, display host, port, connection status (yes/no), and connect time in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`
- [x] T004 [US1] Add `[UDP]` section to converter — detect via `responded` + `payloadSentBytes` keys, display response status (yes/no), RTT, payload sent/received bytes, and response data preview in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`

**Checkpoint**: Ping, DnsResolve, TcpPortOpen, and UdpPortOpen test results now show dedicated sections with full diagnostic data.

---

## Phase 4: User Story 2 - System & Security Test Details (Priority: P1)

**Goal**: Add dedicated detail sections for DiskSpace, WindowsService, MtlsConnect, and CertificateExpiry tests so IT administrators can see system state without external tools.

**Independent Test**: Run each of the four system/security tests, view the results details, and verify each displays a dedicated section with its key evidence fields.

### Implementation for User Story 2

- [x] T005 [US2] Add `[Disk Space]` section to converter — detect via `totalSpaceGB` + `freeSpaceGB` keys, display path, total space (GB), free space (GB), percent free, minimum required (GB), and threshold status (met/not met) in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`
- [x] T006 [US2] Add `[Service]` section to converter — detect via `serviceName` + `expectedStatus` keys, display service name, display name, current status, expected status, start type, and status match (yes/no) in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`
- [x] T007 [US2] Add `[mTLS]` section to converter — detect via `certificateSubject` + `certificateThumbprint` keys, display connected (yes/no), response time, certificate subject, issuer, thumbprint, valid from/to dates, and private key status (yes/no) in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`
- [x] T008 [US2] Add `[Certificate]` section to converter — detect via `daysUntilExpiry` + `isExpired` keys, display host, port, subject, issuer, thumbprint, expiry date, days left, expired (yes/no), and not-yet-valid (yes/no, omit if false) in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`

**Checkpoint**: DiskSpace, WindowsService, MtlsConnect, and CertificateExpiry test results now show dedicated sections with full diagnostic data.

---

## Phase 5: User Story 3 - File System Test Details (Priority: P2)

**Goal**: Add dedicated detail sections for FileExists and DirectoryExists tests so QA engineers can see file/directory metadata beyond pass/fail.

**Independent Test**: Run FileExists and DirectoryExists tests, view the results details, and verify each displays a dedicated section with its key evidence fields.

### Implementation for User Story 3

- [x] T009 [US3] Add `[File]` section to converter — detect via `path` + `exists` + `size` keys, display path, exists (yes/no), expected exists (yes/no), file size (formatted bytes), and last modified date in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`
- [x] T010 [US3] Add `[Directory]` section to converter — detect via `path` + `exists` + `directoryCount` keys, display path, exists (yes/no), expected exists (yes/no), file count, directory count, and creation time in `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`

**Checkpoint**: All 26 test types now have dedicated detail sections — zero show only `[General]` + `[Timing]`.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verify build and ensure no regressions.

- [x] T011 Verify `dotnet build` succeeds with 0 errors and 0 warnings for `src/ReqChecker.App/ReqChecker.App.csproj`
- [x] T012 Verify existing dedicated sections (OsVersion, InstalledSoftware, EnvironmentVariable, SystemRam, CpuCores, WebSocket, Proxy, Traceroute) are not altered — visual diff of converter code above insertion point

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: N/A — no setup needed
- **Foundational (Phase 2)**: N/A — no foundation needed
- **User Story 1 (Phase 3)**: Can start immediately — no dependencies
- **User Story 2 (Phase 4)**: Depends on US1 completion (same file, sequential edits)
- **User Story 3 (Phase 5)**: Depends on US2 completion (same file, sequential edits)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies — first to implement
- **User Story 2 (P1)**: Follows US1 (same file constraint only — logically independent)
- **User Story 3 (P2)**: Follows US2 (same file constraint only — logically independent)

### Within Each User Story

- Sections within a story are sequential (same file, each builds on previous insertion)
- T001 → T002 → T003 → T004 (US1)
- T005 → T006 → T007 → T008 (US2)
- T009 → T010 (US3)

### Parallel Opportunities

- **None within stories**: All tasks modify the same file
- **Cross-story**: Stories could theoretically be parallel if using separate feature branches, but for simplicity they are sequential in priority order

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Implement T001–T004 (4 network test sections)
2. **STOP and VALIDATE**: Run Ping, DnsResolve, TcpPortOpen, UdpPortOpen tests and verify details
3. The 4 most commonly used diagnostic tests now have rich output

### Incremental Delivery

1. US1 (T001–T004): Network tests → validate → 4 of 10 done
2. US2 (T005–T008): System/security tests → validate → 8 of 10 done
3. US3 (T009–T010): File system tests → validate → 10 of 10 done (SC-001 met)
4. Polish (T011–T012): Build verification and regression check

---

## Notes

- All tasks modify `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`
- Each section follows the exact same pattern as existing sections (e.g., `[WebSocket]`, `[Proxy]`, `[Traceroute]`)
- Evidence keys are from research.md — verified against actual test implementations
- Detection keys are from plan.md — verified for uniqueness across all 26 test types
- FR-011 (null field omission) is inherently satisfied by the `TryGetValue + null check` pattern
- FR-012 (unique detection keys) is satisfied by using key pairs/triples per plan.md design decisions
