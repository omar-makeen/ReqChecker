using ReqChecker.App.ViewModels;
using ReqChecker.Core.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ReqChecker.App.Views;

public partial class ProfileSelectorView
{
    public ProfileSelectorView(ProfileSelectorViewModel viewModel)
    {
        InitializeComponent();

        if (!SystemParameters.ClientAreaAnimation)
        {
            Resources["AnimatedProfileRow"] = Resources["ProfileRowStatic"];
        }

        DataContext = viewModel;
    }

    private void Row_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border || border.Tag is not Profile profile)
            return;

        if (DataContext is ProfileSelectorViewModel vm)
        {
            vm.SelectProfileCommand.Execute(profile);
        }
    }

    private void ProfileListBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter && e.Key != Key.Space)
            return;

        if (DataContext is ProfileSelectorViewModel vm && vm.SelectedItem?.Profile is Profile profile)
        {
            vm.SelectProfileCommand.Execute(profile);
            e.Handled = true;
        }
    }

    private void Kebab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.ContextMenu == null)
            return;

        btn.ContextMenu.PlacementTarget = btn;
        btn.ContextMenu.IsOpen = true;
        e.Handled = true;
    }

    private void OpenFileLocationMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem mi &&
            mi.DataContext is ProfileListItemViewModel item &&
            DataContext is ProfileSelectorViewModel vm)
        {
            vm.OpenFileLocationCommand.Execute(item);
        }
    }

    private void DeleteProfileMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem mi &&
            mi.DataContext is ProfileListItemViewModel item &&
            DataContext is ProfileSelectorViewModel vm)
        {
            vm.DeleteProfileCommand.Execute(item);
        }
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
