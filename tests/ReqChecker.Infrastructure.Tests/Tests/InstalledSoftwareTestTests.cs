using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Tests;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ReqChecker.Infrastructure.Tests.Tests;

/// <summary>
/// Unit tests for InstalledSoftwareTest — covers parameter validation, registry search,
/// version comparison, evidence capture, and cancellation. Uses "Microsoft Edge"
/// (universally present on Windows) for found-software scenarios and a guaranteed-absent
/// name for not-found scenarios.
/// </summary>
public class InstalledSoftwareTestTests
{
    private readonly InstalledSoftwareTest _test = new();

    #region Parameter Validation Tests

    [Fact]
    public async Task ExecuteAsync_EmptySoftwareName_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("softwareName", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_NullSoftwareName_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = null,
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceSoftwareName_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "   ",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidMinimumVersionFormat_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = "abc"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("abc", result.Error!.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidMinimumVersionFormat_AlphaNumeric_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = "1.0.0-beta"
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
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = null
        });
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
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal("test-sw-001", result.TestId);
        Assert.Equal("InstalledSoftware", result.TestType);
        Assert.Equal("Installed Software Test", result.DisplayName);
        Assert.NotEqual(default, result.StartTime);
        Assert.NotEqual(default, result.EndTime);
        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_HumanSummary_IsAlwaysPopulated()
    {
        // Configuration error path
        var def1 = MakeDefinition(new JsonObject { ["softwareName"] = "", ["minimumVersion"] = null });
        var r1 = await _test.ExecuteAsync(def1, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r1.HumanSummary));

        // Found path
        var def2 = MakeDefinition(new JsonObject { ["softwareName"] = "Microsoft Edge", ["minimumVersion"] = null });
        var r2 = await _test.ExecuteAsync(def2, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r2.HumanSummary));

        // Not found path
        var def3 = MakeDefinition(new JsonObject { ["softwareName"] = "NonExistentSoftware12345", ["minimumVersion"] = null });
        var r3 = await _test.ExecuteAsync(def3, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r3.HumanSummary));
    }

    #endregion

    #region Found Software Tests (User Story 1 / US3 Informational)

    [Fact]
    public async Task ExecuteAsync_MicrosoftEdge_ReturnsPass()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Null(result.Error);
        Assert.False(string.IsNullOrWhiteSpace(result.HumanSummary));
    }

    [Fact]
    public async Task ExecuteAsync_MicrosoftEdge_HumanSummary_ContainsInstalled()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Contains("installed", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_CaseInsensitiveSearch_ReturnsPass()
    {
        // "microsoft edge" in lowercase should still find "Microsoft Edge"
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "microsoft edge",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task ExecuteAsync_NonExistentSoftware_ReturnsFail()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "NonExistentSoftware12345",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistentSoftware_HumanSummary_MentionsSoftwareName()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "NonExistentSoftware12345",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Contains("NonExistentSoftware12345", result.HumanSummary);
    }

    #endregion

    #region Minimum Version Tests (User Story 2)

    [Fact]
    public async Task ExecuteAsync_MinimumVersion_VeryLow_ReturnsPass()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = "1.0"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Null(result.Error);
        Assert.Contains("meets minimum", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_MinimumVersion_VeryHigh_ReturnsFail()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = "999.0"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Contains("does not meet minimum", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_MinimumVersion_NotFound_ReturnsNotFoundError()
    {
        // Not found should produce a "not found" error, not a version comparison error
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "NonExistentSoftware12345",
            ["minimumVersion"] = "1.0"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_MinimumVersion_HumanSummary_ContainsVersionStrings()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = "1.0"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Contains("1.0", result.HumanSummary);
    }

    #endregion

    #region Evidence Tests

    [Fact]
    public async Task ExecuteAsync_Evidence_ContainsAllRequiredFields()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotNull(result.Evidence);
        Assert.False(string.IsNullOrWhiteSpace(result.Evidence.ResponseData));

        var doc = JsonDocument.Parse(result.Evidence.ResponseData!);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("displayName", out _), "Missing 'displayName'");
        Assert.True(root.TryGetProperty("version", out _), "Missing 'version'");
        Assert.True(root.TryGetProperty("installLocation", out _), "Missing 'installLocation'");
        Assert.True(root.TryGetProperty("publisher", out _), "Missing 'publisher'");
        Assert.True(root.TryGetProperty("installDate", out _), "Missing 'installDate'");
        Assert.True(root.TryGetProperty("allMatches", out _), "Missing 'allMatches'");
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_DisplayName_ContainsSoftwareName()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        var doc = JsonDocument.Parse(result.Evidence!.ResponseData!);
        var displayName = doc.RootElement.GetProperty("displayName").GetString();
        Assert.Contains("Edge", displayName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_Timing_IsPopulated()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotNull(result.Evidence?.Timing);
        Assert.True(result.Evidence!.Timing!.TotalMs >= 0);
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_AllMatches_IsArray()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["softwareName"] = "Microsoft Edge",
            ["minimumVersion"] = null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        var doc = JsonDocument.Parse(result.Evidence!.ResponseData!);
        var allMatches = doc.RootElement.GetProperty("allMatches");
        Assert.Equal(JsonValueKind.Array, allMatches.ValueKind);
        Assert.True(allMatches.GetArrayLength() >= 1);
    }

    #endregion

    #region Helpers

    private static TestDefinition MakeDefinition(JsonObject parameters) => new()
    {
        Id = "test-sw-001",
        Type = "InstalledSoftware",
        DisplayName = "Installed Software Test",
        Parameters = parameters,
        DependsOn = new List<string>()
    };

    #endregion
}
