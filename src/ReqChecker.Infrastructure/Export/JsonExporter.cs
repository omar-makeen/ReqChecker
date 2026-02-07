using System.Text.Json;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Services;
using ReqChecker.Core.Utilities;

namespace ReqChecker.Infrastructure.Export;

/// <summary>
/// Exports run reports to JSON format using System.Text.Json with source generation.
/// </summary>
public class JsonExporter : IExporter
{
    /// <inheritdoc />
    public string FileExtension => ".json";

    /// <inheritdoc />
    public async Task ExportAsync(RunReport report, string filePath, bool maskCredentials = true)
    {
        if (report == null)
        {
            throw new ArgumentNullException(nameof(report));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty", nameof(filePath));
        }

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Sanitize sensitive data before export
        var sanitizedReport = TestResultSanitizer.SanitizeForPersistence(report);

        // Apply credential masking if requested (in addition to sanitization)
        var reportToExport = maskCredentials ? CredentialMasker.MaskCredentials(sanitizedReport) : sanitizedReport;

        // Serialize using source-generated context
        var json = JsonSerializer.Serialize(reportToExport, AppJsonContext.Default.RunReport);

        // Write to file
        await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
    }
}
