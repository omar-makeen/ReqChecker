using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Validates the local machine's Windows version and build number against
/// configurable requirements (minimum build, exact match, or informational).
/// </summary>
[TestType("OsVersion")]
public class OsVersionTest : ITest
{
    // major.minor.build — three numeric segments required for expectedVersion
    private static readonly Regex VersionPattern = new(@"^\d+\.\d+\.\d+$", RegexOptions.Compiled);

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
            var minimumBuild = testDefinition.Parameters["minimumBuild"]?.GetValue<int?>();
            var expectedVersion = testDefinition.Parameters["expectedVersion"]?.GetValue<string?>();

            // --- Validation: conflict ---
            if (minimumBuild.HasValue && !string.IsNullOrWhiteSpace(expectedVersion))
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Configuration,
                    Message = "Invalid configuration: specify either minimumBuild or expectedVersion, not both"
                };
                result.HumanSummary = result.Error.Message;
                return await Task.FromResult(result);
            }

            // --- Validation: minimumBuild range ---
            if (minimumBuild.HasValue && minimumBuild.Value <= 0)
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Configuration,
                    Message = $"Invalid minimumBuild value '{minimumBuild.Value}': must be a positive integer"
                };
                result.HumanSummary = result.Error.Message;
                return await Task.FromResult(result);
            }

            // --- Validation: expectedVersion format ---
            if (!string.IsNullOrWhiteSpace(expectedVersion) && !VersionPattern.IsMatch(expectedVersion))
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Configuration,
                    Message = $"Invalid expectedVersion format '{expectedVersion}': expected major.minor.build (e.g. '10.0.22631')"
                };
                result.HumanSummary = result.Error.Message;
                return await Task.FromResult(result);
            }

            // --- OS detection ---
            var osVersion = Environment.OSVersion.Version;
            var detectedVersionStr = $"{osVersion.Major}.{osVersion.Minor}.{osVersion.Build}";
            var detectedBuild = osVersion.Build;
            var architecture = RuntimeInformation.OSArchitecture.ToString();
            var productName = ReadProductName();

            // --- Evidence (FR-008) ---
            var evidenceDict = new Dictionary<string, object>
            {
                ["productName"] = productName,
                ["version"] = detectedVersionStr,
                ["buildNumber"] = detectedBuild,
                ["architecture"] = architecture
            };

            // --- Comparison logic ---
            bool isPass;
            string humanSummary;

            if (minimumBuild.HasValue)
            {
                // US1: minimum build comparison (FR-003)
                isPass = detectedBuild >= minimumBuild.Value;
                humanSummary = isPass
                    ? $"OS build {detectedBuild} meets minimum requirement of {minimumBuild.Value}"
                    : $"OS build {detectedBuild} does not meet minimum requirement of {minimumBuild.Value}";
            }
            else if (!string.IsNullOrWhiteSpace(expectedVersion))
            {
                // US2: exact version match (FR-004)
                var expected = Version.Parse(expectedVersion);
                isPass = osVersion.Major == expected.Major
                      && osVersion.Minor == expected.Minor
                      && osVersion.Build == expected.Build;
                humanSummary = isPass
                    ? $"OS version {detectedVersionStr} matches expected version"
                    : $"OS version {detectedVersionStr} does not match expected version {expectedVersion}";
            }
            else
            {
                // Informational mode — no constraints (FR-005)
                isPass = true;
                humanSummary = $"{productName} — version {detectedVersionStr} ({architecture})";
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = isPass ? TestStatus.Pass : TestStatus.Fail;
            result.HumanSummary = humanSummary;

            if (!isPass)
            {
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = humanSummary
                };
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidenceDict),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds
                }
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

    private static string ReadProductName()
    {
#if WINDOWS
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return key?.GetValue("ProductName")?.ToString() ?? "Windows";
        }
        catch
        {
            return "Windows";
        }
#else
        return RuntimeInformation.OSDescription;
#endif
    }
}
