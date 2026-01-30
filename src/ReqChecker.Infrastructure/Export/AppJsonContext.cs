using System.Text.Json;
using System.Text.Json.Serialization;
using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;

namespace ReqChecker.Infrastructure.Export;

/// <summary>
/// JSON serialization context for AOT compatibility.
/// Uses source generation to eliminate reflection overhead.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(RunReport))]
[JsonSerializable(typeof(TestResult))]
[JsonSerializable(typeof(MachineInfo))]
[JsonSerializable(typeof(RunSummary))]
[JsonSerializable(typeof(TestEvidence))]
[JsonSerializable(typeof(TestError))]
[JsonSerializable(typeof(NetworkInterfaceInfo))]
[JsonSerializable(typeof(TimingBreakdown))]
[JsonSerializable(typeof(TestStatus))]
[JsonSerializable(typeof(ErrorCategory))]
public partial class AppJsonContext : JsonSerializerContext
{
}
