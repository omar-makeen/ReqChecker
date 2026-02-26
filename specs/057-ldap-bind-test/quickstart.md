# Quickstart: LdapBind Test Type

**Feature**: 057-ldap-bind-test | **Date**: 2026-02-26

## Implementation Order

Execute tasks in this order (each builds on the previous):

### 1. Add NuGet Package
```bash
dotnet add src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj package System.DirectoryServices.Protocols --version 8.0.0
```

### 2. Create Test Implementation
**File**: `src/ReqChecker.Infrastructure/Tests/LdapBindTest.cs`

Follow the SmtpConnectTest pattern:
- `[TestType("LdapBind")]` attribute
- Implements `ITest` interface
- Parameters from `testDefinition.Parameters` JsonObject
- Credentials from `TestExecutionContext`
- Evidence as `Dictionary<string, object>` → serialized to `TestEvidence.ResponseData`

### 3. Register in Build Manifest
**File**: `src/ReqChecker.Infrastructure/TestManifest.props`

Add `KnownTestType` entry and conditional `Compile Include` block.

### 4. Add Details Converter Section
**File**: `src/ReqChecker.App/Converters/TestResultDetailsConverter.cs`

Add `[LdapBind]` section detection using `bindType` + `responseTimeMs` keys.

### 5. Add Default Profile Entry
**File**: `src/ReqChecker.App/Profiles/default-profile.json`

Add test-043 entry with type `LdapBind`.

### 6. Update README
**File**: `README.md`

Add LdapBind to the Network category table and detailed reference section. Update test type count from 31 to 32.

## Build & Verify
```bash
# Full build (includes all 32 test types)
dotnet build src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj

# Selective build (LdapBind only)
dotnet build src/ReqChecker.Infrastructure/ReqChecker.Infrastructure.csproj /p:IncludeTests="LdapBind"

# Full app build
dotnet build src/ReqChecker.App/ReqChecker.App.csproj
```
