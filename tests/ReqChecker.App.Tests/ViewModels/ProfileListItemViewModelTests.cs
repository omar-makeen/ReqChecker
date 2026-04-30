using ReqChecker.App.ViewModels;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using System.IO;
using Xunit;

namespace ReqChecker.App.Tests.ViewModels;

public class ProfileListItemViewModelTests
{
    private static Profile CreateProfile(
        string id = "test-id",
        string name = "Test Profile",
        int schemaVersion = 2,
        int testCount = 5,
        ProfileSource source = ProfileSource.Bundled)
    {
        var tests = new List<TestDefinition>();
        for (int i = 0; i < testCount; i++)
        {
            tests.Add(new TestDefinition { Id = $"test-{i}", DisplayName = $"Test {i}" });
        }

        return new Profile
        {
            Id = id,
            Name = name,
            SchemaVersion = schemaVersion,
            Source = source,
            Tests = tests
        };
    }

    [Fact]
    public void Constructor_ComputesIsRecommended_WhenIdMatchesDefault()
    {
        var profile = CreateProfile(id: ProfileSelectorViewModel.DefaultProfileId);
        var vm = new ProfileListItemViewModel(profile, null, isRecommended: true);

        Assert.True(vm.IsRecommended);
    }

    [Fact]
    public void Constructor_DefaultsIsActive_ToFalse()
    {
        var profile = CreateProfile();
        var vm = new ProfileListItemViewModel(profile, null, isRecommended: false);

        Assert.False(vm.IsActive);
    }

    [Fact]
    public void ModifiedLabel_IsNull_WhenSourcePathIsNull()
    {
        var profile = CreateProfile(source: ProfileSource.Bundled);
        var vm = new ProfileListItemViewModel(profile, null, isRecommended: false);

        Assert.Null(vm.ModifiedLabel);
        Assert.Null(vm.LastModifiedUtc);
    }

    [Fact]
    public void ModifiedLabel_IsPopulated_FromFileLastWriteTime()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "{}");
            File.SetLastWriteTimeUtc(tempFile, DateTime.UtcNow.AddDays(-3));

            var profile = CreateProfile(source: ProfileSource.UserProvided);
            var vm = new ProfileListItemViewModel(profile, tempFile, isRecommended: false);

            Assert.NotNull(vm.ModifiedLabel);
            Assert.Equal("modified 3 days ago", vm.ModifiedLabel);
            Assert.NotNull(vm.LastModifiedUtc);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Theory]
    [InlineData(0, "today")]
    [InlineData(1, "yesterday")]
    public void ModifiedLabel_FormatsRecentDate(int daysAgo, string expectedSuffix)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "{}");
            File.SetLastWriteTimeUtc(tempFile, DateTime.UtcNow.AddDays(-daysAgo));

            var profile = CreateProfile(source: ProfileSource.UserProvided);
            var vm = new ProfileListItemViewModel(profile, tempFile, isRecommended: false);

            Assert.Equal($"modified {expectedSuffix}", vm.ModifiedLabel);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ModifiedLabel_FormatsOldDate_WithFullDateFormat()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "{}");
            File.SetLastWriteTimeUtc(tempFile, DateTime.UtcNow.AddDays(-35));

            var profile = CreateProfile(source: ProfileSource.UserProvided);
            var vm = new ProfileListItemViewModel(profile, tempFile, isRecommended: false);

            Assert.NotNull(vm.ModifiedLabel);
            Assert.StartsWith("modified ", vm.ModifiedLabel);
            var datePart = vm.ModifiedLabel["modified ".Length..];
            Assert.DoesNotContain("ago", vm.ModifiedLabel);
            Assert.True(DateTime.TryParse(datePart, out _), $"Expected parseable date, got: {datePart}");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ModifiedLabel_FallsBackToFullDate_WhenFileMtimeIsInTheFuture()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "{}");
            File.SetLastWriteTimeUtc(tempFile, DateTime.UtcNow.AddDays(2));

            var profile = CreateProfile(source: ProfileSource.UserProvided);
            var vm = new ProfileListItemViewModel(profile, tempFile, isRecommended: false);

            Assert.NotNull(vm.ModifiedLabel);
            Assert.DoesNotContain("ago", vm.ModifiedLabel);
            Assert.DoesNotContain("today", vm.ModifiedLabel);
            Assert.DoesNotContain("yesterday", vm.ModifiedLabel);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void SchemaVersionLabel_IsNull_WhenSchemaVersionIsZero()
    {
        var profile = CreateProfile(schemaVersion: 0);
        var vm = new ProfileListItemViewModel(profile, null, isRecommended: false);

        Assert.Null(vm.SchemaVersionLabel);
    }

    [Theory]
    [InlineData(0, "0 tests")]
    [InlineData(1, "1 test")]
    [InlineData(2, "2 tests")]
    public void TestCountLabel_PluralizesCorrectly(int count, string expected)
    {
        var profile = CreateProfile(testCount: count);
        var vm = new ProfileListItemViewModel(profile, null, isRecommended: false);

        Assert.Equal(expected, vm.TestCountLabel);
    }

    [Fact]
    public void AccessibleName_IncludesRecommendedSuffix_WhenIsRecommended()
    {
        var profile = CreateProfile(name: "My Profile");
        var vm = new ProfileListItemViewModel(profile, null, isRecommended: true);

        Assert.Equal("My Profile (recommended)", vm.AccessibleName);
    }

    [Fact]
    public void IsActive_RaisesPropertyChanged_WhenSet()
    {
        var profile = CreateProfile();
        var vm = new ProfileListItemViewModel(profile, null, isRecommended: false);

        bool eventFired = false;
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.IsActive))
                eventFired = true;
        };

        vm.IsActive = true;

        Assert.True(eventFired);
        Assert.True(vm.IsActive);
    }
}
