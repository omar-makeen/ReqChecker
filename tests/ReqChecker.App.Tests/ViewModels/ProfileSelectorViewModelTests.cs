using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.ProfileManagement;
using ReqChecker.Infrastructure.ProfileManagement.Migrations;
using System.IO;

namespace ReqChecker.App.Tests.ViewModels;

/// <summary>
/// Unit tests for ProfileSelectorViewModel import and validation functionality.
/// </summary>
public class ProfileSelectorViewModelTests
{
    private static ProfileMigrationPipeline CreateMigrationPipeline()
    {
        return new ProfileMigrationPipeline(new List<IProfileMigrator> { new V1ToV2Migration() });
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
            mockProfileStorageService.Object);

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
            mockProfileStorageService.Object);

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
            mockProfileStorageService.Object);

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
            mockProfileStorageService.Object);

        // Act
        await viewModel.ImportProfileCommand.ExecuteAsync(null);

        // Assert - If profile was migrated successfully (no error), it should be added to profiles
        Assert.False(viewModel.HasError);
        // The migrated profile should be in the profiles collection
        Assert.Contains(viewModel.Profiles, p => p.Name == "V1 Profile" && p.SchemaVersion == 2);
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

        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
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
            mockProfileStorageService.Object);

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
            mockProfileStorageService.Object);

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
            mockProfileStorageService.Object);

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

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object);

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

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Returns(Array.Empty<string>());

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object);

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

        mockProfileStorageService.Setup(x => x.GetProfileFilePaths())
            .Throws(new IOException("Directory not found"));

        var viewModel = new ProfileSelectorViewModel(
            mockProfileLoader.Object,
            mockProfileValidator.Object,
            migrationPipeline,
            mockAppState.Object,
            mockDialogService.Object,
            mockNavigationService.Object,
            mockProfileStorageService.Object);

        // Act
        await viewModel.LoadProfilesCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Contains("Failed to load profiles", viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
    }
}
