using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using System.Data.Common;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests database server connectivity for SQL Server, PostgreSQL, and MySQL by opening
/// a connection and executing a lightweight health-check query (<c>SELECT 1</c>).
/// Connection pooling is always disabled to measure real TCP connection establishment time.
/// </summary>
[TestType("SqlConnection")]
public class SqlConnectionTest : ITest
{
    private const string HealthCheckQuery = "SELECT 1";

    private static readonly string[] SupportedTypes = ["SqlServer", "PostgreSQL", "MySQL"];

    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, TestExecutionContext? context, CancellationToken cancellationToken = default)
    {
        var result = new TestResult
        {
            TestId = testDefinition.Id,
            TestType = testDefinition.Type,
            DisplayName = testDefinition.DisplayName,
            Status = TestStatus.Fail,
            StartTime = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        // Sentinel values for always-present evidence keys on failure paths
        var dbType = string.Empty;
        var server = "n/a";
        var database = "n/a";
        var serverVersion = "n/a";
        var responseTimeMs = -1L;
        bool? credentialRefProvided = null;

        try
        {
            var parameters = testDefinition.Parameters;

            // Extract connectionString (required)
            var connectionString = parameters["connectionString"]?.ToString();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string is required");
            }

            // Extract dbType (required, case-insensitive)
            var dbTypeRaw = parameters["dbType"]?.ToString();
            if (string.IsNullOrEmpty(dbTypeRaw))
            {
                throw new ArgumentException($"Database type is required. Supported types: {string.Join(", ", SupportedTypes)}");
            }

            dbType = NormalizeDbType(dbTypeRaw)
                ?? throw new ArgumentException($"Unsupported database type '{dbTypeRaw}'. Supported types: {string.Join(", ", SupportedTypes)}");

            // Extract credentialRef (optional)
            var credentialRef = parameters["credentialRef"]?.ToString();
            var hasCredentialRef = !string.IsNullOrEmpty(credentialRef);
            if (hasCredentialRef)
            {
                credentialRefProvided = false; // will be set to true on success
            }

            // Build connection string via driver-specific builder, disabling pooling
            var builder = CreateBuilder(dbType, connectionString);
            builder["Pooling"] = false;

            // Inject credentials from credential store (overrides any in connection string)
            if (hasCredentialRef)
            {
                if (context?.Username == null)
                {
                    throw new ArgumentException($"Credential reference '{credentialRef}' could not be resolved from the credential store");
                }

                SetCredentials(dbType, builder, context.Username, context.Password ?? string.Empty);
            }

            var finalConnectionString = builder.ConnectionString;

            // Parse server and database from builder for evidence (before connection attempt)
            server = ParseServer(dbType, builder);
            database = ParseDatabase(dbType, builder);

            // Open connection and execute health-check query
            await using var connection = CreateConnection(dbType, finalConnectionString);

            stopwatch.Restart();
            await connection.OpenAsync(cancellationToken);
            serverVersion = connection.ServerVersion ?? "n/a";

            await using var command = connection.CreateCommand();
            command.CommandText = HealthCheckQuery;
            var queryResult = await command.ExecuteScalarAsync(cancellationToken);

            stopwatch.Stop();
            responseTimeMs = stopwatch.ElapsedMilliseconds;
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Pass;

            // Build evidence — always-present keys
            var evidence = new Dictionary<string, object>
            {
                ["dbType"] = dbType,
                ["server"] = server,
                ["database"] = database,
                ["serverVersion"] = serverVersion,
                ["responseTimeMs"] = responseTimeMs,
                ["connectionSucceeded"] = true
            };

            // Conditional: authenticated (only when credentialRef was provided)
            if (hasCredentialRef)
            {
                evidence["authenticated"] = true;
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = JsonSerializer.Serialize(evidence),
                Timing = new TimingBreakdown { TotalMs = (int)responseTimeMs }
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Skipped;
            result.Error = new TestError
            {
                Category = ErrorCategory.Unknown,
                Message = "Test was cancelled or timed out"
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, -1L, credentialRefProvided);
        }
        catch (ArgumentException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = ex.Message
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, -1L, credentialRefProvided);
        }
        catch (FormatException ex)
        {
            // SqlConnectionStringBuilder throws FormatException for invalid values (e.g., "Connect Timeout=abc")
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = $"Invalid connection string format: {ex.Message}"
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, -1L, credentialRefProvided);
        }
        catch (KeyNotFoundException ex)
        {
            // SqlConnectionStringBuilder throws KeyNotFoundException for unrecognized keywords
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Configuration,
                Message = $"Invalid connection string keyword: {ex.Message}"
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, -1L, credentialRefProvided);
        }
        catch (DbException ex) when (ex.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase) ||
                                     ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                                     ex.Message.Contains("connection attempt failed", StringComparison.OrdinalIgnoreCase))
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Timeout,
                Message = $"Database server unreachable: connection timed out"
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, -1L, credentialRefProvided);
        }
        catch (DbException ex) when (ex.Message.Contains("Login failed", StringComparison.OrdinalIgnoreCase) ||
                                     ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                                     ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                                     ex.Message.Contains("Access denied", StringComparison.OrdinalIgnoreCase))
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Permission,
                Message = $"Database authentication failed: {ex.Message}"
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, stopwatch.ElapsedMilliseconds,
                credentialRefProvided.HasValue ? false : null);
        }
        catch (DbException ex) when (ex.Message.Contains("database", StringComparison.OrdinalIgnoreCase) &&
                                     (ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
                                      ex.Message.Contains("cannot open", StringComparison.OrdinalIgnoreCase) ||
                                      ex.Message.Contains("unknown database", StringComparison.OrdinalIgnoreCase)))
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = $"Database not accessible: {ex.Message}"
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, stopwatch.ElapsedMilliseconds, credentialRefProvided);
        }
        catch (DbException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = $"Database connection failed: {ex.Message}"
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, -1L, credentialRefProvided);
        }
        catch (SocketException ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = $"Database server unreachable: {ex.Message}"
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, -1L, credentialRefProvided);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
            SetFailureEvidence(result, dbType, server, database, serverVersion, -1L, credentialRefProvided);
        }

        return result;
    }

    /// <summary>
    /// Normalises a case-insensitive dbType string to the canonical casing,
    /// or returns <c>null</c> if not a supported type.
    /// </summary>
    private static string? NormalizeDbType(string dbTypeRaw) =>
        dbTypeRaw.ToUpperInvariant() switch
        {
            "SQLSERVER" => "SqlServer",
            "POSTGRESQL" => "PostgreSQL",
            "MYSQL" => "MySQL",
            _ => null
        };

    /// <summary>
    /// Returns a <see cref="DbConnectionStringBuilder"/> populated with <paramref name="connectionString"/>
    /// for the given <paramref name="dbType"/>.
    /// </summary>
    private static DbConnectionStringBuilder CreateBuilder(string dbType, string connectionString) =>
        dbType switch
        {
            "SqlServer" => new SqlConnectionStringBuilder(connectionString),
            "PostgreSQL" => new NpgsqlConnectionStringBuilder(connectionString),
            "MySQL" => new MySqlConnectionStringBuilder(connectionString),
            _ => throw new ArgumentException($"Unsupported database type '{dbType}'")
        };

    /// <summary>
    /// Returns a <see cref="DbConnection"/> for the given <paramref name="dbType"/> and
    /// <paramref name="connectionString"/>.
    /// </summary>
    private static DbConnection CreateConnection(string dbType, string connectionString) =>
        dbType switch
        {
            "SqlServer" => new SqlConnection(connectionString),
            "PostgreSQL" => new NpgsqlConnection(connectionString),
            "MySQL" => new MySqlConnection(connectionString),
            _ => throw new ArgumentException($"Unsupported database type '{dbType}'")
        };

    /// <summary>
    /// Extracts the server address from a builder using driver-specific property names.
    /// </summary>
    private static string ParseServer(string dbType, DbConnectionStringBuilder builder)
    {
        var value = dbType switch
        {
            "SqlServer" => ((SqlConnectionStringBuilder)builder).DataSource,
            "PostgreSQL" => ((NpgsqlConnectionStringBuilder)builder).Host,
            "MySQL" => ((MySqlConnectionStringBuilder)builder).Server,
            _ => null
        };
        return string.IsNullOrEmpty(value) ? "n/a" : value;
    }

    /// <summary>
    /// Extracts the database name from a builder using driver-specific property names.
    /// </summary>
    private static string ParseDatabase(string dbType, DbConnectionStringBuilder builder)
    {
        var value = dbType switch
        {
            "SqlServer" => ((SqlConnectionStringBuilder)builder).InitialCatalog,
            "PostgreSQL" => ((NpgsqlConnectionStringBuilder)builder).Database,
            "MySQL" => ((MySqlConnectionStringBuilder)builder).Database,
            _ => null
        };
        return string.IsNullOrEmpty(value) ? "n/a" : value;
    }

    /// <summary>
    /// Injects username and password into the builder using driver-specific property names,
    /// overriding any credentials already present in the connection string.
    /// </summary>
    private static void SetCredentials(string dbType, DbConnectionStringBuilder builder, string username, string password)
    {
        switch (dbType)
        {
            case "SqlServer":
                var sqlBuilder = (SqlConnectionStringBuilder)builder;
                sqlBuilder.UserID = username;
                sqlBuilder.Password = password;
                break;
            case "PostgreSQL":
                var npgsqlBuilder = (NpgsqlConnectionStringBuilder)builder;
                npgsqlBuilder.Username = username;
                npgsqlBuilder.Password = password;
                break;
            case "MySQL":
                var mysqlBuilder = (MySqlConnectionStringBuilder)builder;
                mysqlBuilder.UserID = username;
                mysqlBuilder.Password = password;
                break;
        }
    }

    /// <summary>
    /// Populates failure-path evidence with always-present fields.
    /// Per FR-014: all "always" fields must appear on every outcome; use -1 for responseTimeMs as sentinel (rendered as n/a).
    /// </summary>
    private static void SetFailureEvidence(
        TestResult result,
        string dbType,
        string server,
        string database,
        string serverVersion,
        long responseTimeMs,
        bool? authenticated = null)
    {
        var evidence = new Dictionary<string, object>
        {
            ["dbType"] = string.IsNullOrEmpty(dbType) ? "n/a" : dbType,
            ["server"] = server,
            ["database"] = database,
            ["serverVersion"] = serverVersion,
            ["responseTimeMs"] = responseTimeMs,
            ["connectionSucceeded"] = false
        };

        if (authenticated.HasValue)
        {
            evidence["authenticated"] = authenticated.Value;
        }

        result.Evidence = new TestEvidence
        {
            ResponseData = JsonSerializer.Serialize(evidence),
            Timing = new TimingBreakdown
            {
                TotalMs = responseTimeMs > 0 ? (int)responseTimeMs : 0
            }
        };
    }
}
