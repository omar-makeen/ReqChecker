# Quickstart: 063-ux-quick-wins

## Prerequisites
- .NET 8.0 SDK
- Windows 10/11 (WPF app)

## Build & Run
```bash
dotnet build src/ReqChecker.App/
dotnet run --project src/ReqChecker.App/
```

## Test
```bash
dotnet test tests/ReqChecker.App.Tests/
```

## Files to Modify

### P1: Profile Name in RunProgress Header
1. `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` — Add `ProfileName` computed property
2. `src/ReqChecker.App/Views/RunProgressView.xaml` — Add TextBlock in header StackPanel

### P2: Test Count Badge on Sidebar
1. `src/ReqChecker.App/ViewModels/MainViewModel.cs` — Add `TestCount` / `HasTests` properties, subscribe to `CurrentProfileChanged`
2. `src/ReqChecker.App/Views/MainWindow.xaml` — Add badge overlay on NavTests item

### P3: Export Keyboard Shortcut
1. `src/ReqChecker.App/Views/ResultsView.xaml` — Add `InputBinding` for Ctrl+E → `ToggleExportMenuCommand`

### P4: Filter Tab Animation
1. `src/ReqChecker.App/Views/ResultsView.xaml` — Name the results ListBox container, add fade Storyboard resources
2. `src/ReqChecker.App/Views/ResultsView.xaml.cs` — Add fade-out/fade-in helper triggered on filter change

### P5: Tooltip Audit
1. Audit all `.xaml` files in `Views/` and `Controls/` for interactive elements without tooltips
2. Add missing tooltips following existing `ModernToolTip` / 400ms delay pattern

## Key Patterns
- Observable properties: `[ObservableProperty]` attribute (CommunityToolkit.Mvvm source generators)
- Animations: Storyboard with `QuadraticEase EaseOut` (200-300ms), `EaseIn` for exits (150ms)
- Tooltips: `ToolTipService.InitialShowDelay="400"`, `ShowOnDisabled="True"`, `ModernToolTip` style
- Navigation badge: Custom overlay Border inside NavigationViewItem (no native InfoBadge in WPF-UI 4.2.0)
