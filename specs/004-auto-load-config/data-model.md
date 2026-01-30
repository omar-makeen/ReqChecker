# Data Model: Auto-Load Bundled Configuration

**Feature**: 004-auto-load-config
**Date**: 2026-01-30

## Entities

### Existing Entities (No Changes)

#### Profile
The startup profile uses the existing `Profile` model with no modifications.

```csharp
// Location: src/ReqChecker.Core/Models/Profile.cs
public class Profile
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int SchemaVersion { get; set; }
    public ProfileSource Source { get; set; }
    public RunSettings RunSettings { get; set; }
    public List<TestDefinition> Tests { get; set; }
    public string? Signature { get; set; }
}
```

#### ProfileSource Enum
The startup profile will use `ProfileSource.UserProvided` since it's an external file (not embedded).

```csharp
// Location: src/ReqChecker.Core/Enums/ProfileSource.cs
public enum ProfileSource
{
    Bundled,      // Embedded resource profiles
    UserProvided  // External file profiles (including startup-profile.json)
}
```

### New Entities

#### StartupProfileResult
Result of attempting to load a startup profile.

```csharp
// Location: src/ReqChecker.Core/Models/StartupProfileResult.cs
public class StartupProfileResult
{
    /// <summary>
    /// Whether a startup profile was found and loaded successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The loaded profile, if successful.
    /// </summary>
    public Profile? Profile { get; init; }

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Whether a startup profile file was found (regardless of validity).
    /// </summary>
    public bool FileFound { get; init; }

    // Factory methods for clarity
    public static StartupProfileResult NotFound() => new()
    {
        Success = false,
        FileFound = false
    };

    public static StartupProfileResult Loaded(Profile profile) => new()
    {
        Success = true,
        Profile = profile,
        FileFound = true
    };

    public static StartupProfileResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error,
        FileFound = true
    };
}
```

## Interfaces

### IStartupProfileService
Service for detecting and loading startup profiles.

```csharp
// Location: src/ReqChecker.Core/Interfaces/IStartupProfileService.cs
public interface IStartupProfileService
{
    /// <summary>
    /// Attempts to load a startup profile from the application directory.
    /// </summary>
    /// <returns>Result containing the profile or error information.</returns>
    Task<StartupProfileResult> TryLoadStartupProfileAsync();

    /// <summary>
    /// Gets the expected path for the startup profile.
    /// </summary>
    string GetStartupProfilePath();
}
```

## File Formats

### startup-profile.json
Uses existing profile JSON schema. Located alongside application executable.

**Location**: `{AppDirectory}/startup-profile.json`

**Schema**: Same as existing profile schema (validated by `IProfileValidator`)

```json
{
  "id": "startup-profile-guid",
  "name": "Client Diagnostics",
  "schemaVersion": 2,
  "source": "UserProvided",
  "runSettings": {
    "defaultTimeout": 30000,
    "defaultRetryCount": 0,
    "retryBackoff": "None",
    "adminBehavior": "SkipWithReason"
  },
  "tests": [
    // TestDefinition objects
  ],
  "signature": null
}
```

### sample-diagnostics.json (Embedded Resource)
Template profile for support teams. Embedded as application resource.

**Location**: `src/ReqChecker.App/Profiles/sample-diagnostics.json`

**Purpose**: Provides a starting point for support teams to create client-specific profiles.

```json
{
  "id": "sample-diagnostics-001",
  "name": "Sample Diagnostics",
  "schemaVersion": 2,
  "source": "Bundled",
  "runSettings": {
    "defaultTimeout": 30000,
    "defaultRetryCount": 0,
    "retryBackoff": "None",
    "adminBehavior": "SkipWithReason"
  },
  "tests": [
    {
      "id": "sample-001",
      "type": "Ping",
      "displayName": "Network Connectivity Check",
      "description": "Pings Google DNS to verify basic network connectivity.",
      "parameters": { "host": "8.8.8.8", "timeout": 5000 },
      "fieldPolicy": { "host": "Editable", "timeout": "Editable" },
      "timeout": null,
      "retryCount": null,
      "requiresAdmin": false
    },
    {
      "id": "sample-002",
      "type": "HttpGet",
      "displayName": "HTTPS Connectivity Check",
      "description": "Verifies HTTPS connectivity to example.com.",
      "parameters": {
        "url": "https://www.example.com",
        "expectedStatusCodes": [200],
        "validateSsl": true
      },
      "fieldPolicy": { "url": "Editable", "expectedStatusCodes": "Editable" },
      "timeout": null,
      "retryCount": null,
      "requiresAdmin": false
    },
    {
      "id": "sample-003",
      "type": "DnsLookup",
      "displayName": "DNS Resolution Check",
      "description": "Verifies DNS can resolve google.com.",
      "parameters": { "hostname": "www.google.com" },
      "fieldPolicy": { "hostname": "Editable" },
      "timeout": null,
      "retryCount": null,
      "requiresAdmin": false
    },
    {
      "id": "sample-004",
      "type": "FileExists",
      "displayName": "System File Check",
      "description": "Verifies Windows hosts file exists.",
      "parameters": { "path": "C:\\Windows\\System32\\drivers\\etc\\hosts" },
      "fieldPolicy": { "path": "Editable" },
      "timeout": null,
      "retryCount": null,
      "requiresAdmin": false
    }
  ],
  "signature": null
}
```

## State Transitions

### Application Startup Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                        App.OnStartup()                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────────┐
                    │ TryLoadStartupProfile│
                    └─────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
        ┌─────────┐    ┌──────────┐    ┌──────────┐
        │Not Found│    │ Success  │    │  Error   │
        └─────────┘    └──────────┘    └──────────┘
              │               │               │
              │               │               │
              ▼               ▼               ▼
     ┌────────────┐  ┌─────────────┐  ┌──────────────┐
     │Show Profile│  │Set AppState │  │Show Error    │
     │ Selector   │  │CurrentProfile│  │Dialog, then  │
     └────────────┘  └─────────────┘  │Profile Select│
                           │          └──────────────┘
                           ▼
                    ┌─────────────┐
                    │Navigate to  │
                    │ Test List   │
                    └─────────────┘
```

## Validation Rules

### Startup Profile Validation
Uses existing `IProfileValidator` with these checks:
1. Valid JSON syntax
2. Required fields present (id, name, schemaVersion, tests)
3. Tests array not empty (empty = treat as "not found")
4. Schema version supported (migrated if needed via `ProfileMigrationPipeline`)

### File Validation
1. File exists at expected path
2. File is readable (not locked, has permissions)
3. File size > 0 bytes
