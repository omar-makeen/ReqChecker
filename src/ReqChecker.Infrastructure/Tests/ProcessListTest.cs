using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests if specific processes are running.
/// </summary>
[TestType("ProcessList")]
public class ProcessListTest : ITest
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
            // Get parameters
            var processNames = testDefinition.Parameters["processNames"] as System.Text.Json.Nodes.JsonArray;
            var requireAll = testDefinition.Parameters["requireAll"]?.GetValue<bool>() ?? true;
            var includeDetails = testDefinition.Parameters["includeDetails"]?.GetValue<bool>() ?? false;

            if (processNames == null || processNames.Count == 0)
            {
                throw new ArgumentException("processNames parameter is required", nameof(processNames));
            }

            // Get all running processes
            var allProcesses = Process.GetProcesses();
            var processDict = new Dictionary<string, List<Process>>();

            foreach (var process in allProcesses)
            {
                try
                {
                    var processName = process.ProcessName.ToLower();
                    if (!processDict.ContainsKey(processName))
                    {
                        processDict[processName] = new List<Process>();
                    }
                    processDict[processName].Add(process);
                }
                catch
                {
                    // Some processes may not be accessible
                }
            }

            // Check for required processes
            var foundProcesses = new Dictionary<string, bool>();
            var processDetails = new Dictionary<string, List<Dictionary<string, object>>>();

            foreach (var node in processNames)
            {
                var name = node?.ToString()?.ToLower() ?? string.Empty;
                if (string.IsNullOrEmpty(name)) continue;

                var found = processDict.ContainsKey(name);
                foundProcesses[name] = found;

                if (found && includeDetails)
                {
                    var details = new List<Dictionary<string, object>>();
                    foreach (var proc in processDict[name])
                    {
                        try
                        {
                            details.Add(new Dictionary<string, object>
                            {
                                ["processId"] = proc.Id,
                                ["processName"] = proc.ProcessName,
                                ["startTime"] = proc.StartTime,
                                ["mainWindowTitle"] = proc.MainWindowTitle,
                                ["workingSet"] = proc.WorkingSet64
                            });
                        }
                        catch
                        {
                            // Some process info may not be accessible
                        }
                    }
                    processDetails[name] = details;
                }
            }

            // Determine pass/fail
            var isPass = requireAll
                ? foundProcesses.Values.All(v => v)
                : foundProcesses.Values.Any(v => v);

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = isPass ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["requireAll"] = requireAll,
                ["isPass"] = isPass,
                ["foundProcesses"] = foundProcesses,
                ["totalProcesses"] = allProcesses.Length
            };

            if (includeDetails)
            {
                evidence["processDetails"] = processDetails;
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                ProcessList = foundProcesses.Keys.ToArray(),
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
        finally
        {
            // Clean up process handles
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    proc.Dispose();
                }
                catch
                {
                    // Ignore dispose errors
                }
            }
        }

        return await Task.FromResult(result);
    }
}
