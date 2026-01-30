using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Code-behind for RunProgressView with screen reader support.
/// </summary>
public partial class RunProgressView
{
    private readonly RunProgressViewModel _viewModel;

    public RunProgressView(RunProgressViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Subscribe to property changes for screen reader announcements
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Clean up subscription when unloaded
        Unloaded += OnUnloaded;
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

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
}
