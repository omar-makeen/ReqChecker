using ReqChecker.Core.Models;
using System.Text.RegularExpressions;

namespace ReqChecker.Infrastructure.Export;

/// <summary>
/// Masks credential values in run reports for secure export.
/// </summary>
public static class CredentialMasker
{
    private const string MaskValue = "*****";

    /// <summary>
    /// Creates a copy of the report with credential values masked.
    /// </summary>
    public static RunReport MaskCredentials(RunReport report)
    {
        return new RunReport
        {
            RunId = report.RunId,
            ProfileId = report.ProfileId,
            ProfileName = report.ProfileName,
            StartTime = report.StartTime,
            EndTime = report.EndTime,
            Duration = report.Duration,
            MachineInfo = report.MachineInfo,
            Summary = report.Summary,
            Results = report.Results.Select(MaskTestResult).ToList()
        };
    }

    /// <summary>
    /// Masks credential values in a test result.
    /// </summary>
    private static TestResult MaskTestResult(TestResult result)
    {
        return new TestResult
        {
            TestId = result.TestId,
            TestType = result.TestType,
            DisplayName = result.DisplayName,
            Status = result.Status,
            StartTime = result.StartTime,
            EndTime = result.EndTime,
            Duration = result.Duration,
            AttemptCount = result.AttemptCount,
            Error = result.Error,
            HumanSummary = result.HumanSummary,
            TechnicalDetails = MaskCredentialsInText(result.TechnicalDetails) ?? string.Empty,
            Evidence = MaskEvidence(result.Evidence)
        };
    }

    /// <summary>
    /// Masks credential values in test evidence.
    /// </summary>
    private static TestEvidence MaskEvidence(TestEvidence evidence)
    {
        if (evidence == null)
        {
            return new TestEvidence();
        }

        return new TestEvidence
        {
            ResponseData = MaskCredentialsInText(evidence.ResponseData),
            ResponseCode = evidence.ResponseCode,
            ResponseHeaders = evidence.ResponseHeaders,
            FileContent = MaskCredentialsInText(evidence.FileContent),
            ProcessList = evidence.ProcessList,
            RegistryValue = MaskCredentialsInText(evidence.RegistryValue),
            Timing = evidence.Timing
        };
    }

    /// <summary>
    /// Masks credential values in text content.
    /// Looks for common credential patterns and replaces them with asterisks.
    /// </summary>
    public static string? MaskCredentialsInText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var masked = text;

        // Mask common credential patterns (password, authorization headers, tokens, keys)
        masked = Regex.Replace(
            masked,
            @"(?i)(password|pwd|authorization|token|api[_-]?key|secret)\s*[:=]\s*[""']?([^""'\s,}]+)[""']?",
            m => $"{m.Groups[1].Value}: {MaskValue}");

        // Mask basic auth patterns
        masked = Regex.Replace(
            masked,
            @"Basic\s+[A-Za-z0-9+/=]+",
            $"Basic {MaskValue}");

        // Mask bearer tokens
        masked = Regex.Replace(
            masked,
            @"Bearer\s+[A-Za-z0-9\-._~+/]+=*",
            $"Bearer {MaskValue}");

        // Mask potential connection strings (password, user id, uid)
        masked = Regex.Replace(
            masked,
            @"(?i)(password|user id|uid)\s*=\s*[^;]+",
            m => $"{m.Groups[1].Value}={MaskValue}");

        return masked;
    }
}
