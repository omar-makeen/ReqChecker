using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Interfaces;
using ReqChecker.App.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for test list view.
/// </summary>
public partial class TestListViewModel : ObservableObject, IDisposable
{
    private readonly IAppState _appState;
    private readonly NavigationService _navigationService;
    private readonly ITestRunner _testRunner;
    private readonly IProfileValidator _profileValidator;

    [ObservableProperty]
    private Profile? _currentProfile;

    [ObservableProperty]
    private TestDefinition? _selectedTest;

    [ObservableProperty]
    private DialogService? _dialogService;

    [ObservableProperty]
    private string? _validationErrorMessage;

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
    /// Gets or sets whether all tests are selected. Returns true if all selected, false if none, null if mixed.
    /// Setting a value checks or unchecks all tests. When IsFilterActive is true, operates on visible items only.
    /// </summary>
    public bool? IsAllSelected
    {
        get
        {
            // When filter is active, check only visible items
            if (IsFilterActive && FilteredTestsView != null)
            {
                var visibleItems = FilteredTestsView.Cast<SelectableTestItem>().ToList();
                if (visibleItems.Count == 0)
                {
                    return null;
                }

                var allSelected = visibleItems.All(item => item.IsSelected);
                var noneSelected = visibleItems.All(item => !item.IsSelected);

                if (allSelected) return true;
                if (noneSelected) return false;
                return null;
            }

            // When no filter, check all items
            if (SelectableTests.Count == 0)
            {
                return null;
            }

            var allSelectedAll = SelectableTests.All(item => item.IsSelected);
            var noneSelectedAll = SelectableTests.All(item => !item.IsSelected);

            if (allSelectedAll) return true;
            if (noneSelectedAll) return false;
            return null;
        }
        set
        {
            var newState = value == true;
            foreach (var item in SelectableTests)
                item.IsSelected = newState;
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
    /// Gets the count of tests passing the current filter.
    /// </summary>
    public int FilteredCount => FilteredTestsView?.Cast<SelectableTestItem>().Count() ?? 0;

    /// <summary>
    /// Gets whether a filter is active (search text is non-empty after trimming).
    /// </summary>
    public bool IsFilterActive => !string.IsNullOrWhiteSpace(SearchText);

    /// <summary>
    /// Gets the test count display string. Returns "X of Y tests" when filtered, "Y tests" when unfiltered.
    /// </summary>
    public string TestCountDisplay
    {
        get
        {
            if (IsFilterActive)
            {
                return $"{FilteredCount} of {TotalCount} tests";
            }
            return $"{TotalCount} tests";
        }
    }

    /// <summary>
    /// Gets whether there are no filter results (filter is active but no items match).
    /// </summary>
    public bool IsNoFilterResults => IsFilterActive && FilteredCount == 0;

    /// <summary>
    /// Gets whether the test list should be shown (profile loaded and no empty filter results).
    /// </summary>
    public bool ShouldShowTestList => CurrentProfile != null && !IsNoFilterResults;

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Gets the filtered collection view of tests.
    /// </summary>
    public ICollectionView? FilteredTestsView { get; private set; }

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

    public TestListViewModel(IAppState appState, NavigationService navigationService, ITestRunner testRunner, IProfileValidator profileValidator)
    {
        _appState = appState;
        _navigationService = navigationService;
        _testRunner = testRunner;
        _profileValidator = profileValidator;

        // Get current profile from shared state
        CurrentProfile = _appState.CurrentProfile;

        // Subscribe to profile changes
        _appState.CurrentProfileChanged += OnCurrentProfileChanged;

        // Initialize SelectableTests if profile is already loaded
        if (CurrentProfile != null)
        {
            _ = PopulateSelectableTests(CurrentProfile);
        }
    }

    private async void OnCurrentProfileChanged(object? sender, EventArgs e)
    {
        CurrentProfile = _appState.CurrentProfile;

        // Rebuild SelectableTests when profile changes
        if (CurrentProfile != null)
        {
            await PopulateSelectableTests(CurrentProfile);
        }
        else
        {
            // Unsubscribe from old items' PropertyChanged events
            foreach (var item in SelectableTests)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
            SelectableTests.Clear();
            ValidationErrorMessage = null;
            OnPropertyChanged(nameof(HasSelectedTests));
            OnPropertyChanged(nameof(IsAllSelected));
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(RunButtonLabel));
            OnPropertyChanged(nameof(ValidationErrorMessage));
            RunAllTestsCommand.NotifyCanExecuteChanged();
        }
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableTestItem.IsSelected))
        {
            OnPropertyChanged(nameof(HasSelectedTests));
            OnPropertyChanged(nameof(IsAllSelected));
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(RunButtonLabel));
            RunAllTestsCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Called when SearchText property changes. Refreshes the filter and updates computed properties.
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        FilteredTestsView?.Refresh();
        OnPropertyChanged(nameof(FilteredCount));
        OnPropertyChanged(nameof(IsFilterActive));
        OnPropertyChanged(nameof(TestCountDisplay));
        OnPropertyChanged(nameof(IsAllSelected));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(RunButtonLabel));
        OnPropertyChanged(nameof(IsNoFilterResults));
        OnPropertyChanged(nameof(ShouldShowTestList));
    }

    /// <summary>
    /// Sets up the filtered collection view with a filter predicate.
    /// </summary>
    private void SetupFilteredView()
    {
        var view = CollectionViewSource.GetDefaultView(SelectableTests);
        view.Filter = FilterTest;
        FilteredTestsView = view;
    }

    /// <summary>
    /// Filter predicate for test items. Returns true if the item matches the search text.
    /// </summary>
    private bool FilterTest(object obj)
    {
        if (obj is not SelectableTestItem item)
            return false;

        // Return true for all items when search is empty or whitespace
        var trimmedSearch = SearchText.Trim();
        if (string.IsNullOrWhiteSpace(trimmedSearch))
            return true;

        // Match against DisplayName (US1: name search), Type (US2: type search), Description (US3: description search)
        return item.Test.DisplayName.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase)
            || item.Test.Type.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase)
            || (item.Test.Description?.Contains(trimmedSearch, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Populates SelectableTests from given profile.
    /// </summary>
    private async Task PopulateSelectableTests(Profile profile)
    {
        // Clear search text before rebuilding the collection
        SearchText = string.Empty;

        // Unsubscribe from old items before clearing
        foreach (var item in SelectableTests)
            item.PropertyChanged -= OnItemPropertyChanged;

        SelectableTests.Clear();

        foreach (var test in profile.Tests)
        {
            var item = new SelectableTestItem(test);
            item.PropertyChanged += OnItemPropertyChanged;
            item.UpdateDependencyDisplayText(profile.Tests);
            SelectableTests.Add(item);
        }

        // Validate the profile and check for dependency-related errors
        var validationResult = await _profileValidator.ValidateAsync(profile);
        if (validationResult.Any())
        {
            // Set validation error message with summary of errors
            ValidationErrorMessage = $"Profile validation errors: {string.Join("; ", validationResult)}";
        }
        else
        {
            ValidationErrorMessage = null;
        }

        // Set up the filtered view after populating the collection
        SetupFilteredView();

        OnPropertyChanged(nameof(HasSelectedTests));
        OnPropertyChanged(nameof(IsAllSelected));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(RunButtonLabel));
        OnPropertyChanged(nameof(ValidationErrorMessage));
        OnPropertyChanged(nameof(FilteredCount));
        OnPropertyChanged(nameof(IsFilterActive));
        OnPropertyChanged(nameof(TestCountDisplay));
        OnPropertyChanged(nameof(IsNoFilterResults));
        OnPropertyChanged(nameof(ShouldShowTestList));
        RunAllTestsCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Toggles all test selections. When IsFilterActive is true, toggles only visible items.
    /// </summary>
    [RelayCommand]
    private void ToggleSelectAll()
    {
        // If indeterminate or none selected, check all
        // If all selected, uncheck all
        var newState = IsAllSelected != true;

        // When filter is active, iterate only visible items
        if (IsFilterActive && FilteredTestsView != null)
        {
            foreach (var item in FilteredTestsView.Cast<SelectableTestItem>())
            {
                item.IsSelected = newState;
            }
        }
        else
        {
            // When no filter, iterate all items
            foreach (var item in SelectableTests)
            {
                item.IsSelected = newState;
            }
        }
    }

    /// <summary>
    /// Clears the search text.
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
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

        // Store selected test IDs in AppState (null = run all, avoids unnecessary filtering)
        if (SelectableTests.All(item => item.IsSelected))
        {
            _appState.SetSelectedTestIds(null);
        }
        else
        {
            var selectedTestIds = SelectableTests
                .Where(item => item.IsSelected)
                .Select(item => item.Test.Id)
                .ToList();

            _appState.SetSelectedTestIds(selectedTestIds);
        }

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
