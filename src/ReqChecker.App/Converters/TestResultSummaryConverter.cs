using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using System.Globalization;
using System.Text.Json;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Generates human-readable summary text from a TestResult object.
/// </summary>
public class TestResultSummaryConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TestResult result)
            return null!;

        return GenerateSummary(result)!;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("TestResultSummaryConverter is one-way only.");
    }

    private static string? GenerateSummary(TestResult result)
    {
        // For failed tests, return the error message
        if (result.Status == TestStatus.Fail)
        {
            return result.Error?.Message ?? "Test failed";
        }

        // For skipped tests, return the error message (if any) or default message
        if (result.Status == TestStatus.Skipped)
        {
            return result.Error?.Message ?? "Test was skipped";
        }

        // For passed tests, generate test-type specific summary
        if (result.Status == TestStatus.Pass)
        {
            var summary = GeneratePassSummary(result);
            // If no summary could be generated, return fallback message
            return summary ?? "Test completed with no additional details";
        }

        return null;
    }

    private static string? GeneratePassSummary(TestResult result)
    {
        // Try to extract data from ResponseData JSON
        string? url = null;
        string? host = null;
        string? path = null;
        string? server = null;

        if (!string.IsNullOrEmpty(result.Evidence.ResponseData))
        {
            try
            {
                var evidenceData = JsonSerializer.Deserialize<Dictionary<string, object>>(result.Evidence.ResponseData);
                if (evidenceData != null)
                {
                    if (evidenceData.TryGetValue("url", out var urlObj))
                        url = urlObj?.ToString();
                    if (evidenceData.TryGetValue("host", out var hostObj))
                        host = hostObj?.ToString();
                    if (evidenceData.TryGetValue("path", out var pathObj))
                        path = pathObj?.ToString();
                    if (evidenceData.TryGetValue("server", out var serverObj))
                        server = serverObj?.ToString();
                }
            }
            catch
            {
                // If JSON parsing fails, continue with default behavior
            }
        }

        // Generate test-type specific summary
        return result.TestType switch
        {
            "HttpGet" => !string.IsNullOrEmpty(url) && result.Evidence.ResponseCode.HasValue
                ? $"HTTP GET to {url} returned {result.Evidence.ResponseCode.Value}"
                : null,
            "Ping" => !string.IsNullOrEmpty(host)
                ? $"Ping to {host} succeeded"
                : null,
            "HttpPost" => !string.IsNullOrEmpty(url) && result.Evidence.ResponseCode.HasValue
                ? $"HTTP POST to {url} returned {result.Evidence.ResponseCode.Value}"
                : null,
            "FileExists" => !string.IsNullOrEmpty(path)
                ? $"File exists: {path}"
                : null,
            "FileRead" => !string.IsNullOrEmpty(path)
                ? $"Successfully read file: {path}"
                : null,
            "FileWrite" => !string.IsNullOrEmpty(path)
                ? $"Successfully wrote to: {path}"
                : null,
            "DirectoryExists" => !string.IsNullOrEmpty(path)
                ? $"Directory exists: {path}"
                : null,
            "FtpRead" => !string.IsNullOrEmpty(server)
                ? $"FTP read from {server} successful"
                : null,
            "FtpWrite" => !string.IsNullOrEmpty(server)
                ? $"FTP write to {server} successful"
                : null,
            "ProcessList" => "Process check completed",
            "RegistryRead" => !string.IsNullOrEmpty(path)
                ? $"Registry value retrieved: {path}"
                : null,
            _ => null
        };
    }
}
