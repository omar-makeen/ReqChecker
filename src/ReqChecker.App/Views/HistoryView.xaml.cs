using System.Windows;
using System.Windows.Controls;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Code-behind for history view.
/// </summary>
public partial class HistoryView : Page
{
    private readonly HistoryViewModel _viewModel;

    public HistoryView(HistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Page loaded - initialization handled by ViewModel
    }
}
