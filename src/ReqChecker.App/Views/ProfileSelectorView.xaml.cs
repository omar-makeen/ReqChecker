using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

/// <summary>
/// Interaction logic for ProfileSelectorView.xaml.
/// </summary>
public partial class ProfileSelectorView
{
    public ProfileSelectorView(ProfileSelectorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
