using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using ReqChecker.Infrastructure.Tests;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.Execution;

/// <summary>
/// Executes tests sequentially with progress reporting and cancellation support.
/// </summary>
public class SequentialTestRunner : ITestRunner
{
    private readonly Dictionary<string, ITest> _tests;
    private readonly ICredentialProvider? _credentialProvider;

    /// <summary>
    /// Type alias mappings for backward compatibility.
    /// </summary>
    private static readonly Dictionary<string, string> _typeAliases = new()
    {
        { "DnsLookup", "DnsResolve" }
    };

    /// <summary>
    /// Callback for prompting credentials during test execution.
    /// </summary>
    public Func<string, string, string?, Task<(string? username, string? password, bool rememberCredentials)>>? PromptForCredentials { get; set; }

    /// <summary>
    /// Initializes a new instance of SequentialTestRunner.
    /// </summary>
    /// <param name="tests">The available test implementations.</param>
    /// <param name="credentialProvider">Optional credential provider for secure credential storage.</param>
    public SequentialTestRunner(IEnumerable<ITest> tests, ICredentialProvider? credentialProvider = null)
    {
        _tests = tests.ToDictionary(t => GetTestType(t), t => t);
        _credentialProvider = credentialProvider;
    }

    /// <summary>
    /// Gets test type name from an ITest instance.
    /// </summary>
    private static string GetTestType(ITest test)
    {
        var testType = test.GetType();
        var attribute = testType.GetCustomAttributes(typeof(TestTypeAttribute), false).FirstOrDefault() as TestTypeAttribute;
        return attribute?.TypeIdentifier ?? testType.Name;
    }

    /// <summary>
    /// Resolves a test type identifier through the alias mapping.
    /// </summary>
    /// <param name="typeIdentifier">The type identifier to resolve.</param>
    /// <returns>The resolved type identifier (original if no alias exists).</returns>
    private static string ResolveTypeAlias(string typeIdentifier)
    {
        return _typeAliases.TryGetValue(typeIdentifier, out var resolvedType) ? resolvedType : typeIdentifier;
    }

    /// <inheritdoc/>
    public async Task<RunReport> RunTestsAsync(
        ProfileModel profile,
        IProgress<TestResult> progress,
        CancellationToken cancellationToken,
        RunSettings? runSettings = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<TestResult>();
        var machineInfo = ReqChecker.Infrastructure.Platform.MachineInfoCollector.Collect();

        // Use provided settings or create default
        runSettings ??= new RunSettings();

        // Track completed test results for dependency checking
        var completedResults = new Dictionary<string, TestResult>();

        for (int i = 0; i < profile.Tests.Count; i++)
        {
            var testDefinition = profile.Tests[i];
            cancellationToken.ThrowIfCancellationRequested();

            // Resolve type alias for backward compatibility
            var resolvedType = ResolveTypeAlias(testDefinition.Type);

            // Check if test type is registered
            if (!_tests.TryGetValue(resolvedType, out var test))
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
                completedResults[testDefinition.Id] = result;
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
                completedResults[testDefinition.Id] = result;
                continue;
            }

            // Check dependency requirements
            if (testDefinition.DependsOn != null && testDefinition.DependsOn.Count > 0)
            {
                bool shouldSkip = false;
                string skipReason = string.Empty;

                foreach (var depId in testDefinition.DependsOn)
                {
                    if (!completedResults.TryGetValue(depId, out var depResult))
                    {
                        // Prerequisite not yet executed
                        var depTest = profile.Tests.FirstOrDefault(t => t.Id == depId);
                        var depDisplayName = depTest?.DisplayName ?? depId;
                        skipReason = $"Prerequisite test '{depDisplayName}' has not yet been executed";
                        shouldSkip = true;
                        break;
                    }
                    else if (depResult.Status != TestStatus.Pass)
                    {
                        // Prerequisite failed or was skipped
                        var depDisplayName = depResult.DisplayName ?? depId;
                        var statusText = depResult.Status == TestStatus.Fail ? "failed" : "was skipped";
                        skipReason = $"Prerequisite test '{depDisplayName}' {statusText}";
                        shouldSkip = true;
                        break;
                    }
                }

                if (shouldSkip)
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
                            Category = ErrorCategory.Dependency,
                            Message = skipReason
                        },
                        HumanSummary = skipReason
                    };
                    results.Add(result);
                    progress?.Report(result);
                    completedResults[testDefinition.Id] = result;
                    continue;
                }
            }

            // Apply inter-test delay before execution (skip for first test)
            // This makes the delay visible to users as they see "Running Test X" during the delay
            if (runSettings.InterTestDelayMs > 0 && i > 0)
            {
                await Task.Delay(runSettings.InterTestDelayMs, cancellationToken);
            }

            // Check for PromptAtRun fields and prompt for credentials if needed
            TestExecutionContext? context;
            try
            {
                context = await PromptForCredentialsIfNeededAsync(testDefinition, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
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
                        Message = $"Failed to prompt for credentials: {ex.Message}"
                    }
                };
                results.Add(result);
                progress?.Report(result);
                completedResults[testDefinition.Id] = result;
                continue;
            }

            // Only skip if test requires credentials but user cancelled the prompt
            var hasCredentialRef = testDefinition.Parameters?.ContainsKey("credentialRef") == true;
            if (context == null && hasCredentialRef)
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
                        Category = ErrorCategory.Configuration,
                        Message = "Credentials not provided"
                    }
                };
                results.Add(result);
                progress?.Report(result);
                completedResults[testDefinition.Id] = result;
                continue;
            }

            // Execute test with retry policy
            var testResult = await RetryPolicy.ExecuteWithRetryAsync(test, testDefinition, runSettings, context, cancellationToken);
            results.Add(testResult);
            progress?.Report(testResult);

            // Store result for dependency checking
            completedResults[testDefinition.Id] = testResult;
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
            RunId = Guid.NewGuid().ToString("N"),
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
    /// Prompts for credentials if any test parameter has PromptAtRun policy.
    /// </summary>
    /// <returns>The execution context with credentials, or null if no credentials needed.</returns>
    private async Task<TestExecutionContext?> PromptForCredentialsIfNeededAsync(TestDefinition testDefinition, CancellationToken cancellationToken)
    {
        if (testDefinition.Parameters == null)
            return null;

        // Check for pfxPassword parameter first (takes precedence over credentialRef)
        // This allows mTLS tests to read the passphrase directly from configuration without prompting
        if (testDefinition.Parameters.ContainsKey("pfxPassword"))
        {
            var pfxPasswordNode = testDefinition.Parameters["pfxPassword"];
            var pfxPassword = pfxPasswordNode?.ToString() ?? string.Empty;
            return new TestExecutionContext(string.Empty, pfxPassword);
        }

        // Check for credentialRef parameter (indicates PromptAtRun)
        if (testDefinition.Parameters.ContainsKey("credentialRef"))
        {
            var credentialRefNode = testDefinition.Parameters["credentialRef"];
            var credentialRef = credentialRefNode?.ToString();
            if (!string.IsNullOrEmpty(credentialRef))
            {
                string? username = null;
                string? password = null;

                // First, try to get credentials from the credential provider
                if (_credentialProvider != null)
                {
                    var storedCredentials = await _credentialProvider.GetCredentialsAsync(credentialRef);
                    if (storedCredentials.HasValue)
                    {
                        username = storedCredentials.Value.Username;
                        password = storedCredentials.Value.Password;
                    }
                }

                // If credentials not found and prompt callback is available, prompt user
                if ((username == null || password == null) && PromptForCredentials != null)
                {
                    var fieldLabel = FormatFieldName("credentialRef");
                    var result = await PromptForCredentials.Invoke(fieldLabel, credentialRef, null);
                    username = result.username;
                    password = result.password;

                    // Only store credentials if user opted in
                    if (result.rememberCredentials && username != null && password != null && _credentialProvider != null)
                    {
                        try
                        {
                            await _credentialProvider.StoreCredentialsAsync(credentialRef, username, password);
                        }
                        catch (Exception ex)
                        {
                            // Log warning but continue with test execution
                            System.Diagnostics.Debug.WriteLine($"Failed to store credentials: {ex.Message}");
                        }
                    }
                }

                // Return credentials in execution context (not in Parameters)
                // Return null if both username and password are null (user cancelled)
                if (username == null && password == null)
                {
                    return null;
                }
                return new TestExecutionContext(username ?? string.Empty, password ?? string.Empty);
            }
        }

        return null;
    }

    /// <summary>
    /// Formats a field name for display (camelCase to Title Case).
    /// </summary>
    private static string FormatFieldName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return fieldName;

        return System.Text.RegularExpressions.Regex.Replace(
            fieldName,
            "(?<=[a-z])([A-Z])",
            " $1")
            .Trim();
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
