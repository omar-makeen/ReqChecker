# Tasks: Fix Profile Selector View

**Input**: Design documents from `/specs/011-fix-profile-view/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not required for this bug fix (manual verification only).

**Organization**: Tasks are grouped by user story. Note that US1 and US2 are both P1 priority and are fixed by the same code change, so they share implementation tasks.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: `src/ReqChecker.App/` (WPF Application)
- **Target File**: `src/ReqChecker.App/Views/ProfileSelectorView.xaml`

---

## Phase 1: Implementation (Bug Fix)

**Purpose**: Fix the WPF binding error that causes cascading error dialogs

**⚠️ NOTE**: This is a minimal bug fix with no setup or foundational phase needed.

### User Story 1 & 2 - Navigate to Profiles Without Crash + View Profile Cards (Priority: P1) 🎯 MVP

**Goal**: Fix binding error so users can navigate to Profiles view and see profile cards with correct test counts

**Independent Test**: Click "Profiles" in navigation → view loads without error dialogs → profile cards display test counts correctly

### Implementation

- [x] T001 [US1] Add `Mode=OneWay` to `Tests.Count` binding on line 214 in `src/ReqChecker.App/Views/ProfileSelectorView.xaml`
- [x] T002 [US1] Add `Mode=OneWay` to `SchemaVersion` binding on line 229 in `src/ReqChecker.App/Views/ProfileSelectorView.xaml` (preventive fix)

**Checkpoint**: Profile Selector view loads without errors, displays test counts correctly

---

## Phase 2: Verification

**Purpose**: Verify the fix works correctly

- [x] T003 Build application with `dotnet build src/ReqChecker.App`
- [ ] T004 Run application and navigate to Profiles view
- [ ] T005 Verify no error dialogs appear
- [ ] T006 Verify profile cards display correct test counts
- [ ] T007 Verify empty state displays when no profiles exist
- [ ] T008 Verify import and refresh functionality works
- [ ] T009 Verify visual styling matches app aesthetic (premium design)

**Checkpoint**: All acceptance criteria from spec.md verified

---

## Phase 3: Finalize

**Purpose**: Commit and complete

- [x] T010 Commit changes with descriptive message
- [x] T011 Push to feature branch

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Implementation)**: No dependencies - start immediately
- **Phase 2 (Verification)**: Depends on Phase 1 completion
- **Phase 3 (Finalize)**: Depends on Phase 2 verification passing

### Task Dependencies

```
T001 → T002 (same file, sequential edits)
T002 → T003 (build after code changes)
T003 → T004-T009 (run app after build)
T004-T009 → T010 (commit after verification)
T010 → T011 (push after commit)
```

### Parallel Opportunities

- T004-T009 (verification steps) can be performed in a single testing session
- No code-level parallelization needed (single file change)

---

## Implementation Strategy

### Single-Pass Fix (Recommended)

1. Complete T001-T002 (code changes)
2. Complete T003 (build)
3. Complete T004-T009 (verification)
4. Complete T010-T011 (commit and push)

Total: ~5-10 minutes for complete fix

---

## User Story Mapping

| Story | Priority | Tasks | Description |
|-------|----------|-------|-------------|
| US1 | P1 | T001, T002 | Navigate without crash |
| US2 | P1 | T001, T002 | View profile cards with test count |
| US3 | P2 | - | Visual consistency (no changes needed - design already premium) |

---

## Notes

- Both US1 and US2 are fixed by the same binding mode changes
- US3 (visual consistency) requires no code changes - design is already premium
- This is a minimal bug fix: 2 lines changed in 1 file
- No automated tests required - manual verification sufficient for UI bug fix
- Commit after verification to ensure fix is complete
