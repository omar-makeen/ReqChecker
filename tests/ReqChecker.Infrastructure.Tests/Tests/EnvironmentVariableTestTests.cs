using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Tests;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ReqChecker.Infrastructure.Tests.Tests;

/// <summary>
/// Unit tests for EnvironmentVariableTest — covers parameter validation, existence checking,
/// value matching (exact, contains, regex, pathContains), evidence capture, and cancellation.
/// Uses universally-present variables (PATH, OS) for found scenarios and a guaranteed-absent
/// name for not-found scenarios.
/// </summary>
public class EnvironmentVariableTestTests
{
    private readonly EnvironmentVariableTest _test = new();

    #region Parameter Validation Tests

    [Fact]
    public async Task ExecuteAsync_EmptyVariableName_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("variableName", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_NullVariableName_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = (string?)null,
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceVariableName_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "   ",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_MatchTypeWithoutExpectedValue_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = (string?)null,
            ["matchType"] = "exact"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("expectedValue", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_UnrecognizedMatchType_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = "something",
            ["matchType"] = "fuzzy"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("fuzzy", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidRegexPattern_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "[invalid(",
            ["matchType"] = "regex"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("Invalid regex pattern", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task ExecuteAsync_CancelledToken_ReturnsSkipped()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
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
            ["variableName"] = "PATH",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal("test-env-001", result.TestId);
        Assert.Equal("EnvironmentVariable", result.TestType);
        Assert.Equal("Environment Variable Test", result.DisplayName);
        Assert.NotEqual(default, result.StartTime);
        Assert.NotEqual(default, result.EndTime);
        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_HumanSummary_IsAlwaysPopulated()
    {
        // Configuration error path
        var def1 = MakeDefinition(new JsonObject { ["variableName"] = "", ["expectedValue"] = (string?)null, ["matchType"] = (string?)null });
        var r1 = await _test.ExecuteAsync(def1, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r1.HumanSummary));

        // Found path
        var def2 = MakeDefinition(new JsonObject { ["variableName"] = "PATH", ["expectedValue"] = (string?)null, ["matchType"] = (string?)null });
        var r2 = await _test.ExecuteAsync(def2, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r2.HumanSummary));

        // Not found path
        var def3 = MakeDefinition(new JsonObject { ["variableName"] = "NONEXISTENT_VAR_12345", ["expectedValue"] = (string?)null, ["matchType"] = (string?)null });
        var r3 = await _test.ExecuteAsync(def3, null, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(r3.HumanSummary));
    }

    #endregion

    #region Existence Check Tests (User Story 1)

    [Fact]
    public async Task ExecuteAsync_PathVariable_NoExpectedValue_ReturnsPass()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Null(result.Error);
        Assert.False(string.IsNullOrWhiteSpace(result.HumanSummary));
    }

    [Fact]
    public async Task ExecuteAsync_PathVariable_HumanSummary_MentionsVariableName()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Contains("PATH", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistentVariable_ReturnsFail()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "NONEXISTENT_VAR_12345",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Contains("not defined", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistentVariable_HumanSummary_MentionsVariableName()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "NONEXISTENT_VAR_12345",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Contains("NONEXISTENT_VAR_12345", result.HumanSummary);
    }

    #endregion

    #region Exact Match Tests (User Story 2)

    [Fact]
    public async Task ExecuteAsync_ExactMatch_Matching_ReturnsPass()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "Windows_NT",
            ["matchType"] = "exact"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Null(result.Error);
        Assert.Contains("equals", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ExactMatch_CaseInsensitive_ReturnsPass()
    {
        // "windows_nt" (lowercase) should still match via case-insensitive comparison
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "windows_nt",
            ["matchType"] = "exact"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ExactMatch_NonMatching_ReturnsFail()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "Linux",
            ["matchType"] = "exact"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_ExactMatch_DefaultsToExact_WhenMatchTypeOmitted()
    {
        // When expectedValue is set and matchType is null, it should default to exact
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "Windows_NT",
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
    }

    #endregion

    #region Contains Match Tests (User Story 2)

    [Fact]
    public async Task ExecuteAsync_ContainsMatch_Matching_ReturnsPass()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "Windows",
            ["matchType"] = "contains"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Contains("contains", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ContainsMatch_NonMatching_ReturnsFail()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "macOS",
            ["matchType"] = "contains"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
    }

    #endregion

    #region Regex Match Tests (User Story 2)

    [Fact]
    public async Task ExecuteAsync_RegexMatch_Matching_ReturnsPass()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "^Windows.*",
            ["matchType"] = "regex"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Contains("matches pattern", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RegexMatch_NonMatching_ReturnsFail()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "OS",
            ["expectedValue"] = "^Linux.*",
            ["matchType"] = "regex"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
    }

    #endregion

    #region PathContains Tests (User Story 3)

    [Fact]
    public async Task ExecuteAsync_PathContains_System32Present_ReturnsPass()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = @"C:\Windows\System32",
            ["matchType"] = "pathContains"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.Contains("contains path", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_PathContains_TrailingBackslashNormalized_ReturnsPass()
    {
        // C:\Windows\System32\ (with trailing slash) should still match C:\Windows\System32
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = @"C:\Windows\System32\",
            ["matchType"] = "pathContains"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_PathContains_AbsentPath_ReturnsFail()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = @"C:\NonExistent\Path\12345",
            ["matchType"] = "pathContains"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Contains("does not contain path", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Evidence Tests

    [Fact]
    public async Task ExecuteAsync_Evidence_ContainsRequiredFields()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotNull(result.Evidence);
        Assert.False(string.IsNullOrWhiteSpace(result.Evidence.ResponseData));

        var doc = JsonDocument.Parse(result.Evidence.ResponseData!);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("variableName", out _), "Missing 'variableName'");
        Assert.True(root.TryGetProperty("found", out _), "Missing 'found'");
        Assert.True(root.TryGetProperty("actualValue", out _), "Missing 'actualValue'");
        Assert.True(root.TryGetProperty("matchType", out _), "Missing 'matchType'");
        Assert.True(root.TryGetProperty("expectedValue", out _), "Missing 'expectedValue'");
        Assert.True(root.TryGetProperty("matchResult", out _), "Missing 'matchResult'");
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_Found_IsTrue_ForExistingVariable()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        var doc = JsonDocument.Parse(result.Evidence!.ResponseData!);
        var found = doc.RootElement.GetProperty("found").GetBoolean();
        Assert.True(found);
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_Found_IsFalse_ForMissingVariable()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "NONEXISTENT_VAR_12345",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        var doc = JsonDocument.Parse(result.Evidence!.ResponseData!);
        var found = doc.RootElement.GetProperty("found").GetBoolean();
        Assert.False(found);
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_Timing_IsPopulated()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = (string?)null,
            ["matchType"] = (string?)null
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotNull(result.Evidence?.Timing);
        Assert.True(result.Evidence!.Timing!.TotalMs >= 0);
    }

    [Fact]
    public async Task ExecuteAsync_Evidence_PathEntries_PresentForPathContains()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["variableName"] = "PATH",
            ["expectedValue"] = @"C:\Windows\System32",
            ["matchType"] = "pathContains"
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        var doc = JsonDocument.Parse(result.Evidence!.ResponseData!);
        Assert.True(doc.RootElement.TryGetProperty("pathEntries", out var pe), "Missing 'pathEntries'");
        Assert.Equal(JsonValueKind.Array, pe.ValueKind);
    }

    #endregion

    #region Helpers

    private static TestDefinition MakeDefinition(JsonObject parameters) => new()
    {
        Id = "test-env-001",
        Type = "EnvironmentVariable",
        DisplayName = "Environment Variable Test",
        Parameters = parameters,
        DependsOn = new List<string>()
    };

    #endregion
}
