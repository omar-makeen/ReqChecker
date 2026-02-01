# Tasks: Fix Test Configuration View

**Input**: Design documents from `/specs/012-fix-test-config-view/`
**Prerequisites**: plan.md (required), spec.md (required), research.md

**Tests**: Not required for this bug fix (manual verification only).

**Organization**: Tasks are grouped by user story. US1 and US2 are both P1 priority and are fixed by the same style additions.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Project**: `src/ReqChecker.App/`
- **Target File**: `src/ReqChecker.App/Resources/Styles/Controls.xaml`

---

## Phase 1: Implementation (Add Missing Styles)

**Purpose**: Add the two missing style definitions that cause the StaticResource lookup failure

**⚠️ NOTE**: This is adding new styles - no setup or foundational phase needed.

### User Story 1 & 2 - View Test Configuration Without Crash + Premium Styling (Priority: P1) 🎯 MVP

**Goal**: Add missing `ParameterGroupCard` and `PromptAtRunIndicator` styles so the Test Configuration view loads and displays with premium aesthetic

**Independent Test**: Click any test item in Tests view → Test Configuration view loads without errors → sections display in styled cards

### Implementation

- [x] T001 [P] [US1] Add `ParameterGroupCard` style to `src/ReqChecker.App/Resources/Styles/Controls.xaml` after CardSelected style (around line 313)
- [x] T002 [P] [US1] Add `PromptAtRunIndicator` style to `src/ReqChecker.App/Resources/Styles/Controls.xaml` after ParameterGroupCard style

**Style Specifications** (from plan.md):

**T001 - ParameterGroupCard**:
```xml
<!-- Parameter Group Card - For grouped form sections -->
<Style x:Key="ParameterGroupCard" TargetType="Border">
    <Setter Property="Background" Value="{DynamicResource BackgroundSurface}"/>
    <Setter Property="BorderBrush" Value="{DynamicResource BorderDefault}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="CornerRadius" Value="12"/>
    <Setter Property="Padding" Value="20"/>
    <Setter Property="Margin" Value="0,0,0,16"/>
    <Setter Property="Effect">
        <Setter.Value>
            <DropShadowEffect Color="{DynamicResource ElevationGlowColor}"
                              BlurRadius="10"
                              ShadowDepth="0"
                              Opacity="1"
                              Direction="0"/>
        </Setter.Value>
    </Setter>
</Style>
```

**T002 - PromptAtRunIndicator**:
```xml
<!-- Prompt At Run Indicator - For parameters that will be prompted during execution -->
<Style x:Key="PromptAtRunIndicator" TargetType="Border">
    <Setter Property="Background" Value="{DynamicResource AccentSecondary}"/>
    <Setter Property="CornerRadius" Value="6"/>
    <Setter Property="Padding" Value="12,8"/>
    <Setter Property="Effect">
        <Setter.Value>
            <DropShadowEffect Color="{DynamicResource AccentSecondaryColor}"
                              BlurRadius="8"
                              ShadowDepth="0"
                              Opacity="0.3"
                              Direction="0"/>
        </Setter.Value>
    </Setter>
</Style>
```

**Checkpoint**: Both styles defined in Controls.xaml

---

## Phase 2: Verification

**Purpose**: Verify the fix works correctly

- [x] T003 Build application with `dotnet build src/ReqChecker.App`
- [x] T004 Run application and navigate to Tests view
- [x] T005 Click on any test item to open Test Configuration view
- [x] T006 Verify no error dialogs appear
- [x] T007 Verify Basic Information section displays in styled card
- [x] T008 Verify Execution Settings section displays in styled card
- [x] T009 Verify Test Parameters section displays in styled card
- [x] T010 Verify "Prompt at run" parameters show indicator badge (if any exist)
- [x] T011 Verify visual styling matches premium aesthetic (dark surfaces, rounded corners, glow effects)

**Checkpoint**: All acceptance criteria from spec.md verified

---

## Phase 3: User Story 3 - Edit Test Settings (Priority: P2)

**Goal**: Verify editing functionality works (no implementation needed - depends on view loading)

**Independent Test**: Change timeout/retry values and verify they can be saved

- [x] T012 [US3] Verify timeout input field accepts numeric values
- [x] T013 [US3] Verify retry count input field accepts numeric values
- [x] T014 [US3] Verify Save button is functional

**Checkpoint**: Editing functionality verified

---

## Phase 4: Finalize

**Purpose**: Commit and complete

- [ ] T015 Commit changes with descriptive message
- [ ] T016 Push to feature branch

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Implementation)**: No dependencies - start immediately
- **Phase 2 (Verification)**: Depends on Phase 1 completion
- **Phase 3 (US3 Verification)**: Depends on Phase 2 (view must load)
- **Phase 4 (Finalize)**: Depends on all verification passing

### Task Dependencies

```
T001, T002 (parallel - different style definitions)
    ↓
T003 (build after code changes)
    ↓
T004-T011 (verification - sequential testing session)
    ↓
T012-T014 (US3 verification)
    ↓
T015 → T016 (commit then push)
```

### Parallel Opportunities

- T001 and T002 can be done in parallel (adding to same file but independent styles)
- T004-T011 (verification steps) performed in a single testing session
- T012-T014 (US3 verification) performed in same testing session

---

## Implementation Strategy

### Single-Pass Fix (Recommended)

1. Complete T001-T002 (add both styles)
2. Complete T003 (build)
3. Complete T004-T011 (verification)
4. Complete T012-T014 (US3 verification)
5. Complete T015-T016 (commit and push)

Total: ~10-15 minutes for complete fix

---

## User Story Mapping

| Story | Priority | Tasks | Description |
|-------|----------|-------|-------------|
| US1 | P1 | T001, T002 | View without crash (ParameterGroupCard style) |
| US2 | P1 | T001, T002 | Premium styling (both styles) |
| US3 | P2 | T012-T014 | Edit settings (verification only - no code changes) |

---

## Notes

- Both US1 and US2 are fixed by the same style additions (T001, T002)
- US3 requires no code changes - just verification that existing edit functionality works
- This is a style-only fix: 2 new styles added to 1 file
- No automated tests required - manual verification sufficient for UI styling fix
- Commit after verification to ensure fix is complete
