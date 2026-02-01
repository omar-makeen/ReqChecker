# Tasks: Improve Test List UI/UX

**Input**: Design documents from `/specs/014-improve-test-list/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not required for this UI enhancement (manual verification only).

**Organization**: Tasks are grouped by user story. US1 and US3 are both P1 priority and share the icon converter. US2 is also P1 and can be done in parallel.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: `src/ReqChecker.App/`
- **Converters**: `src/ReqChecker.App/Converters/`
- **Views**: `src/ReqChecker.App/Views/`

---

## Phase 1: Foundational (Converters)

**Purpose**: Create the value converters that will be used by the view

**⚠️ NOTE**: Both US1 (icons) and US3 (colors) require converters before XAML changes can be made

### Converter Implementation

- [ ] T001 [P] Create `TestTypeToIconConverter.cs` in `src/ReqChecker.App/Converters/` - Maps test Type string to SymbolRegular icon (Ping→Signal24, HttpGet→Globe24, etc.)
- [ ] T002 [P] Create `TestTypeToColorConverter.cs` in `src/ReqChecker.App/Converters/` - Maps test Type string to category color brush (Network→StatusInfo, FileSystem→StatusSkip, System→AccentSecondary)

**Checkpoint**: Both converters compile and are ready for use in XAML

---

## Phase 2: User Story 1 & 3 - Visual Test Type Icons with Color Coding (Priority: P1) 🎯 MVP

**Goal**: Display distinct icons with category colors for each test type

**Independent Test**: Load test list with multiple test types → verify each shows its distinctive colored icon

### Implementation for User Story 1 & 3

- [ ] T003 [US1] Add converter resource declarations to Page.Resources in `src/ReqChecker.App/Views/TestListView.xaml`
- [ ] T004 [US1] [US3] Update SymbolIcon binding in TestListView.xaml to use TestTypeToIconConverter for Symbol property
- [ ] T005 [US3] Update SymbolIcon binding in TestListView.xaml to use TestTypeToColorConverter for Foreground property

**Checkpoint**: Each test type displays its designated icon with category color. Unknown types show beaker with gray.

---

## Phase 3: User Story 2 - Display Test Descriptions (Priority: P1) 🎯 MVP

**Goal**: Show test descriptions below test names in the list

**Independent Test**: Load test list → verify descriptions appear below test names, truncated if long, hidden if empty

### Implementation for User Story 2

- [ ] T006 [US2] Add description TextBlock to test info StackPanel in `src/ReqChecker.App/Views/TestListView.xaml` - positioned between DisplayName and Type row
- [ ] T007 [US2] Configure description TextBlock with TextTrimming="CharacterEllipsis" and MaxHeight="32" for 2-line truncation
- [ ] T008 [US2] Add Visibility binding to description TextBlock using existing NullToVisibilityConverter to hide when Description is null/empty

**Checkpoint**: Descriptions display below test names. Long descriptions truncate. Empty descriptions show no space.

---

## Phase 4: Verification & Polish

**Purpose**: Build, verify all acceptance criteria, ensure theme support

### Build & Test

- [ ] T009 Build application with `dotnet build src/ReqChecker.App`
- [ ] T010 Run application and verify all acceptance criteria from spec.md

### Verification Checklist - US1 (Icons)

- [ ] T011 Verify Ping tests show Signal24 icon
- [ ] T012 Verify HttpGet tests show Globe24 icon
- [ ] T013 Verify DnsLookup tests show Search24 icon
- [ ] T014 Verify FileExists tests show Document24 icon
- [ ] T015 Verify DirectoryExists tests show Folder24 icon
- [ ] T016 Verify ProcessList tests show Apps24 icon
- [ ] T017 Verify RegistryRead tests show Settings24 icon
- [ ] T018 Verify unknown test types show Beaker24 icon (fallback)

### Verification Checklist - US2 (Descriptions)

- [ ] T019 Verify descriptions display below test names
- [ ] T020 Verify long descriptions are truncated with ellipsis
- [ ] T021 Verify tests without descriptions show no empty space
- [ ] T022 Verify layout remains consistent across tests

### Verification Checklist - US3 (Colors)

- [ ] T023 Verify network tests (Ping, HttpGet, DnsLookup) have blue icon color
- [ ] T024 Verify file system tests (FileExists, DirectoryExists) have orange icon color
- [ ] T025 Verify system tests (ProcessList, RegistryRead) have purple icon color
- [ ] T026 Verify unknown types have gray icon color

### Theme & Functionality

- [ ] T027 Verify icons and colors work in dark theme
- [ ] T028 Verify icons and colors work in light theme (if available)
- [ ] T029 Verify existing functionality: click to open test config still works
- [ ] T030 Verify existing functionality: hover effects still work
- [ ] T031 Verify existing functionality: keyboard navigation still works

### Finalize

- [ ] T032 Commit changes with descriptive message
- [ ] T033 Push to feature branch

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Foundational)**: No dependencies - start immediately
- **Phase 2 (US1 & US3)**: Depends on Phase 1 completion (converters must exist)
- **Phase 3 (US2)**: No dependencies on other phases (different XAML changes)
- **Phase 4 (Verification)**: Depends on Phases 1-3 completion

### Task Dependencies

```
T001, T002 (parallel - different files)
    ↓
T003 (must add resources before using)
    ↓
T004, T005 (sequential - same element, but can be combined)
    ↓
T006 → T007 → T008 (sequential - same element)
    ↓
T009 (build after all code changes)
    ↓
T010-T031 (verification - single testing session)
    ↓
T032 → T033 (commit then push)
```

### Parallel Opportunities

- T001 and T002 can be done in parallel (different converter files)
- T006-T008 (US2) can be done in parallel with T003-T005 (US1/US3) after Phase 1
- T011-T031 (verification) can be performed in a single testing session

---

## Implementation Strategy

### Recommended Order (Single Developer)

1. Complete T001-T002 (Foundational - create converters)
2. Complete T003-T005 (US1/US3 - icon binding with color)
3. Complete T006-T008 (US2 - description display)
4. Complete T009 (Build)
5. Complete T010-T031 (Verification)
6. Complete T032-T033 (Commit and push)

### MVP Delivery

After completing T001-T005, US1 (Icons) and US3 (Colors) are independently testable.
After completing T006-T008, US2 (Descriptions) is independently testable.
All three user stories together form the complete feature MVP.

---

## User Story Mapping

| Story | Priority | Tasks | Description |
|-------|----------|-------|-------------|
| Foundation | - | T001, T002 | Create converters (shared infrastructure) |
| US1 | P1 | T003, T004 | Type-specific icons |
| US2 | P1 | T006, T007, T008 | Display descriptions |
| US3 | P2 | T005 | Icon color coding by category |
| Polish | - | T009-T033 | Build, verify, commit, push |

---

## Notes

- US1 (icons) and US3 (colors) share the same SymbolIcon element, so they're combined in Phase 2
- US2 (descriptions) is independent and can be done in parallel after foundational phase
- All three P1 stories can be delivered together as a single MVP
- No automated tests required - manual verification sufficient for UI changes
- Commit after all verification passes to ensure fix is complete
