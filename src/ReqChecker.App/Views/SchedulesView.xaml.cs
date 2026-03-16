using ReqChecker.App.ViewModels;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace ReqChecker.App.Views;

/// <summary>
/// Code-behind for the Schedules page.
/// </summary>
public partial class SchedulesView : Page
{
    private readonly SchedulesViewModel _viewModel;
    private readonly CreateScheduleViewModel _createVm;

    public SchedulesView(SchedulesViewModel viewModel, CreateScheduleViewModel createVm)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _createVm = createVm;
        DataContext = viewModel;

        _viewModel.CreateScheduleRequested += OnCreateScheduleRequested;
        _viewModel.EditScheduleRequested += OnEditScheduleRequested;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        _ = _viewModel.LoadSchedulesAsync();
    }

    private void OnCreateScheduleRequested(object? sender, EventArgs? e)
    {
        var dialog = new CreateScheduleDialog(_createVm);
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true)
            _ = _viewModel.LoadSchedulesAsync();
    }

    private void OnEditScheduleRequested(object? sender, Schedule schedule)
    {
        var dialog = new CreateScheduleDialog(_createVm);
        dialog.Owner = Window.GetWindow(this);
        dialog.LoadSchedule(schedule);
        if (dialog.ShowDialog() == true)
            _ = _viewModel.LoadSchedulesAsync();
    }
}
