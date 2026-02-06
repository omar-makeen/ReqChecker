# Tasks: Premium Results Dashboard

**Input**: Design documents from `/specs/027-premium-results/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)

**Tests**: Not requested — no test tasks included.

**Organization**: Tasks are grouped by user story. All changes target a single file (`src/ReqChecker.App/Views/ResultsView.xaml`), so parallelism within stories is limited, but stories themselves are independent.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: User Story 1 — Readable Date and Duration in Run Summary (Priority: P1) MVP

**Goal**: Replace raw machine-format date and duration in the summary card with human-friendly formats using existing converters.

**Independent Test**: Run any test suite, navigate to Results Dashboard, confirm Started shows "Today at h:mm AM/PM" and Duration shows compact format like "4.6s".

### Implementation for User Story 1

- [X] T001 [US1] Replace the Started field `<TextBlock>` with `<Run>` elements with a `<StackPanel>` containing label and value `<TextBlock>`, applying `FriendlyDateConverter` to `Report.StartTime` in `src/ReqChecker.App/Views/ResultsView.xaml` (lines ~232-238)
- [X] T002 [US1] Replace the Duration field `<TextBlock>` with `<Run>` elements with a `<StackPanel>` containing label and value `<TextBlock>`, applying `DurationFormatConverter` to `Report.Duration` in `src/ReqChecker.App/Views/ResultsView.xaml` (lines ~240-245)
- [X] T003 [US1] Build and verify: run `dotnet build src/ReqChecker.App` — must compile with 0 errors

**Checkpoint**: Summary card shows friendly dates and compact durations. Core formatting issue resolved.

---

## Phase 2: User Story 2 — Profile Name Overflow Handling (Priority: P2)

**Goal**: Prevent long profile names from breaking the summary card layout by adding text truncation with ellipsis and a tooltip showing the full name.

**Independent Test**: Load a profile with a 60+ character name, navigate to Results Dashboard, confirm the name truncates with "..." and hovering shows the full name.

### Implementation for User Story 2

- [X] T004 [US2] Replace the Profile field `<TextBlock>` with `<Run>` elements with a `<StackPanel>` containing label `<TextBlock>` and value `<TextBlock>` with `TextTrimming="CharacterEllipsis"`, `MaxWidth="350"`, and `ToolTip="{Binding Report.ProfileName}"` in `src/ReqChecker.App/Views/ResultsView.xaml` (lines ~224-230)
- [X] T005 [US2] Build and verify: run `dotnet build src/ReqChecker.App` — must compile with 0 errors

**Checkpoint**: Summary card handles long profile names gracefully. Layout remains intact with names up to 100 characters.

---

## Phase 3: User Story 3 — Consistent Duration Format in Test Result Cards (Priority: P3)

**Goal**: Replace the raw millisecond `StringFormat` on ExpanderCard subtitles with the same `DurationFormatConverter` used in the summary card, ensuring consistency across the entire Results page.

**Independent Test**: Run a test suite with tests of varying durations, expand result cards, confirm subtitles show "250ms" / "5.3s" / "1m 30s" instead of "250ms" / "5,300ms" / "90,000ms".

### Implementation for User Story 3

- [X] T006 [US3] Replace `Subtitle="{Binding Duration, StringFormat='{}{0:N0}ms'}"` with `Subtitle="{Binding Duration, Converter={StaticResource DurationFormatConverter}}"` on the ExpanderCard in `src/ReqChecker.App/Views/ResultsView.xaml` (lines ~389)
- [X] T007 [US3] Build and verify: run `dotnet build src/ReqChecker.App` — must compile with 0 errors

**Checkpoint**: All duration values on the Results page (summary + individual cards) use the same compact format.

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Final verification across all changes

- [X] T008 Visual consistency check: compare Results Dashboard date/duration formatting with Test History page — both must use identical styles
- [X] T009 Verify edge cases: 0-second duration shows "0s", short profile names display without truncation, tooltip only appears on truncated names

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (US1)**: No dependencies — can start immediately
- **Phase 2 (US2)**: No dependencies on US1 — can start immediately (different XAML block)
- **Phase 3 (US3)**: No dependencies on US1/US2 — can start immediately (different XAML block)
- **Phase 4 (Polish)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent — summary card date/duration (lines ~232-245)
- **User Story 2 (P2)**: Independent — summary card profile name (lines ~224-230)
- **User Story 3 (P3)**: Independent — ExpanderCard subtitle (lines ~389)

### Parallel Opportunities

- T001 and T002 are sequential (same XAML block, adjacent lines)
- T004 can run in parallel with T001/T002 (different XAML block)
- T006 can run in parallel with T001/T002/T004 (different XAML block)
- All three user stories touch different parts of the same file and could theoretically be implemented in a single pass

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001 + T002 + T003: Summary card date/duration fixed
2. **STOP and VALIDATE**: Run app, check friendly formatting
3. This alone resolves the most visible issue from the screenshot

### Recommended: Single Pass

Given that all changes are in one file and total ~20 lines of XAML:

1. Implement T001 + T002 + T004 + T006 in a single editing session
2. Build once (T003/T005/T007 collapse into one build)
3. Verify all changes together (T008 + T009)

---

## Notes

- All tasks modify `src/ReqChecker.App/Views/ResultsView.xaml` only
- No new files, converters, services, or models needed
- Existing converters (`FriendlyDateConverter`, `DurationFormatConverter`) are already registered in `App.xaml`
- The pattern change from `<Run>` to `<StackPanel>` + `<TextBlock>` is required because `<Run>` does not support converter bindings
