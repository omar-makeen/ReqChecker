using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Tests.ViewModels;

public class SettingsViewModelTests
{
    private Mock<IPreferencesService> CreateMockPreferencesService(AppTheme theme = AppTheme.Dark, bool sidebarExpanded = true)
    {
        var mock = new Mock<IPreferencesService>();
        mock.SetupProperty(p => p.Theme, theme);
        mock.SetupProperty(p => p.SidebarExpanded, sidebarExpanded);
        return mock;
    }

    private ThemeService CreateThemeService(Mock<IPreferencesService> mockPrefs)
    {
        return new ThemeService(mockPrefs.Object);
    }

    [Fact]
    public void Constructor_InitialCurrentThemeReflectsThemeServiceValue()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Light);
        var themeService = CreateThemeService(mockPrefs);

        // Act
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);

        // Assert
        Assert.Equal(AppTheme.Light, viewModel.CurrentTheme);
    }

    [Fact]
    public void Constructor_InitialCurrentThemeIsDark_WhenThemeServiceIsDark()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Dark);
        var themeService = CreateThemeService(mockPrefs);

        // Act
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);

        // Assert
        Assert.Equal(AppTheme.Dark, viewModel.CurrentTheme);
    }

    [Fact]
    public void SelectDarkThemeCommand_SetsCurrentThemeToDark()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Light);
        var themeService = CreateThemeService(mockPrefs);
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);

        // Act
        viewModel.SelectDarkThemeCommand.Execute(null);

        // Assert
        Assert.Equal(AppTheme.Dark, viewModel.CurrentTheme);
    }

    [Fact]
    public void SelectDarkThemeCommand_CallsThemeServiceSetThemeWithDark()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Light);
        var themeService = CreateThemeService(mockPrefs);
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);

        // Act
        viewModel.SelectDarkThemeCommand.Execute(null);

        // Assert - ThemeService.CurrentTheme should be Dark
        Assert.Equal(AppTheme.Dark, themeService.CurrentTheme);
    }

    [Fact]
    public void SelectLightThemeCommand_SetsCurrentThemeToLight()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Dark);
        var themeService = CreateThemeService(mockPrefs);
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);

        // Act
        viewModel.SelectLightThemeCommand.Execute(null);

        // Assert
        Assert.Equal(AppTheme.Light, viewModel.CurrentTheme);
    }

    [Fact]
    public void SelectLightThemeCommand_CallsThemeServiceSetThemeWithLight()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Dark);
        var themeService = CreateThemeService(mockPrefs);
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);

        // Act
        viewModel.SelectLightThemeCommand.Execute(null);

        // Assert - ThemeService.CurrentTheme should be Light
        Assert.Equal(AppTheme.Light, themeService.CurrentTheme);
    }

    [Fact]
    public void AppVersion_IsNotNullOrEmpty()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService();
        var themeService = CreateThemeService(mockPrefs);

        // Act
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);

        // Assert
        Assert.False(string.IsNullOrEmpty(viewModel.AppVersion));
    }

    [Fact]
    public void AppVersion_IsNotUnknown()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService();
        var themeService = CreateThemeService(mockPrefs);

        // Act
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);

        // Assert
        Assert.NotEqual("Unknown", viewModel.AppVersion);
    }

    [Fact]
    public void SelectDarkThemeCommand_DoesNotChangeCurrentTheme_WhenAlreadyDark()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Dark);
        var themeService = CreateThemeService(mockPrefs);
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);
        var initialTheme = viewModel.CurrentTheme;

        // Act
        viewModel.SelectDarkThemeCommand.Execute(null);

        // Assert
        Assert.Equal(AppTheme.Dark, initialTheme);
        Assert.Equal(AppTheme.Dark, viewModel.CurrentTheme);
    }

    [Fact]
    public void SelectLightThemeCommand_DoesNotChangeCurrentTheme_WhenAlreadyLight()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Light);
        var themeService = CreateThemeService(mockPrefs);
        var viewModel = new SettingsViewModel(mockPrefs.Object, themeService);
        var initialTheme = viewModel.CurrentTheme;

        // Act
        viewModel.SelectLightThemeCommand.Execute(null);

        // Assert
        Assert.Equal(AppTheme.Light, initialTheme);
        Assert.Equal(AppTheme.Light, viewModel.CurrentTheme);
    }
}
