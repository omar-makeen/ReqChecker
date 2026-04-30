using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.ProfileManagement;
using ReqChecker.App.Services;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Serilog;

namespace ReqChecker.App.ViewModels;

/// <summary>
/// View model for profile selection and management.
/// </summary>
public partial class ProfileSelectorViewModel : ObservableObject, IDisposable
{
    public const string DefaultProfileId = "00000001-0000-0000-0000-000000000001";

    private readonly IProfileLoader _profileLoader;
    private readonly IProfileValidator _profileValidator;
    private readonly ProfileMigrationPipeline _profileMigrator;
    private readonly IAppState _appState;
    private readonly DialogService _dialogService;
    private readonly NavigationService _navigationService;
    private readonly IProfileStorageService _profileStorageService;
    private readonly IPreferencesService _preferencesService;
    private bool _disposed;

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

    public ObservableCollection<ProfileListItemViewModel> Items { get; } = new();

    [ObservableProperty]
    private ProfileListItemViewModel? _selectedItem;

    public bool ShowWelcomeBanner => !_preferencesService.HasSeenOnboarding;

    public ProfileSelectorViewModel(
        IProfileLoader profileLoader,
        IProfileValidator profileValidator,
        ProfileMigrationPipeline profileMigrator,
        IAppState appState,
        DialogService dialogService,
        NavigationService navigationService,
        IProfileStorageService profileStorageService,
        IPreferencesService preferencesService)
    {
        _profileLoader = profileLoader;
        _profileValidator = profileValidator;
        _profileMigrator = profileMigrator;
        _appState = appState;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _profileStorageService = profileStorageService;
        _preferencesService = preferencesService;

        _preferencesService.PropertyChanged += OnPreferencesPropertyChanged;
        _appState.CurrentProfileChanged += OnCurrentProfileChanged;

        _ = LoadProfilesAsync();
    }

    private void OnPreferencesPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IPreferencesService.HasSeenOnboarding))
        {
            OnPropertyChanged(nameof(ShowWelcomeBanner));
        }
    }

    private void OnCurrentProfileChanged(object? sender, EventArgs e)
    {
        var currentProfile = _appState.CurrentProfile;
        foreach (var item in Items)
        {
            item.IsActive = currentProfile != null && item.Profile.Id == currentProfile.Id;
        }

        if (currentProfile != null)
        {
            SelectedItem = Items.FirstOrDefault(i => i.Profile.Id == currentProfile.Id);
        }
        else
        {
            SelectedItem = null;
        }
    }

    public bool IsRecommendedProfile(Profile profile)
    {
        return profile.Id == DefaultProfileId && profile.Source == ProfileSource.Bundled;
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
            var loadedItems = new List<(Profile profile, string? filePath)>();

            var bundledProfiles = await LoadBundledProfilesAsync();
            foreach (var profile in bundledProfiles)
            {
                loadedItems.Add((profile, null));
            }

            var userProfileData = await LoadUserProfilesWithPathsAsync();
            loadedItems.AddRange(userProfileData.Select(t => (t.profile, (string?)t.filePath)));

            Profiles.Clear();
            Items.Clear();
            foreach (var (profile, filePath) in loadedItems.OrderBy(x => x.profile.Name))
            {
                Profiles.Add(profile);

                Items.Add(new ProfileListItemViewModel(profile, filePath, IsRecommendedProfile(profile)));
            }

            var currentProfile = _appState.CurrentProfile;
            if (currentProfile != null)
            {
                var match = Items.FirstOrDefault(i => i.Profile.Id == currentProfile.Id);
                if (match != null)
                {
                    match.IsActive = true;
                    SelectedItem = match;
                }
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

    private async Task<List<(Profile profile, string filePath)>> LoadUserProfilesWithPathsAsync()
    {
        var results = new List<(Profile profile, string filePath)>();
        var filePaths = _profileStorageService.GetProfileFilePaths();

        foreach (var filePath in filePaths)
        {
            try
            {
                var profile = await _profileLoader.LoadFromFileAsync(filePath);
                profile.Source = ProfileSource.UserProvided;

                var validationErrors = await _profileValidator.ValidateAsync(profile);
                if (validationErrors.Any())
                {
                    continue;
                }

                if (_profileMigrator.NeedsMigration(profile))
                {
                    profile = await _profileMigrator.MigrateAsync(profile);
                }

                results.Add((profile, filePath));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load user profile from file: {FilePath}", filePath);
            }
        }

        return results;
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
            var destPath = _profileStorageService.CopyProfileToUserDirectory(filePath, overwrite: true);

            // Add to profiles list
            Profiles.Add(profile);

            // Add to bound items list
            Items.Add(new ProfileListItemViewModel(profile, destPath, IsRecommendedProfile(profile)));
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

    [RelayCommand]
    private void SelectProfile(Profile? profile)
    {
        if (profile == null)
        {
            return;
        }

        if (_appState.CurrentProfile != null && _appState.CurrentProfile.Id == profile.Id)
        {
            return;
        }

        DismissWelcomeBanner();
        SelectedProfile = profile;
        _appState.SetCurrentProfile(profile);
        _navigationService.NavigateToTestList();
    }

    /// <summary>
    /// Dismisses the welcome banner by marking onboarding as seen.
    /// </summary>
    [RelayCommand]
    private void DismissWelcomeBanner()
    {
        _preferencesService.HasSeenOnboarding = true;
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _preferencesService.PropertyChanged -= OnPreferencesPropertyChanged;
        _appState.CurrentProfileChanged -= OnCurrentProfileChanged;
        _disposed = true;
    }
}
