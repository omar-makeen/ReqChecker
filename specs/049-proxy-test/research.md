# Research: ProxyConnectivity Test Type

**Feature Branch**: `049-proxy-test`
**Date**: 2026-02-22

## R1: SOCKS Proxy Support in .NET 8

**Decision**: Use built-in `System.Net.WebProxy` with `SocketsHttpHandler` — no new packages required.

**Rationale**: .NET 6+ added native SOCKS4/SOCKS4a/SOCKS5 proxy support via `SocketsHttpHandler`. Setting `new WebProxy("socks5://host:port")` on `HttpClientHandler.Proxy` works out of the box on .NET 8. This aligns with the project's "no new packages" pattern used in recent features (046, 048).

**Alternatives considered**:
- `MihaZupan/HttpToSocks5Proxy` third-party library — unnecessary since .NET 6+ has native support.
- `SocksSharp` library — same reason; adds a dependency for functionality already built into the runtime.

## R2: Proxy Configuration via HttpClient

**Decision**: Create a new `HttpClient` instance per test execution with a configured `HttpClientHandler` (not a static shared client).

**Rationale**: Unlike `HttpGetTest` which uses a static `HttpClient`, the ProxyConnectivity test must configure a different proxy per execution. Creating a handler per test ensures no proxy leakage between tests. The handler is disposed after each execution, which is acceptable for a diagnostic tool (not a high-throughput server).

**Alternatives considered**:
- Static `HttpClient` with per-request proxy — not supported; `HttpClientHandler.Proxy` is set at handler creation time.
- `IHttpClientFactory` — overkill for a single-request test; adds DI complexity with no benefit.

## R3: Distinguishing Proxy vs. Target Errors

**Decision**: Use a two-phase approach: first connect to the proxy (catch proxy-specific errors), then relay to the target URL. Proxy errors (connection refused, auth failure) are identified by exception type and HTTP 407 status. Target errors are identified by the response received through the proxy.

**Rationale**: The spec (FR-008) requires distinct error messages for proxy vs. target failures. The `HttpClient` with proxy configured will throw `HttpRequestException` for proxy connection failures. A successful proxy connection followed by a target failure will return an HTTP response with the appropriate status code.

**Alternatives considered**:
- Single-phase connection with heuristic error categorization — less reliable at distinguishing failure source.

## R4: Evidence Serialization

**Decision**: Use `Dictionary<string, object>` with explicit camelCase keys (matching project convention), serialized via `JsonSerializer.Serialize()`.

**Rationale**: This is the pattern used by most existing tests (HttpGet, EnvironmentVariable, etc.). The WebSocket test (048) was recently fixed to use camelCase via `JsonSerializerOptions` with `CamelCase` policy on a typed evidence class. Using a dictionary with explicit keys is simpler and avoids needing a separate options instance.

**Alternatives considered**:
- Typed evidence class with `JsonNamingPolicy.CamelCase` — viable but adds an extra class for no benefit when a dictionary suffices.

## R5: Conditional Build Integration

**Decision**: Add `ProxyConnectivity` to `TestManifest.props` following the exact same two-entry pattern (KnownTestType + conditional Compile ItemGroup).

**Rationale**: All test types follow this pattern. No deviation needed.

## R6: Credential Redaction

**Decision**: Never include `proxyPassword` in evidence output. Include `proxyUsername` only when authentication was attempted (to help diagnose auth failures).

**Rationale**: The spec assumption states credentials are redacted. This matches security best practices and is consistent with how MtlsConnect handles sensitive PFX passwords.
