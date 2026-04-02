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
    private readonly ISchedulerService _schedulerService;

    public SchedulesView(SchedulesViewModel viewModel, ISchedulerService schedulerService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _schedulerService = schedulerService;
        DataContext = viewModel;

        _viewModel.CreateScheduleRequested += OnCreateScheduleRequested;
        _viewModel.EditScheduleRequested += OnEditScheduleRequested;

        Unloaded += OnPageUnloaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        _ = _viewModel.LoadSchedulesAsync();
    }

    private void OnCreateScheduleRequested(object? sender, EventArgs? e)
    {
        var vm = new CreateScheduleViewModel(_schedulerService);
        var dialog = new CreateScheduleDialog(vm);
        dialog.Owner = Window.GetWindow(this);
        if (dialog.ShowDialog() == true)
            _ = _viewModel.LoadSchedulesAsync();
    }

    private void OnEditScheduleRequested(object? sender, Schedule schedule)
    {
        var vm = new CreateScheduleViewModel(_schedulerService);
        var dialog = new CreateScheduleDialog(vm);
        dialog.Owner = Window.GetWindow(this);
        dialog.LoadSchedule(schedule);
        if (dialog.ShowDialog() == true)
            _ = _viewModel.LoadSchedulesAsync();
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.CreateScheduleRequested -= OnCreateScheduleRequested;
        _viewModel.EditScheduleRequested -= OnEditScheduleRequested;

        if (_viewModel is IDisposable disposable)
            disposable.Dispose();
    }
}
