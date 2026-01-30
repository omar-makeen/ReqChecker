using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using ReqChecker.Infrastructure.Tests;
using System.Diagnostics;
using System.Reflection;

namespace ReqChecker.Infrastructure.Execution;

/// <summary>
/// Executes tests sequentially with progress reporting and cancellation support.
/// </summary>
public class SequentialTestRunner : ITestRunner
{
    private readonly Dictionary<string, ITest> _tests;

    /// <summary>
    /// Initializes a new instance of the SequentialTestRunner.
    /// </summary>
    /// <param name="tests">The available test implementations.</param>
    public SequentialTestRunner(IEnumerable<ITest> tests)
    {
        _tests = tests.ToDictionary(t => GetTestType(t), t => t);
    }

    /// <summary>
    /// Gets the test type name from an ITest instance.
    /// </summary>
    private static string GetTestType(ITest test)
    {
        var testType = test.GetType();
        var attribute = testType.GetCustomAttributes(typeof(TestTypeAttribute), false).FirstOrDefault() as TestTypeAttribute;
        return attribute?.TypeIdentifier ?? testType.Name;
    }

    /// <inheritdoc/>
    public async Task<RunReport> RunTestsAsync(
        Profile profile,
        IProgress<TestResult> progress,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<TestResult>();
        var machineInfo = ReqChecker.Infrastructure.Platform.MachineInfoCollector.Collect();

        foreach (var testDefinition in profile.Tests)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check if test type is registered
            if (!_tests.TryGetValue(testDefinition.Type, out var test))
            {
                var result = new TestResult
                {
                    TestId = testDefinition.Id,
                    TestType = testDefinition.Type,
                    DisplayName = testDefinition.DisplayName,
                    Status = TestStatus.Fail,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    Duration = TimeSpan.Zero,
                    Error = new TestError
                    {
                        Category = ErrorCategory.Configuration,
                        Message = $"Test type '{testDefinition.Type}' is not registered"
                    }
                };
                results.Add(result);
                progress?.Report(result);
                continue;
            }

            // Check if test requires admin and user is not admin
            if (testDefinition.RequiresAdmin && !ReqChecker.Infrastructure.Platform.AdminPrivilegeChecker.IsAdministrator())
            {
                var result = new TestResult
                {
                    TestId = testDefinition.Id,
                    TestType = testDefinition.Type,
                    DisplayName = testDefinition.DisplayName,
                    Status = TestStatus.Skipped,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    Duration = TimeSpan.Zero,
                    Error = new TestError
                    {
                        Category = ErrorCategory.Permission,
                        Message = "Test requires administrator privileges"
                    }
                };
                results.Add(result);
                progress?.Report(result);
                continue;
            }

            // Execute test with retry policy
            var testResult = await RetryPolicy.ExecuteWithRetryAsync(test, testDefinition, cancellationToken);
            results.Add(testResult);
            progress?.Report(testResult);
        }

        stopwatch.Stop();

        // Build summary
        var summary = new RunSummary
        {
            TotalTests = results.Count,
            Passed = results.Count(r => r.Status == TestStatus.Pass),
            Failed = results.Count(r => r.Status == TestStatus.Fail),
            Skipped = results.Count(r => r.Status == TestStatus.Skipped),
            PassRate = results.Count > 0 ? (double)results.Count(r => r.Status == TestStatus.Pass) / results.Count * 100 : 0
        };

        // Build report
        var report = new RunReport
        {
            ProfileId = profile.Id,
            ProfileName = profile.Name,
            StartTime = DateTime.UtcNow - stopwatch.Elapsed,
            EndTime = DateTime.UtcNow,
            Duration = stopwatch.Elapsed,
            MachineInfo = machineInfo,
            Results = results,
            Summary = summary
        };

        return report;
    }

    /// <summary>
    /// Registers a test implementation.
    /// </summary>
    /// <param name="test">The test to register.</param>
    public void RegisterTest(ITest test)
    {
        var testType = GetTestType(test);
        _tests[testType] = test;
    }

    /// <summary>
    /// Gets all registered test types.
    /// </summary>
    /// <returns>A list of registered test type names.</returns>
    public IEnumerable<string> GetRegisteredTestTypes()
    {
        return _tests.Keys;
    }
}
