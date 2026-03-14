using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Models;

namespace ReqChecker.App.Tests.ViewModels;

public class MainViewModelTests
{
    private static MainViewModel CreateViewModel(
        Mock<IAppState>? mockAppState = null,
        Mock<IPreferencesService>? mockPreferencesService = null,
        Profile? profile = null)
    {
        mockAppState ??= new Mock<IAppState>();
        mockPreferencesService ??= new Mock<IPreferencesService>();

        if (profile != null)
        {
            mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);
        }

        return new MainViewModel(mockPreferencesService.Object, mockAppState.Object);
    }

    private static Profile CreateProfileWithTests(string name, int testCount)
    {
        var tests = new List<TestDefinition>();
        for (var i = 0; i < testCount; i++)
        {
            tests.Add(new TestDefinition
            {
                Id = $"test-{i}",
                DisplayName = $"Test {i}",
                Type = "HttpGet"
            });
        }

        return new Profile
        {
            Name = name,
            SchemaVersion = 2,
            Tests = tests
        };
    }

    [Fact]
    public void TestCount_ShouldReturnCount_WhenProfileLoaded()
    {
        var profile = CreateProfileWithTests("My Profile", 5);
        var viewModel = CreateViewModel(profile: profile);

        Assert.Equal(5, viewModel.TestCount);
    }

    [Fact]
    public void TestCount_ShouldReturnZero_WhenNoProfile()
    {
        var viewModel = CreateViewModel();

        Assert.Equal(0, viewModel.TestCount);
    }

    [Fact]
    public void HasTests_ShouldReturnTrue_WhenProfileHasTests()
    {
        var profile = CreateProfileWithTests("My Profile", 3);
        var viewModel = CreateViewModel(profile: profile);

        Assert.True(viewModel.HasTests);
    }

    [Fact]
    public void HasTests_ShouldReturnFalse_WhenNoProfile()
    {
        var viewModel = CreateViewModel();

        Assert.False(viewModel.HasTests);
    }

    [Fact]
    public void HasTests_ShouldReturnFalse_WhenProfileHasNoTests()
    {
        var profile = CreateProfileWithTests("Empty Profile", 0);
        var viewModel = CreateViewModel(profile: profile);

        Assert.False(viewModel.HasTests);
    }

    [Fact]
    public void TestCount_ShouldUpdate_WhenProfileChanges()
    {
        var mockAppState = new Mock<IAppState>();
        var viewModel = CreateViewModel(mockAppState: mockAppState);

        Assert.Equal(0, viewModel.TestCount);

        // Simulate profile change via the CurrentProfileChanged event
        var newProfile = CreateProfileWithTests("New Profile", 7);
        mockAppState.SetupGet(x => x.CurrentProfile).Returns(newProfile);
        mockAppState.Raise(x => x.CurrentProfileChanged += null, EventArgs.Empty);

        Assert.Equal(7, viewModel.TestCount);
        Assert.True(viewModel.HasTests);
    }

    [Fact]
    public void ProfileName_ShouldReturnName_WhenProfileLoaded()
    {
        var profile = CreateProfileWithTests("Production Tests", 1);
        var viewModel = CreateViewModel(profile: profile);

        Assert.Equal("Production Tests", viewModel.ProfileName);
    }

    [Fact]
    public void ProfileName_ShouldReturnDefault_WhenNoProfile()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("No profile loaded", viewModel.ProfileName);
    }

    [Fact]
    public void HasProfile_ShouldReturnTrue_WhenProfileLoaded()
    {
        var profile = CreateProfileWithTests("My Profile", 1);
        var viewModel = CreateViewModel(profile: profile);

        Assert.True(viewModel.HasProfile);
    }

    [Fact]
    public void HasProfile_ShouldReturnFalse_WhenNoProfile()
    {
        var viewModel = CreateViewModel();

        Assert.False(viewModel.HasProfile);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        var viewModel = CreateViewModel();

        var exception = Record.Exception(() => viewModel.Dispose());

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var viewModel = CreateViewModel();

        viewModel.Dispose();
        viewModel.Dispose();
        viewModel.Dispose();

        Assert.NotNull(viewModel);
    }

    [Fact]
    public void IsSidebarExpanded_ShouldPersistToPreferencesService()
    {
        var mockPreferencesService = new Mock<IPreferencesService>();
        mockPreferencesService.SetupGet(x => x.SidebarExpanded).Returns(true);

        var viewModel = CreateViewModel(mockPreferencesService: mockPreferencesService);

        viewModel.IsSidebarExpanded = false;

        mockPreferencesService.VerifySet(x => x.SidebarExpanded = false, Times.Once);
    }

    [Fact]
    public void ThemeService_CanBeSet()
    {
        var mockPreferencesService = new Mock<IPreferencesService>();
        var themeService = new ThemeService(mockPreferencesService.Object);
        var viewModel = CreateViewModel(mockPreferencesService: mockPreferencesService);

        viewModel.ThemeService = themeService;

        Assert.Equal(themeService, viewModel.ThemeService);
    }

    [Fact]
    public void ThemeLabel_WhenDarkTheme_ShouldShowLightMode()
    {
        var mockPreferencesService = new Mock<IPreferencesService>();
        mockPreferencesService.SetupGet(x => x.Theme).Returns(AppTheme.Dark);
        var themeService = new ThemeService(mockPreferencesService.Object);
        var viewModel = CreateViewModel(mockPreferencesService: mockPreferencesService);
        viewModel.ThemeService = themeService;

        Assert.Equal("Light Mode", viewModel.ThemeLabel);
    }

    [Fact]
    public void ThemeLabel_WhenLightTheme_ShouldShowDarkMode()
    {
        var mockPreferencesService = new Mock<IPreferencesService>();
        mockPreferencesService.SetupGet(x => x.Theme).Returns(AppTheme.Light);
        var themeService = new ThemeService(mockPreferencesService.Object);
        var viewModel = CreateViewModel(mockPreferencesService: mockPreferencesService);
        viewModel.ThemeService = themeService;

        Assert.Equal("Dark Mode", viewModel.ThemeLabel);
    }
}
