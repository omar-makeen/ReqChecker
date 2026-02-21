# Quickstart: Conditional Test Builds

## Build with all tests (default, unchanged behavior)

```bash
dotnet build
dotnet publish -c Release
```

## Build with specific test types only

```bash
# Single test type — no escaping needed
dotnet publish -c Release -p:IncludeTests=Ping

# Multiple test types — use %3B to escape semicolons on the command line
dotnet publish -c Release "-p:IncludeTests=Ping%3BHttpGet%3BTcpPortOpen"

# In GitHub Actions (workflow YAML) — semicolons work without escaping
# "/p:IncludeTests=${{ inputs.tests }}"
```

## Common customer build examples

```bash
# Network-only customer
dotnet publish -c Release /p:IncludeTests="Ping;HttpGet;HttpPost;DnsResolve;TcpPortOpen;UdpPortOpen"

# File system customer
dotnet publish -c Release /p:IncludeTests="FileExists;DirectoryExists;FileRead;FileWrite;DiskSpace"

# Full security customer
dotnet publish -c Release /p:IncludeTests="MtlsConnect;CertificateExpiry;HttpGet;TcpPortOpen"
```

## Adding a new test type

1. Create the test class file in `src/ReqChecker.Infrastructure/Tests/` (e.g., `FirewallRuleTest.cs`)
2. Add one entry to `src/ReqChecker.Infrastructure/TestManifest.props`:
   ```xml
   <ItemGroup Condition="$(IncludeTests.Contains('FirewallRule')) Or '$(IncludeTests)' == ''">
     <Compile Include="Tests\FirewallRuleTest.cs" />
   </ItemGroup>
   ```
3. Build normally — the manifest sync check will catch missing entries

## CI/CD (GitHub Actions)

Trigger the "Customer Build" workflow from the Actions tab:
- **tests**: Semicolon-separated list (e.g., `Ping;HttpGet;TcpPortOpen`) or `all`
- **customer-name**: Customer identifier (e.g., `Acme`)
- **version**: Version string (e.g., `1.0.0`)

The artifact will be named `ReqChecker-{customer-name}-v{version}.zip`.

## Validation

The build automatically validates:
- All names in `IncludeTests` exist in the manifest (typos cause build failure)
- All `*Test.cs` files have a manifest entry (new unregistered files cause build failure)
- `dependsOn` references to excluded test types trigger a warning
