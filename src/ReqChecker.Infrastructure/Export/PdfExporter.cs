using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Utilities;
using System.Reflection;

namespace ReqChecker.Infrastructure.Export;

/// <summary>
/// Exports run reports to PDF format using QuestPDF.
/// </summary>
public class PdfExporter : IExporter
{
    /// <inheritdoc />
    public string FileExtension => ".pdf";

    /// <inheritdoc />
    public async Task ExportAsync(RunReport report, string filePath, bool maskCredentials = true)
    {
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
                col.Item().Row(logoRow =>
                {
                    // Try to load logo from embedded resource
                    var logoImage = LoadLogoImage();
                    if (logoImage != null)
                    {
                        logoRow.RelativeItem().MaxWidth(150).Image(logoImage);
                    }
                    else
                    {
                        // Fallback to text if logo not available
                        logoRow.RelativeItem().Text("ReqChecker")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    }
                });
                col.Item().Text($"Profile: {report.ProfileName}")
                    .FontSize(12).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"Run Date: {report.StartTime:yyyy-MM-dd HH:mm:ss}")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private byte[]? LoadLogoImage()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ReqChecker.App.Resources.Images.reqchecker-logo.png";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                return null;

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        catch
        {
            // If logo loading fails, return null and use text fallback
            return null;
        }
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
                    header.Cell().Element(c => c.Background(Colors.Grey.Lighten3)).Padding(5).Text("Test Name").Bold();
                    header.Cell().Element(c => c.Background(Colors.Grey.Lighten3)).Padding(5).Text("Status").Bold();
                    header.Cell().Element(c => c.Background(Colors.Grey.Lighten3)).Padding(5).Text("Duration").Bold();
                    header.Cell().Element(c => c.Background(Colors.Grey.Lighten3)).Padding(5).Text("Type").Bold();
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

                    table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Padding(5)
                        .Text(result.DisplayName);
                    table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Padding(5)
                        .Text(result.Status.ToString()).FontColor(statusColor);
                    table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Padding(5)
                        .Text($"{result.Duration.TotalMilliseconds:F0}ms");
                    table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Padding(5)
                        .Text(result.TestType);

                    // Show error details for failed tests
                    if (result.Status == TestStatus.Fail && result.Error != null)
                    {
                        table.Cell().ColumnSpan(4).Element(c => c.Background(Colors.Red.Lighten4).Padding(5).PaddingLeft(25))
                            .Text($"Error: {result.Error.Message}")
                            .FontSize(9).FontColor(Colors.Red.Darken1);
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
