# Research: UDP Port Reachability Test

**Feature**: `039-udp-port-test`
**Date**: 2026-02-13

## Executive Summary

This document captures the technical research and decisions for implementing UDP port reachability testing in ReqChecker. All decisions align with existing architectural patterns (no new dependencies, follows `ITest` interface, uses built-in .NET APIs).

---

## R1: UDP Communication API Selection

**Decision**: Use `System.Net.Sockets.UdpClient` class from .NET 8

**Rationale**:
- Built-in to .NET Framework — no new NuGet dependencies required (matches project constraint)
- Provides high-level async API (`SendAsync`, `ReceiveAsync`) suitable for timeout-based operations
- Supports both IPv4 and IPv6 via constructor overloads and `DualMode` property
- Handles DNS resolution automatically when using hostname-based `Connect()`
- Integrates naturally with `CancellationToken` for timeout enforcement
- Consistent with existing test implementations (e.g., `TcpPortOpenTest` uses `TcpClient`)

**Alternatives Considered**:
1. **Raw `Socket` class**: Lower-level control but requires manual IPv4/IPv6 handling, manual DNS resolution, and more complex async patterns. UdpClient wraps Socket and provides the exact abstraction level needed.
2. **Third-party libraries** (e.g., `DotNetty`, `SuperSocket`): Ruled out due to "no new dependencies" constraint and unnecessary complexity for simple datagram send/receive.

**Implementation Notes**:
- Use `UdpClient.Client.ReceiveTimeout` property to enforce timeout (throw `SocketException` on timeout)
- Alternative: Use `Task.WhenAny` with `ReceiveAsync` and `Task.Delay` for async timeout pattern (preferred for consistency with async/await model)
- ICMP "Port Unreachable" messages are platform-dependent; Windows throws `SocketException` with `SocketError.ConnectionReset`, Linux may require enabling `IP_RECVERR` socket option

---

## R2: Payload Encoding Strategy

**Decision**: Support hex-encoded and base64-encoded string payloads with automatic detection

**Rationale**:
- **Hex encoding** (e.g., `"48656c6c6f"` → `Hello`): Industry standard for protocol specifications (DNS RFCs, SNMP MIBs), familiar to network engineers, unambiguous for binary data
- **Base64 encoding** (e.g., `"SGVsbG8="` → `Hello`): Compact representation, native .NET support via `Convert.FromBase64String()`, handles binary data cleanly
- Automatic detection via heuristic: if string matches `^[0-9A-Fa-f]+$` with even length → hex; if valid base64 → base64; else → raw UTF-8 bytes

**Alternatives Considered**:
1. **Explicit `encoding` parameter** (e.g., `"encoding": "hex"` or `"base64"`): More explicit but adds configuration complexity. Research shows most profiles will use hex (99% of network protocol examples use hex dumps).
2. **UTF-8 string only**: Insufficient for binary protocols; users would need external tools to generate hex strings manually.
3. **File reference** (e.g., `"payloadFile": "dns-query.bin"`): Adds file I/O complexity and breaks profile portability. Inline encoding keeps profiles self-contained.

**Implementation Notes**:
- Default payload when none specified: single null byte (`0x00`) — this ensures the UDP socket sends something (required for many services to respond)
- Use `Convert.FromHexString()` (.NET 5+) for hex decoding
- Use `Convert.FromBase64String()` for base64 decoding
- Validation: Reject payloads exceeding typical UDP MTU (1472 bytes for IPv4, 1452 for IPv6) to prevent fragmentation issues

---

## R3: Response Pattern Matching

**Decision**: Optional expected response pattern as hex/base64-encoded string with exact byte-match validation

**Rationale**:
- Enables protocol-specific validation (e.g., verify DNS response contains answer section, SNMP response has expected OID)
- Simple exact match is sufficient for MVP (P2 user story); regex/partial matching can be added later if needed
- Keeps implementation aligned with existing validation patterns (e.g., `HttpGet` has `expectedStatus`, `RegistryRead` has `expectedValue`)

**Alternatives Considered**:
1. **Regex pattern matching on response bytes**: Overkill for most UDP protocols; network engineers prefer hex comparison over regex on binary data
2. **Protocol-specific parsers** (DNS parser, SNMP parser): Out of scope for MVP; users can validate structure externally and use exact match for critical bytes
3. **Partial/contains matching**: Useful but adds complexity; can be added as `expectedResponseContains` parameter in future iteration

**Implementation Notes**:
- If `expectedResponse` is provided:
  - Decode using same logic as payload (hex/base64/UTF-8 heuristic)
  - Compare received bytes using `SequenceEqual()`
  - On mismatch: Mark as "Fail" with `ErrorCategory.Validation`, include both expected and actual bytes (truncated to first 64 bytes) in technical details
- If `expectedResponse` is null/empty: Any response marks test as "Pass"

---

## R4: Timeout Handling Strategy

**Decision**: Use `Task.WhenAny` pattern with `ReceiveAsync` and `Task.Delay`, default timeout 5000ms

**Rationale**:
- Async-friendly pattern consistent with other test implementations
- Allows clean cancellation via `CancellationTokenSource.CancelAfter(timeout)`
- Default 5 seconds balances responsiveness (UDP is typically fast) with tolerance for high-latency networks
- Lower than TCP default (30s) because UDP has no handshake — response is immediate or never
- Matches industry standard tools (e.g., `nmap -sU` uses 1-10s depending on timing template)

**Alternatives Considered**:
1. **Synchronous `Receive()` with `ReceiveTimeout` property**: Blocks thread, doesn't support `CancellationToken`, inconsistent with async test infrastructure
2. **Exponential backoff retries**: Not applicable to UDP discovery; either service responds or doesn't (stateless protocol)

**Implementation Notes**:
```csharp
using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
cts.CancelAfter(timeout);

var receiveTask = udpClient.ReceiveAsync(cts.Token);
var timeoutTask = Task.Delay(timeout, cts.Token);

var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
if (completedTask == timeoutTask)
{
    return TestResult.Fail(ErrorCategory.Timeout, "No response within timeout");
}
```

---

## R5: ICMP Error Handling

**Decision**: Treat ICMP "Port Unreachable" as "Fail" with `ErrorCategory.Network`, log ICMP type in technical details

**Rationale**:
- ICMP Destination Unreachable (Type 3, Code 3 = Port Unreachable) indicates the host is reachable but the port has no listener
- Distinguishing "timeout" (no response) from "port unreachable" (active rejection) provides valuable diagnostic information
- Consistent with `TcpPortOpenTest` which distinguishes "connection refused" from "timeout"

**Platform Differences**:
- **Windows**: `SocketException` with `SocketError.ConnectionReset` thrown on subsequent `ReceiveAsync` call after ICMP received
- **Linux**: Requires `IP_RECVERR` socket option enabled; otherwise ICMP silently consumed by kernel
- **macOS**: Similar to Linux; ICMP errors may not surface to application layer

**Implementation Notes**:
- Catch `SocketException` during `ReceiveAsync`
- Inspect `SocketError` property:
  - `ConnectionReset`: ICMP Port Unreachable → `ErrorCategory.Network` + "Port unreachable (ICMP response)"
  - `TimedOut`: Socket timeout → `ErrorCategory.Timeout` + "No response"
  - `HostUnreachable`: ICMP Host Unreachable → `ErrorCategory.Network` + "Host unreachable"
- Cross-platform note: Document in technical details that ICMP error detection is platform-dependent

---

## R6: IPv6 Support Strategy

**Decision**: Enable dual-stack mode via `UdpClient.Client.DualMode = true` when IPv6 endpoint detected

**Rationale**:
- Modern networks increasingly use IPv6; enterprise environments often have dual-stack configurations
- .NET's `IPAddress.TryParse()` automatically detects IPv4 vs IPv6 format
- DNS resolution via `Dns.GetHostAddressesAsync()` returns both IPv4 and IPv6 addresses; prioritize based on address family preference
- Dual-mode sockets can communicate with both IPv4 and IPv6 endpoints from a single socket (Windows/Linux support)

**Implementation Notes**:
- Parse `host` parameter: if `IPAddress.TryParse()` succeeds, use directly; else DNS resolve
- For DNS resolution:
  - Filter results by `AddressFamily.InterNetwork` (IPv4) and `AddressFamily.InterNetworkV6` (IPv6)
  - Prefer IPv4 for broadest compatibility (default behavior)
  - If user wants IPv6 priority, they can specify literal IPv6 address (e.g., `2001:4860:4860::8888`)
- Constructor selection:
  - IPv4 endpoint: `new UdpClient(AddressFamily.InterNetwork)`
  - IPv6 endpoint: `new UdpClient(AddressFamily.InterNetworkV6)` + set `DualMode = true`

---

## R7: Test Evidence Structure

**Decision**: Capture structured evidence object with boolean flags and metrics, serialize to JSON in `ResponseData`

**Rationale**:
- Consistent with existing test patterns (e.g., `TcpPortOpenTest` captures `Connected`, `ConnectionTimeMs`, `RemoteEndpoint`)
- Structured data enables future UI enhancements (charts, filtering, aggregation)
- JSON serialization allows backward-compatible schema evolution

**Evidence Schema**:
```csharp
public class UdpPortTestEvidence
{
    public bool Responded { get; set; }                    // Did we receive a response?
    public int? RoundTripTimeMs { get; set; }              // RTT in milliseconds (null if no response)
    public string RemoteEndpoint { get; set; }             // "192.168.1.1:53" or "[2001::1]:53"
    public int PayloadSentBytes { get; set; }              // Size of sent datagram
    public int? PayloadReceivedBytes { get; set; }         // Size of received datagram (null if no response)
    public string ResponseDataPreview { get; set; }        // First 64 bytes as hex (for debugging)
}
```

**Human Summary Format**:
- Success: `"UDP port 53 on 8.8.8.8 responded in 12 ms (sent 32 bytes, received 64 bytes)"`
- Timeout: `"UDP port 161 on 192.168.1.1 did not respond within 5000 ms"`
- Port Unreachable: `"UDP port 9999 on example.com is unreachable (ICMP Port Unreachable)"`

---

## R8: Default Profile Sample Test

**Decision**: Add DNS query to Google Public DNS (8.8.8.8:53) as sample UDP test in `default-profile.json`

**Rationale**:
- DNS is universally understood by IT administrators
- Google Public DNS (8.8.8.8) is globally reachable and reliable (99.99% uptime SLA)
- Demonstrates real-world UDP protocol validation (not just "send random bytes")
- Provides working example of hex-encoded payload

**Sample Test Configuration**:
```json
{
  "id": "test-udp-001",
  "name": "DNS Server Reachability",
  "type": "UdpPortOpen",
  "description": "Verify DNS query to Google Public DNS succeeds",
  "parameters": {
    "host": "8.8.8.8",
    "port": 53,
    "timeout": 5000,
    "payload": "00010100000100000000000003777777076578616d706c6503636f6d0000010001",
    "expectedResponse": null
  },
  "fieldPolicy": {
    "host": "Editable",
    "port": "Editable",
    "timeout": "Editable",
    "payload": "Hidden",
    "expectedResponse": "Hidden"
  }
}
```

**Payload Explanation**: Hex-encoded DNS query for `www.example.com` (standard A record query, transaction ID 0x0001)

---

## R9: Icon and Color Mapping

**Decision**: Use `ConnectedNetwork24` icon (Fluent UI) and network-category color (blue/teal accent)

**Rationale**:
- Visual consistency with `TcpPortOpen` (also uses network connectivity icon)
- Fluent UI `ConnectedNetwork24` symbolizes active network communication (appropriate for UDP datagram exchange)
- Color: Reuse existing network test color (matches `Ping`, `HttpGet`, `DnsResolve` category)

**Converter Updates**:
- `TestTypeToIconConverter.cs`: Add case `"UdpPortOpen" => Symbol.ConnectedNetwork24`
- `TestTypeToColorConverter.cs`: Add case `"UdpPortOpen" => NetworkTestColor` (reference existing color resource)

---

## Summary Table

| Research Area | Decision | Key Trade-off |
|---------------|----------|---------------|
| **UDP API** | `UdpClient` class | Built-in (no deps) vs. raw Socket control |
| **Payload Encoding** | Hex/Base64 auto-detect | Flexibility vs. explicit configuration |
| **Response Validation** | Exact byte match | Simple implementation vs. regex/protocol parsing |
| **Timeout Strategy** | `Task.WhenAny` + 5s default | Async-friendly vs. blocking with shorter timeout |
| **ICMP Handling** | Catch `SocketException` | Platform-dependent error detection |
| **IPv6 Support** | Dual-stack mode | Broad compatibility vs. IPv6-first strategy |
| **Evidence Schema** | Structured JSON object | Schema evolution vs. flat string |
| **Default Sample** | DNS query to 8.8.8.8:53 | Real-world demo vs. simple "hello world" |

---

**Research Complete**: All technical unknowns resolved. Ready for Phase 1 (data model and contracts).
