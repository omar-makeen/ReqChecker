# Research: LdapBind Test Type

**Feature**: 057-ldap-bind-test | **Date**: 2026-02-26

## R-001: System.DirectoryServices.Protocols NuGet Package

**Decision**: Add `System.DirectoryServices.Protocols` NuGet package to `ReqChecker.Infrastructure.csproj`.

**Rationale**: On .NET 8.0 (non-Framework), `System.DirectoryServices.Protocols` is distributed as a separate NuGet package — it is NOT included in the base `net8.0` TFM. The project already follows the pattern of adding platform-specific NuGet packages when needed (e.g., `System.ServiceProcess.ServiceController` for Windows Services, `System.Security.Cryptography.ProtectedData` for DPAPI). Use version `8.0.x` to match the project's .NET 8.0 LTS target.

**Alternatives considered**:
- `System.DirectoryServices` (higher-level, ADSI-based): Rejected — heavier dependency, Windows-only COM interop, less control over LDAP protocol details.
- Raw TCP socket with manual LDAP ASN.1 encoding: Rejected — unnecessary complexity when a well-tested library exists.
- Third-party `Novell.Directory.Ldap.NETStandard`: Rejected — introduces an external dependency when Microsoft's own package is available and sufficient.

## R-002: LDAP Bind Implementation Pattern

**Decision**: Use `LdapConnection` + `LdapDirectoryIdentifier` from `System.DirectoryServices.Protocols`.

**Rationale**: This is the standard .NET approach for low-level LDAP operations. It maps directly to the spec requirements:
- `LdapDirectoryIdentifier(server, port, false, false)` — creates identifier without connection-level referrals
- `connection.SessionOptions.SecureSocketLayer = true` — enables LDAPS (implicit SSL)
- `connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None` — disables referral chasing (per clarification)
- `connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true` — skips certificate validation
- `connection.Bind()` — anonymous bind
- `connection.Bind(new NetworkCredential(username, password))` — authenticated simple bind
- `connection.Timeout = TimeSpan.FromMilliseconds(10000)` — connection timeout

**Alternatives considered**:
- STARTTLS via `connection.SessionOptions.StartTransportLayerSecurity(null)`: Rejected per spec — LDAPS-only approach keeps implementation simpler. Users needing encryption use `useSsl: true` with port 636.

## R-003: Evidence Key Design for Converter Detection

**Decision**: Use `bindType` + `responseTimeMs` as the detection key pair for the `[LdapBind]` section in `TestResultDetailsConverter.cs`.

**Rationale**: The converter uses pairs of unique evidence keys to detect which test type section to render. `bindType` is unique to LdapBind (no other test uses this key), and `responseTimeMs` confirms it's a timed network test. This avoids collision with SmtpConnect (which uses `serverBanner` + `responseTimeMs`) and all other test types.

**Evidence keys**:
- `server` (string) — always present
- `port` (int) — always present
- `useSsl` (bool) — always present
- `bindType` (string: "anonymous" or "authenticated") — always present
- `responseTimeMs` (long) — always present
- `tlsNegotiated` (bool) — conditional, only when useSsl is true
- `tlsVersion` (string) — conditional, only when TLS negotiated
- `authenticated` (bool) — conditional, only when credentialRef provided
- `warning` (string) — conditional, only when credentials sent without TLS

## R-004: TLS Version Detection

**Decision**: Extract TLS version from `SslStream` after wrapping the LDAP connection's underlying stream.

**Rationale**: `LdapConnection` in `System.DirectoryServices.Protocols` does not directly expose the negotiated TLS version. However, since we're using implicit SSL (LDAPS), the TLS negotiation happens at the transport level. The `SessionOptions` class provides `SslInformation` after the connection is established, which contains the protocol version. If `SslInformation` is not available (older .NET behavior), we can fall back to reporting `tlsNegotiated: true` without the specific version.

**Alternatives considered**:
- Manual TCP + SslStream + LDAP: Rejected — overly complex for extracting one field; the `SessionOptions.SslInformation` approach is cleaner.
