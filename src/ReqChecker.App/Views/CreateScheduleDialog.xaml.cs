using Microsoft.Win32;
using ReqChecker.App.ViewModels;
using ReqChecker.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace ReqChecker.App.Views;

/// <summary>
/// Dialog for creating or editing a schedule.
/// </summary>
public partial class CreateScheduleDialog : Window
{
    private readonly CreateScheduleViewModel _viewModel;

    public CreateScheduleDialog(CreateScheduleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        BrowseButton.Click += BrowseButton_Click;
        CancelButton.Click += (_, _) => { DialogResult = false; Close(); };

        Loaded += OnLoaded;
    }

    /// <summary>Pre-populate fields when editing an existing schedule.</summary>
    public void LoadSchedule(Schedule schedule) => _viewModel.LoadSchedule(schedule);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Populate hour ComboBoxes (0-23)
        for (int h = 0; h <= 23; h++)
        {
            ScheduledHourBox.Items.Add(new ComboBoxItem { Content = h.ToString("D2"), Tag = h });
            TimeOfDayHourBox.Items.Add(new ComboBoxItem { Content = h.ToString("D2"), Tag = h });
        }

        // Populate minute ComboBoxes (0, 5, 10, ... 55)
        for (int m = 0; m <= 55; m += 5)
        {
            ScheduledMinuteBox.Items.Add(new ComboBoxItem { Content = m.ToString("D2"), Tag = m });
            TimeOfDayMinuteBox.Items.Add(new ComboBoxItem { Content = m.ToString("D2"), Tag = m });
        }

        // Sync initial selections
        SetComboBoxByTag(ScheduledHourBox, _viewModel.ScheduledHour);
        SetComboBoxByTag(ScheduledMinuteBox, NearestFiveMinutes(_viewModel.ScheduledMinute));
        SetComboBoxByTag(TimeOfDayHourBox, _viewModel.TimeOfDayHour);
        SetComboBoxByTag(TimeOfDayMinuteBox, NearestFiveMinutes(_viewModel.TimeOfDayMinute));

        // Wire selection changes
        ScheduledHourBox.SelectionChanged += (_, _) =>
        {
            if (ScheduledHourBox.SelectedItem is ComboBoxItem { Tag: int h }) _viewModel.ScheduledHour = h;
        };
        ScheduledMinuteBox.SelectionChanged += (_, _) =>
        {
            if (ScheduledMinuteBox.SelectedItem is ComboBoxItem { Tag: int m }) _viewModel.ScheduledMinute = m;
        };
        TimeOfDayHourBox.SelectionChanged += (_, _) =>
        {
            if (TimeOfDayHourBox.SelectedItem is ComboBoxItem { Tag: int h }) _viewModel.TimeOfDayHour = h;
        };
        TimeOfDayMinuteBox.SelectionChanged += (_, _) =>
        {
            if (TimeOfDayMinuteBox.SelectedItem is ComboBoxItem { Tag: int m }) _viewModel.TimeOfDayMinute = m;
        };

        // Watch for save completion
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsSaving) && !_viewModel.IsSaving && _viewModel.Saved)
            {
                DialogResult = true;
                Close();
            }
        };
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Test Profile",
            Filter = "Profile files (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ProfileFilePath = dialog.FileName;
            // Try to infer profile name from filename (without extension)
            _viewModel.ProfileName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
        }
    }

    private static void SetComboBoxByTag(ComboBox box, int tag)
    {
        foreach (ComboBoxItem item in box.Items)
        {
            if (item.Tag is int t && t == tag)
            {
                box.SelectedItem = item;
                return;
            }
        }
        if (box.Items.Count > 0)
            box.SelectedIndex = 0;
    }

    private static int NearestFiveMinutes(int minutes)
        => (int)(Math.Round(minutes / 5.0) * 5) % 60;
}
