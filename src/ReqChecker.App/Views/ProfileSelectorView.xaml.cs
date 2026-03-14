using ReqChecker.App.ViewModels;
using System.Windows;
using System.Windows.Media.Animation;

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

    private void DismissBanner_Click(object sender, RoutedEventArgs e)
    {
        if (WelcomeBanner.Visibility != Visibility.Visible)
        {
            return;
        }

        var storyboard = (Storyboard)Resources["BannerDismissStoryboard"];
        storyboard.Completed -= OnBannerDismissCompleted;
        storyboard.Completed += OnBannerDismissCompleted;
        storyboard.Begin(WelcomeBanner);
    }

    private void OnBannerDismissCompleted(object? sender, EventArgs e)
    {
        if (sender is Storyboard storyboard)
        {
            storyboard.Completed -= OnBannerDismissCompleted;
        }
        WelcomeBanner.Visibility = Visibility.Collapsed;
    }
}
