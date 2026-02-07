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
    private static TestListViewModel CreateViewModel(
        Mock<IAppState>? mockAppState = null,
        Profile? profile = null)
    {
        mockAppState ??= new Mock<IAppState>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var navigationService = new NavigationService(mockServiceProvider.Object);
        var mockTestRunner = new Mock<ITestRunner>();
        var mockProfileValidator = new Mock<IProfileValidator>();

        if (profile != null)
            mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        return new TestListViewModel(mockAppState.Object, navigationService, mockTestRunner.Object, mockProfileValidator.Object);
    }

    private static Profile CreateProfileWithTests(int count = 3)
    {
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        for (var i = 0; i < count; i++)
        {
            profile.Tests.Add(new TestDefinition
            {
                Id = $"test-{i + 1}",
                DisplayName = $"Test {i + 1}",
                Type = "Ping"
            });
        }
        return profile;
    }

    // =========================================================
    // Original tests (restored from HEAD~1)
    // =========================================================

    [Fact]
    public void Constructor_ShouldInitializeWithProfile()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        // Act
        var viewModel = CreateViewModel(mockAppState, profile);

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal(profile, viewModel.CurrentProfile);
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromProfileChangedEvent()
    {
        // Arrange
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        var viewModel = CreateViewModel(profile: profile);

        // Act
        viewModel.Dispose();

        // Assert - After dispose, event handler should not be invoked
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        var viewModel = CreateViewModel(profile: profile);

        // Act - Should not throw
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
        var profile1 = new Profile { Name = "Profile 1", SchemaVersion = 2 };
        var profile2 = new Profile { Name = "Profile 2", SchemaVersion = 2 };
        var viewModel = CreateViewModel(profile: profile1);

        // Act
        Assert.Equal(profile1, viewModel.CurrentProfile);
        viewModel.CurrentProfile = profile2;

        // Assert
        Assert.Equal(profile2, viewModel.CurrentProfile);
    }

    [Fact]
    public void SelectedTest_CanBeSet()
    {
        // Arrange
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        var test = new TestDefinition { DisplayName = "Test 1", Type = "Ping" };
        var viewModel = CreateViewModel(profile: profile);

        // Act
        viewModel.SelectedTest = test;

        // Assert
        Assert.Equal(test, viewModel.SelectedTest);
    }

    [Fact]
    public void NavigateToTestConfigCommand_ShouldNotThrowWhenTestIsNull()
    {
        // Arrange
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        var viewModel = CreateViewModel(profile: profile);

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => viewModel.NavigateToTestConfigCommand.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public void NavigateToProfilesCommand_Exists()
    {
        // Arrange
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        var viewModel = CreateViewModel(profile: profile);

        // Assert
        Assert.NotNull(viewModel.NavigateToProfilesCommand);
    }

    [Fact]
    public void RunAllTestsCommand_ShouldNotThrowWhenProfileIsNull()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act - Should not throw
        var exception = Record.Exception(() => viewModel.RunAllTestsCommand.Execute(null));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void RunAllTestsCommand_Exists()
    {
        // Arrange
        var profile = new Profile { Name = "Test Profile", SchemaVersion = 2 };
        var viewModel = CreateViewModel(profile: profile);

        // Assert
        Assert.NotNull(viewModel.RunAllTestsCommand);
    }

    // =========================================================
    // T005 (US1): Selection initialization and run behavior
    // =========================================================

    [Fact]
    public void SelectableTests_InitializedAllSelected_WhenProfileLoaded()
    {
        // Arrange & Act
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Assert - All tests should be selected by default
        Assert.Equal(3, viewModel.SelectableTests.Count);
        Assert.All(viewModel.SelectableTests, item => Assert.True(item.IsSelected));
    }

    [Fact]
    public void RunCommand_StoresNull_WhenAllTestsSelected()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var profile = CreateProfileWithTests(3);
        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        var viewModel = CreateViewModel(mockAppState, profile);

        // All tests are selected by default

        // Act
        viewModel.RunAllTestsCommand.Execute(null);

        // Assert - null means "run all" (optimization: no filtering needed)
        mockAppState.Verify(
            x => x.SetSelectedTestIds(null),
            Times.Once);
    }

    [Fact]
    public void RunCommand_StoresOnlySelectedTestIds_InAppState()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var profile = CreateProfileWithTests(3);
        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile);

        var viewModel = CreateViewModel(mockAppState, profile);

        // Deselect the second test
        viewModel.SelectableTests[1].IsSelected = false;

        // Act
        viewModel.RunAllTestsCommand.Execute(null);

        // Assert - Only selected test IDs stored
        mockAppState.Verify(
            x => x.SetSelectedTestIds(It.Is<IReadOnlyList<string>>(ids =>
                ids.Count == 2 &&
                ids.Contains("test-1") &&
                ids.Contains("test-3"))),
            Times.Once);
    }

    [Fact]
    public void RunCommand_Disabled_WhenNoTestsSelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Deselect all tests
        foreach (var item in viewModel.SelectableTests)
            item.IsSelected = false;

        // Assert
        Assert.False(viewModel.HasSelectedTests);
        Assert.False(viewModel.RunAllTestsCommand.CanExecute(null));
    }

    // =========================================================
    // T011 (US2): Select All / Deselect All
    // =========================================================

    [Fact]
    public void IsAllSelected_ReturnsTrue_WhenAllSelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // All are selected by default
        // Assert
        Assert.Equal(true, viewModel.IsAllSelected);
    }

    [Fact]
    public void IsAllSelected_ReturnsFalse_WhenNoneSelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Act - Deselect all
        foreach (var item in viewModel.SelectableTests)
            item.IsSelected = false;

        // Assert
        Assert.Equal(false, viewModel.IsAllSelected);
    }

    [Fact]
    public void IsAllSelected_ReturnsNull_WhenPartiallySelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Act - Deselect one
        viewModel.SelectableTests[1].IsSelected = false;

        // Assert
        Assert.Null(viewModel.IsAllSelected);
    }

    [Fact]
    public void ToggleSelectAll_ChecksAll_WhenNoneSelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Deselect all first
        foreach (var item in viewModel.SelectableTests)
            item.IsSelected = false;

        // Act
        viewModel.ToggleSelectAllCommand.Execute(null);

        // Assert
        Assert.All(viewModel.SelectableTests, item => Assert.True(item.IsSelected));
    }

    [Fact]
    public void ToggleSelectAll_UnchecksAll_WhenAllSelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // All selected by default
        Assert.Equal(true, viewModel.IsAllSelected);

        // Act
        viewModel.ToggleSelectAllCommand.Execute(null);

        // Assert
        Assert.All(viewModel.SelectableTests, item => Assert.False(item.IsSelected));
    }

    [Fact]
    public void IsAllSelected_Setter_ChecksAll_WhenSetToTrue()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Deselect all first
        foreach (var item in viewModel.SelectableTests)
            item.IsSelected = false;

        // Act - Use the setter (simulates TwoWay binding from checkbox)
        viewModel.IsAllSelected = true;

        // Assert
        Assert.All(viewModel.SelectableTests, item => Assert.True(item.IsSelected));
        Assert.Equal(true, viewModel.IsAllSelected);
    }

    [Fact]
    public void IsAllSelected_Setter_UnchecksAll_WhenSetToFalse()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Act - Use the setter
        viewModel.IsAllSelected = false;

        // Assert
        Assert.All(viewModel.SelectableTests, item => Assert.False(item.IsSelected));
        Assert.Equal(false, viewModel.IsAllSelected);
    }

    // =========================================================
    // T014 (US3): Run button label
    // =========================================================

    [Fact]
    public void RunButtonLabel_ShowsRunAllTests_WhenAllSelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Assert
        Assert.Equal("Run All Tests", viewModel.RunButtonLabel);
    }

    [Fact]
    public void RunButtonLabel_ShowsRunNofM_WhenPartiallySelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Act - Deselect one
        viewModel.SelectableTests[2].IsSelected = false;

        // Assert
        Assert.Equal("Run 2 of 3 Tests", viewModel.RunButtonLabel);
    }

    [Fact]
    public void RunButtonLabel_ShowsRunTests_WhenNoneSelected()
    {
        // Arrange
        var profile = CreateProfileWithTests(3);
        var viewModel = CreateViewModel(profile: profile);

        // Act - Deselect all
        foreach (var item in viewModel.SelectableTests)
            item.IsSelected = false;

        // Assert
        Assert.Equal("Run Tests", viewModel.RunButtonLabel);
    }

    // =========================================================
    // Issue 4: Memory leak regression test
    // =========================================================

    [Fact]
    public void PopulateSelectableTests_UnsubscribesOldItems_WhenProfileChanges()
    {
        // Arrange
        var mockAppState = new Mock<IAppState>();
        var profile1 = CreateProfileWithTests(2);
        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile1);

        var viewModel = CreateViewModel(mockAppState, profile1);
        var oldItems = viewModel.SelectableTests.ToList();

        // Act - Simulate profile change by raising the event
        var profile2 = CreateProfileWithTests(1);
        mockAppState.SetupGet(x => x.CurrentProfile).Returns(profile2);
        mockAppState.Raise(x => x.CurrentProfileChanged += null, mockAppState.Object, EventArgs.Empty);

        // Assert - New tests loaded
        Assert.Single(viewModel.SelectableTests);

        // Old items' property changes should NOT trigger updates on the viewModel
        // (If unsubscribed properly, changing old items has no effect)
        var propertyChangedCount = 0;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(viewModel.HasSelectedTests))
                propertyChangedCount++;
        };

        oldItems[0].IsSelected = false; // Should NOT trigger viewModel update
        Assert.Equal(0, propertyChangedCount);
    }
}
