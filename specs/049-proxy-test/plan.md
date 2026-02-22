# Implementation Plan: ProxyConnectivity Test Type

**Branch**: `049-proxy-test` | **Date**: 2026-02-22 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/049-proxy-test/spec.md`

## Summary

Add a `ProxyConnectivity` test type that validates HTTP/SOCKS proxy reachability by connecting to a target URL through a specified proxy server. Uses .NET 8's built-in `WebProxy` + `SocketsHttpHandler` for native HTTP, SOCKS4, and SOCKS5 proxy support — no new packages required. The proxy type is inferred from the `proxyUrl` scheme. Supports optional proxy authentication with credential redaction in evidence.

## Technical Context

**Language/Version**: C# 12 / .NET 8.0-windows (net8.0-windows TFM)
**Primary Dependencies**: System.Net.Http (HttpClient, HttpClientHandler, WebProxy — all built-in, no new packages)
**Storage**: N/A (in-memory test results; parameters persisted in profile JSON files)
**Testing**: Manual testing via app launch (consistent with all other test types in this project)
**Target Platform**: Windows 10/11 (x64)
**Project Type**: Single — WPF desktop application with layered architecture
**Performance Goals**: Complete proxy test within configured timeout (default 30000ms)
**Constraints**: Must not introduce new NuGet packages; must follow existing test implementation patterns
**Scale/Scope**: Single new test type; 6 files created/modified

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Constitution is unconfigured (template placeholders only). No gates to evaluate. Proceeding.

**Post-Phase 1 re-check**: No constitution violations — constitution is unconfigured.

## Project Structure

### Documentation (this feature)

```text
specs/049-proxy-test/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output — technology decisions
├── data-model.md        # Phase 1 output — parameters & evidence schema
├── quickstart.md        # Phase 1 output — usage examples
└── checklists/
    └── requirements.md  # Specification quality checklist
```

### Source Code (repository root)

```text
src/
├── ReqChecker.Infrastructure/
│   ├── Tests/
│   │   └── ProxyConnectivityTest.cs    # NEW — test implementation
│   └── TestManifest.props              # MODIFY — add conditional build entry
├── ReqChecker.App/
│   ├── Converters/
│   │   └── TestResultDetailsConverter.cs  # MODIFY — add [Proxy] evidence section
│   └── Profiles/
│       ├── default-profile.json           # MODIFY — add sample ProxyConnectivity test
│       └── sample-diagnostics.json        # MODIFY — add sample ProxyConnectivity test
README.md                                  # MODIFY — update count 24→25, add docs
```

**Structure Decision**: Follows the existing single-project structure. New test class goes in `Infrastructure/Tests/` alongside all other test implementations. No new projects or directories needed.
