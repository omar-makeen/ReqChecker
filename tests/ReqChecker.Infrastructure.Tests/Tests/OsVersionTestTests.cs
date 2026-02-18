using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Tests;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ReqChecker.Infrastructure.Tests.Tests;

/// <summary>
/// Unit tests for OsVersionTest — covers parameter validation, comparison modes,
/// evidence capture, and cancellation without requiring any network or I/O.
/// </summary>
public class OsVersionTestTests
{
    private readonly OsVersionTest _test = new();

    #region Parameter Validation Tests

    [Fact]
    public async Task ExecuteAsync_NullParameters_ReturnsPass_InformationalMode()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["minimumBuild"] = null,
            ["expectedVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Null(result.Error);
        Assert.False(string.IsNullOrWhiteSpace(result.HumanSummary));
    }

    [Fact]
    public async Task ExecuteAsync_EmptyParameters_ReturnsPass_InformationalMode()
    {
        var definition = MakeDefinition(new JsonObject());

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_BothParamsSet_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["minimumBuild"] = 19045,
            ["expectedVersion"] = "10.0.19045"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("both", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidExpectedVersionFormat_TwoSegments_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["expectedVersion"] = "10.0"  // only two segments — invalid
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("10.0", result.Error!.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidExpectedVersionFormat_AlphaString_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["expectedVersion"] = "abc"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_NegativeMinimumBuild_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["minimumBuild"] = -1
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_ZeroMinimumBuild_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["minimumBuild"] = 0
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task ExecuteAsync_CancelledToken_ReturnsSkipped()
    {
        var definition = MakeDefinition(new JsonObject());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _test.ExecuteAsync(definition, null, cts.Token);

        Assert.Equal(TestStatus.Skipped, result.Status);
        Assert.Equal(ErrorCategory.Unknown, result.Error?.Category);
    }

    #endregion

    #region Result Property Tests

    [Fact]
    public async Task ExecuteAsync_ResultProperties_ArePopulated()
    {
        var definition = MakeDefinition(new JsonObject());

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal("test-os-001", result.TestId);
        Assert.Equal("OsVersion", result.TestType);
        Assert.Equal("OS Version Test", result.DisplayName);
        Assert.NotEqual(default, result.StartTime);
        Assert.NotEqual(default, result.EndTime);
        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_HumanSummary_IsAlwaysPopulated()
    {
        // Informational mode
        var def1 = MakeDefinition(new JsonObject());
        var r1 = await _test.ExecuteAsync(def1, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r1.HumanSummary));

        // Configuration error
        var def2 = MakeDefinition(new JsonObject { ["minimumBuild"] = -1 });
        var r2 = await _test.ExecuteAsync(def2, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r2.HumanSummary));

        // Minimum build pass
        var def3 = MakeDefinition(new JsonObject { ["minimumBuild"] = 1 });
        var r3 = await _test.ExecuteAsync(def3, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r3.HumanSummary));
    }

    #endregion

    #region Minimum Build Tests (User Story 1)

    [Fact]
    public async Task ExecuteAsync_MinimumBuild_VeryLow_ReturnsPass()
    {
        // Build 1 — any real Windows machine exceeds this
        var definition = MakeDefinition(new JsonObject
        {
            ["minimumBuild"] = 1
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Null(result.Error);
        Assert.Contains("meets", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_MinimumBuild_VeryHigh_ReturnsFail()
    {
        // Build 999999 — no current Windows release has this
        var definition = MakeDefinition(new JsonObject
        {
            ["minimumBuild"] = 999999
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Contains("does not meet", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_MinimumBuild_HumanSummary_ContainsBuildNumbers()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["minimumBuild"] = 1
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        var detectedBuild = Environment.OSVersion.Version.Build;
        Assert.Contains(detectedBuild.ToString(), result.HumanSummary);
        Assert.Contains("1", result.HumanSummary);
    }

    [Fact]
    public async Task ExecuteAsync_MinimumBuild_Evidence_IsPopulated()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["minimumBuild"] = 1
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotNull(result.Evidence);
        Assert.False(string.IsNullOrWhiteSpace(result.Evidence.ResponseData));
        var doc = JsonDocument.Parse(result.Evidence.ResponseData!);
        Assert.True(doc.RootElement.TryGetProperty("buildNumber", out _));
    }

    #endregion

    #region Exact Version Match Tests (User Story 2)

    [Fact]
    public async Task ExecuteAsync_ExactVersion_CurrentMachine_ReturnsPass()
    {
        var osVersion = Environment.OSVersion.Version;
        var currentVersion = $"{osVersion.Major}.{osVersion.Minor}.{osVersion.Build}";
        var definition = MakeDefinition(new JsonObject
        {
            ["expectedVersion"] = currentVersion
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Null(result.Error);
        Assert.Contains("matches", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ExactVersion_DifferentVersion_ReturnsFail()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["expectedVersion"] = "0.0.1"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Contains("does not match", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ExactVersion_HumanSummary_ContainsVersionStrings()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["expectedVersion"] = "0.0.1"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Contains("0.0.1", result.HumanSummary);
        var detectedVersion = $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}.{Environment.OSVersion.Version.Build}";
        Assert.Contains(detectedVersion, result.HumanSummary);
    }

    #endregion

    #region Evidence Tests (User Story 3)

    [Fact]
    public async Task ExecuteAsync_Evidence_ContainsAllRequiredFields()
    {
        var definition = MakeDefinition(new JsonObject());

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotNull(result.Evidence);
        Assert.False(string.IsNullOrWhiteSpace(result.Evidence.ResponseData));

        var doc = JsonDocument.Parse(result.Evidence.ResponseData!);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("productName", out var productNameProp), "Missing 'productName'");
        Assert.True(root.TryGetProperty("version", out _), "Missing 'version'");
        Assert.True(root.TryGetProperty("buildNumber", out _), "Missing 'buildNumber'");
        Assert.True(root.TryGetProperty("architecture", out _), "Missing 'architecture'");

        Assert.False(string.IsNullOrWhiteSpace(productNameProp.GetString()), "productName must be non-empty");
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_Architecture_MatchesRuntimeInfo()
    {
        var definition = MakeDefinition(new JsonObject());

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        var doc = JsonDocument.Parse(result.Evidence!.ResponseData!);
        var arch = doc.RootElement.GetProperty("architecture").GetString();
        Assert.Equal(RuntimeInformation.OSArchitecture.ToString(), arch);
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_Timing_IsPopulated()
    {
        var definition = MakeDefinition(new JsonObject());

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotNull(result.Evidence?.Timing);
        Assert.True(result.Evidence!.Timing!.TotalMs >= 0);
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_Version_MatchesEnvironmentOsVersion()
    {
        var definition = MakeDefinition(new JsonObject());

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        var doc = JsonDocument.Parse(result.Evidence!.ResponseData!);
        var version = doc.RootElement.GetProperty("version").GetString();
        var expected = $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}.{Environment.OSVersion.Version.Build}";
        Assert.Equal(expected, version);
    }

    #endregion

    #region Helpers

    private static TestDefinition MakeDefinition(JsonObject parameters) => new()
    {
        Id = "test-os-001",
        Type = "OsVersion",
        DisplayName = "OS Version Test",
        Parameters = parameters,
        DependsOn = new List<string>()
    };

    #endregion
}
