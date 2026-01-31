# Research: Display Test Result Details

**Feature**: 010-result-details | **Date**: 2026-01-31

## Research Questions Resolved

### 1. Why is expanded content empty?

**Decision**: Test implementations populate `Evidence` but not `HumanSummary`/`TechnicalDetails`

**Rationale**: Examined `HttpGetTest.cs` (lines 130-140) - creates `TestEvidence` with structured data but never sets the display-oriented string fields. All test implementations follow this pattern.

**Evidence from code**:
```csharp
// HttpGetTest.cs - sets Evidence but not summary fields
result.Evidence = new TestEvidence
{
    ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
    ResponseCode = statusCode,
    // ...
};
// HumanSummary and TechnicalDetails remain as empty strings (default)
```

**Alternatives considered**:
- Binding issue: Verified bindings are correct in ResultsView.xaml (lines 383-385)
- Model issue: TestResult.cs has proper properties defined

### 2. Best approach for generating display text?

**Decision**: Create an IValueConverter that generates formatted text at display time

**Rationale**:
- Keeps test implementations focused on execution, not presentation
- Allows UI-layer control over formatting
- No changes needed to 10+ test implementation files
- Follows existing pattern (CredentialMaskConverter is already used)

**Alternatives considered**:
- Modify test implementations: Too many files (10+), couples UI concerns to tests
- Post-process in runner: Adds display logic to execution layer

### 3. How to style sections for premium look?

**Decision**: Use elevated backgrounds with colored left borders matching existing design system

**Rationale**: App already uses this pattern for:
- Status indicator bar in ExpanderCard header (4px colored bar)
- CardInteractive style with border effects
- StatusBadge styles with glow effects

**Design tokens to use**:
- `BackgroundElevated` - for section backgrounds
- `AccentGradient` - for technical details left border
- `StatusFail` (#EF4444) - for error section left border
- `BorderSubtle` - for section separation

### 4. What data is available in TestEvidence?

**Decision**: Use structured Evidence data to generate readable summaries

**Available fields**:
| Field | Type | Content |
|-------|------|---------|
| ResponseData | string | JSON-serialized evidence dictionary |
| ResponseCode | int? | HTTP status code |
| ResponseHeaders | Dictionary<string,string>? | HTTP headers |
| FileContent | string? | File content sample |
| ProcessList | string[]? | Running processes |
| RegistryValue | string? | Registry value |
| Timing | TimingBreakdown? | Detailed timing (TotalMs, ConnectMs, etc.) |

### 5. How to handle missing/null data?

**Decision**: Hide sections when data is null/empty; show fallback message if all sections empty

**Implementation**:
- Existing `NullToVisibilityConverter` handles section visibility
- Add fallback TextBlock that shows when Summary, TechnicalDetails, AND Error are all null/empty

## Technical Findings

### Existing Infrastructure
- `CredentialMaskConverter` - already masks passwords in technical output
- `NullToVisibilityConverter` - hides empty sections
- `TestStatusBadge` - status indicator styling
- Premium card styles in `Controls.xaml`

### Test Type Summary Patterns
| Test Type | Success Summary | Failure Summary |
|-----------|----------------|-----------------|
| HttpGet | "HTTP GET {url} returned {statusCode}" | "HTTP GET {url} failed: {error}" |
| Ping | "Ping to {host} succeeded ({latency}ms)" | "Ping to {host} failed: {error}" |
| FileExists | "File {path} exists" | "File {path} not found" |
| FileRead | "Successfully read {path}" | "Failed to read {path}: {error}" |
| FtpRead | "FTP read from {server}{path} successful" | "FTP connection failed: {error}" |
| ProcessList | "Process check completed" | "Process check failed: {error}" |

### Technical Details Format
```
Status: {statusCode} {statusText}
Response Time: {responseTimeMs}ms
Content Type: {contentType}
Content Length: {contentLength} bytes

[Headers]
{header1}: {value1}
{header2}: {value2}
...

[Response Body]
{responseBody or "(empty)"}
```

## Dependencies Identified

None - all required infrastructure exists in the codebase.

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Performance with large response bodies | Low | Already truncated to 4KB in tests |
| Credential leakage in technical details | Low | CredentialMaskConverter already applied |
| Theme compatibility | Low | Use existing theme tokens |
