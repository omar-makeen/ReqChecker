using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using ReqChecker.Infrastructure.ProfileManagement;
using Serilog;

namespace ReqChecker.Infrastructure.Profile;

/// <summary>
/// Service for detecting and loading startup profiles from the application directory.
/// </summary>
public class StartupProfileService : IStartupProfileService
{
    private readonly IProfileLoader _profileLoader;
    private readonly IProfileValidator _profileValidator;
    private readonly ProfileMigrationPipeline _migrationPipeline;

    private const string StartupProfileFileName = "startup-profile.json";

    /// <summary>
    /// Initializes a new instance of the StartupProfileService.
    /// </summary>
    /// <param name="profileLoader">Service for loading profile JSON.</param>
    /// <param name="profileValidator">Service for validating profile structure.</param>
    /// <param name="migrationPipeline">Service for migrating profiles to current schema version.</param>
    public StartupProfileService(
        IProfileLoader profileLoader,
        IProfileValidator profileValidator,
        ProfileMigrationPipeline migrationPipeline)
    {
        _profileLoader = profileLoader ?? throw new ArgumentNullException(nameof(profileLoader));
        _profileValidator = profileValidator ?? throw new ArgumentNullException(nameof(profileValidator));
        _migrationPipeline = migrationPipeline ?? throw new ArgumentNullException(nameof(migrationPipeline));
    }

    /// <inheritdoc/>
    public string GetStartupProfilePath()
    {
        return Path.Combine(AppContext.BaseDirectory, StartupProfileFileName);
    }

    /// <inheritdoc/>
    public async Task<StartupProfileResult> TryLoadStartupProfileAsync()
    {
        var startupProfilePath = GetStartupProfilePath();

        // Check if file exists
        if (!File.Exists(startupProfilePath))
        {
            Log.Information("Startup profile not found at {Path}", startupProfilePath);
            return StartupProfileResult.NotFound();
        }

        // Check if file is empty (0 bytes)
        var fileInfo = new FileInfo(startupProfilePath);
        if (fileInfo.Length == 0)
        {
            Log.Information("Startup profile file is empty at {Path}", startupProfilePath);
            return StartupProfileResult.NotFound();
        }

        try
        {
            // Load the profile
            Log.Information("Loading startup profile from {Path}", startupProfilePath);
            var profile = await _profileLoader.LoadFromFileAsync(startupProfilePath);

            // Check if profile has tests (empty tests array = treat as not found)
            if (profile.Tests == null || profile.Tests.Count == 0)
            {
                Log.Information("Startup profile has no tests at {Path}", startupProfilePath);
                return StartupProfileResult.NotFound();
            }

            // Validate the profile
            var validationErrors = await _profileValidator.ValidateAsync(profile);
            if (validationErrors.Any())
            {
                var errorMessage = "Startup profile validation failed: " +
                    string.Join("; ", validationErrors.Take(3));
                Log.Warning("Startup profile validation failed: {Errors}", string.Join("; ", validationErrors));
                return StartupProfileResult.Failed(errorMessage);
            }

            // Migrate if needed
            if (_migrationPipeline.NeedsMigration(profile))
            {
                Log.Information("Migrating startup profile from version {Version} to {TargetVersion}",
                    profile.SchemaVersion, _migrationPipeline.TargetVersion);
                profile = await _migrationPipeline.MigrateAsync(profile);
            }

            // Set source to UserProvided since it's an external file
            profile.Source = ProfileSource.UserProvided;

            Log.Information("Successfully loaded startup profile: {ProfileName} ({TestCount} tests)",
                profile.Name, profile.Tests.Count);

            return StartupProfileResult.Loaded(profile);
        }
        catch (FileNotFoundException)
        {
            Log.Information("Startup profile not found at {Path}", startupProfilePath);
            return StartupProfileResult.NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            var errorMessage = "Cannot read startup configuration. Access denied.";
            Log.Warning(ex, "Cannot read startup profile at {Path}", startupProfilePath);
            return StartupProfileResult.Failed(errorMessage);
        }
        catch (IOException ex)
        {
            var errorMessage = "Cannot read startup configuration. File may be in use.";
            Log.Warning(ex, "IO error reading startup profile at {Path}", startupProfilePath);
            return StartupProfileResult.Failed(errorMessage);
        }
        catch (System.Text.Json.JsonException ex)
        {
            var errorMessage = "Startup configuration could not be read. Invalid format.";
            Log.Warning(ex, "Invalid JSON in startup profile at {Path}", startupProfilePath);
            return StartupProfileResult.Failed(errorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = "Startup configuration could not be loaded.";
            Log.Error(ex, "Unexpected error loading startup profile at {Path}", startupProfilePath);
            return StartupProfileResult.Failed(errorMessage);
        }
    }
}
