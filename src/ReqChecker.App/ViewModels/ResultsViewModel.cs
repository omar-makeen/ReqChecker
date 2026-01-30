using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Models;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for results view.
/// </summary>
public partial class ResultsViewModel : ObservableObject
{
    [ObservableProperty]
    private RunReport? _report;

    [ObservableProperty]
    private Services.NavigationService? _navigationService;

    [ObservableProperty]
    private Services.DialogService? _dialogService;

    /// <summary>
    /// Navigates back to the test list view.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToTestListAsync()
    {
        // TODO: Implement navigation
        await Task.CompletedTask;
    }

    /// <summary>
    /// Exports the test results.
    /// </summary>
    [RelayCommand]
    private async Task ExportResultsAsync()
    {
        if (DialogService == null || Report == null)
        {
            return;
        }

        // TODO: Implement export functionality
        await Task.CompletedTask;
    }
}
