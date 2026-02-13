# Implementation Plan: Mutual TLS Client Certificate Authentication Test

**Branch**: `001-mtls-test` | **Date**: 2026-02-13 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-mtls-test/spec.md`

## Summary

Add a new `MtlsConnect` test type to ReqChecker for validating mutual TLS (client certificate) authentication against HTTPS endpoints. The test loads a PFX/PKCS#12 client certificate, performs a TLS handshake with client certificate presentation, and verifies the HTTP response status code. Implementation uses .NET 8's `HttpClient` with `HttpClientHandler.ClientCertificates` for certificate-based authentication. The test follows the established `ITest` + `[TestType]` pattern, integrating with existing execution pipeline, credential prompting (PromptAtRun for PFX password), icon/color converters, and profile system.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: WPF-UI 4.2.0, CommunityToolkit.Mvvm 8.4.0, Microsoft.Extensions.DependencyInjection (all existing — no new packages)
**Storage**: N/A (test results are in-memory; parameters persisted in profile JSON files)
**Testing**: xUnit (existing test project `ReqChecker.Infrastructure.Tests`)
**Target Platform**: Windows desktop (WPF)
**Project Type**: Single WPF desktop application
**Performance Goals**: mTLS test completes within configured timeout (default 30 seconds)
**Constraints**: No new NuGet dependencies; follow existing test implementation patterns exactly; use System.Security.Cryptography.X509Certificates and System.Net.Http
**Scale/Scope**: 1 new test class, 2 converter updates, 1 profile update, unit tests

## Constitution Check

*GATE: Constitution is a blank template — no project-specific gates defined. Proceeding.*

## Project Structure

### Documentation (this feature)

```text
specs/001-mtls-test/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output (technology decisions)
├── data-model.md        # Phase 1 output (parameter/evidence schemas)
├── quickstart.md        # Phase 1 output (implementation guide)
├── checklists/
│   └── requirements.md  # Quality checklist (completed)
├── contracts/
│   └── README.md        # Phase 1 output (interface contracts)
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── Tests/
│   │   ├── MtlsConnectTest.cs            # NEW — mTLS client certificate test
│   │   └── (existing tests unchanged)
│   └── Execution/                          # NO CHANGES
├── ReqChecker.App/
│   ├── Converters/
│   │   ├── TestTypeToIconConverter.cs     # MODIFY — add MtlsConnect icon mapping
│   │   └── TestTypeToColorConverter.cs    # MODIFY — add MtlsConnect color mapping
│   └── Profiles/
│       └── default-profile.json          # MODIFY — add MtlsConnect sample test
└── ReqChecker.Core/                       # NO CHANGES

tests/
└── ReqChecker.Infrastructure.Tests/
    └── Tests/
        └── MtlsConnectTestTests.cs       # NEW — unit tests
```

**Structure Decision**: New mTLS test implementation goes in `src/ReqChecker.Infrastructure/Tests/MtlsConnectTest.cs`, following the established single-class-per-file convention. Unit tests go in `tests/ReqChecker.Infrastructure.Tests/Tests/MtlsConnectTestTests.cs`.

## Complexity Tracking

> No constitutional violations — standard test type addition following established patterns.
