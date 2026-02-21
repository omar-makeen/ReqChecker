# Quickstart: SystemRam & CpuCores Hardware Tests

**Feature**: 046-hardware-tests | **Date**: 2026-02-21

## Files to Create

| File | Purpose |
|------|---------|
| `src/ReqChecker.Infrastructure/Tests/SystemRamTest.cs` | SystemRam test implementation |
| `src/ReqChecker.Infrastructure/Tests/CpuCoresTest.cs` | CpuCores test implementation |

## Files to Edit

| File | Change |
|------|--------|
| `src/ReqChecker.Infrastructure/TestManifest.props` | Register both test types (KnownTestType + conditional Compile) |
| `src/ReqChecker.App/Profiles/default-profile.json` | Add 6 test entries (test-028 through test-033): 3 SystemRam + 3 CpuCores |
| `src/ReqChecker.App/Profiles/sample-diagnostics.json` | Add 6 test entries (UUIDs …0005 through …000a): 3 SystemRam + 3 CpuCores |

## Implementation Order

1. **SystemRamTest.cs** — Create test class with `[TestType("SystemRam")]`, implement `ExecuteAsync` using `ComputerInfo().TotalPhysicalMemory`, optional `minimumGB` threshold
2. **CpuCoresTest.cs** — Create test class with `[TestType("CpuCores")]`, implement `ExecuteAsync` using `Environment.ProcessorCount`, optional `minimumCores` threshold
3. **TestManifest.props** — Add KnownTestType entries and conditional Compile ItemGroups for both
4. **default-profile.json** — Append SystemRam and CpuCores entries in informational mode
5. **sample-diagnostics.json** — Append SystemRam and CpuCores entries in informational mode

## Key Patterns to Follow

- Reference: `DiskSpaceTest.cs` for threshold comparison pattern
- Reference: `OsVersionTest.cs` for informational mode pattern
- DI registration: Automatic via reflection (no manual wiring needed)
- Test IDs in default-profile: Sequential (`test-028` through `test-033`)
- Test IDs in sample-diagnostics: UUID format (`10000000-0000-0000-0000-000000000005` through `…000a`)
- 3 entries per type: informational, low threshold (expected pass), unreachable threshold (expected fail)

## Verification Commands

```bash
# Full build
dotnet build

# Filtered build — SystemRam only
dotnet build src/ReqChecker.App -p:IncludeTests=SystemRam

# Filtered build — both hardware tests
dotnet build src/ReqChecker.App -p:IncludeTests="SystemRam;CpuCores"
```
