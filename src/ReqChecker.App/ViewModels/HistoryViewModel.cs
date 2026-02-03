using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using ReqChecker.Infrastructure.History;
using ReqChecker.App.Services;
using Serilog;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for history view.
/// </summary>
public partial class HistoryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RunReport> _historyRuns = new();

    [ObservableProperty]
    private ICollectionView? _filteredHistory;

    [ObservableProperty]
    private string? _activeFilter = "All";

    [ObservableProperty]
    private RunReport? _selectedRun;

    [ObservableProperty]
    private NavigationService? _navigationService;

    [ObservableProperty]
    private IAppState? _appState;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isStatusError;

    /// <summary>
    /// Gets whether history is empty.
    /// </summary>
    public bool IsHistoryEmpty => HistoryRuns.Count == 0;

    /// <summary>
    /// Gets whether a run is selected.
    /// </summary>
    public bool HasSelectedRun => SelectedRun != null;

    /// <summary>
    /// Gets unique profile names from history.
    /// </summary>
    public ObservableCollection<string> ProfileNames { get; } = new();

    private readonly IHistoryService _historyService;

    public HistoryViewModel(
        IHistoryService historyService,
        NavigationService? navigationService = null,
        IAppState? appState = null)
    {
        _historyService = historyService;
        NavigationService = navigationService;
        AppState = appState;
    }

    /// <summary>
    /// Loads history from disk.
    /// </summary>
    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        IsLoading = true;
        StatusMessage = null;
        IsStatusError = false;

        try
        {
            var history = await _historyService.LoadHistoryAsync();
            
            HistoryRuns.Clear();
            foreach (var run in history)
            {
                HistoryRuns.Add(run);
            }

            // Extract unique profile names for filters
            var profiles = history
                .Select(r => r.ProfileName)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            ProfileNames.Clear();
            ProfileNames.Add("All");
            foreach (var profile in profiles)
            {
                ProfileNames.Add(profile);
            }

            SetupFilteredHistory();

            // Clear selection to prevent auto-navigation when returning to page
            SelectedRun = null;

            // Only show status message when there's history to display
            if (HistoryRuns.Count > 0)
            {
                StatusMessage = $"Loaded {HistoryRuns.Count} historical runs";
                IsStatusError = false;
            }

            Log.Information("Loaded {RunCount} historical runs", HistoryRuns.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to load history";
            IsStatusError = true;
            Log.Error(ex, "Failed to load history");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Called when ActiveFilter changes. Updates filtered history.
    /// </summary>
    partial void OnActiveFilterChanged(string? value)
    {
        FilteredHistory?.Refresh();
    }

    private void SetupFilteredHistory()
    {
        if (HistoryRuns.Count == 0)
        {
            FilteredHistory = null;
            return;
        }

        var view = CollectionViewSource.GetDefaultView(HistoryRuns);
        view.Filter = FilterHistoryRun;
        FilteredHistory = view;
    }

    private bool FilterHistoryRun(object obj)
    {
        if (obj is not RunReport run)
            return false;

        if (string.IsNullOrEmpty(ActiveFilter) || ActiveFilter == "All")
            return true;

        return run.ProfileName == ActiveFilter;
    }

    /// <summary>
    /// Sets active filter by profile name.
    /// </summary>
    [RelayCommand]
    private void SetFilter(string? profileName)
    {
        ActiveFilter = string.IsNullOrEmpty(profileName) ? "All" : profileName;
    }

    /// <summary>
    /// Navigates to test list view.
    /// </summary>
    [RelayCommand]
    private void NavigateToTestList()
    {
        NavigationService?.NavigateToTestList();
    }

    /// <summary>
    /// Views details of the selected run.
    /// </summary>
    [RelayCommand]
    private void ViewRunDetails()
    {
        if (SelectedRun != null && AppState != null)
        {
            AppState.SetLastRunReport(SelectedRun);
            NavigationService?.NavigateToResults();
        }
    }

    /// <summary>
    /// Called when SelectedRun property changes.
    /// </summary>
    partial void OnSelectedRunChanged(RunReport? value)
    {
        OnPropertyChanged(nameof(HasSelectedRun));
        if (value != null)
        {
            ViewRunDetails();
        }
    }

    /// <summary>
    /// Called when HistoryRuns property changes.
    /// </summary>
    partial void OnHistoryRunsChanged(ObservableCollection<RunReport> value)
    {
        OnPropertyChanged(nameof(IsHistoryEmpty));
    }

    /// <summary>
    /// Deletes a specific run from history.
    /// </summary>
    [RelayCommand]
    private async Task DeleteRunAsync(RunReport? run)
    {
        var targetRun = run ?? SelectedRun;
        if (targetRun == null)
        {
            return;
        }

        try
        {
            // Save run details before deletion
            var runId = targetRun.RunId;
            var profileName = targetRun.ProfileName;

            await _historyService.DeleteRunAsync(runId);

            // Remove from local collection
            HistoryRuns.Remove(targetRun);
            if (SelectedRun == targetRun)
            {
                SelectedRun = null;
            }

            StatusMessage = $"Deleted run from {profileName}";
            IsStatusError = false;

            Log.Information("Deleted run {RunId}", runId);
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to delete run";
            IsStatusError = true;
            Log.Error(ex, "Failed to delete run {RunId}", targetRun.RunId);
        }
    }

    /// <summary>
    /// Clears all history.
    /// </summary>
    [RelayCommand]
    private async Task ClearAllAsync()
    {
        try
        {
            await _historyService.ClearHistoryAsync();
            
            // Clear local collection
            HistoryRuns.Clear();
            SelectedRun = null;

            StatusMessage = "Cleared all history";
            IsStatusError = false;

            Log.Information("Cleared all history");
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to clear history";
            IsStatusError = true;
            Log.Error(ex, "Failed to clear history");
        }
    }
}
