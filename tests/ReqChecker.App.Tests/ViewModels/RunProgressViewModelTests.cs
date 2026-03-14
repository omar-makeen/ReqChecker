using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.History;
using System.Reflection;

namespace ReqChecker.App.Tests.ViewModels;

public class RunProgressViewModelTests
{
    private static RunProgressViewModel CreateViewModel(Profile? profile = null, List<string>? selectedTestIds = null)
    {
        var mockAppState = new Mock<IAppState>();
        var mockTestRunner = new Mock<ITestRunner>();
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockHistoryService = new Mock<IHistoryService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);
        if (selectedTestIds != null)
        {
            mockAppState.SetupGet(x => x.SelectedTestIds).Returns(selectedTestIds);
        }

        return new RunProgressViewModel(
            mockAppState.Object,
            mockTestRunner.Object,
            navigationService,
            mockPreferencesService.Object,
            mockHistoryService.Object);
    }

    private static void SetPrivateField(RunProgressViewModel viewModel, string fieldName, object? value)
    {
        var field = typeof(RunProgressViewModel).GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(viewModel, value);
    }

    private static T? GetPrivateField<T>(RunProgressViewModel viewModel, string fieldName)
    {
        var field = typeof(RunProgressViewModel).GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (T?)field?.GetValue(viewModel);
    }

    private static void InvokePrivateMethod(RunProgressViewModel viewModel, string methodName)
    {
        var method = typeof(RunProgressViewModel).GetMethod(methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(viewModel, null);
    }

    [Fact]
    public void RunProgressViewModel_FiltersProfile_UsingSelectedTestIds()
    {
        var profile = new Profile
        {
            Name = "Test Profile",
            SchemaVersion = 2,
            Tests =
            [
                new TestDefinition { Id = "t1", DisplayName = "Test 1", Type = "Ping" },
                new TestDefinition { Id = "t2", DisplayName = "Test 2", Type = "Ping" },
                new TestDefinition { Id = "t3", DisplayName = "Test 3", Type = "Ping" },
                new TestDefinition { Id = "t4", DisplayName = "Test 4", Type = "Ping" }
            ]
        };

        var mockAppState = new Mock<IAppState>();
        var mockTestRunner = new Mock<ITestRunner>();
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockHistoryService = new Mock<IHistoryService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);
        mockAppState.SetupGet(x => x.SelectedTestIds).Returns(new List<string> { "t1", "t3" });

        var viewModel = new RunProgressViewModel(
            mockAppState.Object,
            mockTestRunner.Object,
            navigationService,
            mockPreferencesService.Object,
            mockHistoryService.Object);

        Assert.Equal(2, viewModel.TotalTests);
        mockAppState.Verify(x => x.SetSelectedTestIds(null), Times.Once);
    }

    [Theory]
    [InlineData(0, 5, "Test 1 of 5")]
    [InlineData(2, 10, "Test 3 of 10")]
    [InlineData(9, 10, "Test 10 of 10")]
    public void TestPositionText_ShouldShowCorrectPosition_DuringExecution(int currentIndex, int totalTests, string expected)
    {
        var viewModel = CreateViewModel();
        viewModel.TotalTests = totalTests;
        viewModel.CurrentTestIndex = currentIndex;
        SetPrivateField(viewModel, "_isRunning", true);

        Assert.Equal(expected, viewModel.TestPositionText);
    }

    [Fact]
    public void TestPositionText_ShouldUpdate_WhenCurrentTestIndexChanges()
    {
        var viewModel = CreateViewModel();
        viewModel.TotalTests = 5;
        SetPrivateField(viewModel, "_isRunning", true);

        viewModel.CurrentTestIndex = 0;
        Assert.Equal("Test 1 of 5", viewModel.TestPositionText);

        viewModel.CurrentTestIndex = 2;
        Assert.Equal("Test 3 of 5", viewModel.TestPositionText);

        viewModel.CurrentTestIndex = 4;
        Assert.Equal("Test 5 of 5", viewModel.TestPositionText);
    }

    [Fact]
    public void TestPositionText_ShouldBeEmpty_WhenNotRunning()
    {
        var viewModel = CreateViewModel();
        viewModel.TotalTests = 5;
        viewModel.CurrentTestIndex = 2;

        Assert.Equal(string.Empty, viewModel.TestPositionText);
    }

    [Fact]
    public void AutoNavTimer_ShouldNotStart_WhenRunCancelled()
    {
        var viewModel = CreateViewModel();
        SetPrivateField(viewModel, "_isCancelling", true);
        SetPrivateField(viewModel, "_runReport", new RunReport());

        InvokePrivateMethod(viewModel, "OnCompletion");

        var timer = GetPrivateField<object>(viewModel, "_autoNavTimer");
        Assert.Null(timer);
    }

    [Fact]
    public void ViewResults_ShouldStopAutoNavTimer()
    {
        var mockAppState = new Mock<IAppState>();
        var mockTestRunner = new Mock<ITestRunner>();
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockHistoryService = new Mock<IHistoryService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);

        mockAppState.SetupGet(x => x.CurrentProfile).Returns((Profile?)null);
        mockAppState.Setup(x => x.SetLastRunReport(It.IsAny<RunReport>()));

        var viewModel = new RunProgressViewModel(
            mockAppState.Object,
            mockTestRunner.Object,
            navigationService,
            mockPreferencesService.Object,
            mockHistoryService.Object);

        SetPrivateField(viewModel, "_runReport", new RunReport());
        InvokePrivateMethod(viewModel, "StartAutoNavigationTimer");

        var timerBefore = GetPrivateField<object>(viewModel, "_autoNavTimer");
        Assert.NotNull(timerBefore);

        viewModel.ViewResultsCommand.Execute(null);

        var timerAfter = GetPrivateField<object>(viewModel, "_autoNavTimer");
        Assert.Null(timerAfter);
    }

    [Fact]
    public void NavigateToTestList_ShouldStopAutoNavTimer()
    {
        var viewModel = CreateViewModel();
        SetPrivateField(viewModel, "_runReport", new RunReport());
        InvokePrivateMethod(viewModel, "StartAutoNavigationTimer");

        var timerBefore = GetPrivateField<object>(viewModel, "_autoNavTimer");
        Assert.NotNull(timerBefore);

        viewModel.NavigateToTestListCommand.Execute(null);

        var timerAfter = GetPrivateField<object>(viewModel, "_autoNavTimer");
        Assert.Null(timerAfter);
    }

    [Fact]
    public void Dispose_ShouldStopAutoNavTimer()
    {
        var viewModel = CreateViewModel();
        SetPrivateField(viewModel, "_runReport", new RunReport());
        InvokePrivateMethod(viewModel, "StartAutoNavigationTimer");

        var timerBefore = GetPrivateField<object>(viewModel, "_autoNavTimer");
        Assert.NotNull(timerBefore);

        viewModel.Dispose();

        var timerAfter = GetPrivateField<object>(viewModel, "_autoNavTimer");
        Assert.Null(timerAfter);
    }

    [Fact]
    public void CompletionSummaryText_ShouldShowAllPassed_WhenNoFailuresOrSkips()
    {
        var viewModel = CreateViewModel();
        viewModel.TotalTests = 5;
        viewModel.CompletedTests = 5;
        viewModel.FailedTests = 0;
        viewModel.SkippedTests = 0;

        Assert.Equal("All 5 tests passed", viewModel.CompletionSummaryText);
    }

    [Fact]
    public void CompletionSummaryText_ShouldShowBreakdown_WhenMixedResults()
    {
        var viewModel = CreateViewModel();
        viewModel.TotalTests = 5;
        viewModel.CompletedTests = 3;
        viewModel.FailedTests = 1;
        viewModel.SkippedTests = 1;

        Assert.Equal("3 passed, 1 failed, 1 skipped", viewModel.CompletionSummaryText);
    }

    [Fact]
    public void CompletionSummaryText_ShouldShowAllSkipped_WhenAllSkipped()
    {
        var viewModel = CreateViewModel();
        viewModel.TotalTests = 5;
        viewModel.CompletedTests = 0;
        viewModel.FailedTests = 0;
        viewModel.SkippedTests = 5;

        Assert.Equal("0 passed, 0 failed, 5 skipped", viewModel.CompletionSummaryText);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    public void HasFailures_ShouldReflectFailedTestsCount(int failedTests, bool expectedHasFailures)
    {
        var viewModel = CreateViewModel();
        viewModel.FailedTests = failedTests;

        Assert.Equal(expectedHasFailures, viewModel.HasFailures);
    }

    [Theory]
    [InlineData(0, 5, "Test 1 of 5")]
    [InlineData(2, 10, "Test 3 of 10")]
    [InlineData(9, 10, "Test 10 of 10")]
    public void ProgressLabel_ShouldShowTestPosition_WhenRunning(int currentIndex, int totalTests, string expected)
    {
        var viewModel = CreateViewModel();
        viewModel.TotalTests = totalTests;
        viewModel.CurrentTestIndex = currentIndex;
        SetPrivateField(viewModel, "_isRunning", true);

        Assert.Equal(expected, viewModel.ProgressLabel);
    }

    [Fact]
    public void ProgressLabel_ShouldShowComplete_WhenNotRunning()
    {
        var viewModel = CreateViewModel();
        viewModel.TotalTests = 5;
        viewModel.CurrentTestIndex = 2;

        Assert.Equal("Complete", viewModel.ProgressLabel);
    }
}
