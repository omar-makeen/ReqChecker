# Implementation Plan: Improve Test List UI/UX

**Branch**: `014-improve-test-list` | **Date**: 2026-02-01 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/014-improve-test-list/spec.md`

## Summary

Enhance the test list view with type-specific icons and test descriptions to improve visual differentiation and information density. Users will be able to identify test types at a glance and understand test purposes without clicking into details.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows
**Primary Dependencies**: WPF-UI 4.2.0 (Fluent icons), CommunityToolkit.Mvvm 8.4.0
**Storage**: N/A (UI-only enhancement, uses existing TestDefinition data)
**Testing**: Manual verification (WPF UI enhancement)
**Target Platform**: Windows (win-x64)
**Project Type**: WPF Desktop Application
**Performance Goals**: N/A (UI enhancement)
**Constraints**: 44x44 icon container, 2-line max description truncation
**Scale/Scope**: Single view modification (TestListView.xaml)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Constitution not configured | PASS | Project uses placeholder constitution; no gates enforced |

## Project Structure

### Documentation (this feature)

```text
specs/014-improve-test-list/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Icon mapping and color decisions
└── checklists/
    └── requirements.md  # Spec validation checklist
```

### Source Code (repository root)

```text
src/
├── ReqChecker.App/
│   ├── Converters/
│   │   ├── TestTypeToIconConverter.cs     # NEW: Maps Type → SymbolRegular icon
│   │   └── TestTypeToColorConverter.cs    # NEW: Maps Type → category color
│   ├── Views/
│   │   └── TestListView.xaml              # MODIFY: Add icon mapping, description display
│   └── Resources/
│       └── Styles/
│           ├── Colors.Dark.xaml           # MODIFY: Add category colors (if needed)
│           └── Colors.Light.xaml          # MODIFY: Add category colors (if needed)
```

**Structure Decision**: Existing WPF structure. Two new converters plus view modifications.

## Complexity Tracking

No violations. This is a UI enhancement adding converters and view updates.

---

## Phase 0: Research

### Icon Mapping Design

Based on WPF-UI Fluent icon analysis, the following icons are suitable for each test type:

| Test Type | Icon Symbol | Category | Rationale |
|-----------|-------------|----------|-----------|
| Ping | `Signal24` or `Wifi124` | Network | Signal/connectivity representation |
| HttpGet | `Globe24` | Network | Web/HTTP request representation |
| DnsLookup | `Search24` or `Globe24` | Network | DNS lookup/search representation |
| FileExists | `Document24` | FileSystem | Document/file representation |
| DirectoryExists | `Folder24` | FileSystem | Folder representation |
| ProcessList | `Apps24` or `WindowApps24` | System | Running processes representation |
| RegistryRead | `Settings24` or `SettingsCogsMultiple24` | System | Registry/settings representation |
| (default) | `Beaker24` | Default | Current fallback icon |

### Category Color Design

Using existing design system color tokens for consistency:

| Category | Color Token | Hex Value | Usage |
|----------|-------------|-----------|-------|
| Network | `StatusInfo` / `AccentPrimary` | #3b82f6 / #00d9ff | Network tests (Ping, HttpGet, DnsLookup) |
| FileSystem | `StatusSkip` | #f59e0b | File system tests (FileExists, DirectoryExists) |
| System | `AccentSecondary` | #6366f1 | System tests (ProcessList, RegistryRead) |
| Default | `TextTertiary` | #8585a0 | Unknown test types |

### Description Display Design

- **Position**: Below the test name, before the Type badge
- **Style**: `TextCaption` style with `TextTertiary` color
- **Truncation**: `TextTrimming="CharacterEllipsis"` with `MaxHeight` for 2 lines
- **Visibility**: Collapsed when Description is null or empty

### WPF-UI SymbolRegular Enum

The WPF-UI library uses `SymbolRegular` enum for icons. Based on analysis of existing XAML:
- Icons are referenced as `Symbol="IconName24"` (e.g., `Symbol="Globe24"`)
- Available in `Wpf.Ui.Controls` namespace

---

## Phase 1: Implementation Design

### TestTypeToIconConverter.cs

```csharp
// Maps TestDefinition.Type string to SymbolRegular icon
public class TestTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string testType)
        {
            return testType switch
            {
                "Ping" => SymbolRegular.Signal24,
                "HttpGet" => SymbolRegular.Globe24,
                "DnsLookup" => SymbolRegular.Search24,
                "FileExists" => SymbolRegular.Document24,
                "DirectoryExists" => SymbolRegular.Folder24,
                "ProcessList" => SymbolRegular.Apps24,
                "RegistryRead" => SymbolRegular.Settings24,
                _ => SymbolRegular.Beaker24  // Default fallback
            };
        }
        return SymbolRegular.Beaker24;
    }
}
```

### TestTypeToColorConverter.cs

```csharp
// Maps TestDefinition.Type string to category color brush
public class TestTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string testType)
        {
            return testType switch
            {
                "Ping" or "HttpGet" or "DnsLookup" =>
                    Application.Current.FindResource("StatusInfo") as SolidColorBrush,
                "FileExists" or "DirectoryExists" =>
                    Application.Current.FindResource("StatusSkip") as SolidColorBrush,
                "ProcessList" or "RegistryRead" =>
                    Application.Current.FindResource("AccentSecondary") as SolidColorBrush,
                _ => Application.Current.FindResource("TextTertiary") as SolidColorBrush
            };
        }
        return Application.Current.FindResource("TextTertiary") as SolidColorBrush;
    }
}
```

### TestListView.xaml Changes

1. **Add converter resources**:
```xml
<Page.Resources>
    <converters:TestTypeToIconConverter x:Key="TestTypeToIconConverter"/>
    <converters:TestTypeToColorConverter x:Key="TestTypeToColorConverter"/>
    <!-- existing resources... -->
</Page.Resources>
```

2. **Update icon binding**:
```xml
<ui:SymbolIcon Symbol="{Binding Type, Converter={StaticResource TestTypeToIconConverter}}"
               Foreground="{Binding Type, Converter={StaticResource TestTypeToColorConverter}}"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"/>
```

3. **Add description display** (after DisplayName, before Type):
```xml
<TextBlock Text="{Binding Description}"
           Style="{DynamicResource TextCaption}"
           Foreground="{DynamicResource TextSecondary}"
           TextTrimming="CharacterEllipsis"
           MaxHeight="32"
           Visibility="{Binding Description, Converter={StaticResource NullToVisibilityConverter}}"/>
```

### Verification Steps

1. Build: `dotnet build src/ReqChecker.App`
2. Run application and navigate to Tests view
3. Verify:
   - Each test type shows its designated icon
   - Icons have category-appropriate colors
   - Descriptions display below test names
   - Long descriptions are truncated with ellipsis
   - Tests without descriptions show no empty space
   - Dark/light theme switching works correctly
   - Existing functionality (click, hover, keyboard) still works

### Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Icon not available in WPF-UI | Low | Medium | Fallback to similar icon or Beaker24 |
| Color token not found | Very Low | Low | Fallback to TextTertiary |
| Layout overflow with descriptions | Low | Medium | MaxHeight constraint, TextTrimming |
| Theme switching breaks colors | Low | Medium | Use DynamicResource for all colors |

---

## Artifacts Generated

- [x] `plan.md` - This implementation plan
- [x] `research.md` - Consolidated in this document (icon/color mapping)
- [ ] `data-model.md` - Not applicable (no data model changes)
- [ ] `contracts/` - Not applicable (no API changes)
- [ ] `quickstart.md` - Not applicable (UI enhancement with manual verification)

---

## Next Steps

1. Run `/speckit.tasks` to generate implementation tasks
2. Create `TestTypeToIconConverter.cs`
3. Create `TestTypeToColorConverter.cs`
4. Modify `TestListView.xaml` to use converters and add description
5. Build and verify
6. Commit and push changes
