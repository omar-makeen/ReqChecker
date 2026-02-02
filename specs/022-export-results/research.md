# Research: Export Test Results

## PDF Library Selection

### Decision: QuestPDF

**Rationale**: QuestPDF is a modern, fluent C# library for PDF generation that offers:
- Clean, readable fluent API
- Excellent performance for document generation
- Active development and community support
- MIT license (community version) suitable for commercial use
- Native .NET support without external dependencies
- Built-in layout engine for complex documents

### Alternatives Considered

| Library | Pros | Cons | Decision |
|---------|------|------|----------|
| **QuestPDF** | Fluent API, modern, fast, .NET native | Community license limits, newer library | ✅ Selected |
| **iTextSharp** | Mature, full-featured | AGPL license requires open-source or commercial license | ❌ License concern |
| **PDFsharp** | Free, mature | Older API style, limited layout features | ❌ Poor layout support |
| **Syncfusion PDF** | Enterprise features | Commercial license required | ❌ Cost |
| **IronPDF** | HTML to PDF conversion | Commercial license, heavy dependency | ❌ Cost & complexity |

### QuestPDF License Note
QuestPDF Community License is free for companies with annual gross revenue less than $1M USD. For larger organizations, a Professional or Enterprise license is required. For this internal tool, the Community license is appropriate.

## Existing Export Pattern Analysis

### Current Architecture
```
IExporter (interface)
├── JsonExporter (System.Text.Json)
└── CsvExporter (CsvHelper)
```

The existing pattern:
1. All exporters implement `IExporter.ExportAsync(RunReport, filePath, maskCredentials)`
2. Exporters are registered in DI and injected into `ResultsViewModel`
3. ViewModel exposes `RelayCommand` for each export type
4. UI buttons trigger commands, file dialog handled in ViewModel

### Integration Approach
Add `PdfExporter` following the exact same pattern:
1. Implement `IExporter` interface
2. Register in DI alongside existing exporters
3. Inject into `ResultsViewModel`
4. Add `ExportToPdfCommand` matching existing pattern

## PDF Layout Design

### Page Structure
```
┌─────────────────────────────────────────────────┐
│  [Logo]  ReqChecker Test Results Report         │ Header
│          Profile: {name}  |  Date: {date}       │
├─────────────────────────────────────────────────┤
│  Summary                                        │
│  ┌─────┬─────┬─────┬─────┐                     │
│  │Total│Pass │Fail │Skip │  Pass Rate: XX%     │
│  │ 50  │ 45  │  3  │  2  │                     │
│  └─────┴─────┴─────┴─────┘                     │
├─────────────────────────────────────────────────┤
│  Machine Information                            │
│  Hostname: xxx  |  OS: xxx  |  User: xxx       │
├─────────────────────────────────────────────────┤
│  Test Results                                   │
│  ┌────────────────┬────────┬──────────┬──────┐ │
│  │ Test Name      │ Status │ Duration │ Type │ │
│  ├────────────────┼────────┼──────────┼──────┤ │
│  │ Test 1         │ ✓ Pass │ 125ms    │ HTTP │ │
│  │ Test 2         │ ✗ Fail │ 3.2s     │ DNS  │ │
│  │   Error: Connection timeout...              │ │
│  └────────────────┴────────┴──────────┴──────┘ │
├─────────────────────────────────────────────────┤
│  Page 1 of N  |  Generated: {timestamp}         │ Footer
└─────────────────────────────────────────────────┘
```

### Design Decisions
- **A4 page size**: Standard for reports, prints well
- **Color coding**: Green for pass, red for fail, amber for skip
- **Error details inline**: Show error message directly under failed tests
- **Page numbers**: For multi-page reports
- **Generation timestamp**: Audit trail

## Logging Strategy

### Log Events for FR-020
```csharp
// On export start
Log.Information("Export started: {Format}, {TestCount} tests", format, count);

// On success
Log.Information("Export completed: {Format}, {FilePath}, {FileSize} bytes, {Duration}ms",
    format, path, size, elapsed);

// On failure
Log.Error(ex, "Export failed: {Format}, {FilePath}", format, path);
```

### Structured Fields
- `Format`: "PDF", "CSV", or "JSON"
- `TestCount`: Number of tests in report
- `FilePath`: Target file path (may need sanitization for logs)
- `FileSize`: Output file size in bytes
- `Duration`: Export time in milliseconds
- `Outcome`: "Success" or "Failed"

## Branding Assets

### Requirements
- ReqChecker logo PNG at 150x50px (approximate, for PDF header)
- Should work on white/light backgrounds
- Must be embedded in assembly (not external file)

### Implementation
1. Add `reqchecker-logo.png` to `ReqChecker.App/Resources/Images/`
2. Mark as EmbeddedResource in .csproj
3. Load via `Assembly.GetManifestResourceStream()` in PdfExporter
