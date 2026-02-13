# Plan: Fix UDP Port Test Code Review Issues

## Overview

This plan addresses 5 issues identified in the code review for the UdpPortOpen test type implementation. The issues range from medium severity (task leaks, spec contradictions) to low severity (quality improvements).

## Summary

| Issue | Severity | File | Description |
|-------|----------|------|-------------|
| 1 | Medium | UdpPortOpenTest.cs | ReceiveAsync task leak on timeout |
| 2 | Medium | UdpPortOpenTest.cs | Prefix match contradicts spec |
| 3 | Low | default-profile.json | Empty payload wont elicit DNS response |
| 4 | Low | UdpPortOpenTest.cs | ICMP/generic errors missing context |
| 5 | Low | default-profile.json | Accidental test dependency |

---

## Issue 1: UdpClient.ReceiveAsync() Task Leak (Medium)

### Problem

Location: [`ExecuteUdpTestAsync()`](src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs:286) lines 302-318

```csharp
var receiveTask = udpClient.ReceiveAsync();  // No CancellationToken!
var delayTask = Task.Delay(timeout, linkedCts.Token);
var completedTask = await Task.WhenAny(receiveTask, delayTask);
```

When the delay wins, `receiveTask` continues running in the background with the disposed UdpClient (from `using`). This can cause `ObjectDisposedException` on the threadpool.

### Solution

Use the `ReceiveAsync(CancellationToken)` overload available in .NET 7+. This eliminates the `Task.WhenAny` pattern entirely - if the token fires, `ReceiveAsync` throws `OperationCanceledException`, which is already handled.

### Implementation

Replace the try block in `ExecuteUdpTestAsync()` (lines 302-329):

**Before:**
```csharp
try
{
    var receiveTask = udpClient.ReceiveAsync();
    var delayTask = Task.Delay(timeout, linkedCts.Token);

    var completedTask = await Task.WhenAny(receiveTask, delayTask);

    if (completedTask == receiveTask && receiveTask.IsCompletedSuccessfully)
    {
        var result = await receiveTask;
        return (true, result.Buffer, result.RemoteEndPoint);
    }
    else
    {
        // Timeout occurred
        return (false, null, null);
    }
}
catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
{
    // ICMP Port Unreachable received - rethrow for specific handling
    throw;
}
catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
{
    // Timeout occurred
    return (false, null, null);
}
```

**After:**
```csharp
try
{
    var result = await udpClient.ReceiveAsync(linkedCts.Token);
    return (true, result.Buffer, result.RemoteEndPoint);
}
catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
{
    // ICMP Port Unreachable received - rethrow for specific handling
    throw;
}
catch (OperationCanceledException)
{
    // Timeout or user cancellation occurred
    return (false, null, null);
}
```

### Notes

- The linkedCts already combines user cancellation and timeout
- OperationCanceledException handles both timeout and user cancel scenarios
- The existing `OperationCanceledException` handler at line 108 already differentiates user cancellation

---

## Issue 2: ValidateResponse Uses Prefix Match (Medium)

### Problem

Location: [`ValidateResponse()`](src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs:432) lines 444-445

```csharp
// Check if the response starts with the expected pattern
return received.AsSpan(0, expected.Length).SequenceEqual(expected);
```

The spec (FR-008), research.md (R3), and data-model.md all specify exact byte match via `SequenceEqual()`. The current implementation does a prefix match - a 2-byte expected pattern would match any response starting with those bytes.

### Solution

Change to exact match to align with specification.

### Implementation

**Before:**
```csharp
if (received == null || received.Length < expected.Length)
{
    return false;
}

// Check if the response starts with the expected pattern
return received.AsSpan(0, expected.Length).SequenceEqual(expected);
```

**After:**
```csharp
if (received == null || received.Length != expected.Length)
{
    return false;
}

// Check if the response exactly matches the expected pattern
return received.AsSpan().SequenceEqual(expected);
```

### Alternative Considered

The review suggests prefix matching may be more practical for real-world use (e.g., checking DNS response header). However, the spec explicitly requires exact match. If prefix matching is desired, the spec should be updated accordingly.

---

## Issue 3: Default Profile Empty Payload (Low)

### Problem

Location: [`default-profile.json`](src/ReqChecker.App/Profiles/default-profile.json:154) lines 154-155

```json
"payload": "",
"expectedResponse": "",
```

An empty string payload sends a null byte (0x00) to DNS port 53, which wont produce a valid DNS response - the test will timeout.

### Solution

Use the hex-encoded DNS query from the spec to make the default profile demonstrate a working UDP test.

### Implementation

**Before:**
```json
"payload": "",
"expectedResponse": "",
```

**After:**
```json
"payload": "00010100000100000000000003777777076578616d706c6503636f6d0000010001",
"expectedResponse": "",
```

### Notes

- The hex payload is a DNS query for www.example.com
- expectedResponse remains empty since DNS responses vary
- The Hidden field policy hides these from users, so complexity is not a UX concern

---

## Issue 4: ICMP/Generic Error Results Missing Context (Low)

### Problem

Location: [`BuildIcmpErrorResult()`](src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs:604) and [`BuildErrorResult()`](src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs:623)

```csharp
result.HumanSummary = "UDP port is unreachable (ICMP Port Unreachable received)";
```

Compare with `BuildTimeoutResult()` which includes specific context:
```csharp
result.HumanSummary = $"UDP port {parameters.Port} on {parameters.Host} did not respond...";
```

The ICMP handler doesnt have access to `UdpTestParameters` since it catches at the `ExecuteAsync` level.

### Solution

Catch the ICMP `SocketException` inside `ExecuteUdpTestAsync` and return a structured result with endpoint info instead of rethrowing.

### Implementation

1. Modify `ExecuteUdpTestAsync` to return a structured result that includes error type
2. Update `BuildIcmpErrorResult` to accept parameters

**Step 1: Update ExecuteUdpTestAsync return handling:**

```csharp
catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
{
    // ICMP Port Unreachable received - return special result instead of throwing
    return (false, null, null, isIcmpError: true);
}
```

**Step 2: Update method signature and caller:**

The simpler approach is to pass parameters to the build methods:

Update [`BuildIcmpErrorResult()`](src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs:604):

```csharp
private TestResult BuildIcmpErrorResult(TestResult result, SocketException ex, TimeSpan elapsed, UdpTestParameters parameters)
{
    var evidence = new UdpPortTestEvidence
    {
        Responded = false,
        RoundTripTimeMs = null,
        RemoteEndpoint = $"{parameters.Host}:{parameters.Port}",
        PayloadSentBytes = parameters.Payload.Length,
        PayloadReceivedBytes = null,
        ResponseDataPreview = null
    };

    result.Status = TestStatus.Fail;
    result.Error = new TestError
    {
        Category = ErrorCategory.Network,
        Message = "ICMP Port Unreachable received",
        StackTrace = ex.StackTrace
    };
    result.HumanSummary = $"UDP port {parameters.Port} on {parameters.Host} is unreachable (ICMP Port Unreachable received)";
    result.TechnicalDetails = $"ICMP Destination Unreachable (Port Unreachable) received for {parameters.Host}:{parameters.Port}\n" +
                              $"Host is reachable but port has no listener";
    result.Evidence = new TestEvidence
    {
        ResponseData = JsonSerializer.Serialize(evidence),
        Timing = new TimingBreakdown
        {
            TotalMs = (int)elapsed.TotalMilliseconds
        }
    };

    return result;
}
```

**Step 3: Update the catch block in ExecuteAsync to pass parameters:**

The challenge is that `parameters` is defined inside the try block. Need to restructure slightly:

```csharp
UdpTestParameters? parameters = null;
try
{
    parameters = ExtractParameters(testDefinition);
    // ... rest of code
}
catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
{
    stopwatch.Stop();
    result.EndTime = DateTime.UtcNow;
    result.Duration = stopwatch.Elapsed;
    result = BuildIcmpErrorResult(result, ex, stopwatch.Elapsed, parameters!);
}
```

Also update [`BuildErrorResult()`](src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs:623) similarly.

---

## Issue 5: Accidental Test Dependency (Low)

### Problem

Location: [`default-profile.json`](src/ReqChecker.App/Profiles/default-profile.json:169) line 169

```json
"dependsOn": ["test-001"]
```

The UDP test depending on Ping means if Ping fails (ICMP blocked), UDP is automatically skipped - even though UDP to 8.8.8.8:53 might work fine via a different protocol.

### Solution

Remove the dependency.

### Implementation

**Before:**
```json
"dependsOn": ["test-001"]
```

**After:**
```json
"dependsOn": []
```

---

## Implementation Order

1. **Issue 1** - Fix ReceiveAsync leak (most critical - resource leak)
2. **Issue 2** - Fix ValidateResponse to exact match (spec compliance)
3. **Issue 4** - Add context to error results (requires refactoring)
4. **Issue 5** - Remove dependency (simple JSON edit)
5. **Issue 3** - Update payload (simple JSON edit)

## Testing

After implementation:
1. Run existing unit tests: `dotnet test tests/ReqChecker.Infrastructure.Tests`
2. Verify UDP test works with the new DNS payload
3. Verify timeout handling doesnt leak resources
4. Verify error messages include host/port context

## Files to Modify

| File | Changes |
|------|---------|
| `src/ReqChecker.Infrastructure/Tests/UdpPortOpenTest.cs` | Issues 1, 2, 4 |
| `src/ReqChecker.App/Profiles/default-profile.json` | Issues 3, 5 |
