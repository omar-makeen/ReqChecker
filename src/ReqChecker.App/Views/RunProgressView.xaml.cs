using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Threading;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Code-behind for RunProgressView with screen reader support.
/// </summary>
public partial class RunProgressView
{
    private readonly RunProgressViewModel _viewModel;

    // Stick-to-bottom auto-scroll state for the Completed Tests list.
    // True initially because an empty list is implicitly "at the bottom".
    private bool _isCompletedListAtBottom = true;

    public RunProgressView(RunProgressViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Subscribe to property changes for screen reader announcements
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Subscribe to test-result collection changes to follow the latest entry
        _viewModel.TestResults.CollectionChanged += OnTestResultsCollectionChanged;

        // Clean up subscription when unloaded
        Unloaded += OnUnloaded;

        // Start tests when the view is loaded
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Start test execution automatically
        await _viewModel.StartTestsAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(RunProgressViewModel.CurrentTestName):
                AnnounceForScreenReader($"Running test: {_viewModel.CurrentTestName}");
                break;
            case nameof(RunProgressViewModel.ProgressPercentage):
                // Only announce at 25% intervals to avoid too many announcements
                var progress = (int)_viewModel.ProgressPercentage;
                if (progress == 25 || progress == 50 || progress == 75 || progress == 100)
                {
                    AnnounceForScreenReader($"Test progress: {progress} percent complete");
                }
                break;
            case nameof(RunProgressViewModel.IsComplete):
                if (_viewModel.IsComplete)
                {
                    AnnounceForScreenReader("All tests completed");
                }
                break;
        }
    }

    /// <summary>
    /// Announces a message to screen readers using UI Automation.
    /// </summary>
    private void AnnounceForScreenReader(string message)
    {
        try
        {
            // Use Dispatcher to ensure we're on the UI thread
            Dispatcher.BeginInvoke(() =>
            {
                AutomationProperties.SetName(this, message);
                AutomationProperties.SetLiveSetting(this, AutomationLiveSetting.Polite);

                var peer = UIElementAutomationPeer.FromElement(this);
                peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            });
        }
        catch
        {
            // Silently fail if automation is not available
        }
    }

    /// <summary>
    /// Tracks whether the user is at the bottom of the Completed Tests list.
    /// When at the bottom, newly completed tests auto-scroll into view; when the
    /// user has scrolled up to inspect an earlier result, follow-mode pauses.
    /// </summary>
    private void OnCompletedTestsScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // 2-px tolerance for floating-point and sub-pixel rendering.
        _isCompletedListAtBottom = e.ExtentHeight - (e.VerticalOffset + e.ViewportHeight) < 2.0;
    }

    /// <summary>
    /// When a test completes, scroll the Completed Tests list to the bottom so the
    /// new entry is visible — but only if the user hasn't scrolled up to read an
    /// earlier result. On Reset (run restart) follow-mode resumes for the new run.
    /// </summary>
    private void OnTestResultsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _isCompletedListAtBottom = true;
            return;
        }

        if (e.Action != NotifyCollectionChangedAction.Add || !_isCompletedListAtBottom)
        {
            return;
        }

        // Defer to Background priority so the layout pass can expand ExtentHeight
        // for the newly added item before we scroll. Without this, ScrollToEnd
        // would target the previous (smaller) extent and miss the new entry.
        CompletedTestsScrollViewer.Dispatcher.BeginInvoke(
            new Action(() => CompletedTestsScrollViewer.ScrollToEnd()),
            DispatcherPriority.Background);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.TestResults.CollectionChanged -= OnTestResultsCollectionChanged;
    }
}
