# Quickstart: Fix Test Configuration Page UX

**Feature**: 037-fix-test-config-ux
**Branch**: `037-fix-test-config-ux`

## Prerequisites

- .NET 8.0 SDK installed
- Windows (WPF target)

## Build & Verify

```bash
# Build the app
dotnet build src/ReqChecker.App/ReqChecker.App.csproj

# Run tests (no regressions)
dotnet test tests/ReqChecker.App.Tests/ReqChecker.App.Tests.csproj
```

## Files Modified

| File | Change |
|------|--------|
| `src/ReqChecker.App/Views/TestConfigView.xaml` | Remove footer bar, move Save to header, remove MaxWidth="800", fix margin 24→32 |
| `src/ReqChecker.App/ViewModels/TestConfigViewModel.cs` | Remove CancelCommand property, constructor init, and OnCancel() method |
| `src/ReqChecker.App/Views/ProfileSelectorView.xaml` | Restyle error bar with theme tokens, glow effect, larger corner radius |

## Manual Verification

1. Launch the app and navigate to a test configuration page
2. Verify: No footer bar visible, "Save Changes" button is in the header (right-aligned)
3. Verify: Content fills available width (no narrow centered column)
4. Verify: Left/right margins match other pages (e.g., test list page)
5. Verify: Back button navigates back to test list
6. Verify: Save Changes button saves correctly
7. Trigger a profile validation error and verify the error bar has premium styling (glow, rounded corners, theme colors)
