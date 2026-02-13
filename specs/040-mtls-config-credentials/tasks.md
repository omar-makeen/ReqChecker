# Tasks: Move mTLS Credentials to Test Configuration

**Input**: Design documents from `/specs/040-mtls-config-credentials/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Test tasks are included for the core credential resolution logic (SequentialTestRunner) as it is the critical behavioral change.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: No project initialization needed — all changes are to existing files. This phase is empty.

*No setup tasks required. All existing projects, dependencies, and structure are in place.*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Update the default profile to use `pfxPassword` instead of `credentialRef`. This must be done first because both US1 and US3 depend on the profile having the new parameter.

- [x] T001 Update mTLS test entries in `src/ReqChecker.App/Profiles/default-profile.json`: replace `credentialRef` with `pfxPassword` parameter, update `fieldPolicy` to set `pfxPassword` as `Editable`, remove `credentialRef` and its field policy entry from all mTLS test definitions

**Checkpoint**: Default profile uses `pfxPassword`. User story implementation can now begin.

---

## Phase 3: User Story 1 + 3 - Configure PFX Passphrase & Remove Runtime Dialog (Priority: P1) MVP

**Goal**: The test runner reads `pfxPassword` from test parameters and creates a `TestExecutionContext` directly — no credential dialog is shown. US1 and US3 are merged because they are both P1 and tightly coupled: reading the passphrase from config (US1) inherently removes the dialog (US3).

**Independent Test**: Run an mTLS test with `pfxPassword` configured in the profile. The test should execute without any dialog appearing. Run an mTLS test with only `credentialRef` (no `pfxPassword`) — the legacy credential dialog should still appear (backward compatibility).

### Tests for User Story 1+3

- [x] T002 [P] [US1] Add test `PfxPassword_InParameters_CreatesContextWithoutPrompt` in `tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs`: given a TestDefinition with `pfxPassword` parameter, verify `PromptForCredentials` callback is NOT invoked and the returned `TestExecutionContext.Password` matches the configured value
- [x] T003 [P] [US1] Add test `PfxPassword_Empty_CreatesContextWithNullPassword` in `tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs`: given a TestDefinition with `pfxPassword` set to empty string, verify context is created with null/empty password (no passphrase)
- [x] T004 [P] [US1] Add test `PfxPassword_TakesPrecedenceOverCredentialRef` in `tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs`: given a TestDefinition with BOTH `pfxPassword` and `credentialRef`, verify `pfxPassword` is used and prompt is NOT invoked
- [x] T005 [P] [US1] Add test `CredentialRef_WithoutPfxPassword_StillPromptsUser` in `tests/ReqChecker.Infrastructure.Tests/Execution/SequentialTestRunnerTests.cs`: given a TestDefinition with `credentialRef` but no `pfxPassword`, verify the existing prompt flow still works (backward compatibility)

### Implementation for User Story 1+3

- [x] T006 [US1] Add `pfxPassword` check in `PromptForCredentialsIfNeededAsync` method in `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs`: before the existing `credentialRef` check, add a block that checks if `testDefinition.Parameters.ContainsKey("pfxPassword")`, reads the value, and returns `new TestExecutionContext(string.Empty, pfxPassword)` directly without invoking any prompt callback
- [x] T007 [P] [US1] Update error message in `LoadClientCertificate` method in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs`: change the `CryptographicException` catch block's error detail to include guidance about the `pfxPassword` parameter (e.g., "Cannot load PFX file: incorrect passphrase. Check the pfxPassword parameter in test configuration.")

**Checkpoint**: mTLS tests with `pfxPassword` run without dialog. Legacy `credentialRef` tests still prompt. All automated tests pass.

---

## Phase 4: User Story 2 - Edit PFX Passphrase via Test Configuration UI (Priority: P2)

**Goal**: The test configuration panel renders the `pfxPassword` field as a masked `PasswordBox` input so users can configure the passphrase visually.

**Independent Test**: Open an mTLS test configuration. Verify a "PFX Password" field appears with masked (dot) input. Enter a value, navigate away, and return — the value should be preserved (masked).

### Implementation for User Story 2

- [x] T008 [P] [US2] Add `IsPassword` boolean property to `TestParameterViewModel` inner class in `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`: set it to `true` when the parameter `Name` ends with "Password" (case-insensitive). Add corresponding `IsNotPassword` property (inverse) for XAML visibility binding
- [x] T009 [US2] Add `PasswordBox` to the editable field DataTemplate in `src/ReqChecker.App/Views/TestConfigView.xaml`: within the existing editable field section of the parameter `ItemsControl` DataTemplate, add a `PasswordBox` with `Style="{StaticResource CredentialPasswordBox}"` that is visible when `IsPassword` is true, and hide the existing `TextBox` when `IsPassword` is true (using `IsNotPassword` binding). Wire `PasswordChanged` event to sync the value back to the `TestParameterViewModel.Value` property via code-behind
- [x] T010 [US2] Add `ParameterPasswordBox_PasswordChanged` event handler in `src/ReqChecker.App/Views/TestConfigView.xaml.cs` (code-behind): when the `PasswordBox` value changes, update the bound `TestParameterViewModel.Value` property. On DataTemplate load, if `IsPassword` is true and `Value` is not empty, set the `PasswordBox.Password` to the existing value

**Checkpoint**: mTLS test configuration shows masked PFX Password field. Value persists when navigating away and back. All automated tests pass.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [x] T011 Build solution: run `dotnet build src/ReqChecker.App/ReqChecker.App.csproj` and verify zero errors
- [x] T012 Run all tests: run `dotnet test` and verify all tests pass (including new T002-T005 tests)
- [ ] T013 Run quickstart.md manual verification checklist

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies — can start immediately
- **User Story 1+3 (Phase 3)**: Depends on Phase 2 (needs `pfxPassword` in profile)
- **User Story 2 (Phase 4)**: Depends on Phase 2 (needs `pfxPassword` in profile). Can run in parallel with Phase 3
- **Polish (Phase 5)**: Depends on all phases complete

### User Story Dependencies

- **User Story 1+3 (P1)**: Can start after Foundational (Phase 2) — no dependencies on US2
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — no dependencies on US1/US3

### Within Each User Story

- Tests MUST be written and FAIL before implementation (T002-T005 before T006)
- Implementation tasks within a story follow sequential order unless marked [P]

### Parallel Opportunities

- T002, T003, T004, T005 can all run in parallel (all add independent test methods to the same file but different test cases)
- T007 can run in parallel with T006 (different files)
- T008 can run in parallel with Phase 3 tasks (different files)
- Phase 3 (US1+3) and Phase 4 (US2) can run in parallel after Phase 2

---

## Parallel Example: User Story 1+3

```bash
# Launch all test tasks in parallel:
Task: "T002 - Test PfxPassword creates context without prompt"
Task: "T003 - Test PfxPassword empty creates null password context"
Task: "T004 - Test PfxPassword takes precedence over credentialRef"
Task: "T005 - Test credentialRef still prompts without pfxPassword"

# Then implementation (T006 sequential, T007 parallel):
Task: "T006 - Add pfxPassword check in SequentialTestRunner"
Task: "T007 - Update error message in MtlsConnectTest" (parallel with T006)
```

---

## Implementation Strategy

### MVP First (User Story 1+3 Only)

1. Complete Phase 2: Foundational (update default profile)
2. Complete Phase 3: User Story 1+3 (credential resolution + tests)
3. **STOP and VALIDATE**: Run mTLS test with `pfxPassword` — no dialog should appear
4. Deploy/demo if ready — dialog issue is fully resolved at this point

### Incremental Delivery

1. Phase 2 → Profile updated with `pfxPassword`
2. Phase 3 → Core fix: tests run without dialog (MVP!)
3. Phase 4 → UI enhancement: masked password field in config panel
4. Phase 5 → Final validation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- US1 and US3 are merged into a single phase because they are both P1 and implementing one inherently implements the other
- The existing `CredentialPromptDialog` and `CredentialPromptViewModel` are NOT deleted — they remain for potential future use by other test types
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
