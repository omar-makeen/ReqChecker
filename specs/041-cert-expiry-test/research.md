# Research: SSL Certificate Expiry Test

**Branch**: `041-cert-expiry-test` | **Date**: 2026-02-18

## R1: TLS Connection Approach (SslStream vs HttpClient)

**Decision**: Use `TcpClient` + `SslStream` for the TLS handshake.

**Rationale**: The spec requires testing any direct TLS endpoint (HTTPS, LDAPS, SMTPS, custom ports), not just HTTP/S. `SslStream` over `TcpClient` performs a pure TLS handshake without sending an HTTP request, making it protocol-agnostic. The `RemoteCertificateValidationCallback` captures the server certificate during handshake.

**Alternatives considered**:
- `HttpClient` with `ServerCertificateCustomValidationCallback` — limits testing to HTTP/S endpoints only. The existing `MtlsConnectTest` uses this approach because it needs to verify HTTP status codes, but `CertificateExpiry` only needs the certificate.
- Third-party libraries (e.g., BouncyCastle) — unnecessary; .NET's built-in `SslStream` + `X509Certificate2` provide all required functionality.

## R2: SAN Extension Extraction

**Decision**: Extract Subject Alternative Names from `X509Certificate2.Extensions` using OID `2.5.29.17` (`SubjectAlternativeName`).

**Rationale**: .NET 8 provides `X509SubjectAlternativeNameExtension` which can be cast from the extension collection. This gives access to `EnumerateDnsNames()` for DNS SANs and `EnumerateIPAddresses()` for IP SANs. No additional dependencies needed.

**Alternatives considered**:
- Manual ASN.1 parsing — fragile and unnecessary when .NET provides typed access.
- Only reading Subject DN — would miss SANs, which is where modern certificates place hostnames (per clarification session).

## R3: SNI (Server Name Indication) Behavior

**Decision**: Always send the configured `host` parameter as the SNI hostname during TLS handshake via `SslStream.AuthenticateAsClientAsync(targetHost)`.

**Rationale**: SNI is standard TLS behavior. Sending the hostname ensures the correct certificate is returned from multi-tenant hosts (CDNs, shared hosting, load balancers). The `targetHost` parameter of `AuthenticateAsClientAsync` automatically sets the SNI extension.

**Alternatives considered**:
- Not sending SNI — would retrieve a default certificate that may not match the intended service.
- Making SNI configurable — over-engineering for this use case; the host parameter serves as the natural SNI value.

## R4: Certificate Chain Validation Skip

**Decision**: Use `RemoteCertificateValidationCallback` returning `true` when `skipChainValidation` is enabled, similar to `MtlsConnectTest`'s `skipServerCertValidation` pattern.

**Rationale**: Self-signed certificates and internal CAs are common in enterprise environments. The test's primary purpose is expiry checking, not chain validation. The callback still captures the certificate for inspection regardless of the return value.

**Alternatives considered**:
- Always skip chain validation — would remove a useful safety check for production certificates.
- Separate chain validation as a different test type — over-engineering; a boolean flag is simpler.

## R5: Test Pattern Alignment

**Decision**: Follow the `UdpPortOpenTest` / `MtlsConnectTest` pattern with typed evidence class, inner parameters class, and region-organized BuildXxxResult helpers.

**Rationale**: Consistency with the existing codebase. The newer test types (UDP, mTLS) use this structured approach which provides:
- Type-safe evidence serialization
- Clear parameter extraction with validation
- Consistent result-building helpers for each error category

**Alternatives considered**:
- Dictionary-based evidence (like `TcpPortOpenTest`) — less type-safe, harder to maintain.

## R6: Default Profile Entries

**Decision**: Add two sample `CertificateExpiry` entries to the default profile: one targeting a well-known public endpoint (e.g., `www.google.com:443`) and one targeting `expired.badssl.com:443` to demonstrate the failure path.

**Rationale**: Follows the established pattern where each test type has at least one happy-path and one expected-failure entry in the default profile (see MtlsConnect entries test-013 through test-016, UDP entries test-008 through test-011).

**Alternatives considered**:
- No default profile entries — inconsistent with other test types; users wouldn't see examples.
- Single entry only — misses the opportunity to demonstrate expiry detection.
