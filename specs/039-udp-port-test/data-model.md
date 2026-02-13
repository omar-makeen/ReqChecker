# Data Model: UDP Port Reachability Test

**Feature**: `039-udp-port-test`
**Date**: 2026-02-13

## Overview

This document defines the data structures for the `UdpPortOpen` test type, including test parameters (stored in profile JSON), evidence output (captured during execution), and validation rules.

---

## Test Parameters

### Schema

Parameters are stored in the `TestDefinition.Parameters` JSON object within profile files.

| Parameter | Type | Required | Default | Validation | Description |
|-----------|------|----------|---------|------------|-------------|
| `host` | string | Yes | — | Non-empty, valid hostname or IP | Target host (hostname, IPv4, or IPv6 address) |
| `port` | integer | Yes | — | 1-65535 | Target UDP port number |
| `timeout` | integer | No | 5000 | > 0 | Timeout in milliseconds |
| `payload` | string | No | `"00"` | Valid hex/base64 | Hex or base64-encoded datagram payload |
| `expectedResponse` | string | No | null | Valid hex/base64 or null | Expected response pattern (hex/base64) |
| `encoding` | string | No | `"auto"` | `"auto"`, `"hex"`, `"base64"`, `"utf8"` | Payload encoding hint (auto-detect if not specified) |

### Example: Minimal Configuration

```json
{
  "id": "test-udp-minimal",
  "name": "NTP Server Check",
  "type": "UdpPortOpen",
  "parameters": {
    "host": "time.windows.com",
    "port": 123
  }
}
```

**Behavior**: Sends single null byte (`0x00`) to `time.windows.com:123`, expects any response within 5 seconds.

---

### Example: DNS Query with Custom Payload

```json
{
  "id": "test-udp-dns",
  "name": "DNS Query for www.google.com",
  "type": "UdpPortOpen",
  "parameters": {
    "host": "8.8.8.8",
    "port": 53,
    "timeout": 3000,
    "payload": "AAEBAAABAAAAAAAAA3d3dwZnb29nbGUDY29tAAABAAE=",
    "encoding": "base64"
  }
}
```

**Behavior**: Sends base64-decoded DNS query (A record for `www.google.com`), expects any response within 3 seconds.

---

### Example: SNMP GET with Response Validation

```json
{
  "id": "test-udp-snmp",
  "name": "SNMP System Description",
  "type": "UdpPortOpen",
  "parameters": {
    "host": "192.168.1.1",
    "port": 161,
    "timeout": 10000,
    "payload": "302902010004067075626c6963a01c02047b5a8c1e020100020100300e300c06082b060102010101000500",
    "expectedResponse": "3082",
    "encoding": "hex"
  }
}
```

**Behavior**: Sends SNMP v2c GET request for sysDescr OID, expects response starting with `0x30 0x82` (SEQUENCE tag for SNMP response) within 10 seconds.

---

## Field-Level Policies

All parameters support the standard policy types:

| Policy | Behavior | Use Case |
|--------|----------|----------|
| `Locked` | Read-only with lock icon + tooltip | Company-managed endpoints (e.g., internal DNS server) |
| `Editable` | Normal input field | User-customizable (e.g., timeout adjustment) |
| `Hidden` | Not rendered in UI | Internal/advanced parameters (e.g., protocol payloads) |
| `PromptAtRun` | Dialog prompt before execution | Dynamic values (e.g., test environment hostname) |

### Example: Locked Internal NTP Server

```json
{
  "id": "test-udp-ntp",
  "name": "Corporate NTP Server",
  "type": "UdpPortOpen",
  "parameters": {
    "host": "ntp.corp.internal",
    "port": 123,
    "timeout": 5000,
    "payload": "1B0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"
  },
  "fieldPolicy": {
    "host": "Locked",
    "port": "Locked",
    "timeout": "Editable",
    "payload": "Hidden"
  }
}
```

---

## Evidence Output

### Schema

Evidence is captured during test execution and stored in `TestResult.ResponseData` as serialized JSON.

```csharp
public class UdpPortTestEvidence
{
    /// <summary>
    /// Indicates whether a UDP response was received.
    /// </summary>
    public bool Responded { get; set; }

    /// <summary>
    /// Round-trip time in milliseconds (from send to receive).
    /// Null if no response received.
    /// </summary>
    public int? RoundTripTimeMs { get; set; }

    /// <summary>
    /// Remote endpoint in "IP:Port" format (e.g., "8.8.8.8:53" or "[2001:4860:4860::8888]:53").
    /// </summary>
    public string RemoteEndpoint { get; set; }

    /// <summary>
    /// Size of the sent UDP datagram in bytes.
    /// </summary>
    public int PayloadSentBytes { get; set; }

    /// <summary>
    /// Size of the received UDP datagram in bytes.
    /// Null if no response received.
    /// </summary>
    public int? PayloadReceivedBytes { get; set; }

    /// <summary>
    /// First 64 bytes of response data as hex string (for debugging).
    /// Null if no response received.
    /// Example: "30820145020100040670..." (truncated DNS response)
    /// </summary>
    public string ResponseDataPreview { get; set; }
}
```

### Example: Successful Response

```json
{
  "responded": true,
  "roundTripTimeMs": 12,
  "remoteEndpoint": "8.8.8.8:53",
  "payloadSentBytes": 32,
  "payloadReceivedBytes": 64,
  "responseDataPreview": "3082014502010004067075626c6963a0363704047b5a8c1e020100020100..."
}
```

### Example: Timeout (No Response)

```json
{
  "responded": false,
  "roundTripTimeMs": null,
  "remoteEndpoint": "192.168.1.1:161",
  "payloadSentBytes": 48,
  "payloadReceivedBytes": null,
  "responseDataPreview": null
}
```

---

## Human Summary Messages

The `TestResult.HumanSummary` field provides user-friendly result descriptions:

| Scenario | Example Summary |
|----------|-----------------|
| **Success** | `"UDP port 53 on 8.8.8.8 responded in 12 ms (sent 32 bytes, received 64 bytes)"` |
| **Timeout** | `"UDP port 161 on 192.168.1.1 did not respond within 5000 ms"` |
| **Port Unreachable** | `"UDP port 9999 on example.com is unreachable (ICMP Port Unreachable received)"` |
| **Validation Failed** | `"UDP port 53 on 8.8.8.8 responded but data did not match expected pattern (received 64 bytes)"` |
| **DNS Resolution Failed** | `"Could not resolve hostname 'invalid.local' (DNS resolution failed)"` |

---

## Technical Details Messages

The `TestResult.TechnicalDetails` field provides diagnostic information:

| Scenario | Example Details |
|----------|-----------------|
| **Success** | `"Sent 32-byte datagram to 8.8.8.8:53\nReceived 64-byte response in 12 ms\nResponse (hex): 3082014502010004067075626c6963a0363704..."` |
| **Timeout** | `"Sent 48-byte datagram to 192.168.1.1:161\nNo response received within 5000 ms timeout\nPossible causes: service not running, firewall blocking, incorrect port"` |
| **Port Unreachable** | `"Sent 32-byte datagram to example.com:9999\nICMP Destination Unreachable (Port Unreachable) received\nHost is reachable but port has no listener"` |
| **Validation Failed** | `"Expected response pattern (32 bytes): 3082...\nActual response (64 bytes): 3001...\nMismatch at byte offset 1"` |

---

## Validation Rules

### Parameter Validation (Pre-Execution)

Performed by `FluentValidation` rules before test execution:

1. **host**: Non-null, non-empty string
2. **port**: Integer in range `[1, 65535]`
3. **timeout**: Positive integer `> 0`
4. **payload**:
   - If hex: Valid hexadecimal characters `[0-9A-Fa-f]`, even length
   - If base64: Valid base64 string (passes `Convert.FromBase64String()`)
   - If UTF-8: Any string (converted to UTF-8 bytes)
   - Max size: 1472 bytes (IPv4 MTU - IP/UDP headers) — warn if larger
5. **expectedResponse**: Same validation as `payload` (optional)

### Execution Validation

1. **DNS Resolution**: If `host` is a hostname, resolve via `Dns.GetHostAddressesAsync()`
   - Prefer IPv4 addresses for broad compatibility
   - If resolution fails: Mark as `Fail` with `ErrorCategory.Network`
2. **Socket Creation**: Create `UdpClient` with appropriate address family
   - IPv4: `new UdpClient(AddressFamily.InterNetwork)`
   - IPv6: `new UdpClient(AddressFamily.InterNetworkV6)` + set `DualMode = true`
3. **Timeout Enforcement**: Use `CancellationTokenSource.CancelAfter(timeout)`
4. **Response Validation** (if `expectedResponse` provided):
   - Decode expected pattern using same encoding as payload
   - Compare received bytes using `SequenceEqual()`
   - On mismatch: `ErrorCategory.Validation`

---

## State Transitions

UDP tests follow the standard `TestStatus` state machine:

```
[Pending] → [Pass]      # Response received (and validated if pattern provided)
         → [Fail]       # Timeout, ICMP error, validation mismatch, or DNS failure
         → [Skipped]    # Dependency failed, user cancelled, or admin elevation blocked
```

### Error Categories

| Category | Trigger Condition |
|----------|------------------|
| `Network` | DNS resolution failed, ICMP unreachable, socket error |
| `Timeout` | No response within configured timeout |
| `Validation` | Response received but did not match `expectedResponse` pattern |
| `Configuration` | Invalid port, malformed payload, encoding error |

---

## Integration Points

### Profile Loader

- `JsonProfileLoader` deserializes `parameters` into `JsonObject`
- `UdpPortOpenTest.ExecuteAsync()` extracts parameters using `.GetValue<T>()`
- Field policies loaded from `TestDefinition.FieldPolicy` dictionary

### Test Runner

- `SequentialTestRunner` invokes `UdpPortOpenTest.ExecuteAsync()`
- Timeout passed via `TestDefinition.Timeout` (overrides global default)
- Cancellation token propagated from UI "Cancel" button

### UI Converters

- `TestTypeToIconConverter`: Maps `"UdpPortOpen"` → `Symbol.ConnectedNetwork24`
- `TestTypeToColorConverter`: Maps `"UdpPortOpen"` → Network test color (blue/teal accent)

### Evidence Serialization

```csharp
var evidence = new UdpPortTestEvidence
{
    Responded = true,
    RoundTripTimeMs = 12,
    RemoteEndpoint = "8.8.8.8:53",
    PayloadSentBytes = 32,
    PayloadReceivedBytes = 64,
    ResponseDataPreview = "3082014502010004067075626c6963a036..."
};

var json = JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = false });
// Store in TestResult.ResponseData
```

---

## Summary

- **6 parameters**: `host`, `port`, `timeout`, `payload`, `expectedResponse`, `encoding`
- **6 evidence fields**: Captured metrics + response preview for diagnostics
- **4 error categories**: Network, Timeout, Validation, Configuration
- **Field-level policies**: Full support for Locked/Editable/Hidden/PromptAtRun
- **Cross-platform**: Works on Windows/Linux/macOS via .NET 8 `UdpClient`

**Data model complete** — ready for contract generation and quickstart guide.
