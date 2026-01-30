using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// View for test configuration with field-level policy enforcement.
/// </summary>
public partial class TestConfigView
{
    public TestConfigView(TestConfigViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
