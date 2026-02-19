using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Win32;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Checks whether a named application is installed on the local machine by
/// searching the Windows registry uninstall keys. Supports informational mode
/// (found/not found) and optional minimum-version enforcement.
/// </summary>
[TestType("InstalledSoftware")]
public class InstalledSoftwareTest : ITest
{
    private static readonly string[] UninstallKeyPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
        @"Software\Microsoft\Windows\CurrentVersion\Uninstall"   // HKCU
    ];

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
            var softwareName = testDefinition.Parameters["softwareName"]?.GetValue<string?>();
            var minimumVersion = testDefinition.Parameters["minimumVersion"]?.GetValue<string?>();

            // --- Validation: softwareName required ---
            if (string.IsNullOrWhiteSpace(softwareName))
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Configuration,
                    Message = "Invalid configuration: softwareName is required"
                };
                result.HumanSummary = result.Error.Message;
                return await Task.FromResult(result);
            }

            // --- Validation: minimumVersion format ---
            Version? minimumVersionParsed = null;
            if (!string.IsNullOrWhiteSpace(minimumVersion))
            {
                if (!Version.TryParse(minimumVersion, out minimumVersionParsed))
                {
                    stopwatch.Stop();
                    result.EndTime = DateTime.UtcNow;
                    result.Duration = stopwatch.Elapsed;
                    result.Status = TestStatus.Fail;
                    result.Error = new TestError
                    {
                        Category = ErrorCategory.Configuration,
                        Message = $"Invalid minimumVersion format '{minimumVersion}': expected major.minor[.patch[.revision]]"
                    };
                    result.HumanSummary = result.Error.Message;
                    return await Task.FromResult(result);
                }
            }

            // --- Registry search ---
            cancellationToken.ThrowIfCancellationRequested();
            var allMatches = SearchRegistry(softwareName, cancellationToken);

            // --- Not found ---
            if (allMatches.Count == 0)
            {
                stopwatch.Stop();
                result.EndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.Status = TestStatus.Fail;
                result.Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = $"'{softwareName}' not found in installed programs"
                };
                result.HumanSummary = result.Error.Message;
                result.Evidence = new TestEvidence
                {
                    Timing = new TimingBreakdown { TotalMs = (int)stopwatch.ElapsedMilliseconds }
                };
                return await Task.FromResult(result);
            }

            // --- Select primary match (highest parseable version) ---
            var primary = SelectPrimaryMatch(allMatches);
            var displayName = primary["displayName"] ?? softwareName;
            var installedVersionStr = primary["version"];
            var installLocation = primary["installLocation"];
            var publisher = primary["publisher"];
            var installDate = primary["installDate"];

            // --- Build evidence dictionary ---
            var allMatchesSummary = allMatches
                .Select(m => new { displayName = m["displayName"], version = m["version"] })
                .ToArray();

            var evidenceDict = new Dictionary<string, object?>
            {
                ["displayName"] = displayName,
                ["version"] = string.IsNullOrEmpty(installedVersionStr) ? "unknown" : installedVersionStr,
                ["installLocation"] = installLocation,
                ["publisher"] = publisher,
                ["installDate"] = installDate,
                ["allMatches"] = allMatchesSummary
            };

            // --- Comparison logic ---
            bool isPass;
            string humanSummary;
            TestError? error = null;

            var versionLabel = string.IsNullOrEmpty(installedVersionStr) ? null : installedVersionStr;

            if (minimumVersionParsed != null)
            {
                // minimumVersion mode
                Version? installedVersionParsed = null;
                if (!string.IsNullOrEmpty(installedVersionStr))
                {
                    // Strip non-numeric suffix (e.g. "3.12.0-beta" → "3.12.0")
                    var numericPart = StripVersionSuffix(installedVersionStr);
                    Version.TryParse(numericPart, out installedVersionParsed);
                }

                if (installedVersionParsed == null)
                {
                    isPass = false;
                    humanSummary = $"Cannot verify version for {displayName} — version unknown";
                    error = new TestError { Category = ErrorCategory.Validation, Message = humanSummary };
                }
                else if (installedVersionParsed >= minimumVersionParsed)
                {
                    isPass = true;
                    humanSummary = $"{displayName} {installedVersionStr} meets minimum {minimumVersion}";
                }
                else
                {
                    isPass = false;
                    humanSummary = $"{displayName} {installedVersionStr} does not meet minimum {minimumVersion}";
                    error = new TestError { Category = ErrorCategory.Validation, Message = humanSummary };
                }
            }
            else
            {
                // Informational mode (US1 / US3)
                isPass = true;
                humanSummary = versionLabel != null
                    ? $"{displayName} {versionLabel} installed"
                    : $"{displayName} installed (version unknown)";
            }

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

    /// <summary>
    /// Searches the three standard Windows registry uninstall hives for entries whose
    /// DisplayName contains <paramref name="softwareName"/> (case-insensitive).
    /// </summary>
    private static List<Dictionary<string, string?>> SearchRegistry(string softwareName, CancellationToken cancellationToken)
    {
        var matches = new List<Dictionary<string, string?>>();

        // Hive 1 & 2: HKLM (64-bit and 32-bit)
        SearchHive(Registry.LocalMachine, UninstallKeyPaths[0], softwareName, matches, cancellationToken);
        SearchHive(Registry.LocalMachine, UninstallKeyPaths[1], softwareName, matches, cancellationToken);
        // Hive 3: HKCU (per-user installs)
        SearchHive(Registry.CurrentUser, UninstallKeyPaths[2], softwareName, matches, cancellationToken);

        return matches;
    }

    private static void SearchHive(RegistryKey hive, string subKeyPath, string softwareName,
        List<Dictionary<string, string?>> results, CancellationToken cancellationToken)
    {
#if WINDOWS
        try
        {
            using var uninstallKey = hive.OpenSubKey(subKeyPath);
            if (uninstallKey == null) return;

            foreach (var subKeyName in uninstallKey.GetSubKeyNames())
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using var appKey = uninstallKey.OpenSubKey(subKeyName);
                    if (appKey == null) continue;

                    var displayName = appKey.GetValue("DisplayName")?.ToString();
                    if (string.IsNullOrEmpty(displayName)) continue;

                    if (displayName.Contains(softwareName, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new Dictionary<string, string?>
                        {
                            ["displayName"] = displayName,
                            ["version"] = appKey.GetValue("DisplayVersion")?.ToString(),
                            ["installLocation"] = appKey.GetValue("InstallLocation")?.ToString(),
                            ["publisher"] = appKey.GetValue("Publisher")?.ToString(),
                            ["installDate"] = appKey.GetValue("InstallDate")?.ToString()
                        });
                    }
                }
                catch (Exception)
                {
                    // Skip inaccessible subkeys
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Hive not accessible — skip silently
        }
#endif
    }

    /// <summary>
    /// Returns the match with the highest parseable version. Entries with
    /// unparseable versions sort last; if all are unparseable, returns the first entry.
    /// </summary>
    private static Dictionary<string, string?> SelectPrimaryMatch(List<Dictionary<string, string?>> matches)
    {
        return matches
            .OrderByDescending(m =>
            {
                var v = m["version"];
                if (string.IsNullOrEmpty(v)) return null;
                var numeric = StripVersionSuffix(v);
                return Version.TryParse(numeric, out var parsed) ? parsed : null;
            })
            .First();
    }

    /// <summary>
    /// Strips any non-numeric suffix from a version string so it can be parsed by
    /// <see cref="Version"/>. E.g. "3.12.0-beta" → "3.12.0".
    /// </summary>
    private static string StripVersionSuffix(string version)
    {
        var idx = version.IndexOfAny(['-', '+', ' ']);
        return idx >= 0 ? version[..idx] : version;
    }
}
