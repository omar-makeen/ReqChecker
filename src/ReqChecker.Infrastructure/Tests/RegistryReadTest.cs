using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using Microsoft.Win32;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests reading values from the Windows registry.
/// </summary>
[TestType("RegistryRead")]
public class RegistryReadTest : ITest
{
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
#if WINDOWS
            // Get parameters
            var hiveName = testDefinition.Parameters["hive"]?.ToString() ?? "LocalMachine";
            var subKeyPath = testDefinition.Parameters["subKeyPath"]?.ToString() ?? string.Empty;
            var valueName = testDefinition.Parameters["valueName"]?.ToString();
            var expectedValue = testDefinition.Parameters["expectedValue"]?.ToString();
            var expectedType = testDefinition.Parameters["expectedType"]?.ToString();

            if (string.IsNullOrEmpty(subKeyPath))
            {
                throw new ArgumentException("subKeyPath parameter is required", nameof(subKeyPath));
            }

            // Parse hive
            var hive = hiveName.ToLower() switch
            {
                "classesroot" => RegistryHive.ClassesRoot,
                "currentconfig" => RegistryHive.CurrentConfig,
                "currentuser" => RegistryHive.CurrentUser,
                "localmachine" => RegistryHive.LocalMachine,
                "users" => RegistryHive.Users,
                _ => throw new ArgumentException($"Invalid hive: {hiveName}", nameof(hiveName))
            };

            // Open registry key
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var subKey = baseKey.OpenSubKey(subKeyPath);

            if (subKey == null)
            {
                throw new KeyNotFoundException($"Registry key not found: {hiveName}\\{subKeyPath}");
            }

            // Read value
            object? value = null;
            RegistryValueKind? valueKind = null;
            bool isPass = true;

            if (!string.IsNullOrEmpty(valueName))
            {
                // Just check if key exists
                isPass = true;
            }
            else
            {
                value = subKey.GetValue(valueName);
                valueKind = subKey.GetValueKind(valueName);

                // Check expected value
                if (!string.IsNullOrEmpty(expectedValue))
                {
                    var valueStr = value?.ToString() ?? string.Empty;
                    isPass = valueStr == expectedValue;
                }

                // Check expected type
                if (!string.IsNullOrEmpty(expectedType))
                {
                    var expectedKind = expectedType.ToLower() switch
                    {
                        "string" => RegistryValueKind.String,
                        "dword" => RegistryValueKind.DWord,
                        "qword" => RegistryValueKind.QWord,
                        "multistring" => RegistryValueKind.MultiString,
                        "binary" => RegistryValueKind.Binary,
                        "expandstring" => RegistryValueKind.ExpandString,
                        _ => null
                    };

                    if (expectedKind.HasValue && valueKind != expectedKind.Value)
                    {
                        isPass = false;
                    }
                }
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = isPass ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["hive"] = hiveName,
                ["subKeyPath"] = subKeyPath,
                ["valueName"] = valueName ?? "(default)",
                ["valueKind"] = valueKind?.ToString() ?? "N/A",
                ["value"] = value?.ToString() ?? "N/A",
                ["isPass"] = isPass
            };

            if (!string.IsNullOrEmpty(expectedValue))
            {
                evidence["expectedValue"] = expectedValue;
                evidence["valueMatches"] = value?.ToString() == expectedValue;
            }

            if (!string.IsNullOrEmpty(expectedType))
            {
                evidence["expectedType"] = expectedType;
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                RegistryValue = value?.ToString(),
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds
                }
            };
#else
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Skipped;
            result.Error = new TestError
            {
                Category = ErrorCategory.Permission,
                Message = "Registry operations are only supported on Windows"
            };
#endif
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
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Validation,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }

        return await Task.FromResult(result);
    }
}
