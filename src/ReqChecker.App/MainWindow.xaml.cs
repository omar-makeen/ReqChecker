using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Media.Animation;
using ReqChecker.App.ViewModels;
using ReqChecker.App.Services;
using ReqChecker.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Serilog;

namespace ReqChecker.App;

/// <summary>
/// Main window with FluentWindow base for Mica backdrop and modern UI.
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;
    private readonly NavigationService _navigationService;
    private readonly ThemeService _themeService;
    private readonly ISchedulerService _schedulerService;
    private bool _isNavigating;
    private Hardcodet.Wpf.TaskbarNotification.TaskbarIcon? _taskbarIcon;
    private System.Windows.Controls.MenuItem? _nextRunMenuItem;

    public MainWindow()
    {
        InitializeComponent();

        // Get services from DI
        _viewModel = App.Services.GetRequiredService<MainViewModel>();
        _navigationService = App.Services.GetRequiredService<NavigationService>();
        _themeService = App.Services.GetRequiredService<ThemeService>();
        _schedulerService = App.Services.GetRequiredService<ISchedulerService>();

        // Initialize navigation with the content frame
        _navigationService.Initialize(ContentFrame);
        _navigationService.Navigated += OnNavigationServiceNavigated;

        // Set up ViewModel with services
        _viewModel.NavigationService = _navigationService;
        _viewModel.ThemeService = _themeService;
        DataContext = _viewModel;

        // Click events are wired in XAML on each NavigationViewItem
        // This is more reliable than ItemInvoked/SelectionChanged with custom navigation

        // Navigate to test list by default with fade animation
        Loaded += OnWindowLoaded;
        Closed += OnWindowClosed;
    }

    // ── Tray Icon ─────────────────────────────────────────────────────

    private void InitializeTrayIcon()
    {
        // Build WPF context menu for the tray icon
        var contextMenu = new System.Windows.Controls.ContextMenu();

        var openItem = new System.Windows.Controls.MenuItem { Header = "Open ReqChecker" };
        openItem.Click += (_, _) => RestoreWindow();
        contextMenu.Items.Add(openItem);

        _nextRunMenuItem = new System.Windows.Controls.MenuItem
        {
            Header = "No upcoming runs",
            IsEnabled = false
        };
        contextMenu.Items.Add(_nextRunMenuItem);
        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => System.Windows.Application.Current.Shutdown();
        contextMenu.Items.Add(exitItem);

        // Create the TaskbarIcon (Hardcodet.Wpf.TaskbarNotification)
        _taskbarIcon = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon
        {
            ToolTipText = "ReqChecker",
            ContextMenu = contextMenu,
            Visibility = Visibility.Collapsed
        };

        _taskbarIcon.TrayLeftMouseUp += (_, _) => RestoreWindow();

        // Subscribe to scheduler to update next-run tooltip
        _schedulerService.ScheduleRunCompleted += (_, _) => Dispatcher.InvokeAsync(UpdateNextRunMenuItem);
    }

    private void UpdateNextRunMenuItem()
    {
        if (_nextRunMenuItem == null) return;
        var schedules = _schedulerService.GetSchedulesAsync().GetAwaiter().GetResult();
        var next = schedules
            .Where(s => s.NextRunTime.HasValue)
            .OrderBy(s => s.NextRunTime)
            .FirstOrDefault();

        _nextRunMenuItem.Header = next?.NextRunTime.HasValue == true
            ? $"Next: {next.Name} at {next.NextRunTime.Value:h:mm tt}"
            : "No upcoming runs";
    }

    private void RestoreWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        if (_taskbarIcon != null)
            _taskbarIcon.Visibility = Visibility.Collapsed;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_schedulerService.HasActiveSchedules)
        {
            e.Cancel = true;
            Hide();
            if (_taskbarIcon != null)
            {
                UpdateNextRunMenuItem();
                _taskbarIcon.Visibility = Visibility.Visible;
                _taskbarIcon.ShowBalloonTip("ReqChecker",
                    "Active schedules will continue to run in the background.",
                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
        }
        else
        {
            base.OnClosing(e);
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _taskbarIcon?.Dispose();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        // Initialize system tray icon
        InitializeTrayIcon();

        // Apply window fade-in animation if not reduced motion
        if (!_themeService.IsReducedMotionEnabled)
        {
            ApplyWindowFadeIn();
        }

        // Check if a startup profile was loaded
        var appState = App.Services.GetRequiredService<IAppState>();
        if (appState.CurrentProfile != null)
        {
            // Startup profile was loaded, navigate directly to test list
            _navigationService.NavigateToTestList();
        }
        else
        {
            // No startup profile, show profile selector
            _navigationService.NavigateToProfileSelector();
        }
    }

    /// <summary>
    /// Clears the selection state of all navigation items.
    /// </summary>
    private void ClearNavigationSelection()
    {
        NavProfiles.IsActive = false;
        NavTests.IsActive = false;
        NavResults.IsActive = false;
        NavHistory.IsActive = false;
        NavSchedules.IsActive = false;
        NavDiagnostics.IsActive = false;
        NavSettings.IsActive = false;
    }

    /// <summary>
    /// Sets the navigation selection to the specified tag.
    /// Clears all other selections before setting the new one.
    /// </summary>
    /// <param name="tag">The navigation tag to select (Profiles, Tests, Results, Diagnostics, Settings)</param>
    private void SetNavigationSelection(string tag)
    {
        ClearNavigationSelection();
        switch (tag)
        {
            case "Profiles":
                NavProfiles.IsActive = true;
                break;
            case "Tests":
                NavTests.IsActive = true;
                break;
            case "Results":
                NavResults.IsActive = true;
                break;
            case "History":
                NavHistory.IsActive = true;
                break;
            case "Schedules":
                NavSchedules.IsActive = true;
                break;
            case "Diagnostics":
                NavDiagnostics.IsActive = true;
                break;
            case "Settings":
                NavSettings.IsActive = true;
                break;
        }
    }

    /// <summary>
    /// Handles NavigationService.Navigated event to sync sidebar selection.
    /// </summary>
    private void OnNavigationServiceNavigated(string viewName)
    {
        SetNavigationSelection(viewName);
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

    private void NavItem_Click(object sender, RoutedEventArgs args)
    {
        Serilog.Log.Information("NavItem_Click called");

        if (_isNavigating) return;

        var clickedItem = sender as NavigationViewItem;
        var tag = clickedItem?.Tag?.ToString();
        Serilog.Log.Information("Clicked item tag: {Tag}", tag);

        if (string.IsNullOrEmpty(tag)) return;

        NavigateWithAnimation(tag);
    }

    private async void NavigateWithAnimation(string tag)
    {
        Log.Information("NavigateWithAnimation: tag={Tag}, _isNavigating={IsNav}", tag, _isNavigating);
        _isNavigating = true;

        try
        {
            // Apply fade-out animation if not reduced motion
            if (!_themeService.IsReducedMotionEnabled)
            {
                await ApplyViewFadeOut();
            }

            // Navigate to the selected view and get view name for announcement
            string viewName = tag switch
            {
                "Profiles" => "Profile Selector",
                "Tests" => "Test List",
                "Results" => "Test Results",
                "History" => "Test History",
                "Schedules" => "Schedules",
                "Diagnostics" => "Diagnostics",
                "Settings" => "Settings",
                _ => tag
            };

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
                case "History":
                    _navigationService.NavigateToHistory();
                    break;
                case "Schedules":
                    _navigationService.NavigateToSchedules();
                    break;
                case "Diagnostics":
                    _navigationService.NavigateToDiagnostics();
                    break;
                case "Settings":
                    _navigationService.NavigateToSettings();
                    break;
            }

            // Announce navigation change for screen readers
            AnnounceForScreenReader($"Navigated to {viewName}");

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

    /// <summary>
    /// Announces a message to screen readers using UI Automation.
    /// </summary>
    private void AnnounceForScreenReader(string message)
    {
        try
        {
            var peer = UIElementAutomationPeer.FromElement(this);
            peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);

            // Set the automation name on the content frame to announce the change
            System.Windows.Automation.AutomationProperties.SetName(ContentFrame, message);
            System.Windows.Automation.AutomationProperties.SetLiveSetting(ContentFrame, System.Windows.Automation.AutomationLiveSetting.Polite);
        }
        catch
        {
            // Silently fail if automation is not available
        }
    }

    private Task ApplyViewFadeOut()
    {
        var tcs = new TaskCompletionSource<bool>();

        var storyboard = FindResource("ViewFadeOut") as Storyboard;
        if (storyboard == null)
        {
            Log.Warning("ViewFadeOut storyboard NOT FOUND");
        }
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
