using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.ProfileManagement;
using ReqChecker.App.Services;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Linq;
using Serilog;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for profile selection and management.
/// </summary>
public partial class ProfileSelectorViewModel : ObservableObject
{
    private readonly IProfileLoader _profileLoader;
    private readonly IProfileValidator _profileValidator;
    private readonly ProfileMigrationPipeline _profileMigrator;
    private readonly IAppState _appState;
    private readonly DialogService _dialogService;
    private readonly NavigationService _navigationService;
    private readonly IProfileStorageService _profileStorageService;

    [ObservableProperty]
    private ObservableCollection<Profile> _profiles = new();

    [ObservableProperty]
    private Profile? _selectedProfile;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    public ProfileSelectorViewModel(
        IProfileLoader profileLoader,
        IProfileValidator profileValidator,
        ProfileMigrationPipeline profileMigrator,
        IAppState appState,
        DialogService dialogService,
        NavigationService navigationService,
        IProfileStorageService profileStorageService)
    {
        _profileLoader = profileLoader;
        _profileValidator = profileValidator;
        _profileMigrator = profileMigrator;
        _appState = appState;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _profileStorageService = profileStorageService;

        // Load profiles on initialization
        _ = LoadProfilesAsync();
    }

    /// <summary>
    /// Loads all bundled and user profiles atomically.
    /// </summary>
    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var loadedProfiles = new List<Profile>();

            // Load bundled profiles from embedded resources
            var bundledProfiles = await LoadBundledProfilesAsync();
            loadedProfiles.AddRange(bundledProfiles);

            // Load user profiles from AppData
            var userProfiles = await LoadUserProfilesAsync();
            loadedProfiles.AddRange(userProfiles);

            Profiles.Clear();
            foreach (var profile in loadedProfiles.OrderBy(p => p.Name))
            {
                Profiles.Add(profile);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load profiles: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads bundled profiles from embedded resources.
    /// </summary>
    private async Task<List<Profile>> LoadBundledProfilesAsync()
    {
        var profiles = new List<Profile>();
        var assembly = Assembly.GetExecutingAssembly();

        // Find all embedded JSON profile resources
        var profileResources = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".json") && name.Contains("Profiles"));

        foreach (var resourceName in profileResources)
        {
            try
            {
                await using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                var profile = await _profileLoader.LoadFromStreamAsync(stream);
                profile.Source = ProfileSource.Bundled;

                // Validate profile
                var validationErrors = await _profileValidator.ValidateAsync(profile);
                if (validationErrors.Any())
                {
                    continue; // Skip invalid bundled profiles
                }

                // Migrate if needed
                if (_profileMigrator.NeedsMigration(profile))
                {
                    profile = await _profileMigrator.MigrateAsync(profile);
                }

                profiles.Add(profile);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load bundled profile from resource: {ResourceName}", resourceName);
            }
        }

        return profiles;
    }

    /// <summary>
    /// Loads user profiles from AppData directory.
    /// </summary>
    private async Task<List<Profile>> LoadUserProfilesAsync()
    {
        var profiles = new List<Profile>();
        var filePaths = _profileStorageService.GetProfileFilePaths();

        foreach (var filePath in filePaths)
        {
            try
            {
                var profile = await _profileLoader.LoadFromFileAsync(filePath);
                profile.Source = ProfileSource.UserProvided;

                // Validate profile
                var validationErrors = await _profileValidator.ValidateAsync(profile);
                if (validationErrors.Any())
                {
                    continue; // Skip invalid user profiles
                }

                // Migrate if needed
                if (_profileMigrator.NeedsMigration(profile))
                {
                    profile = await _profileMigrator.MigrateAsync(profile);
                }

                profiles.Add(profile);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load user profile from file: {FilePath}", filePath);
            }
        }

        return profiles;
    }

    /// <summary>
    /// Imports a user profile from a file.
    /// </summary>
    [RelayCommand]
    private async Task ImportProfileAsync()
    {
        HasError = false;
        ErrorMessage = null;

        var filePath = _dialogService.OpenProfileFileDialog();
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        IsLoading = true;

        try
        {
            // Load profile
            var profile = await _profileLoader.LoadFromFileAsync(filePath);
            profile.Source = ProfileSource.UserProvided;

            // Validate profile
            var validationErrors = await _profileValidator.ValidateAsync(profile);
            if (validationErrors.Any())
            {
                HasError = true;
                ErrorMessage = $"Profile validation failed:\n{string.Join("\n", validationErrors)}";
                return;
            }

            // Migrate if needed
            if (_profileMigrator.NeedsMigration(profile))
            {
                profile = await _profileMigrator.MigrateAsync(profile);
            }

            // Copy to user profiles directory
            _profileStorageService.CopyProfileToUserDirectory(filePath, overwrite: true);

            // Add to profiles list
            Profiles.Add(profile);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to import profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Navigates to test list with selected profile.
    /// </summary>
    [RelayCommand]
    private void SelectProfile(Profile? profile)
    {
        if (profile == null)
        {
            return;
        }

        SelectedProfile = profile;
        _appState.SetCurrentProfile(profile);
        _navigationService.NavigateToTestList();
    }

    /// <summary>
    /// Clears the error message.
    /// </summary>
    [RelayCommand]
    private void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }
}
