using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.ProfileManagement.Migrations;

/// <summary>
/// Migrates V2 profiles to V3 schema by adding DependsOn property to all test definitions.
/// </summary>
public class V2ToV3Migration : IProfileMigrator
{
    /// <inheritdoc/>
    public int TargetVersion => 3;

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

        // Ensure all test definitions have DependsOn property set to an empty list
        foreach (var test in profile.Tests)
        {
            if (test.DependsOn == null)
            {
                test.DependsOn = new List<string>();
            }
        }

        profile.SchemaVersion = TargetVersion;

        return await Task.FromResult(profile);
    }
}
