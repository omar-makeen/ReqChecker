using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Export;

namespace ReqChecker.App.Tests.ViewModels;

/// <summary>
/// Unit tests for ResultsViewModel filtering and export functionality.
/// </summary>
public class ResultsViewModelTests
{
    private static JsonExporter CreateJsonExporter() => new JsonExporter();
    private static CsvExporter CreateCsvExporter() => new CsvExporter();
    private static PdfExporter CreatePdfExporter() => new PdfExporter();

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        // Act
        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);

        // Assert
        Assert.NotNull(viewModel);
        Assert.Null(viewModel.Report);
        Assert.Equal(ResultsFilter.All, viewModel.ActiveFilter);
        Assert.Null(viewModel.FilteredResults);
    }

    [Fact]
    public void OnReportChanged_ShouldSetupFilteredResults()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>
            {
                new TestResult { DisplayName = "Test 1", Status = TestStatus.Pass },
                new TestResult { DisplayName = "Test 2", Status = TestStatus.Fail },
                new TestResult { DisplayName = "Test 3", Status = TestStatus.Skipped }
            }
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);

        // Act
        viewModel.Report = report;

        // Assert
        Assert.NotNull(viewModel.FilteredResults);
        Assert.Equal(3, viewModel.FilteredResults!.Cast<TestResult>().Count());
        mockAppState.Verify(x => x.SetLastRunReport(report), Times.Once);
    }

    [Fact]
    public void SetFilter_All_ShouldShowAllResults()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>
            {
                new TestResult { DisplayName = "Test 1", Status = TestStatus.Pass },
                new TestResult { DisplayName = "Test 2", Status = TestStatus.Fail },
                new TestResult { DisplayName = "Test 3", Status = TestStatus.Skipped }
            }
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);
        viewModel.Report = report;

        // Act
        viewModel.SetFilterCommand.Execute(ResultsFilter.All);

        // Assert
        Assert.Equal(ResultsFilter.All, viewModel.ActiveFilter);
        Assert.Equal(3, viewModel.FilteredResults!.Cast<TestResult>().Count());
    }

    [Fact]
    public void SetFilter_Passed_ShouldShowOnlyPassedResults()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>
            {
                new TestResult { DisplayName = "Test 1", Status = TestStatus.Pass },
                new TestResult { DisplayName = "Test 2", Status = TestStatus.Fail },
                new TestResult { DisplayName = "Test 3", Status = TestStatus.Pass },
                new TestResult { DisplayName = "Test 4", Status = TestStatus.Skipped }
            }
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);
        viewModel.Report = report;

        // Act
        viewModel.SetFilterCommand.Execute(ResultsFilter.Passed);

        // Assert
        Assert.Equal(ResultsFilter.Passed, viewModel.ActiveFilter);
        var filtered = viewModel.FilteredResults!.Cast<TestResult>().ToList();
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, r => Assert.Equal(TestStatus.Pass, r.Status));
    }

    [Fact]
    public void SetFilter_Failed_ShouldShowOnlyFailedResults()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>
            {
                new TestResult { DisplayName = "Test 1", Status = TestStatus.Pass },
                new TestResult { DisplayName = "Test 2", Status = TestStatus.Fail },
                new TestResult { DisplayName = "Test 3", Status = TestStatus.Fail },
                new TestResult { DisplayName = "Test 4", Status = TestStatus.Skipped }
            }
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);
        viewModel.Report = report;

        // Act
        viewModel.SetFilterCommand.Execute(ResultsFilter.Failed);

        // Assert
        Assert.Equal(ResultsFilter.Failed, viewModel.ActiveFilter);
        var filtered = viewModel.FilteredResults!.Cast<TestResult>().ToList();
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, r => Assert.Equal(TestStatus.Fail, r.Status));
    }

    [Fact]
    public void SetFilter_Skipped_ShouldShowOnlySkippedResults()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>
            {
                new TestResult { DisplayName = "Test 1", Status = TestStatus.Pass },
                new TestResult { DisplayName = "Test 2", Status = TestStatus.Skipped },
                new TestResult { DisplayName = "Test 3", Status = TestStatus.Skipped },
                new TestResult { DisplayName = "Test 4", Status = TestStatus.Fail }
            }
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);
        viewModel.Report = report;

        // Act
        viewModel.SetFilterCommand.Execute(ResultsFilter.Skipped);

        // Assert
        Assert.Equal(ResultsFilter.Skipped, viewModel.ActiveFilter);
        var filtered = viewModel.FilteredResults!.Cast<TestResult>().ToList();
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, r => Assert.Equal(TestStatus.Skipped, r.Status));
    }

    [Fact]
    public void NavigateToTestListCommand_Exists()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);

        // Assert
        Assert.NotNull(viewModel.NavigateToTestListCommand);
    }

    [Fact]
    public async Task ExportToJsonAsync_ShouldDoNothingWhenDialogServiceIsNull()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>()
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object,
            null); // No dialog service
        viewModel.Report = report;

        // Act - Should not throw
        var exception = await Record.ExceptionAsync(() => viewModel.ExportToJsonCommand.ExecuteAsync(null));

        // Assert - No exception thrown
        Assert.Null(exception);
    }

    [Fact]
    public async Task ExportToJsonAsync_ShouldDoNothingWhenReportIsNull()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var dialogService = new DialogService();

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object,
            navigationService,
            dialogService);
        viewModel.Report = null;

        // Act - Should not throw
        var exception = await Record.ExceptionAsync(() => viewModel.ExportToJsonCommand.ExecuteAsync(null));

        // Assert - No exception thrown
        Assert.Null(exception);
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldDoNothingWhenDialogServiceIsNull()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>()
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object,
            null); // No dialog service
        viewModel.Report = report;

        // Act - Should not throw
        var exception = await Record.ExceptionAsync(() => viewModel.ExportToCsvCommand.ExecuteAsync(null));

        // Assert - No exception thrown
        Assert.Null(exception);
    }

    [Fact]
    public async Task ExportToCsvAsync_ShouldDoNothingWhenReportIsNull()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var dialogService = new DialogService();

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object,
            navigationService,
            dialogService);
        viewModel.Report = null;

        // Act - Should not throw
        var exception = await Record.ExceptionAsync(() => viewModel.ExportToCsvCommand.ExecuteAsync(null));

        // Assert - No exception thrown
        Assert.Null(exception);
    }

    [Fact]
    public void ActiveFilter_CanBeSet()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>()
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);
        viewModel.Report = report;

        // Act
        viewModel.ActiveFilter = ResultsFilter.Failed;

        // Assert
        Assert.Equal(ResultsFilter.Failed, viewModel.ActiveFilter);
    }

    [Fact]
    public void CanExport_ReturnsTrueWhenReportIsSet()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>()
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);

        // Act
        viewModel.Report = report;

        // Assert
        Assert.True(viewModel.CanExport);
    }

    [Fact]
    public void CanExport_ReturnsFalseWhenReportIsNull()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);

        // Act - Report is null by default
        var canExport = viewModel.CanExport;

        // Assert
        Assert.False(canExport);
    }

    [Fact]
    public void SettingReport_RaisesPropertyChangedForCanExport()
    {
        // Arrange
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var report = new RunReport
        {
            RunId = "test-run-id",
            Results = new List<TestResult>()
        };

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act
        viewModel.Report = report;

        // Assert
        Assert.Contains(nameof(viewModel.CanExport), propertyChangedEvents);
    }

    [Fact]
    public void SettingReportToNull_RaisesPropertyChangedForCanExport()
    {
        // Arrange - simulates NavigationService loading Report from AppState when null
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);

        // Set initial report
        viewModel.Report = new RunReport { RunId = "initial" };

        var propertyChangedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        // Act - Set to null (simulates navigating when no report in AppState)
        viewModel.Report = null;

        // Assert - CanExport should update even when set to null
        Assert.Contains(nameof(viewModel.CanExport), propertyChangedEvents);
        Assert.False(viewModel.CanExport);
    }

    [Fact]
    public void SettingReportFromAppState_ConfiguresViewModelCorrectly()
    {
        // Arrange - simulates what NavigationService.NavigateToResults() does
        var jsonExporter = CreateJsonExporter();
        var csvExporter = CreateCsvExporter();
        var mockAppState = new Mock<IAppState>();

        var expectedReport = new RunReport
        {
            RunId = "from-appstate",
            Results = new List<TestResult>
            {
                new TestResult { DisplayName = "Test 1", Status = TestStatus.Pass }
            }
        };

        // Simulate AppState having a report (set before navigation)
        mockAppState.SetupGet(x => x.LastRunReport).Returns(expectedReport);

        var viewModel = new ResultsViewModel(
            jsonExporter,
            csvExporter,
            CreatePdfExporter(),
            mockAppState.Object);

        // Act - Simulate what NavigationService does: assign Report from AppState
        viewModel.Report = mockAppState.Object.LastRunReport;

        // Assert - Verify the integration behavior
        Assert.Equal(expectedReport, viewModel.Report);
        Assert.Equal("from-appstate", viewModel.Report?.RunId);
        Assert.True(viewModel.CanExport);
        Assert.NotNull(viewModel.FilteredResults);
        Assert.Single(viewModel.FilteredResults.Cast<TestResult>());
    }
}
