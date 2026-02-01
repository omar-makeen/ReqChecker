# Research: Improve Page Titles and Icons

**Feature**: 015-improve-page-titles
**Date**: 2026-02-01

## Icon Selection Rationale

### Navigation Icons

| Page | Current Icon | New Icon | Rationale |
|------|--------------|----------|-----------|
| Profiles | FolderOpen24 | Folder24 | Closed folder is cleaner, more professional; "open" state implies action in progress |
| Tests | Beaker24 | ClipboardTaskList24 | Clipboard with checklist better represents organized test management than lab beaker |
| Results | DataBarVertical24 | Poll24 | Poll icon conveys survey/results better; more distinctive than generic bar chart |
| Diagnostics | Bug24 | HeartPulse24 | Heart pulse represents system health monitoring; bug implies problems rather than diagnostics |

### Page Header Icons

All page headers will use the same icon as their corresponding navigation item for consistency. This creates visual continuity as users navigate.

### Window Branding

| Element | Current | New | Rationale |
|---------|---------|-----|-----------|
| Title Bar Icon | CheckmarkCircle24 | ShieldCheckmark24 | Shield represents verification/security; checkmark alone is too generic |

### Secondary Page Icons

| Page | Current | New | Rationale |
|------|---------|-----|-----------|
| Test Config | Settings24 | SettingsCog24 | Cog variant is more distinctive, implies configuration mechanics |
| Run Progress | Play24 | PlayCircle24 | Circle variant is more polished, matches modern media player conventions |

## Title Selection Rationale

### Navigation Labels

| Page | Current | New | Rationale |
|------|---------|-----|-----------|
| Profiles | "Profiles" | "Profile Manager" | Action-oriented; implies capability to manage, not just view |
| Tests | "Tests" | "Test Suite" | Professional terminology; implies organized collection |
| Results | "Results" | "Results Dashboard" | Dashboard implies data visualization and overview |
| Diagnostics | "Diagnostics" | "System Diagnostics" | Clarifies scope; sounds more technical/professional |

### Tooltip Updates

| Page | Current | New |
|------|---------|-----|
| Profiles | "Select and manage profiles" | "Manage test profiles" |
| Tests | "Configure and run tests" | "Configure and run tests" (unchanged) |
| Results | "View test results" | "View test results" (unchanged) |
| Diagnostics | "View diagnostic information" | "View system diagnostics" |

## Alternatives Considered

### Icon Alternatives

| Proposed | Alternatives Rejected | Why Rejected |
|----------|----------------------|--------------|
| Folder24 | FolderOpen24 | Open state implies ongoing action |
| ClipboardTaskList24 | TaskListLtr24, Beaker24 | TaskList lacks clipboard context; Beaker is too scientific |
| Poll24 | ChartMultiple24, DataBarVertical24 | Chart is too generic; DataBar lacks the "results" connotation |
| HeartPulse24 | Stethoscope24, Bug24 | Stethoscope is medical; Bug implies problems |
| ShieldCheckmark24 | Shield24, CheckmarkCircle24 | Plain shield lacks verification; checkmark alone is generic |

### Title Alternatives

| Chosen | Alternatives Rejected | Why Rejected |
|--------|----------------------|--------------|
| "Profile Manager" | "Profiles", "Profile Selector" | Generic; Selector implies only selection, not management |
| "Test Suite" | "Tests", "Test Manager" | Generic; Manager conflicts with Profile Manager |
| "Results Dashboard" | "Test Results", "Results View" | Results alone is vague; View is too technical |
| "System Diagnostics" | "Diagnostics", "System Info" | Generic; Info implies static data, not diagnostics |

## WPF-UI Icon Verification

The following icons need verification against WPF-UI SymbolRegular enum:

```csharp
// Icons to verify exist in Wpf.Ui.Controls.SymbolRegular
SymbolRegular.Folder24
SymbolRegular.ClipboardTaskList24
SymbolRegular.Poll24
SymbolRegular.HeartPulse24
SymbolRegular.SettingsCog24
SymbolRegular.PlayCircle24
SymbolRegular.ShieldCheckmark24
```

If any icon is unavailable, use fallback from plan.md Icon Fallback Strategy table.

## References

- `MainWindow.xaml` - Current navigation structure (lines 65-116)
- `*View.xaml` - Current page header implementations
- WPF-UI SymbolRegular documentation
