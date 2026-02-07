using System.Text.Json;
using ReqChecker.Core.Models;

namespace ReqChecker.Core.Services;

/// <summary>
/// Static utility for sanitizing test results before persistence or export.
/// Removes sensitive data while preserving the original in-memory objects.
/// </summary>
public static class TestResultSanitizer
{
    private static readonly string[] SensitiveHeaderKeys =
    [
        "Authorization",
        "Set-Cookie",
        "X-Api-Key",
        "Cookie"
    ];

    private const int MaxTechnicalDetailsLength = 500;
    private const string RedactedPrefix = "[REDACTED — ";
    private const string RedactedSuffix = " chars]";

    /// <summary>
    /// Creates a sanitized copy of a RunReport for persistence or export.
    /// The original report remains unmodified for in-memory use.
    /// </summary>
    /// <param name="report">The original run report.</param>
    /// <returns>A new report with sensitive data redacted.</returns>
    public static RunReport SanitizeForPersistence(RunReport report)
    {
        // Deep clone the report using JSON serialization
        var json = JsonSerializer.Serialize(report);
        var sanitized = JsonSerializer.Deserialize<RunReport>(json) 
            ?? throw new InvalidOperationException("Failed to deserialize RunReport during sanitization.");

        // Sanitize each test result
        foreach (var result in sanitized.Results)
        {
            SanitizeTestResult(result);
        }

        return sanitized;
    }

    private static void SanitizeTestResult(TestResult result)
    {
        // Sanitize evidence
        if (result.Evidence != null)
        {
            SanitizeEvidence(result.Evidence);
        }

        // Truncate technical details
        if (result.TechnicalDetails != null && result.TechnicalDetails.Length > MaxTechnicalDetailsLength)
        {
            result.TechnicalDetails = result.TechnicalDetails.Substring(0, MaxTechnicalDetailsLength);
        }
    }

    private static void SanitizeEvidence(TestEvidence evidence)
    {
        // Redact response data
        if (!string.IsNullOrEmpty(evidence.ResponseData))
        {
            evidence.ResponseData = $"{RedactedPrefix}{evidence.ResponseData.Length}{RedactedSuffix}";
        }

        // Redact file content
        if (!string.IsNullOrEmpty(evidence.FileContent))
        {
            evidence.FileContent = $"{RedactedPrefix}{evidence.FileContent.Length}{RedactedSuffix}";
        }

        // Remove sensitive headers
        if (evidence.ResponseHeaders != null && evidence.ResponseHeaders.Count > 0)
        {
            var headersToRemove = SensitiveHeaderKeys
                .Where(evidence.ResponseHeaders.ContainsKey)
                .ToList();

            foreach (var key in headersToRemove)
            {
                evidence.ResponseHeaders.Remove(key);
            }
        }
    }
}
