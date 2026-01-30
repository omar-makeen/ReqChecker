using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Text;
using FluentFTP;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests reading a file from an FTP server.
/// </summary>
[TestType("FtpRead")]
public class FtpReadTest : ITest
{
    /// <inheritdoc/>
    public async Task<TestResult> ExecuteAsync(TestDefinition testDefinition, CancellationToken cancellationToken = default)
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

        try
        {
            // Get parameters
            var host = testDefinition.Parameters["host"]?.ToString() ?? string.Empty;
            var port = testDefinition.Parameters["port"]?.GetValue<int>() ?? 21;
            var username = testDefinition.Parameters["username"]?.ToString();
            var password = testDefinition.Parameters["password"]?.ToString();
            var remotePath = testDefinition.Parameters["remotePath"]?.ToString() ?? string.Empty;
            var useSsl = testDefinition.Parameters["useSsl"]?.GetValue<bool>() ?? false;
            var validateCertificate = testDefinition.Parameters["validateCertificate"]?.GetValue<bool>() ?? true;

            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Host parameter is required", nameof(host));
            }

            if (string.IsNullOrEmpty(remotePath))
            {
                throw new ArgumentException("RemotePath parameter is required", nameof(remotePath));
            }

            // Configure FTP client
            var client = new AsyncFtpClient(host, port);
            client.Credentials = new System.Net.NetworkCredential(username ?? "anonymous", password ?? "anonymous@");
            client.Config.EncryptionMode = useSsl ? FtpEncryptionMode.Explicit : FtpEncryptionMode.None;
            client.Config.ValidateAnyCertificate = !validateCertificate;
            client.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;
            client.Config.RetryAttempts = 3;

            // Connect to FTP server
            await client.Connect(cancellationToken);

            // Check if file exists
            var exists = await client.FileExists(remotePath, token: cancellationToken);

            if (!exists)
            {
                throw new FileNotFoundException($"Remote file not found: {remotePath}");
            }

            // Get file info
            var size = await client.GetFileSize(remotePath, token: cancellationToken);
            var modified = await client.GetModifiedTime(remotePath, token: cancellationToken);

            // Read file content (download bytes and convert to string)
            var bytes = await client.DownloadBytes(remotePath, token: cancellationToken);
            var content = Encoding.UTF8.GetString(bytes);

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Pass;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["host"] = host,
                ["port"] = port,
                ["remotePath"] = remotePath,
                ["useSsl"] = useSsl,
                ["size"] = size,
                ["lastModified"] = modified,
                ["contentLength"] = content.Length
            };

            // Truncate content if too long
            var maxLength = testDefinition.Parameters["maxLength"]?.GetValue<int>() ?? 10240;
            if (content.Length > maxLength)
            {
                evidence["content"] = content.Substring(0, maxLength) + "... (truncated)";
                evidence["truncated"] = true;
            }
            else
            {
                evidence["content"] = content;
                evidence["truncated"] = false;
            }

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                FileContent = evidence["content"] as string,
                Timing = new TimingBreakdown
                {
                    TotalMs = (int)stopwatch.ElapsedMilliseconds,
                    ConnectMs = (int)stopwatch.ElapsedMilliseconds
                }
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
                Message = "Test was cancelled"
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = TestStatus.Fail;
            result.Error = new TestError
            {
                Category = ErrorCategory.Network,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }

        return result;
    }
}
