using System.Text.Json.Nodes;
using CommunityToolkit.Mvvm.Input;
using Moq;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;

namespace ReqChecker.App.Tests.ViewModels;

/// <summary>
/// Unit tests for TestConfigViewModel dirty tracking behavior.
/// Tests unsaved changes warning functionality (061-unsaved-changes-warning).
/// </summary>
public class TestConfigViewModelTests
{
    private static TestDefinition CreateTestDefinition(
        int? timeout = 5000,
        int? retryCount = 3,
        JsonObject? parameters = null,
        Dictionary<string, FieldPolicyType>? fieldPolicy = null)
    {
        return new TestDefinition
        {
            Id = "test-1",
            DisplayName = "Test 1",
            Type = "HttpGet",
            Timeout = timeout,
            RetryCount = retryCount,
            Parameters = parameters ?? new JsonObject(),
            FieldPolicy = fieldPolicy ?? new Dictionary<string, FieldPolicyType>()
        };
    }

    private static (TestConfigViewModel viewModel, Mock<DialogService> mockDialogService, Mock<NavigationService> mockNavigationService) CreateViewModelWithMocks(
        TestDefinition? testDefinition = null)
    {
        testDefinition ??= CreateTestDefinition();
        
        var mockDialogService = new Mock<DialogService>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        mockNavigationService.CallBase = true;
        
        var viewModel = new TestConfigViewModel(testDefinition, mockNavigationService.Object, mockDialogService.Object);
        return (viewModel, mockDialogService, mockNavigationService);
    }

    // =========================================================
    // T002 (US1): HasUnsavedChanges should be false when no changes
    // =========================================================

    [Fact]
    public void HasUnsavedChanges_ShouldBeFalse_WhenNoChanges()
    {
        // Arrange
        var test = CreateTestDefinition();
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        // Act & Assert
        Assert.False(viewModel.HasUnsavedChanges);
    }

    // =========================================================
    // T003 (US1): HasUnsavedChanges should be true when timeout changed
    // =========================================================

    [Fact]
    public void HasUnsavedChanges_ShouldBeTrue_WhenTimeoutChanged()
    {
        // Arrange
        var test = CreateTestDefinition(timeout: 5000);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        // Act
        viewModel.Timeout = 10000;

        // Assert
        Assert.True(viewModel.HasUnsavedChanges);
    }

    // =========================================================
    // T004 (US1): HasUnsavedChanges should be true when retry count changed
    // =========================================================

    [Fact]
    public void HasUnsavedChanges_ShouldBeTrue_WhenRetryCountChanged()
    {
        // Arrange
        var test = CreateTestDefinition(retryCount: 3);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        // Act
        viewModel.RetryCount = 5;

        // Assert
        Assert.True(viewModel.HasUnsavedChanges);
    }

    // =========================================================
    // T005 (US1): HasUnsavedChanges should be true when parameter value changed
    // =========================================================

    [Fact]
    public void HasUnsavedChanges_ShouldBeTrue_WhenParameterValueChanged()
    {
        // Arrange
        var parameters = new JsonObject
        {
            ["Url"] = "https://example.com"
        };
        var test = CreateTestDefinition(parameters: parameters);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        // Act
        viewModel.Parameters[0].Value = "https://modified.com";

        // Assert
        Assert.True(viewModel.HasUnsavedChanges);
    }

    // =========================================================
    // T006 (US1): HasUnsavedChanges should be true when password parameter changed
    // =========================================================

    [Fact]
    public void HasUnsavedChanges_ShouldBeTrue_WhenPasswordParameterChanged()
    {
        // Arrange
        var parameters = new JsonObject
        {
            ["ApiKeyPassword"] = "original-secret"
        };
        var test = CreateTestDefinition(parameters: parameters);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        // Act
        viewModel.Parameters[0].Value = "new-secret";

        // Assert
        Assert.True(viewModel.HasUnsavedChanges);
    }

    // =========================================================
    // T007 (US1): BackCommand should show dialog when has unsaved changes
    // =========================================================

    [Fact]
    public void BackCommand_ShouldShowDialog_WhenHasUnsavedChanges()
    {
        // Arrange
        var test = CreateTestDefinition(timeout: 5000);
        var (viewModel, mockDialogService, _) = CreateViewModelWithMocks(test);
        
        viewModel.Timeout = 10000; // Make dirty

        // Act
        viewModel.BackCommand.Execute(null);

        // Assert
        mockDialogService.Verify(
            x => x.ShowConfirmationDialog(
                "Unsaved Changes",
                "You have unsaved changes. Do you want to discard them?"),
            Times.Once);
    }

    // =========================================================
    // T008 (US1): BackCommand should navigate back when user discards changes
    // =========================================================

    [Fact]
    public void BackCommand_ShouldNavigateBack_WhenUserDiscardsChanges()
    {
        // Arrange
        var test = CreateTestDefinition(timeout: 5000);
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        mockNavigationService.CallBase = false;
        
        var mockDialogService = new Mock<DialogService>();
        mockDialogService
            .Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true); // User clicks Yes (discard)

        var viewModel = new TestConfigViewModel(test, mockNavigationService.Object, mockDialogService.Object);
        viewModel.Timeout = 10000; // Make dirty

        // Act
        viewModel.BackCommand.Execute(null);

        // Assert
        mockNavigationService.Verify(x => x.GoBack(), Times.Once);
    }

    // =========================================================
    // T009 (US1): BackCommand should stay on page when user chooses stay
    // =========================================================

    [Fact]
    public void BackCommand_ShouldStayOnPage_WhenUserChoosesStay()
    {
        // Arrange
        var test = CreateTestDefinition(timeout: 5000);
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        mockNavigationService.CallBase = false;
        
        var mockDialogService = new Mock<DialogService>();
        mockDialogService
            .Setup(x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false); // User clicks No (stay)

        var viewModel = new TestConfigViewModel(test, mockNavigationService.Object, mockDialogService.Object);
        viewModel.Timeout = 10000; // Make dirty

        // Act
        viewModel.BackCommand.Execute(null);

        // Assert
        mockNavigationService.Verify(x => x.GoBack(), Times.Never);
    }

    // =========================================================
    // T014 (US2): BackCommand should navigate immediately when no changes
    // =========================================================

    [Fact]
    public void BackCommand_ShouldNavigateImmediately_WhenNoChanges()
    {
        // Arrange
        var test = CreateTestDefinition();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNavigationService = new Mock<NavigationService>(mockServiceProvider.Object);
        mockNavigationService.CallBase = false;
        
        var mockDialogService = new Mock<DialogService>();

        var viewModel = new TestConfigViewModel(test, mockNavigationService.Object, mockDialogService.Object);
        // No changes made

        // Act
        viewModel.BackCommand.Execute(null);

        // Assert - Dialog should NOT be shown
        mockDialogService.Verify(
            x => x.ShowConfirmationDialog(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        
        // Navigation should happen directly
        mockNavigationService.Verify(x => x.GoBack(), Times.Once);
    }

    // =========================================================
    // T015 (US2): HasUnsavedChanges should be false when value reverted to original
    // =========================================================

    [Fact]
    public void HasUnsavedChanges_ShouldBeFalse_WhenValueRevertedToOriginal()
    {
        // Arrange
        var test = CreateTestDefinition(timeout: 5000);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        // Act
        viewModel.Timeout = 10000; // Change
        Assert.True(viewModel.HasUnsavedChanges); // Verify dirty
        viewModel.Timeout = 5000; // Revert

        // Assert
        Assert.False(viewModel.HasUnsavedChanges);
    }

    // =========================================================
    // T016 (US3): HasUnsavedChanges should be false after save
    // =========================================================

    [Fact]
    public async Task HasUnsavedChanges_ShouldBeFalse_AfterSave()
    {
        // Arrange
        var test = CreateTestDefinition(timeout: 5000);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        viewModel.Timeout = 10000; // Make dirty
        Assert.True(viewModel.HasUnsavedChanges);

        // Act
        await ((IAsyncRelayCommand)viewModel.SaveCommand).ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.HasUnsavedChanges);
    }

    // =========================================================
    // T017 (US3): HasUnsavedChanges should be true when changed after save
    // =========================================================

    [Fact]
    public async Task HasUnsavedChanges_ShouldBeTrue_WhenChangedAfterSave()
    {
        // Arrange
        var test = CreateTestDefinition(timeout: 5000, retryCount: 3);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        viewModel.Timeout = 10000; // Make dirty
        
        await ((IAsyncRelayCommand)viewModel.SaveCommand).ExecuteAsync(null); // Save
        Assert.False(viewModel.HasUnsavedChanges);

        // Act
        viewModel.RetryCount = 5; // New change after save

        // Assert
        Assert.True(viewModel.HasUnsavedChanges);
    }

    // =========================================================
    // Issue #2: Locked parameter should not trigger dirty state
    // =========================================================

    [Fact]
    public void HasUnsavedChanges_ShouldBeFalse_WhenLockedParameterValueChanged()
    {
        // Arrange
        var parameters = new JsonObject
        {
            ["ReadOnlyField"] = "original-value"
        };
        var fieldPolicy = new Dictionary<string, FieldPolicyType>
        {
            ["ReadOnlyField"] = FieldPolicyType.Locked
        };
        var test = CreateTestDefinition(parameters: parameters, fieldPolicy: fieldPolicy);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        // Act - Try to change the locked parameter (shouldn't affect dirty state)
        viewModel.Parameters[0].Value = "modified-value";

        // Assert - Locked parameters are excluded from dirty tracking
        Assert.False(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void HasUnsavedChanges_ShouldBeFalse_WhenHiddenParameterValueChanged()
    {
        // Arrange
        var parameters = new JsonObject
        {
            ["SecretField"] = "original-value"
        };
        var fieldPolicy = new Dictionary<string, FieldPolicyType>
        {
            ["SecretField"] = FieldPolicyType.Hidden
        };
        var test = CreateTestDefinition(parameters: parameters, fieldPolicy: fieldPolicy);
        var (viewModel, _, _) = CreateViewModelWithMocks(test);

        // Act - Try to change the hidden parameter
        viewModel.Parameters[0].Value = "modified-value";

        // Assert - Hidden parameters are excluded from dirty tracking
        Assert.False(viewModel.HasUnsavedChanges);
    }
}
