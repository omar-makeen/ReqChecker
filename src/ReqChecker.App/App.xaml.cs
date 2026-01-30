using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ReqChecker.Core.Interfaces;
using ReqChecker.Infrastructure.Tests;
using ReqChecker.Infrastructure.Execution;
using ReqChecker.Infrastructure.ProfileManagement;
using ReqChecker.Infrastructure.ProfileManagement.Migrations;
using ReqChecker.Infrastructure.Export;
using ReqChecker.Infrastructure.Logging;
using ReqChecker.Infrastructure.Security;
using ReqChecker.App.ViewModels;
using ReqChecker.App.Services;

namespace ReqChecker.App;

/// <summary>
/// Application entry point with DI container setup.
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    private static string AppDataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ReqChecker");

    public App()
    {
        // Configure logging first
        SerilogConfiguration.Configure(AppDataPath);

        // Configure dependency injection
        ConfigureServices();
    }

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure app state with logs path
        var appState = Services.GetRequiredService<IAppState>();
        appState.SetLogsPath(Path.Combine(AppDataPath, "Logs"));
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register app state (singleton for shared state across ViewModels)
        services.AddSingleton<IAppState, AppState>();

        // Register profile management services
        services.AddSingleton<IProfileLoader, JsonProfileLoader>();
        services.AddSingleton<IProfileValidator, FluentProfileValidator>();
        services.AddSingleton<IProfileMigrator, ProfileMigrationPipeline>();
        services.AddSingleton<V1ToV2Migration>();
        // Note: HmacIntegrityVerifier is optional (null for user-provided profiles)

        // Register test implementations via attribute discovery
        var testAssembly = Assembly.GetAssembly(typeof(PingTest));
        if (testAssembly != null)
        {
            var testTypes = testAssembly.GetTypes()
                .Where(t => typeof(ITest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var testType in testTypes)
            {
                services.AddSingleton(typeof(ITest), testType);
            }
        }

        // Register credential provider for secure credential storage
        services.AddSingleton<ICredentialProvider, WindowsCredentialProvider>();

        // Register test runner with credential provider
        services.AddSingleton<ITestRunner>(sp =>
        {
            var tests = sp.GetServices<ITest>();
            var credentialProvider = sp.GetRequiredService<ICredentialProvider>();
            return new SequentialTestRunner(tests, credentialProvider);
        });

        // Register exporters (both as interface for collection resolution and concrete for direct injection)
        services.AddSingleton<JsonExporter>();
        services.AddSingleton<CsvExporter>();
        services.AddSingleton<IExporter>(sp => sp.GetRequiredService<JsonExporter>());
        services.AddSingleton<IExporter>(sp => sp.GetRequiredService<CsvExporter>());

        // Register services
        services.AddSingleton<NavigationService>(sp =>
            new NavigationService(sp));
        services.AddSingleton<DialogService>();
        services.AddSingleton<IClipboardService, ClipboardService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<ProfileSelectorViewModel>();
        services.AddTransient<TestListViewModel>();
        services.AddTransient<RunProgressViewModel>();
        services.AddTransient<ResultsViewModel>();
        services.AddTransient<DiagnosticsViewModel>();

        // Build service provider
        Services = services.BuildServiceProvider();
    }
}
