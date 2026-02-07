# Tasks: Search & Filter in Test List

**Input**: Design documents from `/specs/035-test-list-search/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Not requested — manual verification only (per plan.md).

**Organization**: Tasks are grouped by user story. Since all three stories share the same two files and the same filter predicate (progressively adding fields), US2 and US3 are incremental single-line additions to the US1 foundation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Foundational (ViewModel Filter Infrastructure)

**Purpose**: Add SearchText property, ICollectionView, filter predicate, and computed properties to the ViewModel. This is the core filtering engine that all user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T001 Add `SearchText` observable property, `FilteredTestsView` ICollectionView property, and `SetupFilteredView()` method that creates the collection view from `SelectableTests` with a Filter predicate in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`
- [x] T002 Add `FilteredCount`, `IsFilterActive`, and `TestCountDisplay` computed properties in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`. `TestCountDisplay` returns "X of Y tests" when filtered, "Y tests" when unfiltered
- [x] T003 Implement `OnSearchTextChanged` partial method that calls `FilteredTestsView.Refresh()` and notifies `FilteredCount`, `IsFilterActive`, `TestCountDisplay`, `IsAllSelected`, `SelectedCount`, and `RunButtonLabel` in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`
- [x] T004 Modify `PopulateSelectableTests()` to set `SearchText = string.Empty` before rebuilding the collection, then call `SetupFilteredView()` after populating `SelectableTests` in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`
- [x] T005 Modify `IsAllSelected` getter to operate on visible items only when `IsFilterActive` is true (iterate `FilteredTestsView.Cast<SelectableTestItem>()` instead of `SelectableTests`) in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`
- [x] T006 Modify `ToggleSelectAllCommand` handler to iterate only visible items in `FilteredTestsView` when `IsFilterActive` is true (existing behavior when no filter) in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`

**Checkpoint**: ViewModel has full filter infrastructure. Filter predicate exists but initially only returns true (no field matching yet — that comes in US1).

---

## Phase 2: User Story 1 — Search Tests by Name (Priority: P1) 🎯 MVP

**Goal**: Users can type in a search box and filter tests by display name. The list updates in real-time, shows "X of Y tests" count, and has a clear (X) button.

**Independent Test**: Load a profile with 8+ tests, type a partial test name, verify only matching tests appear. Clear the search box, verify all tests return.

### Implementation for User Story 1

- [x] T007 [US1] Implement the `FilterTest(object obj)` filter predicate to match `item.Test.DisplayName.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase)` — return true when SearchText is empty/whitespace in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`
- [x] T008 [P] [US1] Add search TextBox to toolbar row (Row 2) in `src/ReqChecker.App/Views/TestListView.xaml`: use `InputField` style, `PlaceholderText="Search by name, type, or description..."`, `Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"`, Search16 icon as leading decoration, inline clear (X) button visible only when text is present
- [x] T009 [US1] Change ListBox `ItemsSource` from `"{Binding SelectableTests}"` to `"{Binding FilteredTestsView}"` in `src/ReqChecker.App/Views/TestListView.xaml`
- [x] T010 [US1] Update the test count badge in the header to bind to `TestCountDisplay` instead of the existing count format in `src/ReqChecker.App/Views/TestListView.xaml`
- [x] T011 [US1] Add no-results empty state in Row 3 (visible when `IsFilterActive` is true AND `FilteredCount` is 0): centered Search24 icon, "No tests match your search" message, styled consistently with existing empty states in `src/ReqChecker.App/Views/TestListView.xaml`

**Checkpoint**: Search by name is fully functional. Users can type, filter, clear, see count. Select All respects filter. Profile reload clears search.

---

## Phase 3: User Story 2 — Search Tests by Type (Priority: P2)

**Goal**: The search also matches against test type labels (e.g., typing "Ping" shows all Ping tests even if "Ping" is not in their display name).

**Independent Test**: Type a test type (e.g., "Ping") into the search box and verify all tests of that type appear, regardless of their display name.

### Implementation for User Story 2

- [x] T012 [US2] Extend the `FilterTest` predicate to also match `item.Test.Type.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase)` using OR logic with the existing DisplayName check in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`

**Checkpoint**: Search now matches both name and type. All US1 behavior still works.

---

## Phase 4: User Story 3 — Search Tests by Description (Priority: P3)

**Goal**: The search also matches against test description text (e.g., typing "google" finds a DNS test whose description mentions google.com).

**Independent Test**: Type a keyword that appears only in a test's description (not its name or type) and verify the test appears in filtered results.

### Implementation for User Story 3

- [x] T013 [US3] Extend the `FilterTest` predicate to also match `item.Test.Description?.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase) == true` using OR logic with the existing DisplayName and Type checks in `src/ReqChecker.App/ViewModels/TestListViewModel.cs`

**Checkpoint**: Search now matches name, type, and description. All previous behavior still works.

---

## Phase 5: Polish & Verification

**Purpose**: Build verification and manual testing per quickstart.md

- [x] T014 Run `dotnet build` and verify zero errors and zero warnings
- [x] T015 Run quickstart.md manual verification: load profile with 5+ mixed-type tests, verify all 9 test scenarios pass (name search, type search, description search, count display, clear button, Select All with filter, run with hidden selected tests, profile reload clears search, empty state message)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 1)**: No dependencies — can start immediately. BLOCKS all user stories.
- **User Story 1 (Phase 2)**: Depends on Phase 1 completion.
- **User Story 2 (Phase 3)**: Depends on Phase 2 completion (extends the filter predicate created in US1).
- **User Story 3 (Phase 4)**: Depends on Phase 3 completion (extends the filter predicate further).
- **Polish (Phase 5)**: Depends on all user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Foundational (Phase 1). Creates the filter predicate.
- **User Story 2 (P2)**: Depends on US1 — adds Type matching to the existing filter predicate.
- **User Story 3 (P3)**: Depends on US2 — adds Description matching to the existing filter predicate.

> **Note**: US2 and US3 are each a single-line addition to the filter predicate created in US1. They are sequential because they modify the same method in the same file.

### Parallel Opportunities

- **T008 [P]** (XAML search box) can run in parallel with **T007** (filter predicate) since they modify different files
- All Phase 1 tasks (T001-T006) are sequential within the same file
- US2 (T012) and US3 (T013) are each one line — minimal overhead

---

## Parallel Example: User Story 1

```
# These can run in parallel (different files):
Task T007: Implement FilterTest predicate in TestListViewModel.cs
Task T008: Add search TextBox to toolbar in TestListView.xaml

# Then sequential (same file - XAML):
Task T009: Change ListBox ItemsSource binding
Task T010: Update test count badge binding
Task T011: Add no-results empty state
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Foundational (T001-T006)
2. Complete Phase 2: User Story 1 (T007-T011)
3. **STOP and VALIDATE**: Test search by name independently
4. Build and verify — feature is usable with name-only search

### Incremental Delivery

1. Complete Foundational → Filter infrastructure ready
2. Add US1 → Name search works → MVP!
3. Add US2 → Type search works → One-line addition to predicate
4. Add US3 → Description search works → One-line addition to predicate
5. Each story adds a search field without breaking previous behavior

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- This feature modifies only 2 files: `TestListViewModel.cs` and `TestListView.xaml`
- US2 and US3 are trivially small (one line each) because they extend the filter predicate from US1
- No new files, packages, or abstractions needed
- Commit after each phase checkpoint for clean history
