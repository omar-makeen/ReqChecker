# Research: ReqChecker Desktop Application

**Branch**: `001-reqchecker-desktop-app` | **Date**: 2026-01-30

## Technology Decisions

### TD-001: UI Framework - WPF-UI

**Decision**: WPF-UI 3.x (https://wpfui.lepo.co/)

**Rationale**:
- Native Fluent Design System implementation for WPF
- Windows 11 style controls out of the box
- Active development (v3.x released 2024, ongoing updates)
- MIT license - suitable for commercial distribution
- Includes: NavigationView, InfoBar, ProgressRing, ContentDialog, NumberBox
- Automatic dark/light theme support via Windows accent colors
- Minimal learning curve for WPF developers

**Alternatives Considered**:
| Library | Rejected Because |
|---------|------------------|
| MaterialDesignInXaml | Material Design doesn't fit Windows enterprise aesthetic |
| MahApps.Metro | Metro design is dated; no Fluent Design support |
| HandyControl | Less polished; Chinese documentation primary |
| ModernWpf | Abandoned; no .NET 8 support |
| Custom XAML | Too time-intensive for required quality level |

**Integration Notes**:
```xml
<PackageReference Include="WPF-UI" Version="3.*" />
```
- Requires `ApplicationHostBuilder` for proper theme initialization
- Use `UiWindow` base class instead of `Window`
- Include `ui:ThemesDictionary` and `ui:ControlsDictionary` in App.xaml

---

### TD-002: Dependency Injection - Microsoft.Extensions.DependencyInjection

**Decision**: Use Microsoft.Extensions.DependencyInjection with keyed services

**Rationale**:
- Official Microsoft library with long-term support
- Keyed services (.NET 8+) perfect for test type resolution
- Familiar to .NET developers
- Integrates with Microsoft.Extensions.Hosting for app lifecycle

**Pattern for Test Registration**:
```csharp
// Attribute on test implementations
[TestType("HttpGet")]
public class HttpGetTest : ITest { }

// Registration at startup via reflection
services.AddKeyedSingleton<ITest, HttpGetTest>("HttpGet");

// Resolution by profile test type
var test = provider.GetRequiredKeyedService<ITest>(testDefinition.Type);
```

---

### TD-003: Logging - Serilog

**Decision**: Serilog with File and Debug sinks

**Rationale**:
- Industry standard for .NET structured logging
- Excellent performance for high-volume logging
- Rolling file sink with size/age limits built-in
- JSON structured output for diagnostics parsing
- Integrates with Microsoft.Extensions.Logging abstraction

**Configuration**:
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        path: Path.Combine(AppData, "Logs", "reqchecker-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 100_000_000)
    .WriteTo.Debug()
    .CreateLogger();
```

---

### TD-004: JSON Serialization - System.Text.Json

**Decision**: System.Text.Json with source generators

**Rationale**:
- Built into .NET, no additional dependency
- Source generators eliminate reflection for AOT compatibility
- Performance superior to Newtonsoft.Json for our use case
- Native `JsonDocument` for schema validation without full deserialization

**Configuration**:
```csharp
[JsonSerializable(typeof(Profile))]
[JsonSerializable(typeof(RunReport))]
public partial class AppJsonContext : JsonSerializerContext { }
```

---

### TD-005: CSV Export - CsvHelper

**Decision**: CsvHelper library

**Rationale**:
- Most popular .NET CSV library (100M+ downloads)
- Handles edge cases (escaping, encoding) correctly
- Streaming support for large exports
- MIT license

**Alternative Considered**:
- Manual CSV writing: Error-prone for edge cases (embedded commas, quotes)

---

### TD-006: Validation - FluentValidation

**Decision**: FluentValidation for profile schema validation

**Rationale**:
- Expressive validation rules that map to user-friendly messages
- Separation of validation logic from models
- Supports complex cross-property validation
- Well-documented for generating specific error messages

**Example**:
```csharp
public class TestDefinitionValidator : AbstractValidator<TestDefinition>
{
    public TestDefinitionValidator()
    {
        RuleFor(x => x.Type).NotEmpty().WithMessage("Test type is required");
        RuleFor(x => x.Id).NotEmpty().WithMessage("Test ID is required");
        RuleFor(x => x.Parameters).NotNull().WithMessage("Parameters must be provided");
    }
}
```

---

### TD-007: Windows Credential Manager - CredentialManagement

**Decision**: Use Windows Credential Manager via P/Invoke wrapper

**Rationale**:
- Native Windows secure storage
- User-scoped credentials (not machine-wide)
- No additional software required on target machines
- Survives application uninstall (credentials persist if user wants)

**Implementation Approach**:
- Use `Credential` class from CredentialManagement NuGet package (or direct P/Invoke)
- Store with target name pattern: `ReqChecker:{credentialRef}`
- Retrieve username/password pair when test references `credentialRef`

**Fallback**:
- If credential not found: prompt user via secure dialog
- Offer "Remember" checkbox to save to Credential Manager

---

### TD-008: FTP Client - FluentFTP

**Decision**: FluentFTP library

**Rationale**:
- Most comprehensive .NET FTP library
- Native async/await support
- TLS/SSL support (FTPS) built-in
- Passive mode configuration
- Active maintenance

**Usage Pattern**:
```csharp
using var client = new AsyncFtpClient(host);
client.Config.EncryptionMode = FtpEncryptionMode.Explicit; // FTPS
client.Config.DataConnectionType = FtpDataConnectionType.PASV;
await client.Connect(cancellationToken);
```

---

### TD-009: HTTP Client - System.Net.Http.HttpClient

**Decision**: Built-in HttpClient with IHttpClientFactory

**Rationale**:
- No external dependency needed
- IHttpClientFactory manages connection pooling
- Supports all required features (headers, timeouts, cancellation)
- Native async support

**Configuration**:
```csharp
services.AddHttpClient("ReqChecker", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); // Default
});
```

---

### TD-010: Profile Integrity - HMAC-SHA256

**Decision**: HMAC-SHA256 signature for company-managed profiles

**Implementation**:
1. Normalize profile JSON (sorted keys, no whitespace)
2. Compute HMAC-SHA256 using embedded key
3. Store signature in separate `.sig` file alongside profile
4. Verify before loading; reject on mismatch

**Key Storage**:
- Embed key in application binary as obfuscated byte array
- Not cryptographically secure against determined attackers
- Sufficient to prevent accidental edits and casual tampering

**Verification Flow**:
```
Load JSON → Normalize → Compute HMAC → Compare to .sig → Accept/Reject
```

---

### TD-011: Installer - WiX Toolset v4

**Decision**: WiX Toolset v4 for MSI creation

**Rationale**:
- Industry standard for Windows installers
- MSI format expected in enterprise environments
- Supports per-user and per-machine install
- Can bundle self-contained .NET app

**Alternative Considered**:
- MSIX: Less familiar to enterprise IT; app store association concerns
- Inno Setup: EXE not MSI; some enterprises block non-MSI

---

## Best Practices Applied

### BP-001: MVVM Pattern

- ViewModels implement `INotifyPropertyChanged`
- No code-behind in Views (except InitializeComponent)
- Commands via `RelayCommand` (CommunityToolkit.Mvvm)
- Navigation via service, not direct View references

### BP-002: Async All The Way

- All I/O operations are async
- Use `ValueTask` for hot paths
- `ConfigureAwait(false)` in library code
- Dispatcher for UI updates only

### BP-003: Error Handling Strategy

```
Presentation Layer: Show user-friendly messages
                    ↑ catches domain exceptions
Application Layer:  Map to error categories
                    ↑ catches infrastructure exceptions
Infrastructure:     Throw typed exceptions with context
                    ↑ catches system exceptions
```

### BP-004: Test Evidence Collection

Each test captures:
- Raw response/output (truncated to 4KB)
- Timing breakdown (connect, execute, total)
- Exception details if failed
- Screenshots not applicable (CLI/background tests)

---

## Risk Mitigations

| Risk | Mitigation |
|------|------------|
| WPF-UI breaking changes | Pin to specific version; test upgrades |
| FTP TLS certificate issues | Allow user-configurable certificate validation |
| Large profile files slow UI | Async loading with progress indicator |
| Credential Manager unavailable | Graceful fallback to runtime prompt only |
| Test timeout hangs process | Use CancellationToken with Task.WhenAny timeout |
