using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Interfaces;
using ReqChecker.App.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for test list view.
/// </summary>
public partial class TestListViewModel : ObservableObject, IDisposable
{
    private readonly IAppState _appState;
    private readonly NavigationService _navigationService;
    private readonly ITestRunner _testRunner;

    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private TestDefinition? _selectedTest;

    [ObservableProperty]
    private DialogService? _dialogService;

    [ObservableProperty]
    private bool _isRunning;

    /// <summary>
    /// Gets collection of selectable test items.
    /// </summary>
    public ObservableCollection<SelectableTestItem> SelectableTests { get; } = new();

    /// <summary>
    /// Gets whether any tests are selected.
    /// </summary>
    public bool HasSelectedTests => SelectableTests.Any(item => item.IsSelected);

    /// <summary>
    /// Gets whether all tests are selected. Returns true if all selected, false if none, null if mixed.
    /// </summary>
    public bool? IsAllSelected
    {
        get
        {
            if (SelectableTests.Count == 0)
            {
                return null;
            }

            var allSelected = SelectableTests.All(item => item.IsSelected);
            var noneSelected = SelectableTests.All(item => !item.IsSelected);

            if (allSelected) return true;
            if (noneSelected) return false;
            return null;
        }
    }

    /// <summary>
    /// Gets the count of selected tests.
    /// </summary>
    public int SelectedCount => SelectableTests.Count(item => item.IsSelected);

    /// <summary>
    /// Gets the total count of tests.
    /// </summary>
    public int TotalCount => SelectableTests.Count;

    /// <summary>
    /// Gets the run button label based on selection count.
    /// </summary>
    public string RunButtonLabel
    {
        get
        {
            if (SelectedCount == 0)
            {
                return "Run Tests";
            }
            if (SelectedCount == TotalCount)
            {
                return "Run All Tests";
            }
            return $"Run {SelectedCount} of {TotalCount} Tests";
        }
    }

    public TestListViewModel(IAppState appState, NavigationService navigationService, ITestRunner testRunner)
    {
        _appState = appState;
        _navigationService = navigationService;
        _testRunner = testRunner;

        // Get current profile from shared state
        CurrentProfile = _appState.CurrentProfile;

        // Subscribe to profile changes
        _appState.CurrentProfileChanged += OnCurrentProfileChanged;

        // Initialize SelectableTests if profile is already loaded
        if (CurrentProfile != null)
        {
            PopulateSelectableTests(CurrentProfile);
        }
    }

    private void OnCurrentProfileChanged(object? sender, EventArgs e)
    {
        CurrentProfile = _appState.CurrentProfile;

        // Rebuild SelectableTests when profile changes
        if (CurrentProfile != null)
        {
            PopulateSelectableTests(CurrentProfile);
        }
        else
        {
            // Unsubscribe from old items' PropertyChanged events
            foreach (var item in SelectableTests)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
            SelectableTests.Clear();
        }
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableTestItem.IsSelected))
        {
            OnPropertyChanged(nameof(HasSelectedTests));
            OnPropertyChanged(nameof(IsAllSelected));
            OnPropertyChanged(nameof(RunButtonLabel));
        }
    }

    /// <summary>
    /// Populates SelectableTests from given profile.
    /// </summary>
    private void PopulateSelectableTests(Profile profile)
    {
        SelectableTests.Clear();

        foreach (var test in profile.Tests)
        {
            var item = new SelectableTestItem(test);
            item.PropertyChanged += OnItemPropertyChanged;
            SelectableTests.Add(item);
        }

        OnPropertyChanged(nameof(HasSelectedTests));
        OnPropertyChanged(nameof(IsAllSelected));
    }

    /// <summary>
    /// Toggles all test selections.
    /// </summary>
    [RelayCommand]
    private void ToggleSelectAll()
    {
        // If indeterminate or none selected, check all
        // If all selected, uncheck all
        var newState = IsAllSelected != true;
        foreach (var item in SelectableTests)
        {
            item.IsSelected = newState;
        }
    }

    /// <summary>
    /// Runs selected tests in current profile.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRunAllTests))]
    private void RunAllTests()
    {
        if (CurrentProfile == null)
        {
            return;
        }

        // Store selected test IDs in AppState
        var selectedTestIds = SelectableTests
            .Where(item => item.IsSelected)
            .Select(item => item.Test.Id)
            .ToList();

        _appState.SetSelectedTestIds(selectedTestIds);

        // Navigate to run progress view - it will handle actual execution
        _navigationService.NavigateToRunProgress();
    }

    /// <summary>
    /// Determines if RunAllTests command can execute.
    /// </summary>
    private bool CanRunAllTests()
    {
        return CurrentProfile != null && HasSelectedTests;
    }

    /// <summary>
    /// Navigates to test configuration for selected test.
    /// </summary>
    [RelayCommand]
    private void NavigateToTestConfig(TestDefinition? test)
    {
        if (CurrentProfile == null || test == null)
            return;

        // Navigate to TestConfigView
        _navigationService.NavigateToTestConfig(test);
    }

    /// <summary>
    /// Navigates to profile selector view.
    /// </summary>
    [RelayCommand]
    private void NavigateToProfiles()
    {
        _navigationService.NavigateToProfileSelector();
    }

    /// <summary>
    /// Disposes resources and unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        _appState.CurrentProfileChanged -= OnCurrentProfileChanged;

        // Unsubscribe from all items' PropertyChanged events
        foreach (var item in SelectableTests)
        {
            item.PropertyChanged -= OnItemPropertyChanged;
        }
    }
}
