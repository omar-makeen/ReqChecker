using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.ProfileManagement;
using ReqChecker.Infrastructure.ProfileManagement.Migrations;
using System.IO;
using System.ComponentModel;

namespace ReqChecker.App.Tests.ViewModels;

/// <summary>
/// Unit tests for ProfileSelectorViewModel import and validation functionality.
/// </summary>
public class ProfileSelectorViewModelTests
{
    private static ProfileMigrationPipeline CreateMigrationPipeline()
    {
        return new ProfileMigrationPipeline(new List<IProfileMigrator> { new V1ToV2Migration(), new V2ToV3Migration() });
    }

    private static Mock<IPreferencesService> CreateMockPreferencesService()
    {
        var mock = new Mock<IPreferencesService>();
        mock.SetupAllProperties();
        mock.Setup(x => x.HasSeenOnboarding).Returns(false);
        return mock;
    }

    [Fact]
    public void Constructor_ShouldLoadProfilesOnInitialization()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        mockProfileLoader.Setup(x => x.LoadFromStreamAsync(It.IsAny<Stream>()))
            .ReturnsAsync(new Profile { Name = "Test Profile", SchemaVersion = 2 });
        mockProfileValidator.Setup(x => x.ValidateAsync(It.IsAny<Profile>()))
            .ReturnsAsync(new List<string>());
        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        // Act
        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Assert
        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.Profiles);
    }

    [Fact]
    public async Task ImportProfileAsync_ShouldSetErrorWhenValidationFails()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        var invalidProfile = new Profile { Name = "Invalid Profile", SchemaVersion = 2 };
        var validationErrors = new List<string> { "Missing required field: Name" };

        mockDialogService.Setup(x => x.OpenProfileFileDialog()).Returns("C:\\test\\profile.json");
        mockProfileLoader.Setup(x => x.LoadFromFileAsync(It.IsAny<string>()))
            .ReturnsAsync(invalidProfile);
        mockProfileValidator.Setup(x => x.ValidateAsync(invalidProfile))
            .ReturnsAsync(validationErrors);
        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act
        await viewModel.ImportProfileCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Contains("Profile validation failed", viewModel.ErrorMessage);
        Assert.Contains("Missing required field: Name", viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task ImportProfileAsync_ShouldNotImportWhenUserCancels()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        mockDialogService.Setup(x => x.OpenProfileFileDialog()).Returns(string.Empty);
        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act
        await viewModel.ImportProfileCommand.ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.Null(viewModel.ErrorMessage);
        mockProfileLoader.Verify(x => x.LoadFromFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ImportProfileAsync_ShouldMigrateProfileIfNeeded()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        // V1 profile that needs migration
        var v1Profile = new Profile { Name = "V1 Profile", SchemaVersion = 1 };

        mockDialogService.Setup(x => x.OpenProfileFileDialog()).Returns("C:\\test\\profile.json");
        mockProfileLoader.Setup(x => x.LoadFromFileAsync(It.IsAny<string>()))
            .ReturnsAsync(v1Profile);
        mockProfileValidator.Setup(x => x.ValidateAsync(It.IsAny<Profile>()))
            .ReturnsAsync(new List<string>());
        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act
        await viewModel.ImportProfileCommand.ExecuteAsync(null);

        // Assert - If profile was migrated successfully (no error), it should be added to profiles
        Assert.False(viewModel.HasError);
        // The migrated profile should be in the profiles collection (migrated to version3 which is current)
        Assert.Contains(viewModel.Profiles, p => p.Name == "V1 Profile" && p.SchemaVersion == 3);
    }

    [Fact]
    public async Task ImportProfileAsync_ShouldCopyProfileToUserDirectory()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        // Profile at current schema version (3) so no migration is needed
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var filePath = "C:\\test\\profile.json";

        mockDialogService.Setup(x => x.OpenProfileFileDialog()).Returns(filePath);
        mockProfileLoader.Setup(x => x.LoadFromFileAsync(filePath))
            .ReturnsAsync(profile);
        mockProfileValidator.Setup(x => x.ValidateAsync(profile))
            .ReturnsAsync(new List<string>());
        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act
        await viewModel.ImportProfileCommand.ExecuteAsync(null);

        // Assert
        mockProfileStorageService.Verify(
            x => x.CopyProfileToUserDirectory(filePath, true), Times.Once);
        Assert.Contains(profile, viewModel.Profiles);
    }

    [Fact]
    public async Task ImportProfileAsync_ShouldSetErrorOnException()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        mockDialogService.Setup(x => x.OpenProfileFileDialog()).Returns("C:\\test\\profile.json");
        mockProfileLoader.Setup(x => x.LoadFromFileAsync(It.IsAny<string>()))
            .ThrowsAsync(new IOException("File not found"));
        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act
        await viewModel.ImportProfileCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Contains("Failed to import profile", viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public void SelectProfile_ShouldSetCurrentProfileInAppState()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            navigationService,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act - Execute will try to navigate which will fail due to missing service provider registrations
        // But we can verify that SelectedProfile is set correctly
        try
        {
            viewModel.SelectProfileCommand.Execute(profile);
        }
        catch (InvalidOperationException)
        {
            // Expected - navigation requires full DI setup
        }

        // Assert
        Assert.Equal(profile, viewModel.SelectedProfile);
        mockAppState.Verify(x => x.SetCurrentProfile(profile), Times.Once);
    }

    [Fact]
    public void SelectProfile_ShouldNotNavigateWhenProfileIsNull()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act
        viewModel.SelectProfileCommand.Execute(null);

        // Assert
        Assert.Null(viewModel.SelectedProfile);
        // SetCurrentProfile is called with null, navigation should not happen
    }

    [Fact]
    public void ClearError_ShouldClearErrorState()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Simulate error state
        viewModel.HasError = true;
        viewModel.ErrorMessage = "Test error";

        // Act
        viewModel.ClearErrorCommand.Execute(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.Null(viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadProfilesAsync_ShouldSetErrorOnException()
    {
        // Arrange
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Throws(new IOException("Directory not found"));

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act
        await viewModel.LoadProfilesCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Contains("Failed to load profiles", viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public void ShowWelcomeBanner_ShouldReturnTrue_WhenHasSeenOnboardingIsFalse()
    {
        // Arrange
        var mockPreferencesService = CreateMockPreferencesService();
        mockPreferencesService.Setup(x => x.HasSeenOnboarding).Returns(false);

        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Assert
        Assert.True(viewModel.ShowWelcomeBanner);
    }

    [Fact]
    public void ShowWelcomeBanner_ShouldReturnFalse_WhenHasSeenOnboardingIsTrue()
    {
        // Arrange
        var mockPreferencesService = CreateMockPreferencesService();
        mockPreferencesService.Setup(x => x.HasSeenOnboarding).Returns(true);

        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Assert
        Assert.False(viewModel.ShowWelcomeBanner);
    }

    [Fact]
    public void DismissWelcomeBannerCommand_ShouldSetHasSeenOnboardingToTrue()
    {
        // Arrange
        var mockPreferencesService = CreateMockPreferencesService();
        mockPreferencesService.Setup(x => x.HasSeenOnboarding).Returns(false);
        mockPreferencesService.SetupSet(x => x.HasSeenOnboarding = It.IsAny<bool>())
            .Callback<bool>(value => mockPreferencesService.Setup(x => x.HasSeenOnboarding).Returns(value));

        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        // Act
        viewModel.DismissWelcomeBannerCommand.Execute(null);

        // Assert
        mockPreferencesService.VerifySet(x => x.HasSeenOnboarding = true, Times.Once);
    }

    [Fact]
    public void IsRecommendedProfile_ShouldReturnTrue_ForDefaultProfileId()
    {
        // Arrange
        var mockPreferencesService = CreateMockPreferencesService();
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        var profile = new Profile
        {
            Id = "00000001-0000-0000-0000-000000000001",
            Name = "Default Profile",
            Source = ProfileSource.Bundled
        };

        // Act & Assert
        Assert.True(viewModel.IsRecommendedProfile(profile));
    }

    [Fact]
    public void IsRecommendedProfile_ShouldReturnFalse_ForUserSourceProfileWithDefaultId()
    {
        // A user-imported profile that happens to share the bundled default's ID
        // must NOT be flagged as Recommended (regression guard for the duplicate-badge bug).
        var mockPreferencesService = CreateMockPreferencesService();
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        var profile = new Profile
        {
            Id = "00000001-0000-0000-0000-000000000001",
            Name = "User Copy of Default",
            Source = ProfileSource.UserProvided
        };

        Assert.False(viewModel.IsRecommendedProfile(profile));
    }

    [Fact]
    public void IsRecommendedProfile_ShouldReturnFalse_ForOtherProfileIds()
    {
        // Arrange
        var mockPreferencesService = CreateMockPreferencesService();
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);

        var profile = new Profile { Id = "00000002-0000-0000-0000-000000000002", Name = "Other Profile" };

        // Act & Assert
        Assert.False(viewModel.IsRecommendedProfile(profile));
    }

    [Fact]
    public void ShowWelcomeBanner_ShouldUpdate_WhenPreferencesPropertyChangedFires()
    {
        // Arrange
        var preferencesService = new PreferencesService();
        preferencesService.HasSeenOnboarding = false;

        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            preferencesService);

        // Assert initial state
        Assert.True(viewModel.ShowWelcomeBanner);

        // Track property changed events
        bool showWelcomeBannerChanged = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ShowWelcomeBanner))
            {
                showWelcomeBannerChanged = true;
            }
        };

        // Act - change the preference
        preferencesService.HasSeenOnboarding = true;

        // Assert
        Assert.True(showWelcomeBannerChanged, "ShowWelcomeBanner PropertyChanged should have been raised");
        Assert.False(viewModel.ShowWelcomeBanner);

        // Cleanup
        viewModel.Dispose();
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromPreferencesPropertyChanged()
    {
        // Arrange
        var preferencesService = new PreferencesService();
        preferencesService.HasSeenOnboarding = false;

        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            preferencesService);

        // Act - dispose the view model
        viewModel.Dispose();

        // Track property changed events after disposal
        bool showWelcomeBannerChanged = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.ShowWelcomeBanner))
            {
                showWelcomeBannerChanged = true;
            }
        };

        // Change the preference - should NOT trigger the disposed handler
        preferencesService.HasSeenOnboarding = true;

        // Assert - no change should be detected since we unsubscribed
        Assert.False(showWelcomeBannerChanged, "ShowWelcomeBanner PropertyChanged should NOT have been raised after disposal");
    }

    private static ProfileSelectorViewModel CreateViewModelForItemsTests(
        out Mock<IAppState> mockAppState,
        out Mock<NavigationService> mockNavigationService,
        List<Profile>? bundledProfiles = null,
        string[]? userProfilePaths = null,
        Profile? currentProfile = null)
    {
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockAppState = new Mock<IAppState>();
        mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        var allProfiles = bundledProfiles ?? new List<Profile>();
        mockProfileLoader.Setup(x => x.LoadFromStreamAsync(It.IsAny<Stream>()))
            .ReturnsAsync((Stream s) =>
            {
                if (allProfiles.Count > 0)
                {
                    var p = allProfiles[0];
                    allProfiles.RemoveAt(0);
                    return p;
                }
                return new Profile { Name = "Fallback", SchemaVersion = 3 };
            });
        mockProfileValidator.Setup(x => x.ValidateAsync(It.IsAny<Profile>()))
            .ReturnsAsync(new List<string>());

        var paths = userProfilePaths ?? Array.Empty<string>();
        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(paths);

        if (currentProfile != null)
        {
            mockAppState.Setup(x => x.CurrentProfile).Returns(currentProfile);
        }

        return new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);
    }

    [Fact]
    public void LoadProfiles_PopulatesItemsCollection()
    {
        var profile = new Profile { Id = "p1", Name = "Test Profile", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForItemsTests(out _, out _, bundledProfiles: new List<Profile> { profile });

        Assert.NotEmpty(vm.Items);
        var item = vm.Items.FirstOrDefault(i => i.Name == "Test Profile");
        Assert.NotNull(item);
        Assert.Equal("Test Profile", item.Name);
        Assert.Equal("p1", item.Profile.Id);
    }

    [Fact]
    public void LoadProfiles_SetsSelectedItem_WhenAppStateHasCurrentProfile()
    {
        var profile = new Profile { Id = "p1", Name = "Test Profile", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForItemsTests(out _, out _, bundledProfiles: new List<Profile> { profile }, currentProfile: profile);

        Assert.NotNull(vm.SelectedItem);
        Assert.Equal("p1", vm.SelectedItem.Profile.Id);
        Assert.True(vm.SelectedItem.IsActive);
    }

    [Fact]
    public void OnCurrentProfileChanged_UpdatesIsActiveForMatchingItem()
    {
        var profile1 = new Profile { Id = "p1", Name = "Profile 1", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var profile2 = new Profile { Id = "p2", Name = "Profile 2", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForItemsTests(out var mockAppState, out _, bundledProfiles: new List<Profile> { profile1, profile2 });

        var handler = mockAppState.Invocations.FirstOrDefault(i => i.Method.Name == "add_CurrentProfileChanged");
        Assert.NotNull(handler);

        mockAppState.Setup(x => x.CurrentProfile).Returns(profile1);

        mockAppState.Raise(x => x.CurrentProfileChanged += null, EventArgs.Empty);

        Assert.True(vm.Items.First(i => i.Profile.Id == "p1").IsActive);
        Assert.False(vm.Items.First(i => i.Profile.Id == "p2").IsActive);
    }

    [Fact]
    public void OnCurrentProfileChanged_ClearsIsActiveForOtherItems()
    {
        var profile1 = new Profile { Id = "p1", Name = "Profile 1", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var profile2 = new Profile { Id = "p2", Name = "Profile 2", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForItemsTests(out var mockAppState, out _, bundledProfiles: new List<Profile> { profile1, profile2 });

        mockAppState.Setup(x => x.CurrentProfile).Returns(profile1);
        mockAppState.Raise(x => x.CurrentProfileChanged += null, EventArgs.Empty);

        Assert.True(vm.Items.First(i => i.Profile.Id == "p1").IsActive);

        mockAppState.Setup(x => x.CurrentProfile).Returns(profile2);
        mockAppState.Raise(x => x.CurrentProfileChanged += null, EventArgs.Empty);

        Assert.False(vm.Items.First(i => i.Profile.Id == "p1").IsActive);
        Assert.True(vm.Items.First(i => i.Profile.Id == "p2").IsActive);
    }

    [Fact]
    public void SelectingItemThatEqualsCurrentProfile_DoesNotRefireNavigation()
    {
        var profile = new Profile { Id = "p1", Name = "Profile 1", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForItemsTests(out var mockAppState, out _, bundledProfiles: new List<Profile> { profile });

        mockAppState.Setup(x => x.CurrentProfile).Returns(profile);

        vm.SelectProfileCommand.Execute(profile);

        mockAppState.Verify(x => x.SetCurrentProfile(It.IsAny<Profile>()), Times.Never);
    }

    [Fact]
    public void Dispose_UnsubscribesFromCurrentProfileChanged()
    {
        var profile = new Profile { Id = "p1", Name = "Profile 1", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForItemsTests(out var mockAppState, out _, bundledProfiles: new List<Profile> { profile });

        vm.Dispose();

        var initialActiveState = vm.Items[0].IsActive;
        mockAppState.Setup(x => x.CurrentProfile).Returns(profile);
        mockAppState.Raise(x => x.CurrentProfileChanged += null, EventArgs.Empty);

        Assert.Equal(initialActiveState, vm.Items[0].IsActive);
    }

    private static ProfileSelectorViewModel CreateViewModelForRowActions(
        out Mock<IAppState> mockAppState,
        out Mock<DialogService> mockDialogService,
        out Mock<IProfileStorageService> mockProfileStorageService,
        string filePath,
        Profile profile,
        Profile? currentProfile = null)
    {
        // Write a temp file at the given path so ProfileListItemViewModel captures a real mtime.
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{}");

        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockAppState = new Mock<IAppState>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        mockProfileStorageService = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        mockProfileLoader.Setup(x => x.LoadFromFileAsync(filePath))
            .ReturnsAsync(profile);
        mockProfileValidator.Setup(x => x.ValidateAsync(It.IsAny<Profile>()))
            .ReturnsAsync(new List<string>());
        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(new[] { filePath });
        mockProfileStorageService.Setup(x => x.GetUserProfilesDirectory())
            .Returns(Path.GetDirectoryName(filePath)!);

        if (currentProfile != null)
        {
            mockAppState.Setup(x => x.CurrentProfile).Returns(currentProfile);
        }

        return new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object,
            mockPreferencesService.Object);
    }

    [Fact]
    public void DeleteProfile_ShouldNoOp_WhenItemIsNull()
    {
        var vm = CreateViewModelForItemsTests(out _, out _);

        vm.DeleteProfileCommand.Execute(null);

        Assert.False(vm.HasError);
    }

    [Fact]
    public void DeleteProfile_ShouldNoOp_WhenSourceFilePathIsNull()
    {
        // A bundled-source row has no SourceFilePath, so DeleteProfile must be a no-op
        // (no confirmation prompt, no storage call, no error).
        var bundled = new Profile { Id = "p1", Name = "Bundled Profile", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForItemsTests(out _, out _, bundledProfiles: new List<Profile> { bundled });

        var item = vm.Items.First();
        Assert.Null(item.SourceFilePath);

        vm.DeleteProfileCommand.Execute(item);

        Assert.False(vm.HasError);
        Assert.Contains(item, vm.Items);
    }

    [Fact]
    public void DeleteProfile_ShouldNoOp_WhenUserDeclinesConfirmation()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ReqCheckerTests_" + Guid.NewGuid());
        var filePath = Path.Combine(tempDir, "user.json");
        var profile = new Profile { Id = "p1", Name = "User Profile", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForRowActions(out _, out var mockDialogService, out var mockStorage,
            filePath, profile);

        try
        {
            mockDialogService.Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var item = vm.Items.First();
            vm.DeleteProfileCommand.Execute(item);

            mockStorage.Verify(x => x.DeleteProfile(It.IsAny<string>()), Times.Never);
            Assert.Contains(item, vm.Items);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void DeleteProfile_ShouldCallStorageWithFileName_AndRemoveFromCollections_WhenConfirmed()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ReqCheckerTests_" + Guid.NewGuid());
        var filePath = Path.Combine(tempDir, "user.json");
        var profile = new Profile { Id = "p1", Name = "User Profile", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForRowActions(out _, out var mockDialogService, out var mockStorage,
            filePath, profile);

        try
        {
            mockDialogService.Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var item = vm.Items.First();
            vm.DeleteProfileCommand.Execute(item);

            mockStorage.Verify(x => x.DeleteProfile("user.json"), Times.Once);
            Assert.DoesNotContain(item, vm.Items);
            Assert.DoesNotContain(profile, vm.Profiles);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void DeleteProfile_ShouldClearAppStateCurrentProfile_WhenDeletingActiveProfile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ReqCheckerTests_" + Guid.NewGuid());
        var filePath = Path.Combine(tempDir, "user.json");
        var profile = new Profile { Id = "p1", Name = "User Profile", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForRowActions(out var mockAppState, out var mockDialogService, out _,
            filePath, profile, currentProfile: profile);

        try
        {
            mockDialogService.Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var item = vm.Items.First();
            Assert.True(item.IsActive);

            vm.DeleteProfileCommand.Execute(item);

            mockAppState.Verify(x => x.ClearCurrentProfile(), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void DeleteProfile_ShouldNotMutateCollections_WhenStorageServiceThrows()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ReqCheckerTests_" + Guid.NewGuid());
        var filePath = Path.Combine(tempDir, "user.json");
        var profile = new Profile { Id = "p1", Name = "User Profile", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForRowActions(out _, out var mockDialogService, out var mockStorage,
            filePath, profile);

        try
        {
            mockDialogService.Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            mockStorage.Setup(x => x.DeleteProfile(It.IsAny<string>()))
                .Throws(new IOException("Boom"));

            var item = vm.Items.First();
            vm.DeleteProfileCommand.Execute(item);

            Assert.True(vm.HasError);
            Assert.Contains("Failed to delete profile", vm.ErrorMessage);
            Assert.Contains(item, vm.Items);
            Assert.Contains(profile, vm.Profiles);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void OpenFileLocation_ShouldCallDialogServiceOpenInExplorer_WithProfileFolder()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ReqCheckerTests_" + Guid.NewGuid());
        var filePath = Path.Combine(tempDir, "user.json");
        var profile = new Profile { Id = "p1", Name = "User Profile", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForRowActions(out _, out var mockDialogService, out _,
            filePath, profile);

        try
        {
            var item = vm.Items.First();
            vm.OpenFileLocationCommand.Execute(item);

            mockDialogService.Verify(x => x.OpenInExplorer(tempDir), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void LoadProfiles_SortsRecommendedBeforeOtherBundled_RegardlessOfName()
    {
        // Recommended profile has a name that sorts AFTER another bundled profile alphabetically.
        // The rank-then-name sort must put Recommended first anyway.
        var alphaBundled = new Profile
        {
            Id = "alpha-bundled",
            Name = "Alpha Bundled Profile",
            SchemaVersion = 3,
            Source = ProfileSource.Bundled,
            Tests = new List<TestDefinition>()
        };
        var recommendedBundled = new Profile
        {
            Id = ProfileSelectorViewModel.DefaultProfileId,
            Name = "Zulu Default",
            SchemaVersion = 3,
            Source = ProfileSource.Bundled,
            Tests = new List<TestDefinition>()
        };
        var vm = CreateViewModelForItemsTests(out _, out _,
            bundledProfiles: new List<Profile> { alphaBundled, recommendedBundled });

        var alphaIndex = vm.Items.ToList().FindIndex(i => i.Profile.Id == "alpha-bundled");
        var recommendedIndex = vm.Items.ToList().FindIndex(i => i.Profile.Id == ProfileSelectorViewModel.DefaultProfileId);

        Assert.True(recommendedIndex >= 0, "Recommended item must be present");
        Assert.True(alphaIndex >= 0, "Alpha item must be present");
        Assert.True(recommendedIndex < alphaIndex,
            $"Recommended should come before alpha-bundled, got recommendedIndex={recommendedIndex}, alphaIndex={alphaIndex}");
    }

    [Fact]
    public void LoadProfiles_PinsActiveProfileAboveRecommended()
    {
        // Active profile is a non-recommended bundled. It must rank above the recommended one.
        var recommendedBundled = new Profile
        {
            Id = ProfileSelectorViewModel.DefaultProfileId,
            Name = "Default",
            SchemaVersion = 3,
            Source = ProfileSource.Bundled,
            Tests = new List<TestDefinition>()
        };
        var someBundled = new Profile
        {
            Id = "active-bundled",
            Name = "Some Other",
            SchemaVersion = 3,
            Source = ProfileSource.Bundled,
            Tests = new List<TestDefinition>()
        };
        var vm = CreateViewModelForItemsTests(out _, out _,
            bundledProfiles: new List<Profile> { recommendedBundled, someBundled },
            currentProfile: someBundled);

        var activeIndex = vm.Items.ToList().FindIndex(i => i.Profile.Id == "active-bundled");
        var recommendedIndex = vm.Items.ToList().FindIndex(i => i.Profile.Id == ProfileSelectorViewModel.DefaultProfileId);

        Assert.True(activeIndex >= 0, "Active item must be present");
        Assert.True(recommendedIndex >= 0, "Recommended item must be present");
        Assert.True(activeIndex < recommendedIndex,
            $"Active should come before recommended, got activeIndex={activeIndex}, recommendedIndex={recommendedIndex}");
    }

    [Fact]
    public void OpenFileLocation_ShouldNoOp_WhenItemIsNullOrBundled()
    {
        var bundled = new Profile { Id = "b1", Name = "Bundled", SchemaVersion = 3, Tests = new List<TestDefinition>() };
        var vm = CreateViewModelForItemsTests(out _, out _, bundledProfiles: new List<Profile> { bundled });
        var bundledItem = vm.Items.First();
        Assert.Null(bundledItem.SourceFilePath);

        // Cast to access the private DialogService mock indirectly via the page VM's behavior.
        // We can't observe DialogService here without rebuilding the VM. The contract is:
        // OpenFileLocation must return without throwing for null and for bundled rows.
        var ex1 = Record.Exception(() => vm.OpenFileLocationCommand.Execute(null));
        var ex2 = Record.Exception(() => vm.OpenFileLocationCommand.Execute(bundledItem));

        Assert.Null(ex1);
        Assert.Null(ex2);
    }

    private static (ProfileSelectorViewModel vm, Mock<DialogService> dialog, Mock<IProfileStorageService> storage)
        CreateViewModelForImportCollisionTests(
            Profile bundledLoaded,
            Profile fileBeingImported,
            string sourceFilePath = "C:\\import\\incoming.json",
            string destPath = "C:\\dest\\incoming.json")
    {
        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var dialog = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var storage = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        // Bundled-load path: returns the pre-existing profile (so it lands in Profiles before import runs).
        mockProfileLoader.Setup(x => x.LoadFromStreamAsync(It.IsAny<Stream>()))
            .ReturnsAsync(bundledLoaded);
        // File-load path (the import): returns the colliding-id profile.
        mockProfileLoader.Setup(x => x.LoadFromFileAsync(sourceFilePath))
            .ReturnsAsync(fileBeingImported);
        mockProfileValidator.Setup(x => x.ValidateAsync(It.IsAny<Profile>()))
            .ReturnsAsync(new List<string>());
        storage.Setup(x => x.GetProfileFilePaths()).Returns(Array.Empty<string>());
        storage.Setup(x => x.CopyProfileToUserDirectory(sourceFilePath, true)).Returns(destPath);
        storage.Setup(x => x.SaveProfileWithRegeneratedIdAsync(sourceFilePath, It.IsAny<string>()))
            .ReturnsAsync(destPath);
        dialog.Setup(x => x.OpenProfileFileDialog()).Returns(sourceFilePath);

        var vm = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            dialog.Object,
            mockNavigationService.Object,
            storage.Object,
            mockPreferencesService.Object);

        return (vm, dialog, storage);
    }

    [Fact]
    public async Task ImportProfileAsync_NoCollision_UsesCopyProfileToUserDirectory()
    {
        // Bundled has id=A, file being imported has id=B. No collision -> normal copy path.
        var bundled = new Profile { Id = "id-A", Name = "Bundled A", SchemaVersion = 3, Source = ProfileSource.Bundled, Tests = new List<TestDefinition>() };
        var imported = new Profile { Id = "id-B", Name = "Imported B", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var (vm, dialog, storage) = CreateViewModelForImportCollisionTests(bundled, imported);

        await vm.ImportProfileCommand.ExecuteAsync(null);

        storage.Verify(x => x.CopyProfileToUserDirectory(It.IsAny<string>(), true), Times.Once);
        storage.Verify(x => x.SaveProfileWithRegeneratedIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        dialog.Verify(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.Contains(vm.Profiles, p => p.Id == "id-B");
    }

    [Fact]
    public async Task ImportProfileAsync_BundledIdCollision_UserCancels_AbortsImport()
    {
        var bundled = new Profile { Id = "shared-id", Name = "Bundled Original", SchemaVersion = 3, Source = ProfileSource.Bundled, Tests = new List<TestDefinition>() };
        var imported = new Profile { Id = "shared-id", Name = "User Copy", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var (vm, dialog, storage) = CreateViewModelForImportCollisionTests(bundled, imported);

        dialog.Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        await vm.ImportProfileCommand.ExecuteAsync(null);

        dialog.Verify(x => x.ShowConfirmationDialog("Profile ID conflict", It.IsAny<string>()), Times.Once);
        storage.Verify(x => x.CopyProfileToUserDirectory(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        storage.Verify(x => x.SaveProfileWithRegeneratedIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        // The colliding User Copy must NOT have been added.
        Assert.DoesNotContain(vm.Profiles, p => p.Name == "User Copy");
        Assert.False(vm.HasError);
    }

    [Fact]
    public async Task ImportProfileAsync_BundledIdCollision_UserConfirms_RegeneratesId()
    {
        var bundled = new Profile { Id = "shared-id", Name = "Bundled Original", SchemaVersion = 3, Source = ProfileSource.Bundled, Tests = new List<TestDefinition>() };
        var imported = new Profile { Id = "shared-id", Name = "User Copy", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var (vm, dialog, storage) = CreateViewModelForImportCollisionTests(bundled, imported);

        dialog.Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        await vm.ImportProfileCommand.ExecuteAsync(null);

        // Storage write went through the regenerated-id path, not plain copy.
        storage.Verify(x => x.SaveProfileWithRegeneratedIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        storage.Verify(x => x.CopyProfileToUserDirectory(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);

        // Imported profile is now in the list with a NEW id (not "shared-id").
        var userCopy = vm.Profiles.FirstOrDefault(p => p.Name == "User Copy");
        Assert.NotNull(userCopy);
        Assert.NotEqual("shared-id", userCopy.Id);
        Assert.True(Guid.TryParse(userCopy.Id, out _), "Regenerated id should be a parseable GUID");

        // Original bundled profile still has its original id.
        var stillBundled = vm.Profiles.FirstOrDefault(p => p.Name == "Bundled Original");
        Assert.NotNull(stillBundled);
        Assert.Equal("shared-id", stillBundled.Id);
    }

    [Fact]
    public async Task ImportProfileAsync_UserToUserIdCollision_DetectedSameAsBundled()
    {
        // First user-imported profile (no collision) lands in Profiles via normal copy.
        // Second import has the same id as the first user profile -> collision must be detected
        // (proves the check covers User-vs-User, not just Bundled-vs-User).
        var firstUser = new Profile { Id = "shared-id", Name = "First User", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };
        var secondUser = new Profile { Id = "shared-id", Name = "Second User", SchemaVersion = 3, Source = ProfileSource.UserProvided, Tests = new List<TestDefinition>() };

        var mockProfileLoader = new Mock<IProfileLoader>();
        var mockProfileValidator = new Mock<IProfileValidator>();
        var migrationPipeline = CreateMigrationPipeline();
        var mockAppState = new Mock<IAppState>();
        var dialog = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        var storage = new Mock<IProfileStorageService>();
        var mockPreferencesService = CreateMockPreferencesService();

        // Pretend the first user profile was already on disk at construction time.
        storage.Setup(x => x.GetProfileFilePaths())
            .Returns(new[] { "C:\\users\\first.json" });
        mockProfileLoader.Setup(x => x.LoadFromFileAsync("C:\\users\\first.json"))
            .ReturnsAsync(firstUser);
        // The import dialog returns a different file path for the second one.
        dialog.Setup(x => x.OpenProfileFileDialog()).Returns("C:\\import\\second.json");
        mockProfileLoader.Setup(x => x.LoadFromFileAsync("C:\\import\\second.json"))
            .ReturnsAsync(secondUser);
        mockProfileValidator.Setup(x => x.ValidateAsync(It.IsAny<Profile>()))
            .ReturnsAsync(new List<string>());
        storage.Setup(x => x.SaveProfileWithRegeneratedIdAsync("C:\\import\\second.json", It.IsAny<string>()))
            .ReturnsAsync("C:\\dest\\second.json");
        // User confirms.
        dialog.Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var vm = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            dialog.Object,
            mockNavigationService.Object,
            storage.Object,
            mockPreferencesService.Object);

        await vm.ImportProfileCommand.ExecuteAsync(null);

        // Confirmation should have been shown (collision detected against the User-source profile).
        dialog.Verify(x => x.ShowConfirmationDialog("Profile ID conflict", It.IsAny<string>()), Times.Once);
        storage.Verify(x => x.SaveProfileWithRegeneratedIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var secondCopy = vm.Profiles.FirstOrDefault(p => p.Name == "Second User");
        Assert.NotNull(secondCopy);
        Assert.NotEqual("shared-id", secondCopy.Id);
    }
}
