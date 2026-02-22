# Research: Traceroute Test Type

**Feature Branch**: `050-traceroute-test`
**Date**: 2026-02-22

## R1: ICMP Traceroute via System.Net.NetworkInformation.Ping

**Decision**: Use built-in `Ping.SendPingAsync()` with `PingOptions { Ttl = n }` to implement traceroute — the same API already used by PingTest. No new packages required.

**Rationale**: .NET's `Ping` class supports setting TTL via `PingOptions.Ttl`. When a packet with TTL=N reaches the Nth router, that router decrements TTL to 0, drops the packet, and returns an ICMP "TTL Expired" response. The `PingReply` object contains:
- `Reply.Status == IPStatus.TtlExpired` — intermediate hop responded
- `Reply.Status == IPStatus.Success` — target host reached
- `Reply.Address` — IP address of the responding node
- `Reply.RoundtripTime` — round-trip time in milliseconds

Incrementing TTL from 1 to `maxHops` and collecting each reply implements standard traceroute behavior.

**Alternatives considered**:
- Raw ICMP sockets (`Socket` with `SocketType.Raw`) — requires elevated privileges on Windows and is unnecessary when `Ping` already supports TTL manipulation.
- `Process.Start("tracert")` — fragile, platform-specific output parsing, no structured data. Inconsistent with all other test implementations.

## R2: DNS Resolution Before Tracing

**Decision**: Use `Dns.GetHostAddressesAsync()` to resolve the hostname before beginning the trace loop.

**Rationale**: Traceroute needs to compare each hop's reply address against the target IP to detect when the destination is reached (FR-011). The resolved IP also appears in evidence (FR-006). Using `Dns.GetHostAddressesAsync()` handles both IPv4 and IPv6 resolution. The first returned address is used.

**Alternatives considered**:
- Resolve inline during the first hop — complicates the loop logic and makes DNS errors harder to distinguish from network errors (FR-009 requires distinct DNS failure messages).

## R3: Detecting Target Reached vs. TTL Expired

**Decision**: Compare `reply.Address` against the resolved target IP. When `reply.Status == IPStatus.Success` or the reply address matches the target, the trace stops and `reachedTarget` is set to true.

**Rationale**: The standard traceroute termination condition. `IPStatus.TtlExpired` means an intermediate hop responded. `IPStatus.Success` means the target itself responded. Checking both status and address covers edge cases where some targets respond with TTL expired instead of echo reply.

## R4: Handling Timed-Out Hops

**Decision**: When `reply.Status == IPStatus.TimedOut`, record the hop with address `*` and `roundtripMs` as null. Continue to the next hop.

**Rationale**: FR-010 requires tracing through timed-out hops rather than stopping. This matches standard `tracert` behavior where `*` indicates no response.

## R5: Evidence Serialization

**Decision**: Use `Dictionary<string, object>` with explicit camelCase keys, serialized via `JsonSerializer.Serialize()`. The hop list is a `List<Dictionary<string, object>>` nested inside the evidence dictionary.

**Rationale**: This is the pattern used by PingTest (which stores `pingResults` as a nested list) and most other tests. Consistent with project convention.

## R6: Conditional Build Integration

**Decision**: Add `Traceroute` to `TestManifest.props` following the exact same two-entry pattern (KnownTestType + conditional Compile ItemGroup).

**Rationale**: All test types follow this pattern. No deviation needed.
