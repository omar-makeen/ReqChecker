# Quickstart: Premium Test Execution Page

**Branch**: `020-premium-test-execution` | **Date**: 2026-02-02

## Prerequisites

- Visual Studio 2022 or VS Code with C# extension
- .NET 8.0 SDK
- Windows 10/11 (WPF target platform)

## Build & Run

```bash
# Clone and checkout
git checkout 020-premium-test-execution

# Build
dotnet build src/ReqChecker.App

# Run
dotnet run --project src/ReqChecker.App
```

## Testing the Feature

1. **Launch application** and load a test profile
2. **Navigate to Test Suite** page - observe premium header styling
3. **Click "Run All Tests"** to navigate to Test Execution page
4. **Verify**:
   - Header has gradient accent line at top
   - Header icon is in rounded container with accent background
   - Title "Test Execution" with subtitle showing run progress
   - Header animates in on page load
5. **Watch tests complete** and verify:
   - Each test result card appears with slide-in animation
   - Status badges show Pass (green), Fail (red), or Skip (amber)
   - Cards have colored left border matching status
6. **Compare** Test Execution header to other pages (Test Suite, Results Dashboard) - should match styling

## Key Files

| File | Purpose |
|------|---------|
| `src/ReqChecker.App/Views/RunProgressView.xaml` | Main view - header and result list |
| `src/ReqChecker.App/ViewModels/RunProgressViewModel.cs` | ViewModel with HeaderSubtitle property |
| `src/ReqChecker.App/Resources/Styles/Controls.xaml` | AnimatedPageHeader style reference |
| `src/ReqChecker.App/Controls/TestStatusBadge.xaml` | Status badge control |

## Design Reference

Copy header structure from `TestListView.xaml` lines 64-120:
- AnimatedPageHeader style
- Gradient accent line
- Icon container with accent background
- Title + subtitle layout

## Success Criteria Checklist

- [ ] Header matches other pages' premium styling
- [ ] Gradient accent line visible (4px, horizontal gradient)
- [ ] Icon in 48x48 rounded container with accent background
- [ ] Subtitle updates dynamically ("Running X of Y tests" → "Y tests completed")
- [ ] Header entrance animation plays on page load
- [ ] Test result cards have visible status badges
- [ ] Test result cards have colored left border (green/red/amber)
- [ ] Result cards animate in when tests complete
