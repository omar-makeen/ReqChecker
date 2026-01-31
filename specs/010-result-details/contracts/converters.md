# Converter Contracts

**Feature**: 010-result-details | **Date**: 2026-01-31

## TestResultSummaryConverter

Generates human-readable summary text from a TestResult object.

### Interface
```csharp
public class TestResultSummaryConverter : IValueConverter
```

### Input
- `value`: `TestResult` object

### Output
- `string`: Human-readable summary, or `null` if input is invalid

### Summary Generation Rules

| Status | Test Type | Output Pattern |
|--------|-----------|----------------|
| Pass | HttpGet | "HTTP GET to {url} returned {statusCode}" |
| Pass | Ping | "Ping to {host} succeeded ({latency}ms)" |
| Pass | FileExists | "File exists: {path}" |
| Pass | FileRead | "Successfully read file: {path}" |
| Pass | FileWrite | "Successfully wrote to: {path}" |
| Pass | DirectoryExists | "Directory exists: {path}" |
| Pass | FtpRead | "FTP read from {server} successful" |
| Pass | FtpWrite | "FTP write to {server} successful" |
| Pass | HttpPost | "HTTP POST to {url} returned {statusCode}" |
| Pass | ProcessList | "Process check completed" |
| Pass | RegistryRead | "Registry value retrieved: {key}" |
| Fail | Any | Error.Message or "Test failed" |
| Skipped | Any | Error.Message or "Test was skipped" |

### Examples

```csharp
// Input: Passed HTTP GET test
TestResult { Status = Pass, Evidence = { ResponseCode = 200, ResponseData = "{\"url\":\"https://api.example.com\"}" } }
// Output: "HTTP GET to https://api.example.com returned 200 OK"

// Input: Failed ping test
TestResult { Status = Fail, Error = { Message = "Host unreachable" } }
// Output: "Host unreachable"

// Input: Skipped test (no admin)
TestResult { Status = Skipped, Error = { Message = "Test requires administrator privileges" } }
// Output: "Test requires administrator privileges"
```

---

## TestResultDetailsConverter

Generates formatted technical details from a TestResult's Evidence.

### Interface
```csharp
public class TestResultDetailsConverter : IValueConverter
```

### Input
- `value`: `TestResult` object

### Output
- `string`: Formatted technical details, or `null` if no evidence

### Output Format

```
[General]
Duration: {duration}ms
Attempts: {attemptCount}

[Response]
Status: {statusCode} {statusText}
Response Time: {responseTimeMs}ms
Content-Type: {contentType}
Content-Length: {contentLength} bytes

[Headers]
{header1}: {value1}
{header2}: {value2}

[Body]
{responseBody}
```

### Section Visibility

| Section | Condition |
|---------|-----------|
| General | Always shown |
| Response | If Evidence.ResponseCode is set |
| Headers | If Evidence.ResponseHeaders is not empty |
| Body | If ResponseData contains "body" field |
| File Content | If Evidence.FileContent is not empty |
| Process List | If Evidence.ProcessList is not empty |
| Registry | If Evidence.RegistryValue is not empty |
| Timing | If Evidence.Timing is not null |

### Examples

```csharp
// Input: HTTP GET with headers and body
// Output:
// [General]
// Duration: 234ms
// Attempts: 1
//
// [Response]
// Status: 200 OK
// Response Time: 189ms
// Content-Type: application/json
// Content-Length: 1234 bytes
//
// [Body]
// {"data": "example"}

// Input: File read test
// Output:
// [General]
// Duration: 12ms
// Attempts: 1
//
// [File Content]
// First 1024 bytes of file...
```

---

## Credential Masking

Both converters MUST ensure credentials are masked before output. This is handled by:
1. The existing `CredentialMaskConverter` applied in XAML binding
2. Converters should NOT attempt additional masking to avoid double-masking

The binding chain is:
```
TestResult → TestResultDetailsConverter → CredentialMaskConverter → Display
```
