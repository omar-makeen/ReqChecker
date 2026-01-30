using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using ReqChecker.Infrastructure.Execution;
using ReqChecker.Infrastructure.Tests;
using System.Diagnostics;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.Tests.Execution;

/// <summary>
/// Unit tests for SequentialTestRunner inter-test delay behavior.
/// </summary>
public class SequentialTestRunnerTests
{
    /// <summary>
    /// A stub test that completes instantly for timing measurements.
    /// </summary>
    [TestType("InstantTest")]
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
                Duration = TimeSpan.Zero
            });
        }
    }

    private static ProfileModel CreateProfileWithTests(int testCount)
    {
        var tests = new List<TestDefinition>();
        for (int i = 0; i < testCount; i++)
        {
            tests.Add(new TestDefinition
            {
                Id = $"test-{i}",
                Type = "InstantTest",
                DisplayName = $"Test {i}"
            });
        }

        return new ProfileModel
        {
            Id = "test-profile",
            Name = "Test Profile",
            Tests = tests
        };
    }

    [Fact]
    public async Task RunTestsAsync_WithDelay_AppliesDelayBetweenTests()
    {
        // Arrange
        var runner = new SequentialTestRunner(new[] { new InstantTest() });
        var profile = CreateProfileWithTests(3);
        var runSettings = new RunSettings { InterTestDelayMs = 100 };
        var progress = new Progress<TestResult>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        await runner.RunTestsAsync(profile, progress, CancellationToken.None, runSettings);
        stopwatch.Stop();

        // Assert: With 3 tests, expect 2 delays of 100ms each = 200ms minimum
        Assert.True(stopwatch.ElapsedMilliseconds >= 180,
            $"Expected at least 180ms (2 delays of ~100ms), but got {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task RunTestsAsync_WithDelay_NoDelayAfterLastTest()
    {
        // Arrange
        var runner = new SequentialTestRunner(new[] { new InstantTest() });
        var profile = CreateProfileWithTests(2);
        var runSettings = new RunSettings { InterTestDelayMs = 100 };
        var progress = new Progress<TestResult>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        await runner.RunTestsAsync(profile, progress, CancellationToken.None, runSettings);
        stopwatch.Stop();

        // Assert: With 2 tests, expect 1 delay of 100ms (not 2)
        // Should be around 100ms, not 200ms
        Assert.True(stopwatch.ElapsedMilliseconds >= 80 && stopwatch.ElapsedMilliseconds < 180,
            $"Expected ~100ms (1 delay), but got {stopwatch.ElapsedMilliseconds}ms. Should not delay after last test.");
    }

    [Fact]
    public async Task RunTestsAsync_WithZeroDelay_NoDelayApplied()
    {
        // Arrange
        var runner = new SequentialTestRunner(new[] { new InstantTest() });
        var profile = CreateProfileWithTests(3);
        var runSettings = new RunSettings { InterTestDelayMs = 0 };
        var progress = new Progress<TestResult>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        await runner.RunTestsAsync(profile, progress, CancellationToken.None, runSettings);
        stopwatch.Stop();

        // Assert: With 0ms delay, should complete quickly (under 50ms for 3 instant tests)
        Assert.True(stopwatch.ElapsedMilliseconds < 50,
            $"Expected quick completion with zero delay, but got {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task RunTestsAsync_CancellationDuringDelay_CancelsImmediately()
    {
        // Arrange
        var runner = new SequentialTestRunner(new[] { new InstantTest() });
        var profile = CreateProfileWithTests(3);
        var runSettings = new RunSettings { InterTestDelayMs = 5000 }; // Long delay
        var progress = new Progress<TestResult>();
        var cts = new CancellationTokenSource();
        var stopwatch = Stopwatch.StartNew();

        // Cancel after a short delay (after first test completes, during first inter-test delay)
        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            cts.Cancel();
        });

        // Act & Assert (TaskCanceledException inherits from OperationCanceledException)
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await runner.RunTestsAsync(profile, progress, cts.Token, runSettings);
        });
        stopwatch.Stop();

        // Should cancel quickly, not wait for the full 5000ms delay
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Expected quick cancellation, but got {stopwatch.ElapsedMilliseconds}ms. Cancellation during delay should be immediate.");
    }
}
