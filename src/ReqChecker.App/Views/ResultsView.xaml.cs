using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ReqChecker.App.ViewModels;

namespace ReqChecker.App.Views;

public partial class ResultsView
{
    private Storyboard? _filterFadeOutStoryboard;
    private Storyboard? _filterFadeInStoryboard;
    private bool _isFilterTransitioning;
    private bool _storyboardsInitialized;

    public ResultsView(ResultsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_storyboardsInitialized)
            return;

        _filterFadeOutStoryboard = (Storyboard)FindResource("FilterFadeOutStoryboard");
        _filterFadeInStoryboard = (Storyboard)FindResource("FilterFadeInStoryboard");

        _filterFadeOutStoryboard.Completed += FilterFadeOutStoryboard_Completed;
        _filterFadeInStoryboard.Completed += FilterFadeInStoryboard_Completed;
        _storyboardsInitialized = true;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ResultsViewModel.ActiveFilter))
        {
            BeginFilterTransition();
        }
    }

    private void BeginFilterTransition()
    {
        if (_isFilterTransitioning)
        {
            _filterFadeOutStoryboard?.Stop(FilterTransitionContainer);
            _filterFadeInStoryboard?.Stop(FilterTransitionContainer);
            FilterTransitionContainer.Opacity = 1;
        }

        if (FilterTransitionContainer.Opacity > 0 && _filterFadeOutStoryboard != null)
        {
            _isFilterTransitioning = true;
            _filterFadeOutStoryboard.Begin(FilterTransitionContainer, true);
        }
    }

    private void FilterFadeOutStoryboard_Completed(object? sender, System.EventArgs e)
    {
        if (DataContext is ResultsViewModel vm)
        {
            vm.FilteredResults?.Refresh();
        }

        if (_filterFadeInStoryboard != null)
        {
            _filterFadeInStoryboard.Begin(FilterTransitionContainer, true);
        }
    }

    private void FilterFadeInStoryboard_Completed(object? sender, System.EventArgs e)
    {
        _isFilterTransitioning = false;
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
