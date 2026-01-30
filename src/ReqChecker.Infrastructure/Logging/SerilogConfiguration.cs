using Serilog;
using Serilog.Events;

namespace ReqChecker.Infrastructure.Logging;

/// <summary>
/// Configures Serilog for structured logging.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog with file and debug sinks.
    /// </summary>
    /// <param name="appDataPath">The path to AppData directory.</param>
    public static void Configure(string appDataPath)
    {
        var logPath = Path.Combine(appDataPath, "Logs", "reqchecker-.log");
        var logDirectory = Path.GetDirectoryName(logPath);
        
        if (logDirectory != null && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ReqChecker")
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 100_000_000,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff z} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug()
            .CreateLogger();
    }

    /// <summary>
    /// Gets the current logger instance.
    /// </summary>
    public static ILogger Logger => Log.ForContext("SourceContext", "ReqChecker");
}
