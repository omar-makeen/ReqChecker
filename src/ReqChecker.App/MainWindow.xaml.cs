using ReqChecker.App.ViewModels;
using ReqChecker.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ReqChecker.App;

/// <summary>
/// Main window code-behind.
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    private readonly MainViewModel _viewModel;
    private readonly NavigationService _navigationService;

    public MainWindow()
    {
        InitializeComponent();

        // Get services from DI
        _viewModel = App.Services.GetRequiredService<MainViewModel>();
        _navigationService = App.Services.GetRequiredService<NavigationService>();

        // Initialize navigation with the content frame
        _navigationService.Initialize(ContentFrame);

        // Set up ViewModel with navigation service
        _viewModel.NavigationService = _navigationService;
        DataContext = _viewModel;

        // Navigate to test list by default
        Loaded += (s, e) => _navigationService.NavigateToTestList();
    }
}
