using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using System.Globalization;
using System.Text.Json;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Generates formatted technical details from a TestResult's Evidence.
/// </summary>
public class TestResultDetailsConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TestResult result)
            return null!;

        return GenerateTechnicalDetails(result)!;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("TestResultDetailsConverter is one-way only.");
    }

    private static string? GenerateTechnicalDetails(TestResult result)
    {
        var sections = new List<string>();

        // Parse ResponseData JSON for additional information
        Dictionary<string, object>? evidenceData = null;
        if (!string.IsNullOrEmpty(result.Evidence.ResponseData))
        {
            try
            {
                evidenceData = JsonSerializer.Deserialize<Dictionary<string, object>>(result.Evidence.ResponseData);
            }
            catch
            {
                // If JSON parsing fails, continue with default behavior
            }
        }

        // [General] section - always shown
        sections.Add("[General]");
        sections.Add($"Duration: {result.Duration.TotalMilliseconds:F0}ms");
        sections.Add($"Attempts: {result.AttemptCount}");
        sections.Add(string.Empty);

        // [Response] section - if ResponseCode is set
        if (result.Evidence.ResponseCode.HasValue)
        {
            sections.Add("[Response]");
            var statusCode = result.Evidence.ResponseCode.Value;
            var statusText = GetStatusText(statusCode);
            sections.Add($"Status: {statusCode} {statusText}");

            // Try to get response time from evidence data
            if (evidenceData != null && evidenceData.TryGetValue("responseTime", out var rtObj))
            {
                if (rtObj != null && int.TryParse(rtObj.ToString(), out var responseTime))
                {
                    sections.Add($"Response Time: {responseTime}ms");
                }
            }

            // Try to get content-type from headers
            if (result.Evidence.ResponseHeaders != null &&
                result.Evidence.ResponseHeaders.TryGetValue("Content-Type", out var contentType))
            {
                sections.Add($"Content-Type: {contentType}");
            }

            // Try to get content length from evidence data
            if (evidenceData != null && evidenceData.TryGetValue("contentLength", out var clObj))
            {
                if (clObj != null && int.TryParse(clObj.ToString(), out var contentLength))
                {
                    sections.Add($"Content-Length: {contentLength} bytes");
                }
            }

            sections.Add(string.Empty);
        }

        // [Headers] section - if ResponseHeaders is not empty
        if (result.Evidence.ResponseHeaders != null && result.Evidence.ResponseHeaders.Count > 0)
        {
            sections.Add("[Headers]");
            foreach (var header in result.Evidence.ResponseHeaders)
            {
                sections.Add($"{header.Key}: {header.Value}");
            }
            sections.Add(string.Empty);
        }

        // [Body] section - if ResponseData contains body field
        if (evidenceData != null && evidenceData.TryGetValue("body", out var bodyObj))
        {
            var body = bodyObj?.ToString();
            if (!string.IsNullOrEmpty(body))
            {
                sections.Add("[Body]");
                sections.Add(body);
                sections.Add(string.Empty);
            }
        }

        // [File Content] section - if FileContent is not empty
        if (!string.IsNullOrEmpty(result.Evidence.FileContent))
        {
            sections.Add("[File Content]");
            sections.Add(result.Evidence.FileContent);
            sections.Add(string.Empty);
        }

        // [Process List] section - if ProcessList is not empty
        if (result.Evidence.ProcessList != null && result.Evidence.ProcessList.Length > 0)
        {
            sections.Add("[Process List]");
            foreach (var process in result.Evidence.ProcessList)
            {
                sections.Add(process);
            }
            sections.Add(string.Empty);
        }

        // [Registry] section - if RegistryValue is not empty
        if (!string.IsNullOrEmpty(result.Evidence.RegistryValue))
        {
            sections.Add("[Registry]");
            sections.Add(result.Evidence.RegistryValue);
            sections.Add(string.Empty);
        }

        // [Timing] section - if Timing is not null
        if (result.Evidence.Timing != null)
        {
            sections.Add("[Timing]");
            sections.Add($"Total: {result.Evidence.Timing.TotalMs}ms");
            if (result.Evidence.Timing.ConnectMs.HasValue)
            {
                sections.Add($"Connect: {result.Evidence.Timing.ConnectMs.Value}ms");
            }
            if (result.Evidence.Timing.ExecuteMs.HasValue)
            {
                sections.Add($"Execute: {result.Evidence.Timing.ExecuteMs.Value}ms");
            }
            sections.Add(string.Empty);
        }

        // If no sections were added, return null
        if (sections.Count == 0)
            return null;

        // Remove trailing empty string if present
        if (sections.Count > 0 && string.IsNullOrEmpty(sections[^1]))
            sections.RemoveAt(sections.Count - 1);

        return string.Join(Environment.NewLine, sections);
    }

    private static string GetStatusText(int statusCode)
    {
        return statusCode switch
        {
            200 => "OK",
            201 => "Created",
            204 => "No Content",
            301 => "Moved Permanently",
            302 => "Found",
            304 => "Not Modified",
            307 => "Temporary Redirect",
            308 => "Permanent Redirect",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            409 => "Conflict",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => string.Empty
        };
    }
}
