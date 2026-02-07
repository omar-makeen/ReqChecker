using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.History;

namespace ReqChecker.App.Tests.ViewModels;

public class RunProgressViewModelTests
{
    [Fact]
    public void RunProgressViewModel_FiltersProfile_UsingSelectedTestIds()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockTestRunner = new Mock<ITestRunner>();
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockHistoryService = new Mock<IHistoryService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);

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

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);
        mockAppState.SetupGet(x => x.SelectedTestIds).Returns(new List<string> { "t1", "t3" });

        // Act
        var viewModel = new RunProgressViewModel(
            mockAppState.Object,
            mockTestRunner.Object,
            navigationService,
            mockPreferencesService.Object,
            mockHistoryService.Object);

        // Assert - Only the 2 selected tests should remain
        Assert.Equal(2, viewModel.TotalTests);

        // Assert - SelectedTestIds consumed (cleared after use)
        mockAppState.Verify(x => x.SetSelectedTestIds(null), Times.Once);
    }
}
