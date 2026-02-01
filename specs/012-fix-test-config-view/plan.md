# Implementation Plan: Fix Test Configuration View

**Branch**: `012-fix-test-config-view` | **Date**: 2026-02-01 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/012-fix-test-config-view/spec.md`

## Summary

Fix the Test Configuration view crash caused by missing style definitions. The view references two undefined styles (`ParameterGroupCard` and `PromptAtRunIndicator`) which causes a `StaticResourceExtension` exception at runtime. The fix adds these styles to Controls.xaml following the existing premium design patterns.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only fix)
**Testing**: Manual verification (WPF UI testing)
**Target Platform**: Windows (win-x64)
**Project Type**: WPF Desktop Application
**Performance Goals**: N/A (bug fix)
**Constraints**: Must follow existing design system patterns
**Scale/Scope**: Single resource file change (add 2 styles)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Constitution not configured | PASS | Project uses placeholder constitution; no gates enforced |

## Project Structure

### Documentation (this feature)

```text
specs/012-fix-test-config-view/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output (style pattern analysis)
└── checklists/
    └── requirements.md  # Spec validation checklist
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Resources/
│   │   └── Styles/
│   │       └── Controls.xaml     # FILE TO MODIFY (add missing styles)
│   └── Views/
│       └── TestConfigView.xaml   # View that uses the styles (no changes needed)
```

**Structure Decision**: Existing WPF structure. Single file modification in `src/ReqChecker.App/Resources/Styles/Controls.xaml`.

## Complexity Tracking

No violations. This is adding two missing style definitions.

---

## Phase 0: Research

### Style Pattern Analysis

**Existing Card Styles in Controls.xaml**:
- `Card` - Base card with surface background, border, rounded corners (8px), padding (16px), elevation glow
- `CardElevated` - Elevated background with stronger glow effect
- `CardInteractive` - Hover effects with cursor, translation, stronger glow
- `CardSelected` - Accent border with accent glow

**Design System Tokens Used**:
- Backgrounds: `BackgroundSurface`, `BackgroundElevated`
- Borders: `BorderDefault`, `BorderSubtle`, `BorderStrong`
- Accents: `AccentPrimary`, `AccentSecondary`, `StatusSkip`
- Glow Effects: `ElevationGlowColor`, `ElevationGlowHoverColor`

### Missing Style Specifications

#### 1. ParameterGroupCard

**Purpose**: Container for grouped parameter sections (Basic Info, Execution Settings, Test Parameters)

**Design Decision**: Base on existing `Card` style with larger corner radius (12px) and more padding (20px) for section grouping

**Pattern**: Similar to `Card` but optimized for form sections

#### 2. PromptAtRunIndicator

**Purpose**: Badge/indicator showing that a parameter will be prompted during test execution

**Design Decision**: Eye-catching badge with accent secondary color, similar styling to status badges

**Pattern**: Similar to `StatusBadgeBase` with distinctive "prompt" styling (accent secondary background)

---

## Phase 1: Implementation Design

### Style Specifications

**File**: `src/ReqChecker.App/Resources/Styles/Controls.xaml`

### 1. ParameterGroupCard Style

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

### 2. PromptAtRunIndicator Style

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

### Insertion Location

Add after the existing `CardSelected` style (around line 313) in the CARD STYLES section.

### Verification Steps

1. Build: `dotnet build src/ReqChecker.App`
2. Run application
3. Navigate to Tests view
4. Click on any test item
5. Verify:
   - Test Configuration view loads without error dialogs
   - Basic Information section displays in styled card
   - Execution Settings section displays in styled card
   - Test Parameters section displays in styled card
   - "Prompt at run" parameters show indicator badge (if any exist)
   - Visual styling matches premium aesthetic

### Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Style doesn't match design | Low | Medium | Follow existing Card patterns exactly |
| Missing resource reference | Very Low | High | Only use existing DynamicResource keys |
| Breaking other views | Very Low | Low | New styles, not modifying existing |

---

## Artifacts Generated

- [x] `plan.md` - This implementation plan
- [x] `research.md` - Consolidated in this document (minimal research needed)
- [ ] `data-model.md` - Not applicable (no data model changes)
- [ ] `contracts/` - Not applicable (no API changes)
- [ ] `quickstart.md` - Not applicable (simple bug fix)

---

## Next Steps

1. Run `/speckit.tasks` to generate implementation tasks
2. Add styles to `Controls.xaml`
3. Build and verify fix
4. Commit and push changes
