using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.ProfileManagement;

/// <summary>
/// Orchestrates profile migration through registered migrators.
/// </summary>
public class ProfileMigrationPipeline : IProfileMigrator
{
    private readonly IEnumerable<IProfileMigrator> _migrators;
    private const int CurrentSchemaVersion = 3;

    /// <summary>
    /// Initializes a new instance of the ProfileMigrationPipeline.
    /// </summary>
    /// <param name="migrators">Collection of registered migrators.</param>
    public ProfileMigrationPipeline(IEnumerable<IProfileMigrator> migrators)
    {
        _migrators = migrators ?? throw new ArgumentNullException(nameof(migrators));
    }

    /// <inheritdoc/>
    public int TargetVersion => CurrentSchemaVersion;

    /// <inheritdoc/>
    public bool NeedsMigration(ProfileModel profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        return profile.SchemaVersion < CurrentSchemaVersion;
    }

    /// <inheritdoc/>
    public async Task<ProfileModel> MigrateAsync(ProfileModel profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        if (!NeedsMigration(profile))
        {
            return profile;
        }

        var currentProfile = profile;

        // Apply migrations in sequence from current version to target
        while (currentProfile.SchemaVersion < CurrentSchemaVersion)
        {
            var nextVersion = currentProfile.SchemaVersion + 1;
            var migrator = _migrators.FirstOrDefault(m => m.TargetVersion == nextVersion);

            if (migrator == null)
            {
                throw new InvalidOperationException(
                    $"No migrator found for schema version {nextVersion}. " +
                    $"Cannot migrate profile '{profile.Name}' from version {profile.SchemaVersion}.");
            }

            currentProfile = await migrator.MigrateAsync(currentProfile);
        }

        return currentProfile;
    }
}
