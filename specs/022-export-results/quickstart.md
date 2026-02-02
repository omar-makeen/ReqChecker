# Quickstart: Export Test Results Implementation

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- Existing ReqChecker codebase with JSON/CSV export working

## Step 1: Add QuestPDF Package

```bash
cd src/ReqChecker.Infrastructure
dotnet add package QuestPDF
```

## Step 2: Create PdfExporter

Create `src/ReqChecker.Infrastructure/Export/PdfExporter.cs`:

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Utilities;
using Serilog;

namespace ReqChecker.Infrastructure.Export;

public class PdfExporter : IExporter
{
    public string FileExtension => ".pdf";

    public async Task ExportAsync(RunReport report, string filePath, bool maskCredentials = true)
    {
        // Validation
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var reportToExport = maskCredentials
            ? CredentialMasker.MaskCredentials(report)
            : report;

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // Configure QuestPDF license (Community)
        QuestPDF.Settings.License = LicenseType.Community;

        // Generate PDF
        var document = CreateDocument(reportToExport);
        document.GeneratePdf(filePath);

        await Task.CompletedTask; // Async signature for interface compatibility
    }

    private Document CreateDocument(RunReport report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, report));
                page.Content().Element(c => ComposeContent(c, report));
                page.Footer().Element(ComposeFooter);
            });
        });
    }

    private void ComposeHeader(IContainer container, RunReport report)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("ReqChecker Test Results")
                    .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                col.Item().Text($"Profile: {report.ProfileName}")
                    .FontSize(12).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"Run Date: {report.StartTime:yyyy-MM-dd HH:mm:ss}")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container, RunReport report)
    {
        container.Column(col =>
        {
            // Summary section
            col.Item().PaddingVertical(10).Element(c => ComposeSummary(c, report.Summary));

            // Machine info section
            col.Item().PaddingVertical(10).Element(c => ComposeMachineInfo(c, report.MachineInfo));

            // Test results table
            col.Item().PaddingVertical(10).Element(c => ComposeTestResults(c, report.Results));
        });
    }

    private void ComposeSummary(IContainer container, RunSummary summary)
    {
        container.Column(col =>
        {
            col.Item().Text("Summary").FontSize(14).Bold();
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Total: {summary.TotalTests}");
                row.RelativeItem().Text($"Passed: {summary.Passed}").FontColor(Colors.Green.Darken1);
                row.RelativeItem().Text($"Failed: {summary.Failed}").FontColor(Colors.Red.Darken1);
                row.RelativeItem().Text($"Skipped: {summary.Skipped}").FontColor(Colors.Orange.Darken1);
                row.RelativeItem().Text($"Pass Rate: {summary.PassRate:F1}%");
            });
        });
    }

    private void ComposeMachineInfo(IContainer container, MachineInfo info)
    {
        container.Column(col =>
        {
            col.Item().Text("Machine Information").FontSize(14).Bold();
            col.Item().PaddingTop(5).Text($"Hostname: {info.Hostname} | OS: {info.OsVersion} | User: {info.UserName}");
        });
    }

    private void ComposeTestResults(IContainer container, List<TestResult> results)
    {
        container.Column(col =>
        {
            col.Item().Text("Test Results").FontSize(14).Bold();
            col.Item().PaddingTop(5).Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3); // Name
                    cols.RelativeColumn(1); // Status
                    cols.RelativeColumn(1); // Duration
                    cols.RelativeColumn(1); // Type
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Text("Test Name").Bold();
                    header.Cell().Text("Status").Bold();
                    header.Cell().Text("Duration").Bold();
                    header.Cell().Text("Type").Bold();
                });

                // Data rows
                foreach (var result in results)
                {
                    var statusColor = result.Status switch
                    {
                        TestStatus.Pass => Colors.Green.Darken1,
                        TestStatus.Fail => Colors.Red.Darken1,
                        _ => Colors.Orange.Darken1
                    };

                    table.Cell().Text(result.DisplayName);
                    table.Cell().Text(result.Status.ToString()).FontColor(statusColor);
                    table.Cell().Text($"{result.Duration.TotalMilliseconds:F0}ms");
                    table.Cell().Text(result.TestType);

                    // Show error details for failed tests
                    if (result.Status == TestStatus.Fail && result.Error != null)
                    {
                        table.Cell().ColumnSpan(4).PaddingLeft(20)
                            .Text($"Error: {result.Error.Message}")
                            .FontSize(9).FontColor(Colors.Red.Medium);
                    }
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
            row.RelativeItem().AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        });
    }
}
```

## Step 3: Register in DI

Update `src/ReqChecker.App/App.xaml.cs` service registration:

```csharp
services.AddSingleton<PdfExporter>();
```

## Step 4: Update ResultsViewModel

Add to `ResultsViewModel.cs`:

```csharp
private readonly PdfExporter _pdfExporter;

// In constructor, add parameter:
public ResultsViewModel(
    JsonExporter jsonExporter,
    CsvExporter csvExporter,
    PdfExporter pdfExporter,  // NEW
    IAppState appState,
    NavigationService? navigationService = null,
    DialogService? dialogService = null)
{
    _jsonExporter = jsonExporter;
    _csvExporter = csvExporter;
    _pdfExporter = pdfExporter;  // NEW
    // ...
}

// Add command:
[RelayCommand]
private async Task ExportToPdfAsync()
{
    if (DialogService == null || Report == null) return;

    var filePath = DialogService.SaveFileDialog(
        $"{Report.ProfileName}-{Report.StartTime:yyyy-MM-dd}.pdf",
        "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*");

    if (string.IsNullOrEmpty(filePath)) return;

    await ExportAsync(() => _pdfExporter.ExportAsync(Report, filePath), filePath);
}
```

## Step 5: Update ResultsView.xaml

Add PDF button after CSV button in header:

```xml
<Button Style="{StaticResource SecondaryButton}"
        Command="{Binding ExportToPdfCommand}"
        IsEnabled="{Binding CanExport}"
        TabIndex="4"
        Margin="8,0,0,0">
    <StackPanel Orientation="Horizontal">
        <ui:SymbolIcon Symbol="Document24" FontSize="16" Margin="0,0,8,0"/>
        <TextBlock Text="PDF"/>
    </StackPanel>
</Button>
```

## Step 6: Add Export Logging

Update the `ExportAsync` method in `ResultsViewModel`:

```csharp
private async Task ExportAsync(Func<Task> exportAction, string filePath)
{
    IsExporting = true;
    StatusMessage = null;
    IsStatusError = false;
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var format = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant();

    Log.Information("Export started: {Format}, {TestCount} tests",
        format, Report?.Results?.Count ?? 0);

    try
    {
        await exportAction();
        sw.Stop();

        var fileSize = new FileInfo(filePath).Length;
        Log.Information("Export completed: {Format}, {FilePath}, {FileSize} bytes, {Duration}ms",
            format, filePath, fileSize, sw.ElapsedMilliseconds);

        StatusMessage = $"Exported to {Path.GetFileName(filePath)}";
        IsStatusError = false;
    }
    catch (Exception ex)
    {
        sw.Stop();
        Log.Error(ex, "Export failed: {Format}, {FilePath}", format, filePath);
        // ... existing error handling
    }
    finally
    {
        IsExporting = false;
    }
}
```

## Build and Test

```bash
dotnet build src/ReqChecker.App
dotnet run --project src/ReqChecker.App
```

1. Run some tests to generate results
2. Navigate to Results Dashboard
3. Click PDF button
4. Verify PDF is generated with proper formatting
