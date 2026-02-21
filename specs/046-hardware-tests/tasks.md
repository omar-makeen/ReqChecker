# Tasks: SystemRam & CpuCores Hardware Tests

**Input**: Design documents from `/specs/046-hardware-tests/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: No automated test tasks — not requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Foundational (Build Manifest Registration)

**Purpose**: Register both new test types in TestManifest.props so their source files compile. This MUST be done before creating the test classes — the manifest controls which `*Test.cs` files are included in compilation.

- [x] T001 Register SystemRam and CpuCores in `src/ReqChecker.Infrastructure/TestManifest.props` — add two `<KnownTestType>` entries in the Step 2 registry (`<KnownTestType Include="SystemRam" SourceFile="Tests\SystemRamTest.cs" />` and `<KnownTestType Include="CpuCores" SourceFile="Tests\CpuCoresTest.cs" />`), and add two conditional `<ItemGroup Condition="...">` blocks in Step 4 following the existing pattern (e.g., `$(_IncludeTestsFenced.Contains(';SystemRam;'))`)

**Checkpoint**: Manifest updated — test class files will compile once created

---

## Phase 2: User Story 1 — Verify System RAM Meets Deployment Minimum (Priority: P1) 🎯 MVP

**Goal**: Provide a `SystemRam` test type that detects total physical RAM and optionally enforces a minimum threshold.

**Independent Test**: Create a profile with a SystemRam test entry, run it, and verify the result displays detected RAM with pass/fail against the threshold.

### Implementation for User Story 1

- [x] T002 [P] [US1] Create `src/ReqChecker.Infrastructure/Tests/SystemRamTest.cs` — implement `ITest` with `[TestType("SystemRam")]` attribute. Use `new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory` to detect total physical RAM in bytes, convert to GB (`bytes / 1073741824.0`). Extract optional `minimumGB` parameter (`double?`) from `testDefinition.Parameters`. Informational mode when null: always pass, display detected RAM. Threshold mode: pass if `detectedGB >= minimumGB`, fail otherwise. Negative `minimumGB` → `ErrorCategory.Configuration` error. Evidence dict: `detectedGB` (1 decimal), `detectedBytes`, `minimumGB`, `thresholdMet`. HumanSummary: `"{detectedGB:F1} GB RAM detected"` (info) or `"{detectedGB:F1} GB RAM detected (minimum: {minimumGB:F1} GB) — Pass/Fail"`. Follow `DiskSpaceTest.cs` pattern for stopwatch, timing, error handling.

**Checkpoint**: SystemRam test compiles and executes — can be verified with a manually created profile entry

---

## Phase 3: User Story 2 — Verify CPU Core Count Meets Deployment Minimum (Priority: P1)

**Goal**: Provide a `CpuCores` test type that detects logical processor count and optionally enforces a minimum threshold.

**Independent Test**: Create a profile with a CpuCores test entry, run it, and verify the result displays detected core count with pass/fail against the threshold.

### Implementation for User Story 2

- [x] T003 [P] [US2] Create `src/ReqChecker.Infrastructure/Tests/CpuCoresTest.cs` — implement `ITest` with `[TestType("CpuCores")]` attribute. Use `Environment.ProcessorCount` to detect logical processor count (includes hyperthreading). Extract optional `minimumCores` parameter (`int?`) from `testDefinition.Parameters`. Informational mode when null: always pass, display core count. Threshold mode: pass if `detectedCores >= minimumCores`, fail otherwise. Negative `minimumCores` → `ErrorCategory.Configuration` error. Evidence dict: `detectedCores`, `minimumCores`, `thresholdMet`. HumanSummary: `"{detectedCores} logical processors detected"` (info) or `"{detectedCores} logical processors detected (minimum: {minimumCores}) — Pass/Fail"`. Follow `DiskSpaceTest.cs` pattern for stopwatch, timing, error handling.

**Checkpoint**: CpuCores test compiles and executes — can be verified with a manually created profile entry

---

## Phase 4: User Story 3 — Default Profile Includes Hardware Tests (Priority: P2)

**Goal**: Add SystemRam and CpuCores entries to both bundled profiles so users see hardware tests out of the box.

**Independent Test**: Launch the application with the default profile, verify both hardware test entries appear, run them, and confirm both pass in informational mode.

### Implementation for User Story 3

- [x] T004 [P] [US3] Add 6 SystemRam and CpuCores test entries to `src/ReqChecker.App/Profiles/default-profile.json` — append after the last existing test, following the OsVersion/InstalledSoftware 3-variant pattern: (1) `test-028` SystemRam informational (`minimumGB: null`, displayName "Check System RAM"), (2) `test-029` SystemRam minimum 4 GB expected pass (`minimumGB: 4`, displayName "Check Minimum RAM (4 GB)"), (3) `test-030` SystemRam minimum 1024 GB expected fail (`minimumGB: 1024`, displayName "Check RAM 1024 GB (Expected Fail)"), (4) `test-031` CpuCores informational (`minimumCores: null`, displayName "Check CPU Cores"), (5) `test-032` CpuCores minimum 2 expected pass (`minimumCores: 2`, displayName "Check Minimum CPU Cores (2)"), (6) `test-033` CpuCores minimum 256 expected fail (`minimumCores: 256`, displayName "Check CPU Cores 256 (Expected Fail)"). All with `dependsOn: []`, `requiresAdmin: false`, fieldPolicy `"Editable"`. See `data-model.md` for full JSON templates.
- [x] T005 [P] [US3] Add 6 SystemRam and CpuCores test entries to `src/ReqChecker.App/Profiles/sample-diagnostics.json` — same 3-variant pattern as default-profile using UUID format: `10000000-0000-0000-0000-000000000005` through `10000000-0000-0000-0000-00000000000a`. Same parameters, displayNames, and descriptions as the default-profile entries. See `data-model.md` for full JSON templates.

**Checkpoint**: Both bundled profiles load without validation errors and both hardware tests execute successfully in informational mode

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Build verification and CLAUDE.md update

- [x] T006 Run `dotnet build` to verify full build succeeds with both new test types
- [x] T007 Run `dotnet build src/ReqChecker.App -p:IncludeTests="SystemRam;CpuCores"` to verify conditional build filtering works for both new types

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 1)**: No dependencies — start immediately. BLOCKS all user stories.
- **US1 — SystemRam (Phase 2)**: Depends on Phase 1 (manifest registration)
- **US2 — CpuCores (Phase 3)**: Depends on Phase 1 (manifest registration). **Can run in parallel with US1.**
- **US3 — Profile Entries (Phase 4)**: Depends on Phase 1 (manifest). Can start in parallel with US1/US2 (profile entries are JSON, not code). But full verification requires US1 + US2 complete.
- **Polish (Phase 5)**: Depends on all previous phases

### User Story Dependencies

- **User Story 1 (P1)**: Depends only on Phase 1 — no cross-story dependencies
- **User Story 2 (P1)**: Depends only on Phase 1 — no cross-story dependencies
- **User Story 3 (P2)**: Profile entries can be written in parallel with US1/US2, but full validation requires both test classes to exist

### Parallel Opportunities

- **T002 + T003**: SystemRamTest.cs and CpuCoresTest.cs are independent files — can be implemented in parallel
- **T004 + T005**: default-profile.json and sample-diagnostics.json are independent files — can be edited in parallel
- **T002/T003 + T004/T005**: Test implementations and profile entries touch different files — all four can run in parallel after T001

---

## Parallel Example: All User Stories

```text
# After T001 (manifest), launch all implementation tasks in parallel:
Task: "T002 [US1] Create SystemRamTest.cs"
Task: "T003 [US2] Create CpuCoresTest.cs"
Task: "T004 [US3] Add entries to default-profile.json"
Task: "T005 [US3] Add entries to sample-diagnostics.json"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Register in TestManifest.props (T001)
2. Complete Phase 2: SystemRamTest.cs (T002)
3. **STOP and VALIDATE**: Create a test profile entry manually, run it, verify RAM detection works
4. Ship if needed — SystemRam alone delivers value

### Incremental Delivery

1. T001 → Manifest ready
2. T002 → SystemRam works → can demo RAM detection
3. T003 → CpuCores works → can demo full hardware checks
4. T004 + T005 → Bundled profiles include both → out-of-box experience complete
5. T006 + T007 → Build verification passes

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- No new NuGet packages required — `Microsoft.VisualBasic` is part of the .NET 8 Windows desktop runtime
- DI registration is automatic via reflection — no manual wiring task needed
- Both test classes follow the established pattern in `DiskSpaceTest.cs` and `OsVersionTest.cs`
- Commit after each phase or logical group
