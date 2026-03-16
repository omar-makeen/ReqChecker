using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using System.Windows;

namespace ReqChecker.App.Views;

/// <summary>
/// Displays schedules that were missed while the application was closed.
/// Offers "Run All Now" or "Dismiss" options.
/// </summary>
public partial class MissedRunsDialog : Window
{
    private readonly ISchedulerService _schedulerService;
    private readonly List<Schedule> _missedSchedules;

    public MissedRunsDialog(ISchedulerService schedulerService, List<Schedule> missedSchedules)
    {
        InitializeComponent();
        _schedulerService = schedulerService;
        _missedSchedules = missedSchedules;

        MissedRunsList.ItemsSource = missedSchedules;

        RunNowButton.Click += RunNowButton_Click;
        DismissButton.Click += DismissButton_Click;
    }

    private async void RunNowButton_Click(object sender, RoutedEventArgs e)
    {
        RunNowButton.IsEnabled = false;
        DismissButton.IsEnabled = false;

        // Run each missed schedule sequentially
        foreach (var schedule in _missedSchedules)
        {
            try
            {
                await _schedulerService.RunNowAsync(schedule.Id);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to run missed schedule '{Name}'", schedule.Name);
            }
        }

        DialogResult = true;
        Close();
    }

    private async void DismissButton_Click(object sender, RoutedEventArgs e)
    {
        // Re-activate missed schedules so they get rescheduled
        foreach (var schedule in _missedSchedules)
        {
            try
            {
                await _schedulerService.ResumeScheduleAsync(schedule.Id);
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex, "Failed to resume missed schedule '{Name}'", schedule.Name);
            }
        }

        DialogResult = false;
        Close();
    }
}
