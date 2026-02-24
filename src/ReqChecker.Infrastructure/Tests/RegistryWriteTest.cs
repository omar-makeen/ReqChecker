using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Security;
using System.Text.Json;
#if WINDOWS
using Microsoft.Win32;
#endif

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests write access to a Windows registry key by performing a write-read-delete
/// cycle with a temporary test value.
/// </summary>
[TestType("RegistryWrite")]
public class RegistryWriteTest : ITest
{
    private const string DefaultTestValueName = "_ReqChecker_WriteTest";

    /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' — registry I/O is synchronous
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
#pragma warning restore CS1998
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

        // Evidence is always populated — initialise with safe defaults
        var targetStr = string.Empty;
        var keyPath = string.Empty;
        var testValueName = DefaultTestValueName;
        var writeSucceeded = false;
        var cleanupSucceeded = false;

        try
        {
            // ── Parameter extraction ─────────────────────────────────────────
            targetStr = testDefinition.Parameters["target"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(targetStr))
                throw new ArgumentException("Parameter 'target' is required.");

            keyPath = testDefinition.Parameters["keyPath"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(keyPath))
                throw new ArgumentException("Parameter 'keyPath' is required.");

            var testValueNameParam = testDefinition.Parameters["testValueName"]?.ToString();
            if (!string.IsNullOrWhiteSpace(testValueNameParam))
                testValueName = testValueNameParam;

            // ── Target → RegistryHive mapping ────────────────────────────────
#if WINDOWS
            var hive = targetStr.ToUpperInvariant() switch
            {
                "HKLM" => RegistryHive.LocalMachine,
                "HKCU" => RegistryHive.CurrentUser,
                _ => throw new ArgumentException(
                    $"Parameter 'target' must be \"HKLM\" or \"HKCU\". Got: \"{targetStr}\".")
            };

            // ── Open registry key (writable) ─────────────────────────────────
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var subKey = baseKey.OpenSubKey(keyPath, writable: true);

            if (subKey == null)
                throw new KeyNotFoundException(
                    $"Registry key not found: {targetStr}\\{keyPath}");

            // ── Write-read-delete cycle ───────────────────────────────────────
            var testValue = DateTime.UtcNow.ToString("o"); // ISO-8601 timestamp
            subKey.SetValue(testValueName, testValue);

            // Read back to verify the write persisted
            var readBack = subKey.GetValue(testValueName)?.ToString();
            if (readBack != testValue)
                throw new InvalidOperationException(
                    $"Registry write verification failed: value read back did not match written value for {targetStr}\\{keyPath}\\{testValueName}");

            writeSucceeded = true;

            // Cleanup — wrapped so a delete failure doesn't hide the pass
            try
            {
                subKey.DeleteValue(testValueName);
                cleanupSucceeded = true;
            }
            catch
            {
                // Write access confirmed even if cleanup fails
                cleanupSucceeded = false;
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Pass;
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
        catch (KeyNotFoundException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Validation,
                Message = ex.Message
            };
        }
        catch (UnauthorizedAccessException)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Permission,
                Message = $"Access denied: the current process does not have write permission to {targetStr}\\{keyPath}"
            };
        }
        catch (SecurityException)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Permission,
                Message = $"Access denied: the current process does not have write permission to {targetStr}\\{keyPath}"
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
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }
        finally
        {
            // Evidence always populated on all paths
            var evidence = new Dictionary<string, object>
            {
                ["target"] = targetStr,
                ["keyPath"] = keyPath,
                ["testValueName"] = testValueName,
                ["writeSucceeded"] = writeSucceeded,
                ["cleanupSucceeded"] = cleanupSucceeded
            };

            result.Evidence ??= new TestEvidence();
            result.Evidence.ResponseData = JsonSerializer.Serialize(evidence);
            result.Evidence.Timing = new TimingBreakdown
            {
                TotalMs = (int)stopwatch.ElapsedMilliseconds
            };
        }

        return await Task.FromResult(result);
    }
}
