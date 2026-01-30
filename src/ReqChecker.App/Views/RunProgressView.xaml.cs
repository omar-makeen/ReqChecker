using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Code-behind for RunProgressView.
/// </summary>
public partial class RunProgressView
{
    public RunProgressView(RunProgressViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
