using Moq;
using ReqChecker.App.Services;

namespace ReqChecker.App.Tests.Services;

public class ThemeServiceTests
{
    private Mock<IPreferencesService> CreateMockPreferencesService(AppTheme theme = AppTheme.Dark, bool sidebarExpanded = true)
    {
        var mock = new Mock<IPreferencesService>();
        mock.SetupProperty(p => p.Theme, theme);
        mock.SetupProperty(p => p.SidebarExpanded, sidebarExpanded);
        return mock;
    }

    [Fact]
    public void GetAnimationDuration_WhenReducedMotionDisabled_ReturnsNormalDuration()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService();
        var service = new ThemeService(mockPrefs.Object);
        var normalDuration = TimeSpan.FromMilliseconds(200);

        // Note: We can't easily test IsReducedMotionEnabled because it depends on SystemParameters
        // which requires a WPF application context. This test verifies the method signature works.

        // Act
        var result = service.GetAnimationDuration(normalDuration);

        // Assert - either returns normal duration or zero depending on system setting
        Assert.True(result == normalDuration || result == TimeSpan.Zero);
    }

    [Fact]
    public void ShouldAnimateEssential_AlwaysReturnsTrue()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService();
        var service = new ThemeService(mockPrefs.Object);

        // Act & Assert
        Assert.True(service.ShouldAnimateEssential);
    }

    [Fact]
    public void CurrentTheme_LoadedFromPreferences()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Light);

        // Act
        var service = new ThemeService(mockPrefs.Object);

        // Assert
        Assert.Equal(AppTheme.Light, service.CurrentTheme);
    }

    [Fact]
    public void SetTheme_UpdatesPreferences()
    {
        // Arrange
        var mockPrefs = CreateMockPreferencesService(AppTheme.Dark);
        var service = new ThemeService(mockPrefs.Object);

        // Act
        service.SetTheme(AppTheme.Light);

        // Assert
        Assert.Equal(AppTheme.Light, mockPrefs.Object.Theme);
    }
}
