# Tasks: UDP Port Reachability Test

**Input**: Design documents from `/specs/039-udp-port-test/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Not explicitly requested in spec - implementing without separate test-first phase. Unit tests included as part of implementation tasks.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Single WPF desktop application:
- `src/ReqChecker.Infrastructure/` - Test implementations
- `src/ReqChecker.App/` - UI converters and profiles
- `tests/ReqChecker.Infrastructure.Tests/` - Unit tests

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No setup required - leveraging existing ReqChecker infrastructure

This feature extends an existing application with established patterns (`ITest` interface, test type auto-discovery, UI converters). No new project structure or dependencies needed.

**Checkpoint**: Skip directly to User Story 1 implementation

---

## Phase 2: User Story 1 - Basic UDP Port Connectivity Check (Priority: P1) 🎯 MVP

**Goal**: Enable administrators to verify UDP service reachability with basic send/receive validation, timeout handling, and error categorization

**Independent Test**: Configure a single UDP port test pointing to 8.8.8.8:53 (Google DNS), run the test, and verify a "Pass" result with evidence showing response time and confirmation of receipt

**Why this is the MVP**: This delivers the core value proposition - basic UDP reachability validation that administrators cannot currently perform in ReqChecker. Covers the most common use case (verifying UDP services are responding).

### Implementation for User Story 1

- [x] T001 [US1] Create UdpPortTestEvidence class in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs with properties: Responded, RoundTripTimeMs, RemoteEndpoint, PayloadSentBytes, PayloadReceivedBytes, ResponseDataPreview
- [x] T002 [US1] Create UdpPortOpenTest class with [TestType("UdpPortOpen")] attribute in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs implementing ITest interface
- [x] T003 [US1] Implement parameter extraction method ExtractParameters in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs to validate and extract host, port, timeout (default 5000ms) from TestDefinition.Parameters
- [x] T004 [US1] Implement DNS resolution method ResolveHostAsync in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs to handle both IP addresses and hostnames, preferring IPv4 for compatibility
- [x] T005 [US1] Implement UDP send/receive method ExecuteUdpTestAsync in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs using UdpClient with Task.WhenAny timeout pattern and default single null byte payload
- [x] T006 [US1] Implement timeout handling in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs to return Fail result with ErrorCategory.Timeout when no response received
- [x] T007 [US1] Implement ICMP error handling in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs to catch SocketException with ConnectionReset and return Fail with ErrorCategory.Network
- [x] T008 [US1] Implement TestResult building methods (BuildResult, BuildCancelledResult, BuildErrorResult) in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs with human-readable summaries and evidence serialization
- [x] T009 [US1] Implement main ExecuteAsync method in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs orchestrating parameter extraction, DNS resolution, UDP test execution, and result building with proper exception handling
- [x] T010 [P] [US1] Add UdpPortOpen case to TestTypeToIconConverter in src/ReqChecker.App/Converters/TestTypeToIconConverter.cs mapping to Symbol.ConnectedNetwork24
- [x] T011 [P] [US1] Add UdpPortOpen to network test category in TestTypeToColorConverter in src/ReqChecker.App/Converters/TestTypeToColorConverter.cs
- [x] T012 [P] [US1] Add sample UDP test (DNS query to 8.8.8.8:53) to tests array in src/ReqChecker.App/Profiles/default-profile.json with host/port/timeout parameters and Editable field policies
- [x] T013 [US1] Create unit test file UdpPortOpenTestTests.cs in tests/ReqChecker.Infrastructure.Tests/Tests/ with test cases for valid DNS query (Pass), unreachable port (Fail/Timeout), invalid port (Configuration error), and missing host (Configuration error)
- [x] T014 [US1] Build and run application to verify UdpPortOpen test appears in UI with correct icon and loads from default profile
- [ ] T015 [US1] Manual test: Run DNS test against 8.8.8.8:53 and verify Pass result with evidence showing response time and received bytes
- [ ] T016 [US1] Manual test: Edit timeout to 1ms and verify Fail result with ErrorCategory.Timeout
- [ ] T017 [US1] Manual test: Edit host to invalid hostname and verify Fail result with ErrorCategory.Network and DNS resolution error message

**Checkpoint**: At this point, User Story 1 (MVP) should be fully functional - basic UDP port connectivity testing works end-to-end with timeout and error handling

---

## Phase 3: User Story 2 - Custom Payload Validation (Priority: P2)

**Goal**: Enable administrators to send protocol-specific UDP payloads (hex/base64-encoded) and optionally validate response patterns for real-world UDP protocol testing (DNS, SNMP, NTP)

**Independent Test**: Configure a DNS query payload (hex-encoded DNS question packet) targeting 8.8.8.8:53, run the test, and verify the response contains a DNS answer section

**Depends on**: User Story 1 (core UDP test infrastructure)

### Implementation for User Story 2

- [x] T018 [US2] Implement payload decoding method DecodePayload in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs supporting hex, base64, utf8, and auto-detection based on string format
- [x] T019 [US2] Update ExtractParameters method in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs to extract optional payload and expectedResponse parameters with encoding hint (default "auto")
- [x] T020 [US2] Implement auto-detection heuristic TryDecodeAuto in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs: hex if even length and all hex chars, else try base64, else UTF-8
- [x] T021 [US2] Update ExecuteUdpTestAsync method in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs to use custom payload instead of default null byte when provided
- [x] T022 [US2] Implement response validation logic in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs using SequenceEqual to compare received bytes with expectedResponse pattern
- [x] T023 [US2] Create ValidationException class in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs for response mismatch errors
- [x] T024 [US2] Update BuildResult method in src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs to return Fail with ErrorCategory.Validation when response doesn't match expected pattern
- [x] T025 [US2] Update default profile sample test in src/ReqChecker.App/Profiles/default-profile.json to include hex-encoded DNS query payload and hide payload/expectedResponse fields
- [x] T026 [US2] Add unit tests to tests/ReqChecker.Infrastructure.Tests/Tests/UdpPortOpenTestTests.cs for hex payload decoding, base64 payload decoding, auto-detection, and response validation (match and mismatch)
- [ ] T027 [US2] Manual test: Configure DNS query with hex payload to 8.8.8.8:53 and verify Pass result with DNS response in evidence
- [ ] T028 [US2] Manual test: Configure test with expectedResponse pattern and mismatching service response, verify Fail with ErrorCategory.Validation and expected vs actual comparison

**Checkpoint**: At this point, User Story 2 should work independently - custom payloads and response validation enable protocol-specific UDP testing

---

## Phase 4: User Story 3 - Field-Level Policy Support (Priority: P3)

**Goal**: Ensure UDP test parameters support all existing field-level policies (Locked, Editable, Hidden, PromptAtRun) for consistency with other ReqChecker test types

**Independent Test**: Load a profile with a UdpPortOpen test where the host parameter has policy "Locked" and the timeout parameter has policy "Editable", verify the UI renders the host field as read-only with a lock icon and the timeout field as editable

**Depends on**: User Story 1 (parameters must exist before policies can be applied)

### Implementation for User Story 3

- [ ] T029 [US3] Verify field policy support: Create test profile with Locked policy on host/port parameters in a temporary test file
- [ ] T030 [US3] Verify field policy support: Load test profile and confirm UI Test Configuration page renders Locked fields with lock icon and read-only state (should work automatically via existing framework)
- [ ] T031 [US3] Verify field policy support: Create test profile with Hidden policy on payload/expectedResponse parameters
- [ ] T032 [US3] Verify field policy support: Load test profile and confirm Hidden fields do not appear in UI Test Configuration page
- [ ] T033 [US3] Verify field policy support: Create test profile with PromptAtRun policy on timeout parameter
- [ ] T034 [US3] Verify field policy support: Start test run and confirm system prompts for timeout value before execution
- [ ] T035 [US3] Update default-profile.json sample test in src/ReqChecker.App/Profiles/default-profile.json to demonstrate mixed policies: host/port/timeout Editable, payload/expectedResponse/encoding Hidden
- [ ] T036 [US3] Document field policy behavior in quickstart.md verification checklist

**Note**: Field-level policies are handled by the existing TestDefinition and UI framework - no code changes required in UdpPortOpenTest.cs, only verification that it integrates correctly.

**Checkpoint**: At this point, User Story 3 should be complete - all UDP test parameters support field-level policies consistent with other test types

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final quality checks and documentation

- [x] T037 [P] Run full test suite with `dotnet test tests/ReqChecker.Infrastructure.Tests/ReqChecker.Infrastructure.Tests.csproj` and verify all tests pass
- [x] T038 [P] Run full application build with `dotnet build src/ReqChecker.App/ReqChecker.App.csproj` and verify zero errors/warnings
- [ ] T039 [P] Validate quickstart.md verification checklist - confirm all 13 items pass
- [x] T040 [P] Add IPv6 address test case to unit tests in tests/ReqChecker.Infrastructure.Tests/Tests/UdpPortOpenTestTests.cs
- [ ] T041 [P] Add ICMP Port Unreachable test case to unit tests (may require mocking SocketException with ConnectionReset)
- [ ] T042 Update CLAUDE.md to document UdpPortOpen test type addition in Active Technologies section (run update-agent-context.ps1 if needed)
- [x] T043 Code review: Verify all error messages are user-friendly and technical details are helpful for troubleshooting
- [x] T044 Code review: Verify evidence JSON schema matches data-model.md specification exactly

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: N/A (no setup required - using existing infrastructure)
- **User Story 1 (Phase 2)**: Can start immediately - MVP implementation
- **User Story 2 (Phase 3)**: Depends on User Story 1 completion (extends core UDP test with payload features)
- **User Story 3 (Phase 4)**: Depends on User Story 1 completion (verifies policy integration with existing parameters)
- **Polish (Phase 5)**: Depends on desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies - core UDP test implementation
- **User Story 2 (P2)**: Depends on US1 (builds on core UDP send/receive to add payload encoding)
- **User Story 3 (P3)**: Depends on US1 (verifies policies work with parameters created in US1)

**Critical Path**: US1 → US2, US3 in parallel → Polish

### Within Each User Story

**User Story 1**:
1. T001-T009: Core implementation (can parallelize T001, T010-T012)
2. T010-T012: UI integration (can run in parallel with each other)
3. T013: Unit tests (after T001-T009 complete)
4. T014-T017: Manual testing (after T001-T013 complete)

**User Story 2**:
1. T018-T024: Payload/validation implementation (sequential due to method dependencies)
2. T025-T026: Profile update and unit tests (can run in parallel)
3. T027-T028: Manual testing (after T018-T026 complete)

**User Story 3**:
1. T029-T036: Policy verification (sequential testing workflow)

### Parallel Opportunities

- **Within US1**: T010, T011, T012 can run in parallel (different files)
- **After US1**: US2 and US3 can start in parallel if team capacity allows
- **Within Polish**: T037, T038, T039, T040, T041, T042 can run in parallel (different files/independent tasks)

---

## Parallel Example: User Story 1

```bash
# Launch UI integration tasks together (different files):
Task T010: "Add UdpPortOpen case to TestTypeToIconConverter.cs"
Task T011: "Add UdpPortOpen to network test category in TestTypeToColorConverter.cs"
Task T012: "Add sample UDP test to default-profile.json"

# After core implementation (T001-T009), these are independent:
Task T010-T012: UI converters and profile (parallel)
Task T013: Unit tests (after T001-T009)
```

## Parallel Example: After US1 Complete

```bash
# If team has 2 developers:
Developer A: User Story 2 (T018-T028)
Developer B: User Story 3 (T029-T036)

# Stories work independently and can be integrated in any order
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: User Story 1 (T001-T017)
2. **STOP and VALIDATE**: Test UDP connectivity independently with 8.8.8.8:53
3. Verify timeout, error handling, and UI display work correctly
4. Deploy/demo if ready - **basic UDP port testing now available**

**Result**: Administrators can now validate UDP service reachability, a capability that did not exist before. This is a complete, shippable feature.

### Incremental Delivery

1. Ship User Story 1 → **MVP: Basic UDP reachability** ✅
2. Add User Story 2 → Test independently → Ship → **Protocol-specific testing (DNS/SNMP/NTP)** ✅
3. Add User Story 3 → Test independently → Ship → **Policy integration verified** ✅
4. Each story adds value without breaking previous functionality

### Parallel Team Strategy

With 2 developers:

1. Developer A completes User Story 1 (MVP)
2. After US1 validation:
   - Developer A: User Story 2 (payload features)
   - Developer B: User Story 3 (policy verification)
3. Stories complete independently, integrate seamlessly

---

## Summary

- **Total tasks**: 44
- **User Story 1 (MVP)**: 17 tasks (T001-T017)
- **User Story 2**: 11 tasks (T018-T028)
- **User Story 3**: 8 tasks (T029-T036)
- **Polish**: 8 tasks (T037-T044)

**Parallel opportunities**: 12 tasks marked [P] can run concurrently

**Independent test criteria**:
- US1: Run DNS test to 8.8.8.8:53, verify Pass with evidence
- US2: Send hex-encoded DNS query, verify response validation
- US3: Load profile with policies, verify UI renders correctly

**Suggested MVP scope**: User Story 1 only (T001-T017) - delivers core UDP reachability testing

**Estimated time** (from quickstart.md): 3-4 hours total for all user stories

---

## Notes

- [P] tasks = different files, no dependencies within that phase
- [US#] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Field-level policies (US3) require no code changes - only verification
- Commit after each completed task or logical group
- Stop at any checkpoint to validate story independently
- ReqChecker test infrastructure handles discovery, execution, and UI integration automatically
