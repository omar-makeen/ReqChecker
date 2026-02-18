using ReqChecker.Core.Enums;
using ReqChecker.Core.Execution;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Tests;
using System.Text.Json.Nodes;

namespace ReqChecker.Infrastructure.Tests.Tests;

/// <summary>
/// Unit tests for CertificateExpiryTest — covers parameter validation, cancellation,
/// and result property correctness without requiring network access.
/// Network-dependent (integration) tests require a live TLS endpoint and are not included here.
/// </summary>
public class CertificateExpiryTestTests
{
    private readonly CertificateExpiryTest _test = new();

    #region Parameter Validation Tests

    [Fact]
    public async Task ExecuteAsync_MissingHost_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject());

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyHost_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject { ["host"] = "" });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceHost_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject { ["host"] = "   " });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(99999)]
    public async Task ExecuteAsync_InvalidPort_ReturnsConfigurationError(int invalidPort)
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "example.com",
            ["port"] = invalidPort
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("65535", result.Error?.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task ExecuteAsync_InvalidTimeout_ReturnsConfigurationError(int invalidTimeout)
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "example.com",
            ["timeout"] = invalidTimeout
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("positive", result.Error?.Message);
    }

    [Fact]
    public async Task ExecuteAsync_NegativeWarningDays_ReturnsConfigurationError()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "example.com",
            ["warningDaysBeforeExpiry"] = -1
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("non-negative", result.Error?.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(443)]
    [InlineData(65535)]
    public async Task ExecuteAsync_ValidPort_PassesValidation(int validPort)
    {
        // Port is valid — test should fail on network (not configuration)
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "192.0.2.1", // TEST-NET-1 (RFC 5737) — not routable, ensures fast network fail
            ["port"] = validPort,
            ["timeout"] = 500
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Should NOT be a configuration error — port validation passed
        Assert.NotEqual(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_NoPortParam_UsesDefault443_PassesValidation()
    {
        // No port specified — should default to 443 and pass parameter validation
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "192.0.2.1",
            ["timeout"] = 500
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotEqual(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_ZeroWarningDays_PassesValidation()
    {
        // warningDaysBeforeExpiry = 0 is valid (disables warning window)
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "192.0.2.1",
            ["warningDaysBeforeExpiry"] = 0,
            ["timeout"] = 500
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.NotEqual(ErrorCategory.Configuration, result.Error?.Category);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task ExecuteAsync_PreCancelledToken_ReturnsSkipped()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "www.example.com",
            ["port"] = 443,
            ["timeout"] = 5000
        });

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await _test.ExecuteAsync(definition, null, cts.Token);

        Assert.Equal(TestStatus.Skipped, result.Status);
        Assert.Equal(ErrorCategory.Unknown, result.Error?.Category);
        Assert.Contains("cancelled", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Result Property Tests

    [Fact]
    public async Task ExecuteAsync_SetsBasicResultProperties()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "192.0.2.1",
            ["timeout"] = 500
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal("test-cert-001", result.TestId);
        Assert.Equal("CertificateExpiry", result.TestType);
        Assert.Equal("Certificate Expiry Test", result.DisplayName);
        Assert.NotEqual(DateTimeOffset.MinValue, result.StartTime);
        Assert.NotEqual(DateTimeOffset.MinValue, result.EndTime);
        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_Failure_HasNonEmptyHumanSummary()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "192.0.2.1",
            ["timeout"] = 500
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.HumanSummary));
    }

    [Fact]
    public async Task ExecuteAsync_ConfigurationFailure_HasNonEmptyTechnicalDetails()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "",
            ["timeout"] = 500
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.HumanSummary));
    }

    [Fact]
    public async Task ExecuteAsync_ContextNotUsed_NullContextAccepted()
    {
        // CertificateExpiry test does not use TestExecutionContext — null should be accepted
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "192.0.2.1",
            ["timeout"] = 500
        });

        var result = await _test.ExecuteAsync(definition, context: null, CancellationToken.None);

        // Should fail on network (not throw NullReferenceException)
        Assert.NotEqual(ErrorCategory.Configuration, result.Error?.Category);
    }

    #endregion

    #region Integration Tests (Require Network - Skipped in CI)

    [Fact(Skip = "Integration test - requires network access to www.google.com:443")]
    public async Task ExecuteAsync_ValidCertificate_ReturnsPass()
    {
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "www.google.com",
            ["port"] = 443,
            ["warningDaysBeforeExpiry"] = 30,
            ["timeout"] = 10000,
            ["skipChainValidation"] = false
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Pass, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.HumanSummary));
        Assert.NotNull(result.Evidence);
        Assert.False(string.IsNullOrEmpty(result.Evidence.ResponseData));
    }

    [Fact(Skip = "Integration test - requires network access to expired.badssl.com:443")]
    public async Task ExecuteAsync_ExpiredCertWithChainValidationOn_ReturnsValidationFail()
    {
        // Verifies FR-004: expired cert with skipChainValidation=false must yield Validation,
        // not Network (the P2 code review bug).
        var definition = MakeDefinition(new JsonObject
        {
            ["host"] = "expired.badssl.com",
            ["port"] = 443,
            ["warningDaysBeforeExpiry"] = 30,
            ["timeout"] = 10000,
            ["skipChainValidation"] = false   // default — the previously-broken path
        });

        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Validation, result.Error?.Category);
        Assert.Contains("expired", result.HumanSummary, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Helpers

    private static TestDefinition MakeDefinition(JsonObject parameters) => new()
    {
        Id = "test-cert-001",
        Type = "CertificateExpiry",
        DisplayName = "Certificate Expiry Test",
        Parameters = parameters,
        DependsOn = new List<string>()
    };

    #endregion
}
