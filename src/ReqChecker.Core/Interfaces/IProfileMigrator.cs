using ReqChecker.Core.Models;

namespace ReqChecker.Core.Interfaces;

/// <summary>
/// Migrates profiles between schema versions.
/// </summary>
public interface IProfileMigrator
{
    /// <summary>
    /// Gets the supported schema version.
    /// </summary>
    int TargetVersion { get; }

    /// <summary>
    /// Checks if a profile needs migration.
    /// </summary>
    /// <param name="profile">The profile to check.</param>
    /// <returns>True if migration is needed.</returns>
    bool NeedsMigration(Profile profile);

    /// <summary>
    /// Migrates a profile to the target schema version.
    /// </summary>
    /// <param name="profile">The profile to migrate.</param>
    /// <returns>The migrated profile.</returns>
    Task<Profile> MigrateAsync(Profile profile);
}
