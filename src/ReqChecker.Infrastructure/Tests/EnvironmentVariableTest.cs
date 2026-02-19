using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Verifies that a named environment variable exists on the local machine and
/// optionally validates its value using exact, contains, regex, or pathContains matching.
/// </summary>
[TestType("EnvironmentVariable")]
public class EnvironmentVariableTest : ITest
{
    private static readonly string[] ValidMatchTypes = ["exact", "contains", "regex", "pathContains"];

    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
    {
        var result = new TestResult
        {
            TestId = testDefinition.Id,
            TestType = testDefinition.Type,
            DisplayName = testDefinition.DisplayName,
            Status = TestStatus.Fail,
            StartTime = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // --- Parameter extraction ---
            var variableName = testDefinition.Parameters["variableName"]?.GetValue<string?>();
            var expectedValue = testDefinition.Parameters["expectedValue"]?.GetValue<string?>();
            var matchType = testDefinition.Parameters["matchType"]?.GetValue<string?>();

            // --- Validation: variableName required ---
            if (string.IsNullOrWhiteSpace(variableName))
            {
                return await Fail(result, stopwatch, ErrorCategory.Configuration,
                    "Invalid configuration: variableName is required");
            }

            // --- Validation: matchType without expectedValue ---
            if (!string.IsNullOrWhiteSpace(matchType) && string.IsNullOrEmpty(expectedValue))
            {
                return await Fail(result, stopwatch, ErrorCategory.Configuration,
                    "Invalid configuration: expectedValue is required when matchType is specified");
            }

            // --- Validation: unrecognized matchType ---
            if (!string.IsNullOrWhiteSpace(matchType) &&
                !ValidMatchTypes.Contains(matchType, StringComparer.OrdinalIgnoreCase))
            {
                return await Fail(result, stopwatch, ErrorCategory.Configuration,
                    $"Invalid configuration: matchType '{matchType}' is not valid. Valid values: exact, contains, regex, pathContains");
            }

            // --- Default matchType ---
            var resolvedMatchType = string.IsNullOrWhiteSpace(matchType)
                ? (string.IsNullOrEmpty(expectedValue) ? "existence" : "exact")
                : matchType.ToLowerInvariant();

            // --- Read environment variable ---
            cancellationToken.ThrowIfCancellationRequested();
            var actualValue = Environment.GetEnvironmentVariable(variableName);

            // --- Existence check (US1) ---
            var found = !string.IsNullOrEmpty(actualValue);
            if (!found)
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = $"'{variableName}' is not defined"
                };
                result.HumanSummary = result.Error.Message;
                result.Evidence = new TestEvidence
                {
                    ResponseData = JsonSerializer.Serialize(new Dictionary<string, object?>
                    {
                        ["variableName"] = variableName,
                        ["found"] = false,
                        ["actualValue"] = null,
                        ["matchType"] = resolvedMatchType,
                        ["expectedValue"] = expectedValue,
                        ["matchResult"] = null
                    }),
                    Timing = new TimingBreakdown { TotalMs = (int)stopwatch.ElapsedMilliseconds }
                };
                return await Task.FromResult(result);
            }

            // --- Value matching (US2 / US3) ---
            bool isPass;
            string humanSummary;
            TestError? error = null;
            List<string>? pathEntries = null;

            var truncated = Truncate(actualValue!, 200);

            if (resolvedMatchType == "existence")
            {
                // US1: just presence
                isPass = true;
                humanSummary = $"{variableName} is set to '{truncated}'";
            }
            else if (resolvedMatchType == "exact")
            {
                isPass = string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
                humanSummary = isPass
                    ? $"{variableName} equals '{expectedValue}'"
                    : $"{variableName} value '{truncated}' does not equal '{expectedValue}'";
                if (!isPass)
                    error = new TestError { Category = ErrorCategory.Validation, Message = humanSummary };
            }
            else if (resolvedMatchType == "contains")
            {
                isPass = actualValue!.Contains(expectedValue!, StringComparison.OrdinalIgnoreCase);
                humanSummary = isPass
                    ? $"{variableName} contains '{expectedValue}'"
                    : $"{variableName} value does not contain '{expectedValue}'";
                if (!isPass)
                    error = new TestError { Category = ErrorCategory.Validation, Message = humanSummary };
            }
            else if (resolvedMatchType == "regex")
            {
                try
                {
                    isPass = Regex.IsMatch(actualValue!, expectedValue!, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));
                    humanSummary = isPass
                        ? $"{variableName} matches pattern '{expectedValue}'"
                        : $"{variableName} value does not match pattern '{expectedValue}'";
                    if (!isPass)
                        error = new TestError { Category = ErrorCategory.Validation, Message = humanSummary };
                }
                catch (RegexMatchTimeoutException)
                {
                    return await Fail(result, stopwatch, ErrorCategory.Configuration,
                        "Regex evaluation timed out after 5 seconds");
                }
                catch (ArgumentException ex)
                {
                    return await Fail(result, stopwatch, ErrorCategory.Configuration,
                        $"Invalid regex pattern: {ex.Message}");
                }
            }
            else // pathContains
            {
                var entries = actualValue!
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim().TrimEnd('\\', '/'))
                    .ToList();

                pathEntries = entries.Take(50).ToList();

                var normalized = expectedValue!.TrimEnd('\\', '/');
                isPass = entries.Any(e => string.Equals(e, normalized, StringComparison.OrdinalIgnoreCase));
                humanSummary = isPass
                    ? $"{variableName} contains path '{expectedValue}'"
                    : $"{variableName} does not contain path '{expectedValue}'";
                if (!isPass)
                    error = new TestError { Category = ErrorCategory.Validation, Message = humanSummary };
            }

            // --- Build evidence ---
            var evidenceDict = new Dictionary<string, object?>
            {
                ["variableName"] = variableName,
                ["found"] = found,
                ["actualValue"] = actualValue,
                ["matchType"] = resolvedMatchType,
                ["expectedValue"] = expectedValue,
                ["matchResult"] = isPass ? "pass" : "fail"
            };
            if (pathEntries != null)
                evidenceDict["pathEntries"] = pathEntries;

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = isPass ? TestStatus.Pass : TestStatus.Fail;
            result.HumanSummary = humanSummary;
            result.Error = error;
            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidenceDict),
                Timing = new TimingBreakdown { TotalMs = (int)stopwatch.ElapsedMilliseconds }
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Skipped;
            result.Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = "Test was cancelled"
            };
            result.HumanSummary = "Test was cancelled";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace
            };
            result.HumanSummary = $"Unexpected error: {ex.Message}";
        }

        return await Task.FromResult(result);
    }

    private static Task<TestResult> Fail(TestResult result, Stopwatch stopwatch,
        ErrorCategory category, string message)
    {
        stopwatch.Stop();
        result.EndTime = DateTime.UtcNow;
        result.Duration = stopwatch.Elapsed;
        result.Status = TestStatus.Fail;
        result.Error = new TestError { Category = category, Message = message };
        result.HumanSummary = message;
        return Task.FromResult(result);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength) return value;
        return value[..maxLength] + "…";
    }
}
