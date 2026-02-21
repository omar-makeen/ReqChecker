using ReqChecker.Core.Enums;
using ReqChecker.Core.Models;
using System.Globalization;
using System.Text.Json;
using System.Windows.Data;

namespace ReqChecker.App.Converters;

/// <summary>
/// Generates formatted technical details from a TestResult's Evidence.
/// </summary>
public class TestResultDetailsConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TestResult result)
            return null!;

        return GenerateTechnicalDetails(result)!;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("TestResultDetailsConverter is one-way only.");
    }

    private static string? GenerateTechnicalDetails(TestResult result)
    {
        var sections = new List<string>();

        // Parse ResponseData JSON for additional information
        Dictionary<string, object>? evidenceData = null;
        if (!string.IsNullOrEmpty(result.Evidence.ResponseData))
        {
            try
            {
                evidenceData = JsonSerializer.Deserialize<Dictionary<string, object>>(result.Evidence.ResponseData);
            }
            catch
            {
                // If JSON parsing fails, continue with default behavior
            }
        }

        // [General] section - always shown
        sections.Add("[General]");
        sections.Add($"Duration: {result.Duration.TotalMilliseconds:F0}ms");
        sections.Add($"Attempts: {result.AttemptCount}");
        sections.Add(string.Empty);

        // [OS Version] section — emitted when Evidence contains OS version data
        if (evidenceData != null && evidenceData.ContainsKey("productName"))
        {
            sections.Add("[OS Version]");
            if (evidenceData.TryGetValue("productName", out var pnObj) && pnObj != null)
                sections.Add($"Product:      {pnObj}");
            if (evidenceData.TryGetValue("version", out var vObj) && vObj != null)
                sections.Add($"Version:      {vObj}");
            if (evidenceData.TryGetValue("buildNumber", out var bnObj) && bnObj != null)
                sections.Add($"Build:        {bnObj}");
            if (evidenceData.TryGetValue("architecture", out var archObj) && archObj != null)
                sections.Add($"Architecture: {archObj}");
            sections.Add(string.Empty);
        }

        // [Installed Software] section — emitted when Evidence contains installed software data (keyed on allMatches, which is unique to InstalledSoftwareTest)
        if (evidenceData != null && evidenceData.ContainsKey("allMatches"))
        {
            sections.Add("[Installed Software]");
            if (evidenceData.TryGetValue("displayName", out var dnObj) && dnObj != null)
                sections.Add($"Name:         {dnObj}");
            if (evidenceData.TryGetValue("version", out var isVerObj) && isVerObj != null)
                sections.Add($"Version:      {isVerObj}");
            if (evidenceData.TryGetValue("installLocation", out var locObj) && locObj != null && locObj.ToString() is { Length: > 0 } loc)
                sections.Add($"Location:     {loc}");
            if (evidenceData.TryGetValue("publisher", out var pubObj) && pubObj != null && pubObj.ToString() is { Length: > 0 } pub)
                sections.Add($"Publisher:    {pub}");
            if (evidenceData.TryGetValue("installDate", out var idObj) && idObj != null && idObj.ToString() is { Length: > 0 } id)
                sections.Add($"Install Date: {id}");

            // Show additional matches if more than one entry was found
            if (evidenceData.TryGetValue("allMatches", out var amObj) && amObj?.ToString() is { } amStr)
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(amStr);
                    if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array &&
                        doc.RootElement.GetArrayLength() > 1)
                    {
                        sections.Add(string.Empty);
                        sections.Add("[All Matches]");
                        foreach (var match in doc.RootElement.EnumerateArray())
                        {
                            var mName = match.TryGetProperty("displayName", out var mn) ? mn.GetString() : null;
                            var mVer = match.TryGetProperty("version", out var mv) ? mv.GetString() : null;
                            sections.Add($"  {mName} ({mVer ?? "unknown"})");
                        }
                    }
                }
                catch
                {
                    // ignore parse errors for allMatches
                }
            }

            sections.Add(string.Empty);
        }

        // [Environment Variable] section — emitted when Evidence contains environment variable data (keyed on variableName AND found, unique to EnvironmentVariableTest)
        if (evidenceData != null && evidenceData.ContainsKey("variableName") && evidenceData.ContainsKey("found"))
        {
            sections.Add("[Environment Variable]");
            if (evidenceData.TryGetValue("variableName", out var vnObj) && vnObj != null)
                sections.Add($"Variable:     {vnObj}");

            var isFound = evidenceData.TryGetValue("found", out var foundObj) && foundObj?.ToString() is "True" or "true";
            sections.Add($"Found:        {(isFound ? "yes" : "no")}");

            if (isFound && evidenceData.TryGetValue("actualValue", out var avObj) && avObj != null)
            {
                var avStr = avObj.ToString() ?? string.Empty;
                var display = avStr.Length > 200 ? avStr[..200] + "…" : avStr;
                sections.Add($"Value:        {display}");
            }
            else if (!isFound)
            {
                sections.Add("Value:        N/A");
            }

            if (evidenceData.TryGetValue("matchType", out var mtObj) && mtObj?.ToString() is { } mt
                && mt != "existence")
            {
                sections.Add($"Match Type:   {mt}");
                if (evidenceData.TryGetValue("expectedValue", out var evObj) && evObj != null)
                    sections.Add($"Expected:     {evObj}");
                if (evidenceData.TryGetValue("matchResult", out var mrObj) && mrObj != null)
                    sections.Add($"Match Result: {mrObj}");

                // pathContains: show individual path entries
                if (string.Equals(mt, "pathContains", StringComparison.OrdinalIgnoreCase) &&
                    evidenceData.TryGetValue("pathEntries", out var peObj) && peObj?.ToString() is { } peStr)
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(peStr);
                        if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            sections.Add(string.Empty);
                            sections.Add("[Path Entries]");
                            var entries = doc.RootElement.EnumerateArray().ToList();
                            var shown = entries.Take(20).ToList();
                            foreach (var entry in shown)
                                sections.Add($"  {entry.GetString()}");
                            if (entries.Count > 20)
                                sections.Add($"  … and {entries.Count - 20} more");
                        }
                    }
                    catch
                    {
                        // ignore parse errors
                    }
                }
            }

            sections.Add(string.Empty);
        }

        // [System RAM] section — emitted when Evidence contains SystemRam test data
        if (evidenceData != null && evidenceData.ContainsKey("detectedGB"))
        {
            sections.Add("[System RAM]");
            if (evidenceData.TryGetValue("detectedGB", out var dgObj) && dgObj != null)
                sections.Add($"Detected:  {dgObj} GB");
            if (evidenceData.TryGetValue("minimumGB", out var mgObj) && mgObj != null)
                sections.Add($"Minimum:   {mgObj} GB");
            else
                sections.Add("Minimum:   none (informational)");
            if (evidenceData.TryGetValue("thresholdMet", out var tmObj) && tmObj != null)
                sections.Add($"Result:    {(tmObj.ToString() is "True" or "true" ? "meets requirement" : "below requirement")}");
            else
                sections.Add("Result:    — (informational)");
            sections.Add(string.Empty);
        }

        // [CPU Cores] section — emitted when Evidence contains CpuCores test data
        if (evidenceData != null && evidenceData.ContainsKey("detectedCores"))
        {
            sections.Add("[CPU Cores]");
            if (evidenceData.TryGetValue("detectedCores", out var dcObj) && dcObj != null)
                sections.Add($"Detected:  {dcObj} logical processors");
            if (evidenceData.TryGetValue("minimumCores", out var mcObj) && mcObj != null)
                sections.Add($"Minimum:   {mcObj}");
            else
                sections.Add("Minimum:   none (informational)");
            if (evidenceData.TryGetValue("thresholdMet", out var tmcObj) && tmcObj != null)
                sections.Add($"Result:    {(tmcObj.ToString() is "True" or "true" ? "meets requirement" : "below requirement")}");
            else
                sections.Add("Result:    — (informational)");
            sections.Add(string.Empty);
        }

        // [WebSocket] section — emitted when Evidence contains WebSocket test data
        if (evidenceData != null && evidenceData.ContainsKey("wsUrl"))
        {
            sections.Add("[WebSocket]");
            if (evidenceData.TryGetValue("wsUrl", out var wsUrlObj) && wsUrlObj != null)
                sections.Add($"URL:        {wsUrlObj}");
            if (evidenceData.TryGetValue("connected", out var connObj) && connObj != null)
                sections.Add($"Connected:  {(connObj.ToString() is "True" or "true" ? "yes" : "no")}");
            if (evidenceData.TryGetValue("connectTimeMs", out var ctmObj) && ctmObj != null)
                sections.Add($"Connect:    {ctmObj} ms");
            if (evidenceData.TryGetValue("subprotocol", out var spObj) && spObj != null && spObj.ToString() is { Length: > 0 } sp)
                sections.Add($"Subprotocol: {sp}");
            if (evidenceData.TryGetValue("messageSent", out var msObj) && msObj != null && msObj.ToString() is { Length: > 0 } sent)
                sections.Add($"Sent:       {sent}");
            if (evidenceData.TryGetValue("messageReceived", out var mrObj) && mrObj != null && mrObj.ToString() is { Length: > 0 } received)
                sections.Add($"Received:   {received}");
            if (evidenceData.TryGetValue("responseMatched", out var rmObj) && rmObj is JsonElement rmElem && rmElem.ValueKind != JsonValueKind.Null)
                sections.Add($"Match:      {(rmElem.ToString() is "True" or "true" ? "yes" : "no")}");
            sections.Add(string.Empty);
        }

        // [Response] section - if ResponseCode is set
        if (result.Evidence.ResponseCode.HasValue)
        {
            sections.Add("[Response]");
            var statusCode = result.Evidence.ResponseCode.Value;
            var statusText = GetStatusText(statusCode);
            sections.Add($"Status: {statusCode} {statusText}");

            // Try to get response time from evidence data
            if (evidenceData != null && evidenceData.TryGetValue("responseTime", out var rtObj))
            {
                if (rtObj != null && int.TryParse(rtObj.ToString(), out var responseTime))
                {
                    sections.Add($"Response Time: {responseTime}ms");
                }
            }

            // Try to get content-type from headers
            if (result.Evidence.ResponseHeaders != null &&
                result.Evidence.ResponseHeaders.TryGetValue("Content-Type", out var contentType))
            {
                sections.Add($"Content-Type: {contentType}");
            }

            // Try to get content length from evidence data
            if (evidenceData != null && evidenceData.TryGetValue("contentLength", out var clObj))
            {
                if (clObj != null && int.TryParse(clObj.ToString(), out var contentLength))
                {
                    sections.Add($"Content-Length: {contentLength} bytes");
                }
            }

            sections.Add(string.Empty);
        }

        // [Headers] section - if ResponseHeaders is not empty
        if (result.Evidence.ResponseHeaders != null && result.Evidence.ResponseHeaders.Count > 0)
        {
            sections.Add("[Headers]");
            foreach (var header in result.Evidence.ResponseHeaders)
            {
                sections.Add($"{header.Key}: {header.Value}");
            }
            sections.Add(string.Empty);
        }

        // [Body] section - if ResponseData contains body field
        if (evidenceData != null && evidenceData.TryGetValue("body", out var bodyObj))
        {
            var body = bodyObj?.ToString();
            if (!string.IsNullOrEmpty(body))
            {
                sections.Add("[Body]");
                sections.Add(body);
                sections.Add(string.Empty);
            }
        }

        // [File Content] section - if FileContent is not empty
        if (!string.IsNullOrEmpty(result.Evidence.FileContent))
        {
            sections.Add("[File Content]");
            sections.Add(result.Evidence.FileContent);
            sections.Add(string.Empty);
        }

        // [Process List] section - if ProcessList is not empty
        if (result.Evidence.ProcessList != null && result.Evidence.ProcessList.Length > 0)
        {
            sections.Add("[Process List]");
            foreach (var process in result.Evidence.ProcessList)
            {
                sections.Add(process);
            }
            sections.Add(string.Empty);
        }

        // [Registry] section - if RegistryValue is not empty
        if (!string.IsNullOrEmpty(result.Evidence.RegistryValue))
        {
            sections.Add("[Registry]");
            sections.Add(result.Evidence.RegistryValue);
            sections.Add(string.Empty);
        }

        // [Timing] section - if Timing is not null
        if (result.Evidence.Timing != null)
        {
            sections.Add("[Timing]");
            sections.Add($"Total: {result.Evidence.Timing.TotalMs}ms");
            if (result.Evidence.Timing.ConnectMs.HasValue)
            {
                sections.Add($"Connect: {result.Evidence.Timing.ConnectMs.Value}ms");
            }
            if (result.Evidence.Timing.ExecuteMs.HasValue)
            {
                sections.Add($"Execute: {result.Evidence.Timing.ExecuteMs.Value}ms");
            }
            sections.Add(string.Empty);
        }

        // If no sections were added, return null
        if (sections.Count == 0)
            return null;

        // Remove trailing empty string if present
        if (sections.Count > 0 && string.IsNullOrEmpty(sections[^1]))
            sections.RemoveAt(sections.Count - 1);

        return string.Join(Environment.NewLine, sections);
    }

    private static string GetStatusText(int statusCode)
    {
        return statusCode switch
        {
            200 => "OK",
            201 => "Created",
            204 => "No Content",
            301 => "Moved Permanently",
            302 => "Found",
            304 => "Not Modified",
            307 => "Temporary Redirect",
            308 => "Permanent Redirect",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            409 => "Conflict",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => string.Empty
        };
    }
}
