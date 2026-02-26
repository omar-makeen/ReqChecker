# Implementation Plan: LdapBind Test Type

**Branch**: `057-ldap-bind-test` | **Date**: 2026-02-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/057-ldap-bind-test/spec.md`

## Summary

Add an `LdapBind` test type that validates LDAP/Active Directory server connectivity by performing a bind operation. Supports anonymous and authenticated (simple) binds, implicit SSL/TLS (LDAPS), and credential resolution via `credentialRef`. Uses `System.DirectoryServices.Protocols` NuGet package with `LdapConnection` for the LDAP protocol implementation. Follows the established test type pattern (SmtpConnect, RegistryWrite) for parameter extraction, evidence capture, details converter, build manifest, default profile, and README documentation.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 (net8.0 TFM)
**Primary Dependencies**: `System.DirectoryServices.Protocols` 8.0.0 (new NuGet package), existing project infrastructure
**Storage**: N/A (in-memory test results)
**Testing**: Manual verification (no unit test project exists for Infrastructure)
**Target Platform**: Windows (net8.0-windows for App, net8.0 for Infrastructure)
**Project Type**: Single (WPF desktop application)
**Performance Goals**: Test completes within 10-second timeout
**Constraints**: Connection timeout 10s, no referral chasing, skip certificate validation
**Scale/Scope**: 1 new test type (32 total), 6 files modified/created

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is unconfigured (template placeholders only). No gates to enforce. Proceeding.

**Post-design re-check**: No violations. The implementation follows all established project patterns:
- Single new file in existing `Tests/` directory
- Reuses existing interfaces (`ITest`), models (`TestResult`, `TestEvidence`), and registration (reflection-based DI)
- One new NuGet dependency (Microsoft-maintained, matches existing pattern of `System.*` packages)

## Project Structure

### Documentation (this feature)

```text
specs/057-ldap-bind-test/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 research output
├── data-model.md        # Phase 1 data model
├── quickstart.md        # Phase 1 quickstart guide
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── ReqChecker.Infrastructure.csproj   # MODIFY: add System.DirectoryServices.Protocols package
│   ├── TestManifest.props                 # MODIFY: add LdapBind entry (KnownTestType + Compile)
│   └── Tests/
│       └── LdapBindTest.cs               # CREATE: test implementation
├── ReqChecker.App/
│   ├── Converters/
│   │   └── TestResultDetailsConverter.cs  # MODIFY: add [LdapBind] section
│   └── Profiles/
│       └── default-profile.json           # MODIFY: add test-043 entry
README.md                                  # MODIFY: add LdapBind documentation, update count 31→32
```

**Structure Decision**: Follows the existing single-project structure. All test implementations live in `src/ReqChecker.Infrastructure/Tests/`. The new `LdapBindTest.cs` is auto-discovered via reflection-based DI registration in `App.xaml.cs`.

## File Change Details

### 1. CREATE: `src/ReqChecker.Infrastructure/Tests/LdapBindTest.cs`

New test implementation following the SmtpConnect pattern:

```csharp
namespace ReqChecker.Infrastructure.Tests;

[TestType("LdapBind")]
public class LdapBindTest : ITest
{
    private const int DefaultLdapPort = 389;
    private const int DefaultLdapsPort = 636;
    private const int TimeoutMs = 10000;

    public async Task<TestResult> ExecuteAsync(
        TestDefinition testDefinition,
        TestExecutionContext? context,
        CancellationToken cancellationToken = default)
    { ... }
}
```

**Implementation flow**:
1. Extract parameters: `server` (required), `port` (optional), `useSsl` (optional, default false), `credentialRef` (optional)
2. Validate: server not empty, port in range 1–65535
3. Determine default port: 636 if `useSsl` true, else 389
4. Create `LdapDirectoryIdentifier(server, port, false, false)`
5. Create `LdapConnection(identifier)` with:
   - `SessionOptions.SecureSocketLayer = useSsl`
   - `SessionOptions.ReferralChasing = ReferralChasingOptions.None`
   - `SessionOptions.VerifyServerCertificate = (_, _) => true` (skip validation)
   - `Timeout = TimeSpan.FromMilliseconds(TimeoutMs)`
   - `AuthType = AuthType.Basic` (for authenticated) or `AuthType.Anonymous`
6. Start `Stopwatch`, call `connection.Bind()` or `connection.Bind(new NetworkCredential(...))`
7. Build evidence dictionary with all required keys
8. Populate `TestResult` with Pass/Fail, evidence, timing, and error details

**Error mapping** (exception → ErrorCategory):
- `ArgumentException` → `ErrorCategory.Configuration`
- `LdapException` (timeout) → `ErrorCategory.Timeout`
- `LdapException` (invalid credentials) → `ErrorCategory.Permission`
- `LdapException` (other) → `ErrorCategory.Network`
- `SocketException` → `ErrorCategory.Network`
- `OperationCanceledException` → `TestStatus.Skipped`

### 2. MODIFY: `src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj`

Add NuGet package reference:
```xml
<PackageReference Include="System.DirectoryServices.Protocols" Version="8.0.0" />
```

### 3. MODIFY: `src/ReqChecker.Infrastructure/TestManifest.props`

**Step 2** — Add to KnownTestType registry (after RegistryWrite, line 62):
```xml
<KnownTestType Include="LdapBind"         SourceFile="Tests\LdapBindTest.cs" />
```

**Step 4** — Add conditional compile block (after RegistryWrite block, line 202):
```xml
<ItemGroup Condition="'$(IncludeTests)' == '' Or $(_IncludeTestsFenced.Contains(';LdapBind;'))">
  <Compile Include="Tests\LdapBindTest.cs" />
</ItemGroup>
```

**Comment** — Update count in header comment from 31 to 32.

### 4. MODIFY: `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`

Add `[LdapBind]` section before the `[Response]` section. Detection keys: `bindType` + `responseTimeMs`.

```csharp
// [LdapBind] section
if (evidenceData != null && evidenceData.ContainsKey("bindType") && evidenceData.ContainsKey("responseTimeMs"))
{
    sections.Add("[LdapBind]");
    // Server — always
    if (evidenceData.TryGetValue("server", out var ldapServerObj) && ldapServerObj != null)
        sections.Add($"Server:     {ldapServerObj}");
    // Port — always
    if (evidenceData.TryGetValue("port", out var ldapPortObj) && ldapPortObj != null)
        sections.Add($"Port:       {ldapPortObj}");
    // Bind — always (anonymous/authenticated, n/a on failure)
    if (evidenceData.TryGetValue("bindType", out var bindTypeObj) && bindTypeObj != null)
        sections.Add($"Bind:       {bindTypeObj}");
    // Time — always (n/a if negative)
    if (evidenceData.TryGetValue("responseTimeMs", out var ldapTimeObj) && ldapTimeObj != null &&
        long.TryParse(ldapTimeObj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ldapTime))
        sections.Add($"Time:       {(ldapTime >= 0 ? $"{ldapTime} ms" : "n/a")}");
    // TLS — conditional (only when useSsl true and tlsNegotiated)
    if (evidenceData.TryGetValue("tlsNegotiated", out var ldapTlsNegObj) && ldapTlsNegObj?.ToString() is "True" or "true")
    {
        if (evidenceData.TryGetValue("tlsVersion", out var ldapTlsVerObj) && ldapTlsVerObj != null)
            sections.Add($"TLS:        {ldapTlsVerObj}");
    }
    // Auth — conditional (only when credentialRef was provided)
    if (evidenceData.TryGetValue("authenticated", out var ldapAuthObj) && ldapAuthObj != null)
        sections.Add($"Auth:       {(ldapAuthObj.ToString() is "True" or "true" ? "yes" : "no")}");
    sections.Add(string.Empty);
}
```

### 5. MODIFY: `src/ReqChecker.App/Profiles/default-profile.json`

Add test-043 entry after the RegistryWrite entry (test-042):

```json
{
  "id": "test-043",
  "type": "LdapBind",
  "displayName": "LDAP Server Bind Check",
  "description": "Verifies LDAP/Active Directory server connectivity via anonymous bind.",
  "parameters": {
    "server": "dc.example.com",
    "port": 389
  },
  "fieldPolicy": {
    "server": "Editable",
    "port": "Editable",
    "useSsl": "Editable",
    "credentialRef": "Editable"
  },
  "timeout": null,
  "retryCount": null,
  "requiresAdmin": false,
  "dependsOn": []
}
```

### 6. MODIFY: `README.md`

- Add `LdapBind` to the Network category in the test type summary table
- Update total count from 31 to 32 (in header and any other occurrences)
- Add detailed `#### LdapBind` reference section under Network tests with parameter table and JSON example
