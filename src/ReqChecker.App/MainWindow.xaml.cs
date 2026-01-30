using System.Windows;
using System.Windows.Media.Animation;
using ReqChecker.App.ViewModels;
using ReqChecker.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace ReqChecker.App;

/// <summary>
/// Main window with FluentWindow base for Mica backdrop and modern UI.
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;
    private readonly NavigationService _navigationService;
    private readonly ThemeService _themeService;
    private bool _isNavigating;

    public MainWindow()
    {
        InitializeComponent();

        // Get services from DI
        _viewModel = App.Services.GetRequiredService<MainViewModel>();
        _navigationService = App.Services.GetRequiredService<NavigationService>();
        _themeService = App.Services.GetRequiredService<ThemeService>();

        // Initialize navigation with the content frame
        _navigationService.Initialize(ContentFrame);

        // Set up ViewModel with services
        _viewModel.NavigationService = _navigationService;
        _viewModel.ThemeService = _themeService;
        DataContext = _viewModel;

        // Navigate to test list by default with fade animation
        Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        // Apply window fade-in animation if not reduced motion
        if (!_themeService.IsReducedMotionEnabled)
        {
            ApplyWindowFadeIn();
        }

        // Navigate to default view
        _navigationService.NavigateToTestList();

        // Navigate sets the active navigation item via the NavigationView
        NavTests.IsActive = true;
    }

    private void ApplyWindowFadeIn()
    {
        this.Opacity = 0;
        var storyboard = (Storyboard)FindResource("WindowFadeIn");
        if (storyboard != null)
        {
            var clone = storyboard.Clone();
            clone.Begin(this);
        }
        else
        {
            // Fallback if storyboard not found
            this.Opacity = 1;
        }
    }

    private void NavigationView_SelectionChanged(NavigationView sender, RoutedEventArgs args)
    {
        if (_isNavigating) return;

        var selectedItem = sender.SelectedItem as NavigationViewItem;
        var tag = selectedItem?.Tag?.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        // Handle theme toggle separately
        if (tag == "Theme")
        {
            _themeService.ToggleTheme();
            // Reset selection to previous item since theme is an action, not navigation
            return;
        }

        NavigateWithAnimation(tag);
    }

    private async void NavigateWithAnimation(string tag)
    {
        _isNavigating = true;

        try
        {
            // Apply fade-out animation if not reduced motion
            if (!_themeService.IsReducedMotionEnabled)
            {
                await ApplyViewFadeOut();
            }

            // Navigate to the selected view
            switch (tag)
            {
                case "Profiles":
                    _navigationService.NavigateToProfileSelector();
                    break;
                case "Tests":
                    _navigationService.NavigateToTestList();
                    break;
                case "Results":
                    _navigationService.NavigateToResults();
                    break;
                case "Diagnostics":
                    _navigationService.NavigateToDiagnostics();
                    break;
            }

            // Apply fade-in animation if not reduced motion
            if (!_themeService.IsReducedMotionEnabled)
            {
                await ApplyViewFadeIn();
            }
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private Task ApplyViewFadeOut()
    {
        var tcs = new TaskCompletionSource<bool>();

        var storyboard = (Storyboard)FindResource("ViewFadeOut");
        if (storyboard != null)
        {
            var clone = storyboard.Clone();
            clone.Completed += (s, e) => tcs.TrySetResult(true);
            clone.Begin(ContentFrame);
        }
        else
        {
            ContentFrame.Opacity = 0;
            tcs.TrySetResult(true);
        }

        return tcs.Task;
    }

    private Task ApplyViewFadeIn()
    {
        var tcs = new TaskCompletionSource<bool>();

        // Reset transform for animation
        if (ContentFrame.RenderTransform is System.Windows.Media.TranslateTransform transform)
        {
            transform.Y = 10;
        }
        ContentFrame.Opacity = 0;

        var storyboard = (Storyboard)FindResource("ViewFadeIn");
        if (storyboard != null)
        {
            var clone = storyboard.Clone();
            clone.Completed += (s, e) => tcs.TrySetResult(true);
            clone.Begin(ContentFrame);
        }
        else
        {
            ContentFrame.Opacity = 1;
            if (ContentFrame.RenderTransform is System.Windows.Media.TranslateTransform t)
            {
                t.Y = 0;
            }
            tcs.TrySetResult(true);
        }

        return tcs.Task;
    }
}
