using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Text.Json;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests that the installed .NET runtime or SDK meets a minimum version requirement
/// by scanning the dotnet installation directory and comparing all discovered versions.
/// </summary>
[TestType("DotNetVersion")]
public class DotNetVersionTest : ITest
{
    /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators - interface contract requires async
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
    {
        var result = new TestResult
        {
            TestId = testDefinition.Id,
            TestType = testDefinition.Type,
            DisplayName = testDefinition.DisplayName,
            Status = TestStatus.Fail, // Default to fail, will update on success
            StartTime = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Extract parameters
            var parameters = testDefinition.Parameters;

            // Extract runtimeType (required)
            var runtimeType = parameters["runtimeType"]?.ToString();
            
            // Validate runtimeType
            if (string.IsNullOrEmpty(runtimeType))
            {
                throw new ArgumentException("Parameter 'runtimeType' is required");
            }
            
            var runtimeTypeLower = runtimeType.ToLowerInvariant();
            if (runtimeTypeLower != "runtime" && runtimeTypeLower != "sdk")
            {
                throw new ArgumentException("Parameter 'runtimeType' must be \"runtime\" or \"sdk\"");
            }

            // Display-friendly label: "runtime" stays lowercase, "sdk" becomes "SDK"
            var runtimeTypeDisplay = runtimeTypeLower == "sdk" ? "SDK" : runtimeTypeLower;

            // Extract minimumVersion (optional, default "0.0.0")
            var minimumVersionStr = parameters["minimumVersion"]?.ToString();
            if (string.IsNullOrEmpty(minimumVersionStr))
            {
                minimumVersionStr = "0.0.0";
            }

            // Validate minimumVersion format
            if (!Version.TryParse(minimumVersionStr, out var minimumVersion))
            {
                throw new ArgumentException("Parameter 'minimumVersion' must be a valid version string (e.g., \"8.0.0\")");
            }

            // Resolve dotnet root
            var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (string.IsNullOrEmpty(dotnetRoot) || !Directory.Exists(dotnetRoot))
            {
                dotnetRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
            }

            // Determine subdirectory based on runtimeType
            string versionsDirectory;
            if (runtimeTypeLower == "sdk")
            {
                versionsDirectory = Path.Combine(dotnetRoot, "sdk");
            }
            else
            {
                versionsDirectory = Path.Combine(dotnetRoot, "shared", "Microsoft.NETCore.App");
            }

            // Enumerate installed versions
            var installedVersions = new List<Version>();
            string installedVersionsStr = "";
            string highestVersionStr = "";
            bool minimumSatisfied = false;

            if (Directory.Exists(versionsDirectory))
            {
                var directories = Directory.GetDirectories(versionsDirectory);
                foreach (var dir in directories)
                {
                    var dirName = Path.GetFileName(dir);
                    if (Version.TryParse(dirName, out var version))
                    {
                        installedVersions.Add(version);
                    }
                }

                if (installedVersions.Count > 0)
                {
                    installedVersions.Sort();
                    installedVersionsStr = string.Join(";", installedVersions);
                    highestVersionStr = installedVersions[installedVersions.Count - 1].ToString();
                    minimumSatisfied = installedVersions.Any(v => v >= minimumVersion);
                }
            }

            // Build evidence (always populated)
            var evidence = new Dictionary<string, object>
            {
                ["runtimeType"] = runtimeTypeLower,
                ["minimumVersion"] = minimumVersionStr,
                ["installedVersions"] = installedVersionsStr,
                ["minimumSatisfied"] = minimumSatisfied,
                ["highestVersion"] = highestVersionStr
            };

            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds
                }
            };

            // Set result based on findings
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;

            if (installedVersions.Count == 0)
            {
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = $"No .NET {runtimeTypeDisplay} installation found on this machine"
                };
            }
            else if (!minimumSatisfied)
            {
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = $"No installed .NET {runtimeTypeDisplay} meets minimum version {minimumVersionStr} (highest found: {highestVersionStr})"
                };
            }
            else
            {
                result.Status = TestStatus.Pass;
            }
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
                Message = "Test was cancelled or timed out"
            };
        }
        catch (ArgumentException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = ex.Message
            };
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
                Message = ex.Message
            };
        }

        return result;
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators - interface contract requires async
}
