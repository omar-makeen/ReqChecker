using ReqChecker.Core.Models;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Exports run reports to various formats.
/// </summary>
public interface IExporter
{
    /// <summary>
    /// Gets the file extension for this exporter.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Exports a run report to a file.
    /// </summary>
    /// <param name="report">The run report to export.</param>
    /// <param name="filePath">The destination file path.</param>
    /// <param name="maskCredentials">Whether to mask credential values.</param>
    Task ExportAsync(RunReport report, string filePath, bool maskCredentials = true);
}
