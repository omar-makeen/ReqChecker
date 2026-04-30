---
description: "Task list for Profile Manager List Redesign (Premium UI/UX)"
---

# Tasks: Profile Manager List Redesign (Premium UI/UX)

**Input**: Design documents from `/specs/068-profile-list-redesign/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/view-model-contract.md, contracts/visual-contract.md, quickstart.md

**Tests**: Mandatory unit tests are listed in `contracts/view-model-contract.md` ("Tests required against this contract"). Test tasks below cover those mandatory tests; no UI test harness is introduced (per research R10).

**Organization**: Tasks are grouped by user story (P1 → P3 from spec.md). The redesign touches a single view + a single page VM + one new per-row VM, so tasks are scoped to specific edits inside those files.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: User story label (US1 … US6) — only on user-story phase tasks
- All paths are absolute under the repo root: `C:\workspace\pulsar\ReqChecker\`

## Path Conventions

- App view: `src/ReqChecker.App/Views/ProfileSelectorView.xaml` and `.xaml.cs`
- App view models: `src/ReqChecker.App/ViewModels/`
- App tests: `tests/ReqChecker.App.Tests/ViewModels/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm a clean baseline before redesign edits begin.

- [x] T001 Confirm baseline build succeeds on branch `068-profile-list-redesign` by running `dotnet build src/ReqChecker.App/ReqChecker.App.csproj -c Debug` and `dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj` from the repo root; resolve any pre-existing failures before continuing.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build the per-row VM and the page-VM extensions every user story binds against. No user-story phase can begin until this phase completes.

**⚠️ CRITICAL**: Phase 3+ work depends on these.

- [x] T002 [P] Author `tests/ReqChecker.App.Tests/ViewModels/ProfileListItemViewModelTests.cs` with the eight unit tests listed in `specs/068-profile-list-redesign/contracts/view-model-contract.md` ("Tests required against this contract" → `ProfileListItemViewModelTests`): `Constructor_ComputesIsRecommended_WhenIdMatchesDefault`, `Constructor_DefaultsIsActive_ToFalse`, `ModifiedLabel_IsNull_WhenSourcePathIsNull`, `ModifiedLabel_IsPopulated_FromFileLastWriteTime` (use a temp file to control mtime), `SchemaVersionLabel_IsNull_WhenSchemaVersionIsZero`, `TestCountLabel_PluralizesCorrectly` (cases 0/1/2), `AccessibleName_IncludesRecommendedSuffix_WhenIsRecommended`, `IsActive_RaisesPropertyChanged_WhenSet`. Tests MUST fail before T003 lands.
- [x] T003 Implement `src/ReqChecker.App/ViewModels/ProfileListItemViewModel.cs` per `specs/068-profile-list-redesign/data-model.md` ("New presentation entity"): `ObservableObject` subclass with constructor `(Profile profile, string? sourceFilePath, bool isRecommended)`, read-only properties `Profile`/`Name`/`SourceLabel`/`TestCountLabel`/`SchemaVersionLabel`/`ModifiedLabel`/`LastModifiedUtc`/`IsRecommended`/`AccessibleName`, observable `IsActive` with `internal` setter, file-mtime read via `File.GetLastWriteTimeUtc(sourceFilePath)` guarded for missing file, formatted strings produced in-VM (no XAML converters). Make T002 tests pass.
- [x] T004 [P] Extend `tests/ReqChecker.App.Tests/ViewModels/ProfileSelectorViewModelTests.cs` with the six tests listed in `contracts/view-model-contract.md` ("Tests required against this contract" → `ProfileSelectorViewModelTests` (extensions)): `LoadProfiles_PopulatesItemsCollection`, `LoadProfiles_SetsSelectedItem_WhenAppStateHasCurrentProfile`, `OnCurrentProfileChanged_UpdatesIsActiveForMatchingItem`, `OnCurrentProfileChanged_ClearsIsActiveForOtherItems`, `SelectingItemThatEqualsCurrentProfile_DoesNotRefireNavigation`, `Dispose_UnsubscribesFromCurrentProfileChanged`. Tests MUST fail before T005 lands.
- [x] T005 Extend `src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs` to add: (a) `ObservableCollection<ProfileListItemViewModel> Items` populated alongside `Profiles` during `LoadProfilesAsync` (capture each user profile's file path from `IProfileStorageService.GetProfileFilePaths()`; pass `null` for bundled profiles), (b) two-way `SelectedItem` observable property whose setter is a no-op when the value's `Profile` equals the current `_appState.CurrentProfile`, (c) subscription to `_appState.CurrentProfileChanged` that toggles `IsActive` on every item and refreshes `SelectedItem`, (d) initial `SelectedItem` set after load when `_appState.CurrentProfile` matches an item, (e) extend `Dispose()` to unsubscribe from `CurrentProfileChanged`. Make T004 tests pass and keep T002/T003 tests green.

**Checkpoint**: VM contract from `view-model-contract.md` is fully implemented and tested. User-story phases (mostly XAML) can now begin.

---

## Phase 3: User Story 1 — Profiles match the rest of the app visually (P1) 🎯 MVP

**Goal**: Replace the wrapping grid of fixed-width cards with a vertical, virtualized list of full-width rows that mirrors `HistoryView`'s pattern.

**Independent Test**: Open Profile Manager and Test History side-by-side at the same window width; the list-item layout pattern (row width, gap, padding, hover treatment) reads as the same component family. Resizing the window keeps rows full width with no wrapping.

### Implementation for User Story 1

- [x] T006 [US1] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, replace the existing `<ScrollViewer>` + `<ItemsControl>` + `<WrapPanel>` block (lines ~265–400 of the current file) with a `<ListBox>` configured exactly as specified in `contracts/view-model-contract.md` ("Wiring contract: `ListBox` configuration"): `ItemsSource="{Binding Items}"`, `SelectedItem="{Binding SelectedItem, Mode=TwoWay}"`, `SelectionMode="Single"`, transparent background, `BorderThickness="0"`, virtualization properties (`IsVirtualizing=True`, `VirtualizationMode=Recycling`, `ScrollUnit=Pixel`), and `AutomationProperties.Name="Profiles list"`.
- [x] T007 [US1] In the same XAML file, add `<ListBox.ItemContainerStyle>` matching `HistoryView`'s pattern (`Padding=0`, `Margin="0,0,0,12"`, `HorizontalContentAlignment=Stretch`, transparent background, `BorderThickness=0`, `FocusVisualStyle="{StaticResource FocusVisualStyle}"`, stripped `Template` whose body is a single `ContentPresenter`) and `<ListBox.ItemsPanel>` set to `<VirtualizingStackPanel/>`.
- [x] T008 [US1] In the same XAML file, define the row `DataTemplate` (skeleton only — no badges or metadata segments yet): outer `<Border Style="{StaticResource Card}" Padding="16" Cursor="Hand">` containing a two-row Grid where the top cell holds the profile name (FontSize=15, FontWeight=SemiBold, `Foreground="{DynamicResource TextPrimary}"`, `TextTrimming=CharacterEllipsis`) and the bottom cell is reserved for metadata (added in US4).
- [x] T009 [US1] In the same XAML file, remove the page-level `<Page.Resources>` entry for the `AnimatedProfileCard` style (the old per-card entrance) since US5 will introduce a list-aware entrance animation.

**Checkpoint**: Profile Manager renders as a virtualized vertical list of full-width rows. Side-by-side with Test History, the rhythm reads as the same component family. (FR-001/002/003 satisfied; US1 acceptance scenarios pass.)

---

## Phase 4: User Story 2 — Selecting a profile takes one obvious action (P1)

**Goal**: Whole-row click activates the profile; keyboard activation via Enter/Space; no per-row Select Profile button; selection is idempotent.

**Independent Test**: Click anywhere on a profile row → it loads. Tab into the list, arrow-key to a row, press Enter → it loads. There is no per-row Select Profile button anywhere in the row template.

### Implementation for User Story 2

- [x] T010 [US2] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, ensure the row `DataTemplate` from T008 contains NO per-row `<Button Content="Select Profile">` (regression guard for FR-006); confirm by searching the file for "Select Profile" and verifying zero matches.
- [x] T011 [US2] In the same XAML file, attach a `MouseLeftButtonUp` event handler on the row's outer Border that invokes `SelectProfileCommand` with the current row's `Profile` (use `Tag="{Binding Profile}"` on the Border or `RelativeSource AncestorType=Page` to reach the page VM; wire the handler in code-behind in T012).
- [x] T012 [US2] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml.cs`, add (a) the `Border_MouseLeftButtonUp` handler that resolves `(sender as Border)?.Tag as Profile` and calls `((ProfileSelectorViewModel)DataContext).SelectProfileCommand.Execute(profile)`, and (b) a `ListBox.KeyDown` handler that on `Key.Enter` or `Key.Space` invokes the same command using `((ProfileSelectorViewModel)DataContext).SelectedItem?.Profile`.
- [x] T013 [US2] In `src/ReqChecker.App/ViewModels/ProfileSelectorViewModel.cs`, modify `SelectProfile(Profile? profile)` so that when `profile` equals `_appState.CurrentProfile` it returns early without invoking `_navigationService.NavigateToTestList()` (idempotence guard verified by `SelectingItemThatEqualsCurrentProfile_DoesNotRefireNavigation` from T004).

**Checkpoint**: Whole-row click and keyboard activation both load the profile; no per-row Select Profile button exists; re-selecting the active profile does not double-navigate. (FR-005/006/007/008 satisfied; US2 acceptance scenarios pass.)

---

## Phase 5: User Story 3 — Recommended profile is identifiable with one signal (P2)

**Goal**: Exactly one labeled `Recommended` badge marks the recommended row; no accent border, no gradient stripe specific to the recommended state.

**Independent Test**: With at least one profile recommended, exactly one design element on its row says "Recommended". With no profile recommended, every row looks identical.

### Implementation for User Story 3

- [x] T014 [US3] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, inside the row template's top-row Grid, add a right-aligned `<Border Style="{StaticResource RecommendedBadge}" Visibility="{Binding IsRecommended, Converter={StaticResource BoolToVisibilityConverter}}">` whose child is `<TextBlock Text="Recommended" FontSize="11" FontWeight="Medium" Foreground="White"/>`. Verify (regression guard for FR-015/016) that the row template contains NO `<DataTrigger>` setting `BorderThickness="2"` or `BorderBrush="{DynamicResource AccentPrimary}"` on the outer Border for the recommended state, and NO 6-px gradient `<Border>` strip at the top of the row (these were in the old card template — they MUST NOT be re-introduced).
- [x] T015 [US3] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, remove the `<converters:ProfileRecommendedConverter x:Key="ProfileRecommendedConverter"/>` entry from `<Page.Resources>` since the new view binds directly to `IsRecommended`. The converter file in `src/ReqChecker.App/Converters/ProfileRecommendedConverter.cs` and its existing test stay in place untouched.

**Checkpoint**: Recommended profile carries exactly one labeled signal; baseline row chrome is identical for every row. (FR-015/016 satisfied; US3 acceptance scenarios pass.)

---

## Phase 6: User Story 4 — Each row carries enough information to choose confidently (P2)

**Goal**: Each row shows source (quiet outlined chip), test count, schema version (when available), and the file's last-modified date (when available); the active profile is rendered in the selected/active visual state.

**Independent Test**: With ≥3 profiles loaded, the user can answer "which has the most tests?" and "which is the newest user-supplied profile?" without clicking. Returning to Profile Manager after selecting a profile shows that row in the selected/active state.

### Implementation for User Story 4

- [x] T016 [US4] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, in the row template's bottom-row cell (reserved by T008), add a horizontal `<StackPanel>` with these segments in order: (1) outlined source chip — `<Border BorderBrush="{DynamicResource BorderSubtle}" BorderThickness="1" Background="Transparent" CornerRadius="4" Padding="8,2"><TextBlock Text="{Binding SourceLabel}" FontSize="11" FontWeight="Medium" Foreground="{DynamicResource TextSecondary}"/></Border>`, (2) `<TextBlock Text=" · " Foreground="{DynamicResource TextTertiary}"/>` separator, (3) `<TextBlock Text="{Binding TestCountLabel}" FontSize="13" Foreground="{DynamicResource TextSecondary}"/>`, (4) separator, (5) `<TextBlock Text="{Binding SchemaVersionLabel}" FontSize="13" Foreground="{DynamicResource TextSecondary}" Visibility="{Binding SchemaVersionLabel, Converter={StaticResource NullToVisibilityConverter}}"/>` plus its preceding separator gated by the same converter, (6) similarly for `{Binding ModifiedLabel}`. Source chip styling must follow `contracts/visual-contract.md` "Row anatomy" exactly (transparent background — NOT a saturated colored pill).
- [x] T017 [US4] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, set `ToolTipService.ToolTip="{Binding Name}"` and `ToolTipService.InitialShowDelay="400"` on the row's outer Border so long names that ellipsize show their full value on hover (FR-010).
- [x] T018 [US4] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, extend the `<ListBox.ItemContainerStyle>` from T007 with a `<Style.Triggers>` block: a `Trigger` on `Property=IsSelected, Value=True` that retargets the inner row Border's style from `Card` to `CardSelected`. Implement this by giving the inner Border `x:Name="RowChrome"` and using `<Setter TargetName="RowChrome" Property="Style" Value="{StaticResource CardSelected}"/>` — OR by introducing a tiny inline style with a `MultiBinding` on `ListBoxItem.IsSelected`; either is acceptable as long as it produces the visual swap defined in `contracts/visual-contract.md` "Visual states → Selected / Active".
- [x] T019 [US4] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, set `AutomationProperties.Name="{Binding AccessibleName}"` on the row's outer Border so screen readers announce the recommended row with its " (recommended)" suffix (FR-019b).

**Checkpoint**: Rows carry full metadata; active profile shows in the selected/active visual state on return; long names tooltip; accessible names reach assistive tech. (FR-009/009a/009b/010/011/012/013/014/019b satisfied; US4 acceptance scenarios pass.)

---

## Phase 7: User Story 5 — The list feels premium under interaction (P3)

**Goal**: Subtle entrance, calm hover, clear focus, and reduced-motion respect — all complete within ≤200 ms (hover/focus/select) and ≤300 ms (entrance stagger).

**Independent Test**: Tab into the list — focus ring is clear. Hover a row — quick, calm color/elevation change with no layout shift. With Windows "animation effects" turned OFF in Accessibility settings, entrance and hover animations are reduced.

### Implementation for User Story 5

- [x] T020 [US5] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, define a `Page.Resources` style `AnimatedProfileRow` that fades from Opacity 0→1 and translates Y from 8→0 over 250 ms with a `CubicEase EaseOut`, and apply it to the inner row Border via the `DataTemplate` (or via a `Setter` in the `ItemContainerStyle` so virtualized rows also animate). Keep the displacement small (≤8 px) per `contracts/visual-contract.md` "Motion contract".
- [x] T021 [US5] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, add a `<Style.Triggers>` block to the row's inner Border style with a `Trigger Property=IsMouseOver Value=True` that swaps Background to `BackgroundElevated`, BorderBrush to `BorderStrong`, and applies a `TranslateTransform Y=-1` — all transitions must complete in ≤200 ms (use `Storyboard.Duration="0:0:0.15"` on the implicit visual transitions). MUST NOT cause layout shift (FR-017).
- [x] T022 [US5] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml.cs`, on the `Loaded` event check `SystemParameters.ClientAreaAnimation`; when `false`, set the page's `Resources["AnimatedProfileRow"]` storyboard durations to `0:0:0` (or remove the style from the items). Document the chosen approach with a single short comment because the WHY (reduced-motion preference) is non-obvious from the code alone.

**Checkpoint**: The list feels premium under interaction; reduced-motion users get a calm static experience. (FR-017/018/019 satisfied; US5 acceptance scenarios pass.)

---

## Phase 8: User Story 6 — Empty, loading, and error states feel coherent (P3)

**Goal**: The welcome banner stops competing with the page header; loading and error states keep their established positions and never visually stack.

**Independent Test**: With the welcome banner visible, the page header is the dominant element (only it has the gradient line). Trigger loading and an error state and confirm they each appear in a single predictable place.

### Implementation for User Story 6

- [x] T023 [US6] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, remove the 4-px gradient accent line at the top of the welcome banner (the `<Border Grid.Row="0" Height="4" Background="{DynamicResource AccentGradientHorizontal}" .../>` inside `<Border x:Name="WelcomeBanner">`). Keep the icon tile, headline, body text, and dismiss button untouched. The page header keeps its gradient line — that line is the page's identity element (FR-004).
- [x] T024 [US6] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, verify that while `IsLoading` is true the centered progress overlay is the only thing visible inside the list grid (the `<ListBox>` from T006 is wrapped or hidden so the list shell does not flash before profiles arrive — bind the `<ListBox>`'s `Visibility` to `IsLoading` via `BoolToVisibilityConverter` with `ConverterParameter=Invert`).
- [x] T025 [US6] In `src/ReqChecker.App/Views/ProfileSelectorView.xaml`, confirm the inline error banner (the `<Border>` bound to `HasError`) sits above the list area with the same vertical position used by other screens' error banners; visually verify in the running app that it does not appear stacked on top of the welcome banner or the page header decoration (FR-023).

**Checkpoint**: Page header alone owns the gradient-line identity; loading, empty, and error states each occupy one calm position. (FR-004/021/022/023 satisfied; US6 acceptance scenarios pass.)

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Verify the full feature builds, tests pass, and the visual contract holds under manual review.

- [x] T026 [P] Run `dotnet build src/ReqChecker.App/ReqChecker.App.csproj -c Debug` from the repo root and resolve any compile errors. Output must be clean (zero warnings introduced by this feature, zero errors).
- [x] T027 [P] Run `dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj --filter "FullyQualifiedName~ProfileSelector|FullyQualifiedName~ProfileListItem"` from the repo root and confirm all 14 mandatory tests (8 from T002 + 6 from T004) pass green. Existing tests in the same file MUST remain green.
- [ ] T028 Walk through the manual checklist in `specs/068-profile-list-redesign/quickstart.md` against a `dotnet run` build of the app. Tick every item or capture violations as follow-up work; do NOT fix scope creep here.
- [ ] T029 Side-by-side visual review: open `Profile Manager` and `Test History` at the same window width and confirm the row pattern reads as the same component family. This validates SC-001. Capture a screenshot (not required to commit) for the PR description.
- [x] T030 Regression-guard scan: search `src/ReqChecker.App/Views/ProfileSelectorView.xaml` for the strings `"Select Profile"` (zero matches expected — FR-006), `WrapPanel` (zero matches — FR-001), and the gradient strip pattern inside the row template (`Height="6"` + `AccentGradientHorizontal` together — zero matches — FR-016). Each match is a regression and MUST be removed before completion.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)** — no dependencies; runs first.
- **Foundational (Phase 2)** — depends on Phase 1; **blocks all user stories**. Within Phase 2: T002∥T004 (parallel test files), then T003 (depends on T002), then T005 (depends on T003 because the page VM constructs `ProfileListItemViewModel` instances and on T004 because its tests target the new behavior).
- **User Stories (Phases 3–8)** — all depend on Phase 2 complete. They share a single XAML file (`ProfileSelectorView.xaml`), so cross-story parallelization within that file is limited; phases are intended to be tackled in priority order **P1 → P2 → P3** but each phase is independently verifiable per its checkpoint.
- **Polish (Phase 9)** — depends on Phases 3–8 being complete; T026 and T027 are [P] (different verbs on different artifacts), T028/T029/T030 run sequentially after them.

### User Story Dependencies

- **US1 (P1)** — depends only on Phase 2.
- **US2 (P1)** — depends on US1 (the row template introduced in T008 is where the click handler lands).
- **US3 (P2)** — depends on US1 (operates inside the same row template). Independent of US2.
- **US4 (P2)** — depends on US1 (row template) and on Phase 2 (`SelectedItem`/`IsActive` plumbing).
- **US5 (P3)** — depends on US1 (the `AnimatedProfileRow` style and item container style are extended).
- **US6 (P3)** — independent of US2/US3/US4/US5; only touches the welcome banner and IsLoading wrapping. Can run as soon as Phase 2 finishes if desired.

### Within Each User Story

- Tests for the foundational phase are written first (T002 before T003, T004 before T005) to honor TDD where it applies.
- The remaining phases are XAML-only and do not have dedicated unit tests — verification is via manual checklist (T028) and the regression-guard scan (T030).
- Stop at each checkpoint and verify against the corresponding US acceptance scenarios in `spec.md` before advancing.

### Parallel Opportunities

- **Within Phase 2**: T002 ∥ T004 (different test files); T003 ∥ {part of T005 that does not reference `ProfileListItemViewModel`} — but in practice T003 finishes first.
- **Within Phase 9**: T026 ∥ T027 (build vs. test).
- **Across user-story phases**: US6 can be implemented in parallel with US3/US4/US5 by a second developer because it touches a different part of the XAML (the welcome banner and IsLoading binding), not the row template.

---

## Parallel Example: Foundational phase (Phase 2)

```bash
# Author both test files in parallel:
Task: "Author tests/ReqChecker.App.Tests/ViewModels/ProfileListItemViewModelTests.cs (8 tests from view-model-contract.md)"
Task: "Extend tests/ReqChecker.App.Tests/ViewModels/ProfileSelectorViewModelTests.cs (6 new tests)"

# Then implement the production code sequentially:
Task: "Implement ProfileListItemViewModel.cs to satisfy the 8 row-VM tests"
Task: "Extend ProfileSelectorViewModel.cs to satisfy the 6 page-VM tests"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 only)

1. Phase 1 → Phase 2 → Phase 3 (US1) → Phase 4 (US2).
2. **STOP and VALIDATE**: Open the running app. The list looks like the rest of the app and selection works on click + keyboard.
3. This is a shippable MVP: visual consistency + correct selection behavior is already a meaningful improvement, even before the recommended-signal cleanup and metadata work.

### Incremental Delivery

1. MVP (above) — ship.
2. Add US3 (single recommended signal) → validate against FR-015/016 → ship.
3. Add US4 (per-row metadata + active state) → validate against FR-009a/010-014/019b → ship.
4. Add US5 (premium polish) → validate against FR-017/018 → ship.
5. Add US6 (edge-state coherence) → validate against FR-004/021/022/023 → ship.

### Parallel Team Strategy

If two developers are available after Phase 2:

- **Developer A**: US1 → US2 → US3 → US4 → US5 (sequential, single file).
- **Developer B**: US6 (welcome banner + IsLoading wrapping — separate region of the XAML; minimal merge friction with Developer A).

Single developer: follow the priority order P1 → P2 → P3 phases as written.

---

## Notes

- [P] tasks operate on different files with no inter-task dependencies.
- [Story] labels (US1–US6) provide traceability back to spec.md user stories.
- The XAML file is the single largest file edited; phases 3–8 each describe targeted edits inside it.
- Verify each test set FAILS before implementing (T002 before T003; T004 before T005).
- Commit after each phase checkpoint — do not bundle all six user stories into a single commit.
- Stop at every checkpoint and validate against the matching user story's acceptance scenarios in `spec.md`.
- Reference `contracts/visual-contract.md` "Decorations explicitly forbidden" before each commit; T030 is the automated regression-guard scan, but a human eye on every PR is the surest defence.
