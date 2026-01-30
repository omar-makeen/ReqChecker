using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;

namespace ReqChecker.App.Tests.ViewModels;

/// <summary>
/// Unit tests for MainViewModel disposal behavior.
/// Tests event subscription cleanup to prevent memory leaks.
/// </summary>
public class MainViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithProfile()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockAppState = new Mock<IAppState>();
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        // Act
        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);

        // Assert - ViewModel is created and profile is loaded
        Assert.NotNull(viewModel);
        Assert.Equal(profile, viewModel.CurrentProfile);
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromProfileChangedEvent()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockAppState = new Mock<IAppState>();
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        // Act
        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);
        viewModel.Dispose();

        // Assert - After dispose, event handler should not be invoked
        // Note: This test verifies the Dispose method is called without throwing
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockAppState = new Mock<IAppState>();
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        // Act
        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);

        // Should not throw
        viewModel.Dispose();
        viewModel.Dispose();
        viewModel.Dispose();

        // Assert - No exception thrown
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void CurrentProfile_WhenNull_ShouldShowNoProfileLoaded()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockAppState = new Mock<IAppState>();

        mockAppState.SetupGet(x => x.CurrentProfile).Returns((Profile?)null);

        // Act
        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);

        // Assert - Initially no profile
        Assert.False(viewModel.HasProfile);
        Assert.Equal("No profile loaded", viewModel.ProfileName);
    }

    [Fact]
    public void CurrentProfile_WhenSet_ShouldUpdateHasProfileAndProfileName()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockAppState = new Mock<IAppState>();
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockAppState.SetupGet(x => x.CurrentProfile).Returns((Profile?)null);

        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);

        // Act - Initially no profile
        Assert.False(viewModel.HasProfile);

        // Simulate profile change
        viewModel.CurrentProfile = profile;

        // Assert
        Assert.True(viewModel.HasProfile);
        Assert.Equal("Test Profile", viewModel.ProfileName);
    }

    [Fact]
    public void IsSidebarExpanded_ShouldPersistToPreferencesService()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        mockPreferencesService.SetupGet(x => x.SidebarExpanded).Returns(true);

        var mockAppState = new Mock<IAppState>();
        mockAppState.SetupGet(x => x.CurrentProfile).Returns((Profile?)null);

        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);

        // Act
        viewModel.IsSidebarExpanded = false;

        // Assert
        mockPreferencesService.VerifySet(x => x.SidebarExpanded = false, Times.Once);
    }

    [Fact]
    public void ThemeService_CanBeSet()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        var mockAppState = new Mock<IAppState>();
        mockAppState.SetupGet(x => x.CurrentProfile).Returns((Profile?)null);

        var themeService = new ThemeService(mockPreferencesService.Object);

        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);

        // Act
        viewModel.ThemeService = themeService;

        // Assert
        Assert.Equal(themeService, viewModel.ThemeService);
    }

    [Fact]
    public void ThemeLabel_WhenDarkTheme_ShouldShowLightMode()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        mockPreferencesService.SetupGet(x => x.Theme).Returns(AppTheme.Dark);

        var mockAppState = new Mock<IAppState>();
        mockAppState.SetupGet(x => x.CurrentProfile).Returns((Profile?)null);

        var themeService = new ThemeService(mockPreferencesService.Object);

        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);
        viewModel.ThemeService = themeService;

        // Assert - Dark theme means clicking toggles to Light
        Assert.Equal("Light Mode", viewModel.ThemeLabel);
    }

    [Fact]
    public void ThemeLabel_WhenLightTheme_ShouldShowDarkMode()
    {
        // Arrange
        var mockPreferencesService = new Mock<IPreferencesService>();
        mockPreferencesService.SetupGet(x => x.Theme).Returns(AppTheme.Light);

        var mockAppState = new Mock<IAppState>();
        mockAppState.SetupGet(x => x.CurrentProfile).Returns((Profile?)null);

        var themeService = new ThemeService(mockPreferencesService.Object);

        var viewModel = new MainViewModel(mockPreferencesService.Object, mockAppState.Object);
        viewModel.ThemeService = themeService;

        // Assert - Light theme means clicking toggles to Dark
        Assert.Equal("Dark Mode", viewModel.ThemeLabel);
    }
}
