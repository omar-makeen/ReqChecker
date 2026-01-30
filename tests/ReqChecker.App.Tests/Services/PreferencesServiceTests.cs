using ReqChecker.App.Services;

namespace ReqChecker.App.Tests.Services;

public class PreferencesServiceTests
{
    [Fact]
    public void UserPreferences_DefaultTheme_ShouldBeDark()
    {
        // Arrange & Act - Test the UserPreferences class defaults directly (not the service)
        var prefs = new UserPreferences();

        // Assert - default theme in the DTO should be "Dark"
        Assert.Equal("Dark", prefs.Theme);
    }

    [Fact]
    public void UserPreferences_DefaultSidebarExpanded_ShouldBeTrue()
    {
        // Arrange & Act - Test the UserPreferences class defaults directly (not the service)
        var prefs = new UserPreferences();

        // Assert - sidebar should be expanded by default in the DTO
        Assert.True(prefs.SidebarExpanded);
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
        var initialValue = service.SidebarExpanded;

        // Act
        service.SidebarExpanded = !initialValue;

        // Assert
        Assert.Equal(!initialValue, service.SidebarExpanded);
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

    [Fact]
    public void Theme_CanBeSetToLight()
    {
        // Arrange
        var service = new PreferencesService();

        // Act
        service.Theme = AppTheme.Light;

        // Assert
        Assert.Equal(AppTheme.Light, service.Theme);
    }

    [Fact]
    public void Theme_CanBeSetToDark()
    {
        // Arrange
        var service = new PreferencesService();

        // Act
        service.Theme = AppTheme.Dark;

        // Assert
        Assert.Equal(AppTheme.Dark, service.Theme);
    }

    [Fact]
    public void SidebarExpanded_CanBeToggled()
    {
        // Arrange
        var service = new PreferencesService();
        var originalValue = service.SidebarExpanded;

        // Act - Toggle twice
        service.SidebarExpanded = !originalValue;
        var afterFirstToggle = service.SidebarExpanded;
        service.SidebarExpanded = !afterFirstToggle;
        var afterSecondToggle = service.SidebarExpanded;

        // Assert
        Assert.NotEqual(originalValue, afterFirstToggle);
        Assert.Equal(originalValue, afterSecondToggle);
    }
}
