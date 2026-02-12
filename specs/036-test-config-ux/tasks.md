# Tasks: Improve Test Configuration Page UI/UX

**Input**: Design documents from `/specs/036-test-config-ux/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Tests**: Not requested for this UI-only feature. Verification is visual per quickstart.md.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- Files affected: `src/ReqChecker.App/Views/TestConfigView.xaml`, `src/ReqChecker.App/Controls/LockedFieldControl.xaml`, `src/ReqChecker.App/Resources/Styles/Controls.xaml`

---

## Phase 1: Setup

**Purpose**: No setup needed — all files already exist, no new dependencies required.

*This phase is empty. Proceed directly to Phase 2.*

---

## Phase 2: Foundational (Shared Style Changes)

**Purpose**: Update the shared ParameterGroupCard style that all three sections depend on. MUST complete before user story work since US1 and US3 both depend on card styling.

- [x] T001 Update ParameterGroupCard style in `src/ReqChecker.App/Resources/Styles/Controls.xaml`: change Padding from "20" to "16", change Margin from "0,0,0,16" to "0,0,0,12", change BorderBrush from `BorderDefault` to `BorderSubtle`, and remove the entire `<Setter Property="Effect">` block (DropShadowEffect)

**Checkpoint**: Build succeeds. Cards render with tighter padding, smaller gaps, no glow. All 3 sections still visible on TestConfigView.

---

## Phase 3: User Story 1 - Scan and Edit Configuration Quickly (Priority: P1) — MVP

**Goal**: Reduce excessive spacing throughout the page so all three sections fit on a 1080p display without scrolling.

**Independent Test**: Navigate to any test's configuration page on a 1080p display and verify all three sections (Basic Information, Execution Settings, Test Parameters) are fully visible without scrolling.

### Implementation for User Story 1

- [x] T002 [US1] Reduce page outer margin in `src/ReqChecker.App/Views/TestConfigView.xaml`: change root Grid Margin from "32" to "24"
- [x] T003 [US1] Reduce header bottom margin in `src/ReqChecker.App/Views/TestConfigView.xaml`: change the header Grid Margin from "0,0,0,24" to "0,0,0,16"
- [x] T004 [US1] Reduce all section header bottom margins in `src/ReqChecker.App/Views/TestConfigView.xaml`: change the 3 section header StackPanel Margins from "0,0,0,20" to "0,0,0,12" (Basic Information, Execution Settings, Test Parameters sections)
- [x] T005 [US1] Reduce all field row bottom margins in `src/ReqChecker.App/Views/TestConfigView.xaml`: change all field Grid Margins from "0,0,0,16" to "0,0,0,12" (Name, Type, Timeout rows and parameter ItemsControl DataTemplate row)
- [x] T006 [US1] Reduce label column width in `src/ReqChecker.App/Views/TestConfigView.xaml`: change all ColumnDefinition Width values from "140" to "120" (all field rows in Basic Information, Execution Settings, and Test Parameters sections)
- [x] T007 [US1] Reduce section header icon badges in `src/ReqChecker.App/Views/TestConfigView.xaml`: change all 3 icon badge Borders from Width="32" Height="32" to Width="24" Height="24", and change their inner SymbolIcon FontSize from "16" to "14" (Info24, Timer24, SlideSettings24 icons)
- [x] T008 [US1] Update footer margins to match new page margin in `src/ReqChecker.App/Views/TestConfigView.xaml`: change footer Border Margin from "-32,24,-32,-32" to "-24,16,-24,-24" and Padding from "32,16" to "24,12"

**Checkpoint**: Build succeeds. All 3 sections visible without scrolling on 1080p. Labels closer to values. Icons proportional to headings. Footer aligns with new page margins. All buttons (Save, Cancel, Back) still functional. Entrance animations still play.

---

## Phase 4: User Story 2 - Distinguish Editable from Read-Only Fields (Priority: P2)

**Goal**: Simplify locked field indicators and make "Requires Admin" visually consistent with other read-only fields.

**Independent Test**: Open any test configuration and verify locked fields (Name, Type, Requires Admin) display as muted inline text with a small lock icon, while editable fields (Timeout, Retries) have standard input styling.

### Implementation for User Story 2

- [x] T009 [US2] Simplify LockedFieldControl layout in `src/ReqChecker.App/Controls/LockedFieldControl.xaml`: replace the two-column Grid (separate lock icon Border + separate value Border) with a single Border containing a horizontal StackPanel with an inline LockClosed24 icon (FontSize="14", Foreground=TextTertiary, Margin="0,0,8,0") followed by the value TextBlock. Keep root Opacity="0.7", use Background=BackgroundElevated, BorderBrush=BorderSubtle, BorderThickness="1", CornerRadius="6", Padding="10,8". Preserve the ToolTip on the container Border.
- [x] T010 [US2] Replace "Requires Admin" custom badge markup in `src/ReqChecker.App/Views/TestConfigView.xaml`: remove the existing StackPanel > Border > StackPanel > Shield icon + TextBlock markup and replace with `<controls:LockedFieldControl Grid.Column="1" Value="{Binding RequiresAdminText}"/>` where RequiresAdminText displays "Yes" or "No"
- [x] T011 [US2] Add RequiresAdminText computed property to `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs`: add a string property `RequiresAdminText` that returns `RequiresAdmin ? "Yes" : "No"` and ensure it notifies on change when RequiresAdmin changes

**Checkpoint**: Build succeeds. Name, Type, and Requires Admin fields all display with identical locked field styling (inline lock icon + muted text). Editable fields (Timeout, Retries) remain as standard input boxes. Save/Cancel still work. LockedFieldControl in parameter list still renders correctly for locked parameters.

---

## Phase 5: User Story 3 - Visual Hierarchy Without Excessive Decoration (Priority: P2)

**Goal**: Ensure section groupings are clear through the updated card styling (already done in Phase 2) and add label truncation for edge cases.

**Independent Test**: Open a test configuration page and verify cards have subtle borders without glows, section headers have proportional icons, and long parameter labels truncate with tooltips.

### Implementation for User Story 3

- [x] T012 [US3] Add label truncation to parameter labels in `src/ReqChecker.App/Views/TestConfigView.xaml`: in the ItemsControl DataTemplate, add `TextTrimming="CharacterEllipsis"` and `ToolTip="{Binding Label}"` to the parameter label TextBlock, and set `MaxWidth="110"` to ensure truncation works within the 120px column

**Checkpoint**: Build succeeds. Long parameter labels truncate with ellipsis and show full text in tooltip. Cards display with subtle borders (no glow). Section header icons are 24x24.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and build verification

- [x] T013 Build the full solution to verify no compilation errors: `dotnet build src/ReqChecker.App/ReqChecker.App.csproj`
- [x] T014 Run existing unit tests to verify no regressions: `dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj`
- [ ] T015 Run quickstart.md visual verification checklist (manual): launch app, navigate to test configuration, verify all acceptance scenarios from spec.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies — can start immediately. Updates ParameterGroupCard style.
- **User Story 1 (Phase 3)**: Depends on Phase 2 (card style must be updated first since spacing compounds)
- **User Story 2 (Phase 4)**: Depends on Phase 2 only (LockedFieldControl changes are independent of spacing)
- **User Story 3 (Phase 5)**: Depends on Phase 2 (card styling already done) — can run in parallel with US1/US2
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Depends on T001 (ParameterGroupCard). No dependencies on US2 or US3.
- **User Story 2 (P2)**: Depends on T001 (ParameterGroupCard). No dependencies on US1 or US3. Can run in parallel with US1.
- **User Story 3 (P2)**: Depends on T001 (ParameterGroupCard). No dependencies on US1 or US2. Can run in parallel with US1 and US2.

### Within Each User Story

- US1: T002-T008 are all in the same file (TestConfigView.xaml) — execute sequentially
- US2: T009 (LockedFieldControl.xaml) and T011 (ViewModel) can run in parallel, then T010 (TestConfigView.xaml) depends on both
- US3: T012 is a single task

### Parallel Opportunities

- After T001 completes: US1 (T002-T008), US2 (T009+T011), and US3 (T012) can all start in parallel since they touch different files initially
- Within US2: T009 (LockedFieldControl.xaml) and T011 (TestConfigViewModel.cs) can run in parallel

---

## Parallel Example: After Phase 2

```bash
# These can all run in parallel after T001 completes:

# US1 - Spacing changes (TestConfigView.xaml):
Task: T002-T008 (sequential within same file)

# US2 - Locked field redesign (LockedFieldControl.xaml + TestConfigViewModel.cs):
Task: T009 "Simplify LockedFieldControl in LockedFieldControl.xaml"
Task: T011 "Add RequiresAdminText property in TestConfigViewModel.cs"
# Then T010 depends on T009 + T011 (TestConfigView.xaml)

# US3 - Label truncation (TestConfigView.xaml):
Task: T012 "Add label truncation to parameter labels"
```

**Note**: US1 and US3 both modify TestConfigView.xaml. If implementing sequentially, complete US1 first (P1 priority), then US3. If implementing in parallel, US3 (T012) modifies a different section (ItemsControl DataTemplate) than US1 spacing changes, so merge conflicts would be minimal.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete T001: ParameterGroupCard style update
2. Complete T002-T008: All spacing reductions
3. **STOP and VALIDATE**: All 3 sections visible on 1080p without scrolling
4. This alone achieves SC-001 and SC-002 (25% vertical space reduction)

### Incremental Delivery

1. T001 → Foundation ready (lighter cards)
2. T002-T008 → US1 complete (compact layout) → Validate SC-001, SC-002
3. T009-T011 → US2 complete (consistent locked fields) → Validate SC-003
4. T012 → US3 complete (label truncation) → Validate SC-004
5. T013-T015 → Polish (build, test, visual check) → Validate SC-005

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- No new files are created — all changes modify existing files
- No new dependencies — all packages already in project
- ViewModel change (T011) is the only non-XAML change — a single computed property
- Total scope: 3 XAML files + 1 minor ViewModel addition
