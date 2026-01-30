using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.ProfileManagement;
using ReqChecker.Infrastructure.ProfileManagement.Migrations;
using System.Text.Json.Nodes;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.Tests.Profile;

/// <summary>
/// Integration tests for profile migration from V1 to V2.
/// </summary>
public class ProfileMigrationTests
{
    [Fact]
    public void ProfileMigrationPipeline_TargetVersion_ShouldBe2()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>
        {
            new V1ToV2Migration()
        };
        var pipeline = new ProfileMigrationPipeline(migrators);

        // Act & Assert
        Assert.Equal(2, pipeline.TargetVersion);
    }

    [Fact]
    public void NeedsMigration_ShouldReturnTrueForV1Profile()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>
        {
            new V1ToV2Migration()
        };
        var pipeline = new ProfileMigrationPipeline(migrators);
        var v1Profile = new ProfileModel
        {
            Name = "V1 Profile",
            SchemaVersion = 1,
            Tests = new List<TestDefinition>()
        };

        // Act
        var needsMigration = pipeline.NeedsMigration(v1Profile);

        // Assert
        Assert.True(needsMigration);
    }

    [Fact]
    public void NeedsMigration_ShouldReturnFalseForV2Profile()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>
        {
            new V1ToV2Migration()
        };
        var pipeline = new ProfileMigrationPipeline(migrators);
        var v2Profile = new ProfileModel
        {
            Name = "V2 Profile",
            SchemaVersion = 2,
            Tests = new List<TestDefinition>()
        };

        // Act
        var needsMigration = pipeline.NeedsMigration(v2Profile);

        // Assert
        Assert.False(needsMigration);
    }

    [Fact]
    public void NeedsMigration_ShouldThrowWhenProfileIsNull()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>
        {
            new V1ToV2Migration()
        };
        var pipeline = new ProfileMigrationPipeline(migrators);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => pipeline.NeedsMigration(null!));
    }

    [Fact]
    public async Task MigrateAsync_ShouldUpdateSchemaVersionFromV1ToV2()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>
        {
            new V1ToV2Migration()
        };
        var pipeline = new ProfileMigrationPipeline(migrators);
        var v1Profile = new ProfileModel
        {
            Name = "V1 Profile",
            SchemaVersion = 1,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-1",
                    DisplayName = "Test 1",
                    Type = "Ping",
                    Parameters = new JsonObject
                    {
                        ["target"] = "localhost"
                    }
                }
            }
        };

        // Act
        var migratedProfile = await pipeline.MigrateAsync(v1Profile);

        // Assert
        Assert.Equal(2, migratedProfile.SchemaVersion);
        Assert.Equal("V1 Profile", migratedProfile.Name);
        Assert.Single(migratedProfile.Tests);
    }

    [Fact]
    public async Task MigrateAsync_ShouldReturnSameProfileWhenNoMigrationNeeded()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>
        {
            new V1ToV2Migration()
        };
        var pipeline = new ProfileMigrationPipeline(migrators);
        var v2Profile = new ProfileModel
        {
            Name = "V2 Profile",
            SchemaVersion = 2,
            Tests = new List<TestDefinition>()
        };

        // Act
        var migratedProfile = await pipeline.MigrateAsync(v2Profile);

        // Assert
        Assert.Same(v2Profile, migratedProfile);
        Assert.Equal(2, migratedProfile.SchemaVersion);
    }

    [Fact]
    public async Task MigrateAsync_ShouldThrowWhenProfileIsNull()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>
        {
            new V1ToV2Migration()
        };
        var pipeline = new ProfileMigrationPipeline(migrators);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await pipeline.MigrateAsync(null!));
    }

    [Fact]
    public async Task MigrateAsync_ShouldThrowWhenMigratorNotFound()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>(); // No migrators registered
        var pipeline = new ProfileMigrationPipeline(migrators);
        var v1Profile = new ProfileModel
        {
            Name = "V1 Profile",
            SchemaVersion = 1,
            Tests = new List<TestDefinition>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await pipeline.MigrateAsync(v1Profile));
        Assert.Contains("No migrator found for schema version 2", exception.Message);
    }

    [Fact]
    public void V1ToV2Migration_TargetVersion_ShouldBe2()
    {
        // Arrange
        var migrator = new V1ToV2Migration();

        // Act & Assert
        Assert.Equal(2, migrator.TargetVersion);
    }

    [Fact]
    public void V1ToV2Migration_NeedsMigration_ShouldReturnTrueForV1Profile()
    {
        // Arrange
        var migrator = new V1ToV2Migration();
        var v1Profile = new ProfileModel
        {
            Name = "V1 Profile",
            SchemaVersion = 1,
            Tests = new List<TestDefinition>()
        };

        // Act
        var needsMigration = migrator.NeedsMigration(v1Profile);

        // Assert
        Assert.True(needsMigration);
    }

    [Fact]
    public void V1ToV2Migration_NeedsMigration_ShouldReturnFalseForV2Profile()
    {
        // Arrange
        var migrator = new V1ToV2Migration();
        var v2Profile = new ProfileModel
        {
            Name = "V2 Profile",
            SchemaVersion = 2,
            Tests = new List<TestDefinition>()
        };

        // Act
        var needsMigration = migrator.NeedsMigration(v2Profile);

        // Assert
        Assert.False(needsMigration);
    }

    [Fact]
    public async Task V1ToV2Migration_MigrateAsync_ShouldUpdateSchemaVersion()
    {
        // Arrange
        var migrator = new V1ToV2Migration();
        var v1Profile = new ProfileModel
        {
            Name = "V1 Profile",
            SchemaVersion = 1,
            Tests = new List<TestDefinition>()
        };

        // Act
        var migratedProfile = await migrator.MigrateAsync(v1Profile);

        // Assert
        Assert.Equal(2, migratedProfile.SchemaVersion);
        Assert.Equal("V1 Profile", migratedProfile.Name);
    }

    [Fact]
    public async Task V1ToV2Migration_MigrateAsync_ShouldReturnSameProfileWhenNoMigrationNeeded()
    {
        // Arrange
        var migrator = new V1ToV2Migration();
        var v2Profile = new ProfileModel
        {
            Name = "V2 Profile",
            SchemaVersion = 2,
            Tests = new List<TestDefinition>()
        };

        // Act
        var migratedProfile = await migrator.MigrateAsync(v2Profile);

        // Assert
        Assert.Same(v2Profile, migratedProfile);
    }

    [Fact]
    public void V1ToV2Migration_NeedsMigration_ShouldThrowWhenProfileIsNull()
    {
        // Arrange
        var migrator = new V1ToV2Migration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => migrator.NeedsMigration(null!));
    }

    [Fact]
    public async Task V1ToV2Migration_MigrateAsync_ShouldThrowWhenProfileIsNull()
    {
        // Arrange
        var migrator = new V1ToV2Migration();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await migrator.MigrateAsync(null!));
    }

    [Fact]
    public async Task MigrateAsync_ShouldPreserveProfileProperties()
    {
        // Arrange
        var migrators = new List<IProfileMigrator>
        {
            new V1ToV2Migration()
        };
        var pipeline = new ProfileMigrationPipeline(migrators);
        var v1Profile = new ProfileModel
        {
            Name = "Test Profile",
            SchemaVersion = 1,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-1",
                    DisplayName = "Test 1",
                    Type = "Ping",
                    Timeout = 30,
                    RetryCount = 3,
                    Parameters = new JsonObject
                    {
                        ["target"] = "example.com",
                        ["count"] = 4
                    }
                },
                new TestDefinition
                {
                    Id = "test-2",
                    DisplayName = "Test 2",
                    Type = "HttpGet",
                    Timeout = 60,
                    RetryCount = 0,
                    Parameters = new JsonObject
                    {
                        ["url"] = "https://example.com/api"
                    }
                }
            }
        };

        // Act
        var migratedProfile = await pipeline.MigrateAsync(v1Profile);

        // Assert
        Assert.Equal(2, migratedProfile.SchemaVersion);
        Assert.Equal("Test Profile", migratedProfile.Name);
        Assert.Equal(2, migratedProfile.Tests.Count);

        // Verify test properties are preserved
        var test1 = migratedProfile.Tests[0];
        Assert.Equal("Test 1", test1.DisplayName);
        Assert.Equal("Ping", test1.Type);
        Assert.Equal(30, test1.Timeout);
        Assert.Equal(3, test1.RetryCount);
        Assert.Equal("example.com", test1.Parameters["target"]?.ToString());

        var test2 = migratedProfile.Tests[1];
        Assert.Equal("Test 2", test2.DisplayName);
        Assert.Equal("HttpGet", test2.Type);
        Assert.Equal(60, test2.Timeout);
        Assert.Equal(0, test2.RetryCount);
        Assert.Equal("https://example.com/api", test2.Parameters["url"]?.ToString());
    }
}
