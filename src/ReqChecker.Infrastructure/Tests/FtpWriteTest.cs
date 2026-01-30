using ReqChecker.Core.Execution;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Enums;
using System.Diagnostics;
using System.Text;
using FluentFTP;

namespace ReqChecker.Infrastructure.Tests;

/// <summary>
/// Tests writing a file to an FTP server.
/// </summary>
[TestType("FtpWrite")]
public class FtpWriteTest : ITest
{
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

        try
        {
            // Get parameters
            var host = testDefinition.Parameters["host"]?.ToString() ?? string.Empty;
            var port = testDefinition.Parameters["port"]?.GetValue<int>() ?? 21;
            var username = context?.Username ?? testDefinition.Parameters["username"]?.ToString();
            var password = context?.Password ?? testDefinition.Parameters["password"]?.ToString();
            var remotePath = testDefinition.Parameters["remotePath"]?.ToString() ?? string.Empty;
            var content = testDefinition.Parameters["content"]?.ToString() ?? string.Empty;
            var encoding = testDefinition.Parameters["encoding"]?.ToString() ?? "utf-8";
            var useSsl = testDefinition.Parameters["useSsl"]?.GetValue<bool>() ?? false;
            var validateCertificate = testDefinition.Parameters["validateCertificate"]?.GetValue<bool>() ?? true;
            var createDirectory = testDefinition.Parameters["createDirectory"]?.GetValue<bool>() ?? false;
            var overwrite = testDefinition.Parameters["overwrite"]?.GetValue<bool>() ?? false;

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

            // Check if file exists and overwrite is false
            var exists = await client.FileExists(remotePath, token: cancellationToken);
            if (exists && !overwrite)
            {
                throw new IOException($"Remote file already exists and overwrite is false: {remotePath}");
            }

            // Create directory if needed
            if (createDirectory)
            {
                var directory = System.IO.Path.GetDirectoryName(remotePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    await client.CreateDirectory(directory, cancellationToken);
                }
            }

            // Convert content to bytes
            var encodingObj = Encoding.GetEncoding(encoding);
            var bytes = encodingObj.GetBytes(content);

            // Upload file
            await client.UploadBytes(bytes, remotePath, FtpRemoteExists.Overwrite, progress: null, token: cancellationToken);

            // Verify upload by reading back
            var downloadedBytes = await client.DownloadBytes(remotePath, token: cancellationToken);
            var downloadedContent = encodingObj.GetString(downloadedBytes);
            var isPass = downloadedContent == content;

            // Get file info
            var size = await client.GetFileSize(remotePath, token: cancellationToken);

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.Duration = stopwatch.Elapsed;
            result.Status = isPass ? TestStatus.Pass : TestStatus.Fail;

            // Build evidence
            var evidence = new Dictionary<string, object>
            {
                ["host"] = host,
                ["port"] = port,
                ["remotePath"] = remotePath,
                ["useSsl"] = useSsl,
                ["encoding"] = encoding,
                ["contentLength"] = content.Length,
                ["size"] = size,
                ["createDirectory"] = createDirectory,
                ["overwrite"] = overwrite,
                ["isPass"] = isPass
            };

            result.Evidence = new TestEvidence
            {
                ResponseData = System.Text.Json.JsonSerializer.Serialize(evidence),
                FileContent = content,
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
