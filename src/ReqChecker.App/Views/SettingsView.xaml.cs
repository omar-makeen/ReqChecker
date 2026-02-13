using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReqChecker.App.Services;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Settings page view.
/// </summary>
public partial class SettingsView : Page
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Subscribe to theme changes to update border indicators
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Initial state
        UpdateThemeBorders(viewModel.CurrentTheme);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.CurrentTheme))
        {
            var viewModel = (SettingsViewModel)sender!;
            UpdateThemeBorders(viewModel.CurrentTheme);
        }
    }

    private void UpdateThemeBorders(AppTheme currentTheme)
    {
        // Update Dark theme border
        DarkThemeBorder.BorderBrush = currentTheme == AppTheme.Dark
            ? (Brush)Application.Current.Resources["AccentPrimary"]
            : Brushes.Transparent;
        
        // Update Light theme border
        LightThemeBorder.BorderBrush = currentTheme == AppTheme.Light
            ? (Brush)Application.Current.Resources["AccentPrimary"]
            : Brushes.Transparent;
    }
}
