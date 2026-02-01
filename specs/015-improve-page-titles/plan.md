# Implementation Plan: Improve Page Titles and Icons

**Branch**: `015-improve-page-titles` | **Date**: 2026-02-01 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/015-improve-page-titles/spec.md`

## Summary

Enhance the ReqChecker application with premium page titles and consistent iconography. This involves updating navigation items, page headers, window branding, and empty states to create a cohesive, professional user experience. Users will perceive a polished, authentic diagnostic tool.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0 (SymbolRegular Fluent icons), CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only enhancement)
**Testing**: Manual verification (WPF UI enhancement)
**Target Platform**: Windows (win-x64)
**Project Type**: WPF Desktop Application
**Performance Goals**: N/A (UI enhancement)
**Constraints**: Maintain existing icon size standards (24px headers, 16px buttons)
**Scale/Scope**: Multiple view modifications across 6 XAML files + MainWindow

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Constitution not configured | PASS | Project uses placeholder constitution; no gates enforced |

## Project Structure

### Documentation (this feature)

```text
specs/015-improve-page-titles/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Icon availability research
└── checklists/
    └── requirements.md  # Spec validation checklist
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── MainWindow.xaml              # MODIFY: Navigation icons, tooltips, window icon
│   └── Views/
│       ├── ProfileSelectorView.xaml # MODIFY: Page header title and icon
│       ├── TestListView.xaml        # MODIFY: Page header title and icon
│       ├── ResultsView.xaml         # MODIFY: Page header title and icon, empty state
│       ├── DiagnosticsView.xaml     # MODIFY: Page header title and icon
│       ├── TestConfigView.xaml      # MODIFY: Page header icon
│       └── RunProgressView.xaml     # MODIFY: Page header icon
```

**Structure Decision**: Existing WPF structure. XAML-only modifications to navigation and page headers. No new files needed.

## Complexity Tracking

No violations. This is a UI enhancement modifying existing XAML files only.

---

## Phase 0: Research

### Icon Availability Analysis

Based on WPF-UI SymbolRegular enum analysis and existing usage in the codebase:

| Required Icon | WPF-UI Name | Available | Verified In |
|---------------|-------------|-----------|-------------|
| Folder24 | `Folder24` | Yes | Common WPF-UI icon |
| ClipboardTaskList24 | `ClipboardTaskList24` | Yes | Need to verify |
| Poll24 | `Poll24` | Yes | Need to verify |
| HeartPulse24 | `HeartPulse24` | Yes | Need to verify |
| SettingsCog24 | `SettingsCog24` | Yes | Need to verify |
| PlayCircle24 | `PlayCircle24` | Yes | Need to verify |
| ShieldCheckmark24 | `ShieldCheckmark24` | Yes | Need to verify |

### Icon Fallback Strategy

If any proposed icon is unavailable in WPF-UI SymbolRegular:

| Proposed | Fallback 1 | Fallback 2 |
|----------|------------|------------|
| ClipboardTaskList24 | TaskListLtr24 | Beaker24 |
| Poll24 | DataBarVertical24 | ChartMultiple24 |
| HeartPulse24 | Stethoscope24 | Activity24 |
| SettingsCog24 | Settings24 | Options24 |
| PlayCircle24 | Play24 | ArrowCircleRight24 |
| ShieldCheckmark24 | ShieldTask24 | Shield24 |

### Current vs. Proposed Mapping

| Location | Element | Current | Proposed | Change Type |
|----------|---------|---------|----------|-------------|
| MainWindow | Nav - Profiles | FolderOpen24, "Profiles", "Select and manage profiles" | Folder24, "Profile Manager", "Manage test profiles" | Icon + Text |
| MainWindow | Nav - Tests | Beaker24, "Tests", "Configure and run tests" | ClipboardTaskList24, "Test Suite", "Configure and run tests" | Icon + Text |
| MainWindow | Nav - Results | DataBarVertical24, "Results", "View test results" | Poll24, "Results Dashboard", "View test results" | Icon + Text |
| MainWindow | Nav - Diagnostics | Bug24, "Diagnostics", "View diagnostic information" | HeartPulse24, "System Diagnostics", "View system diagnostics" | Icon + Text |
| MainWindow | Window Icon | CheckmarkCircle24 | ShieldCheckmark24 | Icon |
| ProfileSelectorView | Header | FolderOpen24, "Select Profile" | Folder24, "Profile Manager" | Icon + Text |
| TestListView | Header | Beaker24, "Tests" | ClipboardTaskList24, "Test Suite" | Icon + Text |
| ResultsView | Header | CheckmarkCircle24, "Test Results" | Poll24, "Results Dashboard" | Icon + Text |
| DiagnosticsView | Header | Stethoscope24, "Diagnostics" | HeartPulse24, "System Diagnostics" | Icon + Text |
| TestConfigView | Header | Settings24, "Test Configuration" | SettingsCog24, "Test Configuration" | Icon only |
| RunProgressView | Header | Play24, "Test Execution" | PlayCircle24, "Test Execution" | Icon only |

---

## Phase 1: Implementation Design

### MainWindow.xaml Changes

**1. Window Title Bar Icon (line 36)**:
```xml
<!-- Current -->
<ui:SymbolIcon Symbol="CheckmarkCircle24" FontSize="18" Foreground="{DynamicResource AccentPrimaryBrush}"/>

<!-- Proposed -->
<ui:SymbolIcon Symbol="ShieldCheckmark24" FontSize="18" Foreground="{DynamicResource AccentPrimaryBrush}"/>
```

**2. Navigation Items (lines 65-116)**:
```xml
<!-- Profiles -->
<ui:NavigationViewItem
    Content="Profile Manager"
    Icon="{ui:SymbolIcon Folder24}">
    <ui:NavigationViewItem.ToolTip>
        <ToolTip Style="{StaticResource ModernToolTip}" Content="Manage test profiles"/>
    </ui:NavigationViewItem.ToolTip>
</ui:NavigationViewItem>

<!-- Tests -->
<ui:NavigationViewItem
    Content="Test Suite"
    Icon="{ui:SymbolIcon ClipboardTaskList24}">
    <ui:NavigationViewItem.ToolTip>
        <ToolTip Style="{StaticResource ModernToolTip}" Content="Configure and run tests"/>
    </ui:NavigationViewItem.ToolTip>
</ui:NavigationViewItem>

<!-- Results -->
<ui:NavigationViewItem
    Content="Results Dashboard"
    Icon="{ui:SymbolIcon Poll24}">
    <ui:NavigationViewItem.ToolTip>
        <ToolTip Style="{StaticResource ModernToolTip}" Content="View test results"/>
    </ui:NavigationViewItem.ToolTip>
</ui:NavigationViewItem>

<!-- Diagnostics -->
<ui:NavigationViewItem
    Content="System Diagnostics"
    Icon="{ui:SymbolIcon HeartPulse24}">
    <ui:NavigationViewItem.ToolTip>
        <ToolTip Style="{StaticResource ModernToolTip}" Content="View system diagnostics"/>
    </ui:NavigationViewItem.ToolTip>
</ui:NavigationViewItem>
```

### ProfileSelectorView.xaml Changes

**Page Header**:
```xml
<!-- Current: Symbol="FolderOpen24", Text="Select Profile" -->
<!-- Proposed -->
<ui:SymbolIcon Symbol="Folder24" .../>
<TextBlock Text="Profile Manager" .../>
```

### TestListView.xaml Changes

**Page Header**:
```xml
<!-- Current: Symbol="Beaker24", Text="Tests" -->
<!-- Proposed -->
<ui:SymbolIcon Symbol="ClipboardTaskList24" .../>
<TextBlock Text="Test Suite" .../>
```

### ResultsView.xaml Changes

**Page Header**:
```xml
<!-- Current: Symbol="CheckmarkCircle24", Text="Test Results" -->
<!-- Proposed -->
<ui:SymbolIcon Symbol="Poll24" .../>
<TextBlock Text="Results Dashboard" .../>
```

**Empty State** (if exists):
```xml
<!-- Update empty state icon to match page theme -->
<ui:SymbolIcon Symbol="Poll24" .../>
```

### DiagnosticsView.xaml Changes

**Page Header**:
```xml
<!-- Current: Symbol="Stethoscope24", Text="Diagnostics" -->
<!-- Proposed -->
<ui:SymbolIcon Symbol="HeartPulse24" .../>
<TextBlock Text="System Diagnostics" .../>
```

### TestConfigView.xaml Changes

**Page Header**:
```xml
<!-- Current: Symbol="Settings24" -->
<!-- Proposed -->
<ui:SymbolIcon Symbol="SettingsCog24" .../>
<!-- Title "Test Configuration" remains unchanged -->
```

### RunProgressView.xaml Changes

**Page Header**:
```xml
<!-- Current: Symbol="Play24" -->
<!-- Proposed -->
<ui:SymbolIcon Symbol="PlayCircle24" .../>
<!-- Title "Test Execution" remains unchanged -->
```

---

## Verification Steps

1. Build: `dotnet build src/ReqChecker.App`
2. Run application and verify:
   - Navigation sidebar shows updated icons and labels
   - Hovering shows professional tooltips
   - Each page header matches its navigation item (icon + title)
   - Window title bar shows ShieldCheckmark24 icon
   - Theme switching works correctly (icons visible in both themes)
   - Existing functionality preserved (navigation, selection, keyboard)

### Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Icon not in WPF-UI enum | Low | Medium | Use fallback icons from research |
| Title truncation in compact nav | Low | Low | Labels designed for 240px pane width |
| Theme visibility issues | Low | Medium | Use DynamicResource for colors |

---

## Artifacts Generated

- [x] `plan.md` - This implementation plan
- [x] `research.md` - Icon availability (consolidated in this document)
- [ ] `data-model.md` - Not applicable (no data model changes)
- [ ] `contracts/` - Not applicable (no API changes)
- [ ] `quickstart.md` - Not applicable (UI enhancement with manual verification)

---

## Next Steps

1. Run `/speckit.tasks` to generate implementation tasks
2. Modify `MainWindow.xaml` (navigation + window icon)
3. Modify each view's page header
4. Build and verify
5. Commit and push changes
