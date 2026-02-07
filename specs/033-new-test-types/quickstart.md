# Quickstart: 033-new-test-types

**Date**: 2026-02-07
**Branch**: `033-new-test-types`

## What This Feature Does

Adds four new test types to ReqChecker that IT admins can use in deployment-readiness profiles:

1. **DnsResolve** — Resolves a hostname and optionally checks for an expected IP address
2. **TcpPortOpen** — Checks if a TCP port is open on a target host
3. **WindowsService** — Checks if a Windows service is installed and in the expected state
4. **DiskSpace** — Checks if a drive/volume has enough free space

## Files to Create

| File | Purpose |
|------|---------|
| `src/ReqChecker.Infrastructure/Tests/DnsResolveTest.cs` | DNS resolution test implementation |
| `src/ReqChecker.Infrastructure/Tests/TcpPortOpenTest.cs` | TCP port connectivity test |
| `src/ReqChecker.Infrastructure/Tests/WindowsServiceTest.cs` | Windows service status check |
| `src/ReqChecker.Infrastructure/Tests/DiskSpaceTest.cs` | Disk space threshold check |

## Files to Modify

| File | Change |
|------|--------|
| `src/ReqChecker.App/Converters/TestTypeToIconConverter.cs` | Add icon mappings for 4 new types |
| `src/ReqChecker.App/Converters/TestTypeToColorConverter.cs` | Add color mappings for 4 new types |
| `src/ReqChecker.Infrastructure/Execution/SequentialTestRunner.cs` | Add `DnsLookup` → `DnsResolve` alias |
| `src/ReqChecker.App/Profiles/sample-diagnostics.json` | Change test 003 type from `DnsLookup` to `DnsResolve` |
| `src/ReqChecker.App/Profiles/default-profile.json` | Add TcpPortOpen and DiskSpace test entries |

## Implementation Pattern

Each test class follows this structure (see `PingTest.cs` as reference):

```
1. [TestType("TypeName")] attribute
2. Implement ITest.ExecuteAsync(TestDefinition, TestExecutionContext?, CancellationToken)
3. Create TestResult with initial fail status
4. Start Stopwatch
5. Extract + validate parameters from TestDefinition.Parameters
6. Execute test logic (async, honor cancellationToken)
7. Build evidence Dictionary<string, object> and serialize to JSON
8. Set TestEvidence.ResponseData + TimingBreakdown
9. Handle OperationCanceledException → Skipped
10. Handle domain exceptions → Fail with appropriate ErrorCategory
```

## Testing Strategy

- Unit tests for each test type in `tests/ReqChecker.Infrastructure.Tests/Tests/`
- Follow existing pattern from `SequentialTestRunnerTests.cs` for mock-based testing
- Test both happy path (pass) and failure scenarios (invalid params, missing resources)
- WindowsService tests use `#if WINDOWS` guards to skip on non-Windows CI

## Build & Verify

```bash
dotnet build
dotnet test
```

No new NuGet packages needed — all four test types use built-in .NET 8 APIs.
