using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.ProfileManagement.Migrations;

/// <summary>
/// Placeholder migrator for V1 to V2 schema migration.
/// Currently no schema changes exist, but this is prepared for future extensions.
/// </summary>
public class V1ToV2Migration : IProfileMigrator
{
    /// <inheritdoc/>
    public int TargetVersion => 2;

    /// <inheritdoc/>
    public bool NeedsMigration(ProfileModel profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        return profile.SchemaVersion < TargetVersion;
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

        // No schema changes for V1 to V2 migration currently
        // This is a placeholder for future schema evolution
        profile.SchemaVersion = TargetVersion;

        return await Task.FromResult(profile);
    }
}
