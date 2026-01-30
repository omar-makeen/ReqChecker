using ReqChecker.App.Services;

namespace ReqChecker.App.Tests.Services;

public class PreferencesServiceTests
{
    [Fact]
    public void DefaultTheme_ShouldBeDark()
    {
        // Arrange & Act
        var service = new PreferencesService();

        // Assert - default theme should be Dark
        Assert.Equal(AppTheme.Dark, service.Theme);
    }

    [Fact]
    public void DefaultSidebarExpanded_ShouldBeTrue()
    {
        // Arrange & Act
        var service = new PreferencesService();

        // Assert - sidebar should be expanded by default
        Assert.True(service.SidebarExpanded);
    }

    [Fact]
    public void SetTheme_ShouldUpdateProperty()
    {
        // Arrange
        var service = new PreferencesService();

        // Act
        service.Theme = AppTheme.Light;

        // Assert
        Assert.Equal(AppTheme.Light, service.Theme);
    }

    [Fact]
    public void SetSidebarExpanded_ShouldUpdateProperty()
    {
        // Arrange
        var service = new PreferencesService();

        // Act
        service.SidebarExpanded = false;

        // Assert
        Assert.False(service.SidebarExpanded);
    }

    [Fact]
    public void UserPreferences_ShouldSerializeCorrectly()
    {
        // Arrange
        var prefs = new UserPreferences
        {
            Theme = "Light",
            SidebarExpanded = false,
            LastUpdated = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Equal("Light", prefs.Theme);
        Assert.False(prefs.SidebarExpanded);
    }

    [Fact]
    public void UserPreferences_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var prefs = new UserPreferences();

        // Assert
        Assert.Equal("Dark", prefs.Theme);
        Assert.True(prefs.SidebarExpanded);
    }
}
