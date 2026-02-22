# Tasks: Traceroute Test Type

**Input**: Design documents from `/specs/050-traceroute-test/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: No automated tests — manual testing via app launch (consistent with all other test types).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Build Manifest Registration)

**Purpose**: Register the new test type in the conditional build system so it compiles

- [x] T001 Add `Traceroute` KnownTestType entry and conditional Compile ItemGroup to `src/ReqChecker.Infrastructure/TestManifest.props` — follow the exact two-entry pattern used by all other test types (KnownTestType with SourceFile + conditional ItemGroup with Compile Include). Update the comment on line 9 from "all 25 test types" to "all 26 test types".

**Checkpoint**: `dotnet build` succeeds (no TracerouteTest.cs file yet — the manifest entry is conditional and won't error)

---

## Phase 2: User Story 1+2 — Core Traceroute Implementation (Priority: P1) 🎯 MVP

**Goal**: Implement the TracerouteTest class that traces network hops to a target host, handling both successful traces (US1) and unreachable/partial routes (US2). These two stories share the same implementation since they represent the pass and fail paths of the same tracing logic.

**Independent Test**: Run the app with a profile containing a Traceroute test targeting `8.8.8.8` (should Pass with hop list) and `192.0.2.1` (should Fail with partial route).

### Implementation

- [x] T002 [US1] Create `src/ReqChecker.Infrastructure/Tests/TracerouteTest.cs` implementing `ITest` with `[TestType("Traceroute")]` attribute. The class must:
  - Extract parameters from `TestDefinition.Parameters`: `host` (required string), `maxHops` (optional int, default 30), `timeout` (optional int, default 5000ms per hop)
  - Validate `host` is non-empty (throw `ArgumentException` if missing — FR-009)
  - Validate `maxHops` is positive (throw `ArgumentException` if <= 0 — FR-009)
  - Resolve hostname to IP via `Dns.GetHostAddressesAsync()` before tracing (FR-006); catch and report DNS failures distinctly (FR-009)
  - Loop TTL from 1 to `maxHops`, sending ICMP echo via `Ping.SendPingAsync(resolvedIp, timeout, buffer, new PingOptions { Ttl = ttl })` (R1)
  - For each hop: record `hop` number, `address` (reply IP or `*` for timeout), `roundtripMs` (reply time or null for timeout) — FR-005
  - Continue through timed-out hops (`IPStatus.TimedOut` → address `*`, roundtripMs null) — FR-010
  - Stop early when target reached (`reply.Status == IPStatus.Success` or `reply.Address` matches resolved IP) — FR-011
  - Set `reachedTarget` = true/false in evidence — FR-007
  - Result: Pass when target reached, Fail otherwise — FR-008
  - Handle `OperationCanceledException` → Skipped status with partial hop data
  - Build evidence dictionary with camelCase keys: `host`, `resolvedIp`, `maxHops`, `hopCount`, `reachedTarget`, `hops` (array of hop entries) — per data-model.md
  - Serialize evidence to `TestEvidence.ResponseData` via `JsonSerializer.Serialize()`
  - Set `TestEvidence.Timing` with `TotalMs` from stopwatch

**Checkpoint**: Build succeeds. App can load a profile with a Traceroute test and execute it. Pass result for reachable hosts, Fail for unreachable. Evidence JSON contains hop list.

---

## Phase 3: User Story 3 — Custom Trace Parameters (Priority: P2)

**Goal**: Ensure `maxHops` and `timeout` parameters are properly respected when customized in the profile. This is inherently built into T002's parameter extraction, but this phase verifies the behavior.

**Independent Test**: Run with `maxHops` set to 5 — trace stops at 5 hops. Run with `timeout` set to 1000 — timed-out hops resolve faster.

### Implementation

- [x] T003 [US3] Verify and validate custom parameter handling in `src/ReqChecker.Infrastructure/Tests/TracerouteTest.cs` — ensure `maxHops` bounds the loop correctly (trace stops at exactly `maxHops` if target not reached), `timeout` is passed as the per-hop timeout to `Ping.SendPingAsync()`, and defaults (30 hops, 5000ms) are applied when parameters are omitted. This is a validation pass on T002, not a new file.

**Checkpoint**: All three parameter combinations work: defaults only, custom maxHops, custom timeout.

---

## Phase 4: App Integration (Details Converter + Profiles)

**Purpose**: Wire up the evidence display and add sample tests to built-in profiles

- [x] T004 [P] Add `[Traceroute]` evidence section to `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs` — detect traceroute evidence by checking for `host` + `hops` keys in evidenceData. Render summary fields (Host, Resolved IP, Hops count / maxHops, Reached yes/no) followed by `tracert`-style compact lines from the `hops` array. Format each hop as `  {hop}   {roundtripMs}ms  {address}` with `*` for timed-out hops (per FR-012 and quickstart.md evidence output example). Parse `hops` array from JSON using `JsonDocument`.

- [x] T005 [P] Add sample Traceroute test entry to `src/ReqChecker.App/Profiles/default-profile.json` — append a new test object with id `test-037`, type `Traceroute`, displayName `Trace Route to DNS`, parameters `{ "host": "8.8.8.8", "maxHops": 30, "timeout": 5000 }`, fieldPolicy with all three parameters as `Editable`, and empty dependsOn. Follow the exact JSON structure of existing test entries.

- [x] T006 [P] Add sample Traceroute test entry to `src/ReqChecker.App/Profiles/sample-diagnostics.json` — append a new test object with a GUID-style id following the existing pattern (e.g., `10000000-0000-0000-0000-00000000000e`), type `Traceroute`, displayName `Trace Route to DNS`, same parameters as T005. Follow the exact JSON structure of existing entries.

**Checkpoint**: Build succeeds. App displays `[Traceroute]` section in results details view with hop-by-hop output. Both profiles include the sample test.

---

## Phase 5: Documentation & Polish

**Purpose**: Update README and project documentation to reflect the new test type

- [x] T007 [P] Update `README.md`:
  - Line 11: Change "25 Built-in Test Types" to "26 Built-in Test Types"
  - Conditional build section (~line 91): Change "all 25 test types" to "all 26 test types"
  - IncludeTests description (~line 101): Change "all 25 test types" to "all 26 test types"
  - Test Types heading (~line 122): Change "26 built-in test types" count (verify current value and increment)
  - Add `| Network | Traceroute | Trace network hops to target (diagnostic) |` row to the test types table, after the ProxyConnectivity row
  - Add a `#### Traceroute` documentation section after the ProxyConnectivity section in the Network Tests area, with parameter table (host, maxHops, timeout), a basic example JSON, and a description matching quickstart.md

- [x] T008 Verify `dotnet build` succeeds with 0 errors and 0 warnings across the full solution (close any running ReqChecker.App instance first to avoid file lock). Verify `dotnet build /p:IncludeTests="Traceroute"` succeeds (selective build). Verify `dotnet build /p:IncludeTests="Ping;Traceroute"` succeeds (multi-type selective build).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (US1+US2)**: Depends on Phase 1 (manifest must register the file before it compiles)
- **Phase 3 (US3)**: Depends on Phase 2 (validates parameter handling in the same file)
- **Phase 4 (App Integration)**: Depends on Phase 2 (needs evidence keys to exist). T004, T005, T006 are all [P] — different files, can run in parallel.
- **Phase 5 (Documentation)**: Depends on Phase 1 (needs manifest count). T007 is independent of Phase 2-4. T008 depends on all prior phases.

### User Story Dependencies

- **US1+US2 (P1)**: Can start after Phase 1 — no dependencies on other stories
- **US3 (P2)**: Validates behavior built into US1+US2 — depends on Phase 2 completion

### Parallel Opportunities

- **Phase 4**: T004, T005, T006 can all run in parallel (converter, default-profile, sample-diagnostics — different files)
- **Phase 4 + Phase 5 T007**: T007 (README) can run in parallel with T004/T005/T006 since it's a different file
- **Cross-phase**: After Phase 2 completes, Phase 3, Phase 4, and T007 can all start simultaneously

```
Phase 1 (T001) → Phase 2 (T002) → Phase 3 (T003)
                                 ↘ Phase 4: T004 ║ T005 ║ T006  (parallel)
                                 ↘ Phase 5: T007                 (parallel with Phase 4)
                                                  ↘ T008         (final verification)
```

---

## Implementation Strategy

### MVP First (User Stories 1+2 Only)

1. Complete Phase 1: Register in manifest
2. Complete Phase 2: Implement TracerouteTest.cs
3. **STOP and VALIDATE**: Test with reachable and unreachable hosts
4. The core test type is functional — everything else is polish

### Incremental Delivery

1. Phase 1 + Phase 2 → Core traceroute works (MVP)
2. Phase 3 → Custom parameters validated
3. Phase 4 → Evidence displays nicely, profiles include samples
4. Phase 5 → Documentation complete, build verified

---

## Notes

- [P] tasks = different files, no dependencies
- US1 and US2 share the same implementation (TracerouteTest.cs) since they represent pass/fail paths of the same tracing logic
- US3 is a validation pass, not a new file — custom parameters are naturally part of the parameter extraction in T002
- The existing PingTest.cs is the closest reference implementation — same Ping API, similar evidence pattern
- Total: 8 tasks across 5 phases
