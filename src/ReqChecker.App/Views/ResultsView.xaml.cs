using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

public partial class ResultsView
{
    public ResultsView(ResultsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ExportPopup_Opened(object sender, System.EventArgs e)
    {
        PopupBorder.Opacity = 0;
        PopupTranslate.Y = 4;

        var storyboard = (Storyboard)FindResource("ExportPopupOpenStoryboard");
        storyboard.Begin(PopupBorder);

        if (ExportMenuItems.Children.Count > 0 && ExportMenuItems.Children[0] is Button first)
            first.Focus();
    }

    private void ExportPopup_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var items = ExportMenuItems.Children.OfType<Button>().Where(b => b.IsEnabled).ToArray();
        var focusedButton = Keyboard.FocusedElement as Button;
        var focused = focusedButton != null ? Array.IndexOf(items, focusedButton) : -1;

        switch (e.Key)
        {
            case Key.Down:
                if (focused >= 0 && focused < items.Length - 1)
                    items[focused + 1].Focus();
                e.Handled = true;
                break;
            case Key.Up:
                if (focused > 0)
                    items[focused - 1].Focus();
                e.Handled = true;
                break;
            case Key.Escape:
                if (DataContext is ResultsViewModel vm)
                    vm.IsExportMenuOpen = false;
                ExportDropdownButton.Focus();
                e.Handled = true;
                break;
        }
    }
}
