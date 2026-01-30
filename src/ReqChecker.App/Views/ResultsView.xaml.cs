using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Code-behind for ResultsView.
/// </summary>
public partial class ResultsView
{
    public ResultsView(ResultsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
