# Tasks: Premium History Cards

**Input**: Design documents from `/specs/026-premium-history-cards/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Not requested — no test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/ReqChecker.App/` at repository root
- Converters: `src/ReqChecker.App/Converters/`
- Views: `src/ReqChecker.App/Views/`
- App resources: `src/ReqChecker.App/App.xaml`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Register new converters in the application resource dictionary so they are available to all views.

- [X] T001 Register `FriendlyDateConverter` as a StaticResource in `src/ReqChecker.App/App.xaml` — add `<converters:FriendlyDateConverter x:Key="FriendlyDateConverter" />` alongside the existing converter registrations
- [X] T002 Register `PassRateToBrushConverter` as a StaticResource in `src/ReqChecker.App/App.xaml` — add `<converters:PassRateToBrushConverter x:Key="PassRateToBrushConverter" />` alongside the existing converter registrations

> **Note**: T001 and T002 edit the same file (`App.xaml`) so they must be done sequentially. These registrations will reference converter classes created in Phase 2.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the two new converter classes and update the existing duration converter. These converters MUST be complete before any XAML card template changes can reference them.

**⚠️ CRITICAL**: No XAML view changes (Phase 3–6) can begin until all converters exist and compile.

- [X] T003 [P] Create `FriendlyDateConverter` class in `src/ReqChecker.App/Converters/FriendlyDateConverter.cs` — implement `IValueConverter` that accepts `DateTimeOffset` and returns: "Today at h:mm tt" if same calendar day as `DateTime.Today`, "Yesterday at h:mm tt" if previous calendar day, or "MMM d, yyyy 'at' h:mm tt" for older dates. Use `CultureInfo.InvariantCulture` for formatting. Return `string.Empty` for non-DateTimeOffset input.

- [X] T004 [P] Create `PassRateToBrushConverter` class in `src/ReqChecker.App/Converters/PassRateToBrushConverter.cs` — implement `IValueConverter` that accepts `double` (pass rate 0–100) and `string` parameter ("Background" or "Foreground"). For Foreground mode: return `SolidColorBrush` using `#10b981` (green) for ≥80%, `#f59e0b` (amber) for 50–79%, `#ef4444` (red) for <50%. For Background mode: return the same colors at 20% opacity (alpha `0x33`). Default to Foreground if parameter is null.

- [X] T005 [P] Update `DurationFormatConverter` in `src/ReqChecker.App/Converters/DurationFormatConverter.cs` — add a third formatting branch for durations ≥ 60 seconds that returns `"{minutes}m {seconds}s"` format (e.g., "2m 15s"). Keep existing `<1s` → "Xms" and `1–59s` → "X.Xs" branches unchanged. Handle the 0-second edge case by returning "0s".

**Checkpoint**: All three converters compile. `App.xaml` references resolve. `dotnet build` succeeds.

---

## Phase 3: User Story 1 — Readable Date & Time (Priority: P1) 🎯 MVP

**Goal**: Replace the ambiguous date format with friendly relative dates and 12-hour clock.

**Independent Test**: Run a test suite, navigate to Test History, confirm today's run shows "Today at [time] PM/AM" instead of "Feb 06, 2026 12:07".

### Implementation for User Story 1

- [X] T006 [US1] Update the date/time binding in the card item template in `src/ReqChecker.App/Views/HistoryView.xaml` — replace the `StartTime` TextBlock (currently using `StringFormat='{}{0:MMM dd, yyyy HH:mm}'`) with a binding that uses the `FriendlyDateConverter`: `Text="{Binding StartTime, Converter={StaticResource FriendlyDateConverter}}"`. Keep the same `Style="{DynamicResource TextCaption}"` and `Foreground="{DynamicResource TextTertiary}"` styling. Also add a calendar icon (`ui:SymbolIcon Symbol="CalendarLtr24" FontSize="12"`) before the date text for visual context.

**Checkpoint**: Date field now shows "Today at 2:07 PM" / "Yesterday at 9:30 AM" / "Feb 3, 2026 at 12:01 AM".

---

## Phase 4: User Story 2 — Labeled Duration with Icon (Priority: P1)

**Goal**: Make the duration field self-explanatory by adding a timer icon and "Duration:" label.

**Independent Test**: Run a test suite, navigate to Test History, confirm duration shows "Duration: 4.6s" with a timer icon instead of bare "4.6s".

### Implementation for User Story 2

- [X] T007 [US2] Update the duration section in the card item template in `src/ReqChecker.App/Views/HistoryView.xaml` — replace the current duration TextBlock with a StackPanel containing: (1) a timer icon `ui:SymbolIcon Symbol="Timer24" FontSize="12"` with `Foreground="{DynamicResource TextTertiary}"`, (2) a TextBlock with `Text="Duration:"` styled as `TextCaption` with `TextTertiary` foreground, and (3) the existing duration binding `Text="{Binding Duration, Converter={StaticResource DurationFormatConverter}}"`. Separate the date section from the duration section with a dot separator `" • "` as currently exists. Keep items horizontally aligned.

**Checkpoint**: Duration field shows timer icon + "Duration: 4.6s" (or "Duration: 2m 15s" for longer runs).

---

## Phase 5: User Story 3 — Descriptive Pass Rate Badge (Priority: P1)

**Goal**: Replace the bare "75%" with a labeled, color-coded "Pass Rate: 75%" badge.

**Independent Test**: Run tests with varying pass rates, navigate to Test History, confirm the badge shows "Pass Rate: [N]%" with green/amber/red coloring.

### Implementation for User Story 3

- [X] T008 [US3] Replace the pass rate badge in the card item template in `src/ReqChecker.App/Views/HistoryView.xaml` — update the existing `Border` + `TextBlock` that displays `Summary.PassRate` (currently a plain badge with `BackgroundSurface` background and `TextSecondary` foreground). Change the Border background to bind `Summary.PassRate` through `PassRateToBrushConverter` with `ConverterParameter=Background`. Change the TextBlock foreground to bind through the same converter with `ConverterParameter=Foreground`. Update the text `StringFormat` from `'{}{0:N0}%'` to `'Pass Rate: {0:N0}%'`. Add `FontWeight="Medium"` to the text for emphasis. Keep existing `CornerRadius="4"` and `Padding="8,4"`.

**Checkpoint**: Badge shows "Pass Rate: 75%" with amber background, "Pass Rate: 100%" with green background, "Pass Rate: 25%" with red background.

---

## Phase 6: User Story 4 — Labeled Test Status Indicators (Priority: P1)

**Goal**: Add text labels ("Passed", "Failed", "Skipped") next to the colored status dots so meaning is conveyed without relying on color alone.

**Independent Test**: Run a test suite, navigate to Test History, confirm status row shows "3 Passed", "1 Failed", "0 Skipped" with colored dots.

### Implementation for User Story 4

- [X] T009 [US4] Update the status indicators section in the card item template in `src/ReqChecker.App/Views/HistoryView.xaml` — modify the three `StackPanel` groups in the status row. For each indicator, change the `TextBlock StringFormat` to include the status label: Passed indicator `StringFormat='{}{0} Passed'`, Failed indicator `StringFormat='{}{0} Failed'`, Skipped indicator `StringFormat='{}{0} Skipped'`. Keep existing `Ellipse` colored dots (8x8, `StatusPass`/`StatusFail`/`StatusSkip` fills) and margin spacing. Ensure the text `Foreground` for each label matches its status color (`StatusPass` for Passed, `StatusFail` for Failed, `StatusSkip` for Skipped) instead of the current uniform `TextSecondary`, so the label is color-reinforced alongside the dot.

**Checkpoint**: Status row shows "3 Passed" (green text + dot), "1 Failed" (red text + dot), "0 Skipped" (amber text + dot).

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across all stories and both themes.

- [X] T010 Build and launch the application (`dotnet build && dotnet run` from `src/ReqChecker.App/`) — verify no compile errors or XAML parsing failures
- [ ] T011 Visual verification in dark theme — navigate to Test History and confirm all four improvements render correctly: friendly date, labeled duration, color-coded pass rate badge, and labeled status indicators. Verify no text clipping, overflow, or misalignment.
- [ ] T012 Visual verification in light theme — toggle to Light Mode and repeat the same checks. Confirm all labels, badge colors, and icons remain readable against the light background.
- [ ] T013 Edge case verification — confirm: (a) very long profile names truncate with ellipsis without pushing the pass rate badge off-screen, (b) duration of 0 seconds shows "Duration: 0s", (c) runs from a different year show the full date with year

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: T003, T004, T005 can run in parallel (different files). Must complete before Phase 3–6.
- **User Stories (Phase 3–6)**: All depend on Foundational (Phase 2) completion. All edit `HistoryView.xaml` so they must be done **sequentially** (T006 → T007 → T008 → T009).
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on T003 (FriendlyDateConverter). No dependency on other stories.
- **User Story 2 (P1)**: Depends on T005 (DurationFormatConverter update). No dependency on other stories.
- **User Story 3 (P1)**: Depends on T004 (PassRateToBrushConverter). No dependency on other stories.
- **User Story 4 (P1)**: No converter dependency — pure XAML text change. No dependency on other stories.

> **Important**: While the stories are logically independent, they all modify the same XAML card template in `HistoryView.xaml`. To avoid merge conflicts, implement them sequentially within the same file.

### Within Each User Story

- Converter (Phase 2) before XAML binding (Phase 3–6)
- Each story is a single task modifying the card template

### Parallel Opportunities

- **Phase 2**: T003, T004, T005 can all run in parallel (3 separate `.cs` files)
- **Phase 1**: T001, T002 must be sequential (same file `App.xaml`)
- **Phase 3–6**: Must be sequential (same file `HistoryView.xaml`)

---

## Parallel Example: Foundational Phase

```bash
# Launch all converter tasks together (Phase 2):
Task: "Create FriendlyDateConverter in src/ReqChecker.App/Converters/FriendlyDateConverter.cs"
Task: "Create PassRateToBrushConverter in src/ReqChecker.App/Converters/PassRateToBrushConverter.cs"
Task: "Update DurationFormatConverter in src/ReqChecker.App/Converters/DurationFormatConverter.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Register converters in App.xaml
2. Complete Phase 2: Create all 3 converters (parallel)
3. Complete Phase 3: Update date binding in HistoryView.xaml
4. **STOP and VALIDATE**: Friendly date displays correctly in both themes

### Incremental Delivery

1. Complete Setup + Foundational → All converters ready
2. Add User Story 1 (date) → Validate → Most impactful single change
3. Add User Story 2 (duration) → Validate → Second metadata field clarified
4. Add User Story 3 (pass rate) → Validate → Key metric now labeled and color-coded
5. Add User Story 4 (status labels) → Validate → Full accessibility compliance
6. Polish → Final cross-theme verification

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All 4 user stories are P1 (equally critical) but are ordered by visual hierarchy: date → duration → pass rate → status
- No tests were requested in the spec — verification is manual per quickstart.md
- Commit after each phase for clean rollback capability
