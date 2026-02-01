using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Code-behind for DiagnosticsView.
/// </summary>
public partial class DiagnosticsView
{
    public DiagnosticsView(DiagnosticsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (s, e) => viewModel.LoadMachineInfo();
    }
}
