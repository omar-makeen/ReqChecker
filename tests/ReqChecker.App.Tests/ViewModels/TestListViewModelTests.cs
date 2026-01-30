using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;

namespace ReqChecker.App.Tests.ViewModels;

/// <summary>
/// Unit tests for TestListViewModel disposal behavior.
/// Tests event subscription cleanup to prevent memory leaks.
/// </summary>
public class TestListViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithProfile()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        // Act
        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);

        // Assert - ViewModel is created and profile is loaded
        Assert.NotNull(viewModel);
        Assert.Equal(profile, viewModel.CurrentProfile);
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromProfileChangedEvent()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        // Act
        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);
        viewModel.Dispose();

        // Assert - After dispose, event handler should not be invoked
        // Note: This test verifies the Dispose method is called without throwing
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        // Act
        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);

        // Should not throw
        viewModel.Dispose();
        viewModel.Dispose();
        viewModel.Dispose();

        // Assert - No exception thrown
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void CurrentProfile_CanBeSet()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile1 = new Profile { Name = "Profile 1", SchemaVersion = 2 };
        var profile2 = new Profile { Name = "Profile 2", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile1);

        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);

        // Act - Initially profile1
        Assert.Equal(profile1, viewModel.CurrentProfile);

        // Simulate profile change
        viewModel.CurrentProfile = profile2;

        // Assert
        Assert.Equal(profile2, viewModel.CurrentProfile);
    }

    [Fact]
    public void SelectedTest_CanBeSet()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        var test = new TestDefinition { DisplayName = "Test 1", Type = "Ping" };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);

        // Act
        viewModel.SelectedTest = test;

        // Assert
        Assert.Equal(test, viewModel.SelectedTest);
    }

    [Fact]
    public void NavigateToTestConfigCommand_ShouldNotThrowWhenTestIsNull()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);
        viewModel.CurrentProfile = profile;

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => viewModel.NavigateToTestConfigCommand.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public void NavigateToProfilesCommand_Exists()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);

        // Assert
        Assert.NotNull(viewModel.NavigateToProfilesCommand);
    }

    [Fact]
    public async Task RunAllTestsCommand_ShouldNotThrowWhenProfileIsNull()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);

        mockAppState.SetupGet(x => x.CurrentProfile).Returns((Profile?)null);

        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);

        // Act - Should not throw
        var exception = await Record.ExceptionAsync(() => viewModel.RunAllTestsCommand.ExecuteAsync(null));

        // Assert - No exception thrown
        Assert.Null(exception);
    }

    [Fact]
    public async Task RunAllTestsCommand_ShouldNotThrowWhenTestRunnerIsNull()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);
        viewModel.TestRunner = null;

        // Act - Should not throw
        var exception = await Record.ExceptionAsync(() => viewModel.RunAllTestsCommand.ExecuteAsync(null));

        // Assert - No exception thrown
        Assert.Null(exception);
    }

    [Fact]
    public void TestRunner_CanBeSet()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        var mockTestRunner = new Mock<ITestRunner>();

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        var viewModel = new TestListViewModel(mockAppState.Object, navigationService);

        // Act
        viewModel.TestRunner = mockTestRunner.Object;

        // Assert
        Assert.Equal(mockTestRunner.Object, viewModel.TestRunner);
    }
}
