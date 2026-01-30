using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Services;

/// <summary>
/// Manages navigation between views.
/// </summary>
public class NavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Frame? _frame;

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
    }

    /// <summary>
    /// Navigates to the test list view.
    /// </summary>
    public void NavigateToTestList()
    {
        var viewModel = _serviceProvider.GetRequiredService<TestListViewModel>();
        var view = new Views.TestListView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates to the run progress view.
    /// </summary>
    public void NavigateToRunProgress()
    {
        var viewModel = _serviceProvider.GetRequiredService<RunProgressViewModel>();
        var view = new Views.RunProgressView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates to the results view.
    /// </summary>
    public void NavigateToResults()
    {
        var viewModel = _serviceProvider.GetRequiredService<ResultsViewModel>();
        var view = new Views.ResultsView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates back to the previous view.
    /// </summary>
    public void GoBack()
    {
        if (_frame != null && _frame.CanGoBack)
        {
            _frame.GoBack();
        }
    }
}
