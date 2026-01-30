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
    /// Navigates to profile selector view.
    /// </summary>
    public void NavigateToProfileSelector()
    {
        var viewModel = _serviceProvider.GetRequiredService<ProfileSelectorViewModel>();
        var view = new Views.ProfileSelectorView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates to test configuration view.
    /// </summary>
    public void NavigateToTestConfig(ProfileModel profile)
    {
        var firstTest = profile.Tests.FirstOrDefault();
        if (firstTest != null)
        {
            var viewModel = new TestConfigViewModel(firstTest);
            var view = new Views.TestConfigView(viewModel);
            _frame?.Navigate(view);
        }
    }

    /// <summary>
    /// Navigates to test list view.
    /// </summary>
    public void NavigateToTestList()
    {
        var viewModel = _serviceProvider.GetRequiredService<TestListViewModel>();
        var view = new Views.TestListView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates to run progress view.
    /// </summary>
    public void NavigateToRunProgress()
    {
        var viewModel = _serviceProvider.GetRequiredService<RunProgressViewModel>();
        var view = new Views.RunProgressView(viewModel);
        _frame?.Navigate(view);
    }

    /// <summary>
    /// Navigates to results view.
    /// </summary>
    public void NavigateToResults()
    {
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
}
