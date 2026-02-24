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

        // [Proxy] section — emitted when Evidence contains Proxy test data
        if (evidenceData != null && evidenceData.ContainsKey("proxyUrl"))
        {
            sections.Add("[Proxy]");
            if (evidenceData.TryGetValue("proxyUrl", out var proxyUrlObj) && proxyUrlObj != null)
                sections.Add($"Proxy:      {proxyUrlObj}");
            if (evidenceData.TryGetValue("testUrl", out var testUrlObj) && testUrlObj != null)
                sections.Add($"Target:     {testUrlObj}");
            if (evidenceData.TryGetValue("proxyType", out var ptObj) && ptObj != null)
                sections.Add($"Type:       {ptObj}");
            if (evidenceData.TryGetValue("proxyReached", out var prObj) && prObj != null)
                sections.Add($"Connected:  {(prObj.ToString() is "True" or "true" ? "yes" : "no")}");
            if (evidenceData.TryGetValue("statusCode", out var scObj) && scObj != null)
            {
                if (int.TryParse(scObj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var statusCode))
                {
                    var statusText = GetStatusText(statusCode);
                    sections.Add($"Status:     {statusCode} {statusText}");
                }
            }
            if (evidenceData.TryGetValue("connectTimeMs", out var ctmObj) && ctmObj != null)
                sections.Add($"Connect:    {ctmObj} ms");
            if (evidenceData.TryGetValue("proxyUsername", out var puObj) && puObj != null && puObj.ToString() is { Length: > 0 } username)
                sections.Add($"Username:   {username}");
            if (evidenceData.TryGetValue("authSucceeded", out var asObj) && asObj is JsonElement asElem && asElem.ValueKind != JsonValueKind.Null)
                sections.Add($"Auth:       {(asElem.ToString() is "True" or "true" ? "succeeded" : "failed")}");
            sections.Add(string.Empty);
        }

        // [Traceroute] section — emitted when Evidence contains Traceroute test data (host + hops keys)
        if (evidenceData != null && evidenceData.ContainsKey("host") && evidenceData.ContainsKey("hops"))
        {
            sections.Add("[Traceroute]");
            if (evidenceData.TryGetValue("host", out var hostObj) && hostObj != null)
                sections.Add($"Host:       {hostObj}");
            if (evidenceData.TryGetValue("resolvedIp", out var riObj) && riObj != null)
                sections.Add($"Resolved:   {riObj}");
            if (evidenceData.TryGetValue("hopCount", out var hcObj) && hcObj != null &&
                evidenceData.TryGetValue("maxHops", out var mhObj) && mhObj != null)
                sections.Add($"Hops:       {hcObj} / {mhObj}");
            if (evidenceData.TryGetValue("reachedTarget", out var rtObj) && rtObj != null)
                sections.Add($"Reached:    {(rtObj.ToString() is "True" or "true" ? "yes" : "no")}");

            // Parse hops array and render tracert-style compact lines
            if (evidenceData.TryGetValue("hops", out var hopsObj) && hopsObj?.ToString() is { } hopsStr)
            {
                try
                {
                    using var doc = JsonDocument.Parse(hopsStr);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        sections.Add(string.Empty);
                        foreach (var hop in doc.RootElement.EnumerateArray())
                        {
                            var hopNum = hop.TryGetProperty("hop", out var hn) ? hn.GetInt32() : 0;
                            var address = hop.TryGetProperty("address", out var addr) ? addr.GetString() : "*";
                            var roundtripMs = hop.TryGetProperty("roundtripMs", out var rtm) && rtm.ValueKind != JsonValueKind.Null
                                ? rtm.GetInt32().ToString() + "ms"
                                : "*";

                            // Format: "  {hop}   {roundtripMs}ms  {address}" (tracert-style)
                            sections.Add($"  {hopNum,-4} {roundtripMs,-6} {address}");
                        }
                    }
                }
                catch
                {
                    // ignore parse errors for hops array
                }
            }

            sections.Add(string.Empty);
        }

        // [Ping] section — emitted when Evidence contains Ping test data (successRate + pingResults keys)
        if (evidenceData != null && evidenceData.ContainsKey("successRate") && evidenceData.ContainsKey("pingResults"))
        {
            sections.Add("[Ping]");
            if (evidenceData.TryGetValue("host", out var pingHostObj) && pingHostObj != null)
                sections.Add($"Host:       {pingHostObj}");
            if (evidenceData.TryGetValue("successfulCount", out var scObj) && scObj != null &&
                evidenceData.TryGetValue("totalCount", out var tcObj) && tcObj != null)
                sections.Add($"Success:    {scObj}/{tcObj}");
            if (evidenceData.TryGetValue("successRate", out var srObj) && srObj != null)
                sections.Add($"Rate:       {srObj}");
            if (evidenceData.TryGetValue("averageRoundtripTime", out var artObj) && artObj != null)
                sections.Add($"Avg RTT:    {artObj}");

            // Parse pingResults array and render per-attempt lines
            if (evidenceData.TryGetValue("pingResults", out var prObj) && prObj?.ToString() is { } prStr)
            {
                try
                {
                    using var doc = JsonDocument.Parse(prStr);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        sections.Add(string.Empty);
                        foreach (var attempt in doc.RootElement.EnumerateArray())
                        {
                            var attemptNum = attempt.TryGetProperty("attempt", out var an) ? an.GetInt32() : 0;
                            var status = attempt.TryGetProperty("status", out var st) ? st.GetString() : "unknown";
                            var rtt = attempt.TryGetProperty("roundtripTime", out var rttProp) && rttProp.ValueKind != JsonValueKind.Null
                                ? rttProp.ToString()
                                : "*";
                            sections.Add($"  Attempt {attemptNum}: {status} ({rtt})");
                        }
                    }
                }
                catch
                {
                    // ignore parse errors for pingResults array
                }
            }

            sections.Add(string.Empty);
        }

        // [DNS] section — emitted when Evidence contains DnsResolve test data (hostname + addresses keys)
        if (evidenceData != null && evidenceData.ContainsKey("hostname") && evidenceData.ContainsKey("addresses"))
        {
            sections.Add("[DNS]");
            if (evidenceData.TryGetValue("hostname", out var hostnameObj) && hostnameObj != null)
                sections.Add($"Hostname:   {hostnameObj}");

            // Parse addresses array and render indented IP lines
            if (evidenceData.TryGetValue("addresses", out var addrObj) && addrObj?.ToString() is { } addrStr)
            {
                try
                {
                    using var doc = JsonDocument.Parse(addrStr);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        sections.Add("Addresses:");
                        foreach (var addr in doc.RootElement.EnumerateArray())
                        {
                            var ip = addr.GetString();
                            sections.Add($"  {ip}");
                        }
                    }
                }
                catch
                {
                    // ignore parse errors for addresses array
                }
            }

            if (evidenceData.TryGetValue("addressCount", out var acObj) && acObj != null)
                sections.Add($"Count:      {acObj}");
            if (evidenceData.TryGetValue("resolutionTimeMs", out var rtmObj) && rtmObj != null)
                sections.Add($"Resolution: {rtmObj} ms");

            sections.Add(string.Empty);
        }

        // [TCP] section — emitted when Evidence contains TcpPortOpen test data (host + port + connected keys)
        if (evidenceData != null && evidenceData.ContainsKey("host") && evidenceData.ContainsKey("port") && evidenceData.ContainsKey("connected"))
        {
            // Avoid collision with Traceroute which also has "host" but has "hops" instead of "port"
            if (!evidenceData.ContainsKey("hops"))
            {
                sections.Add("[TCP]");
                if (evidenceData.TryGetValue("host", out var tcpHostObj) && tcpHostObj != null)
                    sections.Add($"Host:       {tcpHostObj}");
                if (evidenceData.TryGetValue("port", out var portObj) && portObj != null)
                    sections.Add($"Port:       {portObj}");
                if (evidenceData.TryGetValue("connected", out var connObj) && connObj != null)
                    sections.Add($"Connected:  {(connObj.ToString() is "True" or "true" ? "yes" : "no")}");
                if (evidenceData.TryGetValue("connectTimeMs", out var ctmObj) && ctmObj != null)
                    sections.Add($"Connect:    {ctmObj} ms");
                sections.Add(string.Empty);
            }
        }

        // [UDP] section — emitted when Evidence contains UdpPortOpen test data (Responded + PayloadSentBytes keys)
        if (evidenceData != null && evidenceData.ContainsKey("Responded") && evidenceData.ContainsKey("PayloadSentBytes"))
        {
            sections.Add("[UDP]");
            if (evidenceData.TryGetValue("Responded", out var respObj) && respObj != null)
                sections.Add($"Responded:  {(respObj.ToString() is "True" or "true" ? "yes" : "no")}");
            if (evidenceData.TryGetValue("RoundTripTimeMs", out var rttObj) && rttObj != null)
                sections.Add($"RTT:        {rttObj} ms");
            if (evidenceData.TryGetValue("PayloadSentBytes", out var psbObj) && psbObj != null)
                sections.Add($"Sent:       {psbObj} bytes");
            if (evidenceData.TryGetValue("PayloadReceivedBytes", out var prbObj) && prbObj != null)
                sections.Add($"Received:   {prbObj} bytes");
            if (evidenceData.TryGetValue("ResponseDataPreview", out var rdpObj) && rdpObj != null && rdpObj.ToString() is { Length: > 0 } preview)
                sections.Add($"Data:       {preview}");
            sections.Add(string.Empty);
        }

        // [Disk Space] section — emitted when Evidence contains DiskSpace test data (totalSpaceGB + freeSpaceGB keys)
        if (evidenceData != null && evidenceData.ContainsKey("totalSpaceGB") && evidenceData.ContainsKey("freeSpaceGB"))
        {
            sections.Add("[Disk Space]");
            if (evidenceData.TryGetValue("path", out var dspObj) && dspObj != null)
                sections.Add($"Path:       {dspObj}");
            if (evidenceData.TryGetValue("totalSpaceGB", out var tsgObj) && tsgObj != null)
                sections.Add($"Total:      {tsgObj} GB");
            if (evidenceData.TryGetValue("freeSpaceGB", out var fsgObj) && fsgObj != null)
                sections.Add($"Free:       {fsgObj} GB");
            if (evidenceData.TryGetValue("percentFree", out var pfObj) && pfObj != null)
                sections.Add($"Free %:     {pfObj}%");
            if (evidenceData.TryGetValue("minimumFreeGB", out var mfgObj) && mfgObj != null)
                sections.Add($"Minimum:    {mfgObj} GB");
            if (evidenceData.TryGetValue("thresholdMet", out var tmObj) && tmObj != null)
                sections.Add($"Threshold:  {(tmObj.ToString() is "True" or "true" ? "met" : "not met")}");
            sections.Add(string.Empty);
        }

        // [Service] section — emitted when Evidence contains WindowsService test data (serviceName + expectedStatus keys)
        if (evidenceData != null && evidenceData.ContainsKey("serviceName") && evidenceData.ContainsKey("expectedStatus"))
        {
            sections.Add("[Service]");
            if (evidenceData.TryGetValue("serviceName", out var snObj) && snObj != null)
                sections.Add($"Service:    {snObj}");
            if (evidenceData.TryGetValue("displayName", out var dnObj) && dnObj != null && dnObj.ToString() is { Length: > 0 } dn)
                sections.Add($"Display:    {dn}");
            if (evidenceData.TryGetValue("status", out var stObj) && stObj != null)
                sections.Add($"Status:     {stObj}");
            if (evidenceData.TryGetValue("expectedStatus", out var esObj) && esObj != null)
                sections.Add($"Expected:   {esObj}");
            if (evidenceData.TryGetValue("startType", out var sttObj) && sttObj != null && sttObj.ToString() is { Length: > 0 } stt)
                sections.Add($"Start Type: {stt}");
            if (evidenceData.TryGetValue("statusMatch", out var smObj) && smObj != null)
                sections.Add($"Match:      {(smObj.ToString() is "True" or "true" ? "yes" : "no")}");
            sections.Add(string.Empty);
        }

        // [mTLS] section — emitted when Evidence contains MtlsConnect test data (CertificateSubject + CertificateThumbprint keys)
        if (evidenceData != null && evidenceData.ContainsKey("CertificateSubject") && evidenceData.ContainsKey("CertificateThumbprint"))
        {
            sections.Add("[mTLS]");
            if (evidenceData.TryGetValue("Connected", out var mconnObj) && mconnObj != null)
                sections.Add($"Connected:  {(mconnObj.ToString() is "True" or "true" ? "yes" : "no")}");
            if (evidenceData.TryGetValue("ResponseTimeMs", out var rtmObj) && rtmObj != null)
                sections.Add($"Response:   {rtmObj} ms");
            if (evidenceData.TryGetValue("CertificateSubject", out var csObj) && csObj != null)
                sections.Add($"Subject:    {csObj}");
            if (evidenceData.TryGetValue("CertificateIssuer", out var ciObj) && ciObj != null && ciObj.ToString() is { Length: > 0 } ci)
                sections.Add($"Issuer:     {ci}");
            if (evidenceData.TryGetValue("CertificateThumbprint", out var ctObj) && ctObj != null)
                sections.Add($"Thumbprint: {ctObj}");
            if (evidenceData.TryGetValue("CertificateNotBefore", out var cnbObj) && cnbObj != null && cnbObj.ToString() is { Length: > 0 } cnb)
                sections.Add($"Valid From: {cnb}");
            if (evidenceData.TryGetValue("CertificateNotAfter", out var cnaObj) && cnaObj != null && cnaObj.ToString() is { Length: > 0 } cna)
                sections.Add($"Valid To:   {cna}");
            if (evidenceData.TryGetValue("CertificateHasPrivateKey", out var chpkObj) && chpkObj != null)
                sections.Add($"Private Key: {(chpkObj.ToString() is "True" or "true" ? "yes" : "no")}");
            sections.Add(string.Empty);
        }

        // [Certificate] section — emitted when Evidence contains CertificateExpiry test data (DaysUntilExpiry + IsExpired keys)
        if (evidenceData != null && evidenceData.ContainsKey("DaysUntilExpiry") && evidenceData.ContainsKey("IsExpired"))
        {
            sections.Add("[Certificate]");
            if (evidenceData.TryGetValue("Host", out var chostObj) && chostObj != null)
                sections.Add($"Host:       {chostObj}");
            if (evidenceData.TryGetValue("Port", out var cportObj) && cportObj != null)
                sections.Add($"Port:       {cportObj}");
            if (evidenceData.TryGetValue("Subject", out var csubObj) && csubObj != null)
                sections.Add($"Subject:    {csubObj}");
            if (evidenceData.TryGetValue("Issuer", out var cissObj) && cissObj != null && cissObj.ToString() is { Length: > 0 } ciss)
                sections.Add($"Issuer:     {ciss}");
            if (evidenceData.TryGetValue("Thumbprint", out var cthumbObj) && cthumbObj != null && cthumbObj.ToString() is { Length: > 0 } cthumb)
                sections.Add($"Thumbprint: {cthumb}");
            if (evidenceData.TryGetValue("NotAfter", out var cnaObj) && cnaObj != null)
                sections.Add($"Expires:    {cnaObj}");
            if (evidenceData.TryGetValue("DaysUntilExpiry", out var dueObj) && dueObj != null)
                sections.Add($"Days Left:  {dueObj}");
            if (evidenceData.TryGetValue("IsExpired", out var ieObj) && ieObj != null)
                sections.Add($"Expired:    {(ieObj.ToString() is "True" or "true" ? "yes" : "no")}");
            if (evidenceData.TryGetValue("IsNotYetValid", out var inyvObj) && inyvObj != null && inyvObj.ToString() is "True" or "true")
                sections.Add($"Not Yet Valid: yes");
            sections.Add(string.Empty);
        }

        // [File] section — emitted when Evidence contains FileExists test data (category == "file")
        if (evidenceData != null && evidenceData.TryGetValue("category", out var fileCatObj) && fileCatObj?.ToString() == "file")
        {
            // Avoid collision with DirectoryExists which has path + exists but has directoryCount instead of size
            if (!evidenceData.ContainsKey("directoryCount"))
            {
                sections.Add("[File]");
                if (evidenceData.TryGetValue("path", out var fpathObj) && fpathObj != null)
                    sections.Add($"Path:       {fpathObj}");
                if (evidenceData.TryGetValue("exists", out var fexObj) && fexObj != null)
                    sections.Add($"Exists:     {(fexObj.ToString() is "True" or "true" ? "yes" : "no")}");
                if (evidenceData.TryGetValue("shouldExist", out var fseObj) && fseObj != null)
                    sections.Add($"Expected:   {(fseObj.ToString() is "True" or "true" ? "yes" : "no")}");
                if (evidenceData.TryGetValue("size", out var fsObj) && fsObj != null)
                {
                    if (long.TryParse(fsObj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var sizeBytes))
                        sections.Add($"Size:       {FormatBytes(sizeBytes)}");
                    else
                        sections.Add($"Size:       {fsObj} bytes");
                }
                if (evidenceData.TryGetValue("lastModified", out var flmObj) && flmObj != null && flmObj.ToString() is { Length: > 0 } flm)
                    sections.Add($"Modified:   {flm}");
                sections.Add(string.Empty);
            }
        }

        // [Directory] section — emitted when Evidence contains DirectoryExists test data (category == "directory")
        if (evidenceData != null && evidenceData.TryGetValue("category", out var dirCatObj) && dirCatObj?.ToString() == "directory")
        {
            sections.Add("[Directory]");
            if (evidenceData.TryGetValue("path", out var dpathObj) && dpathObj != null)
                sections.Add($"Path:       {dpathObj}");
            if (evidenceData.TryGetValue("exists", out var dexObj) && dexObj != null)
                sections.Add($"Exists:     {(dexObj.ToString() is "True" or "true" ? "yes" : "no")}");
            if (evidenceData.TryGetValue("shouldExist", out var dseObj) && dseObj != null)
                sections.Add($"Expected:   {(dseObj.ToString() is "True" or "true" ? "yes" : "no")}");
            if (evidenceData.TryGetValue("fileCount", out var dfcObj) && dfcObj != null)
                sections.Add($"Files:      {dfcObj}");
            if (evidenceData.TryGetValue("directoryCount", out var ddcObj) && ddcObj != null)
                sections.Add($"Directories: {ddcObj}");
            if (evidenceData.TryGetValue("creationTime", out var dctObj) && dctObj != null && dctObj.ToString() is { Length: > 0 } dct)
                sections.Add($"Created:    {dct}");
            sections.Add(string.Empty);
        }

        // [Bandwidth] section — emitted when Evidence contains bandwidth test data (measuredMbps + bytesDownloaded)
        if (evidenceData != null && evidenceData.ContainsKey("measuredMbps") && evidenceData.ContainsKey("bytesDownloaded"))
        {
            sections.Add("[Bandwidth]");
            if (evidenceData.TryGetValue("url", out var bwUrlObj) && bwUrlObj != null)
                sections.Add($"URL:        {bwUrlObj}");
            if (evidenceData.TryGetValue("measuredMbps", out var bwSpeedObj) && bwSpeedObj != null && double.TryParse(bwSpeedObj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var bwSpeed))
                sections.Add($"Speed:      {bwSpeed:F2} Mbps");
            if (evidenceData.TryGetValue("minimumMbps", out var bwMinObj) && bwMinObj != null && double.TryParse(bwMinObj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var bwMin))
                sections.Add($"Minimum:    {bwMin:F2} Mbps");
            if (evidenceData.TryGetValue("bytesDownloaded", out var bwBytesObj) && bwBytesObj != null && long.TryParse(bwBytesObj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var bwBytes))
                sections.Add($"Downloaded: {FormatBytes(bwBytes)}");
            if (evidenceData.TryGetValue("elapsedSeconds", out var bwDurObj) && bwDurObj != null && double.TryParse(bwDurObj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var bwDur))
                sections.Add($"Duration:   {bwDur:F2} s");
            if (evidenceData.TryGetValue("thresholdMet", out var bwThreshObj) && bwThreshObj != null)
                sections.Add($"Threshold:  {(bwThreshObj.ToString() is "True" or "true" ? "met" : "not met")}");
            sections.Add(string.Empty);
        }

        // [Latency] section — emitted when Evidence contains latency test data (averageLatencyMs + minLatencyMs)
        if (evidenceData != null && evidenceData.ContainsKey("averageLatencyMs") && evidenceData.ContainsKey("minLatencyMs"))
        {
            sections.Add("[Latency]");
            if (evidenceData.TryGetValue("host", out var latHostObj) && latHostObj != null)
                sections.Add($"Host:       {latHostObj}");
            if (evidenceData.TryGetValue("averageLatencyMs", out var latAvgObj) && latAvgObj != null && double.TryParse(latAvgObj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var latAvg))
                sections.Add($"Avg:        {latAvg:F2} ms");
            if (evidenceData.TryGetValue("minLatencyMs", out var latMinObj) && latMinObj != null && double.TryParse(latMinObj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var latMin))
                sections.Add($"Min:        {latMin:F2} ms");
            if (evidenceData.TryGetValue("maxLatencyMs", out var latMaxObj) && latMaxObj != null && double.TryParse(latMaxObj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var latMax))
                sections.Add($"Max:        {latMax:F2} ms");
            if (evidenceData.TryGetValue("maxThresholdMs", out var latThreshObj) && latThreshObj != null && double.TryParse(latThreshObj.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var latThresh))
                sections.Add($"Threshold:  {(latThresh == 0 ? "0.00 ms (reachability)" : $"{latThresh:F2} ms")}");
            if (evidenceData.TryGetValue("successfulCount", out var latSucObj) && latSucObj != null &&
                evidenceData.TryGetValue("sampleCount", out var latCountObj) && latCountObj != null)
                sections.Add($"Samples:    {latSucObj}/{latCountObj}");
            if (evidenceData.TryGetValue("thresholdMet", out var latMetObj) && latMetObj != null)
                sections.Add($"Result:     {(latMetObj.ToString() is "True" or "true" ? "met" : "not met")}");
            sections.Add(string.Empty);
        }

        // [SmtpConnect] section — emitted when Evidence contains SMTP test data (serverBanner + responseTimeMs)
        if (evidenceData != null && evidenceData.ContainsKey("serverBanner") && evidenceData.ContainsKey("responseTimeMs"))
        {
            sections.Add("[SmtpConnect]");
            if (evidenceData.TryGetValue("host", out var smtpHostObj) && smtpHostObj != null)
                sections.Add($"Host:       {smtpHostObj}");
            if (evidenceData.TryGetValue("port", out var smtpPortObj) && smtpPortObj != null)
                sections.Add($"Port:       {smtpPortObj}");
            if (evidenceData.TryGetValue("serverBanner", out var smtpBannerObj) && smtpBannerObj != null)
                sections.Add($"Banner:     {smtpBannerObj}");
            if (evidenceData.TryGetValue("responseTimeMs", out var smtpTimeObj) && smtpTimeObj != null &&
                long.TryParse(smtpTimeObj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var smtpTime))
                sections.Add($"Time:       {smtpTime} ms");
            // Conditional TLS line - only shown when tlsNegotiated exists and is true
            if (evidenceData.TryGetValue("tlsNegotiated", out var tlsNegObj) && tlsNegObj?.ToString() is "True" or "true")
            {
                if (evidenceData.TryGetValue("tlsVersion", out var tlsVerObj) && tlsVerObj != null)
                    sections.Add($"TLS:        {tlsVerObj}");
            }
            // Conditional Auth line - only shown when authenticated key exists
            if (evidenceData.TryGetValue("authenticated", out var authObj) && authObj != null)
                sections.Add($"Auth:       {(authObj.ToString() is "True" or "true" ? "yes" : "no")}");
            sections.Add(string.Empty);
        }

        // [DotNetVersion] section — emitted when Evidence contains .NET version test data (runtimeType + minimumSatisfied)
        if (evidenceData != null && evidenceData.ContainsKey("runtimeType") && evidenceData.ContainsKey("minimumSatisfied"))
        {
            sections.Add("[DotNetVersion]");
            if (evidenceData.TryGetValue("runtimeType", out var runtimeTypeObj) && runtimeTypeObj != null)
                sections.Add($"Type:       {runtimeTypeObj}");
            if (evidenceData.TryGetValue("minimumVersion", out var minVerObj) && minVerObj != null)
            {
                var minVerStr = minVerObj.ToString();
                sections.Add($"Minimum:    {(minVerStr == "0.0.0" ? "0.0.0 (presence check)" : minVerStr)}");
            }
            if (evidenceData.TryGetValue("installedVersions", out var installedObj) && installedObj != null)
            {
                var installedStr = installedObj.ToString();
                if (string.IsNullOrEmpty(installedStr))
                {
                    sections.Add("Installed:  none");
                }
                else
                {
                    var versions = installedStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    if (versions.Length == 0)
                    {
                        sections.Add("Installed:  none");
                    }
                    else
                    {
                        // First version on the label line
                        sections.Add($"Installed:  {versions[0]}");
                        // Subsequent versions on indented lines (12-space indent)
                        for (int i = 1; i < versions.Length; i++)
                        {
                            sections.Add($"            {versions[i]}");
                        }
                    }
                }
            }
            if (evidenceData.TryGetValue("minimumSatisfied", out var satisfiedObj) && satisfiedObj != null)
                sections.Add($"Satisfied:  {(satisfiedObj.ToString() is "True" or "true" ? "yes" : "no")}");
            sections.Add(string.Empty);
        }

        // [RegistryWrite] section — emitted when Evidence contains registry write test data (writeSucceeded + cleanupSucceeded)
        if (evidenceData != null && evidenceData.ContainsKey("writeSucceeded") && evidenceData.ContainsKey("cleanupSucceeded"))
        {
            sections.Add("[RegistryWrite]");
            if (evidenceData.TryGetValue("target", out var targetObj) && targetObj != null)
                sections.Add($"Target:      {targetObj}");
            if (evidenceData.TryGetValue("keyPath", out var keyPathObj) && keyPathObj != null)
                sections.Add($"Key Path:    {keyPathObj}");
            if (evidenceData.TryGetValue("testValueName", out var testValueNameObj) && testValueNameObj != null)
                sections.Add($"Test Value:  {testValueNameObj}");
            if (evidenceData.TryGetValue("writeSucceeded", out var writeObj) && writeObj != null)
            {
                var writeSucceeded = writeObj.ToString() is "True" or "true";
                sections.Add($"Writable:    {(writeSucceeded ? "yes" : "no")}");
                if (evidenceData.TryGetValue("cleanupSucceeded", out var cleanupObj) && cleanupObj != null)
                    sections.Add($"Cleanup:     {(writeSucceeded ? (cleanupObj.ToString() is "True" or "true" ? "yes" : "no") : "n/a")}");
            }
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
                if (rtObj != null && int.TryParse(rtObj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var responseTime))
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
                if (clObj != null && int.TryParse(clObj.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var contentLength))
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

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return suffixIndex == 0
            ? $"{size:F0} {suffixes[suffixIndex]}"
            : $"{size:F2} {suffixes[suffixIndex]}";
    }
}
