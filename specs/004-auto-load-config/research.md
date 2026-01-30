# Research: Auto-Load Bundled Configuration

**Feature**: 004-auto-load-config
**Date**: 2026-01-30

## Research Questions

### 1. How to determine application executable directory reliably?

**Decision**: Use `AppContext.BaseDirectory` for published/deployed scenarios

**Rationale**:
- `AppContext.BaseDirectory` returns the directory containing the application's main assembly
- Works correctly for single-file published apps (which ReqChecker uses)
- More reliable than `Assembly.GetExecutingAssembly().Location` which returns empty string for single-file apps
- More reliable than `Environment.CurrentDirectory` which can change

**Alternatives Considered**:
- `Assembly.GetExecutingAssembly().Location` - Does not work with single-file deployment
- `Environment.CurrentDirectory` - Can be different from app directory
- `Process.GetCurrentProcess().MainModule?.FileName` - More complex, same result

**Code Pattern**:
```csharp
var appDirectory = AppContext.BaseDirectory;
var startupProfilePath = Path.Combine(appDirectory, "startup-profile.json");
```

### 2. When in app startup to check for startup profile?

**Decision**: Check in `OnStartup` after DI container setup, before showing main window

**Rationale**:
- DI services needed for profile loading/validation are available
- Can set app state before first view is created
- NavigationService can be configured to start on TestList instead of ProfileSelector
- Maintains clean separation - startup logic in App.xaml.cs, not spread across ViewModels

**Alternatives Considered**:
- Check in MainWindow constructor - Too late, ProfileSelector already being navigated
- Check in ProfileSelectorViewModel - Wrong responsibility, ViewModel shouldn't control app startup flow
- Check before DI setup - Services not available for profile loading

**Sequence**:
1. App constructor: Configure DI
2. OnStartup: Configure app state, theme service
3. OnStartup: Check for startup-profile.json
4. OnStartup: If found and valid, set current profile in AppState
5. MainWindow constructor: Check if profile already set, navigate accordingly

### 3. How to handle startup profile validation failures?

**Decision**: Show error dialog with option to proceed to profile selector

**Rationale**:
- User needs clear feedback about what went wrong
- User should not be stuck - must have path forward
- Consistent with existing error handling patterns in App.xaml.cs
- Log error details for support troubleshooting

**Error Cases**:
| Condition | User Message | Action |
|-----------|--------------|--------|
| File exists but invalid JSON | "Startup configuration could not be read" | Show profile selector |
| File exists but schema invalid | "Startup configuration format not compatible" | Show profile selector |
| File exists but empty/no tests | Treat as "no startup config" | Show profile selector |
| File permission denied | "Cannot read startup configuration" | Show profile selector |

### 4. Sample profile content - what tests to include?

**Decision**: Include 4-5 representative tests using safe public endpoints

**Rationale**:
- Demonstrates all common test types without requiring internal infrastructure
- Safe to run on any client machine
- Provides template for support teams to customize

**Sample Tests**:
1. **Ping Test** - 8.8.8.8 (Google DNS) - Verifies basic network connectivity
2. **HTTP GET Test** - https://www.example.com - Verifies HTTPS connectivity
3. **DNS Resolution Test** - Resolve www.google.com - Verifies DNS
4. **File Exists Test** - C:\Windows\System32\drivers\etc\hosts - Common Windows file
5. **Registry Read Test** - Windows version key - Safe read-only check

### 5. How to distinguish startup-profile.json from embedded default-profile.json?

**Decision**: Different file locations and loading mechanisms

**Rationale**:
- `startup-profile.json`: External file in app directory, loaded via `IProfileLoader.LoadFromFileAsync`
- `default-profile.json`: Embedded resource, loaded via `Assembly.GetManifestResourceStream`
- Clear separation of concerns: startup = external file for distribution, default = bundled baseline

**Loading Priority**:
1. Check for `startup-profile.json` in app directory (auto-load scenario)
2. If not found/invalid, show profile selector with:
   - Embedded profiles (including sample-diagnostics.json)
   - User profiles from AppData

## Best Practices Applied

### File System Access
- Use async file operations to avoid blocking UI thread
- Handle file system exceptions gracefully (FileNotFoundException, UnauthorizedAccessException, IOException)
- Check file existence before attempting read (optimization, not for security)

### Logging
- Log startup profile detection at Information level
- Log validation failures at Warning level with details
- Log exceptions at Error level with stack trace

### Testing
- Unit tests mock file system via abstraction
- Integration tests use temp directories with actual files
- Test all error conditions (invalid JSON, missing file, permission denied)

## Dependencies

No new external dependencies required. Reuses:
- `System.IO` for file operations
- Existing `IProfileLoader`, `IProfileValidator`, `ProfileMigrationPipeline`
- Existing `Serilog` logging
