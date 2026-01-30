using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Code-behind for TestListView.
/// </summary>
public partial class TestListView
{
    public TestListView(TestListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
