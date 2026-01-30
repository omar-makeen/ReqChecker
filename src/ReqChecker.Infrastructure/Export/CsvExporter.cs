using CsvHelper;
using CsvHelper.Configuration;
using ReqChecker.Core.Interfaces;
using ReqChecker.Core.Models;
using ReqChecker.Core.Utilities;
using System.Globalization;

namespace ReqChecker.Infrastructure.Export;

/// <summary>
/// Exports run reports to CSV format using CsvHelper.
/// </summary>
public class CsvExporter : IExporter
{
    /// <inheritdoc />
    public string FileExtension => ".csv";

    /// <inheritdoc />
    public async Task ExportAsync(RunReport report, string filePath, bool maskCredentials = true)
    {
        if (report == null)
        {
            throw new ArgumentNullException(nameof(report));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty", nameof(filePath));
        }

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Apply credential masking if requested
        var reportToExport = maskCredentials ? CredentialMasker.MaskCredentials(report) : report;

        // Write CSV file
        await using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        });

        // Write summary section
        WriteSummary(csv, reportToExport);

        // Write machine info section
        WriteMachineInfo(csv, reportToExport.MachineInfo);

        // Write test results section
        WriteTestResults(csv, reportToExport.Results);

        await csv.FlushAsync();
    }

    private void WriteSummary(CsvWriter csv, RunReport report)
    {
        csv.WriteField("Run Summary");
        csv.NextRecord();

        csv.WriteField("Run ID");
        csv.WriteField(report.RunId);
        csv.NextRecord();

        csv.WriteField("Profile ID");
        csv.WriteField(report.ProfileId);
        csv.NextRecord();

        csv.WriteField("Profile Name");
        csv.WriteField(report.ProfileName);
        csv.NextRecord();

        csv.WriteField("Start Time");
        csv.WriteField(report.StartTime.ToString("o"));
        csv.NextRecord();

        csv.WriteField("End Time");
        csv.WriteField(report.EndTime.ToString("o"));
        csv.NextRecord();

        csv.WriteField("Duration");
        csv.WriteField(report.Duration.ToString());
        csv.NextRecord();

        csv.WriteField("Total Tests");
        csv.WriteField(report.Summary.TotalTests.ToString());
        csv.NextRecord();

        csv.WriteField("Passed");
        csv.WriteField(report.Summary.Passed.ToString());
        csv.NextRecord();

        csv.WriteField("Failed");
        csv.WriteField(report.Summary.Failed.ToString());
        csv.NextRecord();

        csv.WriteField("Skipped");
        csv.WriteField(report.Summary.Skipped.ToString());
        csv.NextRecord();

        csv.WriteField("Pass Rate");
        csv.WriteField($"{report.Summary.PassRate:F2}%");
        csv.NextRecord();

        csv.NextRecord(); // Empty row separator
    }

    private void WriteMachineInfo(CsvWriter csv, MachineInfo machineInfo)
    {
        csv.WriteField("Machine Information");
        csv.NextRecord();

        csv.WriteField("Hostname");
        csv.WriteField(machineInfo.Hostname);
        csv.NextRecord();

        csv.WriteField("OS Version");
        csv.WriteField(machineInfo.OsVersion);
        csv.NextRecord();

        csv.WriteField("OS Build");
        csv.WriteField(machineInfo.OsBuild);
        csv.NextRecord();

        csv.WriteField("Processor Count");
        csv.WriteField(machineInfo.ProcessorCount.ToString());
        csv.NextRecord();

        csv.WriteField("Total Memory (MB)");
        csv.WriteField(machineInfo.TotalMemoryMB.ToString());
        csv.NextRecord();

        csv.WriteField("User Name");
        csv.WriteField(machineInfo.UserName);
        csv.NextRecord();

        csv.WriteField("Is Elevated");
        csv.WriteField(machineInfo.IsElevated.ToString());
        csv.NextRecord();

        csv.WriteField("Network Interfaces");
        csv.WriteField(string.Join("; ", machineInfo.NetworkInterfaces.Select(n => $"{n.Name} ({string.Join(", ", n.IpAddresses)})")));
        csv.NextRecord();

        csv.NextRecord(); // Empty row separator
    }

    private void WriteTestResults(CsvWriter csv, List<TestResult> results)
    {
        csv.WriteField("Test Results");
        csv.NextRecord();

        // Write header
        csv.WriteField("Test ID");
        csv.WriteField("Test Type");
        csv.WriteField("Display Name");
        csv.WriteField("Status");
        csv.WriteField("Start Time");
        csv.WriteField("End Time");
        csv.WriteField("Duration (ms)");
        csv.WriteField("Attempts");
        csv.WriteField("Human Summary");
        csv.WriteField("Technical Details");
        csv.WriteField("Error Category");
        csv.WriteField("Error Message");
        csv.WriteField("Response Code");
        csv.WriteField("Response Data");
        csv.NextRecord();

        // Write each test result
        foreach (var result in results)
        {
            csv.WriteField(result.TestId);
            csv.WriteField(result.TestType);
            csv.WriteField(result.DisplayName);
            csv.WriteField(result.Status.ToString());
            csv.WriteField(result.StartTime.ToString("o"));
            csv.WriteField(result.EndTime.ToString("o"));
            csv.WriteField(result.Duration.TotalMilliseconds.ToString("F0"));
            csv.WriteField(result.AttemptCount.ToString());
            csv.WriteField(result.HumanSummary);
            csv.WriteField(result.TechnicalDetails);

            if (result.Error != null)
            {
                csv.WriteField(result.Error.Category.ToString());
                csv.WriteField(result.Error.Message);
            }
            else
            {
                csv.WriteField(string.Empty);
                csv.WriteField(string.Empty);
            }

            csv.WriteField(result.Evidence.ResponseCode?.ToString() ?? string.Empty);
            csv.WriteField(result.Evidence.ResponseData ?? string.Empty);
            csv.NextRecord();
        }
    }
}
