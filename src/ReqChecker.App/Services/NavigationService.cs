using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Models;
using Serilog;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.App.Services;

/// <summary>
/// Manages navigation between views.
/// </summary>
public class NavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Frame? _frame;
    private IDisposable? _currentViewModel;

    /// <summary>
    /// Raised after navigation completes with the view name for sidebar synchronization.
    /// </summary>
    public event Action<string>? Navigated;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Initializes the navigation service with a frame.
    /// </summary>
    public void Initialize(Frame frame)
    {
        _frame = frame;
        Log.Information("NavigationService.Initialize: Frame set, IsNull={IsNull}", frame == null);
    }

    /// <summary>
    /// Navigates to profile selector view.
    /// </summary>
    public void NavigateToProfileSelector()
    {
        Log.Information("NavigateToProfileSelector: Frame null={FrameNull}", _frame == null);
        if (_frame == null)
        {
            Log.Error("Cannot navigate to ProfileSelector: Frame not initialized");
            return;
        }
        var viewModel = _serviceProvider.GetRequiredService<ProfileSelectorViewModel>();
        TrackViewModel(viewModel);
        var view = new Views.ProfileSelectorView(viewModel);
        var result = _frame.Navigate(view);
        Log.Information("NavigateToProfileSelector: Navigate result={Result}", result);
        RaiseNavigated("Profiles");
    }

    /// <summary>
    /// Navigates to test configuration view.
    /// </summary>
    public void NavigateToTestConfig(TestDefinition test)
    {
        Log.Information("NavigateToTestConfig: Frame null={FrameNull}, test null={TestNull}", _frame == null, test == null);
        if (_frame == null)
        {
            Log.Error("Cannot navigate to TestConfig: Frame not initialized");
            return;
        }
        if (test != null)
        {
            var viewModel = new TestConfigViewModel(test, this);
            TrackViewModel(viewModel);
            var view = new Views.TestConfigView(viewModel);
            var result = _frame.Navigate(view);
            Log.Information("NavigateToTestConfig: Navigate result={Result}", result);
        }
    }

    /// <summary>
    /// Navigates to test list view.
    /// </summary>
    public void NavigateToTestList()
    {
        Log.Information("NavigateToTestList: Frame null={FrameNull}", _frame == null);
        if (_frame == null)
        {
            Log.Error("Cannot navigate to TestList: Frame not initialized");
            return;
        }
        var viewModel = _serviceProvider.GetRequiredService<TestListViewModel>();
        TrackViewModel(viewModel);
        var view = new Views.TestListView(viewModel);
        var result = _frame.Navigate(view);
        Log.Information("NavigateToTestList: Navigate result={Result}", result);
        RaiseNavigated("Tests");
    }

    /// <summary>
    /// Navigates to test list view with a specific profile.
    /// Sets the profile in AppState before navigation.
    /// </summary>
    /// <param name="profile">The profile to load.</param>
    public void NavigateToTestListWithProfile(ProfileModel profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        // Set the profile in app state
        var appState = _serviceProvider.GetRequiredService<IAppState>();
        appState.SetCurrentProfile(profile);

        // Navigate to test list
        NavigateToTestList();
    }

    /// <summary>
    /// Navigates to run progress view.
    /// </summary>
    public void NavigateToRunProgress()
    {
        Log.Information("NavigateToRunProgress: Frame null={FrameNull}", _frame == null);
        if (_frame == null)
        {
            Log.Error("Cannot navigate to RunProgress: Frame not initialized");
            return;
        }
        var viewModel = _serviceProvider.GetRequiredService<RunProgressViewModel>();
        TrackViewModel(viewModel);
        var view = new Views.RunProgressView(viewModel);
        var result = _frame.Navigate(view);
        Log.Information("NavigateToRunProgress: Navigate result={Result}", result);
    }

    /// <summary>
    /// Navigates to results view.
    /// Loads the last run report from AppState and sets it on the ViewModel.
    /// </summary>
    public void NavigateToResults()
    {
        Log.Information("NavigateToResults: Frame null={FrameNull}", _frame == null);
        if (_frame == null)
        {
            Log.Error("Cannot navigate to Results: Frame not initialized");
            return;
        }
        var viewModel = _serviceProvider.GetRequiredService<ResultsViewModel>();
        var appState = _serviceProvider.GetRequiredService<IAppState>();

        // Load the last run report from AppState
        viewModel.Report = appState.LastRunReport;

        TrackViewModel(viewModel);
        var view = new Views.ResultsView(viewModel);
        var result = _frame.Navigate(view);
        Log.Information("NavigateToResults: Navigate result={Result}", result);
        RaiseNavigated("Results");
    }

    /// <summary>
    /// Navigates to history view.
    /// Loads history and sets it on ViewModel.
    /// </summary>
    public void NavigateToHistory()
    {
        Log.Information("NavigateToHistory: Frame null={FrameNull}", _frame == null);
        if (_frame == null)
        {
            Log.Error("Cannot navigate to History: Frame not initialized");
            return;
        }
        var viewModel = _serviceProvider.GetRequiredService<HistoryViewModel>();

        TrackViewModel(viewModel);
        var view = new Views.HistoryView(viewModel);
        var result = _frame.Navigate(view);
        Log.Information("NavigateToHistory: Navigate result={Result}", result);
        RaiseNavigated("History");
    }

    /// <summary>
    /// Navigates back to previous view.
    /// </summary>
    public void GoBack()
    {
        if (_frame != null && _frame.CanGoBack)
        {
            _frame.GoBack();
        }
    }

    /// <summary>
    /// Navigates to diagnostics view.
    /// </summary>
    public void NavigateToDiagnostics()
    {
        Log.Information("NavigateToDiagnostics: Frame null={FrameNull}", _frame == null);
        if (_frame == null)
        {
            Log.Error("Cannot navigate to Diagnostics: Frame not initialized");
            return;
        }
        var viewModel = _serviceProvider.GetRequiredService<DiagnosticsViewModel>();
        TrackViewModel(viewModel);
        var view = new Views.DiagnosticsView(viewModel);
        var result = _frame.Navigate(view);
        Log.Information("NavigateToDiagnostics: Navigate result={Result}", result);
        RaiseNavigated("Diagnostics");
    }

    /// <summary>
    /// Tracks and disposes ViewModels that implement IDisposable.
    /// </summary>
    private void TrackViewModel(object? viewModel)
    {
        // Dispose the previous ViewModel first
        _currentViewModel?.Dispose();

        if (viewModel is IDisposable disposable)
        {
            _currentViewModel = disposable;
        }
        else
        {
            _currentViewModel = null;
        }
    }

    /// <summary>
    /// Raises the Navigated event with the specified view name.
    /// </summary>
    private void RaiseNavigated(string viewName)
    {
        Navigated?.Invoke(viewName);
    }
}
