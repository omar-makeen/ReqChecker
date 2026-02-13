using ReqChecker.Core.Enums;
using ReqChecker.Core.Execution;
using ReqChecker.Core.Models;
using ReqChecker.Infrastructure.Tests;
using System.Text.Json.Nodes;

namespace ReqChecker.Infrastructure.Tests.Tests;

/// <summary>
/// Unit tests for MtlsConnectTest including parameter validation, certificate loading, and cancellation handling.
/// Note: Actual mTLS connection tests require network access and valid certificates - marked as integration tests.
/// </summary>
public class MtlsConnectTestTests
{
    private readonly MtlsConnectTest _test = new();

    #region Parameter Validation Tests

    [Fact]
    public async Task ExecuteAsync_MissingUrl_ReturnsConfigurationError()
    {
        // Arrange - Don't include url at all
        var jsonParameters = new JsonObject
        {
            ["clientCertPath"] = "C:\\certs\\test.pfx"
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyUrl_ReturnsConfigurationError()
    {
        // Arrange
        var jsonParameters = new JsonObject
        {
            ["url"] = "",
            ["clientCertPath"] = "C:\\certs\\test.pfx"
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_MissingClientCertPath_ReturnsConfigurationError()
    {
        // Arrange - Don't include clientCertPath at all
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api"
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyClientCertPath_ReturnsConfigurationError()
    {
        // Arrange
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api",
            ["clientCertPath"] = ""
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_HttpUrl_ReturnsConfigurationError()
    {
        // Arrange - HTTP (not HTTPS) should be rejected
        var jsonParameters = new JsonObject
        {
            ["url"] = "http://example.com/api",
            ["clientCertPath"] = "C:\\certs\\test.pfx"
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("HTTPS", result.Error?.Message);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(600)]
    public async Task ExecuteAsync_InvalidExpectedStatus_ReturnsConfigurationError(int invalidStatus)
    {
        // Arrange
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api",
            ["clientCertPath"] = "C:\\certs\\test.pfx",
            ["expectedStatus"] = invalidStatus
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("HTTP status code", result.Error?.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidTimeout_ReturnsConfigurationError()
    {
        // Arrange
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api",
            ["clientCertPath"] = "C:\\certs\\test.pfx",
            ["timeout"] = 0
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("Timeout must be a positive integer", result.Error?.Message);
    }

    #endregion

    #region Certificate File Tests

    [Fact]
    public async Task ExecuteAsync_PfxNotFound_ReturnsConfigurationError()
    {
        // Arrange - Use a path that definitely doesn't exist
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api",
            ["clientCertPath"] = "C:\\nonexistent-path-12345\\cert.pfx"
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal(TestStatus.Fail, result.Status);
        Assert.Equal(ErrorCategory.Configuration, result.Error?.Category);
        Assert.Contains("PFX file not found", result.Error?.Message);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task ExecuteAsync_PreCancellation_ReturnsSkipped()
    {
        // Arrange - Use valid params so we pass validation before cancellation check
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api",
            ["clientCertPath"] = "C:\\nonexistent\\cert.pfx",
            ["timeout"] = 5000
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await _test.ExecuteAsync(definition, null, cts.Token);

        // Assert - Pre-cancelled token should result in Skipped status
        Assert.Equal(TestStatus.Skipped, result.Status);
        Assert.Equal(ErrorCategory.Unknown, result.Error?.Category);
        Assert.Contains("cancelled", result.HumanSummary.ToLowerInvariant());
    }

    #endregion

    #region Result Properties Tests

    [Fact]
    public async Task ExecuteAsync_SetsBasicResultProperties()
    {
        // Arrange
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api",
            ["clientCertPath"] = "C:\\nonexistent\\cert.pfx",
            ["timeout"] = 100
        };

        var definition = new TestDefinition
        {
            Id = "test-mtls-001",
            Type = "MtlsConnect",
            DisplayName = "mTLS Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.Equal("test-mtls-001", result.TestId);
        Assert.Equal("MtlsConnect", result.TestType);
        Assert.Equal("mTLS Test", result.DisplayName);
        Assert.NotEqual(DateTimeOffset.MinValue, result.StartTime);
        Assert.NotEqual(DateTimeOffset.MinValue, result.EndTime);
        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Fact]
    public async Task ExecuteAsync_Failure_IncludesHumanReadableSummary()
    {
        // Arrange
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api",
            ["clientCertPath"] = "C:\\nonexistent\\cert.pfx",
            ["timeout"] = 100
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrEmpty(result.HumanSummary));
        Assert.False(string.IsNullOrEmpty(result.TechnicalDetails));
    }

    #endregion

    #region Expected Status Code Tests

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(204)]
    [InlineData(301)]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(503)]
    [InlineData(599)]
    public async Task ExecuteAsync_ValidExpectedStatus_AcceptsStatus(int validStatus)
    {
        // Arrange - Use nonexistent cert to fail fast without network call
        var jsonParameters = new JsonObject
        {
            ["url"] = "https://example.com/api",
            ["clientCertPath"] = "C:\\nonexistent\\cert.pfx",
            ["expectedStatus"] = validStatus,
            ["timeout"] = 100
        };

        var definition = new TestDefinition
        {
            Id = "test-001",
            Type = "MtlsConnect",
            DisplayName = "Test",
            Parameters = jsonParameters,
            DependsOn = new List<string>()
        };

        // Act
        var result = await _test.ExecuteAsync(definition, null, CancellationToken.None);

        // Assert - Should fail (cert not found) but NOT because of invalid expectedStatus
        Assert.Equal(TestStatus.Fail, result.Status);
        // Configuration error should be about the cert file, not the status code
        if (result.Error?.Category == ErrorCategory.Configuration)
        {
            Assert.DoesNotContain("HTTP status code", result.Error?.Message);
        }
    }

    #endregion
}
