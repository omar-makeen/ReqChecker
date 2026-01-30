# Quickstart: Auto-Load Bundled Configuration

**Feature**: 004-auto-load-config
**Date**: 2026-01-30

## Overview

This feature enables automatic loading of a pre-configured test profile when ReqChecker starts. Support teams can bundle a `startup-profile.json` file with the application to provide clients with ready-to-run diagnostics.

## For Support Teams

### Creating a Startup Profile

1. **Open ReqChecker** and select or create a profile with the tests needed for the client
2. **Export the profile**: Use the export function to save the profile as JSON
3. **Rename** the exported file to `startup-profile.json`
4. **Place** the file in the same folder as `ReqChecker.exe`
5. **Package** the folder (zip) and send to the client

### Using the Sample Profile

1. Open ReqChecker without a startup profile
2. Select "Sample Diagnostics" from the bundled profiles
3. Export it and customize for the client's needs
4. Rename to `startup-profile.json` and bundle with the app

### Folder Structure for Distribution

```
ReqChecker/
├── ReqChecker.exe
├── startup-profile.json    ← Auto-loaded on startup
└── (other app files)
```

## For Clients

### When You Receive ReqChecker with Pre-configured Tests

1. Extract the zip file
2. Run `ReqChecker.exe`
3. Tests appear automatically - no setup needed
4. Click "Run" to execute the diagnostics

### If Something Goes Wrong

If the configuration file is corrupted or incompatible:
1. An error message will appear explaining the issue
2. Click "Continue" to open the profile selector
3. Import a new configuration file or contact support

## For Developers

### Key Files

| File | Purpose |
|------|---------|
| `IStartupProfileService.cs` | Interface for startup profile detection |
| `StartupProfileService.cs` | Implementation with file system operations |
| `StartupProfileResult.cs` | Result type for load operations |
| `App.xaml.cs` | Startup flow integration |

### Testing

```bash
# Run all tests
dotnet test

# Run startup profile tests specifically
dotnet test --filter "FullyQualifiedName~StartupProfile"
```

### Startup Flow

```csharp
// In App.OnStartup()
var startupService = Services.GetRequiredService<IStartupProfileService>();
var result = await startupService.TryLoadStartupProfileAsync();

if (result.Success)
{
    appState.SetCurrentProfile(result.Profile);
    // MainWindow will navigate to TestList instead of ProfileSelector
}
else if (result.FileFound)
{
    // Show error dialog, then fall back to ProfileSelector
    ShowStartupProfileError(result.ErrorMessage);
}
// If !FileFound, normal startup to ProfileSelector
```

### Adding to DI Container

```csharp
// In App.ConfigureServices()
services.AddSingleton<IStartupProfileService, StartupProfileService>();
```

## Configuration

### Expected File Name
`startup-profile.json` (case-insensitive on Windows)

### Expected Location
Same directory as `ReqChecker.exe` (`AppContext.BaseDirectory`)

### Profile Schema
Uses existing profile schema (v2). Older schemas are auto-migrated.

## Troubleshooting

| Symptom | Cause | Solution |
|---------|-------|----------|
| Tests don't auto-load | File not named correctly | Rename to `startup-profile.json` |
| Tests don't auto-load | File in wrong location | Move to same folder as .exe |
| "Configuration could not be read" | Invalid JSON | Check JSON syntax |
| "Configuration format not compatible" | Unknown schema version | Re-export from current app version |
| Profile selector appears | startup-profile.json not found | Verify file exists alongside .exe |
