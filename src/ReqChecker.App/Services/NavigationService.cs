using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Models;
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

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private void DisposeCurrentViewModel()
    {
        _currentViewModel?.Dispose();
        _currentViewModel = null;
    }

    /// <summary>
    /// Initializes the navigation service with a frame.
    /// </summary>
    public void Initialize(Frame frame)
    {
        _frame = frame;
    }

    /// <summary>
    /// Navigates to profile selector view.
    /// </summary>
    public void NavigateToProfileSelector()
    {
        DisposeCurrentViewModel();
        var viewModel = _serviceProvider.GetRequiredService<ProfileSelectorViewModel>();
        var view = new Views.ProfileSelectorView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates to test configuration view.
    /// </summary>
    public void NavigateToTestConfig(TestDefinition test)
    {
        if (test != null)
        {
            DisposeCurrentViewModel();
            var viewModel = new TestConfigViewModel(test);
            var view = new Views.TestConfigView(viewModel);
            _frame?.Navigate(view);
        }
    }

    /// <summary>
    /// Navigates to test list view.
    /// </summary>
    public void NavigateToTestList()
    {
        DisposeCurrentViewModel();
        var viewModel = _serviceProvider.GetRequiredService<TestListViewModel>();
        _currentViewModel = viewModel;
        var view = new Views.TestListView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates to run progress view.
    /// </summary>
    public void NavigateToRunProgress()
    {
        DisposeCurrentViewModel();
        var viewModel = _serviceProvider.GetRequiredService<RunProgressViewModel>();
        var view = new Views.RunProgressView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates to results view.
    /// </summary>
    public void NavigateToResults()
    {
        DisposeCurrentViewModel();
        var viewModel = _serviceProvider.GetRequiredService<ResultsViewModel>();
        var view = new Views.ResultsView(viewModel);
        _frame?.Navigate(view);
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
        DisposeCurrentViewModel();
        var viewModel = _serviceProvider.GetRequiredService<DiagnosticsViewModel>();
        var view = new Views.DiagnosticsView(viewModel);
        _frame?.Navigate(view);
    }
}
