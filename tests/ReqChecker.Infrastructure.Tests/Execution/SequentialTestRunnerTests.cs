using ReqChecker.Core.Enums;
using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Execution;
using ReqChecker.Infrastructure.Tests;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.Tests.Execution;

/// <summary>
/// Unit tests for SequentialTestRunner including dependency skip logic.
/// </summary>
public class SequentialTestRunnerTests
{
    /// <summary>
    /// A test that always fails, for testing dependency skip behavior.
    /// </summary>
    [TestType("Failing")]
    private class FailingTest : ITest
    {
        public Task<TestResult> ExecuteAsync(TestDefinition definition, TestExecutionContext? context, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult
            {
                TestId = definition.Id,
                TestType = definition.Type,
                DisplayName = definition.DisplayName,
                Status = TestStatus.Fail,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Duration = TimeSpan.Zero,
                Error = new TestError
                {
                    Category = ErrorCategory.Validation,
                    Message = "This test always fails"
                },
                HumanSummary = "This test always fails"
            });
        }
    }

    /// <summary>
    /// A test that completes instantly, for testing purposes.
    /// </summary>
    [TestType("Instant")]
    private class InstantTest : ITest
    {
        public Task<TestResult> ExecuteAsync(TestDefinition definition, TestExecutionContext? context, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult
            {
                TestId = definition.Id,
                TestType = definition.Type,
                DisplayName = definition.DisplayName,
                Status = TestStatus.Pass,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                Duration = TimeSpan.Zero,
                HumanSummary = "Test passed"
            });
        }
    }

    [Fact]
    public async Task RunTestsAsync_WithDependency_SkipsTestWhenPrerequisiteFails()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Failing",
                    DisplayName = "Prerequisite Test",
                    DependsOn = new List<string>()
                },
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Instant",
                    DisplayName = "Dependent Test",
                    DependsOn = new List<string> { "test-001" }
                }
            },
            RunSettings = new RunSettings()
        };

        var tests = new List<ITest> { new FailingTest(), new InstantTest() };
        var runner = new SequentialTestRunner(tests);

        // Act
        var report = await runner.RunTestsAsync(profile, null!, CancellationToken.None);
        var results = report.Results;

        // Assert
        Assert.Equal(2, results.Count);

        var prerequisiteResult = results.First(r => r.TestId == "test-001");
        Assert.Equal(TestStatus.Fail, prerequisiteResult.Status);

        var dependentResult = results.First(r => r.TestId == "test-002");
        Assert.Equal(TestStatus.Skipped, dependentResult.Status);
        Assert.Equal(ErrorCategory.Dependency, dependentResult.Error?.Category);
        Assert.Contains("Prerequisite test 'Prerequisite Test' failed", dependentResult.Error?.Message);
    }

    [Fact]
    public async Task RunTestsAsync_WithDependency_RunsTestWhenPrerequisitePasses()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Instant",
                    DisplayName = "Prerequisite Test",
                    DependsOn = new List<string>()
                },
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Instant",
                    DisplayName = "Dependent Test",
                    DependsOn = new List<string> { "test-001" }
                }
            },
            RunSettings = new RunSettings()
        };

        var tests = new List<ITest> { new InstantTest() };
        var runner = new SequentialTestRunner(tests);

        // Act
        var report = await runner.RunTestsAsync(profile, null!, CancellationToken.None);
        var results = report.Results;

        // Assert
        Assert.Equal(2, results.Count);

        var prerequisiteResult = results.First(r => r.TestId == "test-001");
        Assert.Equal(TestStatus.Pass, prerequisiteResult.Status);

        var dependentResult = results.First(r => r.TestId == "test-002");
        Assert.Equal(TestStatus.Pass, dependentResult.Status);
        Assert.Null(dependentResult.Error);
    }

    [Fact]
    public async Task RunTestsAsync_TransitiveDependency_SkipsCascades()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Failing",
                    DisplayName = "Root Test",
                    DependsOn = new List<string>()
                },
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Instant",
                    DisplayName = "Middle Test",
                    DependsOn = new List<string> { "test-001" }
                },
                new TestDefinition
                {
                    Id = "test-003",
                    Type = "Instant",
                    DisplayName = "Leaf Test",
                    DependsOn = new List<string> { "test-002" }
                }
            },
            RunSettings = new RunSettings()
        };

        var tests = new List<ITest> { new FailingTest(), new InstantTest() };
        var runner = new SequentialTestRunner(tests);

        // Act
        var report = await runner.RunTestsAsync(profile, null!, CancellationToken.None);
        var results = report.Results;

        // Assert
        Assert.Equal(3, results.Count);

        var rootResult = results.First(r => r.TestId == "test-001");
        Assert.Equal(TestStatus.Fail, rootResult.Status);

        var middleResult = results.First(r => r.TestId == "test-002");
        Assert.Equal(TestStatus.Skipped, middleResult.Status);
        Assert.Equal(ErrorCategory.Dependency, middleResult.Error?.Category);

        var leafResult = results.First(r => r.TestId == "test-003");
        Assert.Equal(TestStatus.Skipped, leafResult.Status);
        Assert.Equal(ErrorCategory.Dependency, leafResult.Error?.Category);
    }

    [Fact]
    public async Task RunTestsAsync_OutOfOrderDependency_SkipsWithReason()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Instant",
                    DisplayName = "Dependent Test",
                    DependsOn = new List<string> { "test-001" }
                },
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Instant",
                    DisplayName = "Prerequisite Test",
                    DependsOn = new List<string>()
                }
            },
            RunSettings = new RunSettings()
        };

        var tests = new List<ITest> { new InstantTest() };
        var runner = new SequentialTestRunner(tests);

        // Act
        var report = await runner.RunTestsAsync(profile, null!, CancellationToken.None);
        var results = report.Results;

        // Assert
        Assert.Equal(2, results.Count);

        var dependentResult = results.First(r => r.TestId == "test-002");
        Assert.Equal(TestStatus.Skipped, dependentResult.Status);
        Assert.Equal(ErrorCategory.Dependency, dependentResult.Error?.Category);
        Assert.Contains("not yet been executed", dependentResult.Error?.Message);

        var prerequisiteResult = results.First(r => r.TestId == "test-001");
        Assert.Equal(TestStatus.Pass, prerequisiteResult.Status);
    }

    [Fact]
    public async Task RunTestsAsync_NoDependencies_AllTestsRun()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Instant",
                    DisplayName = "Test 1",
                    DependsOn = new List<string>()
                },
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Instant",
                    DisplayName = "Test 2",
                    DependsOn = new List<string>()
                },
                new TestDefinition
                {
                    Id = "test-003",
                    Type = "Instant",
                    DisplayName = "Test 3",
                    DependsOn = new List<string>()
                }
            },
            RunSettings = new RunSettings()
        };

        var tests = new List<ITest> { new InstantTest() };
        var runner = new SequentialTestRunner(tests);

        // Act
        var report = await runner.RunTestsAsync(profile, null!, CancellationToken.None);
        var results = report.Results;

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.Equal(TestStatus.Pass, r.Status));
    }
}
