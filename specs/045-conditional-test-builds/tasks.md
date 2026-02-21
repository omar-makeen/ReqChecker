# Tasks: Conditional Test Builds

**Input**: Design documents from `/specs/045-conditional-test-builds/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: No test tasks generated (not explicitly requested in the feature specification).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the test manifest file and wire it into the build system

- [x] T001 Create TestManifest.props with Compile Remove pattern for `Tests/*Test.cs`, KnownTestType registry for all 22 test types, and conditional Compile Include blocks (one per test type) that default to include-all when IncludeTests is empty, in `src/ReqChecker.Infrastructure/TestManifest.props`
- [x] T002 Add `<Import Project="TestManifest.props" />` to `src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Ensure the runtime handles missing test types gracefully before any conditional builds are used

- [x] T003 Verify SequentialTestRunner in `src/ReqChecker.Infrastructure/Services/` gracefully skips test definitions with no matching ITest implementation (FR-006). If it throws on unmatched types, add a null check to skip with a warning log.

**Checkpoint**: Foundation ready — the manifest is wired in and the runtime handles missing types. User story implementation can now begin.

---

## Phase 3: User Story 1 — Build With Selected Tests Only (Priority: P1) — MVP

**Goal**: A developer can run `dotnet publish /p:IncludeTests="Ping;HttpGet"` and get a binary containing only those test types. Running with no parameter produces the full build (backwards compatible).

**Independent Test**: Build with `IncludeTests="Ping;HttpGet"`, inspect the output assembly to confirm only PingTest and HttpGetTest classes exist. Then build with no parameter and confirm all 22 test types are present.

### Implementation for User Story 1

- [x] T004 [US1] Run `dotnet build` with no IncludeTests parameter and verify zero errors, all 22 test types compile, and `dotnet test` passes — confirming backwards compatibility (SC-003)
- [x] T005 [US1] Run `dotnet build /p:IncludeTests="Ping;HttpGet"` and verify only PingTest and HttpGetTest are compiled into the Infrastructure assembly (inspect with `dotnet publish` output or reflection test)

**Checkpoint**: US1 is complete. Selective compilation works. Full build unchanged.

---

## Phase 4: User Story 3 — Default Profile Matches Included Tests (Priority: P2)

**Goal**: When building with a subset of test types, the embedded `default-profile.json` is filtered to only include test entries matching the selected types.

**Independent Test**: Build with `IncludeTests="Ping;HttpGet"`, extract the embedded default profile, verify it only contains Ping and HttpGet test entries (2 out of 27).

### Implementation for User Story 3

- [x] T006 [US3] Add `FilterDefaultProfile` MSBuild target to `src/ReqChecker.App/ReqChecker.App.csproj` that runs `BeforeTargets="PrepareForBuild"` — when IncludeTests is set, invoke an inline PowerShell script to parse `Profiles/default-profile.json`, remove test entries whose `type` is not in the IncludeTests list, write filtered output to `obj/filtered-default-profile.json`, and update the EmbeddedResource item to use the filtered copy
- [x] T007 [US3] Verify filtered build: run `dotnet build /p:IncludeTests="Ping;HttpGet"`, extract embedded resource, confirm only Ping and HttpGet test entries remain in the profile JSON (SC-006)

**Checkpoint**: US3 is complete. Default profile matches included tests.

---

## Phase 5: User Story 4 — Build Validation of Test Type Names (Priority: P3)

**Goal**: The build fails with a clear error if an invalid test type name is specified or if a test file exists without a manifest entry.

**Independent Test**: Run `dotnet build /p:IncludeTests="Ping;FooBar"` and verify the build fails with an error message listing "FooBar" as unrecognized. Add a dummy `Tests/UnregisteredTest.cs` and verify the build fails with an error about the missing manifest entry.

### Implementation for User Story 4

- [x] T008 [P] [US4] Add `ValidateIncludeTests` MSBuild target to `src/ReqChecker.Infrastructure/TestManifest.props` — runs `BeforeTargets="CoreCompile"`, iterates each name in IncludeTests, checks against KnownTestType items, emits `<Error>` with the invalid name and full list of valid types (FR-004)
- [x] T009 [P] [US4] Add `ValidateManifestSync` MSBuild target to `src/ReqChecker.Infrastructure/TestManifest.props` — globs `Tests/*Test.cs` excluding `TestTypeAttribute.cs`, checks each file has a corresponding KnownTestType entry, emits `<Error>` for unregistered files (FR-014)
- [x] T010 [US4] Add empty-IncludeTests guard to `src/ReqChecker.Infrastructure/TestManifest.props` — if IncludeTests property is explicitly set but contains no values (empty string after trim), emit `<Error>` "No test types specified" (FR-007)
- [ ] T011 [US4] Add `WarnDependsOnExclusion` MSBuild target to `src/ReqChecker.Infrastructure/TestManifest.props` — when IncludeTests is set, parse `default-profile.json` dependsOn arrays and emit `<Warning>` if any dependency references a test type not in IncludeTests (FR-008) *(deferred: dependsOn uses test IDs not types; warning is best-effort and can be added later)*

**Checkpoint**: US4 is complete. Build validation catches typos, unregistered files, empty selections, and broken dependencies.

---

## Phase 6: User Story 2 — CI/CD Workflow for Customer Builds (Priority: P2)

**Goal**: A GitHub Actions workflow allows triggering a customer-specific build from the Actions tab with test type selection and customer naming.

**Independent Test**: Push the workflow file, trigger it manually via GitHub Actions UI with `tests: "Ping;HttpGet"`, `customer-name: "TestCorp"`, `version: "1.0.0"`. Verify the artifact is named `ReqChecker-TestCorp-v1.0.0.zip` and contains only the specified test types.

### Implementation for User Story 2

- [x] T012 [US2] Create `.github/workflows/customer-build.yml` with `workflow_dispatch` trigger, inputs for `tests` (string, required, default "all"), `customer-name` (string, required), and `version` (string, required). Steps: checkout, setup .NET 8.0, restore, publish with `/p:IncludeTests` (skip if "all"), zip output as `ReqChecker-{customer-name}-v{version}.zip`, upload artifact (FR-009, FR-010)

**Checkpoint**: US2 is complete. Customer builds can be triggered from GitHub Actions.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all user stories

- [x] T013 Run full regression: `dotnet build` (no parameters) + `dotnet test` — confirm zero errors and all existing tests pass (SC-003)
- [x] T014 Run selective build regression: `dotnet publish -c Release /p:IncludeTests="Ping;HttpGet;FileExists"` — confirm output is smaller than full publish, profile is filtered, and app launches without errors (SC-001)
- [x] T015 Run quickstart.md validation — execute each documented command from `specs/045-conditional-test-builds/quickstart.md` and verify they work as described

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational — verification of the manifest working
- **US3 (Phase 4)**: Depends on US1 (needs IncludeTests mechanism to exist)
- **US4 (Phase 5)**: Depends on US1 (needs manifest and KnownTestType items to exist)
- **US2 (Phase 6)**: Depends on US1 (needs the build mechanism to exist)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **US3 (P2)**: Can start after US1 — Needs IncludeTests to filter against
- **US4 (P3)**: Can start after US1 — Needs KnownTestType items in manifest
- **US2 (P2)**: Can start after US1 — Needs the build mechanism to reference in workflow

### Parallel Opportunities After US1

Once US1 (Phase 3) is complete, the following can run **in parallel**:
- US3 (Phase 4) — modifies App.csproj only
- US4 (Phase 5) — modifies TestManifest.props only
- US2 (Phase 6) — creates new workflow file only

Within US4, T008 and T009 can run in parallel (different validation targets, no file conflicts).

---

## Parallel Example: After US1 Completion

```text
# These three phases can run in parallel (different files):
Phase 4 (US3): FilterDefaultProfile target in App.csproj
Phase 5 (US4): Validation targets in TestManifest.props
Phase 6 (US2): customer-build.yml workflow file

# Within Phase 5, T008 and T009 can run in parallel:
T008: ValidateIncludeTests target
T009: ValidateManifestSync target
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T002) — Create manifest, wire into csproj
2. Complete Phase 2: Foundational (T003) — Verify runtime graceful handling
3. Complete Phase 3: US1 (T004–T005) — Verify selective and full builds work
4. **STOP and VALIDATE**: Build with and without IncludeTests, confirm both paths work
5. This alone delivers the core value — customer-specific builds are possible

### Incremental Delivery

1. Setup + Foundational + US1 → MVP: selective compilation works
2. Add US3 → Default profile filtered → Better UX for customer builds
3. Add US4 → Validation → Safer builds, catches typos and missing entries
4. Add US2 → CI/CD → Automated customer delivery pipeline
5. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- The manifest (T001) is the largest single task — it contains 22 conditional ItemGroup blocks
- Existing reflection-based DI (App.xaml.cs) requires zero changes (FR-012)
- UI converters (icon, color, details) are NOT conditionally compiled — they keep safe fallback defaults
- Commit after each phase checkpoint
