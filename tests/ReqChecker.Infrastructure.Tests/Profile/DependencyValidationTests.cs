using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.ProfileManagement;
using ProfileModel = ReqChecker.Core.Models.Profile;

namespace ReqChecker.Infrastructure.Tests.Profile;

/// <summary>
/// Unit tests for dependency validation in FluentProfileValidator.
/// </summary>
public class DependencyValidationTests
{
    private readonly IProfileValidator _validator;

    public DependencyValidationTests()
    {
        _validator = new FluentProfileValidator();
    }

    [Fact]
    public async Task ValidateAsync_MissingDependencyId_ReturnsError()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Ping",
                    DisplayName = "Test 1",
                    DependsOn = new List<string>()
                },
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Ping",
                    DisplayName = "Test 2",
                    DependsOn = new List<string> { "non-existent-id" }
                }
            },
            RunSettings = new RunSettings()
        };

        // Act
        var errors = await _validator.ValidateAsync(profile);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("dependency") || e.Contains("DependsOn"));
    }

    [Fact]
    public async Task ValidateAsync_CircularDependency_ReturnsError()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Ping",
                    DisplayName = "Test A",
                    DependsOn = new List<string> { "test-002" }
                },
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Ping",
                    DisplayName = "Test B",
                    DependsOn = new List<string> { "test-001" }
                }
            },
            RunSettings = new RunSettings()
        };

        // Act
        var errors = await _validator.ValidateAsync(profile);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("circular") || e.Contains("cycle"));
    }

    [Fact]
    public async Task ValidateAsync_ValidDependencies_NoErrors()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Ping",
                    DisplayName = "Test 1",
                    DependsOn = new List<string>()
                },
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Ping",
                    DisplayName = "Test 2",
                    DependsOn = new List<string> { "test-001" }
                },
                new TestDefinition
                {
                    Id = "test-003",
                    Type = "Ping",
                    DisplayName = "Test 3",
                    DependsOn = new List<string> { "test-001", "test-002" }
                }
            },
            RunSettings = new RunSettings()
        };

        // Act
        var errors = await _validator.ValidateAsync(profile);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_EmptyDependsOn_NoErrors()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Ping",
                    DisplayName = "Test 1",
                    DependsOn = new List<string>()
                },
                new TestDefinition
                {
                    Id = "test-002",
                    Type = "Ping",
                    DisplayName = "Test 2",
                    DependsOn = new List<string>()
                }
            },
            RunSettings = new RunSettings()
        };

        // Act
        var errors = await _validator.ValidateAsync(profile);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_SelfDependency_ReturnsError()
    {
        // Arrange
        var profile = new ProfileModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Profile",
            SchemaVersion = 3,
            Tests = new List<TestDefinition>
            {
                new TestDefinition
                {
                    Id = "test-001",
                    Type = "Ping",
                    DisplayName = "Test 1",
                    DependsOn = new List<string> { "test-001" }
                }
            },
            RunSettings = new RunSettings()
        };

        // Act
        var errors = await _validator.ValidateAsync(profile);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("circular") || e.Contains("cycle"));
    }
}
